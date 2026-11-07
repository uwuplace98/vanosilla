#!powershell # Startscript für alle Core-Server (Database + Bazaar)

# ============================================
# Vanosilla Core-Services Startup
# Datenbank-Persistenz & Trading System
# ============================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Vanosilla Core-Servers Startup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$requiredServices = @(
    'Docker Desktop',
    'PostgreSQL (port 5432)',
    'MongoDB (port 27017)',
    'Redis (port 6379)'
)

foreach ($svc in $requiredServices) {
    Write-Host "  - $($svc)" -ForegroundColor Gray
}

# PostgreSQL prüfen
if (Test-NetConnection -Port 5432 -ErrorAction SilentlyContinue | Select-Object -First 1) {
    Write-Host "  ✓ PostgreSQL läuft" -ForegroundColor Green
} else {
    Write-Host "  ✗ PostgreSQL NACHRUSTEN (Docker: docker run -d postgres)" -ForegroundColor Red
}

# MongoDB prüfen
if (Test-NetConnection -Port 27017 -ErrorAction SilentlyContinue | Select-Object -First 1) {
    Write-Host "  ✓ MongoDB läuft" -ForegroundColor Green
} else {
    Write-Host "  ✗ MongoDB NACHRUSTEN (Docker: docker run -d mongo)" -ForegroundColor Red
}

# Redis prüfen
if (Test-NetConnection -Port 6379 -ErrorAction SilentlyContinue | Select-Object -First 1) {
    Write-Host "  ✓ Redis läuft" -ForegroundColor Green
} else {
    Write-Host "  ✗ Redis NACHRUSTEN (Docker: docker run -d redis)" -ForegroundColor Red
}

# --------------------------------------------
# Build alle Core-Projekte prüfen
# --------------------------------------------
Write-Host "`n[1/5] Build-Prozess für Core-Services prüfen..." -ForegroundColor Yellow

$buildPath = "C:\Users\nizar\Desktop\vanosilla\server"

# DatabaseServer-Projekt finden
$dbcsproj = Join-Path $buildPath "srcs\DatabaseServer\DatabaseServer.csproj"
$bazcProj = Join-Path $buildPath "srcs\BazaarServer\BazaarServer.csproj"

$projects = @($dbcsproj, $bazcProj)

foreach ($proj in $projects) {
    if (Test-Path $proj) {
        Write-Host "  ✓ Projekt gefunden: $($proj)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Projekt nicht gefunden: $($proj)" -ForegroundColor Red
    }
}

# Build ausführen
try {
    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
        $dbcsproj /t:Restore,Build /p:Configuration=Release 2>&1 | Out-Null
    Write-Host "  ✓ DatabaseServer Build erfolgreich" -ForegroundColor Green
} catch {
    Write-Host "  ✗ DatabaseServer Build fehlgeschlagen (Fehler: $_)" -ForegroundColor Red
}

try {
    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
        $bazcProj /t:Restore,Build /p:Configuration=Release 2>&1 | Out-Null
    Write-Host "  ✓ BazaarServer Build erfolgreich" -ForegroundColor Green
} catch {
    Write-Host "  ✗ BazaarServer Build fehlgeschlagen (Fehler: $_)" -ForegroundColor Red
}

# --------------------------------------------
# Datenbank initianisieren (optional)
# --------------------------------------------
Write-Host "`n[2/5] Datenbank-Schema applyieren..." -ForegroundColor Yellow

$schemaFile = "Schemas\bazaar-schema.sql"
if (Test-Path $schemaFile) {
    Write-Host "  Schema-Datei gefunden: $($schemaFile)" -ForegroundColor Green

    # PostgreSQL init via psql
    try {
        # Start DatabaseServer im Hintergrund (mit AutoMigrate)
        & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe" run `
            --project $dbcsproj --configuration Release --no-build

        Write-Host "  ✓ DatabaseServer läuft (automatisch Migrations apply)" -ForegroundColor Green

    } catch {
        Write-Host "  ⚠ DatabaseServer Start fehlgeschlagen (manuell via pgAdmin möglich)" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ✗ Schema-Datei nicht gefunden: $($schemaFile)" -ForegroundColor Red
}

# --------------------------------------------
# Core-Services starten
# --------------------------------------------
Write-Host "`n[3/5] DatabaseServer starten..." -ForegroundColor Yellow

try {
    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe" restore `
        $dbcsproj

    Write-Host "  Restoring packages..." -ForegroundColor Gray

    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe" build `
        $dbcsproj --configuration Release

    Write-Host "  Build erfolgreich" -ForegroundColor Green

    # gRPC Server starten (im Hintergrund)
    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe" run `
        --project $dbcsproj --configuration Release --no-build
} catch {
    Write-Host "  ✗ DatabaseServer-Start fehlgeschlagen (Fehler: $_)" -ForegroundColor Red
}

Write-Host "`n[4/5] BazaarServer starten..." -ForegroundColor Yellow

try {
    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe" restore `
        $bazcProj

    Write-Host "  Restoring packages..." -ForegroundColor Gray

    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe" build `
        $bazcProj --configuration Release

    Write-Host "  Build erfolgreich" -ForegroundColor Green

    # gRPC Server starten (im Hintergrund)
    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe" run `
        --project $bazcProj --configuration Release --no-build

} catch {
    Write-Host "  ✗ BazaarServer-Start fehlgeschlagen (Fehler: $_)" -ForegroundColor Red
}

# --------------------------------------------
# Integration Status prüfen
# --------------------------------------------
Write-Host "`n[5/5] Integration Status prüfen..." -ForegroundColor Yellow

# gRPC Health Endpoints
$dbUrl = "http://localhost:50052/Health"
$bazUrl = "http://localhost:50051/Health"

Write-Host ""
Write-Host "Integration-Status:" -ForegroundColor Gray
Write-Host "- DatabaseServer (Port 50052): $(if ($dbUrl) { Write-Host 'Verbindungsstatus prüfen mit curl' })" -ForegroundColor Gray
Write-Host "- BazaarServer (Port 50051): $(if ($bazUrl) { Write-Host 'Verbindungsstatus prüfen mit curl' })" -ForegroundColor Gray

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Core-Servers Status:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Test Health Endpoints via curl (falls verfügbar)
try {
    $dbHealth = & "curl.exe -s http://localhost:50052/Health" 2>$null
    Write-Host "DatabaseServer-Health: $dbHealth" -ForegroundColor Gray

    $bazHealth = & "curl.exe -s http://localhost:50051/Health" 2>$null
    Write-Host "BazaarServer-Health: $bazHealth" -ForegroundColor Gray
} catch {
    Write-Host "Service-Status kann nicht geprüft werden (Startete im Hintergrund)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Zusätzliche Informationen:" -ForegroundColor Gray
Write-Host "- Logs-Directory: server/dist/*/*/Logs/" -ForegroundColor DarkGray
Write-Host "- Event-Log: EventViewer → Applications" -ForegroundColor DarkGray
Write-Host ""
