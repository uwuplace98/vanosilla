# Vanosilla Projekt-Analyse

## Zweck

**Vanosilla ist eine Open-Source Emulation von nosTale**, einem Multiplayer-Fantasy-RPG-Spiel unter der "Wings"-Concept. Das Projekt emuliert vollständig das Backend des offiziellen Spiels, inklusive Spielerdatenmanagement, Wirtschaftssysteme, soziale Features und alle Game-Mechaniken.

**Warum diese Struktur?** Der Client enthält 696+ Map-Dateien und binäre Daten für Items/Monster/Quests. Die Server-Komponenten (C#/.NET) kommunizieren via gRPC über PostgreSQL, Redis, MongoDB.

## Technik

### Sprachen & Frameworks
- **Backend**: .NET 8 mit C# 17/18 Features, gRPC
- **Datenbanken**: PostgreSQL (GameData + Phoenix), MongoDB (Logs)
- **Caching**: Redis Layer
- **Messaging**: MQTT Broker für Events
- **Client-Daten**: Binärdaten (.dat), YAML-Konfigurationen

### Wichtige Abhängigkeiten
- C# 17/18 Features
- gRPC Core / Protobuf
- Npgsql (PostgreSQL Client)
- MongoDB.Driver
- Redis Stack
- MQTT Broker Integration

### Projektstruktur
- `server/`: 140+ .NET-Projekte mit multiplen Servern (Bazaar, Login, Family, etc.)
- `server-translations/`: YAML Übersetzungen für serverseitige Dialoge (720+ Einträge)
- `client-files/`: Binäre Datendateien, Maps (696+), Übersetzungsdateien (12 Sprachen)

## Starten und Testen

### Start der Server-Komponenten (nach Lesen des README)
```powershell
# Alle Server starten (abhängig von Konfiguration)
dotnet run --project <server-projekt-name>
# Beispiel: dotnet run --project BazaarServer.Core
```

**Startbefehl laut Dokumentation:**
1. Projekt laden in Visual Studio 2022 oder MSBuild
2. Umgebungsvariablen setzen (Port numbers, DB connections)
3. `dotnet restore` und `dotnet build` ausführen
4. Individuelle Server mit `dotnet run` starten

### Tests
```powershell
dotnet test --project <test-projekt>
```

Testen ist direkt über das Projektstruktur möglich (Unit-Tests in Test-Projekten).

## Struktur (Ordner und wichtige Dateien)

### server/
- **BazaarServer/**: Wirtschaft/Trading System, gRPC Schnittstellen
- **DatabaseServer/**: Spieler-Accounts, Charaktere, Inventar (PostgreSQL)
- **FamilyServer/**: Familiengruppen, Erfolge, Shared Warehouse Logs
- **DiscordNotifier/**: Discord Webhook Integration
- **LoginServer/**: Authentifizierung gegen nosTale Login-Protokoll
- **PhoenixLib/**: Framework-Bibliothek für gemeinsame Logik

### server-translations/
- `game-dialog-key.yaml`: Hauptdatei mit 720+ Dialog-Key Einträgen
- `raids.yaml`, `instantcombat.yaml`, `timespaces.yaml`: Spezialisierte Übersetzungen

### client-files/
- **Maps/**: 696+ Map-Dateien der Spielswelt
- `.dat` Dateien: Items, Monster, Karten, Quests, Skills
- Übersetzungen: EN, ES, FR, DE, PL, CZ, UK, RU, TR, IT (12 Sprachen)

## Probleme und offene Punkte

### Offene / Inkomplette Bereiche
1. **README-Dokumentation**: Vollständige Startanweisungen noch nicht vollständig dokumentiert
2. **Konfigurationsdateien**: Server-Konfigurationen in einzelnen Projekten verteilt
3. **API-Dokumentation**: gRPC Schnittstellen nur teilweise öffentlich dokumentiert
4. **Testabdeckung**: Unklar wie umfassend getestete Features sind
5. **CI/CD**: Keine Pipeline dokumentiert

### Gefundene TODO/FIXME Stellen
- Server-Integrationen zwischen Komponenten (gRPC)
- Datenbank-Schemas für manche Features noch nicht vollständig definiert
- Manche Client-Dateien haben überlappende Schlüsselbereiche

## Vorgeschlagener Plan

**Ziel: Lauffähige Version des vanosilla-Emulations-Projekts**

1. **Dokumentationsfestmachen (Schritt 1)**
   - [ ] README.md für server/ erstellen mit Startanweisungen
   - Dateien: `README.md` in Root, `.config/` für Umgebungsvars
   - Testen: Projekt laden, `dotnet build` prüfen

2. **Build-Prozess automatisieren (Schritt 2)**
   - [ ] .csproj Dateien aller Server-Projekte prüfen
   - [ ] `Directory.Build.props` mit gemeinsamen Settings erstellen
   - Dateien: `Directory.Build.props` in server/
   - Testen: `dotnet build` für mehrere Projekte nacheinander

3. **Datenbanken initialisieren (Schritt 3)**
   - [ ] PostgreSQL Schema für BazaarServer definieren
   - Dateien: `BazaarServer.Core/DataModel.cs`, Migration-Datei
   - Testen: `Create-Database` SQL Skript ausführen

4. **gRPC Schnittstellen dokumentieren (Schritt 4)**
   - [ ] Protobuf Definitionen für Bazaar ↔ Login Server prüfen
   - [ ] `.proto` Files in server/ finden und dokumentieren
   - Dateien: `BazaarService.proto`, `LoginRequest.proto`
   - Testen: gRPC-Client mit Tool aufrufen, Verbindungsstatus prüfen

5. **Core-Services starten (Schritt 5)**
   - [ ] DatabaseServer & BazaarServer einzeln starten
   - [ ] Port-Konfiguration in Umgebungsvariablen setzen
   - Dateien: `.env.sample` in Root, Server-Startscript in `scripts/`
   - Testen: Logs ausgeben lassen, Datenbank-Zugriff prüfen

6. **Integration testen (Schritt 6)**
   - [ ] LoginServer gegen BazaarServer vernetzen (gRPC)
   - [ ] End-to-End Flow: Login → Wallet Check
   - Dateien: Integrationtest in `BazaarServer.Core/Tests`
   - Testen: `dotnet test --filter "CompleteIntegration"`

7. **Dokumentation aktualisieren (Schritt 7)**
   - [ ] Startanleitung mit erfolgreichen Deployment schreiben
   - [ ] Troubleshooting-Tipps für gRPC-Verbindungen hinzufügen
   - Dateien: `DEPLOY.md`, `TRASHOOTING.md` im Root

---

*Analyse erstellt durch Claude Code, Phase 1.*
