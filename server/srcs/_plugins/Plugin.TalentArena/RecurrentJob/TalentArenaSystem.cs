using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.Scheduler;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Arena.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TalentArena.RecurrentJob;

public class TalentArenaSystem : BackgroundService
{
    private static TimeSpan Interval = TimeSpan.FromSeconds(1);
    private readonly ITalentArenaManager _talentArenaManager;
    private readonly ISessionManager _sessionManager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IScheduler _scheduler;

    private long _groupId;
    private DateTime _lastTickSecond;

    public TalentArenaSystem(ITalentArenaManager talentArenaManager, ISessionManager sessionManager, IAsyncEventPipeline asyncEventPipeline, IScheduler scheduler)
    {
        _talentArenaManager = talentArenaManager;
        _sessionManager = sessionManager;
        _asyncEventPipeline = asyncEventPipeline;
        _scheduler = scheduler;
        _lastTickSecond = DateTime.UtcNow;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[TALENT_ARENA_SYSTEM] Start Talent Arena system...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MainProcess();
            }
            catch (Exception e)
            {
                Log.Error("[TALENT_ARENA_SYSTEM] ", e);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task MainProcess()
    {
        DateTime dateNow = DateTime.UtcNow;

        if (!_talentArenaManager.IsActive)
        {
            return;
        }

        if (!_talentArenaManager.IsRegistrationActive)
        {
            ProcessWarningOpen();
            return;
        }

        await RefreshCheckOrders();

        // Functions that need 500ms of delay
        if (_lastTickSecond.AddMilliseconds(400) > dateNow) // 400 + 100 ms
        {
            return;
        }

        await ProcessInstancesLife();

        // Function That need 1 second of delay
        if (_lastTickSecond.AddMilliseconds(900) > dateNow) // 900 + 100 ms
        {
            return;
        }

        _lastTickSecond = DateTime.UtcNow;

        await TryMatchmaking();
        await TeleportToFightZone(dateNow);
    }

    private void ProcessWarningOpen()
    {
        if (_talentArenaManager.TalentArenaProcessTime is null)
        {
            return;
        }

        _talentArenaManager.IsRegistrationActive = true;
        Interval = TimeSpan.FromMilliseconds(100);

        _sessionManager.Broadcast(x => x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.TALENT_ARENA_SHOUTMESSAGE_STARTED), MsgMessageType.Middle));
    }

    private async Task TryMatchmaking()
    {
        List<IClientSession> membersInLobby = _talentArenaManager.GetRegisteredMembers(ArenaEventType.TalentArena);

        if (membersInLobby == null)
        {
            return;
        }

        foreach (IClientSession session in membersInLobby)
        {
            ITalentArenaComponent member = session.PlayerEntity.TalentArenaComponent;
            member.Time--;
            if (member.GroupId != -1)
            {
                continue;
            }

            // o.Session.SendPacket(o.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SEARCH_ARENA_TEAM"), 10));
            var members = membersInLobby.Where(e => e != session && e.PlayerEntity.TalentArenaComponent.StateType == ArenaMemberStateType.TeamSearch &&
                e.PlayerEntity.Level <= session.PlayerEntity.Level + 5 &&
                e.PlayerEntity.Level >= session.PlayerEntity.Level - 5).ToList();
            IClientSession otherMember = members.FirstOrDefault();

            if (otherMember == null)
            {
                continue;
            }

            if (otherMember.PlayerEntity.TalentArenaComponent.GroupId == -1)
            {
                _groupId++;
                otherMember.PlayerEntity.TalentArenaComponent.GroupId = _groupId;
            }

            member.GroupId = otherMember.PlayerEntity.TalentArenaComponent.GroupId;

            if (membersInLobby.Count(g => g.PlayerEntity.TalentArenaComponent.GroupId == member.GroupId) == 2)
            {
                _talentArenaManager.ChangeGroupMemberState(member.GroupId, ArenaMemberStateType.OpponentSearch);
            }
        }

        List<IClientSession> membersSearchingOpponent =
            _talentArenaManager.GetRegisteredMembers(ArenaEventType.TalentArena, s => s.PlayerEntity.TalentArenaComponent.StateType == ArenaMemberStateType.OpponentSearch);

        foreach (IClientSession session in membersSearchingOpponent)
        {
            ITalentArenaComponent member = session.PlayerEntity.TalentArenaComponent;
            if (membersSearchingOpponent.Count(g => g.PlayerEntity.TalentArenaComponent.GroupId == member.GroupId) < 2)
            {
                continue;
            }

            if (member.StateType != ArenaMemberStateType.OpponentSearch)
            {
                continue;
            }

            member.Time--;

            IClientSession opponentMember = membersSearchingOpponent.FirstOrDefault(e => e.PlayerEntity.TalentArenaComponent.GroupId != member.GroupId &&
                e.PlayerEntity.Level <= session.PlayerEntity.Level + 5 &&
                e.PlayerEntity.Level >= session.PlayerEntity.Level - 5);
            if (opponentMember == null)
            {
                continue;
            }

            var zenasTeam = membersSearchingOpponent.Where(s => s.PlayerEntity.TalentArenaComponent.GroupId == member.GroupId).ToList();
            var ereniaTeam = membersSearchingOpponent.Where(s => s.PlayerEntity.TalentArenaComponent.GroupId == opponentMember.PlayerEntity.TalentArenaComponent.GroupId).ToList();

            foreach (IClientSession s in zenasTeam)
            {
                s.PlayerEntity.TalentArenaComponent.StateType = ArenaMemberStateType.Lobby;
                s.SendPacket(TalentArenaExtensions.GenerateTaClose());
            }

            foreach (IClientSession s in ereniaTeam)
            {
                s.PlayerEntity.TalentArenaComponent.StateType = ArenaMemberStateType.Lobby;
                s.SendPacket(TalentArenaExtensions.GenerateTaClose());
            }

            _scheduler.Schedule(TimeSpan.FromSeconds(5), async () =>
                await _asyncEventPipeline.ProcessEventAsync(new TalentArenaInstanceStartEvent(zenasTeam, ereniaTeam)));
        }
    }

    private async Task RefreshCheckOrders()
    {
        List<TalentArenaInstance> instances = _talentArenaManager.GetArenaInstances();

        if (instances == null)
        {
            return;
        }

        foreach (TalentArenaInstance instance in instances)
        {
            IMapInstance mapInstance = instance.MapInstance;

            bool resettap = false;

            foreach (MapDesignObject mapObject in mapInstance.MapDesignObjects)
            {
                GameItemInstance iteminstance = mapObject.InventoryItem.ItemInstance;
                var pos = new Position(mapObject.MapX, mapObject.MapY);

                TalentArenaTeam team = mapObject.MapX == 120 ? instance.ZenasTeam : instance.EreniaTeam;
                switch (iteminstance.Design)
                {
                    case >= 4433 and <= 4435:
                        {
                            IPlayerEntity playerEntity = mapInstance.GetCharactersInRange(pos, 0).FirstOrDefault();

                            if (playerEntity == null)
                            {
                                continue;
                            }

                            resettap = true;

                            IClientSession teamMember = instance.ZenasTeam.Members.FirstOrDefault(at => at == playerEntity.Session) ??
                                instance.EreniaTeam.Members.FirstOrDefault(at => at == playerEntity.Session);

                            if (teamMember == null || team.Members.Any(at => at.PlayerEntity.TalentArenaComponent.Order == (iteminstance.Design - 4433)))
                            {
                                continue;
                            }

                            ITalentArenaComponent memberTaComponent = teamMember.PlayerEntity.TalentArenaComponent;
                            if (memberTaComponent.Order != -1)
                            {
                                MapDesignObject obj =
                                    mapInstance.MapDesignObjects.FirstOrDefault(mapObj => mapObj.InventoryItem.ItemInstance.ItemVNum == iteminstance.ItemVNum &&
                                        mapObject.MapX == (memberTaComponent.TeamType == ArenaTeamType.ZENAS ? 120 : 19));
                                if (obj != null)
                                {
                                    obj.InventoryItem.ItemInstance.Design = (short)obj.InventoryItem.ItemInstance.ItemVNum;
                                }
                            }

                            memberTaComponent.Order = (byte)(iteminstance.ItemVNum - 4433);
                            break;
                        }
                    case 4436:
                        {
                            if (!mapInstance.GetCharactersInRange(pos, 0).Any())
                            {
                                resettap = true;
                                IClientSession teamMember = team.Members.FirstOrDefault(s => s.PlayerEntity.TalentArenaComponent.Order == (iteminstance.ItemVNum - 4433));

                                if (teamMember != null)
                                {
                                    teamMember.PlayerEntity.TalentArenaComponent.Order = -1;
                                }
                            }

                            break;
                        }
                }

                if (team.Members.All(at => at.PlayerEntity.TalentArenaComponent.Order != (iteminstance.ItemVNum - 4433)))
                {
                    if (iteminstance.Design != 4436)
                    {
                        return;
                    }

                    iteminstance.Design = (short)iteminstance.ItemVNum;
                    mapInstance.Broadcast(mapObject.GenerateEffectFromDesign(false));
                }
                else if (iteminstance.Design != 4436)
                {
                    iteminstance.Design = 4436;
                    mapInstance.Broadcast(mapObject.GenerateEffectFromDesign(false));
                }

                if (resettap)
                {
                    team.Members.ForEach(arenaUser => { arenaUser.SendPacket(arenaUser.GenerateTaP(2, false)); });
                }
            }
        }
    }

    private async Task TeleportToFightZone(DateTime currentTime)
    {
        List<TalentArenaInstance> instances = _talentArenaManager.GetArenaInstances();

        if (instances == null)
        {
            return;
        }

        foreach (TalentArenaInstance instance in instances)
        {
            DateTime time = instance.EventDateTick + instance.SecondsAmount;

            if (time >= currentTime)
            {
                continue;
            }

            if (instance.IsFighting)
            {
                continue;
            }

            instance.EventDateTick = DateTime.UtcNow;
            instance.IsFighting = true;
            instance.SecondsAmount = TimeSpan.FromSeconds(300);
            instance.Round++;

            await _asyncEventPipeline.ProcessEventAsync(new TalentArenaTeleportToFightZoneEvent(instance, false));
        }
    }

    private async Task ProcessInstancesLife()
    {
        List<TalentArenaInstance> instances = _talentArenaManager.GetArenaInstances();

        if (instances == null)
        {
            return;
        }

        foreach (TalentArenaInstance instance in instances)
        {
            await _asyncEventPipeline.ProcessEventAsync(new TalentArenaProcessInstanceLifeEvent
            {
                Instance = instance
            });
        }
    }
}