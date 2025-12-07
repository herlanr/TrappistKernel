Verfügbare Befehle:

Dateisystem:
- freespace: Zeigt den freien Speicherplatz an.
- touch <Dateiname>: Erstellt eine neue Datei.
- mkdir <Verzeichnisname>: Erstellt ein neues Verzeichnis.
- ls: Listet alle Dateien im aktuellen Verzeichnis auf.
- mv <Dateipfad> <Zielpfad>: Verschiebt eine Datei.
- cat <Dateiname>: Zeigt den Inhalt einer Datei an.
- rmfile <Dateiname>: Löscht eine Datei.
- rmdir <Verzeichnisname>: Löscht ein Verzeichnis.
- rename <alter Name> <neuer Name>: Benennt eine Datei oder ein Verzeichnis um.
- cd <Verzeichnispfad>: Wechselt in ein anderes Verzeichnis.
- pwd: Zeigt das aktuelle Verzeichnis an.

System:
- help: Zeigt verfügbare Befehle und deren Verwendung an.
- clear: Löscht den Bildschirm.
- shutdown: Fährt das System herunter.
- force-shutdown: Das System wird ohne Speichern der Änderungen zwangsweise heruntergefahren.
- reboot: Startet das System neu.
- force-reboot: Das System wird ohne Speichern der Änderungen zwangsweise neu gestartet.

Rechteverwaltung:
- setowner <path>  <newowner>: Ändert den Besitzer der angegebenen Datei/des angegebenen Verzeichnisses in das angegebene Benutzer.
- gperm / givepermissions <path> <user>: Gewährt einem Benutzer Lese- und/oder Schreibrechte für die angegebene Datei/das angegebene Verzeichnis.
- tperm / takepermissions <path> <user>: Entfernt Lese- und/oder Schreibrechte eines Benutzers für die angegebene Datei/das angegebene Verzeichnis.
- perm: Zeigt den Besitzer, die Leser und die Schreiber einer Datei oder eines Verzeichnisses an.
- saveperms: Saves all file permissions.
- initperms: Initialisiert Dateiberechtigungen. (Nur Admins)
- clearperms: Löscht alle Dateiberechtigungen. (Nur Admins)

Benutzerverwaltung:
- login: Meldet einen Benutzer an.
- logout: Meldet den Benutzer ab.
- listusers: Zeigt alle Benutzer im System an.
- createuser / mkusr: Erstellt einen neuen Benutzer. Argumente: -a für Admin (nur Admins).
- changepwd: Ändert das Passwort des aktuellen Benutzers.
- deleteuser <Benutzername> / delusr <Benutzername>: Löscht einen Benutzer (nur Admins).

Text-Editor:
- miv <Dateiname>: Öffnet die Datei im MIV-Viewer.cr

Misc
- trappist: Zeigt Informationen über das TRAPPIST-1-Sternsystem an.
- snake: Startet das Snake-Spiel.
