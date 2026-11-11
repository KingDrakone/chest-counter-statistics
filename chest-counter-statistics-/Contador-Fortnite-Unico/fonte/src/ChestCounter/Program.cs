using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using ChestCounter;

// --standby: the invisible process that waits for Fortnite to start ("open with Fortnite"). No console, nothing else.
if (args.Contains("--standby"))
{
    var dataIndex = Array.IndexOf(args, "--data");
    return OperatingSystem.IsWindows()
        ? FortniteWatch.RunStandby(dataIndex >= 0 && dataIndex + 1 < args.Length ? args[dataIndex + 1] : Store.DefaultFolder())
        : 1;
}

// Windows app: open a console window (or write into the terminal for command line options).
if (OperatingSystem.IsWindows())
{
    NativeConsole.Ensure(attachToParent: args.Any(a => a is "--once" or "--help" or "-h" or "--overlay-preview"));
}
Console.OutputEncoding = Encoding.UTF8;

// A crash must not close the window before the message can be read (the exe is usually double-clicked).
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    ErrorLog.Write("Crash", e.ExceptionObject);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine();
    Console.WriteLine(L.T("Erreur inattendue : ", "Unexpected error: ", "Error inesperado: ", "Erro inesperado: ") + e.ExceptionObject);
    Console.ResetColor();
    if (!Console.IsInputRedirected)
    {
        Console.WriteLine(L.T("Appuie sur une touche pour fermer.", "Press any key to close.", "Pulsa una tecla para cerrar.", "Prime uma tecla para fechar."));
        try { Console.ReadKey(intercept: true); } catch { }
    }
};

// ---- Arguments ---------------------------------------------------------------------------
string? dataDir = null, demosArg = null, langArg = null;
var once = false;
var reset = false;
var auto = false;          // opened by the standby when Fortnite started: closes itself after Fortnite
var afterUpdate = false;   // restarted by the updater: waits for the previous instance to quit
string? previewPath = null;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--data": dataDir = args[++i]; break;
        case "--demos": demosArg = args[++i]; break;
        case "--lang": langArg = args[++i]; break;
        case "--once": once = true; break;
        case "--reset-config": reset = true; break;
        case "--overlay-preview": previewPath = args[++i]; break;
        case "--auto": auto = true; break;
        case "--after-update": afterUpdate = true; break;
        case "-h":
        case "--help":
            var langIndex = Array.IndexOf(args, "--lang");
            L.Set(langIndex >= 0 && langIndex + 1 < args.Length ? L.Normalize(args[langIndex + 1]) ?? L.SystemDefault : L.SystemDefault);
            Console.WriteLine(L.T("""
                Compteur de coffres Fortnite

                Options :
                  --lang <fr|en|es>   langue de l'outil (français, anglais ou espagnol)
                  --demos <dossier>   dossier des replays (par défaut %LOCALAPPDATA%\FortniteGame\Saved\Demos)
                  --data <dossier>    dossier des données (par défaut %APPDATA%\ChestCounter)
                  --once              analyse les replays terminés puis quitte
                  --reset-config      repose les questions de configuration
                  --overlay-preview <fichier.png>   enregistre des aperçus de l'overlay puis quitte
                """, """
                Fortnite Chest Counter

                Options:
                  --lang <fr|en|es>   language of the tool (French, English or Spanish)
                  --demos <folder>    replays folder (default %LOCALAPPDATA%\FortniteGame\Saved\Demos)
                  --data <folder>     data folder (default %APPDATA%\ChestCounter)
                  --once              analyze the finished replays, then quit
                  --reset-config      ask the setup questions again
                  --overlay-preview <file.png>   save previews of the overlay, then quit
                """, """
                Contador de cofres de Fortnite

                Opciones:
                  --lang <fr|en|es>   idioma de la herramienta (francés, inglés o español)
                  --demos <carpeta>   carpeta de repeticiones (por defecto %LOCALAPPDATA%\FortniteGame\Saved\Demos)
                  --data <carpeta>    carpeta de datos (por defecto %APPDATA%\ChestCounter)
                  --once              analiza las repeticiones terminadas y sale
                  --reset-config      vuelve a hacer las preguntas de configuración
                  --overlay-preview <archivo.png>   guarda vistas previas del overlay y sale
                """, """
                Contador de baús do Fortnite

                Opções:
                  --lang <fr|en|es|pt>   idioma da ferramenta (francês, inglês, espanhol ou português)
                  --demos <pasta>     pasta das repetições (por defeito %LOCALAPPDATA%\FortniteGame\Saved\Demos)
                  --data <pasta>      pasta dos dados (por defeito %APPDATA%\ChestCounter)
                  --once              analisa as repetições terminadas e sai
                  --reset-config      volta a fazer as perguntas de configuração
                  --overlay-preview <ficheiro.png>   guarda pré-visualizações do overlay e sai
                """));
            return 0;
    }
}

dataDir ??= Store.DefaultFolder();
var store = new Store(dataDir);
ErrorLog.FilePath = store.ErrorLogPath;
var interactive = ConsoleUi.Interactive;
if (interactive && !once && previewPath == null)
{
    ConsoleUi.EnsureSize(110, 40);
}

// One instance at a time: two would count the same matches and fight over the shortcuts.
using var instance = new Mutex(false, AppInfo.InstanceMutexFor(dataDir));
if (previewPath == null && !AcquireInstance(instance, afterUpdate ? TimeSpan.FromSeconds(20) : TimeSpan.Zero))
{
    if (auto)
    {
        return 0; // opened with Fortnite while the player had already opened it: nothing to do
    }
    L.Set(store.LoadConfig()?.Language ?? L.SystemDefault);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(L.T("  Le compteur de coffres est déjà ouvert : regarde dans la barre des tâches.",
        "  The chest counter is already open: look in the taskbar.",
        "  El contador de cofres ya está abierto: búscalo en la barra de tareas.", "  O contador de baús já está aberto: procura-o na barra de tarefas."));
    Console.ResetColor();
    if (interactive)
    {
        Console.WriteLine(L.T("  (cette fenêtre se ferme dans 10 secondes)", "  (this window closes in 10 seconds)", "  (esta ventana se cierra en 10 segundos)", "  (esta janela fecha-se dentro de 10 segundos)"));
        Thread.Sleep(10_000);
    }
    return 0;
}

// ---- Configuration -----------------------------------------------------------------------
var config = reset ? null : store.LoadConfig();
var firstRun = config == null;
var previousLanguage = reset ? store.LoadConfig()?.Language : null;
config ??= new Config();

// Language: --lang, else the saved choice, else asked once (Windows language when nobody can answer).
if (langArg != null)
{
    config.Language = L.Normalize(langArg) ?? config.Language;
}
if (L.Normalize(config.Language) == null)
{
    var suggested = L.Normalize(previousLanguage) ?? L.SystemDefault;
    if (interactive)
    {
        ConsoleUi.Screen("Fortnite Chest Counter  ·  Compteur de coffres  ·  Contador de cofres", "1/5");
        ConsoleUi.Text("Choisis ta langue  /  Choose your language  /  Elige tu idioma");
        var choice = ConsoleUi.Menu(L.Codes.Select(L.NativeName).ToList(), Math.Max(0, Array.IndexOf(L.Codes, suggested)), "↑ ↓  +  Entrée / Enter / Intro");
        config.Language = L.Codes[choice];
    }
    else
    {
        config.Language = suggested;
    }
}
L.Set(config.Language);
var appName = L.T("Compteur de coffres Fortnite", "Fortnite Chest Counter", "Contador de cofres de Fortnite", "Contador de baús do Fortnite");
ConsoleUi.SetTitle($"{appName} {AppInfo.VersionText}");

if (demosArg != null) config.DemosFolder = demosArg;
var defaultDemos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FortniteGame", "Saved", "Demos");
if (string.IsNullOrWhiteSpace(config.DemosFolder))
{
    config.DemosFolder = defaultDemos;
}

if (previewPath != null)
{
    // Development: draws the overlay off-screen and saves it as PNG (idle + match in progress), then quits.
    var previewData = Profiles.View(store.LoadData(), config);
    // In memory only: the 3 latest matches count as "this session", to show a realistic session line.
    foreach (var m in previewData.Matches.OrderByDescending(m => m.MatchStartUtc ?? m.AnalyzedAtUtc).Take(3))
    {
        m.MatchStartUtc = DateTime.UtcNow;
    }
    if (!OperatingSystem.IsWindows())
    {
        return 1;
    }
    NativeOverlay.SetAppearance(config.OverlaySize, config.OverlayOpacity);
    // Locked, as in game. Each style in its three states: idle, match in progress, live count.
    NativeOverlay.Start(-20000, -20000, true, (_, _) => { }, _ => { });
    var fakeLive = new MatchRecord { ChestsOpened = 12, TreasureFinds = { new TreasureFind { Item = "Trésor" } } };
    foreach (var style in NativeOverlay.Styles)
    {
        NativeOverlay.SetStyle(style);
        foreach (var (suffix, live, inProgress) in new[] { ("-idle", (MatchRecord?)null, false), ("-inprogress", null, true), ("-live", fakeLive, true) })
        {
            NativeOverlay.Update(OverlayModel.Build(previewData, live, inProgress));
            Thread.Sleep(600);
            var name = Path.GetFileNameWithoutExtension(previewPath) + (style == "classic" ? "" : "-" + style) + suffix + ".png";
            var file = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(previewPath)) ?? ".", name);
            Console.WriteLine(NativeOverlay.Snapshot(file) ? L.T($"Aperçu : {file}", $"Preview: {file}", $"Vista previa: {file}", $"Pré-visualização: {file}") : L.T($"Échec de la capture : {file}", $"Capture failed: {file}", $"Error de captura: {file}", $"Falha na captura: {file}"));
        }
    }
    return 0;
}

LowerPriority();
var data = store.LoadData();
config.UpdateMode = "off"; // this build is a custom one: the official updater would replace it
Unified.Init(store);
var tracker = new Tracker(store, config, data);
// Before any scan: otherwise replays analyzed during the setup would be analyzed a second time right after.
var reanalyzing = tracker.UpgradeIfNeeded();
var reportOpened = false;

// ---- First launch: setup screens ----------------------------------------------------------
if (firstRun && interactive)
{
    // Step 2: the replays folder.
    ConsoleUi.Screen(appName, "2/5");
    ConsoleUi.Text(L.T("Tes replays", "Your replays", "Tus repeticiones", "As tuas repetições"), ConsoleColor.White);
    ConsoleUi.Blank();
    if (!Directory.Exists(config.DemosFolder))
    {
        ConsoleUi.Text(L.T("✖ Dossier des replays introuvable :", "✖ Replays folder not found:", "✖ Carpeta de repeticiones no encontrada:", "✖ Pasta das repetições não encontrada:"), ConsoleColor.Red);
        ConsoleUi.Hint("  " + config.DemosFolder);
        ConsoleUi.Blank();
        ConsoleUi.Text(L.T("Fortnite le crée à la première partie enregistrée. Si tu viens d'activer les replays, c'est normal.",
            "Fortnite creates it with the first recorded match. If you just turned replays on, that is expected.",
            "Fortnite la crea con la primera partida grabada. Si acabas de activar las repeticiones, es normal.", "O Fortnite cria-a com a primeira partida gravada. Se acabaste de ativar as repetições, é normal."));
        var where = ConsoleUi.Menu(new[]
        {
            L.T("Garder ce dossier", "Keep this folder", "Mantener esta carpeta", "Manter esta pasta"),
            L.T("Indiquer un autre dossier", "Enter another folder", "Indicar otra carpeta", "Indicar outra pasta"),
        });
        if (where == 1)
        {
            config.DemosFolder = ConsoleUi.Input(L.T("Chemin du dossier des replays : ", "Path of the replays folder: ", "Ruta de la carpeta de repeticiones: ", "Caminho da pasta das repetições: "), config.DemosFolder);
            ConsoleUi.Blank();
        }
    }
    else
    {
        ConsoleUi.Text(L.T("✔ Dossier des replays trouvé :", "✔ Replays folder found:", "✔ Carpeta de repeticiones encontrada:", "✔ Pasta das repetições encontrada:"), ConsoleColor.Green);
        ConsoleUi.Hint("  " + config.DemosFolder);
    }
    ConsoleUi.Blank();
    ConsoleUi.Text(L.T("Important : dans Fortnite, active  Paramètres → Jeu → Rediffusions → Enregistrer les rediffusions.",
        "Important: in Fortnite, turn on  Settings → Game → Replays → Record Replays.",
        "Importante: en Fortnite, activa  Ajustes → Juego → Repeticiones → Grabar repeticiones.", "Importante: no Fortnite, ativa  Definições → Jogo → Repetições → Gravar repetições."), ConsoleColor.Yellow);
    ConsoleUi.Hint(L.T("Sans ça, Fortnite n'enregistre aucun replay (« rediffusion ») et l'outil n'a rien à compter.",
        "Without it, Fortnite saves no replay and the tool has nothing to count.",
        "Sin eso, Fortnite no graba ninguna repetición y la herramienta no tiene nada que contar.", "Sem isso, o Fortnite não grava nenhuma repetição e a ferramenta não tem nada para contar."));
    ConsoleUi.Blank();

    // Replays already on the PC.
    var existing = tracker.UnprocessedFiles().Where(f => ReplayHeaderProbe.IsLive(f.FullName) != true).ToList();
    var analyzeNow = false;
    if (existing.Count > 0)
    {
        var present = L.Count(existing.Count, "replay est déjà présent", "replays sont déjà présents", "replay is already on your PC", "replays are already on your PC",
            "repetición ya está en tu PC", "repeticiones ya están en tu PC", "repetição já está no teu PC", "repetições já estão no teu PC");
        ConsoleUi.Text(L.T($"{present} sur ton PC. Les compter aussi ?", $"{present}. Count them too?", $"{present}. ¿Contarlas también?", $"{present}. Contá-las também?"));
        analyzeNow = ConsoleUi.Menu(new[]
        {
            L.T("Oui, les analyser maintenant", "Yes, analyze them now", "Sí, analizarlas ahora", "Sim, analisá-las agora"),
            L.T("Non, seulement mes prochaines parties", "No, only my next matches", "No, solo mis próximas partidas", "Não, só as minhas próximas partidas"),
        }) == 0;
        if (!analyzeNow)
        {
            tracker.MarkSkipped(existing);
        }
    }
    else
    {
        ConsoleUi.WaitEnter(L.T("Entrée pour continuer", "Enter to continue", "Intro para continuar", "Enter para continuar"));
    }
    store.SaveConfig(config);

    if (analyzeNow)
    {
        ConsoleUi.Screen(appName, L.T("analyse", "analysis", "análisis", "análise"));
        ConsoleUi.Text(L.T("Analyse de tes replays…", "Analyzing your replays…", "Analizando tus repeticiones…", "A analisar as tuas repetições…"), ConsoleColor.White);
        ConsoleUi.Hint(L.T("Le rapport s'ouvre dans ton navigateur et se remplit au fur et à mesure.",
            "The report opens in your browser and fills in as the replays are read.",
            "El informe se abre en tu navegador y se completa al leer las repeticiones.", "O relatório abre no teu navegador e completa-se à medida que as repetições são lidas."));
        ConsoleUi.Blank();
        ReportWriter.Progress = new ScanProgress(0, existing.Count);
        ReportWriter.WriteAll(store, config, data);
        if (config.OpenReportOnStart)
        {
            Open(store.ReportPath);
            reportOpened = true;
        }
        tracker.Scan();
        tracker.NewProfilesFound = false;
        ConsoleUi.Blank();
        if (Profiles.List(data).Count < 2)
        {
            ConsoleUi.WaitEnter(L.T("Entrée pour continuer", "Enter to continue", "Intro para continuar", "Enter para continuar"));
        }
    }

    if (Profiles.List(data).Count >= 2)
    {
        ConsoleUi.Screen(appName, L.T("profil", "profile", "perfil", "perfil"));
        ChooseProfile(data, config, store);
    }

    // Step 3: the overlay (some players only want the report).
    if (OperatingSystem.IsWindows())
    {
        ConsoleUi.Screen(appName, "3/5");
        ConsoleUi.Text(L.T("Afficher un overlay en jeu ?", "Show an overlay in game?", "¿Mostrar un overlay en el juego?", "Mostrar um overlay no jogo?"), ConsoleColor.White);
        ConsoleUi.Blank();
        ConsoleUi.Text(L.T("Une petite fenêtre toujours au premier plan affiche tes coffres pendant que tu joues.",
            "A small always-on-top window shows your chests while you play.",
            "Una pequeña ventana siempre visible muestra tus cofres mientras juegas.", "Uma pequena janela sempre visível mostra os teus baús enquanto jogas."));
        ConsoleUi.Hint(L.T("Sans overlay, tes statistiques restent dans le rapport. Modifiable à tout moment dans les réglages.",
            "Without it, your statistics are in the report. Can be changed at any time in the settings.",
            "Sin overlay, tus estadísticas están en el informe. Se puede cambiar en cualquier momento en los ajustes.", "Sem overlay, as tuas estatísticas ficam no relatório. Podes mudar isto a qualquer momento nas definições."));
        config.ShowOverlay = ConsoleUi.Menu(new[]
        {
            L.T("Oui, afficher l'overlay", "Yes, show the overlay", "Sí, mostrar el overlay", "Sim, mostrar o overlay"),
            L.T("Non, seulement le rapport", "No, only the report", "No, solo el informe", "Não, só o relatório"),
        }) == 0;
        store.SaveConfig(config);
    }

    // Step 4: open with Fortnite. Nothing is recommended: most players simply start it themselves.
    if (OperatingSystem.IsWindows() && AppInfo.ExePath != null)
    {
        ConsoleUi.Screen(appName, "4/5");
        ConsoleUi.Text(L.T("Ouvrir le compteur automatiquement quand Fortnite démarre ?", "Open the counter automatically when Fortnite starts?", "¿Abrir el contador automáticamente cuando se inicia Fortnite?", "Abrir o contador automaticamente quando o Fortnite arranca?"), ConsoleColor.White);
        ConsoleUi.Blank();
        ConsoleUi.Text(L.T("Un tout petit processus invisible attend le lancement de Fortnite, puis ouvre le compteur.\nLe compteur se ferme tout seul après Fortnite.",
            "A tiny invisible process waits for Fortnite to start, then opens the counter.\nThe counter closes by itself after Fortnite.",
            "Un pequeño proceso invisible espera a que se inicie Fortnite y luego abre el contador.\nEl contador se cierra solo después de Fortnite.", "Um pequeno processo invisível espera que o Fortnite arranque e depois abre o contador.\nO contador fecha-se sozinho depois do Fortnite."));
        ConsoleUi.Hint(L.T("Modifiable à tout moment dans les réglages. Désactive-le avant de supprimer ChestCounter.exe.",
            "Can be changed at any time in the settings. Turn it off before deleting ChestCounter.exe.",
            "Se puede cambiar en cualquier momento en los ajustes. Desactívalo antes de borrar ChestCounter.exe.", "Podes mudar isto a qualquer momento nas definições. Desativa-o antes de apagar o ChestCounter.exe."));
        var withFortnite = ConsoleUi.Menu(new[]
        {
            L.T("Non, je le lancerai moi-même", "No, I will start it myself", "No, lo abriré yo mismo", "Não, abro-o eu mesmo"),
            L.T("Oui, l'ouvrir avec Fortnite", "Yes, open it with Fortnite", "Sí, abrirlo con Fortnite", "Sim, abrir com o Fortnite"),
        }) == 1;
        config.StartWithFortnite = withFortnite && FortniteWatch.Apply(true, dataDir);
        store.SaveConfig(config);
    }

    // Step 5: ready.
    ConsoleUi.Screen(appName, "5/5");
    ConsoleUi.Text(L.T("C'est prêt !", "All set!", "¡Todo listo!", "Tudo pronto!"), ConsoleColor.Green);
    ConsoleUi.Blank();
    ConsoleUi.Text(L.T("Laisse cette fenêtre ouverte (tu peux la réduire) pendant que tu joues :\nles coffres de chaque partie sont comptés quelques secondes après sa fin.",
        "Keep this window open (you can minimize it) while you play:\nthe chests of each match are counted a few seconds after it ends.",
        "Deja esta ventana abierta (puedes minimizarla) mientras juegas:\nlos cofres de cada partida se cuentan unos segundos después de que termine.", "Deixa esta janela aberta (podes minimizá-la) enquanto jogas:\nos baús de cada partida são contados alguns segundos depois de ela terminar."));
    ConsoleUi.Blank();
    ConsoleUi.Text(L.T("Tout se fait avec les flèches et Entrée. Les réglages et l'aide sont dans les onglets en haut de l'écran.",
        "Everything works with the arrow keys and Enter. Settings and help are in the tabs at the top of the screen.",
        "Todo funciona con las flechas e Intro. Los ajustes y la ayuda están en las pestañas de arriba.", "Tudo funciona com as setas e o Enter. As definições e a ajuda estão nos separadores de cima."));
    if (config.ShowOverlay)
    {
        ConsoleUi.Blank();
        ConsoleUi.Text(L.T("Overlay : Ctrl+Maj+O pour l'afficher ou le masquer, Ctrl+Maj+L pour le verrouiller (même en jeu).",
            "Overlay: Ctrl+Shift+O to show or hide it, Ctrl+Shift+L to lock it (even in game).",
            "Overlay: Ctrl+Mayús+O para mostrarlo u ocultarlo, Ctrl+Mayús+L para bloquearlo (incluso en el juego).", "Overlay: Ctrl+Shift+O para o mostrar ou esconder, Ctrl+Shift+L para o bloquear (mesmo no jogo)."));
        ConsoleUi.Hint(L.T("Pour le voir en jeu, mets Fortnite en « Plein écran fenêtré » (Paramètres → Vidéo → Mode de fenêtre).",
            "To see it in game, set Fortnite to \"Windowed Fullscreen\" (Settings → Video → Window Mode).",
            "Para verlo en el juego: Ajustes → Vídeo → Modo ventana → «Pantalla completa en ventana».", "Para o ver no jogo: Definições → Vídeo → Modo de janela → «Ecrã inteiro em janela»."));
    }
    ConsoleUi.WaitEnter(L.T("Appuie sur Entrée pour démarrer", "Press Enter to start", "Pulsa Intro para empezar", "Prime Enter para começar"));
}
else
{
    store.SaveConfig(config);
    if (interactive && !once && Profiles.List(data).Count >= 2)
    {
        ConsoleUi.Screen(appName, L.T("profil", "profile", "perfil", "perfil"));
        ChooseProfile(data, config, store);
    }
}

// ---- Command line: analyze once and quit ---------------------------------------------------
if (once)
{
    ConsoleUi.Screen(appName, $"v{AppInfo.VersionText}");
    tracker.Scan(ignoreQuietPeriod: true);
    ReportWriter.WriteAll(store, config, data);
    tracker.PrintSummary();
    Console.WriteLine(L.T($"  Rapport : {store.ReportPath}", $"  Report: {store.ReportPath}", $"  Informe: {store.ReportPath}", $"  Relatório: {store.ReportPath}"));
    return 0;
}

// ---- Main screen ---------------------------------------------------------------------------
// One screen, redrawn in place: tabs (home / settings / help), a menu driven by the arrow keys, and an activity
// area that shows what the counter prints (captured from Console.Out).
var quit = false;
var restart = false;
var wake = new AutoResetEvent(false);
var keys = new BlockingCollection<ConsoleKeyInfo>();
var log = new ActivityLog();
var mainThread = Environment.CurrentManagedThreadId;
var dashboard = false;
var lastDraw = DateTime.MinValue;
var tab = 0;
var selection = new int[3];
UpdateInfo? update = null;
var updateAnnounced = false;
var notesHidden = false;
var showWhatsNew = !firstRun && config.LastVersion != AppInfo.VersionText;
if (!showWhatsNew && config.LastVersion != AppInfo.VersionText)
{
    config.LastVersion = AppInfo.VersionText;
    store.SaveConfig(config);
}
DateTime? autoCloseAt = null;
DateTime? fortniteClosedSince = null;
FileSystemWatcher? watcher = null;
var watcherFired = false;
var nextScan = DateTime.UtcNow.AddSeconds(2);

if (interactive)
{
    log.Changed += () =>
    {
        if (!dashboard)
        {
            return;
        }
        if (Environment.CurrentManagedThreadId == mainThread)
        {
            // While a scan runs (progress bar), redraw at most ~12 times a second.
            if ((DateTime.UtcNow - lastDraw).TotalMilliseconds >= 80) Draw();
        }
        else
        {
            wake.Set(); // written by another thread (overlay): the main loop redraws
        }
    };
    Console.SetOut(log);
    dashboard = true;
    try { Console.Clear(); } catch { }
    Frames.Invalidate();
    Draw();
}

if (reanalyzing)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(L.T("Nouvelle version de l'analyse : les replays encore présents vont être réanalysés (sans doublon).",
        "New analysis version: the replays still on disk will be analyzed again (no duplicates).",
        "Nueva versión del análisis: las repeticiones del disco se analizarán de nuevo (sin duplicados).", "Nova versão da análise: as repetições do disco serão analisadas de novo (sem duplicados)."));
    Console.ResetColor();
}

if (config.ShowOverlay)
{
    StartOverlay();
}

if (OperatingSystem.IsWindows())
{
    Updater.CleanupOld();
    if (config.StartWithFortnite)
    {
        FortniteWatch.Apply(true, dataDir); // follows the exe if it was moved or updated, and restarts the standby
    }
    if (Process.GetProcessesByName("ChestTracker") is { Length: > 0 } oldTool)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(L.T("L'ancienne version (ChestTracker.exe) est encore ouverte : ferme-la, sinon les deux comptent en même temps.",
            "The old version (ChestTracker.exe) is still open: close it, otherwise both count at the same time.",
            "La versión antigua (ChestTracker.exe) sigue abierta: ciérrala, si no las dos cuentan a la vez.", "A versão antiga (ChestTracker.exe) continua aberta: fecha-a, senão as duas contam ao mesmo tempo."));
        Console.ResetColor();
        foreach (var p in oldTool) p.Dispose();
    }
}

// Nothing to read yet: explain why, so that a new user is not left wondering.
if (!tracker.ReplayFiles().Any())
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine(Directory.Exists(config.DemosFolder)
        ? L.T("Aucun replay pour l'instant : c'est normal si tu n'as pas encore joué depuis que les replays sont activés.",
              "No replay yet: that is expected if you have not played since replays were turned on.",
              "Aún no hay repeticiones: es normal si no has jugado desde que activaste las repeticiones.", "Ainda não há repetições: é normal se não jogaste desde que ativaste as repetições.")
        : L.T("Le dossier des replays n'existe pas encore : Fortnite le crée à la première partie enregistrée.",
              "The replays folder does not exist yet: Fortnite creates it with the first recorded match.",
              "La carpeta de repeticiones aún no existe: Fortnite la crea con la primera partida grabada.", "A pasta das repetições ainda não existe: o Fortnite cria-a com a primeira partida gravada."));
    Console.WriteLine(L.T("Vérifie dans Fortnite : Paramètres → Jeu → Rediffusions → Enregistrer les rediffusions.",
        "Check in Fortnite: Settings → Game → Replays → Record Replays.",
        "Compruébalo en Fortnite: Ajustes → Juego → Repeticiones → Grabar repeticiones.", "Confirma no Fortnite: Definições → Jogo → Repetições → Gravar repetições."));
    Console.ResetColor();
}

// Replays left from earlier: the report opens with a loading card, the screen shows a progress bar.
var backlog = tracker.BacklogCount();
if (backlog > 1)
{
    ReportWriter.Progress = new ScanProgress(0, backlog);
}
ReportWriter.WriteAll(store, config, data);
if (config.OpenReportOnStart && !reportOpened && !auto)
{
    Open(store.ReportPath);
}
if (backlog > 0)
{
    tracker.Scan();
    ReportWriter.Progress = null;
    ReportWriter.WriteAll(store, config, data);
}
AnnounceNewProfiles();
UpdateOverlay(store, config, data, tracker.LiveMatch, tracker.IsLive);

// Stream Deck / manual keys (F13 to F24): corrections and level, applied at once.
if (OperatingSystem.IsWindows())
{
    ManualKeys.Start(action =>
    {
        if (action == "overlay")
        {
            if (NativeOverlay.IsRunning)
            {
                NativeOverlay.Toggle();
            }
            return;
        }
        Unified.Apply(action);
        if (action != "edit")
        {
            UpdateOverlay(store, config, data, tracker.LiveMatch, tracker.IsLive);
        }
    });
}

if (auto && OperatingSystem.IsWindows() && !(config.MinimizeToTray && interactive))
{
    NativeConsole.Minimize(); // opened with Fortnite: the game keeps the screen
}

// New version on GitHub (checked in the background; the only network access of the tool).
if (config.UpdateMode != "off")
{
    _ = Task.Run(async () =>
    {
        var found = await Updater.CheckAsync();
        if (found != null)
        {
            Volatile.Write(ref update, found);
            wake.Set();
        }
    });
}

Console.CancelKeyPress += (_, e) => { e.Cancel = true; quit = true; wake.Set(); };

// Minimized: into the notification area (classic console window only).
if (interactive && OperatingSystem.IsWindows())
{
    Tray.Enabled = config.MinimizeToTray;
    Tray.OpenReport = () => Open(store.ReportPath);
    Tray.Quit = () => { quit = true; wake.Set(); };
    Tray.Start();
    if (auto && config.MinimizeToTray && Tray.Available)
    {
        Tray.Hide(notify: false); // opened with Fortnite: straight to the tray, the game keeps the screen
    }
    else if (auto && config.MinimizeToTray)
    {
        NativeConsole.Minimize(); // no tray for this window (terminal): minimized as before
    }
}

// Keyboard: one thread blocked on ReadKey (no polling). Menus opened from the main loop read the same queue.
if (interactive)
{
    new Thread(() =>
    {
        while (true)
        {
            try { keys.Add(Console.ReadKey(intercept: true)); wake.Set(); }
            catch { return; }
        }
    }) { IsBackground = true, Name = "Keyboard" }.Start();
    ConsoleUi.ReadKey = () => keys.Take();
}

WatchReplays();

nextScan = DateTime.UtcNow.AddSeconds(2);
while (!quit)
{
    if (DateTime.UtcNow >= nextScan)
    {
        try
        {
            tracker.Scan();
            AnnounceNewProfiles();
            // Match in progress: re-read the replay being recorded (throttled inside).
            tracker.UpdateLive();
            UpdateOverlay(store, config, data, tracker.LiveMatch, tracker.IsLive);
        }
        catch (Exception ex)
        {
            Console.WriteLine(L.T($"Erreur pendant la surveillance : {ex.Message}", $"Error while watching: {ex.Message}", $"Error durante la vigilancia: {ex.Message}", $"Erro durante a vigilância: {ex.Message}"));
        }
        // A replay being written: check again soon. Otherwise only a safety scan once a minute.
        nextScan = DateTime.UtcNow.AddSeconds(tracker.IsFinishing ? 2 : tracker.HasPending ? 15 : (watcher != null && !auto ? 60 : 10));
    }

    if (auto && OperatingSystem.IsWindows())
    {
        CheckAutoClose();
    }

    var pendingUpdate = Volatile.Read(ref update);
    if (pendingUpdate != null && !updateAnnounced)
    {
        var idle = !tracker.IsLive && !tracker.HasPending && !tracker.IsFinishing;
        if (config.UpdateMode == "auto")
        {
            if (idle && AppInfo.ExePath != null)
            {
                updateAnnounced = true;
                InstallUpdate(pendingUpdate);
            }
        }
        else
        {
            updateAnnounced = true;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(L.T($"Nouvelle version {pendingUpdate.Version.ToString(3)} disponible : ses nouveautés sont affichées dans l'accueil.",
                $"New version {pendingUpdate.Version.ToString(3)} available: what's new is shown on the home tab.",
                $"Nueva versión {pendingUpdate.Version.ToString(3)} disponible: las novedades se muestran en el inicio.", $"Nova versão {pendingUpdate.Version.ToString(3)} disponível: as novidades aparecem no início."));
            Console.ResetColor();
        }
    }

    if (quit)
    {
        break;
    }
    Draw();

    // Sleep until something happens (file change, key press) or the next scheduled scan. At most one second, so that
    // a resized window or the auto-close countdown are redrawn.
    var wait = nextScan - DateTime.UtcNow;
    if (interactive && wait > TimeSpan.FromSeconds(1))
    {
        wait = TimeSpan.FromSeconds(1);
    }
    if (wait > TimeSpan.Zero && wake.WaitOne(wait))
    {
        // A file change: check again within 2 s (the readiness checks decide when a replay is complete).
        if (keys.Count == 0 && !quit && watcherFired)
        {
            watcherFired = false;
            nextScan = DateTime.UtcNow.AddSeconds(2) < nextScan ? DateTime.UtcNow.AddSeconds(2) : nextScan;
        }
    }

    while (!quit && keys.TryTake(out var key))
    {
        if (autoCloseAt != null)
        {
            // Any key keeps the counter open after Fortnite.
            autoCloseAt = null;
            auto = false;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(L.T("Le compteur reste ouvert.", "The counter stays open.", "El contador sigue abierto.", "O contador continua aberto."));
            Console.ResetColor();
            continue;
        }
        HandleKey(key);
    }
}

dashboard = false;
watcher?.Dispose();
if (OperatingSystem.IsWindows())
{
    NativeOverlay.Stop();
    Tray.Stop();
}
Console.SetOut(ConsoleUi.Out);
if (interactive)
{
    try { Console.Clear(); } catch { }
    ConsoleUi.RestoreCursor(true);
}

if (restart && AppInfo.ExePath is { } newExe)
{
    try { instance.ReleaseMutex(); } catch { }
    var restartArgs = string.Join(" ", args.Where(a => a is not ("--after-update" or "--reset-config")).Select(a => a.Contains(' ') ? $"\"{a}\"" : a).Append("--after-update"));
    Process.Start(new ProcessStartInfo(newExe, restartArgs) { UseShellExecute = true })?.Dispose();
    return 0;
}
if (restart)
{
    Console.WriteLine(L.T("  Relance le compteur pour continuer.", "  Start the counter again to continue.", "  Vuelve a abrir el contador para continuar.", "  Volta a abrir o contador para continuar."));
}

Console.WriteLine(L.T("  À bientôt !", "  See you soon!", "  ¡Hasta pronto!", "  Até breve!"));
return 0;

// ---- Main screen: drawing ---------------------------------------------------------------------

// A menu entry of the main screen: its label, what Enter does, and a line that explains it.
static (string Label, Action Run, string? About) Item(string label, Action run, string? about = null) => (label, run, about);

List<(string Label, Action Run, string? About)> Items()
{
    var list = new List<(string Label, Action Run, string? About)>();
    switch (tab)
    {
        case 0:
            list.Add(Item(L.T("Ouvrir le rapport (statistiques détaillées)", "Open the report (detailed statistics)", "Abrir el informe (estadísticas detalladas)", "Abrir o relatório (estatísticas detalhadas)"),
                () => Open(store.ReportPath)));
            if (OperatingSystem.IsWindows() && NativeOverlay.IsRunning)
            {
                list.Add(Item(NativeOverlay.IsVisible ? L.T("Masquer l'overlay", "Hide the overlay", "Ocultar el overlay", "Esconder o overlay") : L.T("Afficher l'overlay", "Show the overlay", "Mostrar el overlay", "Mostrar o overlay"),
                    NativeOverlay.Toggle));
                list.Add(Item(NativeOverlay.IsLocked ? L.T("Déverrouiller l'overlay (pour le déplacer)", "Unlock the overlay (to move it)", "Desbloquear el overlay (para moverlo)", "Desbloquear o overlay (para o mover)")
                        : L.T("Verrouiller l'overlay (la souris passe au travers)", "Lock the overlay (the mouse goes through it)", "Bloquear el overlay (el ratón lo atraviesa)", "Bloquear o overlay (o rato atravessa-o)"),
                    NativeOverlay.ToggleLock));
            }
            if (Profiles.List(data).Count >= 2)
            {
                list.Add(Item(L.T("Changer de profil", "Switch profile", "Cambiar de perfil", "Mudar de perfil"), () =>
                {
                    ConsoleUi.Screen(appName, L.T("profil", "profile", "perfil", "perfil"));
                    ChooseProfile(data, config, store);
                    ReportWriter.WriteAll(store, config, data);
                    UpdateOverlay(store, config, data, tracker.LiveMatch, tracker.IsLive);
                }));
            }
            if (Volatile.Read(ref update) is { } u)
            {
                list.Add(Item(L.T($"Installer la version {u.Version.ToString(3)}", $"Install version {u.Version.ToString(3)}", $"Instalar la versión {u.Version.ToString(3)}", $"Instalar a versão {u.Version.ToString(3)}"),
                    () => InstallUpdate(u)));
                list.Add(notesHidden
                    ? Item(L.T("Voir les nouveautés", "See what's new", "Ver las novedades", "Ver as novidades"), () => notesHidden = false)
                    : Item(L.T("Plus tard (masquer les nouveautés)", "Later (hide what's new)", "Más tarde (ocultar las novedades)", "Mais tarde (esconder as novidades)"), () => notesHidden = true));
            }
            else if (config.UpdateMode != "off")
            {
                list.Add(Item(L.T("Rechercher une mise à jour", "Check for updates", "Buscar actualizaciones", "Procurar atualizações"), CheckForUpdate));
            }
            if (showWhatsNew && Volatile.Read(ref update) == null)
            {
                list.Add(Item(L.T("OK, masquer les nouveautés", "OK, hide what's new", "OK, ocultar las novedades", "OK, esconder as novidades"), () =>
                {
                    showWhatsNew = false;
                    config.LastVersion = AppInfo.VersionText;
                    store.SaveConfig(config);
                }));
            }
            list.Add(Item(L.T("Quitter", "Quit", "Salir", "Sair"), () => quit = true));
            break;

        case 1:
            string YesNo(bool b) => b ? L.T("oui", "yes", "sí", "sim") : L.T("non", "no", "no", "não");
            string Percent(int p) => L.T($"{p} %", $"{p}%", $"{p} %", $"{p}%");
            list.Add(Item($"{L.T("Niveau du compte", "Account level", "Nivel de la cuenta", "Nível da conta")} : {Unified.Manual().Level}",
                () => Unified.Edit(appName, "level"),
                L.T("Le niveau suit les replays (montée de niveau détectée). Entrée pour le corriger (aussi : F18 +1, Ctrl+F18 -1, Maj+F18 +10).", "The level follows the replays (level-ups are detected). Enter to correct it (also: F18 +1, Ctrl+F18 -1, Shift+F18 +10).", "El nivel sigue las repeticiones (se detectan las subidas). Intro para corregirlo (también: F18 +1, Ctrl+F18 -1, Mayús+F18 +10).", "O nível acompanha as repetições (as subidas são detetadas). Enter para o corrigir (também: F18 +1, Ctrl+F18 -1, Shift+F18 +10).")));
            (string Label, Action Run, string? About) EditItem(string field, string value, string fr, string en, string es, string pt) =>
                Item($"✎ {L.T(fr, en, es, pt)} : {value}", () =>
                {
                    Unified.Edit(appName, field);
                    UpdateOverlay(store, config, data, tracker.LiveMatch, tracker.IsLive);
                }, L.T("Entrée pour saisir la valeur à la main. Le compteur continue d'ajouter les prochaines parties à partir de cette valeur.",
                    "Enter to type the value by hand. The counter keeps adding the next matches from that value.",
                    "Intro para escribir el valor a mano. El contador sigue sumando las próximas partidas a partir de ese valor.",
                    "Enter para escrever o valor à mão. O contador continua a somar as próximas partidas a partir desse valor."));
            var figures = Unified.Last;
            var playSeconds = figures?.Seconds ?? 0;
            list.Add(EditItem("chests", L.N(figures?.Chests ?? 0), "Coffres au total", "Total chests", "Cofres en total", "Baús no total"));
            list.Add(EditItem("best", L.N(figures?.Best ?? 0), "Meilleur score", "Best match", "Mejor marca", "Melhor marca"));
            list.Add(EditItem("matches", L.N(figures?.Matches ?? 0), "Parties", "Matches", "Partidas", "Partidas"));
            list.Add(EditItem("wins", L.N(figures?.Wins ?? 0), "Victoires", "Wins", "Victorias", "Vitórias"));
            list.Add(EditItem("losses", L.N(figures?.Losses ?? 0), "Défaites", "Losses", "Derrotas", "Derrotas"));
            list.Add(EditItem("kills", L.N(figures?.Kills ?? 0), "Kills", "Kills", "Kills", "Kills"));
            list.Add(EditItem("time", $"{playSeconds / 3600} h {playSeconds % 3600 / 60:00} min", "Temps de jeu", "Play time", "Tiempo de juego", "Tempo de jogo"));
            list.Add(EditItem("damage", L.N(figures?.DamageDealt ?? 0), "Dégâts", "Damage", "Daño", "Dano"));
            list.Add(EditItem("assists", L.N(figures?.Assists ?? 0), "Assistances", "Assists", "Asistencias", "Assistências"));
            list.Add(EditItem("accuracy", $"{(figures?.AccuracyAvg ?? 0):0.0} %", "Précision", "Accuracy", "Precisión", "Precisão"));
            list.Add(Item($"{L.T("Nom (cartes OBS)", "Name (OBS cards)", "Nombre (tarjetas OBS)", "Nome (cartões OBS)")} : {Unified.Manual().Name}",
                () => Unified.Edit(appName, "name"),
                L.T("Nom affiché sur la carte de statistiques d'OBS.", "Name shown on the OBS stats card.", "Nombre que muestra la tarjeta de estadísticas de OBS.", "Nome que o cartão de estatísticas do OBS mostra.")));
            list.Add(Item($"{L.T("Saison (cartes OBS)", "Season (OBS cards)", "Temporada (tarjetas OBS)", "Temporada (cartões OBS)")} : {Unified.Manual().Season}",
                () => Unified.Edit(appName, "season"),
                L.T("Ex. : CHAPTER 7 SEASON MS. Affichée sur la carte de statistiques d'OBS.", "E.g. CHAPTER 7 SEASON MS. Shown on the OBS stats card.", "Ej.: CHAPTER 7 SEASON MS. Se muestra en la tarjeta de estadísticas de OBS.", "Ex.: CHAPTER 7 SEASON MS. Aparece no cartão de estatísticas do OBS.")));
            list.Add(Item(L.T("Ouvrir le dossier des fichiers OBS", "Open the OBS files folder", "Abrir la carpeta de archivos de OBS", "Abrir a pasta dos ficheiros OBS"), () => Open(Unified.ObsFolder),
                L.T($"overlay-obs.html et overlay-stats.html (source navigateur locale dans OBS) : {Unified.ObsFolder}", $"overlay-obs.html and overlay-stats.html (local browser source in OBS): {Unified.ObsFolder}", $"overlay-obs.html y overlay-stats.html (fuente de navegador local en OBS): {Unified.ObsFolder}", $"overlay-obs.html e overlay-stats.html (fonte de navegador local no OBS): {Unified.ObsFolder}")));
            list.Add(Item($"{L.T("Langue", "Language", "Idioma", "Idioma")} : {L.NativeName(L.Code)}", CycleLanguage,
                L.T("Langue du compteur, du rapport et de l'overlay. Entrée pour passer à la suivante.",
                    "Language of the counter, the report and the overlay. Enter for the next one.",
                    "Idioma del contador, del informe y del overlay. Intro para pasar al siguiente.", "Idioma do contador, do relatório e do overlay. Enter para passar ao seguinte.")));
            if (OperatingSystem.IsWindows())
            {
                list.Add(Item($"{L.T("Overlay en jeu", "In-game overlay", "Overlay en el juego", "Overlay no jogo")} : {(config.ShowOverlay ? L.T("activé", "on", "activado", "ativado") : L.T("désactivé", "off", "desactivado", "desativado"))}",
                    ToggleOverlay,
                    L.T("Petite fenêtre toujours au premier plan qui affiche tes coffres en jeu. Désactivé : aucune fenêtre ni raccourci, tout reste dans le rapport.",
                        "Small always-on-top window that shows your chests in game. Off: no window and no shortcut, everything stays in the report.",
                        "Pequeña ventana siempre visible que muestra tus cofres en el juego. Desactivado: ni ventana ni atajos, todo queda en el informe.", "Pequena janela sempre visível que mostra os teus baús no jogo. Desativado: sem janela nem atalhos, tudo fica no relatório.")));
                if (config.ShowOverlay)
                {
                    string StyleName(string s) => s == "classic"
                        ? L.T("classique (carte sombre)", "classic (dark card)", "clásico (tarjeta oscura)", "clássico (cartão escuro)")
                        : s == "cards"
                            ? L.T("cartes (niveau et toutes les stats)", "cards (level and all the stats)", "tarjetas (nivel y todas las stats)", "cartões (nível e todas as estatísticas)")
                            : L.T("HUD (intégré au jeu)", "HUD (blends into the game)", "HUD (integrado en el juego)", "HUD (integrado no jogo)");
                    list.Add(Item($"{L.T("Style de l'overlay", "Overlay style", "Estilo del overlay", "Estilo do overlay")} : {StyleName(config.OverlayStyle)}", () =>
                    {
                        var styles = NativeOverlay.Styles;
                        config.OverlayStyle = styles[(Array.IndexOf(styles, config.OverlayStyle) + 1) % styles.Length];
                        NativeOverlay.SetStyle(config.OverlayStyle);
                    }, L.T("HUD : seuls les chiffres restent, sans fond, comme ceux du jeu (place-le sous la minimap ; un fond apparaît quand il est déverrouillé, pour le déplacer). Classique : la carte sombre des versions précédentes. Entrée pour changer, appliqué tout de suite.",
                        "HUD: only the figures remain, with no background, like the game's (put it under the minimap; a background shows while it is unlocked, to move it). Classic: the dark card of earlier versions. Enter to switch, applied at once.",
                        "HUD: solo quedan las cifras, sin fondo, como las del juego (ponlo bajo el minimapa; aparece un fondo al desbloquearlo, para moverlo). Clásico: la tarjeta oscura de versiones anteriores. Intro para cambiar, se aplica al instante.", "HUD: ficam só os números, sem fundo, como os do jogo (põe-no por baixo do minimapa; aparece um fundo ao desbloqueá-lo, para o mover). Clássico: o cartão escuro das versões anteriores. Cartões: nível e todas as estatísticas. Enter para mudar, aplica-se logo.")));
                    list.Add(Item($"{L.T("Taille de l'overlay", "Overlay size", "Tamaño del overlay", "Tamanho do overlay")} : {Percent(config.OverlaySize)}", () =>
                    {
                        config.OverlaySize = OverlayAppearance.Next(OverlayAppearance.Sizes, config.OverlaySize);
                        ApplyOverlayAppearance();
                    }, L.T("Entrée pour passer à la taille suivante (appliquée tout de suite).", "Enter for the next size (applied at once).", "Intro para pasar al tamaño siguiente (se aplica al instante).", "Enter para passar ao tamanho seguinte (aplica-se logo).")));
                    list.Add(Item($"{L.T("Opacité de l'overlay", "Overlay opacity", "Opacidad del overlay", "Opacidade do overlay")} : {Percent(config.OverlayOpacity)}", () =>
                    {
                        config.OverlayOpacity = OverlayAppearance.Next(OverlayAppearance.Opacities, config.OverlayOpacity);
                        ApplyOverlayAppearance();
                    }, L.T("Entrée pour passer à l'opacité suivante (appliquée tout de suite).", "Enter for the next opacity (applied at once).", "Intro para pasar a la opacidad siguiente (se aplica al instante).", "Enter para passar à opacidade seguinte (aplica-se logo).")));
                }
            }
            if (OperatingSystem.IsWindows())
            {
                list.Add(Item($"{L.T("Réduire dans la zone de notification", "Minimize to the notification area", "Minimizar al área de notificación", "Minimizar para a área de notificação")} : {YesNo(config.MinimizeToTray)}",
                    () => Tray.Enabled = config.MinimizeToTray = !config.MinimizeToTray,
                    L.T("Oui : réduire la fenêtre la range à côté de l'horloge (icône du coffre) au lieu de la barre des tâches. Un clic sur l'icône la rouvre.",
                        "Yes: minimizing the window puts it next to the clock (chest icon) instead of the taskbar. A click on the icon opens it again.",
                        "Sí: minimizar la ventana la coloca junto al reloj (icono del cofre) en lugar de la barra de tareas. Un clic en el icono la vuelve a abrir.", "Sim: minimizar a janela coloca-a junto ao relógio (ícone do baú) em vez da barra de tarefas. Um clique no ícone volta a abri-la.")));
            }
            list.Add(Item($"{L.T("Ouvrir le rapport au démarrage", "Open the report at startup", "Abrir el informe al iniciar", "Abrir o relatório ao iniciar")} : {YesNo(config.OpenReportOnStart)}",
                () => config.OpenReportOnStart = !config.OpenReportOnStart,
                L.T("Ouvre le rapport dans ton navigateur à chaque lancement du compteur.", "Opens the report in your browser each time the counter starts.", "Abre el informe en tu navegador cada vez que se inicia el contador.", "Abre o relatório no teu navegador sempre que o contador arranca.")));
            if (OperatingSystem.IsWindows())
            {
                list.Add(Item($"{L.T("Ouvrir avec Fortnite", "Open with Fortnite", "Abrir con Fortnite", "Abrir com o Fortnite")} : {YesNo(config.StartWithFortnite)}", ToggleStartWithFortnite,
                    L.T("Un petit processus invisible attend Fortnite et ouvre le compteur, qui se ferme après. Il reste actif quand tu quittes le compteur : mets « non » avant de supprimer ou déplacer ChestCounter.exe.",
                        "A tiny invisible process waits for Fortnite and opens the counter, which closes afterwards. It keeps running when you quit the counter: set this to \"no\" before deleting or moving ChestCounter.exe.",
                        "Un pequeño proceso invisible espera a Fortnite y abre el contador, que se cierra después. Sigue activo cuando sales del contador: pon «no» antes de borrar o mover ChestCounter.exe.", "Um pequeno processo invisível espera pelo Fortnite e abre o contador, que se fecha depois. Continua ativo quando sais do contador: põe «não» antes de apagar ou mover o ChestCounter.exe.")));
            }
            var updates = config.UpdateMode switch
            {
                "auto" => L.T("installées automatiquement", "installed automatically", "instaladas automáticamente", "instaladas automaticamente"),
                "off" => L.T("désactivées", "off", "desactivadas", "desativadas"),
                _ => L.T("proposées", "offered", "propuestas", "propostas"),
            };
            list.Add(Item($"{L.T("Mises à jour", "Updates", "Actualizaciones", "Atualizações")} : {updates}",
                () => config.UpdateMode = config.UpdateMode switch { "prompt" => "auto", "auto" => "off", _ => "prompt" },
                L.T("Vérifiées sur GitHub au démarrage (le seul accès réseau de l'outil). Proposées : les nouveautés s'affichent et tu choisis. Automatiques : installées entre deux parties.",
                    "Checked on GitHub at startup (the only network access of the tool). Offered: what's new is shown and you choose. Automatic: installed between two matches.",
                    "Se comprueban en GitHub al iniciar (único acceso a la red). Propuestas: se muestran las novedades y tú eliges. Automáticas: se instalan entre dos partidas.", "São verificadas no GitHub ao iniciar (único acesso à rede). Propostas: as novidades são mostradas e tu escolhes. Automáticas: instalam-se entre duas partidas.")));
            list.Add(Item($"{L.T("Dossier des replays", "Replays folder", "Carpeta de repeticiones", "Pasta das repetições")} : {config.DemosFolder}", ChangeDemosFolder,
                L.T("Là où Fortnite enregistre ses replays. À changer seulement si tu l'as déplacé. Entrée pour le modifier.",
                    "Where Fortnite saves its replays. Only change it if you moved it. Enter to edit.",
                    "Donde Fortnite guarda sus repeticiones. Cámbiala solo si la moviste. Intro para editarla.", "Onde o Fortnite guarda as suas repetições. Só a mudes se a moveste. Enter para a editar.")));
            list.Add(Item(L.T("Recommencer la configuration", "Start the setup again", "Repetir la configuración", "Repetir a configuração"), ResetConfiguration,
                L.T("Efface tes réglages et relance les questions du premier lancement. Ton historique de parties est conservé.",
                    "Clears your settings and asks the first-launch questions again. Your match history is kept.",
                    "Borra tus ajustes y vuelve a hacer las preguntas del primer inicio. Tu historial de partidas se conserva.", "Apaga as tuas definições e volta a fazer as perguntas do primeiro arranque. O teu histórico de partidas é mantido.")));
            break;

        case 2:
            list.Add(Item(L.T("Ouvrir le dossier des données", "Open the data folder", "Abrir la carpeta de datos", "Abrir a pasta dos dados"), () => Open(store.Folder),
                L.T($"Historique, réglages et rapport : {store.Folder}", $"History, settings and report: {store.Folder}", $"Historial, ajustes e informe: {store.Folder}", $"Histórico, definições e relatório: {store.Folder}")));
            if (File.Exists(store.ErrorLogPath))
            {
                list.Add(Item(L.T("Ouvrir le journal d'erreurs", "Open the error log", "Abrir el registro de errores", "Abrir o registo de erros"), () => Open(store.ErrorLogPath),
                    L.T("À joindre si tu signales un problème.", "Attach it when you report a problem.", "Adjúntalo si informas de un problema.", "Anexa-o se reportares um problema.")));
            }
            list.Add(Item(L.T("Signaler un problème (GitHub)", "Report a problem (GitHub)", "Informar de un problema (GitHub)", "Reportar um problema (GitHub)"), () => Open(AppInfo.IssuesUrl), AppInfo.IssuesUrl));
            list.Add(Item(L.T("Page du projet (GitHub)", "Project page (GitHub)", "Página del proyecto (GitHub)", "Página do projeto (GitHub)"), () => Open(AppInfo.RepoUrl), AppInfo.RepoUrl));
            break;
    }
    return list;
}

string[] TabNames() => new[]
{
    L.T("Accueil", "Home", "Inicio", "Início"),
    L.T("Réglages", "Settings", "Ajustes", "Definições"),
    L.T("Aide", "Help", "Ayuda", "Ajuda"),
};

void Draw()
{
    if (!dashboard || !interactive)
    {
        return;
    }
    lastDraw = DateTime.UtcNow;
    var height = ConsoleUi.Height;
    var width = ConsoleUi.Width;
    var f = new Frame();
    f.Blank();
    f.Add(new Seg($"  {appName}", ConsoleColor.Yellow), new Seg($"   v{AppInfo.VersionText}", ConsoleColor.DarkGray));
    f.Add("  " + new string('─', Math.Min(width - 4, 76)), ConsoleColor.DarkGray);

    // Tabs.
    var names = TabNames();
    var tabLine = new List<Seg> { new("  ") };
    for (var i = 0; i < names.Length; i++)
    {
        tabLine.Add(i == tab ? new Seg($" {names[i]} ", ConsoleColor.Black, ConsoleColor.Yellow) : new Seg($" {names[i]} ", ConsoleColor.Gray));
        tabLine.Add(new Seg("  "));
    }
    f.Add(tabLine.ToArray());
    f.Blank();

    var items = Items();
    selection[tab] = Math.Clamp(selection[tab], 0, items.Count - 1);

    // Footer (kept at the bottom of the window).
    var footer = new List<Seg[]>();
    if (items[selection[tab]].About is { Length: > 0 } about)
    {
        footer.Add(Array.Empty<Seg>());
        footer.AddRange(ConsoleUi.Wrap(about, width - 6).Select(l => new[] { new Seg("  " + l, ConsoleColor.Gray) }));
    }
    footer.Add(Array.Empty<Seg>());
    footer.Add(new[] { new Seg("  " + L.T("↑ ↓ choisir   Entrée valider   ← → onglets", "↑ ↓ choose   Enter confirm   ← → tabs", "↑ ↓ elegir   Intro confirmar   ← → pestañas", "↑ ↓ escolher   Enter confirmar   ← → separadores"), ConsoleColor.DarkGray) });
    if (OperatingSystem.IsWindows() && NativeOverlay.IsRunning)
    {
        var o = L.T("Ctrl+Maj+O", "Ctrl+Shift+O", "Ctrl+Mayús+O", "Ctrl+Shift+O") + (NativeOverlay.HotkeyRegistered ? "" : L.T(" (indisponible)", " (unavailable)", " (no disponible)", " (indisponível)"));
        var l = L.T("Ctrl+Maj+L", "Ctrl+Shift+L", "Ctrl+Mayús+L", "Ctrl+Shift+L") + (NativeOverlay.LockHotkeyRegistered ? "" : L.T(" (indisponible)", " (unavailable)", " (no disponible)", " (indisponível)"));
        footer.Add(new[] { new Seg("  " + L.T($"Overlay, même en jeu : {o} afficher / masquer   {l} verrouiller", $"Overlay, even in game: {o} show / hide   {l} lock", $"Overlay, incluso en el juego: {o} mostrar / ocultar   {l} bloquear", $"Overlay, mesmo no jogo: {o} mostrar / esconder   {l} bloquear"), ConsoleColor.DarkGray) });
    }

    // What's new gets the room left once the menu, the footer and a few lines of activity are placed.
    var noteRoom = height - 1 - f.Lines.Count - 7 - items.Count - footer.Count - 4;

    switch (tab)
    {
        case 0: DrawHome(f, noteRoom); break;
        case 2: DrawHelp(f); break;
    }

    // Menu.
    for (var i = 0; i < items.Count; i++)
    {
        f.Add(i == selection[tab]
            ? new Seg($"  › {items[i].Label} ", ConsoleColor.Black, ConsoleColor.Yellow)
            : new Seg($"    {items[i].Label}", ConsoleColor.Gray));
    }

    // Activity (home tab): what the counter did lately, in the space left.
    var room = height - 1 - f.Lines.Count - footer.Count;
    if (tab == 0 && room >= 3)
    {
        f.Blank();
        f.Add("  " + L.T("Activité", "Activity", "Actividad", "Atividade"), ConsoleColor.DarkGray);
        foreach (var line in log.Last(room - 2))
        {
            f.Add(new[] { new Seg("  ") }.Concat(line).ToArray());
        }
    }
    while (f.Lines.Count < height - 1 - footer.Count)
    {
        f.Blank();
    }
    foreach (var line in footer)
    {
        f.Add(line);
    }
    Frames.Draw(f);
}

void DrawHome(Frame f, int noteRoom)
{
    var view = Profiles.View(data, config);
    var matches = view.Matches;
    var profile = Profiles.Current(data, config);
    var profiles = Profiles.List(data);
    var labelWidth = new[] { L.T("Profil", "Profile", "Perfil", "Perfil"), L.T("État", "Status", "Estado", "Estado"), L.T("Dernière partie", "Last match", "Última partida", "Última partida") }.Max(x => x.Length) + 3;
    void Row(string label, params Seg[] value) => f.Add(new[] { new Seg("  " + label.PadRight(labelWidth), ConsoleColor.DarkGray) }.Concat(value).ToArray());

    Row(L.T("Profil", "Profile", "Perfil", "Perfil"), new Seg(profile?.Name ?? L.T("créé à ta première partie", "created with your first match", "se crea con tu primera partida", "cria-se com a tua primeira partida"), ConsoleColor.White),
        new Seg(profiles.Count >= 2 ? L.T($"   ({profiles.Count} comptes sur ce PC)", $"   ({profiles.Count} accounts on this PC)", $"   ({profiles.Count} cuentas en este PC)", $"   ({profiles.Count} contas neste PC)") : "", ConsoleColor.DarkGray));

    // State: what the counter is doing right now.
    if (autoCloseAt is { } closeAt)
    {
        var s = Math.Max(0, (int)Math.Ceiling((closeAt - DateTime.UtcNow).TotalSeconds));
        Row(L.T("État", "Status", "Estado", "Estado"), new Seg(L.T($"Fortnite est fermé : le compteur se ferme dans {s} s. Appuie sur une touche pour le garder ouvert.",
            $"Fortnite is closed: the counter closes in {s} s. Press any key to keep it open.",
            $"Fortnite está cerrado: el contador se cierra en {s} s. Pulsa una tecla para mantenerlo abierto.", $"O Fortnite está fechado: o contador fecha-se dentro de {s} s. Prime uma tecla para o manter aberto."), ConsoleColor.Yellow));
    }
    else if (tracker.IsLive)
    {
        var live = tracker.LiveMatch;
        Row(L.T("État", "Status", "Estado", "Estado"), new Seg("● " + L.T("Partie en cours", "Match in progress", "Partida en curso", "Partida em curso"), ConsoleColor.Red),
            new Seg(live != null
                ? L.T($" : {L.Count(live.ChestsOpened, "coffre", "chest", "cofre", "baú")} pour l'instant", $": {L.Count(live.ChestsOpened, "coffre", "chest", "cofre", "baú")} so far", $": {L.Count(live.ChestsOpened, "coffre", "chest", "cofre", "baú")} por ahora", $": {L.Count(live.ChestsOpened, "coffre", "chest", "cofre", "baú")} por agora")
                : L.T(" : ses coffres seront comptés à la fin", ": its chests will be counted at the end", ": sus cofres se contarán al final", ": os baús serão contados no fim"), ConsoleColor.Gray));
    }
    else if (tracker.IsFinishing || tracker.HasPending)
    {
        Row(L.T("État", "Status", "Estado", "Estado"), new Seg(L.T("Partie terminée : comptage des coffres…", "Match over: counting the chests…", "Partida terminada: contando los cofres…", "Partida terminada: a contar os baús…"), ConsoleColor.Cyan));
    }
    else
    {
        Row(L.T("État", "Status", "Estado", "Estado"), Directory.Exists(config.DemosFolder)
            ? new Seg(L.T("En attente d'une partie", "Waiting for a match", "Esperando una partida", "À espera de uma partida"), ConsoleColor.Green)
            : new Seg(L.T("Dossier des replays introuvable (voir l'aide)", "Replays folder not found (see help)", "Carpeta de repeticiones no encontrada (ver ayuda)", "Pasta das repetições não encontrada (ver ajuda)"), ConsoleColor.DarkYellow));
    }

    var stats = Unified.Compute(view, tracker.LiveMatch);
    var chests = stats.Chests;
    var treasures = matches.Sum(m => m.TreasureFinds.Count);
    var avg = stats.Matches > 0 ? ((double)chests / stats.Matches).ToString("F1", L.Culture) : "0";
    Row(L.T("Total", "Total", "Total", "Total"), new Seg(L.Count(chests, "coffre", "chest", "cofre", "baú"), ConsoleColor.Yellow),
        new Seg($"  ·  {L.Count(stats.Matches, "partie", "parties", "match", "matches", "partida", "partidas", "partida", "partidas")}  ·  {L.T($"{avg} par partie", $"{avg} per match", $"{avg} por partida", $"{avg} por partida")}", ConsoleColor.Gray),
        new Seg(treasures > 0 ? $"  ·  ★ {L.Count(treasures, "trésor", "treasure", "tesoro", "tesouro")}" : "", ConsoleColor.Magenta),
        new Seg(treasures > 0 ? L.T($" (1 tous les {L.N((int)Math.Round((double)chests / treasures))} coffres)", $" (1 every {L.N((int)Math.Round((double)chests / treasures))} chests)", $" (1 cada {L.N((int)Math.Round((double)chests / treasures))} cofres)", $" (1 a cada {L.N((int)Math.Round((double)chests / treasures))} baús)") : "", ConsoleColor.DarkGray));
    Row(L.T("Stats", "Stats", "Stats", "Stats"), new Seg($"{L.T("Victoires", "Wins", "Victorias", "Vitórias")} {stats.Wins}  ·  {L.T("Défaites", "Losses", "Derrotas", "Derrotas")} {stats.Losses}  ·  Kills {L.N(stats.Kills)}  ·  {L.T("Niveau", "Level", "Nivel", "Nível")} {stats.Level}  ·  {Unified.Duration(stats.Seconds)}", ConsoleColor.Gray));
    Row(L.T("Combat", "Combat", "Combate", "Combate"), new Seg($"{L.T("Dégâts", "Damage", "Daño", "Dano")} {L.N(stats.DamageDealt)}  ·  {L.T("Reçus", "Taken", "Recibido", "Recebido")} {L.N(stats.DamageTaken)}  ·  {L.T("Assistances", "Assists", "Asistencias", "Assistências")} {stats.Assists}  ·  {L.T("Précision", "Accuracy", "Precisión", "Precisão")} {stats.AccuracyAvg.ToString("0.0", L.Culture)} %  ·  {L.T("Réanimations", "Revives", "Reanimaciones", "Reanimações")} {stats.Revives}  ·  {L.T("Matériaux", "Materials", "Materiales", "Materiais")} {L.N(stats.MaterialsGathered)}", ConsoleColor.Gray));
    var session = matches.Where(ReportWriter.IsThisSession).ToList();
    Row(L.T("Session", "Session", "Sesión", "Sessão"), new Seg(L.Count(session.Sum(m => m.ChestsOpened), "coffre", "chest", "cofre", "baú"), ConsoleColor.Yellow),
        new Seg($"  ·  {L.Count(session.Count, "partie", "parties", "match", "matches", "partida", "partidas", "partida", "partidas")}", ConsoleColor.Gray));
    if (matches.OrderByDescending(m => m.MatchStartUtc ?? m.AnalyzedAtUtc).FirstOrDefault() is { } last)
    {
        Row(L.T("Dernière partie", "Last match", "Última partida", "Última partida"), new Seg(L.Count(last.ChestsOpened, "coffre", "chest", "cofre", "baú"), ConsoleColor.Yellow),
            new Seg($"  ·  {L.DateTimeShort((last.MatchStartUtc ?? last.AnalyzedAtUtc).ToLocalTime())}  ·  {Pretty.Playlist(last.Playlist)}  ·  {Pretty.Placement(last)}", ConsoleColor.Gray));
    }
    f.Blank();

    // New version: what's new, in the chosen language.
    if (Volatile.Read(ref update) is { } u && !notesHidden)
    {
        NotesBox(f, noteRoom, L.T($"Nouvelle version {u.Version.ToString(3)} disponible", $"New version {u.Version.ToString(3)} available", $"Nueva versión {u.Version.ToString(3)} disponible", $"Nova versão {u.Version.ToString(3)} disponível"),
            L.T($"  (tu as la {AppInfo.VersionText}, rien n'est perdu en l'installant)", $"  (you have {AppInfo.VersionText}, nothing is lost when installing it)", $"  (tienes la {AppInfo.VersionText}, no se pierde nada al instalarla)", $"  (tens a {AppInfo.VersionText}, não se perde nada ao instalá-la)"),
            u.WhatsNew, u.PageUrl);
    }
    else if (showWhatsNew && ReleaseNotes.WhatsNew(ReleaseNotes.Embedded, L.Code) is { Count: > 0 } current)
    {
        // First launch after an update: what this version brings (the notes are built into the exe).
        NotesBox(f, noteRoom, L.T($"Nouveautés de la {AppInfo.VersionText}", $"What's new in {AppInfo.VersionText}", $"Novedades de la {AppInfo.VersionText}", $"Novidades da {AppInfo.VersionText}"),
            L.T("  (mise à jour installée)", "  (update installed)", "  (actualización instalada)", "  (atualização instalada)"), current, null);
    }
}

void NotesBox(Frame f, int noteRoom, string title, string subtitle, IReadOnlyList<string> notes, string? pageUrl)
{
    {
        f.Add(new Seg("  ┌ ", ConsoleColor.Green), new Seg(title, ConsoleColor.Green), new Seg(subtitle, ConsoleColor.DarkGray));
        var shown = 0;
        foreach (var note in notes)
        {
            var wrapped = ConsoleUi.Wrap(note, ConsoleUi.Width - 10).ToList();
            if (shown + wrapped.Count > Math.Clamp(noteRoom - 3, 1, 12))
            {
                f.Add(new Seg("  │ ", ConsoleColor.Green), new Seg(pageUrl != null ? L.T($"… et plus : {pageUrl}", $"… and more: {pageUrl}", $"… y más: {pageUrl}", $"… e mais: {pageUrl}") : "…", ConsoleColor.DarkGray));
                break;
            }
            for (var i = 0; i < wrapped.Count; i++)
            {
                f.Add(new Seg("  │ ", ConsoleColor.Green), new Seg((i == 0 ? "• " : "  ") + wrapped[i], ConsoleColor.Gray));
            }
            shown += wrapped.Count;
        }
        if (notes.Count == 0 && pageUrl != null)
        {
            f.Add(new Seg("  │ ", ConsoleColor.Green), new Seg(L.T($"Nouveautés : {pageUrl}", $"What's new: {pageUrl}", $"Novedades: {pageUrl}", $"Novidades: {pageUrl}"), ConsoleColor.Gray));
        }
        f.Add("  └", ConsoleColor.Green);
        f.Blank();
    }
}

void DrawHelp(Frame f)
{
    void Para(string text, ConsoleColor color = ConsoleColor.Gray)
    {
        foreach (var l in ConsoleUi.Wrap(text, ConsoleUi.Width - 6))
        {
            f.Add("  " + l, color);
        }
    }
    Para(L.T("Le compteur lit les replays que Fortnite enregistre et compte tes coffres quelques secondes après la fin de chaque partie. Laisse cette fenêtre ouverte pendant que tu joues (tu peux la réduire).",
        "The counter reads the replays Fortnite saves and counts your chests a few seconds after each match ends. Keep this window open while you play (you can minimize it).",
        "El contador lee las repeticiones que guarda Fortnite y cuenta tus cofres unos segundos después de cada partida. Deja esta ventana abierta mientras juegas (puedes minimizarla).", "O contador lê as repetições que o Fortnite guarda e conta os teus baús alguns segundos depois de cada partida. Deixa esta janela aberta enquanto jogas (podes minimizá-la)."));
    f.Blank();
    Para(L.T("Rien n'est compté ? Dans Fortnite, active  Paramètres → Jeu → Rediffusions → Enregistrer les rediffusions.",
        "Nothing is counted? In Fortnite, turn on  Settings → Game → Replays → Record Replays.",
        "¿No se cuenta nada? En Fortnite, activa  Ajustes → Juego → Repeticiones → Grabar repeticiones.", "Não se conta nada? No Fortnite, ativa  Definições → Jogo → Repetições → Gravar repetições."), ConsoleColor.Yellow);
    if (config.ShowOverlay && OperatingSystem.IsWindows())
    {
        f.Blank();
        f.Add("  " + L.T("Overlay", "Overlay", "Overlay", "Overlay"), ConsoleColor.White);
        Para(L.T("Ctrl+Maj+O : afficher / masquer.  Ctrl+Maj+L : verrouiller / déverrouiller. Ces raccourcis marchent partout, même en jeu.",
            "Ctrl+Shift+O: show / hide.  Ctrl+Shift+L: lock / unlock. These shortcuts work anywhere, even in game.",
            "Ctrl+Mayús+O: mostrar / ocultar.  Ctrl+Mayús+L: bloquear / desbloquear. Estos atajos funcionan en cualquier sitio, incluso en el juego.", "Ctrl+Shift+O: mostrar / esconder.  Ctrl+Shift+L: bloquear / desbloquear. Estes atalhos funcionam em qualquer lado, mesmo no jogo."));
        Para(L.T("Verrouillé, la souris passe au travers. Déverrouillé, clic gauche maintenu pour le déplacer, clic droit pour le masquer.",
            "Locked, the mouse goes through it. Unlocked, hold the left button to move it, right-click to hide it.",
            "Bloqueado, el ratón lo atraviesa. Desbloqueado, arrástralo con el clic izquierdo; clic derecho para ocultarlo.", "Bloqueado, o rato atravessa-o. Desbloqueado, arrasta-o com o clique esquerdo; clique direito para o esconder."));
        Para(L.T("Pour le voir en jeu, mets Fortnite en « Plein écran fenêtré » (Paramètres → Vidéo → Mode de fenêtre).",
            "To see it in game, set Fortnite to \"Windowed Fullscreen\" (Settings → Video → Window Mode).",
            "Para verlo en el juego: Ajustes → Vídeo → Modo ventana → «Pantalla completa en ventana».", "Para o ver no jogo: Definições → Vídeo → Modo de janela → «Ecrã inteiro em janela»."), ConsoleColor.DarkGray);
    }
    f.Blank();
}

// ---- Main screen: keys and actions ----------------------------------------------------------

void HandleKey(ConsoleKeyInfo key)
{
    var items = Items();
    switch (key.Key)
    {
        case ConsoleKey.LeftArrow:
            tab = (tab + 2) % 3;
            break;
        case ConsoleKey.RightArrow:
        case ConsoleKey.Tab:
            tab = (tab + 1) % 3;
            break;
        case ConsoleKey.Escape:
            tab = 0;
            break;
        case ConsoleKey.UpArrow:
            selection[tab] = (selection[tab] + items.Count - 1) % items.Count;
            break;
        case ConsoleKey.DownArrow:
            selection[tab] = (selection[tab] + 1) % items.Count;
            break;
        case ConsoleKey.Home:
            selection[tab] = 0;
            break;
        case ConsoleKey.End:
            selection[tab] = items.Count - 1;
            break;
        case ConsoleKey.Enter:
            var index = Math.Clamp(selection[tab], 0, items.Count - 1);
            items[index].Run();
            if (tab == 1)
            {
                // A setting changed: save it and show it everywhere at once.
                store.SaveConfig(config);
                ReportWriter.WriteAll(store, config, data);
                UpdateOverlay(store, config, data, tracker.LiveMatch, tracker.IsLive);
            }
            break;
    }
}

void StartOverlay()
{
    if (!OperatingSystem.IsWindows() || NativeOverlay.IsRunning)
    {
        return;
    }
    NativeOverlay.SetAppearance(config.OverlaySize, config.OverlayOpacity);
    if (Array.IndexOf(NativeOverlay.Styles, config.OverlayStyle) < 0)
    {
        config.OverlayStyle = "hud"; // a style tried in a test version and removed since
    }
    NativeOverlay.SetStyle(config.OverlayStyle);
    NativeOverlay.Start(config.OverlayX, config.OverlayY, config.OverlayLocked,
        (x, y) =>
        {
            config.OverlayX = x;
            config.OverlayY = y;
            try { store.SaveConfig(config); } catch { /* ignore */ }
        },
        locked =>
        {
            config.OverlayLocked = locked;
            try { store.SaveConfig(config); } catch { /* ignore */ }
        });
    UpdateOverlay(store, config, data, tracker.LiveMatch, tracker.IsLive);
}

void ToggleOverlay()
{
    config.ShowOverlay = !config.ShowOverlay;
    if (config.ShowOverlay)
    {
        StartOverlay();
    }
    else if (OperatingSystem.IsWindows())
    {
        NativeOverlay.Stop();
    }
}

void CycleLanguage()
{
    config.Language = L.Codes[(Array.IndexOf(L.Codes, L.Code) + 1) % L.Codes.Length]; // fr → en → es → fr
    L.Set(config.Language);
    appName = L.T("Compteur de coffres Fortnite", "Fortnite Chest Counter", "Contador de cofres de Fortnite", "Contador de baús do Fortnite");
    ConsoleUi.SetTitle($"{appName} {AppInfo.VersionText}");
}

void ToggleStartWithFortnite()
{
    var on = !config.StartWithFortnite;
    if (OperatingSystem.IsWindows() && FortniteWatch.Apply(on, dataDir))
    {
        config.StartWithFortnite = on;
        return;
    }
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(L.T("Ouvrir avec Fortnite : impossible ici, lance le compteur depuis ChestCounter.exe.",
        "Open with Fortnite: not possible here, run the counter from ChestCounter.exe.",
        "Abrir con Fortnite: no es posible aquí, abre el contador desde ChestCounter.exe.", "Abrir com o Fortnite: não é possível aqui, abre o contador a partir do ChestCounter.exe."));
    Console.ResetColor();
}

void ChangeDemosFolder()
{
    ConsoleUi.Screen(appName, L.T("dossier des replays", "replays folder", "carpeta de repeticiones", "pasta das repetições"));
    ConsoleUi.Text(L.T("Dossier actuel :", "Current folder:", "Carpeta actual:", "Pasta atual:"));
    ConsoleUi.Hint("  " + config.DemosFolder);
    ConsoleUi.Blank();
    ConsoleUi.Hint(L.T("Tape le nouveau chemin puis Entrée. Échap pour annuler.", "Type the new path, then Enter. Esc to cancel.", "Escribe la nueva ruta y pulsa Intro. Esc para cancelar.", "Escreve o novo caminho e prime Enter. Esc para cancelar."));
    ConsoleUi.Blank();
    var folder = ConsoleUi.Input(L.T("Nouveau dossier : ", "New folder: ", "Nueva carpeta: ", "Nova pasta: "), config.DemosFolder);
    if (folder != config.DemosFolder)
    {
        config.DemosFolder = folder;
        WatchReplays();
        Console.WriteLine(L.T($"Dossier des replays : {folder}", $"Replays folder: {folder}", $"Carpeta de repeticiones: {folder}", $"Pasta das repetições: {folder}"));
        nextScan = DateTime.UtcNow;
    }
}

void ResetConfiguration()
{
    ConsoleUi.Screen(appName, L.T("recommencer la configuration", "start the setup again", "repetir la configuración", "repetir a configuração"));
    ConsoleUi.Text(L.T("Tes réglages vont être effacés et le compteur va redémarrer sur les questions du premier lancement.",
        "Your settings will be cleared and the counter will restart with the first-launch questions.",
        "Tus ajustes se borrarán y el contador se reiniciará con las preguntas del primer inicio.", "As tuas definições serão apagadas e o contador reiniciará com as perguntas do primeiro arranque."));
    ConsoleUi.Hint(L.T("Ton historique de parties et tes statistiques sont conservés.", "Your match history and statistics are kept.", "Tu historial de partidas y tus estadísticas se conservan.", "O teu histórico de partidas e as tuas estatísticas são mantidos."));
    if (!ConsoleUi.Confirm(L.T("Oui, recommencer la configuration", "Yes, start the setup again", "Sí, repetir la configuración", "Sim, repetir a configuração"), L.T("Annuler", "Cancel", "Cancelar", "Cancelar")))
    {
        return;
    }
    if (OperatingSystem.IsWindows())
    {
        if (config.StartWithFortnite)
        {
            FortniteWatch.Apply(false, dataDir);
        }
        NativeOverlay.Stop();
    }
    try { File.Delete(store.ConfigPath); } catch { /* the setup will overwrite it */ }
    restart = true;
    quit = true;
}

// Manual check: asks GitHub now (a version may have come out since the counter was opened).
void CheckForUpdate()
{
    Console.WriteLine(L.T("Recherche d'une nouvelle version sur GitHub…", "Checking GitHub for a new version…", "Buscando una nueva versión en GitHub…", "A procurar uma nova versão no GitHub…"));
    Draw();
    var (found, reached) = Updater.CheckDetailedAsync().GetAwaiter().GetResult();
    if (found == null)
    {
        Console.WriteLine(reached
            ? L.T($"Tu as déjà la dernière version ({AppInfo.VersionText}).", $"You already have the latest version ({AppInfo.VersionText}).", $"Ya tienes la última versión ({AppInfo.VersionText}).", $"Já tens a última versão ({AppInfo.VersionText}).")
            : L.T("GitHub est injoignable pour l'instant (pas de connexion ?) : réessaie plus tard.",
                "GitHub cannot be reached right now (no connection?): try again later.",
                "No se puede acceder a GitHub ahora mismo (¿sin conexión?): inténtalo más tarde.", "Não foi possível aceder ao GitHub neste momento (sem ligação?): tenta mais tarde."));
        return;
    }
    Volatile.Write(ref update, found);
    updateAnnounced = true;
    notesHidden = false;
}

void InstallUpdate(UpdateInfo u)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(L.T($"Téléchargement de la version {u.Version.ToString(3)}…", $"Downloading version {u.Version.ToString(3)}…", $"Descargando la versión {u.Version.ToString(3)}…", $"A descarregar a versão {u.Version.ToString(3)}…"));
    Console.ResetColor();
    Draw();
    var error = Updater.Install(u);
    if (error != null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(L.T($"Mise à jour impossible : {error}", $"Update failed: {error}", $"No se pudo actualizar: {error}", $"Não foi possível atualizar: {error}"));
        Console.ResetColor();
        Console.WriteLine(L.T($"Tu peux la télécharger à la main : {u.PageUrl}", $"You can download it manually: {u.PageUrl}", $"Puedes descargarla a mano: {u.PageUrl}", $"Podes descarregá-la manualmente: {u.PageUrl}"));
        return;
    }
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(L.T("Mise à jour installée : redémarrage du compteur…", "Update installed: restarting the counter…", "Actualización instalada: reiniciando el contador…", "Atualização instalada: a reiniciar o contador…"));
    Console.ResetColor();
    Draw();
    restart = true;
    quit = true;
}

void AnnounceNewProfiles()
{
    if (!tracker.NewProfilesFound)
    {
        return;
    }
    tracker.NewProfilesFound = false;
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine(L.T("Plusieurs comptes Fortnite ont enregistré des replays sur ce PC : chacun a son profil. « Changer de profil » dans l'accueil pour choisir le tien.",
        "Several Fortnite accounts recorded replays on this PC: each one has its own profile. \"Switch profile\" on the home tab to choose yours.",
        "Varias cuentas de Fortnite grabaron repeticiones en este PC: cada una tiene su perfil. «Cambiar de perfil» en el inicio para elegir el tuyo.", "Várias contas do Fortnite gravaram repetições neste PC: cada uma tem o seu perfil. «Mudar de perfil» no início para escolher o teu."));
    Console.ResetColor();
}

// Replays folder: Windows tells us when a file changes, instead of us re-reading the folder all the time.
void WatchReplays()
{
    watcher?.Dispose();
    watcher = null;
    try
    {
        if (Directory.Exists(config.DemosFolder))
        {
            var w = new FileSystemWatcher(config.DemosFolder, "*.replay")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true,
            };
            w.Created += (_, _) => { watcherFired = true; wake.Set(); };
            w.Changed += (_, _) => { watcherFired = true; wake.Set(); };
            w.Renamed += (_, _) => { watcherFired = true; wake.Set(); };
            watcher = w;
        }
    }
    catch
    {
        watcher = null; // fall back to the periodic scan
    }
}

void CheckAutoClose()
{
    var now = DateTime.UtcNow;
    if (FortniteWatch.IsFortniteRunning())
    {
        fortniteClosedSince = null;
        autoCloseAt = null; // Fortnite started again
        return;
    }
    fortniteClosedSince ??= now;
    // Wait for the last match to be counted (at most 5 minutes, in case a replay was left unfinished by a crash).
    var lastMatchDone = !tracker.IsLive && !tracker.HasPending && !tracker.IsFinishing;
    if (!lastMatchDone && now - fortniteClosedSince < TimeSpan.FromMinutes(5))
    {
        return;
    }
    if (autoCloseAt == null)
    {
        autoCloseAt = now.AddSeconds(30);
    }
    else if (now >= autoCloseAt)
    {
        quit = true;
    }
}

// Shows the new size / opacity on the overlay right away (it stays visible above the console).
void ApplyOverlayAppearance()
{
    if (OperatingSystem.IsWindows() && NativeOverlay.IsRunning)
    {
        NativeOverlay.SetAppearance(config.OverlaySize, config.OverlayOpacity);
    }
}

static bool AcquireInstance(Mutex m, TimeSpan wait)
{
    try
    {
        return m.WaitOne(wait);
    }
    catch (AbandonedMutexException)
    {
        return true; // the previous instance was killed: it is ours now
    }
}

// Several accounts recorded replays on this PC: which one is played now (the overlay and the report show it).
static void ChooseProfile(TrackerData d, Config c, Store s)
{
    var list = Profiles.List(d);
    var current = Math.Max(0, list.FindIndex(p => p.Key == Profiles.Current(d, c)?.Key));
    ConsoleUi.Text(L.T("Plusieurs comptes Fortnite ont enregistré des replays sur ce PC.", "Several Fortnite accounts recorded replays on this PC.", "Varias cuentas de Fortnite han grabado repeticiones en este PC.", "Várias contas do Fortnite gravaram repetições neste PC."));
    ConsoleUi.Text(L.T("Sur quel compte joues-tu ?", "Which account are you playing on?", "¿Con qué cuenta juegas?", "Com que conta jogas?"), ConsoleColor.White);
    ConsoleUi.Hint(L.T("Chaque compte garde ses propres statistiques. Tu pourras changer à tout moment depuis l'accueil (« Changer de profil »).",
        "Each account keeps its own statistics. You can switch at any time from the home tab (\"Switch profile\").",
        "Cada cuenta conserva sus propias estadísticas. Puedes cambiar cuando quieras desde el inicio («Cambiar de perfil»).", "Cada conta mantém as suas próprias estatísticas. Podes mudar quando quiseres a partir do início («Mudar de perfil»)."));
    var choice = ConsoleUi.Menu(list.Select(Profiles.Describe).ToList(), current, escapeIndex: current);
    c.Profile = list[choice].Key;
    try { s.SaveConfig(c); } catch { /* ignore */ }
}

static void UpdateOverlay(Store store, Config config, TrackerData d, MatchRecord? live, bool inProgress)
{
    var profileView = Profiles.View(d, config);
    var view = OverlayModel.Build(profileView, live, inProgress);
    ReportWriter.WriteOverlay(store, view);
    Unified.Export(profileView, live);
    if (OperatingSystem.IsWindows() && NativeOverlay.IsRunning)
    {
        NativeOverlay.Update(view);
    }
}

// Below-normal priority + Windows 11 "efficiency mode" (EcoQoS): the game always comes first.
static void LowerPriority()
{
    if (!OperatingSystem.IsWindows())
    {
        return;
    }
    try { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal; } catch { }
    try { EcoQos.Enable(); } catch { }
}

static void Open(string path)
{
    try
    {
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }
    catch
    {
        Console.WriteLine(L.T($"Ouvre ce fichier dans ton navigateur : {path}", $"Open this file in your browser: {path}", $"Abre este archivo en tu navegador: {path}", $"Abre este ficheiro no teu navegador: {path}"));
    }
}
