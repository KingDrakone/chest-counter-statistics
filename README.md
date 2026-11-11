Fortnite Chest Counter
A Fortnite chest counter and stats tracker written in C#. It runs as a console app (v1.5.0) and shows your numbers in an in-game overlay, so you can follow your progress without leaving the match.

Features
Total chests opened, plus chests from the last match and the current session
Personal best (most chests in a single match)
Full account stats: wins, losses, win rate, matches, K/D, kills, kills per match, play time, average match time, damage, assists and accuracy
Account level display
In-game overlay with three views (mini bar, chests panel, full stats cards)
OBS cards for streamers, with your name and season shown on them
Multiple languages (Portuguese included)
Overlay views
Mini bar A tiny overlay with the essentials: total chests, last match and session chests.

Mini bar A tiny overlay with the essentials...

Captura de tela 2026-11-10 104636
Chests panel

Total chests...
Captura de tela 2026-11-10 104619
Full stats panel

Chests: ...
Captura de tela 2026-11-10 104425
Chests panel

Total chests and chests from the last match
Session: chests opened, matches played and average per match
Wins / losses
Total kills
Full stats panel

Chests: total and last match
Results: wins, losses, win rate and matches played
Combat: K/D, kills per match and total kills
Time: total play time and average time per match
Performance: damage dealt, assists and accuracy
Account level in the header
Using the app
The console app has three tabs: Home, Settings and Help. In Settings you can edit your stats and change:

Overlay on/off, style (classic dark card or numbers only), size and opacity
Minimize to the notification area
Open the report on startup
Launch together with Fortnite
Replays folder
Automatic updates
OBS card name and season
Keyboard shortcuts
Shortcut	Action
Ctrl+Shift+O	Show / hide the overlay (works in game)
Ctrl+Shift+L	Lock / unlock the overlay (unlock it to move it)
Arrow keys	Navigate the menu
Enter	Confirm / change a setting
How to build
Install the .NET 10 SDK.
Run COMPILAR.bat (takes 1 to 2 minutes the first time).
Open the generated ChestCounter.exe in the main folder.
The executable is self-contained: you don't need .NET installed to run it. The baus.ini file must stay in the same folder as the .exe.

Author
KingDrakone
