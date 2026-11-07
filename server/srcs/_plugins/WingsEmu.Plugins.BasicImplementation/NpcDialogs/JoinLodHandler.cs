using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.NpcDialogs.Teleport;

public class JoinLodHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _langService;
    private readonly IMapManager _mapManager;
    private readonly ILandOfDeathManager _landOfDeathManager;
    private readonly LandOfDeathConfiguration _landOfDeathConfiguration;

    public JoinLodHandler(IGameLanguageService langService, IMapManager mapManager, ILandOfDeathManager landOfDeathManager, LandOfDeathConfiguration landOfDeathConfiguration)
    {
        _mapManager = mapManager;
        _langService = langService;
        _landOfDeathManager = landOfDeathManager;
        _landOfDeathConfiguration = landOfDeathConfiguration;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.LAND_OF_CHAOS_ENTER };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        if (!_landOfDeathManager.IsActive || _landOfDeathManager.IsClosed)
        {
            session.SendMsg(_langService.GetLanguage(GameDialogKey.LAND_OF_DEATH_MESSAGE_CLOSED, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            return;
        }

        if (!session.PlayerEntity.IsInFamily())
        {
            session.SendChatMessage(_langService.GetLanguage(GameDialogKey.FAMILY_INFO_NOT_IN_FAMILY, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (session.PlayerEntity.Level < _landOfDeathConfiguration.MinLevel)
        {
            session.SendChatMessage(_langService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_TOO_LOW_LVL, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (session.PlayerEntity.Family.LandOfDeath == null)
        {
            IMapInstance instance = _mapManager.GenerateMapInstanceByMapId(_landOfDeathConfiguration.MapId, MapInstanceType.LodInstance);
            session.PlayerEntity.Family.LandOfDeath = instance;
            _landOfDeathManager.AddInstance(instance);
        }

        session.ChangeMap(session.PlayerEntity.Family.LandOfDeath, _landOfDeathConfiguration.SpawnPos.X, _landOfDeathConfiguration.SpawnPos.Y);
    }
}