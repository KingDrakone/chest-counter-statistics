using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;

namespace ChestCounter;

/// <summary>
/// Figures that cannot be read from the replays (account level, name, season) and the starting point
/// (what was already counted by hand before the automatic counting began). Saved in manual.json.
/// </summary>
public sealed class ManualStats
{
    /// <summary>Key of the profile (account) this file belongs to; empty in files written before profiles had their own.</summary>
    public string Owner { get; set; } = "";

    public string Name { get; set; } = "Jogador";
    public string Season { get; set; } = "";
    public int Level { get; set; } = 1;

    /// <summary>UTC "yyyyMMddHHmmss": matches that started before this are already inside the Base figures.</summary>
    public string Since { get; set; } = "";

    /// <summary>The level of the replays and the level shown in game may be on different scales: this is the gap between them.</summary>
    public int LevelOffset { get; set; }

    /// <summary>False until the level of the first replay has been compared with <see cref="Level"/>.</summary>
    public bool LevelSynced { get; set; }

    /// <summary>The level was lowered by hand: the next replay tells what the gap is again.</summary>
    public bool Recalibrate { get; set; }

    public int BaseChests { get; set; }
    public int BaseMatches { get; set; }
    public int BaseBest { get; set; }
    public int BaseWins { get; set; }
    public int BaseLosses { get; set; }
    public int BaseKills { get; set; }
    public int BaseSeconds { get; set; }

    // Starting points for the figures that only the replays give (set when they are edited by hand).
    public int BaseDamage { get; set; }
    public int BaseAssists { get; set; }
    /// <summary>Accuracy (percent) of the matches counted before the replays; null = only the replays count.</summary>
    public double? AccuracyBase { get; set; }
}

/// <summary>The automatic part of each figure (what the replays add on top of the manual starting point).</summary>
public sealed record AutoParts(int Chests, int Matches, int Wins, int Losses, int Kills, int Seconds, int OwnDamage, int OwnAssists, double AccuracySum, int AccuracyCount);

/// <summary>Everything the overlays show, in one place.</summary>
public sealed record StatsSnapshot(
    int Cur, int Chests, int Matches, int Best, int Wins, int Losses, int Level,
    int Kills, int KillsMatch, int Seconds, string Name, string Season, int Treasures,
    int Assists, int DamageDealt, int DamageTaken, int DamageStructures, int Revives,
    int MaterialsGathered, int MaterialsUsed, double AccuracyAvg, long TotalTraveled);

/// <summary>
/// The old chest counter (hotkeys, Stream Deck, OBS cards) built into ChestCounter:
/// the replays feed the figures automatically, manual.json holds the rest.
/// </summary>
public static class Unified
{
    private static readonly object Gate = new();
    private static string _folder = "";

    /// <summary>manual.json exists but could not be read (being written, or damaged): never overwrite it in that case.</summary>
    private static bool _loadFailed;

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string ObsFolder => Path.Combine(_folder, "obs");

    // Each profile (Epic account) has its own manual figures, name, season and level. The first profile to be seen takes
    // over the existing manual.json; the others get manual-<account>.json.
    private static string _profileKey = "";
    private static string? _manualFile; // null = manual.json

    public static string ManualPath => Path.Combine(_folder, _manualFile ?? "manual.json");

    private static string LegacyManualPath => Path.Combine(_folder, "manual.json");

    /// <summary>Switches to the manual figures of a profile (cheap when it is already the current one).</summary>
    public static void UseProfile(string? key, string? name)
    {
        key ??= "";
        lock (Gate)
        {
            if (_folder.Length == 0 || key == _profileKey)
            {
                return;
            }
            _profileKey = key;
            if (key.Length == 0)
            {
                _manualFile = null;
                return;
            }

            ManualStats? legacy = null;
            try
            {
                if (File.Exists(LegacyManualPath))
                {
                    legacy = JsonSerializer.Deserialize<ManualStats>(File.ReadAllText(LegacyManualPath), Json);
                }
            }
            catch
            {
                // unreadable: leave it alone
            }
            if (legacy != null && legacy.Owner.Length == 0)
            {
                legacy.Owner = key; // first profile seen: it keeps the figures counted so far
                _manualFile = null;
                SaveManual(legacy);
            }

            if (legacy != null && legacy.Owner == key)
            {
                _manualFile = null;
                return;
            }
            var safe = string.Concat(key.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            _manualFile = "manual-" + safe + ".json";
            if (!File.Exists(ManualPath))
            {
                // New profile: starts from zero (every replay of this account counts), named after the account.
                SaveManual(new ManualStats { Owner = key, Name = string.IsNullOrWhiteSpace(name) ? "Jogador" : name!, Since = "" });
            }
        }
    }

    private static string Stamp(DateTime utc) => utc.ToUniversalTime().ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

    public static void Init(Store store)
    {
        _folder = store.Folder;
        lock (Gate)
        {
            LoadManual();
        }
        WriteStaticFiles();
    }

    // ---- manual.json ------------------------------------------------------------------------

    private static ManualStats LoadManual()
    {
        if (_folder.Length == 0)
        {
            return new ManualStats { Since = Stamp(DateTime.UtcNow) };
        }
        try
        {
            if (File.Exists(ManualPath))
            {
                var m = JsonSerializer.Deserialize<ManualStats>(File.ReadAllText(ManualPath), Json);
                if (m != null)
                {
                    _loadFailed = false;
                    return m;
                }
            }
        }
        catch
        {
            // unreadable file: never overwrite it, work with defaults until it is fixed
            _loadFailed = true;
            return new ManualStats { Since = Stamp(DateTime.UtcNow) };
        }

        var created = ImportLegacy() ?? new ManualStats { Since = Stamp(DateTime.UtcNow) };
        _loadFailed = false;
        SaveManual(created);
        return created;
    }

    private static void SaveManual(ManualStats m)
    {
        if (_loadFailed)
        {
            return;
        }
        try
        {
            var tmp = ManualPath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(m, Json), new UTF8Encoding(false));
            File.Move(tmp, ManualPath, overwrite: true);
        }
        catch
        {
            // not critical
        }
    }

    public static ManualStats Manual()
    {
        lock (Gate)
        {
            return LoadManual();
        }
    }

    /// <summary>First start: takes over the figures of the old counter (baus.ini next to the exe).</summary>
    private static ManualStats? ImportLegacy()
    {
        try
        {
            var dirs = new List<string>();
            var exe = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exe) && Path.GetDirectoryName(exe) is { } exeDir)
            {
                dirs.Add(exeDir);
            }
            dirs.Add(Environment.CurrentDirectory);

            foreach (var dir in dirs)
            {
                var ini = Path.Combine(dir, "baus.ini");
                if (!File.Exists(ini))
                {
                    continue;
                }

                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var section = "";
                foreach (var raw in File.ReadAllText(ini).Split('\n'))
                {
                    var line = raw.Trim();
                    if (line.StartsWith('[') && line.EndsWith(']'))
                    {
                        section = line.Trim('[', ']');
                        continue;
                    }
                    var eq = line.IndexOf('=');
                    if (eq > 0)
                    {
                        values[section + "." + line[..eq].Trim()] = line[(eq + 1)..].Trim();
                    }
                }

                int Int(string key) => values.TryGetValue("dados." + key, out var v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0;

                var hist = new List<int>();
                if (values.TryGetValue("dados.hist", out var h))
                {
                    foreach (var part in h.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (int.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                        {
                            hist.Add(n);
                        }
                    }
                }
                var cur = Int("cur");
                var since = values.TryGetValue("auto.desde", out var d) && d.Length == 14
                    ? d
                    : Stamp(File.GetLastWriteTimeUtc(ini));

                return new ManualStats
                {
                    Name = values.TryGetValue("dados.nome", out var nome) && nome.Length > 0 ? nome : "Jogador",
                    Season = values.TryGetValue("dados.temporada", out var temp) ? temp : "",
                    Level = Math.Max(1, Int("nivelConta")),
                    Since = since,
                    BaseChests = Int("total") > 0 ? Int("total") : hist.Sum() + cur,
                    BaseMatches = Int("matches"),
                    BaseBest = Math.Max(Math.Max(hist.Count > 0 ? hist.Max() : 0, cur), Int("melhor")),
                    BaseWins = Int("vitorias"),
                    BaseLosses = Int("derrotas"),
                    BaseKills = Int("kills") + Int("killsPartida"),
                    BaseSeconds = Int("segundos"),
                };
            }
        }
        catch
        {
            // no import: start from zero
        }
        return null;
    }

    // ---- the figures ------------------------------------------------------------------------

    private static DateTime When(MatchRecord m) => m.MatchStartUtc ?? m.AnalyzedAtUtc;

    private static bool After(MatchRecord m, string since)
    {
        var t = When(m);
        if (t.Kind == DateTimeKind.Unspecified)
        {
            t = DateTime.SpecifyKind(t, DateTimeKind.Utc);
        }
        return string.CompareOrdinal(Stamp(t), since) > 0;
    }

    /// <param name="d">The matches of the selected profile.</param>
    /// <param name="live">The match in progress, when its replay could be read.</param>
    public static StatsSnapshot Compute(TrackerData d, MatchRecord? live)
    {
        var m = Manual();

        // Account level, read from the replays (the level at the start of the match). It only ever goes up by itself:
        // a level typed by hand (or F18) is kept until the replays catch up with it.
        var levelMatch = live is { AccountLevel: not null }
            ? live
            : d.Matches.Where(x => x.NotTrackedOwner == null && x.AccountLevel.HasValue).OrderByDescending(When).FirstOrDefault();
        if (levelMatch?.AccountLevel is int replayLevel && replayLevel >= 1)
        {
            var before = (m.Level, m.LevelOffset, m.LevelSynced, m.Recalibrate);
            if (!m.LevelSynced || m.Recalibrate)
            {
                // Same scale as the game (the usual case): follow the replay. Otherwise remember the gap.
                var sameScale = !m.Recalibrate && replayLevel >= m.Level - 3 && replayLevel <= m.Level + 50;
                m.LevelOffset = sameScale ? 0 : m.Level - replayLevel;
                if (sameScale)
                {
                    m.Level = Math.Max(m.Level, replayLevel);
                }
                m.LevelSynced = true;
                m.Recalibrate = false;
            }
            else
            {
                m.Level = Math.Max(m.Level, replayLevel + m.LevelOffset);
            }
            if (before != (m.Level, m.LevelOffset, m.LevelSynced, m.Recalibrate))
            {
                lock (Gate)
                {
                    var saved = LoadManual();
                    saved.Level = m.Level;
                    saved.LevelOffset = m.LevelOffset;
                    saved.LevelSynced = m.LevelSynced;
                    saved.Recalibrate = m.Recalibrate;
                    SaveManual(saved);
                }
            }
        }

        var finished = d.Matches.Where(x => x.NotTrackedOwner == null && After(x, m.Since)).ToList();
        var last = d.Matches.Where(x => x.NotTrackedOwner == null).OrderByDescending(When).FirstOrDefault();

        var chests = Math.Max(0, m.BaseChests + d.ManualAdjust + finished.Sum(x => x.ChestsOpened) + (live?.ChestsOpened ?? 0));
        var best = Math.Max(m.BaseBest, Math.Max(finished.Count > 0 ? finished.Max(x => x.ChestsOpened) : 0, live?.ChestsOpened ?? 0));
        var wins = m.BaseWins + finished.Count(x => x.Placement == 1);
        var losses = m.BaseLosses + finished.Count(x => x.Placement is > 1);
        var kills = m.BaseKills + finished.Sum(x => x.Eliminations ?? 0);
        var seconds = m.BaseSeconds + finished.Sum(x => x.DurationSeconds ?? 0);
        var cur = live != null ? live.ChestsOpened : last?.ChestsOpened ?? 0;
        var treasures = d.Matches.Sum(x => x.TreasureFinds.Count) + (live?.TreasureFinds.Count ?? 0);

        // The replay's own stats (no manual starting point): every match of the profile read from a replay.
        var own = d.Matches.Where(x => x.NotTrackedOwner == null && x.IsReplayOwner).ToList();
        var accuracies = own.Where(x => x.Accuracy.HasValue).Select(x => AccuracyPercent(x.Accuracy!.Value)).ToList();

        var autoChests = finished.Sum(x => x.ChestsOpened) + (live?.ChestsOpened ?? 0) + d.ManualAdjust;
        var snapshot = new StatsSnapshot(cur, chests, m.BaseMatches + finished.Count, best, wins, losses, m.Level,
            kills, live?.Eliminations ?? 0, seconds, m.Name, m.Season, treasures,
            m.BaseAssists + own.Sum(x => x.Assists ?? 0), m.BaseDamage + own.Sum(x => (x.WeaponDamage ?? 0) + (x.OtherDamage ?? 0)), own.Sum(x => x.DamageTaken ?? 0),
            own.Sum(x => x.DamageToStructures ?? 0), own.Sum(x => x.Revives ?? 0), own.Sum(x => x.MaterialsGathered ?? 0),
            own.Sum(x => x.MaterialsUsed ?? 0), AccuracyCombined(m, accuracies), own.Sum(x => (long)(x.TotalTraveled ?? 0)));
        Last = snapshot;
        _auto = new AutoParts(autoChests, finished.Count, finished.Count(x => x.Placement == 1), finished.Count(x => x.Placement is > 1),
            finished.Sum(x => x.Eliminations ?? 0), finished.Sum(x => x.DurationSeconds ?? 0),
            own.Sum(x => (x.WeaponDamage ?? 0) + (x.OtherDamage ?? 0)), own.Sum(x => x.Assists ?? 0), accuracies.Sum(), accuracies.Count);
        return snapshot;
    }

    /// <summary>The figures as last computed (what the overlay shows), for the manual editing prompts.</summary>
    public static StatsSnapshot? Last { get; private set; }
    private static AutoParts _auto = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>Accuracy: the replays' average, mixed with the accuracy typed by hand for the matches before them.</summary>
    private static double AccuracyCombined(ManualStats m, List<double> accuracies)
    {
        if (m.AccuracyBase is double baseAccuracy && m.BaseMatches > 0)
        {
            return (baseAccuracy * m.BaseMatches + accuracies.Sum()) / (m.BaseMatches + accuracies.Count);
        }
        return accuracies.Count > 0 ? accuracies.Average() : 0;
    }

    /// <summary>The replay gives the accuracy as a fraction (0.25) or already as a percentage (25): both end up as a percentage.</summary>
    public static double AccuracyPercent(float value) => value <= 1.0001f ? value * 100.0 : value;

    public static string Duration(int seconds) => $"{seconds / 3600} h {seconds % 3600 / 60:00} min";

    // ---- OBS files --------------------------------------------------------------------------

    private static string Js(StatsSnapshot s)
    {
        string Q(string v) => JsonSerializer.Serialize(v ?? "");
        return FormattableString.Invariant(
            $"window.DADOS = {{cur:{s.Cur},total:{s.Chests},partidas:{s.Matches},melhor:{s.Best},vitorias:{s.Wins},derrotas:{s.Losses},nivelConta:{s.Level},kills:{s.Kills},killsPartida:{s.KillsMatch},segundos:{s.Seconds},idioma:{Q(L.Code)},nome:{Q(s.Name)},temporada:{Q(s.Season)},tesouros:{s.Treasures},assistencias:{s.Assists},danoJogadores:{s.DamageDealt},danoRecebido:{s.DamageTaken},danoEstruturas:{s.DamageStructures},revives:{s.Revives},materiaisRecolhidos:{s.MaterialsGathered},materiaisUsados:{s.MaterialsUsed},precisao:{s.AccuracyAvg:0.0},distanciaBruta:{s.TotalTraveled}}};");
    }

    /// <summary>Rewrites obs\dados.js (read by overlay-obs.html and overlay-stats.html).</summary>
    public static void Export(TrackerData d, MatchRecord? live)
    {
        try
        {
            if (_folder.Length == 0)
            {
                return;
            }
            var snapshot = Compute(d, live);
            Directory.CreateDirectory(ObsFolder);
            var path = Path.Combine(ObsFolder, "dados.js");
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, Js(snapshot), new UTF8Encoding(false));
            File.Move(tmp, path, overwrite: true);
        }
        catch
        {
            // the OBS files are a convenience: never stop the counter for them
        }
    }

    private static void WriteStaticFiles()
    {
        try
        {
            Directory.CreateDirectory(ObsFolder);
        }
        catch
        {
            return;
        }
        foreach (var name in new[] { "overlay-obs.html", "overlay-stats.html", "bau-obs.png" })
        {
            try
            {
                using var src = typeof(Unified).Assembly.GetManifestResourceStream("obs-" + name);
                if (src == null)
                {
                    continue;
                }
                using var dst = File.Create(Path.Combine(ObsFolder, name));
                src.CopyTo(dst);
            }
            catch
            {
                // file in use by OBS: the previous copy stays
            }
        }
    }

    // ---- hotkeys / Stream Deck --------------------------------------------------------------

    /// <summary>Applies one of the manual corrections (see <see cref="ManualKeys"/>).</summary>
    public static void Apply(string action)
    {
        if (action == "edit")
        {
            OpenManualFile();
            return;
        }
        lock (Gate)
        {
            var m = LoadManual();
            switch (action)
            {
                case "chests+": m.BaseChests++; break;
                case "chests-": m.BaseChests = Math.Max(0, m.BaseChests - 1); break;
                case "win+": m.BaseWins++; break;
                case "win-": m.BaseWins = Math.Max(0, m.BaseWins - 1); break;
                case "loss+": m.BaseLosses++; break;
                case "loss-": m.BaseLosses = Math.Max(0, m.BaseLosses - 1); break;
                case "kill+": m.BaseKills++; break;
                case "kill-": m.BaseKills = Math.Max(0, m.BaseKills - 1); break;
                case "match+": m.BaseMatches++; break;
                case "match-": m.BaseMatches = Math.Max(0, m.BaseMatches - 1); break;
                case "level+": m.Level++; break;
                case "level-": m.Level = Math.Max(1, m.Level - 1); m.Recalibrate = true; break;
                case "level+10": m.Level += 10; break;
                case "time+": m.BaseSeconds += 600; break;
                case "time-": m.BaseSeconds = Math.Max(0, m.BaseSeconds - 600); break;
                default: return; // "refresh" and unknown actions: nothing to save
            }
            SaveManual(m);
        }
    }

    public static void OpenManualFile()
    {
        try
        {
            Process.Start(new ProcessStartInfo("notepad.exe", "\"" + ManualPath + "\"") { UseShellExecute = true });
        }
        catch
        {
            // no notepad: nothing to do
        }
    }

    private static int? ParseNumber(string text)
    {
        var clean = (text ?? "").Replace(" ", "").Replace("\u00A0", "").Replace(".", "").Replace(",", "");
        return int.TryParse(clean, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n >= 0 ? n : null;
    }

    /// <summary>
    /// A figure typed by hand in the settings page: level, name, season, or any of the counters. A counter is set to
    /// the value typed: the manual starting point becomes (value - what the replays added).
    /// </summary>
    public static void SetField(string field, string text)
    {
        lock (Gate)
        {
            var m = LoadManual();
            var a = _auto;
            switch (field)
            {
                case "level":
                    if (ParseNumber(text) is int level && level >= 1 && level != m.Level)
                    {
                        if (level < m.Level)
                        {
                            m.Recalibrate = true; // lowered by hand: the next replay tells the gap again
                        }
                        m.Level = level;
                    }
                    break;
                case "name":
                    m.Name = string.IsNullOrWhiteSpace(text) ? "Jogador" : text.Trim();
                    break;
                case "season":
                    m.Season = (text ?? "").Trim();
                    break;
                case "chests":
                    if (ParseNumber(text) is int chests) m.BaseChests = chests - a.Chests;
                    break;
                case "best":
                    if (ParseNumber(text) is int best) m.BaseBest = best;
                    break;
                case "matches":
                    if (ParseNumber(text) is int matches) m.BaseMatches = matches - a.Matches;
                    break;
                case "wins":
                    if (ParseNumber(text) is int wins) m.BaseWins = wins - a.Wins;
                    break;
                case "losses":
                    if (ParseNumber(text) is int losses) m.BaseLosses = losses - a.Losses;
                    break;
                case "kills":
                    if (ParseNumber(text) is int kills) m.BaseKills = kills - a.Kills;
                    break;
                case "time":
                {
                    var t = System.Text.RegularExpressions.Regex.Match((text ?? "").Trim(), @"^(\d+)\s*(?:[:hH]\s*(\d{1,2})\s*(?:m|min)?)?$");
                    if (t.Success)
                    {
                        var seconds = int.Parse(t.Groups[1].Value, CultureInfo.InvariantCulture) * 3600 + (t.Groups[2].Success ? int.Parse(t.Groups[2].Value, CultureInfo.InvariantCulture) * 60 : 0);
                        m.BaseSeconds = seconds - a.Seconds;
                    }
                    break;
                }
                case "damage":
                    if (ParseNumber(text) is int damage) m.BaseDamage = damage - a.OwnDamage;
                    break;
                case "assists":
                    if (ParseNumber(text) is int assists) m.BaseAssists = assists - a.OwnAssists;
                    break;
                case "accuracy":
                {
                    var clean = (text ?? "").Trim().Replace(',', '.').TrimEnd('%');
                    if (double.TryParse(clean, NumberStyles.Float, CultureInfo.InvariantCulture, out var x) && x >= 0 && x <= 100 && m.BaseMatches > 0)
                    {
                        // chosen so that the figure shown right now is exactly x; the next matches then move it
                        m.AccuracyBase = Math.Clamp((x * (m.BaseMatches + a.AccuracyCount) - a.AccuracySum) / m.BaseMatches, 0, 100);
                    }
                    break;
                }
                default:
                    return;
            }
            SaveManual(m);
        }
    }
}

/// <summary>
/// Global keys F13 to F24 (they do not exist on normal keyboards, so they never clash with the game).
/// They are the keys the Stream Deck profile sends. A key alone adds, with Ctrl it removes.
/// </summary>
[SupportedOSPlatform("windows")]
public static class ManualKeys
{
    private const uint WM_HOTKEY = 0x0312, MOD_CONTROL = 0x2, MOD_SHIFT = 0x4, MOD_NOREPEAT = 0x4000;
    private const uint F13 = 0x7C, F14 = 0x7D, F15 = 0x7E, F16 = 0x7F, F17 = 0x80, F18 = 0x81, F19 = 0x82, F20 = 0x83, F21 = 0x84, F22 = 0x85;

    private static readonly (uint Mods, uint Vk, string Action)[] Keys =
    {
        (0, F13, "chests+"), (MOD_CONTROL, F13, "chests-"),
        (0, F14, "win+"), (MOD_CONTROL, F14, "win-"),
        (0, F15, "loss+"), (MOD_CONTROL, F15, "loss-"),
        (0, F16, "kill+"), (MOD_CONTROL, F16, "kill-"),
        (0, F17, "match+"), (MOD_CONTROL, F17, "match-"),
        (0, F18, "level+"), (MOD_CONTROL, F18, "level-"), (MOD_SHIFT, F18, "level+10"),
        (0, F19, "time+"), (MOD_CONTROL, F19, "time-"),
        (0, F20, "refresh"),
        (0, F21, "edit"),
        (0, F22, "overlay"),
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct Msg
    {
        public IntPtr Hwnd;
        public uint Message;
        public UIntPtr WParam;
        public IntPtr LParam;
        public uint Time;
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint vk);
    [DllImport("user32.dll")] private static extern int GetMessageW(out Msg msg, IntPtr hwnd, uint min, uint max);

    /// <summary>Starts listening (own thread, no window). <paramref name="handler"/> gets the action name.</summary>
    public static void Start(Action<string> handler)
    {
        var thread = new Thread(() =>
        {
            for (var i = 0; i < Keys.Length; i++)
            {
                RegisterHotKey(IntPtr.Zero, 100 + i, Keys[i].Mods | MOD_NOREPEAT, Keys[i].Vk);
            }
            while (GetMessageW(out var msg, IntPtr.Zero, 0, 0) > 0)
            {
                if (msg.Message != WM_HOTKEY)
                {
                    continue;
                }
                var index = (int)msg.WParam.ToUInt64() - 100;
                if (index >= 0 && index < Keys.Length)
                {
                    try
                    {
                        handler(Keys[index].Action);
                    }
                    catch
                    {
                        // a failed refresh must not kill the listener
                    }
                }
            }
        })
        {
            IsBackground = true,
            Name = "ManualKeys",
        };
        thread.Start();
    }
}
