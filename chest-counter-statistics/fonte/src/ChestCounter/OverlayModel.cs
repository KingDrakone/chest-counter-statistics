namespace ChestCounter;

/// <summary>Size and opacity choices of the overlay window (percent), offered in the settings.</summary>
public static class OverlayAppearance
{
    public static readonly int[] Sizes = { 70, 80, 90, 100, 115, 130, 150, 175 };

    /// <summary>Below 30 % the numbers can no longer be read.</summary>
    public static readonly int[] Opacities = { 100, 90, 80, 70, 60, 50, 40, 30 };

    public static int ClampSize(int percent) => Math.Clamp(percent, Sizes[0], Sizes[^1]);
    public static int ClampOpacity(int percent) => Math.Clamp(percent, Opacities[^1], 100);

    /// <summary>Next value of the list, back to the first one after the last (or for a value edited by hand).</summary>
    public static int Next(int[] values, int current) => values[(Array.IndexOf(values, current) + 1) % values.Length];
}

/// <summary>Builds what the overlays (window and OBS page) show from the saved matches and the match in progress.</summary>
public static class OverlayModel
{
    /// <summary>Caption of the right counter between two matches (also used to keep the overlay width stable).</summary>
    public static string LastMatchCaption => L.T("dernière partie", "last match", "última partida", "última partida");

    /// <param name="live">Figures of the match in progress, when the replay could be read (null otherwise).</param>
    /// <param name="inProgress">A match is being recorded (even if its figures are not readable yet).</param>
    public static OverlayView Build(TrackerData d, MatchRecord? live, bool inProgress, bool showSquad = true)
    {
        var view = BuildCore(d, live, inProgress, showSquad);
        var u = Unified.Compute(d, live);
        var games = u.Matches;
        var deaths = games - u.Wins;
        var kd = deaths > 0 ? (double)u.Kills / deaths : u.Kills;
        var avg = games > 0 ? u.Seconds / games : 0;
        // Captions follow the interface language; figures use that language's decimal and thousands separators.
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        string Dec(double x, string f = "0.00") => L.En ? x.ToString(f, inv) : x.ToString(f, inv).Replace('.', ',');
        string Thousands(int x) => L.En ? x.ToString("N0", inv) : x.ToString("N0", inv).Replace(",", "\u00A0");
        var currentCaption = view.CurrentLive
            ? (view.Current == "…" ? L.T("EN COURS", "IN PROGRESS", "EN CURSO", "EM CURSO") : L.T("PARTIE ACTUELLE", "CURRENT MATCH", "PARTIDA ACTUAL", "PARTIDA ATUAL"))
            : L.T("DERNIÈRE PARTIE", "LAST MATCH", "ÚLTIMA PARTIDA", "ÚLTIMA PARTIDA");
        var cards = new List<OverlayCard>
        {
            new OverlayCard("gold", new[] { new OverlayCell(view.Total, L.T("TOTAL", "TOTAL", "TOTAL", "TOTAL"), "chest"), new OverlayCell(view.Current ?? "—", currentCaption, "pad") }),
            new OverlayCard("blue", new[]
            {
                new OverlayCell(u.Wins.ToString(inv), L.T("VICTOIRES", "WINS", "VICTORIAS", "VITÓRIAS"), "trophy"),
                new OverlayCell(u.Losses.ToString(inv), L.T("DÉFAITES", "LOSSES", "DERROTAS", "DERROTAS"), "x"),
                new OverlayCell(Dec(games > 0 ? u.Wins * 100.0 / games : 0, "0.0") + "%", L.T("% VICTOIRES", "WIN %", "% VICTORIAS", "% VITÓRIAS"), "pct"),
                new OverlayCell(games.ToString(inv), L.T("PARTIES", "MATCHES", "PARTIDAS", "PARTIDAS"), "pad"),
            }),
            new OverlayCard("purple", new[]
            {
                new OverlayCell(Dec(kd), "K/D", "cross"),
                new OverlayCell(Dec(games > 0 ? (double)u.Kills / games : 0), L.T("KILLS/PARTIE", "KILLS/MATCH", "KILLS/PARTIDA", "KILLS/PARTIDA"), "kpm"),
                new OverlayCell(u.Kills.ToString(inv), "KILLS", "skull"),
            }),
            new OverlayCard("red", new[]
            {
                new OverlayCell($"{u.Seconds / 3600}H {u.Seconds % 3600 / 60:00}M", L.T("TEMPS DE JEU", "PLAY TIME", "TIEMPO DE JUEGO", "TEMPO DE JOGO"), "clock"),
                new OverlayCell($"{avg / 60}M {avg % 60:00}S", L.T("TEMPS MOYEN", "AVG. TIME", "TIEMPO MEDIO", "TEMPO MÉDIO"), "hourglass"),
            }),
            new OverlayCard("green", new[]
            {
                new OverlayCell(Thousands(u.DamageDealt), L.T("DÉGÂTS", "DAMAGE", "DAÑO", "DANO"), "bolt"),
                new OverlayCell(u.Assists.ToString(inv), L.T("ASSISTANCES", "ASSISTS", "ASISTENCIAS", "ASSISTÊNCIAS"), "assist"),
                new OverlayCell(Dec(u.AccuracyAvg, "0.0") + "%", L.T("PRÉCISION", "ACCURACY", "PRECISIÓN", "PRECISÃO"), "target"),
            }),
        };
        return view with { Level = u.Level.ToString(inv), Cards = cards, Badge = view.Badge };
    }

    private static OverlayView BuildCore(TrackerData d, MatchRecord? live, bool inProgress, bool showSquad)
    {
        var liveChests = live?.ChestsOpened ?? 0;
        var liveTreasures = live?.TreasureFinds.Count ?? 0;

        var stats = Unified.Compute(d, live);
        var total = stats.Chests;
        // Squad total: only once a teammate's chest has been counted (never for a solo player).
        var team = d.TeamChests + (live?.TeamChestsOpened ?? 0);
        var squad = showSquad && team > 0 ? L.N(total + team) : null;
        var session = d.Matches.Where(ReportWriter.IsThisSession).ToList();
        var rows = new List<OverlayRow>();

        // Session: chests, then small grey "n parties · moy. x" (finished matches of this session).
        string? sessionDetail = null;
        if (session.Count > 0)
        {
            var avg = (double)session.Sum(m => m.ChestsOpened) / session.Count;
            var avgText = Math.Round(avg, MidpointRounding.AwayFromZero).ToString("0", L.Culture);
            sessionDetail = $"{L.Count(session.Count, "partie", "parties", "match", "matches", "partida", "partidas", "partida", "partidas")} · {L.T("moy.", "avg.", "media", "média")} {avgText}";
        }
        rows.Add(new OverlayRow(L.T("Session", "Session", "Sesión", "Sessão"), L.Count(session.Sum(m => m.ChestsOpened) + liveChests, "coffre", "chest", "cofre", "baú"), Detail: sessionDetail));

        var last = d.Matches.OrderByDescending(m => m.MatchStartUtc ?? m.AnalyzedAtUtc).FirstOrDefault();
        if (last != null)
        {
            rows.Add(new OverlayRow(L.T("Dernière", "Last", "Última", "Última"), L.Count(last.ChestsOpened, "coffre", "chest", "cofre", "baú")));
        }

        var treasures = d.Matches.Sum(m => m.TreasureFinds.Count) + liveTreasures;
        if (squad != null)
        {
            rows.Add(new OverlayRow(SquadLabel, L.Count(total + team, "coffre", "chest", "cofre", "baú")));
        }
        rows.Add(new OverlayRow(L.T("Trésors", "Treasures", "Tesoros", "Tesouros"), $"★ {L.N(treasures)}", Treasure: true));

        rows.Add(new OverlayRow(L.T("Vict. / Déf.", "Wins / Losses", "Vict. / Der.", "Vit. / Der."), $"{stats.Wins} / {stats.Losses}"));
        rows.Add(new OverlayRow("Kills", L.N(stats.Kills)));

        // Right counter: the match in progress when readable, "…" while it cannot be read yet, else the last match.
        if (live != null)
        {
            return new OverlayView(L.N(total), L.N(live.ChestsOpened), L.T("partie en cours", "current match", "partida actual", "partida atual"), true, L.T("● EN DIRECT", "● LIVE", "● EN DIRECTO", "● EM DIRETO"), rows, L.N(treasures), L.N(session.Sum(m => m.ChestsOpened) + liveChests), last != null ? L.N(last.ChestsOpened) : null, squad);
        }
        if (inProgress)
        {
            return new OverlayView(L.N(total), "…", L.T("en cours", "in progress", "en curso", "em curso"), true, L.T("● PARTIE EN COURS", "● MATCH IN PROGRESS", "● PARTIDA EN CURSO", "● PARTIDA EM CURSO"), rows, L.N(treasures), L.N(session.Sum(m => m.ChestsOpened) + liveChests), last != null ? L.N(last.ChestsOpened) : null, squad);
        }
        return new OverlayView(L.N(total), last != null ? L.N(last.ChestsOpened) : null, LastMatchCaption, false, null, rows, L.N(treasures), L.N(session.Sum(m => m.ChestsOpened) + liveChests), last != null ? L.N(last.ChestsOpened) : null, squad);
    }

    /// <summary>Label of the squad total (the player + every teammate, duo, trio or squad).</summary>
    public static string SquadLabel => L.T("Équipe", "Squad", "Equipo", "Equipa");
}
