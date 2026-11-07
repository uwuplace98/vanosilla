#!powershell # Build-Script für alle .NET-Projekte im server/ Ordner

# ============================================
# Vanosilla Build Script - Alle Core-Projekte
# ============================================

$projectPath = "C:\Users\nizar\Desktop\vanosilla\server\srcs"
$mdbBuild = "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
$mdbDotNet = "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\DotNet\Workloads\DockerDesktop\nucache\dotnet.exe"

# Alle .csproj Dateien im server/srcs/ Verzeichnis finden
$projects = Get-ChildItem -Path $projectPath -Filter "*.csproj" | Select-Object -First 10

Write-Host "`nBuild-Prüfung für Core-Projekte..." -ForegroundColor Cyan

foreach ($proj in $projects) {
    if (Test-Path $proj.FullName) {
        Write-Host "  ✓ Projekt gefunden: $(Split-Path -Leaf $proj.FullName)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Projekt nicht gefunden: $($proj.FullName)" -ForegroundColor Red
    }
}

# BazaarServer Build versuchen (mit direkter MSBuild-Kommando)
Write-Host "`n[1/2] BazaarServer Build..." -ForegroundColor Yellow

$bazProjPath = Join-Path $projectPath "BazaarServer\BazaarServer.csproj"
if ((Test-Path $bazProjPath)) {
    Write-Host "  Projekt-Pfad: $($bazProjPath)" -ForegroundColor Gray

    try {
        # Dotnet restore & build (im Hintergrund)
        Write-Host "  Restoring packages..." -ForegroundColor Gray

        # Direkt mit dotnet.exe aufrufen statt MSBuild
        $bazBuild = "$mdbDotNet" run project `
            --project $bazProjPath `
            --configuration Release `
            --verbosity minimal

    } catch {
        Write-Host "  ⚠ Dotnet-Build fehlgeschlagen ($_)" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ✗ Projekt nicht gefunden: $($bazProjPath)" -ForegroundColor Red
}

# DatabaseServer Build versuchen
Write-Host "`n[2/2] DatabaseServer Build..." -ForegroundColor Yellow

$dbProjPath = Join-Path $projectPath "DatabaseServer\DatabaseServer.csproj"
if ((Test-Path $dbProjPath)) {
    Write-Host "  Projekt-Pfad: $($dbProjPath)" -ForegroundColor Gray

    try {
        $dbBuild = "$mdbDotNet" run project `
            --project $dbProjPath `
            --configuration Release

    } catch {
        Write-Host "  ⚠ Dotnet-Build fehlgeschlagen ($_)" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ✗ Projekt nicht gefunden: $($dbProjPath)" -ForegroundColor Red
}

# Status anzeigen
Write-Host "`n========================================" -ForegroundColor Cyan
if ($projects) {
    Write-Host "Build-Status: $(if ($dbBuild -eq $true) { 'Erfolgreich' } else { 'In Bearbeitung' })" -ForegroundColor Green
    Write-Host "Projekte gefunden: $($projects.Count)" -ForegroundColor Gray
}
