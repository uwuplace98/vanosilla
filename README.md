# Vanosilla - nosTale Open-Source Emulation

**Vanosilla ist eine vollständige Backend-Emulation des Fantasy-Multiplayer-RPG nosTale.** Das Projekt enthält einen .NET 8 gRPC Server, Datenhaltung (PostgreSQL/MongoDB/Redis) und Client-Daten für das Spielsystem.

## 🎮 Projektübersicht

- **Spiel**: nosTale (Fantasy MMORPG im "Wings"-Konzept)
- **Technologie**: .NET 8 mit C# 17/18, gRPC Core, ASP.NET Core WebAPI
- **Datenbanken**: PostgreSQL (GameData + Phoenix), MongoDB (Logs), Redis (Cache)
- **Messaging**: MQTT Broker für Event-Kommunikation

## 📁 Projektstruktur

| Ordner | Inhalt |
|--------|--------|
| `server/` | 140+ .NET-Projekte (Bazaar, Login, Family, DiscordNotifier, etc.) |
| `server-translations/` | YAML Übersetzungen für serverseitige Dialoge (720+ Einträge) |
| `client-files/` | Binäre .dat-Daten, 696+ Maps, Client-Übersetzungen (12 Sprachen) |

### Wichtige Server-Komponenten
- **BazaarServer** - Wirtschaft & Trading System via gRPC
- **DatabaseServer** - Spieler-Accounts, Charaktere, Inventar
- **FamilyServer** - Familiengruppen, Erfolge, Shared Warehouse Logs
- **DiscordNotifier** - Discord Webhook Integration
- **LoginServer** - Authentifizierung gegen nosTale Login-Protokoll

## 🚀 Startanweisungen (Vollständige Deploy-Lifecycle)

### 1. Prerequisites prüfen
- Windows 10/11 oder Linux
- .NET 8 SDK installiert (`dotnet --version` ≥ 8.0)
- PostgreSQL Server läuft (lokaal oder Docker: `docker run -d --name postgres -e POSTGRES_PASSWORD=secret -p 5432 postgres:15`)
- MongoDB Server läuft (Docker: `docker run -d --name mongodb -p 27017:27017 mongo:6`)
- Redis Stack läuft (Docker: `docker run -d --name redis-stack -p 6379:6379 redis:7`)
- MQTT Broker (emqx oder Mosquitto)

### 2. Projekt laden & Build-Prozess
```powershell
# Alle Server in einem Restore/Build ausführen
cd C:\Users\nizar\Desktop\vanosilla\server
dotnet restore
dotnet build --configuration Release

# Einzelne Projekte direkt:
dotnet run --project BazaarServer.Core
dotnet run --project DatabaseServer.Core
```

**Startbefehle pro Server:**
```powershell
# Beispiel: BazaarServer starten (Port 50051)
$env:BazaarServer_Port=50051
$env:PostgreSQL_Connection="Host=localhost;Port=5432;Database=bazaar"
dotnet run --project BazaarServer.Core

# DatabaseServer starten
$env:PostgreSQL_Connection="Host=localhost;Port=5432;Database=game-data"
dotnet run --project DatabaseServer.Core

# LoginServer authentifizieren
$env:LoginServer_Port=50055
dotnet run --project LoginServer.Core
```

### 3. Umgebungsvariablen konfiguriert (in `.env` oder direkt über CMD)
```powershell
# Port-Konfiguration für alle Services
BazaarServer_Port=50051
DatabaseServer_Port=50052
LoginServer_Port=50055
FamilyServer_Port=50056
DiscordNotifier_Port=50057
PostgreSQL_Connection="Host=localhost;Port=5432;Database=bazaar"
MongoDB_Connection="mongodb://localhost:27017/vanosilla"
Redis_Connection="localhost:6379"

# Starte alle Server als Prozesse im Hintergrund (PowerShell):
Start-Process powershell -ArgumentList "-NoProfile -Command & { while ($true) {} }", "BazaarServer.Core"
Start-Process powershell -ArgumentList "-NoProfile -Command & { while ($true) {} }", "DatabaseServer.Core"
```

**Alternativ:** Projekt in Visual Studio 2022 laden und Release-Derive mit `Build -> Build Solution` kompilieren, dann `Debug -> Start without Debugging`.

### 4. gRPC Inter-Service Communication testen
```powershell
# LoginAuthentifizierung via gRPC gegen Bazaar prüfen:
dotnet run --project BazaarServer.Core

# Client in einem anderen Fenster für die API aufsetzen:
curl -X POST http://localhost:50051/Auth/CheckIn -H "Content-Type: application/json" -d '{"account":"test","character":"test"}'
```

## 🧪 Testanweisungen

### Unit-Tests
```powershell
# Datenbank-Services testen
dotnet test --project DatabaseServer.Core.Tests
dotnet test --project BazaarServer.Core.Tests

# Integrationstests (Login ↔ Bazaar)
dotnet test --filter "CompleteIntegration"

# Alle Tests in einem Projekt ausführen:
dotnet test --project WingsAPI.Plugins.Plugin.Raids
```

### Manuelle Tests
- **LoginFlow**: LoginServer → DatabaseServer Datenbankcheck
- **WalletCheck**: LoginServer → BazaarServer gRPC Abfrage
- **FamilySystem**: FamilyServer Datenpersistenz via PostgreSQL

## 📚 Dokumentierte Features (nach Server-Feature)

| Feature | Dateien/Projekte |
|---------|------------------|
| Account Management | `DatabaseServer.Core`, `PhoenixLib.DAL.EFCore.PGSQL` |
| Trading System | `BazaarServer.Core`, `PhoenixLib.Events` |
| Family/Clan System | `FamilyServer.Core`, `Plugin.FamilyImpl` |
| Discord Integration | `DiscordNotifier.Core`, `Plugin.DB.EF` |
| Quests & Raids | `Plugin.QuestImpl`, `Plugin.Raids` |
| Minigames | `Plugin.Minigames` |
| TimeSpaces | `Plugin.TimeSpaces` |
| Game Events (Distributed) | `WingsEmu.Plugins.DistributedGameEvents` |

## 🔧 Wichtige Verzeichnisse

### server/
- `server/srcs/BazaarServer/` - Wirtschaftssystem gRPC
- `server/srcs/DatabaseServer/` - Datenbank-Persistenz
- `server/srcs/_plugins/` - Feature-Plugins (Quests, Raids, TimeSpaces)
- `server/srcs/PhoenixLib/` - Shared Framework-Bibliothek
- `server/Tests/` - Integrationstests

### server-translations/
- `game-dialog-key.yaml` - Hauptdialoge (720+ Einträge)
- `raids.yaml`, `timespaces.yaml` - Spezialisierte Übersetzungen
- `translations-update.ps1` - Batch-Übersetzungsskript

### client-files/
- `Maps/` - 696+ Map-Dateien der Spielswelt
- `.dat` Dateien - Items, Monster, Quests, Skills
- Übersetzungsdateien (EN, DE, FR, ES, etc.)

## 🐛 Troubleshooting-Hotfixes

### gRPC Verbindungsfehler
```powershell
# Port prüfen:
netstat -ano | findstr :50051  # BazaarServer
netstat -ano | findstr :50052  # DatabaseServer

# Event-Log für Fehler:
Get-EventLog -LogApplication -Source "*gRPC*"
```

### Datenbank-Zugriff-Fehler
```powershell
# PostgreSQL Verbindung testen:
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0

# Logs auf PostgreSQL-Konnektoren prüfen:
# File path: server/srcs/DatabaseServer/Program.cs
```

### Build-Fehler (Missing Packages)
```powershell
# Paketrestore durchführen:
dotnet restore
# Falls Fehler auftreten:
dotnet tool install --global dotnet-protobuf-net-compiler
```

## 📜 Lizenz & Quellen

- **Projetttyp**: Open-Source nosTale Emulation
- **Hauptkomponenten**: .NET Core gRPC Server, PostgreSQL DataLayer, Redis Cache
- **API-Schnittstellen**: Protobuf (gRPC), REST-Endpoints für Admin

## 📮 Kontakt & Issues

Fehlerbericht an `issues@vanosilla-emu.de` mit:
1. Fehlermeldung (exakt aus dem EventLog oder der AppDomain)
2. Port-Konfiguration (gRPC Endpoints, DB-Verbindung)
3. Server-Log-Auszug (zumindest 50 Zeilen vor dem Fehler)

---

*Vanosilla - Open-Source nosTale Backend Emulation*
