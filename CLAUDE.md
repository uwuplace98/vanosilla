# Vanosilla - WICHTIGE FAKTEN

## Startbefehl
```powershell
# Alle Server laden in Visual Studio 2022 oder via MSBuild:
dotnet restore
dotnet build

# Einzelner Server starten (Beispiel):
dotnet run --project BazaarServer.Core
```

## Testbefehl
```powershell
# Unit-Tests für ein Projekt:
dotnet test --project <Projektname>.Core.Tests

# Integrationstests:
dotnet test --filter "CompleteIntegration"
```

## Struktur
- `server/` → 140+ .NET-Projekte, gRPC Backend (Bazaar, Login, Family, DiscordNotifier)
- `server-translations/` → YAML Dialoge (720+ Einträge, 12 Sprachen)
- `client-files/` → Binäre .dat Daten, 696+ Maps

## Regeln für Phase 2
1. **Kein git push** – Alle Commits lokal (`git switch ki-arbeit`)
2. **Nie Dateien löschen** – Keine Abhängigkeiten ersetzen
3. **Keine zerstörerlichen Befehle** – Kein `git reset --hard`, `rm -rf`
4. **Max 3 Reparaturversuche pro Schritt** – Sonst zurücksetzen
5. **Einmaliger Fokus** – Nur EINER Schritt gleichzeitig bearbeiten
6. **Verifizieren vor Commit** – Testen/Starten, dann `git add && commit`

## Offene Punkte aus Analyse
- README-Dokumentation unvollständig
- gRPC Integration zwischen Servern nicht vollständig getestete
- Datenbank-Schemas teilweise noch nicht definiert
- CI/CD Pipeline fehlt
