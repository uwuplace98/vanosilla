using System;
using System.Threading.Tasks;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.BasicImplementations.Managers;

namespace WingsEmu.Plugins.BasicImplementations.NpcDialogs.Arena;

public class ArenaTalentsRegisterHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _langService;
    private readonly ITalentArenaManager _talentArenaManager;
    private readonly IServerManager _serverManager;

    public ArenaTalentsRegisterHandler(IGameLanguageService langService, ITalentArenaManager talentArenaManager, IServerManager serverManager)
    {
        _langService = langService;
        _talentArenaManager = talentArenaManager;
        _serverManager = serverManager;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ARENA_OF_TALENTS_REGISTRATION };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        if (!_talentArenaManager.IsActive)
        {
            INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
            if (npcEntity == null)
            {
                return;
            }

            session.SendPacket(npcEntity.GenerateSayPacket(_langService.GetLanguageFormat(GameDialogKey.TALENT_ARENA_CLOSED, session.UserLanguage), ChatMessageColorType.Yellow));
            return;
        }

        // if (_serverManager.ChannelId != 3)
        // {
        //     return;
        // }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.ArenaInstance)
        {
            return;
        }

        if (session.PlayerEntity.Level < 30)
        {
            session.SendInfo(_langService.GetLanguage(GameDialogKey.TALENT_ARENA_LOW_LEVEL, e.Sender.UserLanguage));
            return;
        }

        // TODO: Ticket count check

        if (session.PlayerEntity.IsInGroup())
        {
            session.SendPacket(session.GenerateMsgPacket(_langService.GetLanguageFormat(GameDialogKey.INFORMATION_SHOUTMESSAGE_NEED_LEAVE_GROUP, session.UserLanguage), MsgMessageType.Middle));
            session.SendPacket(session.PlayerEntity.GenerateSayPacket(_langService.GetLanguageFormat(GameDialogKey.INFORMATION_SHOUTMESSAGE_NEED_LEAVE_GROUP, session.UserLanguage),
                ChatMessageColorType.Yellow));
            return;
        }

        session.PlayerEntity.TalentArenaComponent.RegisterToTalentArena();

        _talentArenaManager.AddRegisteredMember(session);
        session.SendBsInfoPacket(BsInfoType.OpenWindow, GameType.ArenaOfTalents, session.PlayerEntity.TalentArenaComponent.Time, QueueWindow.SearchRegisteredTeamImmediately);
    }
}