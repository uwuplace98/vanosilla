using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums.Chat;
using Plugin.Icebreaker.Configuration;
using LocalIcebreakerConfig = Plugin.Icebreaker.Configuration.IcebreakerConfiguration;

namespace Plugin.Icebreaker.Event.Icebreaker
{
    public class IcebreakerManager : IIcebreakerManager
    {
        private readonly LocalIcebreakerConfig _config;
        private readonly IGameLanguageService _language;
        private readonly IMapManager _mapManager;
        private readonly IServerManager _serverManager;
        private readonly IReputationConfiguration _reputationConfiguration;
        private readonly IRankingManager _rankingManager;
        private readonly ISessionManager _sessionManager;

        private readonly ConcurrentDictionary<int, IcebreakerInstance> _instances = new();
        private bool _isWaitingForPlayers;

        public bool IsWaitingForPlayers => _isWaitingForPlayers;

        public IcebreakerManager(
            LocalIcebreakerConfig config,
            IGameLanguageService language,
            IMapManager mapManager,
            IReputationConfiguration reputationConfiguration,
            IRankingManager rankingManager,
            IServerManager serverManager,
            ISessionManager sessionManager)
        {
            _config = config;
            _language = language;
            _mapManager = mapManager;
            _reputationConfiguration = reputationConfiguration;
            _rankingManager = rankingManager;
            _serverManager = serverManager;
            _sessionManager = sessionManager;
        }

        public bool StartEvent(int minLevel, int maxLevel)
        {
            Log.Info($"[ICEBREAKER] Starting event for levels {minLevel}-{maxLevel}");

            var channelConfig = _config.Channels.FirstOrDefault(c => c.ChannelId == _serverManager.ChannelId);
            if (channelConfig == null)
            {
                Log.Warn($"[ICEBREAKER] No configuration for channel {_serverManager.ChannelId}");
                return false;
            }

            var instanceConfig = channelConfig.Instances.FirstOrDefault(i => i.MinLevel == minLevel && i.MaxLevel == maxLevel);
            if (instanceConfig == null)
            {
                instanceConfig = channelConfig.Instances.FirstOrDefault(i => minLevel >= i.MinLevel && maxLevel <= i.MaxLevel);
            }

            if (instanceConfig == null)
            {
                Log.Warn($"[ICEBREAKER] No instance configuration for levels {minLevel}-{maxLevel}");
                return false;
            }

            // Create map instance
            var mapInstance = _mapManager.GenerateMapInstanceByMapId(instanceConfig.MapId, MapInstanceType.IceBreakerInstance);
            if (mapInstance == null)
            {
                Log.Warn($"[ICEBREAKER] Failed to create map instance for MapId {instanceConfig.MapId}");
                return false;
            }

            var instance = new IcebreakerInstance(instanceConfig, mapInstance);
            instance.State = IcebreakerState.Lobby;
            instance.StateStartTime = DateTime.UtcNow;

            _instances[instanceConfig.MapId] = instance;
            _isWaitingForPlayers = true;

            // Broadcast join notification to eligible players
            BroadcastEventStart(minLevel, maxLevel);

            Log.Info($"[ICEBREAKER] Event started for levels {minLevel}-{maxLevel}. Lobby duration: {_config.LobbyDurationSeconds}s");
            return true;
        }

        private void BroadcastEventStart(int minLevel, int maxLevel)
        {
            var eligibleSessions = _sessionManager.Sessions
                .Where(s => s.PlayerEntity != null &&
                           s.PlayerEntity.Level >= minLevel &&
                           s.PlayerEntity.Level <= maxLevel)
                .ToList();

            foreach (var session in eligibleSessions)
            {
                // Send qnaml dialog to join
                string dialogPacket = $"qnaml 2 #guri^501 Icebreaker event has started! Would you like to join for {_config.JoinGoldCost} gold?";
                session.SendPacket(dialogPacket);
            }

            Log.Info($"[ICEBREAKER] Broadcast sent to {eligibleSessions.Count} eligible players");
        }

        public bool IsFrozen(long characterId)
        {
            return _instances.Values.Any(i => i.FrozenPlayers.ContainsKey(characterId));
        }

        public async Task Join(IPlayerEntity player)
        {
            if (!_isWaitingForPlayers)
            {
                player.Session.SendMsg("No Icebreaker event is currently active.", MsgMessageType.Middle);
                return;
            }

            if (player.Gold < _config.JoinGoldCost)
            {
                player.Session.SendMsg(_language.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, player.Session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            // Find suitable instance
            var instance = _instances.Values.FirstOrDefault(i =>
                player.Level >= i.Configuration.MinLevel &&
                player.Level <= i.Configuration.MaxLevel &&
                i.State == IcebreakerState.Lobby);

            if (instance == null)
            {
                player.Session.SendMsg("No suitable Icebreaker lobby found for your level.", MsgMessageType.Middle);
                return;
            }

            player.Gold -= _config.JoinGoldCost;
            player.Session.RefreshGold();

            // Store return position before teleporting
            var currentMap = player.Session.CurrentMapInstance;
            instance.ReturnPositions[player.Id] = (currentMap.Id, player.PositionX, player.PositionY);

            // Teleport
            player.Session.ChangeMap(instance.MapInstance.Id, instance.Configuration.PortalX, instance.Configuration.PortalY);

            // Add to teams
            lock (instance.Lock)
            {
                instance.Teams.Add(new HashSet<long> { player.Id });
            }

            player.Session.SendMsg($"You joined the Icebreaker! Waiting for battle to start...", MsgMessageType.Middle);
            Log.Info($"[ICEBREAKER] Player {player.Name} joined the event");

            await Task.CompletedTask;
        }

        public void Freeze(IPlayerEntity player)
        {
            var instance = _instances.Values.FirstOrDefault(i => i.MapInstance.Id == player.MapInstance.Id);
            if (instance == null) return;

            if (instance.FrozenPlayers.TryAdd(player.Id, 0))
            {
                player.Hp = player.MaxHp;
                player.Mp = player.MaxMp;
                player.Session.RefreshStat();

                player.Session.SendCondPacket();
                player.Session.BroadcastEffect(35);
                player.Session.SendMsg("You are frozen! Wait for a teammate to unfreeze you.", MsgMessageType.Middle);
            }
        }

        public async Task Unfreeze(IPlayerEntity target, IPlayerEntity unfreezer)
        {
            var instance = _instances.Values.FirstOrDefault(i => i.MapInstance.Id == target.MapInstance.Id);
            if (instance == null) return;

            if (!instance.FrozenPlayers.TryRemove(target.Id, out _))
            {
                return;
            }

            target.Session.SendCondPacket();
            target.Session.SendMsg($"You were unfrozen by {unfreezer.Name}!", MsgMessageType.Middle);
            target.Session.BroadcastEffect(35);

            lock (instance.Lock)
            {
                var unfreezerTeam = instance.Teams.FirstOrDefault(t => t.Contains(unfreezer.Id));
                var targetTeam = instance.Teams.FirstOrDefault(t => t.Contains(target.Id));

                if (unfreezerTeam != null && targetTeam != null && unfreezerTeam != targetTeam)
                {
                    foreach (var memberId in targetTeam)
                    {
                        unfreezerTeam.Add(memberId);
                    }
                    instance.Teams.Remove(targetTeam);
                }
                else if (unfreezerTeam != null && targetTeam == null)
                {
                    unfreezerTeam.Add(target.Id);
                }
            }

            await Task.CompletedTask;
        }

        public void RemovePlayer(long characterId)
        {
            foreach (var instance in _instances.Values)
            {
                if (instance.FrozenPlayers.TryRemove(characterId, out _))
                {
                    lock (instance.Lock)
                    {
                        instance.Teams.RemoveAll(t => t.Contains(characterId));
                    }
                }
            }
        }

        public void Process()
        {
            foreach (var instance in _instances.Values)
            {
                ProcessInstance(instance);
            }
        }

        private void ProcessInstance(IcebreakerInstance instance)
        {
            switch (instance.State)
            {
                case IcebreakerState.Lobby:
                    if (DateTime.UtcNow - instance.StateStartTime >= TimeSpan.FromSeconds(_config.LobbyDurationSeconds))
                    {
                        StartBattle(instance);
                    }
                    break;
                case IcebreakerState.Battle:
                    if (CheckWinCondition(instance))
                    {
                        FinishInstance(instance);
                    }
                    else if (DateTime.UtcNow - instance.StateStartTime >= TimeSpan.FromSeconds(_config.RoundDurationSeconds))
                    {
                        FinishInstance(instance, true); // Timeout
                    }
                    break;
                case IcebreakerState.Finished:
                    // Cleanup after a delay
                    if (DateTime.UtcNow - instance.StateStartTime >= TimeSpan.FromSeconds(30))
                    {
                        _instances.TryRemove(instance.Configuration.MapId, out _);
                        _isWaitingForPlayers = false;
                    }
                    break;
            }
        }

        private void StartBattle(IcebreakerInstance instance)
        {
            instance.State = IcebreakerState.Battle;
            instance.StateStartTime = DateTime.UtcNow;
            _isWaitingForPlayers = false;

            // Broadcast start message
            foreach (var team in instance.Teams)
            {
                foreach (var playerId in team)
                {
                    var player = instance.MapInstance.GetCharacterById(playerId);
                    player?.Session.SendMsg("Icebreaker battle has started! Freeze your opponents!", MsgMessageType.Middle);
                }
            }

            Log.Info($"[ICEBREAKER] Battle started with {instance.Teams.Count} teams");
        }

        private bool CheckWinCondition(IcebreakerInstance instance)
        {
            lock (instance.Lock)
            {
                return instance.Teams.Count <= 1;
            }
        }

        private void FinishInstance(IcebreakerInstance instance, bool timeout = false)
        {
            instance.State = IcebreakerState.Finished;
            instance.StateStartTime = DateTime.UtcNow;

            lock (instance.Lock)
            {
                Log.Info($"[ICEBREAKER] FinishInstance called. Teams count: {instance.Teams.Count}, timeout: {timeout}");
                
                if (instance.Teams.Count >= 1 && !timeout)
                {
                    var winningTeam = instance.Teams.First();
                    Log.Info($"[ICEBREAKER] Awarding rewards to winning team of {winningTeam.Count} players");
                    
                    foreach (var playerId in winningTeam)
                    {
                        // Try map first, then session manager as fallback
                        var player = instance.MapInstance.GetCharacterById(playerId);
                        if (player == null)
                        {
                            // Fallback to session manager
                            var session = _sessionManager.Sessions.FirstOrDefault(s => s.PlayerEntity?.Id == playerId);
                            player = session?.PlayerEntity;
                        }
                        
                        if (player != null)
                        {
                            player.Gold += _config.WinGoldReward;
                            player.Session.RefreshGold();
                            player.Session.RefreshReputation(_reputationConfiguration, _rankingManager.TopReputation);
                            player.Session.SendMsg($"Congratulations! You won {_config.WinGoldReward} gold!", MsgMessageType.Middle);
                            Log.Info($"[ICEBREAKER] Rewarded player {player.Name} with {_config.WinGoldReward} gold");
                        }
                        else
                        {
                            Log.Warn($"[ICEBREAKER] Could not find player with ID {playerId} for rewards");
                        }
                    }
                    Log.Info($"[ICEBREAKER] Team of {winningTeam.Count} won the event");
                }
                else if (timeout)
                {
                    Log.Info("[ICEBREAKER] Event ended due to timeout");
                }

                // Teleport everyone out to their original positions
                foreach (var team in instance.Teams)
                {
                    foreach (var playerId in team)
                    {
                        var player = instance.MapInstance.GetCharacterById(playerId);
                        if (player != null)
                        {
                            TeleportToReturn(player, instance);
                        }
                    }
                }

                // Also unfreeze and teleport frozen players
                foreach (var playerId in instance.FrozenPlayers.Keys)
                {
                    var player = instance.MapInstance.GetCharacterById(playerId);
                    if (player != null)
                    {
                        TeleportToReturn(player, instance);
                    }
                }

                instance.Teams.Clear();
                instance.FrozenPlayers.Clear();
                instance.ReturnPositions.Clear();
            }
        }

        private void TeleportToReturn(IPlayerEntity player, IcebreakerInstance instance)
        {
            if (instance.ReturnPositions.TryGetValue(player.Id, out var returnPos))
            {
                player.Session.ChangeMap(returnPos.MapId, returnPos.X, returnPos.Y);
            }
            else
            {
                // Fallback to spawn point
                player.Session.ChangeMap(player.Miniland.Id, 10, 10);
            }
        }

        public bool ForceStartBattle()
        {
            // Find any instance in lobby state and force start it
            var lobbyInstance = _instances.Values.FirstOrDefault(i => i.State == IcebreakerState.Lobby);
            if (lobbyInstance == null)
            {
                return false;
            }

            Log.Info("[ICEBREAKER] Force starting battle (admin command)");
            StartBattle(lobbyInstance);
            return true;
        }
    }
}
