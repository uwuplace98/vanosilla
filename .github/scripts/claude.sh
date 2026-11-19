#!/usr/bin/env bash
# Usage: claude.sh SYSTEM_FILE USER_FILE [MAX_TOKENS]  -> Antwort auf stdout (leer, wenn Key fehlt)
set -uo pipefail
SYS="$1"; USR="$2"; MAXT="${3:-4096}"; MODEL="${CLAUDE_MODEL:-claude-sonnet-5-5}"
if [ -z "${ANTHROPIC_API_KEY:-}" ]; then
  echo "::warning::ANTHROPIC_API_KEY fehlt (Settings > Secrets and variables > Actions). KI-Schritt uebersprungen." >&2
  exit 0
fi
REQ=$(jq -n --rawfile s "$SYS" --rawfile u "$USR" --arg m "$MODEL" --argjson t "$MAXT" \
  '{model:$m,max_tokens:$t,system:$s,messages:[{role:"user",content:$u}]}')
for i in 1 2 3; do
  RES=$(printf '%s' "$REQ" | curl -sS --max-time 240 https://api.anthropic.com/v1/messages \
    -H "x-api-key: $ANTHROPIC_API_KEY" -H "anthropic-version: 2023-06-01" -H "content-type: application/json" \
    --data-binary @- 2>&1)
  TXT=$(printf '%s' "$RES" | jq -r '.content[0].text // empty' 2>/dev/null)
  if [ -n "$TXT" ]; then printf '%s\n' "$TXT"; exit 0; fi
  echo "::warning::Claude-Aufruf $i fehlgeschlagen: $(printf '%s' "$RES" | head -c 300)" >&2
  sleep $((i * 6))
done
exit 0
