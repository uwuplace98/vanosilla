using PhoenixLib.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Enum;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Portals;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act4
{
    public class Act4CaligorManager : IAct4CaligorManager
    {
        private readonly Act4CaligorConfiguration _configuration;
        private readonly ISessionManager _sessionManager;
        private readonly IMapManager _mapManager;
        private readonly IPortalFactory _portalFactory;
        private readonly IScheduler _scheduler;
        private readonly IMonsterEntityFactory _entity;
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly IBuffFactory _buffFactory;

        public Act4CaligorManager(Act4CaligorConfiguration configuration, ISessionManager sessionManager, IMapManager mapManager, IPortalFactory portalFactory,
            IScheduler scheduler, IMonsterEntityFactory monsterEntityFactory, IGameItemInstanceFactory gameItemInstanceFactory, IBuffFactory buffFactory)
        {
            _configuration = configuration;
            _sessionManager = sessionManager;
            _mapManager = mapManager;
            _portalFactory = portalFactory;
            _scheduler = scheduler;
            _entity = monsterEntityFactory;
            _gameItemInstanceFactory = gameItemInstanceFactory;
            _buffFactory = buffFactory;
        }

        public DateTime CaligorStart { get; set; }
        public DateTime CaligorEnd { get; set; }
        public bool CaligorActive { get; set; }
        public bool CaligorLocked { get; set; }
        public int AngelDamage { get; set; }
        public int DemonDamage { get; set; }
        public bool FernonMapsActive => FernonMap is not null;

        public void TeleportPlayerToCaligorCamp(IPlayerEntity player)
        {
            short x = 0;
            short y = 0;
            switch (player.Faction)
            {
                case FactionType.Angel:
                    x = 50;
                    y = 172;
                    break;

                case FactionType.Demon:
                    x = 130;
                    y = 172;
                    break;
            }
            player.Session.ChangeMap(CaligorInstance, x, y);
        }

        public void EndCaligorInstance(bool caligorArekilled)
        {
            _sessionManager.Broadcast(x => x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_END), MsgMessageType.Middle));

            foreach (IPortalEntity p in UnknowLandInstance.Portals.Where(s => s.DestinationMapInstance != null && s.DestinationMapInstance.Id == CaligorInstance.Id).ToList())
            {
                p.IsDisabled = true;
                UnknowLandInstance.Broadcast(p.GenerateGp());
                UnknowLandInstance.DeletePortal(p);
            }

            if (caligorArekilled)
            {
                FactionType winningFaction = AngelDamage > DemonDamage ? FactionType.Angel : FactionType.Demon;

                DateTime currentTime = DateTime.UtcNow;

                TimeSpan timeLeft = CaligorEnd - currentTime;

                foreach (IClientSession sess in CaligorInstance.Sessions.ToList())
                {
                    var listItemRewards = new List<GameItemInstance>
                    {
                        _gameItemInstanceFactory.CreateItem(5959),
                    };
                    if (timeLeft.TotalSeconds > 2400)
                    {
                        if (sess.PlayerEntity.Faction == winningFaction)
                        {
                            listItemRewards.Add(_gameItemInstanceFactory.CreateItem(5960));
                            listItemRewards.Add(_gameItemInstanceFactory.CreateItem(30762));
                        }
                        else
                        {
                            listItemRewards.Add(_gameItemInstanceFactory.CreateItem(5961));
                        }
                    }
                    else
                    {
                        if (sess.PlayerEntity.Faction == winningFaction)
                        {
                            listItemRewards.Add(_gameItemInstanceFactory.CreateItem(5961));
                            listItemRewards.Add(_gameItemInstanceFactory.CreateItem(30762));
                        }
                        else
                        {
                            listItemRewards.Add(_gameItemInstanceFactory.CreateItem(5958));
                        }
                    }
                    foreach (GameItemInstance rewards in listItemRewards)
                    {
                        sess.AddNewItemToInventory(rewards, true, ChatMessageColorType.Yellow, true).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    if (sess.PlayerEntity.Family != null)
                    {
                        sess.EmitEvent(new FamilyAddExperienceEvent(500, FamXpObtainedFromType.Raid));
                    }
                }
            }

            _scheduler.Schedule(TimeSpan.FromSeconds(30), () =>
            {
                foreach (IClientSession sess in CaligorInstance.Sessions.ToList())
                {
                    sess.ChangeMap(UnknowLandInstance.Id, sess.PlayerEntity.MapX, sess.PlayerEntity.MapY);
                    sess.PlayerEntity.RemoveBuffAsync((int)BuffVnums.CALIGOR_LOSSER_TEAM).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                CaligorInstance.Destroy();
                CaligorInstance = null;
            });

            CaligorActive = false;
            AngelDamage = 0;
            DemonDamage = 0;

            FernonMap = _mapManager.GenerateMapInstanceByMapId(247, MapInstanceType.NormalInstance);
            IPortalEntity fernonMapPortal = _portalFactory.CreatePortal(PortalType.Raid, FernonMap, new(145, 198), mapDestId: -1, destPos: new Position(), (short)RaidType.Fernon, -1);
            FernonMap.AddPortalToMap(fernonMapPortal);

            _sessionManager.Broadcast(x => x.GenerateMsgPacket(
                    x.GetLanguageFormat(GameDialogKey.ACT4_FERNON_START), MsgMessageType.Middle));

            IPortalEntity portal = _portalFactory.CreatePortal(PortalType.TSNormal, UnknowLandInstance, new Position(93, 93), FernonMap, new Position(145, 242));
            UnknowLandInstance.AddPortalToMap(portal, _scheduler, (int)_configuration.FernonDurationRaidInstance.TotalSeconds, true);

            _scheduler.Schedule(_configuration.FernonDurationRaidInstance, () =>
            {
                _sessionManager.Broadcast(x => x.GenerateMsgPacket(
                    x.GetLanguageFormat(GameDialogKey.ACT4_FERNON_END), MsgMessageType.Middle));

                foreach (IClientSession sess in FernonMap.Sessions.ToList())
                {
                    sess.ChangeMap(153, 93, 93);
                }

                FernonMap?.Destroy();
                FernonMap = null;
            });

            void ShootFernonEnd(double min)
            {
                _scheduler.Schedule(_configuration.FernonDurationRaidInstance - TimeSpan.FromMinutes(min), () =>
                {
                    _sessionManager.Broadcast(x => x.GenerateMsgPacket(
                        x.GetLanguageFormat(GameDialogKey.ACT4_FERNON_END_IN, min), MsgMessageType.Middle));
                });
            }

            ShootFernonEnd(10);
            ShootFernonEnd(5);
            ShootFernonEnd(4);
            ShootFernonEnd(3);
            ShootFernonEnd(2);
            ShootFernonEnd(1);
        }

        private void DelayedCaligor()
        {
            _sessionManager.Broadcast(x => x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_START), MsgMessageType.Middle));

            CaligorInstance = _mapManager.GenerateMapInstanceByMapId(154, MapInstanceType.Caligor);
            UnknowLandInstance = _mapManager.GetBaseMapInstanceByMapId(153);

            IPortalEntity portal = _portalFactory.CreatePortal(PortalType.MapPortal, CaligorInstance, new Position(89, 23), UnknowLandInstance.Id, new Position(91, 35));
            CaligorInstance.AddPortalToMap(portal);
            portal = _portalFactory.CreatePortal(PortalType.MapPortal, CaligorInstance, new Position(128, 175), UnknowLandInstance.Id, new Position(110, 159));
            CaligorInstance.AddPortalToMap(portal);
            portal = _portalFactory.CreatePortal(PortalType.MapPortal, CaligorInstance, new Position(50, 174), UnknowLandInstance.Id, new Position(70, 159));
            CaligorInstance.AddPortalToMap(portal);

            portal = _portalFactory.CreatePortal(PortalType.MapPortal, UnknowLandInstance, new Position(70, 159), CaligorInstance.Id, new Position(50, 174));
            UnknowLandInstance.AddPortalToMap(portal);
            portal = _portalFactory.CreatePortal(PortalType.MapPortal, UnknowLandInstance, new Position(110, 159), CaligorInstance.Id, new Position(128, 175));
            UnknowLandInstance.AddPortalToMap(portal);
            portal = _portalFactory.CreatePortal(PortalType.MapPortal, UnknowLandInstance, new Position(91, 35), CaligorInstance.Id, new Position(89, 23));
            UnknowLandInstance.AddPortalToMap(portal);

            IMonsterEntity monsterEntity = _entity.CreateMonster(2305, CaligorInstance, new MonsterEntityBuilder
            {
                IsWalkingAround = true,
                IsHostile = true,
                IsBoss = true,
                IsTarget = true,
                IsRespawningOnDeath = false,
                PositionX = 70,
                PositionY = 90,
                Direction = 2
            });
            monsterEntity.ChangePosition(new Position(70, 90));
            monsterEntity.FirstX = 70;
            monsterEntity.FirstY = 90;
            monsterEntity.EmitEvent(new MapJoinMonsterEntityEvent(monsterEntity));

            CaligorInstance.IsPvp = true;

            CaligorStart = DateTime.UtcNow;
            CaligorEnd = CaligorStart + _configuration.CaligorDuration;
            CaligorActive = true;

            RefreshCaligorInstance();
        }

        public void InitializeAndStartCaligorInstance()
        {
            void ShoutMsg(byte duration)
            {
                _sessionManager.Broadcast(x =>
                            x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_BEGIN_IN,
                            duration), MsgMessageType.Middle));
            }
            ShoutMsg(5);
            _scheduler.Schedule(TimeSpan.FromMinutes(1), () =>
            {
                ShoutMsg(4);
            });
            _scheduler.Schedule(TimeSpan.FromMinutes(2), () =>
            {
                ShoutMsg(3);
            });
            _scheduler.Schedule(TimeSpan.FromMinutes(3), () =>
            {
                ShoutMsg(2);
            });
            _scheduler.Schedule(TimeSpan.FromMinutes(4), () =>
            {
                ShoutMsg(1);
            });
            _scheduler.Schedule(TimeSpan.FromMinutes(5), () =>
            {
                DelayedCaligor();
            });
        }

        private IMapInstance CaligorInstance { get; set; }
        private IMapInstance UnknowLandInstance { get; set; }
        public IMapInstance FernonMap { get; private set; }

        public void LockCaligorInstance()
        {
            foreach (IPortalEntity p in UnknowLandInstance.Portals.Where(s => s.DestinationMapInstance != null && s.DestinationMapInstance.Id == CaligorInstance.Id).ToList())
            {
                p.IsDisabled = true;
                UnknowLandInstance.Broadcast(p.GenerateGp());
                p.IsDisabled = false;
                p.Type = PortalType.Closed;
                UnknowLandInstance.Broadcast(p.GenerateGp());
            }
            _sessionManager.Broadcast(x => x.GenerateMsgPacket(
                    x.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_LOCKED), MsgMessageType.Middle));
            CaligorLocked = true;
        }

        private void ApplyBuffForLosserFaction()
        {
            BuffVnums losserBuff = BuffVnums.CALIGOR_LOSSER_TEAM;

            if (AngelDamage == DemonDamage) // remove buff for all
            {
                foreach (IClientSession session in CaligorInstance.Sessions.ToList())
                {
                    session.PlayerEntity.RemoveBuffAsync((int)losserBuff).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                return;
            }

            FactionType winningFaction = AngelDamage > DemonDamage ? FactionType.Angel : FactionType.Demon;

            foreach (IClientSession session in CaligorInstance.Sessions.ToList())
            {
                if (session.PlayerEntity.Faction != winningFaction)
                {
                    if (session.PlayerEntity.HasBuff(losserBuff))
                    {
                        continue;
                    }
                    Buff buff = _buffFactory.CreateBuff((int)losserBuff, session.PlayerEntity);
                    session.PlayerEntity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                    continue;
                }

                session.PlayerEntity.RemoveBuffAsync((int)losserBuff).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public void RefreshCaligorInstance()
        {
            ApplyBuffForLosserFaction();

            DateTime currentTime = DateTime.UtcNow;

            TimeSpan timeLeft = CaligorEnd - currentTime;

            const int maxHp = 29086082;
            string packet = UiPacketExtension.GenerateCHDM(maxHp, AngelDamage, DemonDamage, (int)timeLeft.TotalSeconds);
            _sessionManager.Broadcast(packet, new InMapVnumBroadcast(154));

            if (maxHp / 10 * 8 < AngelDamage + DemonDamage && !CaligorLocked)
            {
                LockCaligorInstance();
            }
        }
    }
}