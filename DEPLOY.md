# Vanosilla Deploy-Guide
**Startanleitung für lauffähige nosTale Emulation**

---

## 🚀 Kurzer Deploy (5 Minuten)

### 1. Prerequisites prüfen
```powershell
dotnet --version        # ≥ 8.0 erforderlich
docker --version        # Docker Desktop empfohlen
node --version          # Für npm-Pakete
```

### 2. Konfiguration (.env)
```powershell
# Kopiere .env.sample → .env
Copy-Item .env.sample .env -Force

# Setze Ports (optional, default: BazaarServer=50051)
$env:BazaarServer_Port = "50051"
```

### 3. Build & Start
```powershell
cd C:\Users\nizar\Desktop\vanosilla

# Alle Server ausbauen
.\scripts\start-all-core-servers.ps1
# Alternative: Manuelles Build für BazaarServer
dotnet restore server\srcs\BazaarServer\BazaarServer.csproj
dotnet build server\srcs\BazaarServer\BazaarServer.csproj /p:Configuration=Release
```

### 4. Health Check
```powershell
curl http://localhost:50051/Health
# Antwort: '{"healthy":true,...}'
```

**Startstatus:** ✅ **Laufbereit!** (Core-Services im Hintergrund)

---

## 📦 Voller Deploy Lifecycle

### Schritt 1: PostgreSQL initialisieren
```powershell
# Docker Container erstellen
docker run -d --name postgres `
  -e POSTGRES_PASSWORD=secret `
  -e POSTGRES_DB=bazaar `
  -p 5432:5432 `
  postgres:15

# Datenbank-Schema ausführen
psql -U postgres -d bazaar -f server\Schemas\bazaar-schema.sql
```

### Schritt 2: MongoDB & Redis starten
```powershell
# MongoDB (Logs, Family-Storage)
docker run -d --name mongodb `
  -p 27017:27017 `
  mongo:6

# Redis (Cache Layer)
docker run -d --name redis-stack `
  -p 6379:6379 `
  redis:7
```

### Schritt 3: DatabaseServer starten (Datenbank-Persistenz)
```powershell
# Port setzen
$env:DatabaseServer_Port = "50052"
$env:PostgreSQL_Connection = "Host=localhost;Port=5432;Database=bazaar"

# Server im Hintergrund starten
.\scripts\start-bazaar-server.ps1
```

### Schritt 4: BazaarServer starten (Trading/Wirtschaft)
```powershell
# Port setzen
$env:BazaarServer_Port = "50051"

# gRPC Service starten
dotnet run --project server\srcs\BazaarServer\BazaarServer.csproj
```

### Schritt 5: LoginServer authentifizieren
```powershell
# Auth Flow gegen BazaarServer prüfen
$env:LoginServer_Port = "50055"
dotnet run --project server\srcs\LoginServer\LoginServer.csproj
```

**Ende Deploy:** ✅ **Alle Services im Betrieb!**

---

## 📚 Dokumentierte Features

| Service | Port | Funktion |
|---------|------|----------|
| DatabaseServer | 50052 | PostgreSQL Persistenz (Accounts/Transactions) |
| BazaarServer | 50051 | Trading/Wirtschaft gRPC API |
| LoginServer | 50055 | Authentifizierung (Auth Check-In) |
| FamilyServer | 50056 | Familiensystem & Erfolge |
| DiscordNotifier | 50057 | Discord Webhook Integration |

### API Endpoints
- **Health**: `GET /Health` → Status der Service
- **Auth**: `POST /Auth/CheckIn` → LoginFlow (gRPC)
- **Wallet**: `GET /Wallet/Balance` → Account-Balance abfragen
- **Trade**: `POST /Auth/Trade` → Item-Transaktionen

---

## 🧪 Testbefehle

### Unit-Tests ausführen
```powershell
dotnet test server\srcs\BazaarServer\Core.Tests\BazaarServer.Core.Tests.csproj
# Ausgeben: 5 Tests erfolgreich
```

### Integration Tests
```powershell
dotnet test --filter "CompleteIntegration"
# Testet Login ↔ BazaarFlow
```

---

## 🐛 Troubleshooting

### Problem: Build fehlgeschlagen (Error MSB1009)
**Ursache:** MSBuild-Datei nicht gefunden, Visual Studio 2022 nicht installiert

**Lösung:**
```powershell
# Paket restore ausführen
dotnet restore server\srcs\BazaarServer\BazaarServer.csproj
# Fallback: NuGet direkt laden
Install-Package Grpc.AspNetCore -Version 2.62.0
```

### Problem: gRPC Verbindungsfehler (Port nicht erreichbar)
**Ursache:** DatabaseServer läuft, BazaarServer-Service nicht gestartet

**Lösung:**
```powershell
# Port prüfen (gRPC Endpoints)
netstat -ano | findstr :50051  # BazaarServer
netstat -ano | findstr :50052  # DatabaseServer

# Event-Log prüfen für gRPC-Fehler
Get-EventLog -LogApplication -Source "*grpc*" -MaxEvents 20
```

### Problem: Datenbank-Zugriff nicht möglich (Connection refused)
**Ursache:** PostgreSQL Server nicht läuft oder Schema nicht angewendet

**Lösung:**
```powershell
# PostgreSQL starten (Docker recommended)
docker run -d --name postgres -p 5432:5432 postgres:15

# Schema-Datei ausführen
psql -U postgres -f server\Schemas\bazaar-schema.sql
```

### Problem: Build fehlgeschlagen (Missing Packages)
**Ursache:** NuGet-Pakete nicht installiert

**Lösung:**
```powershell
dotnet tool install --global dotnet-protobuf-net-compiler
# Paket restore neu starten
dotnet restore server\srcs\BazaarServer\BazaarServer.csproj
```

---

## 📜 Lizenz & Quellen

- **Projekttyp**: Open-Source nosTale Emulation (Backend gRPC)
- **Hauptkomponenten**: .NET 8 Core, PostgreSQL, MongoDB, Redis, MQTT
- **API-Schnittstellen**: Protobuf gRPC, REST Endpoints

**Lizenz:** MIT License (Vanosilla Ecosystem)

---

*Vanosilla - Open-Source nosTale Backend Emulation*
