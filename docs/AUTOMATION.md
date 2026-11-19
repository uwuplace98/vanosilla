# Vanosilla – GitHub-Automation

Alles laeuft in GitHub Actions. Einzige Voraussetzung: Repo-Secret `ANTHROPIC_API_KEY`
(Settings → Secrets and variables → Actions → New repository secret).

| Workflow | Wann | Was |
|---|---|---|
| CI | Push/PR auf dev, main | Restore, Build, Test |
| AI Review | PR auf dev/main | Code-Review als Kommentar |
| AI Triage | Neues Issue | Labels, Prioritaet, Rueckfragen; startet bei Eignung den Fix |
| AI Fix Agent | Label `ai-fix` / Dispatch | Claude Code fixt, Build+Test, Self-Review, Auto-Merge nach dev |
| CI Failure Analyst | CI/Promote auf dev/main fehlgeschlagen | Analyse, Issue, startet Fix |
| Backlog Sweeper | stuendlich | Triagiert Unlabeltes, startet den naechsten Bug-Fix |
| Promote | taeglich 02:30 | Build+Test auf dev, Changelog, Merge dev → main, Release |
| Daily Health Report | taeglich 05:00 | Statusbericht als Issue |
| Weekly Architecture Wiki | montags | docs/wiki/Architecture.md (+ GitHub-Wiki, falls initialisiert) |
| CodeQL | woechentlich, Push main | Security-Scan |

Grenzen: max. 3 Fix-Versuche pro Issue (danach `needs-human`), ein Fix-Job gleichzeitig,
der Agent aendert nichts unter `.github/` und `client-files/`.
Manuell starten: Actions → Workflow waehlen → Run workflow.
