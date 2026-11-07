using System;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using Plugin.Minigames.Configuration;
using WingsEmu.Game;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Minigames.Managers
{
    public interface IMeteoriteGameManager
    {
        Task JoinGame(IClientSession session);
        bool IsGameRunning { get; }
    }

    public class MeteoriteGameManager : IMeteoriteGameManager
    {

        private readonly IMapManager _mapManager;
        private readonly MinigameConfiguration _configuration;
        private readonly IMonsterEntityFactory _monsterEntityFactory;
        private readonly INpcMonsterManager _npcMonsterManager;
        private readonly ICharacterAlgorithm _characterAlgorithm;
        private readonly IRandomGenerator _randomGenerator;

        private Guid _currentMapInstanceId;
        private bool _isGameRunning;
        private readonly object _lock = new();

        public bool IsGameRunning => _isGameRunning;

        public MeteoriteGameManager(
            IMapManager mapManager,
            MinigameConfiguration configuration,
            IMonsterEntityFactory monsterEntityFactory,
            INpcMonsterManager npcMonsterManager,
            ICharacterAlgorithm characterAlgorithm,
            IRandomGenerator randomGenerator)
        {
            _mapManager = mapManager;
            _configuration = configuration;
            _monsterEntityFactory = monsterEntityFactory;
            _npcMonsterManager = npcMonsterManager;
            _characterAlgorithm = characterAlgorithm;
            _randomGenerator = randomGenerator;
        }

        public async Task JoinGame(IClientSession session)
        {
            lock (_lock)
            {
                if (_isGameRunning)
                {
                    session.SendChatMessage("Meteorite Game is already in progress!", ChatMessageColorType.Yellow);
                    return;
                }
            }

            Log.Info($"[METEORITE] Player {session.PlayerEntity.Name} joining game");

            // Create or get map instance
            IMapInstance map;
            lock (_lock)
            {
                if (_currentMapInstanceId == Guid.Empty || _mapManager.GetMapInstance(_currentMapInstanceId) == null)
                {
                    map = _mapManager.GenerateMapInstanceByMapId(_configuration.MeteoriteDodgeMapId, MapInstanceType.EventGameInstance);
                    if (map == null)
                    {
                        Log.Warn($"[METEORITE] Failed to create map instance for MapId {_configuration.MeteoriteDodgeMapId}");
                        session.SendChatMessage("Failed to create game instance.", ChatMessageColorType.Red);
                        return;
                    }
                    _currentMapInstanceId = map.Id;
                }
                else
                {
                    map = _mapManager.GetMapInstance(_currentMapInstanceId);
                }
            }

            // Teleport player
            await _mapManager.TeleportOnRandomPlaceInMapAsync(session, map);

            // Initialize player for the game
            InitializePlayer(session);

            // Start game loop if not already running
            lock (_lock)
            {
                if (!_isGameRunning)
                {
                    _isGameRunning = true;
                    _ = Task.Run(() => RunGameLoop(map));
                }
            }
        }

        private void InitializePlayer(IClientSession session)
        {
            var player = session.PlayerEntity;

            // Remove vehicle if equipped
            if (player.IsOnVehicle)
            {
                session.EmitEvent(new RemoveVehicleEvent());
            }

            // Set game morph and speed
            player.Speed = (byte)_configuration.MeteoriteSpeed;
            player.IsOnVehicle = true;
            player.IsCustomSpeed = true;
            player.Morph = _configuration.MeteoriteMorphId;
            player.MorphUpgrade = 0;
            player.MorphUpgrade2 = 0;

            session.RefreshLevel(_characterAlgorithm);
            session.SendCondPacket();
            session.BroadcastCMode();

            Log.Info($"[METEORITE] Player {player.Name} initialized (Morph: {_configuration.MeteoriteMorphId}, Speed: {_configuration.MeteoriteSpeed})");
        }

        private async Task RunGameLoop(IMapInstance map)
        {
            Log.Info("[METEORITE] Game loop starting");

            // Countdown
            for (int i = 10; i > 0; i--)
            {
                map.Broadcast($"msg 0 {i} seconds until Meteorite Game starts!");
                await Task.Delay(1000);
            }

            map.Broadcast("msg 0 Meteorite Game has started! Dodge the circles!");

            int round = 0;
            int maxRounds = _configuration.MeteoriteRounds;

            while (map.Sessions.Any() && round < maxRounds)
            {
                await RunRound(map, round);
                round++;

                if (!map.Sessions.Any()) break;

                // Drop rewards between rounds
                DropRoundRewards(map, round);
                await Task.Delay(5000);
            }

            // End game
            EndGame(map);
        }

        private async Task RunRound(IMapInstance map, int roundNumber)
        {
            Log.Info($"[METEORITE] Starting round {roundNumber + 1}");
            map.Broadcast($"msg 0 Round {roundNumber + 1} starting!");

            // Number of circles increases each round
            int circleCount = 120 + (60 * roundNumber);
            int delayBetweenSpawns = Math.Max(60000 / circleCount, 100);

            for (int i = 0; i < circleCount && map.Sessions.Any(); i++)
            {
                SpawnCircle(map, roundNumber);
                await Task.Delay(delayBetweenSpawns);
            }

            map.Broadcast($"msg 0 Round {roundNumber + 1} complete!");
            Log.Info($"[METEORITE] Round {roundNumber + 1} complete");
        }

        private void SpawnCircle(IMapInstance map, int round)
        {
            // Get random position on map
            Position pos = map.GetRandomPosition();

            // Create circle monster (visual indicator)
            var monster = _monsterEntityFactory.CreateMonster(_configuration.CircleMonsterVNum, map, new MonsterEntityBuilder
            {
                PositionX = pos.X,
                PositionY = pos.Y,
                IsHostile = false,
                IsWalkingAround = false,
                IsRespawningOnDeath = false
            });

            if (monster != null)
            {
                map.AddMonster(monster);
                map.Broadcast(monster.GenerateIn());

                // Show warning effect
                map.Broadcast($"eff 3 {monster.Id} {_configuration.CircleEffectId}");

                // Schedule damage after configured delay
                _ = Task.Delay(_configuration.CircleDamageDelayMs).ContinueWith(_ =>
                {
                    try
                    {
                        // Deal damage to players in range
                        var playersInRange = map.GetCharactersInRange(pos, 2).ToList();
                        foreach (var player in playersInRange)
                        {
                            // Kill player
                            player.IsCustomSpeed = false;
                            player.Session?.EmitEvent(new RemoveVehicleEvent());

                            // Apply massive damage
                            player.Hp = 0;
                            player.Session?.RefreshStat();
                            player.Session?.SendCondPacket();

                            // Broadcast death effect
                            map.Broadcast($"su 3 {monster.Id} 1 {player.Id} 1220 220 0 4983 {pos.X} {pos.Y} 1 0 65535 0 0");

                            Log.Info($"[METEORITE] Player {player.Name} hit by circle at ({pos.X},{pos.Y})");
                        }

                        // Remove the circle monster
                        map.DespawnMonster(monster);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"[METEORITE] Error processing circle damage: {ex.Message}");
                    }
                });
            }
        }

        private void DropRoundRewards(IMapInstance map, int roundCompleted)
        {
            Log.Info($"[METEORITE] Dropping rewards for round {roundCompleted}");

            int multiplier = Math.Min(roundCompleted, 10);

            // For each surviving player, give direct rewards
            foreach (var session in map.Sessions.ToList())
            {
                // Gold reward
                session.PlayerEntity.Gold += _configuration.GoldPerRound * multiplier;
                session.RefreshGold();
                session.SendChatMessage($"Round {roundCompleted} complete! +{_configuration.GoldPerRound * multiplier} gold", ChatMessageColorType.Green);
            }
        }

        private void EndGame(IMapInstance map)
        {
            Log.Info("[METEORITE] Game ending");

            lock (_lock)
            {
                _isGameRunning = false;
                _currentMapInstanceId = Guid.Empty;
            }

            // Give final rewards to survivors
            foreach (var session in map.Sessions.ToList())
            {
                var player = session.PlayerEntity;

                // Reset player state
                player.IsCustomSpeed = false;
                session.EmitEvent(new RemoveVehicleEvent());
                session.RefreshLevel(_characterAlgorithm);
                session.SendCondPacket();
                session.BroadcastCMode();

                // Final reward
                player.Gold += _configuration.SurvivalBonusGold;
                session.RefreshGold();
                session.SendChatMessage($"Congratulations! You survived the Meteorite Game! +{_configuration.SurvivalBonusGold} gold", ChatMessageColorType.Green);

                // Teleport back to miniland
                session.ChangeMap(player.Miniland.Id, 10, 10);
            }

            // Cleanup map instance
            _mapManager.RemoveMapInstance(map.Id);
            Log.Info("[METEORITE] Game ended and cleaned up");
        }
    }
}
