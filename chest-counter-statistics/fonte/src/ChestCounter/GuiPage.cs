namespace ChestCounter;

/// <summary>
/// The page of the graphical interface: one HTML file (styles and script inside), drawn by the script from the state
/// the counter sends (<see cref="GuiServer"/>). Texts come with the state, in the chosen language.
/// </summary>
public static class GuiPage
{
    /// <summary>Every text of the page, in the chosen language.</summary>
    public static Dictionary<string, string> Texts() => new()
    {
        ["home"] = L.T("Accueil", "Home", "Inicio", "Início"),
        ["report"] = L.T("Rapport", "Report", "Informe", "Relatório"),
        ["overlay"] = L.T("Overlay", "Overlay", "Overlay", "Overlay"),
        ["settings"] = L.T("Réglages", "Settings", "Ajustes", "Definições"),
        ["help"] = L.T("Aide", "Help", "Ayuda", "Ajuda"),
        ["total"] = L.T("Coffres au total", "Chests in total", "Cofres en total", "Baús no total"),
        ["totalSub"] = L.T("ouverts ou détruits par toi", "opened or destroyed by you", "abiertos o destruidos por ti", "abertos ou destruídos por ti"),
        ["byHand"] = L.T("dont {0} ajoutés à la main", "{0} of them added by hand", "de ellos, {0} añadidos a mano", "{0} deles adicionados à mão"),
        ["session"] = L.T("Session", "Session", "Sesión", "Sessão"),
        ["last"] = L.T("Dernière partie", "Last match", "Última partida", "Última partida"),
        ["squad"] = L.T("Total d'équipe", "Squad total", "Total del equipo", "Total da equipa"),
        ["squadSub"] = L.T("toi + {0} par tes coéquipiers", "you + {0} by your teammates", "tú + {0} de tus compañeros", "tu + {0} dos teus colegas"),
        ["inaccurate"] = L.T("imprécis", "inaccurate", "impreciso", "impreciso"),
        ["inaccurateWhy"] = L.T("Compté dans TES replays : un coffre ouvert par un coéquipier à plus de 150 m de toi n'y est pas, et rien n'est enregistré après ton départ de la partie. Le vrai total de l'équipe est plus haut.",
            "Counted in YOUR replays: a chest a teammate opens more than 150 m away from you is not in them, and nothing is recorded once you leave the match. The real squad total is higher.",
            "Contado en TUS repeticiones: un cofre que un compañero abre a más de 150 m de ti no está en ellas, y no se graba nada después de que salgas de la partida. El total real del equipo es mayor.", "Contados nas TUAS repetições: um baú que um colega abre a mais de 150 m de ti não aparece nelas, e nada é gravado depois de saíres da partida. O total real da equipa é maior."),
        ["treasures"] = L.T("Trésors", "Treasures", "Tesoros", "Tesouros"),
        ["matches"] = L.T("parties", "matches", "partidas", "partidas"),
        ["perMatch"] = L.T("{0} par partie", "{0} per match", "{0} por partida", "{0} por partida"),
        ["openReport"] = L.T("Ouvrir le rapport", "Open the report", "Abrir el informe", "Abrir o relatório"),
        ["showOverlay"] = L.T("Afficher l'overlay", "Show the overlay", "Mostrar el overlay", "Mostrar o overlay"),
        ["hideOverlay"] = L.T("Masquer l'overlay", "Hide the overlay", "Ocultar el overlay", "Esconder o overlay"),
        ["lock"] = L.T("Verrouiller l'overlay", "Lock the overlay", "Bloquear el overlay", "Bloquear o overlay"),
        ["unlock"] = L.T("Déverrouiller pour le déplacer", "Unlock to move it", "Desbloquear para moverlo", "Desbloquear para o mover"),
        ["activity"] = L.T("Activité", "Activity", "Actividad", "Atividade"),
        ["noActivity"] = L.T("Rien pour l'instant.", "Nothing yet.", "Nada por ahora.", "Ainda nada."),
        ["analyzing"] = L.T("Analyse de tes replays…", "Analyzing your replays…", "Analizando tus repeticiones…", "A analisar as tuas repetições…"),
        ["whatsNew"] = L.T("Nouveautés de la {0}", "What's new in {0}", "Novedades de la {0}", "Novidades da versão {0}"),
        ["newVersion"] = L.T("Nouvelle version {0} disponible", "New version {0} available", "Nueva versión {0} disponible", "Nova versão {0} disponível"),
        ["install"] = L.T("Installer", "Install", "Instalar", "Instalar"),
        ["checkUpdate"] = L.T("Rechercher une mise à jour", "Check for updates", "Buscar actualizaciones", "Procurar atualizações"),
        ["welcome"] = L.T("Bienvenue !", "Welcome!", "¡Bienvenido!", "Bem-vindo!"),
        ["welcomeText"] = L.T("Active d'abord les rediffusions dans Fortnite : Paramètres → Jeu → Rediffusions → Enregistrer les rediffusions. Ensuite, laisse le compteur ouvert : chaque partie est comptée quelques secondes après sa fin. Des coffres ouverts avant ? Le crayon de « Coffres au total » permet de les ajouter.",
            "First turn on replays in Fortnite: Settings → Game → Replays → Record Replays. Then keep the counter open: each match is counted a few seconds after it ends. Chests opened before? The pencil of \"Chests in total\" adds them.",
            "Primero activa las repeticiones en Fortnite: Ajustes → Juego → Repeticiones → Grabar repeticiones. Luego deja el contador abierto: cada partida se cuenta unos segundos después de terminar. ¿Cofres abiertos antes? El lápiz de «Cofres en total» permite añadirlos.", "Primeiro ativa as repetições no Fortnite: Definições → Jogo → Repetições → Gravar repetições. Depois mantém o contador aberto: cada partida é contada alguns segundos depois de acabar. Abriste baús antes? O lápis de \"Baús no total\" adiciona-os."),
        ["ok"] = L.T("OK", "OK", "OK", "OK"),
        ["moreInfo"] = L.T("En savoir plus", "More info", "Más información", "Mais informação"),
        ["wMin"] = L.T("Réduire", "Minimize", "Minimizar", "Minimizar"),
        ["wMax"] = L.T("Agrandir", "Maximize", "Maximizar", "Maximizar"),
        ["wRestore"] = L.T("Niveau inférieur", "Restore down", "Restaurar", "Restaurar"),
        ["wClose"] = L.T("Fermer", "Close", "Cerrar", "Fechar"),
        ["keepOpen"] = L.T("Garder le compteur ouvert", "Keep the counter open", "Mantener el contador abierto", "Manter o contador aberto"),
        ["autoCloseText"] = L.T("Fortnite est fermé : le compteur, ouvert avec Fortnite, va se fermer tout seul.", "Fortnite is closed: the counter, opened with Fortnite, is about to close by itself.", "Fortnite está cerrado: el contador, abierto con Fortnite, se va a cerrar solo.", "O Fortnite está fechado: o contador, aberto com o Fortnite, vai fechar-se sozinho."),
        ["welcomeQuick"] = L.T("Tes premiers réglages (tu les retrouves à tout moment dans Réglages) :", "Your first settings (you find them at any time in Settings):", "Tus primeros ajustes (los encuentras en cualquier momento en Ajustes):", "As tuas primeiras definições (podes encontrá-las sempre em Definições):"),
        ["welcomeDone"] = L.T("C'est parti !", "Let's go!", "¡Vamos!", "Vamos lá!"),
        ["inGame"] = L.T("Overlay en jeu", "In-game overlay", "Overlay en el juego", "Overlay no jogo"),
        ["inGameHelp"] = L.T("Petite fenêtre toujours au premier plan qui affiche tes coffres en jeu (Fortnite en « Plein écran fenêtré »). Ctrl+Maj+O : afficher / masquer, Ctrl+Maj+L : verrouiller.",
            "Small always-on-top window that shows your chests in game (Fortnite in \"Windowed Fullscreen\"). Ctrl+Shift+O: show / hide, Ctrl+Shift+L: lock.",
            "Pequeña ventana siempre visible que muestra tus cofres en el juego (Fortnite en «Pantalla completa en ventana»). Ctrl+Mayús+O: mostrar / ocultar, Ctrl+Mayús+L: bloquear.", "Pequena janela sempre visível que mostra os teus baús durante o jogo (Fortnite em \"Ecrã inteiro em janela\"). Ctrl+Shift+O: mostrar / esconder, Ctrl+Shift+L: bloquear."),
        ["style"] = L.T("Style", "Style", "Estilo", "Estilo"),
        ["hud"] = L.T("HUD (intégré au jeu)", "HUD (blends into the game)", "HUD (integrado en el juego)", "HUD (integrado no jogo)"),
        ["classic"] = L.T("Classique (carte sombre)", "Classic (dark card)", "Clásico (tarjeta oscura)", "Clássico (cartão escuro)"),
        ["size"] = L.T("Taille", "Size", "Tamaño", "Tamanho"),
        ["opacity"] = L.T("Opacité", "Opacity", "Opacidad", "Opacidade"),
        ["squadToggle"] = L.T("Afficher le total d'équipe", "Show the squad total", "Mostrar el total del equipo", "Mostrar o total da equipa"),
        ["squadHelp"] = L.T("Tes coffres + ceux de tous tes coéquipiers (même aléatoires), sur l'overlay et l'accueil.",
            "Your chests + those of all your teammates (random ones too), on the overlay and the home tab.",
            "Tus cofres + los de todos tus compañeros (también los aleatorios), en el overlay y el inicio.", "Os teus baús + os de todos os teus colegas (os aleatórios também), no overlay e no separador inicial."),
        ["obs"] = L.T("Overlay pour OBS (stream)", "Overlay for OBS (stream)", "Overlay para OBS (directo)", "Overlay para o OBS (stream)"),
        ["obsSteps"] = L.T("Dans OBS : Sources → + → Navigateur, coche « Fichier local », choisis ce fichier. Largeur 360, hauteur 220 (450 × 300 en classique). Pour ne l'avoir que sur le stream, désactive l'overlay en jeu : la page continue d'être mise à jour.",
            "In OBS: Sources → + → Browser, tick \"Local file\", pick this file. Width 360, height 220 (450 × 300 for classic). To have it on the stream only, turn the in-game overlay off: the page keeps being updated.",
            "En OBS: Fuentes → + → Navegador, marca «Archivo local», elige este archivo. Ancho 360, alto 220 (450 × 300 en clásico). Para tenerlo solo en el directo, desactiva el overlay en el juego: la página se sigue actualizando.", "No OBS: Fontes → + → Navegador, marca \"Ficheiro local\" e escolhe este ficheiro. Largura 360, altura 220 (450 × 300 para o clássico). Para o ter só na stream, desliga o overlay do jogo: a página continua a ser atualizada."),
        ["copy"] = L.T("Copier le chemin", "Copy the path", "Copiar la ruta", "Copiar o caminho"),
        ["copied"] = L.T("Copié !", "Copied!", "¡Copiado!", "Copiado!"),
        ["preview"] = L.T("Aperçu", "Preview", "Vista previa", "Pré-visualização"),
        ["language"] = L.T("Langue", "Language", "Idioma", "Idioma"),
        ["tray"] = L.T("Réduire dans la zone de notification", "Minimize to the notification area", "Minimizar al área de notificación", "Minimizar para a área de notificação"),
        ["trayHelp"] = L.T("Oui : réduire la fenêtre la range à côté de l'horloge, en bas à droite de la barre des tâches (icône du coffre, parfois derrière la petite flèche ^). Non : elle reste dans la barre des tâches comme une fenêtre normale. Dans les deux cas, le compteur continue de compter.",
            "Yes: minimizing the window puts it next to the clock, at the bottom right of the taskbar (chest icon, sometimes behind the small ^ arrow). No: it stays in the taskbar like a normal window. Either way, the counter keeps counting.",
            "Sí: minimizar la ventana la coloca junto al reloj, abajo a la derecha de la barra de tareas (icono del cofre, a veces detrás de la pequeña flecha ^). No: se queda en la barra de tareas como una ventana normal. En ambos casos, el contador sigue contando.", "Sim: ao minimizar, a janela fica junto ao relógio, em baixo à direita da barra de tarefas (ícone de baú, às vezes atrás da pequena seta ^). Não: fica na barra de tarefas como uma janela normal. Em ambos os casos o contador continua a contar."),
        ["footerWindow"] = L.T("La croix de la fenêtre ferme le compteur (après confirmation).", "The close button of the window stops the counter (after asking).", "La cruz de la ventana cierra el contador (tras confirmarlo).", "O botão de fechar da janela pára o contador (depois de perguntar)."),
        ["withFortnite"] = L.T("Ouvrir avec Fortnite", "Open with Fortnite", "Abrir con Fortnite", "Abrir com o Fortnite"),
        ["withFortniteHelp"] = L.T("Un petit processus invisible attend Fortnite et ouvre le compteur, qui se ferme après.", "A tiny invisible process waits for Fortnite and opens the counter, which closes afterwards.", "Un pequeño proceso invisible espera a Fortnite y abre el contador, que se cierra después.", "Um pequeno processo invisível espera pelo Fortnite e abre o contador, que se fecha depois."),
        ["updates"] = L.T("Mises à jour", "Updates", "Actualizaciones", "Atualizações"),
        ["upPrompt"] = L.T("Proposées", "Offered", "Propuestas", "Proposto"),
        ["upAuto"] = L.T("Installées automatiquement", "Installed automatically", "Instaladas automáticamente", "Instalado automaticamente"),
        ["upOff"] = L.T("Désactivées", "Off", "Desactivadas", "Desligado"),
        ["profile"] = L.T("Compte (profil)", "Account (profile)", "Cuenta (perfil)", "Conta (perfil)"),
        ["demos"] = L.T("Dossier des replays", "Replays folder", "Carpeta de repeticiones", "Pasta das repetições"),
        ["demosMissing"] = L.T("Introuvable : Fortnite le crée à la première partie enregistrée.", "Not found: Fortnite creates it with the first recorded match.", "No encontrada: Fortnite la crea con la primera partida grabada.", "Não encontrada: o Fortnite cria-a com a primeira partida gravada."),
        ["save"] = L.T("Enregistrer", "Save", "Guardar", "Guardar"),
        ["editChests"] = L.T("Modifier mon nombre de coffres", "Edit my number of chests", "Modificar mi número de cofres", "Modificar o meu número de baús"),
        ["close"] = L.T("Fermer", "Close", "Cerrar", "Fechar"),
        ["uStats"] = L.T("Statistiques", "Statistics", "Estadísticas", "Estatísticas"),
        ["uLevel"] = L.T("Niveau", "Level", "Nivel", "Nível"),
        ["uWinsLosses"] = L.T("Victoires / défaites", "Wins / losses", "Victorias / derrotas", "Vitórias / derrotas"),
        ["uDamage"] = L.T("dégâts", "damage", "daño", "dano"),
        ["uAssists"] = L.T("assistances", "assists", "asistencias", "assistências"),
        ["uAccuracy"] = L.T("précision", "accuracy", "precisión", "precisão"),
        ["uName"] = L.T("Nom (cartes OBS)", "Name (OBS cards)", "Nombre (tarjetas OBS)", "Nome (cartões OBS)"),
        ["uSeason"] = L.T("Saison (cartes OBS)", "Season (OBS cards)", "Temporada (tarjetas OBS)", "Temporada (cartões OBS)"),
        ["uEditTitle"] = L.T("Statistiques à la main", "Statistics by hand", "Estadísticas a mano", "Estatísticas à mão"),
        ["uEditHelp"] = L.T("Le niveau se lit dans les replays (celui du début de la partie) ; ici tu peux corriger tout de suite le niveau ou n'importe quel chiffre. Les prochaines parties s'ajoutent ensuite toutes seules. Nom et saison servent aux cartes OBS.", "The level is read from the replays (the one at the start of the match); here you can fix it, or any other figure, right away. The next matches are then added by themselves. Name and season are for the OBS cards.", "El nivel se lee en las repeticiones (el del inicio de la partida); aquí puedes corregir al momento el nivel o cualquier cifra. Las próximas partidas se suman solas después. Nombre y temporada son para las tarjetas de OBS.", "O nível lê-se nas repetições (o do início da partida); aqui podes corrigir já o nível ou qualquer número. As partidas seguintes somam-se depois sozinhas. Nome e temporada servem para os cartões do OBS."),
        ["uFChests"] = L.T("Coffres au total", "Total chests", "Cofres en total", "Baús no total"),
        ["uFBest"] = L.T("Meilleur score (coffres en une partie)", "Best (chests in one match)", "Mejor marca (cofres en una partida)", "Melhor marca (baús numa partida)"),
        ["uFMatches"] = L.T("Parties", "Matches", "Partidas", "Partidas"),
        ["uFWins"] = L.T("Victoires", "Wins", "Victorias", "Vitórias"),
        ["uFLosses"] = L.T("Défaites", "Losses", "Derrotas", "Derrotas"),
        ["uFKills"] = L.T("Kills", "Kills", "Kills", "Kills"),
        ["uFTime"] = L.T("Temps de jeu (h:mm)", "Play time (h:mm)", "Tiempo de juego (h:mm)", "Tempo de jogo (h:mm)"),
        ["uFDamage"] = L.T("Dégâts", "Damage", "Daño", "Dano"),
        ["uFAssists"] = L.T("Assistances", "Assists", "Asistencias", "Assistências"),
        ["uFAccuracy"] = L.T("Précision (%)", "Accuracy (%)", "Precisión (%)", "Precisão (%)"),
        ["uObsFolder"] = L.T("Ouvrir le dossier des fichiers OBS", "Open the OBS files folder", "Abrir la carpeta de archivos de OBS", "Abrir a pasta dos ficheiros OBS"),
        ["uKeys"] = L.T("Touches Stream Deck (F13 à F24) : F13 coffres, F14 victoires, F15 défaites, F16 kills, F17 parties, F18 niveau (Maj = +10), F19 temps (±10 min), F20 actualiser, F21 ouvrir manual.json, F22 afficher/masquer l'overlay. Ctrl + touche = retirer.", "Stream Deck keys (F13 to F24): F13 chests, F14 wins, F15 losses, F16 kills, F17 matches, F18 level (Shift = +10), F19 time (±10 min), F20 refresh, F21 open manual.json, F22 show/hide the overlay. Ctrl + key = remove.", "Teclas del Stream Deck (F13 a F24): F13 cofres, F14 victorias, F15 derrotas, F16 kills, F17 partidas, F18 nivel (Mayús = +10), F19 tiempo (±10 min), F20 actualizar, F21 abrir manual.json, F22 mostrar/ocultar el overlay. Ctrl + tecla = quitar.", "Teclas do Stream Deck (F13 a F24): F13 baús, F14 vitórias, F15 derrotas, F16 kills, F17 partidas, F18 nível (Shift = +10), F19 tempo (±10 min), F20 atualizar, F21 abrir manual.json, F22 mostrar/esconder o overlay. Ctrl + tecla = tirar."),
        ["cards"] = L.T("Cartes", "Cards", "Tarjetas", "Cartões"),
        ["manual"] = L.T("Coffres ajoutés à la main", "Chests added by hand", "Cofres añadidos a mano", "Baús adicionados à mão"),
        ["manualHelp"] = L.T("Les coffres ouverts avant d'utiliser le compteur. Un nombre négatif en retire, 0 enlève l'ajout.", "The chests opened before using the counter. A negative number removes some, 0 clears it.", "Los cofres abiertos antes de usar el contador. Un número negativo quita cofres, 0 lo borra.", "Os baús abertos antes de usares o contador. Um número negativo tira alguns, 0 limpa tudo."),
        ["estimate"] = L.T("Ou estimer à partir de tes parties", "Or estimate from your matches", "O estimar a partir de tus partidas", "Ou estimar a partir das tuas partidas"),
        ["estimateHelp"] = L.T("Nombre de parties Battle Royale de la saison, dans Fortnite : Carrière → Statistiques.", "Number of Battle Royale matches this season, in Fortnite: Career → Stats.", "Número de partidas de Battle Royale de la temporada, en Fortnite: Carrera → Estadísticas.", "Número de partidas de Battle Royale nesta temporada, no Fortnite: Carreira → Estatísticas."),
        ["career"] = L.T("Parties jouées", "Matches played", "Partidas jugadas", "Partidas jogadas"),
        ["compute"] = L.T("Calculer", "Compute", "Calcular", "Calcular"),
        ["add"] = L.T("Ajouter {0} coffres", "Add {0} chests", "Añadir {0} cofres", "Adicionar {0} baús"),
        ["estimateTooFew"] = L.T("Il faut au moins {0} parties comptées pour une moyenne fiable.", "At least {0} counted matches are needed for a reliable average.", "Hacen falta al menos {0} partidas contadas para una media fiable.", "São precisas pelo menos {0} partidas contadas para uma média fiável."),
        ["estimateNothing"] = L.T("Rien à ajouter : tes parties comptées couvrent déjà ce nombre.", "Nothing to add: your counted matches already cover that number.", "Nada que añadir: tus partidas contadas ya cubren ese número.", "Nada a adicionar: as tuas partidas contadas já cobrem esse número."),
        ["dataFolder"] = L.T("Ouvrir le dossier des données", "Open the data folder", "Abrir la carpeta de datos", "Abrir a pasta dos dados"),
        ["errorLog"] = L.T("Ouvrir le journal d'erreurs", "Open the error log", "Abrir el registro de errores", "Abrir o registo de erros"),
        ["issue"] = L.T("Signaler un problème (GitHub)", "Report a problem (GitHub)", "Informar de un problema (GitHub)", "Reportar um problema (GitHub)"),
        ["project"] = L.T("Page du projet (GitHub)", "Project page (GitHub)", "Página del proyecto (GitHub)", "Página do projeto (GitHub)"),
        ["helpText"] = L.T("Le compteur lit les replays que Fortnite enregistre et compte tes coffres quelques secondes après chaque partie (Battle Royale de la saison seulement). Laisse-le ouvert pendant que tu joues : tu peux réduire la fenêtre, il continue de compter.",
            "The counter reads the replays Fortnite saves and counts your chests a few seconds after each match (the season's Battle Royale only). Keep it open while you play: you can minimize the window, it keeps counting.",
            "El contador lee las repeticiones que guarda Fortnite y cuenta tus cofres unos segundos después de cada partida (solo el Battle Royale de la temporada). Déjalo abierto mientras juegas: puedes minimizar la ventana, sigue contando.", "O contador lê as repetições que o Fortnite guarda e conta os teus baús alguns segundos depois de cada partida (só o Battle Royale da temporada). Mantém-no aberto enquanto jogas: podes minimizar a janela, ele continua a contar."),
        ["uninstall"] = L.T("Désinstaller ChestCounter", "Uninstall ChestCounter", "Desinstalar ChestCounter", "Desinstalar o ChestCounter"),
        ["keepData"] = L.T("Garder mon historique et mes réglages", "Keep my history and settings", "Conservar mi historial y mis ajustes", "Manter o meu histórico e definições"),
        ["deleteData"] = L.T("Tout supprimer", "Delete everything", "Borrarlo todo", "Apagar tudo"),
        ["uninstallConfirm"] = L.T("Désinstaller maintenant ? Le compteur se ferme et ChestCounter.exe est supprimé.", "Uninstall now? The counter closes and ChestCounter.exe is deleted.", "¿Desinstalar ahora? El contador se cierra y se borra ChestCounter.exe.", "Desinstalar agora? O contador fecha-se e o ChestCounter.exe é apagado."),
        ["quit"] = L.T("Quitter le compteur", "Quit the counter", "Salir del contador", "Sair do contador"),
        ["quitConfirm"] = L.T("Arrêter le compteur ? Les parties jouées ensuite ne seront comptées qu'à sa prochaine ouverture.", "Stop the counter? Matches played afterwards are only counted when it is opened again.", "¿Detener el contador? Las partidas jugadas después solo se cuentan cuando se vuelva a abrir.", "Parar o contador? As partidas jogadas depois só são contadas quando for aberto outra vez."),
        ["closed"] = L.T("Le compteur est arrêté. Tu peux fermer cette fenêtre.", "The counter is stopped. You can close this window.", "El contador está detenido. Puedes cerrar esta ventana.", "O contador está parado. Podes fechar esta janela."),
        ["offline"] = L.T("Le compteur ne répond plus (fermé ?). Relance ChestCounter.exe.", "The counter does not answer (closed?). Start ChestCounter.exe again.", "El contador no responde (¿cerrado?). Vuelve a abrir ChestCounter.exe.", "O contador não responde (fechado?). Volta a abrir o ChestCounter.exe."),
        ["footer"] = L.T("Fermer cette fenêtre ne coupe pas le compteur.", "Closing this window does not stop the counter.", "Cerrar esta ventana no detiene el contador.", "Fechar esta janela não pára o contador."),
    };

    public static string Html(string token) => Template.Replace("__TOKEN__", token).Replace("__FONT__", ReportWriter.FontFaceCss).Replace("__PENCIL__", ReportWriter.PencilIcon);

    private const string Template = """
        <!doctype html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
        <title>Chest Counter</title>
        <style>__FONT__
        :root{--bg:#0f1b24;--ink:#efe2c4;--muted:#b8b0a0;--title:#f0c057;--card:#f2e6c9;--cink:#2a1c0f;--cmuted:#6b5a44;--line:#d8c59c;--track:#e4d3ad;--gold:#c9a24f;--team:#1f7f7a;--warn:#a3361f}
        @media (prefers-color-scheme:light){:root{--bg:#efe4c9;--ink:#2a1c0f;--muted:#6b5a44;--title:#7a4a1e;--card:#fffaf0;--line:#e2d2ae;--track:#efe2c4}}
        *{box-sizing:border-box}
        body{margin:0;background:var(--bg);color:var(--ink);font:15px/1.5 "Segoe UI",system-ui,sans-serif}
        main{max-width:1080px;margin:0 auto;padding:24px 16px 40px;display:flex;flex-direction:column;gap:20px}
        .pirate{font-family:"Pirata One","Segoe UI",serif;font-weight:400}
        .num{font-family:Bahnschrift,"Segoe UI",sans-serif;font-stretch:75%;font-variant-numeric:tabular-nums}
        #app{display:flex;flex-direction:column;gap:20px}
        #hdr{display:flex;flex-direction:column;gap:16px}
        #view{display:flex;flex-direction:column;gap:20px}
        #view:empty{display:none}
        #rep{width:100%;min-height:400px;border:0;display:block;background:transparent;overflow:hidden}
        .who{display:flex;align-items:center;gap:10px;flex-wrap:wrap}
        .who select{min-height:32px;padding:2px 8px;font-size:14px;border-color:var(--gold);background:transparent;color:var(--ink)}
        .who select option{color:#2a1c0f;background:#fffaf0}
        header{display:flex;flex-wrap:wrap;align-items:center;justify-content:space-between;gap:12px;border-bottom:2px solid var(--gold);padding-bottom:14px}
        .brand{display:flex;align-items:center;gap:12px}
        h1{margin:0;font-size:38px;line-height:1;color:var(--title)}
        .who{color:var(--muted);font-size:14px}
        .pill{display:inline-flex;align-items:center;gap:8px;padding:6px 12px;border-radius:999px;background:var(--card);color:var(--cink);font-weight:600;font-size:14px}
        .pill i{width:10px;height:10px;border-radius:50%;background:#3a9a5b}
        .pill.live i{background:#ff3b30}.pill.busy i{background:#e0a526}.pill.warn i{background:#c8412f}
        nav{display:flex;gap:6px;flex-wrap:wrap}
        nav button{min-height:44px;padding:8px 18px;border-radius:8px;border:1px solid var(--gold);background:transparent;color:var(--ink);font:600 15px "Segoe UI",system-ui,sans-serif;cursor:pointer}
        nav button.on{background:var(--gold);color:var(--cink)}
        h2{margin:0 0 10px;font-size:24px;color:var(--title)}
        .tiles{display:grid;grid-template-columns:repeat(auto-fit,minmax(170px,1fr));gap:12px}
        .tile{background:var(--card);color:var(--cink);border:1px solid var(--line);border-radius:8px;padding:14px 16px}
        .tile .v{font-size:36px;font-weight:700;line-height:1.1}
        .tile .l{font-size:13px;color:var(--cmuted)}
        .tile.hero{position:relative;background:#7a4a1e;color:#fbecc4;border:2px solid #e7b440}
        .editbtn{position:absolute;top:10px;right:10px;width:36px;height:36px;padding:0;border-radius:8px;border:1px solid #e7b440;background:transparent;color:#ffd36a;cursor:pointer;display:grid;place-items:center}
        .editbtn:hover{background:rgba(231,180,64,.18)}.editbtn:focus-visible{outline:3px solid #ffd36a;outline-offset:2px}
        dialog{width:min(560px,calc(100% - 32px));padding:20px 22px;border:2px solid var(--gold);border-radius:10px;background:var(--card);color:var(--cink)}
        dialog::backdrop{background:rgba(5,10,14,.6)}
        .dhead{display:flex;align-items:center;justify-content:space-between;gap:12px;margin-bottom:6px}
        .dhead h2{margin:0;color:#7a4a1e}
        .dclose{width:40px;height:40px;border-radius:8px;border:1px solid var(--line);background:transparent;color:var(--cink);font-size:22px;line-height:1;cursor:pointer}
        .tile.hero .v{font-size:54px;color:#ffd36a}.tile.hero .l{color:#fbecc4}
        .card{background:var(--card);color:var(--cink);border:1px solid var(--line);border-radius:8px;padding:16px}
        .row{display:flex;align-items:center;justify-content:space-between;gap:16px;padding:12px 0;border-bottom:1px solid var(--line)}
        .row:last-child{border-bottom:none}
        .row .t{font-weight:600}.row .h{font-size:13px;color:var(--cmuted);margin-top:2px}
        .actions{display:flex;flex-wrap:wrap;gap:8px}
        button.b{min-height:40px;padding:8px 16px;border-radius:8px;border:1px solid var(--gold);background:var(--card);color:var(--cink);font:600 14px "Segoe UI",system-ui,sans-serif;cursor:pointer}
        button.b:hover{background:var(--track)}
        button.b.main{background:#7a4a1e;color:#fbecc4;border-color:#e7b440}
        button.b.danger{border-color:var(--warn);color:var(--warn)}
        select,input[type=text],input[type=number]{min-height:40px;padding:6px 10px;border-radius:6px;border:1px solid var(--line);background:#fffaf0;color:var(--cink);font:15px "Segoe UI",system-ui,sans-serif}
        input[type=text]{width:min(520px,100%)}
        .switch{position:relative;display:inline-block;width:52px;height:30px;flex:none;vertical-align:middle}
        .switch input{opacity:0;width:100%;height:100%;margin:0;cursor:pointer;position:absolute;z-index:1}
        .switch span{position:absolute;inset:0;border-radius:15px;background:#c9bc9c;transition:background .15s}
        .switch span:after{content:"";position:absolute;top:3px;left:3px;width:24px;height:24px;border-radius:50%;background:#fff;transition:transform .15s}
        .switch input:checked+span{background:var(--team)}.switch input:checked+span:after{transform:translateX(22px)}
        .switch input:focus-visible+span{outline:3px solid var(--gold)}
        .badge{display:inline-block;margin-left:6px;padding:1px 8px;border-radius:999px;background:#f6d8c9;color:var(--warn);font-size:12px;font-weight:700;vertical-align:1px;cursor:help}
        .log{font:13px/1.55 Consolas,"Cascadia Mono",monospace;max-height:260px;overflow:auto;white-space:pre-wrap}
        .log .err{color:#b3261e}.log .warn{color:#9a5b00}.log .ok{color:#1d7a3c}.log .info{color:#1f6f8b}.log .tre{color:#6c3fb5}.log .mut{color:var(--cmuted)}
        .bar{height:8px;background:var(--line);border-radius:4px;overflow:hidden;margin-top:8px}.bar i{display:block;height:100%;background:#b9821c}
        .path{font:13px Consolas,monospace;word-break:break-all;background:var(--track);padding:6px 8px;border-radius:6px;display:block;margin:6px 0}
        .note{font-size:13px;color:var(--muted)}
        .hidden{display:none!important}
        footer{display:flex;justify-content:space-between;align-items:center;gap:12px;flex-wrap:wrap;border-top:1px dashed var(--gold);padding-top:12px;color:var(--muted);font-size:13px}
        .estimate{margin-top:8px;font-weight:600}
        .notice{position:relative;display:flex;gap:12px;align-items:flex-start;border-left:6px solid var(--gold)}
        .notice.warning{border-left-color:#c8412f;background:#fbe9d0}
        .notice .ni{flex:none;width:24px;height:24px;border-radius:50%;display:grid;place-items:center;font:700 14px "Segoe UI",system-ui,sans-serif;background:var(--gold);color:#fff;margin-top:1px}
        .notice.warning .ni{background:#c8412f}
        .notice .nb{flex:1;min-width:0}.notice .nt{font-weight:700;margin-bottom:2px}.notice .nx{white-space:pre-line}
        .notice .actions{margin-top:10px}
        .notice .nclose{flex:none;width:32px;height:32px;margin:-6px -8px 0 0;border:0;border-radius:6px;background:transparent;color:var(--cmuted);font-size:20px;line-height:1;cursor:pointer}
        .notice .nclose:hover{background:rgba(0,0,0,.06);color:var(--cink)}
        @media (max-width:640px){.row{flex-direction:column;align-items:flex-start}}
        /* The counter's own window: its title bar is drawn here (drag to move, double click to maximize). */
        .tbar{display:none}
        body.windowed{height:100vh;overflow:hidden}
        body.windowed .tbar{display:flex;justify-content:flex-end;position:fixed;top:0;left:0;right:0;height:34px;background:var(--bg);z-index:20;app-region:drag;user-select:none}
        body.windowed #scroll{position:absolute;top:34px;left:0;right:0;bottom:0;overflow:auto}
        body.windowed header{app-region:drag}
        body.windowed header select,body.windowed header button{app-region:no-drag}
        .wctl{display:flex;app-region:no-drag}
        .wctl button{width:46px;height:34px;padding:0;border:0;border-radius:0;background:transparent;color:var(--muted);display:grid;place-items:center;cursor:pointer;transition:background .12s,color .12s}
        .wctl button:hover{background:rgba(201,162,79,.18);color:var(--title)}
        .wctl button.x:hover{background:#b3261e;color:#fff}
        .wctl button:focus-visible{outline:2px solid var(--gold);outline-offset:-2px}
        .wctl svg{display:block}
        /* Scrollbars in the colors of the tool (no white bar on the side). */
        #scroll::-webkit-scrollbar,html::-webkit-scrollbar{width:12px}
        #scroll::-webkit-scrollbar-track,html::-webkit-scrollbar-track{background:var(--bg)}
        #scroll::-webkit-scrollbar-thumb,html::-webkit-scrollbar-thumb{background:rgba(201,162,79,.32);border-radius:6px;border:3px solid var(--bg)}
        #scroll::-webkit-scrollbar-thumb:hover,html::-webkit-scrollbar-thumb:hover{background:rgba(201,162,79,.6)}
        .log::-webkit-scrollbar{width:10px}
        .log::-webkit-scrollbar-track{background:transparent}
        .log::-webkit-scrollbar-thumb{background:var(--line);border-radius:5px;border:2px solid var(--card)}
        </style></head><body><div class="tbar"><div class="wctl">
          <button type="button" id="wmin"><svg width="10" height="10" viewBox="0 0 10 10" aria-hidden="true"><path d="M0 5.5h10" stroke="currentColor" stroke-width="1.1"/></svg></button>
          <button type="button" id="wmax"><svg width="10" height="10" viewBox="0 0 10 10" aria-hidden="true"><rect id="wmaxicon" x=".55" y=".55" width="8.9" height="8.9" fill="none" stroke="currentColor" stroke-width="1.1"/><path id="wresicon" d="M2.5 2.5V.55h6.95v6.95H7.5M.55 2.5h6.95v6.95H.55z" fill="none" stroke="currentColor" stroke-width="1.1" style="display:none"/></svg></button>
          <button type="button" id="wclose" class="x"><svg width="10" height="10" viewBox="0 0 10 10" aria-hidden="true"><path d="M.5.5l9 9M9.5.5l-9 9" stroke="currentColor" stroke-width="1.2"/></svg></button>
        </div></div><div id="scroll"><main><div id="app"><p class="note">…</p></div><section id="reportPane" class="hidden"><iframe id="rep" title="Report" scrolling="no"></iframe></section><div id="foot"></div></main></div>
        <dialog id="dlg" aria-labelledby="dlgT"><div class="dhead"><h2 class="pirate" id="dlgT"></h2><button type="button" class="dclose" id="dlgX">×</button></div><div id="dlgB"></div></dialog>
        <script>
        const TOKEN='__TOKEN__';let S=null,tab=(()=>{const h=location.hash.slice(1);if(['home','report','overlay','settings','help'].includes(h))return h;try{return localStorage.getItem('cc-gui-tab')||'home'}catch(e){return 'home'}})(),estimate=null,stopped=false,copied=false;
        const $=s=>document.querySelector(s);
        // In the counter's own window (?w=1): the title bar of the page, its buttons sent to the window.
        const host=window.chrome&&window.chrome.webview;let maximized=false;
        if(host&&new URLSearchParams(location.search).get('w')==='1')document.body.classList.add('windowed');
        const scroller=()=>document.body.classList.contains('windowed')?document.getElementById('scroll'):document.scrollingElement;
        function winBtn(a){if(host)host.postMessage({win:a})}
        function winTitles(){if(!S)return;const T=S.txt;$('#wmin').title=T.wMin;$('#wmin').setAttribute('aria-label',T.wMin);const m=maximized?T.wRestore:T.wMax;$('#wmax').title=m;$('#wmax').setAttribute('aria-label',m);$('#wclose').title=T.wClose;$('#wclose').setAttribute('aria-label',T.wClose)}
        if(host)host.addEventListener('message',e=>{const d=e.data;if(d&&typeof d.maximized==='boolean'){maximized=d.maximized;$('#wmaxicon').style.display=maximized?'none':'';$('#wresicon').style.display=maximized?'':'none';winTitles()}});
        const esc=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
        const fmt=(s,...a)=>a.reduce((r,v,i)=>r.replace('{'+i+'}',v),s);
        async function api(action,value){const r=await fetch('/api/action',{method:'POST',headers:{'X-Token':TOKEN,'Content-Type':'application/json'},body:JSON.stringify({action,value})});const j=await r.json();await refresh(true);return j.result}
        async function refresh(force){if(stopped)return;try{const r=await fetch('/api/state',{headers:{'X-Token':TOKEN}});S=await r.json();render(force)}catch(e){if(S){$('#app').innerHTML='<div class="card">'+esc(S.txt.offline)+'</div>'}}}
        function busy(){const a=document.activeElement;return a&&a.closest&&a.closest('#view')&&/INPUT|SELECT|TEXTAREA/.test(a.tagName)}
        function busyHeader(){const a=document.activeElement;return a&&a.closest&&a.closest('#hdr')&&a.tagName==='SELECT'}
        function sw(id,on,act){return `<label class="switch"><input type="checkbox" id="${id}" ${on?'checked':''} onchange="api('${act}',this.checked)"><span></span></label>`}
        function row(title,help,control){return `<div class="row"><div><div class="t">${title}</div>${help?`<div class="h">${help}</div>`:''}</div><div>${control}</div></div>`}
        function sel(act,value,opts){return `<select onchange="api('${act}',this.value)">${opts.map(o=>`<option value="${esc(o[0])}" ${String(o[0])===String(value)?'selected':''}>${esc(o[1])}</option>`).join('')}</select>`}
        function setTab(t){tab=t;try{localStorage.setItem('cc-gui-tab',t)}catch(e){}render(true)}
        function render(force){
          const T=S.txt;document.documentElement.lang=S.lang;
          const head=`<header><div class="brand"><svg width="44" height="44" viewBox="0 0 56 56" aria-hidden="true"><rect x="6" y="20" width="44" height="28" rx="4" fill="#7a4a1e" stroke="#e7b440" stroke-width="3"/><path d="M6 26 Q28 4 50 26" fill="#965d27" stroke="#e7b440" stroke-width="3"/><rect x="24" y="24" width="8" height="10" rx="2" fill="#e7b440"/></svg><div><h1 class="pirate">${esc(S.appName)}</h1><div class="who">${S.profiles.length>1?`<select aria-label="${esc(T.profile)}" title="${esc(T.profile)}" onchange="api('profile',this.value)">${S.profiles.map(p=>`<option value="${esc(p.key)}" ${p.key===S.profileKey?'selected':''}>${esc(p.name)} (${esc(p.matches)})</option>`).join('')}</select>`:`<span>${esc(S.player||'')}</span>`}<span>v${esc(S.version)}</span></div></div></div><span class="pill ${S.status.kind}"><i></i>${esc(S.status.text)}</span></header>
          <nav>${['home','report','overlay','settings','help'].map(t=>`<button class="${t===tab?'on':''}" onclick="setTab('${t}')">${esc(T[t])}</button>`).join('')}</nav>`;
          if(!force&&busyHeader())return;
          if(!force&&busy()){$('#hdr').innerHTML=head;return}
          let v='';
          if(tab==='home')v=home(T);else if(tab==='overlay')v=overlay(T);else if(tab==='settings')v=settings(T);else if(tab==='help')v=help(T);
          $('#app').innerHTML=`<div id="hdr">${head}</div>${notices(T)}<div id="view">${v}</div>`;
          // In the counter's own window the close button asks and stops it: no "quit" button needed.
          $('#foot').innerHTML=S.settings.windowed?`<footer><span>${esc(T.footerWindow)}</span></footer>`:`<footer><span>${esc(T.footer)}</span><button class="b danger" onclick="quit()">${esc(T.quit)}</button></footer>`;
          showReport();winTitles();
        }
        // The report, in the window: loaded once, then again only when the counter rewrote it (new match, setting...).
        let repStamp=null,repObs=null;
        // Height of the report's content (not of its frame: it shrinks again when a list is folded).
        function saveStats(){const o={};document.querySelectorAll('#stform input,#stform2 input').forEach(i=>{if(i.value!==i.dataset.o)o[i.dataset.f]=i.value});if(Object.keys(o).length)api('manualStats',o)}
        function fitReport(){const f=$('#rep');try{const m=f.contentDocument.querySelector('main');f.style.height=Math.ceil(m.getBoundingClientRect().height)+'px'}catch(e){}}
        function showReport(){
          const pane=$('#reportPane'),f=$('#rep');pane.classList.toggle('hidden',tab!=='report');
          if(tab!=='report'||S.reportStamp===repStamp)return;
          const y=scroller().scrollTop;repStamp=S.reportStamp;
          f.onload=()=>{fitReport();scroller().scrollTop=y;try{if(repObs)repObs.disconnect();repObs=new ResizeObserver(fitReport);repObs.observe(f.contentDocument.querySelector('main'))}catch(e){}};
          f.src='/report?t='+TOKEN+'&v='+encodeURIComponent(repStamp);
        }
        // Messages of content.json (a known problem, a fix to install...): above every tab.
        function notices(T){return (S.notices||[]).map(n=>`<div class="card notice ${n.level==='warning'?'warning':''}" role="${n.level==='warning'?'alert':'status'}"><span class="ni" aria-hidden="true">${n.level==='warning'?'!':'i'}</span><div class="nb">${n.title?`<div class="nt">${esc(n.title)}</div>`:''}<div class="nx">${esc(n.text)}</div>${n.link?`<div class="actions"><button class="b ${n.level==='warning'?'main':''}" onclick="window.open('${esc(n.link)}','_blank')">${esc(T.moreInfo)}</button></div>`:''}</div>${n.level==='info'?`<button class="nclose" title="${esc(T.close)}" aria-label="${esc(T.close)}" onclick="api('dismissNotice','${esc(n.id)}')">×</button>`:''}</div>`).join('')}
        function home(T){
          let h='';
          if(S.autoClose)h+=`<div class="card notice warning"><b>${esc(S.status.text)}</b><br>${esc(T.autoCloseText)}<div class="actions" style="margin-top:10px"><button class="b main" onclick="api('keepOpen')">${esc(T.keepOpen)}</button></div></div>`;
          if(S.firstRun){const s=S.settings;h+=`<div class="card welcome"><h2 class="pirate" style="color:#7a4a1e">${esc(T.welcome)}</h2><p style="margin-top:0">${esc(T.welcomeText)}</p>
            <div class="t" style="font-weight:600;margin-top:14px">${esc(T.welcomeQuick)}</div>
            ${row(esc(T.language),'',sel('language',s.language,s.languages))}
            ${row(esc(T.inGame),esc(T.inGameHelp),sw('wov',S.overlay.show,'showOverlay'))}
            ${s.windowed?row(esc(T.tray),esc(T.trayHelp),sw('wtr',s.minimizeToTray,'minimizeToTray')):''}
            <div class="actions" style="margin-top:12px"><button class="b main" onclick="api('dismissWelcome')">${esc(T.welcomeDone)}</button></div></div>`}
          if(S.progress)h+=`<div class="card"><b>${esc(T.analyzing)} ${S.progress.done} / ${S.progress.total}</b><div class="bar"><i style="width:${Math.round(100*S.progress.done/Math.max(1,S.progress.total))}%"></i></div></div>`;
          if(S.whatsNew&&S.whatsNew.length)h+=`<div class="card"><h2 class="pirate" style="color:#7a4a1e">${esc(fmt(T.whatsNew,S.version))}</h2><ul>${S.whatsNew.map(n=>`<li>${esc(n)}</li>`).join('')}</ul><button class="b main" onclick="api('dismissWhatsNew')">${esc(T.ok)}</button></div>`;
          if(S.update)h+=`<div class="card"><h2 class="pirate">${esc(fmt(T.newVersion,S.update.version))}</h2><ul>${S.update.notes.map(n=>`<li>${esc(n)}</li>`).join('')}</ul><button class="b main" onclick="api('installUpdate')">${esc(T.install)}</button></div>`;
          h+=`<section class="tiles"><div class="tile hero"><button type="button" class="editbtn" onclick="openManual()" title="${esc(T.editChests)}" aria-label="${esc(T.editChests)}">__PENCIL__</button><div class="l">${esc(T.total)}</div><div class="v num">${esc(S.total)}</div><div class="l">${esc(T.totalSub)}${S.manual?'<br>'+esc(fmt(T.byHand,S.manual)):''}</div></div>
            <div class="tile"><div class="v num">${esc(S.session)}</div><div class="l">${esc(T.session)} · ${esc(S.sessionMatches)} ${esc(T.matches)}</div></div>
            <div class="tile"><div class="v num">${esc(S.last?S.last.chests:'—')}</div><div class="l">${esc(T.last)}${S.last?'<br>'+esc(S.last.when)+' · '+esc(S.last.mode):''}</div></div>
            ${S.squad?`<div class="tile"><div class="v num">${esc(S.squad.total)}</div><div class="l">${esc(T.squad)}<span class="badge" title="${esc(T.inaccurateWhy)}">${esc(T.inaccurate)}</span><br>${esc(fmt(T.squadSub,S.squad.team))}</div></div>`:''}
            <div class="tile"><div class="v num">★ ${esc(S.treasures)}</div><div class="l">${esc(T.treasures)} · ${esc(S.matches)} ${esc(T.matches)} · ${esc(fmt(T.perMatch,S.average))}</div></div></section>`;
          {const st=S.stats;h+=`<section class="tiles"><div class="tile"><div class="v num">${esc(st.level)}</div><div class="l">${esc(T.uLevel)}</div></div>
            <div class="tile"><div class="v num">${esc(st.wins)} / ${esc(st.losses)}</div><div class="l">${esc(T.uWinsLosses)}</div></div>
            <div class="tile"><div class="v num">${esc(st.kills)}</div><div class="l">Kills · ${esc(st.matches)} ${esc(T.matches)} · ${esc(st.playTime)}</div></div>
            <div class="tile"><div class="v num">${esc(st.damage)}</div><div class="l">${esc(T.uDamage)} · ${esc(st.assists)} ${esc(T.uAssists)} · ${esc(st.accuracy)} % ${esc(T.uAccuracy)}</div></div></section>`}
          h+=`<div class="actions"><button class="b main" onclick="setTab('report')">${esc(T.openReport)}</button>`;
          if(S.overlay.running)h+=`<button class="b" onclick="api('toggleOverlayVisible')">${esc(S.overlay.visible?T.hideOverlay:T.showOverlay)}</button><button class="b" onclick="api('toggleOverlayLock')">${esc(S.overlay.locked?T.unlock:T.lock)}</button>`;
          if(!S.update&&S.updatesOn)h+=`<button class="b" onclick="api('checkUpdate')">${esc(T.checkUpdate)}</button>`;
          h+=`</div><section><h2 class="pirate">${esc(T.activity)}</h2><div class="card log">${S.activity.length?S.activity.map(l=>l.map(s=>`<span class="${s.c}">${esc(s.t)}</span>`).join('')).join('\n'):esc(T.noActivity)}</div></section>`;
          return h;
        }
        function overlay(T){
          const o=S.overlay;
          let h=`<div class="card">${row(esc(T.inGame),esc(T.inGameHelp),sw('ov',o.show,'showOverlay'))}
            ${row(esc(T.style),'',sel('overlayStyle',o.style,[['hud',T.hud],['classic',T.classic],['cards',T.cards]]))}
            ${o.show?row(esc(T.size),'',sel('overlaySize',o.size,o.sizes.map(x=>[x,x+' %'])))+row(esc(T.opacity),'',sel('overlayOpacity',o.opacity,o.opacities.map(x=>[x,x+' %']))):''}
            ${row(esc(T.squadToggle)+`<span class="badge" title="${esc(T.inaccurateWhy)}">${esc(T.inaccurate)}</span>`,esc(T.squadHelp)+'<br>'+esc(T.inaccurateWhy),sw('sq',o.squad,'showSquadTotal'))}</div>
            <section><h2 class="pirate">${esc(T.obs)}</h2><div class="card"><span class="path">${esc(S.paths.overlay)}</span><p class="note" style="color:var(--cmuted)">${esc(T.obsSteps)}</p>
            <div class="actions"><button class="b main" onclick="copyObs()">${esc(copied?T.copied:T.copy)}</button><button class="b" onclick="window.open('/overlay?t='+TOKEN,'_blank')">${esc(T.preview)}</button></div></div></section>`;
          return h;
        }
        function settings(T){
          const s=S.settings;
          let h=`<div class="card">${row(esc(T.language),'',sel('language',s.language,s.languages))}
            ${S.profiles.length>1?row(esc(T.profile),'',sel('profile',S.profileKey,S.profiles.map(p=>[p.key,p.name+' ('+p.matches+' · '+p.chests+')']))):''}
            ${s.windowed?row(esc(T.tray),esc(T.trayHelp),sw('tr',s.minimizeToTray,'minimizeToTray')):''}
            ${s.canStartWithFortnite?row(esc(T.withFortnite),esc(T.withFortniteHelp),sw('wf',s.startWithFortnite,'startWithFortnite')):''}
            ${row(esc(T.updates),'',sel('updateMode',s.updateMode,[['prompt',T.upPrompt],['auto',T.upAuto],['off',T.upOff]]))}
            <div class="row"><div style="flex:1"><div class="t">${esc(T.demos)}</div>${s.demosExists?'':`<div class="h">${esc(T.demosMissing)}</div>`}<div class="actions" style="margin-top:6px"><input type="text" id="demos" value="${esc(s.demos)}"><button class="b" onclick="api('demosFolder',$('#demos').value)">${esc(T.save)}</button></div></div></div></div>`;
          {const ed=S.stats.edit;const fld=(k,lab,w,ty)=>`<label style="display:flex;flex-direction:column;gap:3px;font-size:12px;color:var(--cmuted)">${esc(lab)}<input type="${ty||'text'}" data-f="${k}" data-o="${esc(ed[k])}" value="${esc(ed[k])}" style="width:${w}px"></label>`;
            h+=`<div class="card" style="margin-top:12px"><div class="t">${esc(T.uEditTitle)}</div><div class="h">${esc(T.uEditHelp)}</div>
            <div id="stform" class="actions" style="margin-top:8px;align-items:flex-end">${fld('level',T.uLevel,90,'number')}${fld('name',T.uName,200)}${fld('season',T.uSeason,230)}</div>
            <div class="actions" style="margin-top:8px;align-items:flex-end" id="stform2">${fld('chests',T.uFChests,120)}${fld('best',T.uFBest,150)}${fld('matches',T.uFMatches,100)}${fld('wins',T.uFWins,100)}${fld('losses',T.uFLosses,100)}${fld('kills',T.uFKills,100)}${fld('time',T.uFTime,120)}${fld('damage',T.uFDamage,120)}${fld('assists',T.uFAssists,100)}${fld('accuracy',T.uFAccuracy,100)}</div>
            <div class="actions" style="margin-top:10px"><button class="b main" onclick="saveStats()">${esc(T.save)}</button><button class="b" onclick="api('openObsFolder')">${esc(T.uObsFolder)}</button></div><div class="h" style="margin-top:6px">${esc(S.paths.obs)}</div>
            <div class="h" style="margin-top:10px">${esc(T.uKeys)}</div></div>`}
          return h;
        }
        // Chests added by hand (and the estimate from the Career matches): p = 's' in the settings, 'd' in the edit box.
        function manualForm(T,p){
          return `<p class="note" style="color:var(--cmuted);margin-top:0">${esc(T.manualHelp)}</p>
            <div class="actions"><input type="number" id="${p}manual" value="${S.manual}" step="1" aria-label="${esc(T.manual)}"><button class="b main" onclick="saveManual('${p}',parseInt($('#${p}manual').value||'0',10))">${esc(T.save)}</button></div>
            <div class="t" style="margin-top:16px;font-weight:600">${esc(T.estimate)}</div><p class="note" style="color:var(--cmuted)">${esc(T.estimateHelp)}</p>
            <div class="actions"><input type="number" id="${p}career" min="0" placeholder="${esc(T.career)}" aria-label="${esc(T.career)}"><button class="b" onclick="doEstimate('${p}')">${esc(T.compute)}</button></div>
            <div class="estimate" id="${p}est">${estimate&&estimate.p===p?estimateHtml(T,p):''}</div>`;
        }
        function estimateHtml(T,p){if(estimate.error)return esc(estimate.error);return esc(estimate.text)+`<div class="actions" style="margin-top:8px"><button class="b main" onclick="saveManual('${p}',${estimate.chests})">${esc(fmt(T.add,estimate.chestsText))}</button></div>`}
        async function doEstimate(p){const r=await api('estimate',parseInt($('#'+p+'career').value||'0',10));estimate=Object.assign({p},r);const e=$('#'+p+'est');if(e)e.innerHTML=estimateHtml(S.txt,p)}
        async function saveManual(p,n){estimate=null;await api('manualChests',n);if(p==='d')$('#dlg').close();else render(true)}
        // The edit box of "chests in total" (home tab, and the report's pencil through a message).
        function openManual(){const T=S.txt,d=$('#dlg');estimate=null;$('#dlgT').textContent=T.editChests;$('#dlgB').innerHTML=manualForm(T,'d');$('#dlgX').setAttribute('aria-label',T.close);if(!d.open)d.showModal();const i=$('#dmanual');if(i){i.focus();i.select()}}
        $('#dlgX').onclick=()=>$('#dlg').close();
        $('#wmin').onclick=()=>winBtn('min');$('#wmax').onclick=()=>winBtn('max');$('#wclose').onclick=()=>winBtn('close');
        $('#dlg').addEventListener('click',e=>{if(e.target.id==='dlg')$('#dlg').close()});
        window.addEventListener('message',e=>{if(e.origin===location.origin&&e.data&&e.data.cc==='editChests'&&S)openManual()});
        async function copyObs(){await api('copyObsPath');copied=true;render(true);setTimeout(()=>{copied=false;render(true)},2000)}
        function help(T){
          return `<div class="card"><p style="margin-top:0">${esc(T.helpText)}</p><div class="actions"><button class="b" onclick="api('openDataFolder')">${esc(T.dataFolder)}</button>${S.paths.errorLog?`<button class="b" onclick="api('openErrorLog')">${esc(T.errorLog)}</button>`:''}<button class="b" onclick="api('openIssues')">${esc(T.issue)}</button><button class="b" onclick="api('openRepo')">${esc(T.project)}</button></div></div>
          ${S.canUninstall?`<section><h2 class="pirate">${esc(T.uninstall)}</h2><div class="card"><span class="path">${esc(S.paths.data)}</span>
          <div class="actions"><button class="b danger" onclick="uninstall(false)">${esc(T.uninstall)} · ${esc(T.keepData)}</button><button class="b danger" onclick="uninstall(true)">${esc(T.uninstall)} · ${esc(T.deleteData)}</button></div></div></section>`:''}`;
        }
        function closedView(){stopped=true;$('#app').innerHTML='<div class="card">'+esc(S.txt.closed)+'</div>';$('#foot').innerHTML='';$('#reportPane').classList.add('hidden')}
        async function uninstall(all){if(!confirm(S.txt.uninstallConfirm))return;await api('uninstall',all);closedView()}
        async function quit(){if(!confirm(S.txt.quitConfirm))return;try{await api('quit')}catch(e){}closedView()}
        refresh(true);setInterval(()=>refresh(false),2000);
        </script></body></html>
        """;
}
