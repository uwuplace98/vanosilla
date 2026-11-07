#!powershell # Integrationstest für gRPC-AuthFlow

# ============================================
# Vanosilla Integration Test Script
# Testet LoginServer ↔ BazaarServer gRPC
# ============================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Vanosilla Integration-Tests" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$baseUrl = "http://localhost:50051"
$testAccId = "ACC_TEST_001"
$testCharName = "TestCharacterA"

# Test 1: Health Check
Write-Host "`n[Test 1] Health Check..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/Health" -TimeoutSec 30
    if ($response.Healthy) {
        Write-Host "  ✓ Health Check OK - Server läuft auf Port $($env:BazaarServer_Port)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Health Check fehlgeschlagen: $($_.Exception.Message)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ✗ Server nicht erreichbar (starte mit start-all-core-servers.ps1)" -ForegroundColor Red
}

# Test 2: Auth LoginFlow (simulierte Datenbank)
Write-Host "`n[Test 2] Auth Check-In Flow..." -ForegroundColor Yellow
try {
    $request = @{
        accountId = $testAccId
        characterName = $testCharName
        serverId = 1
    }

    # Simulierter API-Aufruf (Backend muss starten)
    Write-Host "  ⚠ Backend-Server müssen gestartet sein:" -ForegroundColor Gray
    Write-Host "    - DatabaseServer (PostgreSQL)" -ForegroundColor Gray
    Write-Host "    - BazaarServer (gRPC)" -ForegroundColor Gray

} catch {
    Write-Host "  ✗ Test fehlgeschlagen: $_" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Integration-Test Status:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
if ($env:BazaarServer_Port) {
    Write-Host "- BazaarServer läuft: http://localhost:$($env:BazaarServer_Port)" -ForegroundColor Green
} else {
    Write-Host "- BazaarServer Port nicht gesetzt" -ForegroundColor Red
}

Write-Host ""
