// WingsEmu
// 
// Developed by NosWings Team


using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RaidInstanceStartEventHandler : IAsyncEventProcessor<RaidInstanceStartEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;
    private readonly IRaidFactory _raidFactory;
    private readonly IRaidManager _raidManager;

    public RaidInstanceStartEventHandler(IRaidManager raidManager, IRaidFactory raidFactory, IGameLanguageService gameLanguage, IItemsManager itemsManager)
    {
        _raidManager = raidManager;
        _raidFactory = raidFactory;
        _gameLanguage = gameLanguage;
        _itemsManager = itemsManager;
    }

    public async Task HandleAsync(RaidInstanceStartEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (session.PlayerEntity.HasRaidStarted)
        {
            return;
        }

        if (!session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            return;
        }

        _raidManager.UnregisterRaidFromRaidPublishList(session.PlayerEntity.Raid);

        RaidInstance raidInstance = _raidFactory.CreateRaid(session.PlayerEntity.Raid);

        if (raidInstance == null)
        {
            Log.Warn("Failed to create raid instance");
            return;
        }

        if (session.PlayerEntity.Raid.Members.Count > session.PlayerEntity.Raid.MaximumMembers)
        {
            IClientSession getLastJoinedMember = session.PlayerEntity.Raid.Members[^1];
            getLastJoinedMember?.EmitEvent(new RaidPartyLeaveEvent(false, false));
        }

        RaidType raidType = session.PlayerEntity.Raid.Type;

        session.PlayerEntity.Raid.StartRaid(raidInstance);

        foreach (IClientSession raidSession in session.PlayerEntity.Raid.Members)
        {
            string getRaidName = raidSession.GenerateRaidName(_gameLanguage, raidType);
            if (!raidSession.PlayerEntity.IsAlive())
            {
                raidSession.PlayerEntity.Hp = 1;
                raidSession.PlayerEntity.Mp = 1;
            }

            if (raidType == RaidType.Fernon)
            {
                raidSession.PlayerEntity.SetPositionBeforeFernonRaidEnter(raidSession.PlayerEntity.Position);
            }

            raidSession.ChangeMap(raidInstance.SpawnInstance.MapInstance, raidInstance.SpawnPoint.X, raidInstance.SpawnPoint.Y);
            raidSession.SendRaidUiPacket(raidType, RaidWindowType.MISSION_START);

            //Sending this last two seems useless, but sending them just to be sure
            raidSession.SendRaidPacket(RaidPacketType.INSTANCE_START);
            raidSession.SendRaidPacket(RaidPacketType.AFTER_INSTANCE_START_BUT_BEFORE_REFRESH_MEMBERS);

            
        }

        await session.EmitEventAsync(new RaidStartedEvent());
    }
}