//using System.Threading.Tasks;
//using WingsEmu.DTOs.Maps;
//using WingsEmu.Game._i18n;
//using WingsEmu.Game._NpcDialog;
//using WingsEmu.Game._NpcDialog.Event;
//using WingsEmu.Game.Entities;
//using WingsEmu.Game.Extensions;
//using WingsEmu.Game.Maps;
//using WingsEmu.Game.Networking;
//using WingsEmu.Game.Ship.Configuration;
//using WingsEmu.Game.Ship.Event;
//using WingsEmu.Packets.Enums;

//namespace WingsEmu.Plugins.BasicImplementations.NpcDialogs.Act6;

//public class A7Ship : INpcDialogAsyncHandler
//{
//    private readonly IGameLanguageService _gameLanguage;
//    private readonly IMapManager _mapManager;

//    public A7Ship(IMapManager mapManager, IGameLanguageService gameLanguage)
//    {
//        _mapManager = mapManager;
//        _gameLanguage = gameLanguage;
//    }

//    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ACT7_ENTER_SHIP };

//    public async Task Execute(IClientSession session, NpcDialogEvent e)
//    {
//        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
//        if (npcEntity == null)
//        {
//            return;
//        }
        
//        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
//        {
//            return;
//        }

//        if (session.PlayerEntity.HeroLevel < 20)
//        {
//            session.SendInformationChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_TOO_LOW_LVL, session.UserLanguage));
//            return;
//        }

//        if (session.CantPerformActionOnAct4())
//        {
//            return;
//        }

//        if (session.PlayerEntity.IsInRaidParty)
//        {
//            return;
//        }

//        if (session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
//        {
//            return;
//        }

//        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
//        {
//            return;
//        }

//        if (session.PlayerEntity.Gold >= 50000)
//        {
//            session.PlayerEntity.Gold -= 50000;
//            session.PlayerEntity.Refre
//            await e.Sender.EmitEventAsync(new ShipEnterEvent(ShipType.Act7));
//        }
//        session.ChangeMap(228, 85, 104);
//    }
//}