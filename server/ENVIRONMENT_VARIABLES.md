# WingsEmu Environment Variables

Complete reference for all environment variables used by the WingsEmu server.

---

## Database (PostgreSQL - Game)

| Variable | Default | Description |
|----------|---------|-------------|
| `DATABASE_IP` | `0.tcp.ngrok.io` | Database server IP |
| `DATABASE_PORT` | `13192` | Database port |
| `DATABASE_USER` | `postgres` | Database username |
| `DATABASE_PASSWORD` | `test123` | Database password |
| `DATABASE_NAME` | `game` | Database name |
| `DATABASE_WRITE_BUFFER_SIZE` | `8192` | Write buffer size |
| `DATABASE_READ_BUFFER_SIZE` | `8192` | Read buffer size |
| `DATABASE_ERROR_DETAIL` | `true` | Include error details |

## Database (PostgreSQL - Phoenix)

| Variable | Default | Description |
|----------|---------|-------------|
| `POSTGRES_DATABASE_IP` | `localhost` | Database server IP |
| `POSTGRES_DATABASE_PORT` | `5432` | Database port |
| `POSTGRES_DATABASE_USER` | `postgres` | Database username |
| `POSTGRES_DATABASE_PASSWORD` | `postgres` | Database password |
| `POSTGRES_DATABASE_NAME` | `postgres` | Database name |

---

## Redis

| Variable | Default | Description |
|----------|---------|-------------|
| `REDIS_IP` | `localhost` | Redis server IP |
| `REDIS_PORT` | `6379` | Redis port |
| `REDIS_PASSWORD` | *(none)* | Redis password |

---

## MongoDB (Logs)

| Variable | Default | Description |
|----------|---------|-------------|
| `WINGSEMU_MONGO_HOST` | `localhost` | MongoDB host |
| `WINGSEMU_MONGO_PORT` | `27017` | MongoDB port |
| `WINGSEMU_MONGO_DB` | `wingsemu_logs` | MongoDB database name |
| `WINGSEMU_MONGO_USERNAME` | `root` | MongoDB username |
| `WINGSEMU_MONGO_PWD` | `root` | MongoDB password |

---

## MQTT Broker

| Variable | Default | Description |
|----------|---------|-------------|
| `MQTT_BROKER_ADDRESS` | `localhost` | MQTT broker address |
| `MQTT_BROKER_PORT` | `1883` | MQTT broker port |
| `MQTT_BROKER_CLIENT_NAME` | `client-<GUID>` | MQTT client name |

---

## Master Server

| Variable | Default | Description |
|----------|---------|-------------|
| `MASTER_IP` | `localhost` | Master server IP |
| `MASTER_PORT` | `20500` | Master server port |
| `MASTER_SSL_PATH` | *(none)* | Path to SSL certificate |

---

## Game Server

| Variable | Default | Description |
|----------|---------|-------------|
| `GAME_SERVER_IP` | `127.0.0.1` | Game server public IP |
| `GAME_SERVER_PORT` | `8000` | Game server port |
| `GAME_SERVER_GROUP` | *(none)* | Server group identifier |
| `GAME_SERVER_SESSION_LIMIT` | *(none)* | Max sessions allowed |
| `GAME_SERVER_CHANNEL_ID` | *(none)* | Channel ID |
| `GAME_SERVER_CHANNEL_TYPE` | *(none)* | Channel type |
| `GAME_SERVER_AUTHORITY` | *(none)* | Server authority level |
| `SERVER_PORT` | `4000` | Login server port |

---

## gRPC Service IPs

| Variable | Default | Description |
|----------|---------|-------------|
| `BAZAAR_SERVER_IP` | `localhost` | Bazaar server IP |
| `RELATION_SERVER_IP` | `localhost` | Relation server IP |
| `FAMILY_SERVER_IP` | `localhost` | Family server IP |
| `MAIL_SERVER_IP` | `localhost` | Mail server IP |
| `DB_SERVER_IP` | `localhost` | Database server IP |
| `TRANSLATIONS_SERVER_IP` | `localhost` | Translation server IP |

---

## gRPC Service Ports

| Variable | Default | Description |
|----------|---------|-------------|
| `BAZAAR_SERVER_PORT` | `25555` | Bazaar server port |
| `RELATION_SERVER_PORT` | `21111` | Relation server port |
| `FAMILY_SERVER_PORT` | `26666` | Family server port |
| `MAIL_SERVER_PORT` | `27777` | Mail server port |
| `DATABASE_SERVER_PORT` | `29999` | Database server port |
| `DB_SERVER_PORT` | `29999` | Database server port (client) |
| `TRANSLATIONS_SERVER_PORT` | `23232` | Translation server port (client) |
| `TRANSLATION_SERVER_PORT` | `23232` | Translation server port |
| `LOGS_SERVER_PORT` | `29888` | Logs server port |

---

## Paths

| Variable | Default | Description |
|----------|---------|-------------|
| `WINGSEMU_RESOURCE_PATH` | `../client-files` | Path to client files |
| `TRANSLATIONS_PATH` | `../server-translations` | Path to translations |
| `CONFIG_PATH` | `../server-files/` | Path to server config files |
| `PLUGINS_PATH` | *(none)* | Path to plugins

---

## Game Systems (Intervals/Timers)

| Variable | Default | Description |
|----------|---------|-------------|
| `CHARACTER_SAVE_SYSTEM_INTERVAL_SECONDS` | `15` | Character save interval |
| `BAZAAR_MANAGER_TTL_MINUTES` | `30` | Bazaar cache lifetime |
| `MAIL_MANAGER_REFRESH_DELAY` | `5` | Mail refresh delay (seconds) |
| `MAIL_SERVER_SAVE_INTERVAL_SECONDS` | `10` | Mail server save interval |
| `MAIL_SERVER_CACHE_TTL_MINUTES` | `15` | Mail server cache TTL |
| `WINGSEMU_TICK_IDLE_DELAY` | `10` | Map idle delay (seconds) |
| `FAMILY_ACHIEVEMENT_REFRESH_IN_SECONDS` | `5` | Family achievement refresh |
| `SERVICE_STATUS_REFRESH_IN_SECONDS` | `3` | Service status refresh |
| `SERVICE_STATUS_EXPIRATION_IN_SECONDS` | `5` | Service status expiration |

---

## Tick System

| Variable | Default | Description |
|----------|---------|-------------|
| `WINGSEMU_TICK_RATE` | `20` | Game tick rate |
| `WINGSEMU_TICK_WORKERS` | `2` | Number of tick workers |
| `WINGSEMU_TICK_SYSTEM_TYPE` | `dispatch_workers` | Tick system type |

---

## Health Check

| Variable | Default | Description |
|----------|---------|-------------|
| `WINGSEMU_HEALTHCHECK_SERVICENAME` | `wingsemu-service-<GUID>` | Health check service name |
| `WINGSEMU_HEALTHCHECK_INTERVAL_SECONDS` | `3` | Health check interval |

---

## Feature Toggles

| Variable | Default | Description |
|----------|---------|-------------|
| `ACT4_SHIP_ACTIVATED` | `true` | Enable Act4 ship system |
| `WORLD_PULSE_SYSTEM` | `true` | Enable world pulse system |

---

## Authentication

| Variable | Default | Description |
|----------|---------|-------------|
| `JWT_PRIVATE_KEY` | *(none)* | JWT signing private key |

---

## Logging

| Variable | Default | Description |
|----------|---------|-------------|
| `PHOENIX_LOG_LEVEL` | *(varies)* | Log level (Debug, Info, etc.) |

---

## Discord Integration

| Variable | Default | Description |
|----------|---------|-------------|
| `WINGSEMU_DISCORD_WEBHOOK_URL` | *(none)* | Discord webhook URL for notifications |
