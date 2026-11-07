using System;
using System.Collections.Concurrent;
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
    public interface ISheepFarmManager
    {
        Task JoinGame(IClientSession session);
        bool IsGameRunning { get; }
    }

    public class SheepFarmManager : ISheepFarmManager
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
        private readonly ConcurrentDictionary<long, int> _playerScores = new();

        public bool IsGameRunning => _isGameRunning;

        public SheepFarmManager(
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
                    session.SendChatMessage("Sheep Farm is already in progress!", ChatMessageColorType.Yellow);
                    return;
                }
            }

            Log.Info($"[SHEEP_FARM] Player {session.PlayerEntity.Name} joining game");

            // Create or get map instance
            IMapInstance map;
            lock (_lock)
            {
                if (_currentMapInstanceId == Guid.Empty || _mapManager.GetMapInstance(_currentMapInstanceId) == null)
                {
                    map = _mapManager.GenerateMapInstanceByMapId(_configuration.SheepFarmMapId, MapInstanceType.EventGameInstance);
                    if (map == null)
                    {
                        Log.Warn($"[SHEEP_FARM] Failed to create map instance for MapId {_configuration.SheepFarmMapId}");
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

            // Initialize player score
            _playerScores[session.PlayerEntity.Id] = 0;

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

            // Set sheep morph and speed
            player.Speed = (byte)_configuration.SheepSpeed;
            player.IsOnVehicle = true;
            player.IsCustomSpeed = true;
            player.Morph = _configuration.SheepMorphId;
            player.MorphUpgrade = 0;
            player.MorphUpgrade2 = 0;

            session.RefreshLevel(_characterAlgorithm);
            session.SendCondPacket();
            session.BroadcastCMode();

            // Show transformation effect
            session.BroadcastEffect(4323, new RangeBroadcast(player.PositionX, player.PositionY));

            Log.Info($"[SHEEP_FARM] Player {player.Name} initialized (Morph: {_configuration.SheepMorphId}, Speed: {_configuration.SheepSpeed})");
        }

        private async Task RunGameLoop(IMapInstance map)
        {
            Log.Info("[SHEEP_FARM] Game loop starting");

            // Countdown (30 seconds lobby)
            for (int i = 30; i > 0; i--)
            {
                if (i == 30 || i <= 5)
                {
                    map.Broadcast($"msg 0 {i} seconds until Sheep Farm starts!");
                }
                await Task.Delay(1000);
            }

            map.Broadcast("msg 0 Sheep Farm has started! Collect the sheep!");
            map.Broadcast("srlst 3"); // Start racing list
            map.Broadcast("sh_o"); // Sheep out signal

            // Game duration: 5 minutes (300 seconds)
            DateTime endTime = DateTime.UtcNow.AddMinutes(_configuration.SheepGameDurationMinutes);
            int round = 0;

            while (DateTime.UtcNow < endTime && map.Sessions.Any())
            {
                await RunRound(map, round);
                round++;
            }

            // End game
            EndGame(map);
        }

        private async Task RunRound(IMapInstance map, int roundNumber)
        {
            Log.Info($"[SHEEP_FARM] Starting round {roundNumber + 1}");

            // Number of sheep increases each round
            int sheepCount = 120 + (60 * roundNumber);
            int delayBetweenSpawns = Math.Max(60000 / sheepCount, 100);

            for (int i = 0; i < sheepCount && map.Sessions.Any(); i++)
            {
                SpawnSheep(map);
                await Task.Delay(delayBetweenSpawns);
            }

            // Brief pause between rounds
            await Task.Delay(7000);
        }

        private void SpawnSheep(IMapInstance map)
        {
            // Get random position on map
            Position pos = map.GetRandomPosition();

            // Create sheep monster
            var sheep = _monsterEntityFactory.CreateMonster(_configuration.SheepMonsterVNum, map, new MonsterEntityBuilder
            {
                PositionX = pos.X,
                PositionY = pos.Y,
                IsHostile = false,
                IsWalkingAround = true, // Sheep wander around
                IsRespawningOnDeath = false
            });

            if (sheep != null)
            {
                map.AddMonster(sheep);
                map.Broadcast(sheep.GenerateIn());
            }
        }

        private void EndGame(IMapInstance map)
        {
            Log.Info("[SHEEP_FARM] Game ending");

            lock (_lock)
            {
                _isGameRunning = false;
                _currentMapInstanceId = Guid.Empty;
            }

            // Calculate winners and give rewards
            var sortedScores = _playerScores.OrderByDescending(x => x.Value).ToList();

            map.Broadcast("msg 0 Sheep Farm has ended!");

            int rank = 1;
            foreach (var (playerId, score) in sortedScores)
            {
                var session = map.Sessions.FirstOrDefault(s => s.PlayerEntity.Id == playerId);
                if (session == null) continue;

                var player = session.PlayerEntity;

                // Reset player state
                player.IsCustomSpeed = false;
                session.EmitEvent(new RemoveVehicleEvent());
                session.RefreshLevel(_characterAlgorithm);
                session.SendCondPacket();
                session.BroadcastCMode();

                // Calculate reward based on rank and score
                int goldReward = CalculateReward(rank, score);
                player.Gold += goldReward;
                session.RefreshGold();

                session.SendChatMessage($"Rank #{rank} - Score: {score} - Reward: {goldReward} gold", ChatMessageColorType.Green);

                // Teleport back to miniland
                session.ChangeMap(player.Miniland.Id, 10, 10);

                rank++;
            }

            // Clear scores
            _playerScores.Clear();

            // Cleanup map instance
            _mapManager.RemoveMapInstance(map.Id);
            Log.Info("[SHEEP_FARM] Game ended and cleaned up");
        }

        private int CalculateReward(int rank, int score)
        {
            int baseReward = score * _configuration.GoldPerSheep;

            // Bonus for top 3
            return rank switch
            {
                1 => baseReward + _configuration.FirstPlaceBonus,
                2 => baseReward + _configuration.SecondPlaceBonus,
                3 => baseReward + _configuration.ThirdPlaceBonus,
                _ => baseReward
            };
        }
    }
}
