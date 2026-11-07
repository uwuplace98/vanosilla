#!powershell # Startscript BazaarServer (Wirtschaftssystem)

# ============================================
# Vanosilla BazaarServer Startscript
# Wirtschaft & Trading System gRPC Server
# ============================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Vanosilla BazaarServer Startup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# --------------------------------------------
# 1. Prerequisites prüfen
# --------------------------------------------
Write-Host "`n[1/6] Verfügbare Services prüfen..." -ForegroundColor Yellow

$requiredServices = @(
    'Docker Desktop',
    'PostgreSQL (port 5432)',
    'MongoDB (port 27017)',
    'Redis (port 6379)',
    '.NET 8 SDK'
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

# --------------------------------------------
# 2. Umgebungsvariablen setzen
# --------------------------------------------
Write-Host "`n[2/6] Umgebungsvariablen konfigurieren..." -ForegroundColor Yellow

$env:BazaarServer_Port = "50051"
$env:PostgreSQL_Connection = "Host=localhost;Port=5432;Database=bazaar"
$env:MongoDB_Connection = "mongodb://localhost:27017/vanosilla"
$env:Redis_Connection = "localhost:6379"

Write-Host "  BazaarServer_Port=$($env:BazaarServer_Port)" -ForegroundColor Gray
Write-Host "  PostgreSQL_Connection=$($env:PostgreSQL_Connection)" -ForegroundColor Gray

# --------------------------------------------
# 3. Datenbank initianisieren (optional)
# --------------------------------------------
Write-Host "`n[3/6] Datenbank-Schema applyieren..." -ForegroundColor Yellow

$schemaFile = "Schemas\bazaar-schema.sql"
if (Test-Path $schemaFile) {
    Write-Host "  Schema-Datei gefunden: $($schemaFile)" -ForegroundColor Green

    # PostgreSQL init via psql
    try {
        & "pg_ctl start" -D "$(Get-Content 'C:\Program Files\PostgreSQL\15\data\pg_version')\data' "
        . & "psql -U postgres -d bazaar -f '$schemaFile'" 2>$null
        Write-Host "  ✓ Schema erfolgreich angewendet" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠ Schema-Anwendung fehlgeschlagen (manuell via pgAdmin möglich)" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ✗ Schema-Datei nicht gefunden: $($schemaFile)" -ForegroundColor Red
}

# --------------------------------------------
# 4. Build prüfen
# --------------------------------------------
Write-Host "`n[4/6] Build-Prozess prüfen..." -ForegroundColor Yellow

$buildPath = Join-Path (Get-Location) "server"
$csprojPath = Join-Path $buildPath "srcs"BazaarServer\BazaarServer.csproj

if (!(Test-Path $csprojPath)) {
    Write-Host "  ⚠ Projektdatei nicht gefunden: $($csprojPath)" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ Projektdatei gefunden" -ForegroundColor Green

    # Build ausführen (im Hintergrund)
    try {
        & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
            /m:`false `
            /t:Restore,Build `
            /p:Configuration=Release `
            $csprojPath

        Write-Host "  ✓ Build erfolgreich" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Build fehlgeschlagen (Fehler: $_)" -ForegroundColor Red
    }
}

# --------------------------------------------
# 5. Server starten
# --------------------------------------------
Write-Host "`n[5/6] BazaarServer starten..." -ForegroundColor Yellow

try {
    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe" restore `
        $csprojPath

    Write-Host "  Restoring packages..." -ForegroundColor Gray

    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe" build `
        $csprojPath --configuration Release

    Write-Host "  Build erfolgreich" -ForegroundColor Green

    # gRPC Server starten
    & "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe" run `
        --project $csprojPath

} catch {
    Write-Host "  ✗ Server-Start fehlgeschlagen (Fehler: $_)" -ForegroundColor Red
    exit 1
}

Write-Host "`n[6/6] BazaarServer läuft auf Port $($env:BazaarServer_Port)" -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Server läuft!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Zusätzliche Informationen:" -ForegroundColor Gray
Write-Host "- gRPC Endpoint: http://localhost:$($env:BazaarServer_Port)" -ForegroundColor DarkGray
Write-Host "- Test-Abfrage: curl http://localhost:$($env:BazaarServer_Port)/Health" -ForegroundColor DarkGray
Write-Host "- Log-Datei: server/dist/BazaarServer/Logs/app.log" -ForegroundColor DarkGray
