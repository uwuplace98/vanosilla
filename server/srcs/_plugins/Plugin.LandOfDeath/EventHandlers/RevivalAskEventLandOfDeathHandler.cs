using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace Plugin.LandOfDeath.EventHandlers;

public class RevivalAskEventLandOfDeathHandler : IAsyncEventProcessor<RevivalAskEvent>
{
    private readonly IGameLanguageService _languageService;

    public RevivalAskEventLandOfDeathHandler(IGameLanguageService languageService) => _languageService = languageService;

    public async Task HandleAsync(RevivalAskEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (session.PlayerEntity.IsAlive() || e.AskRevivalType != AskRevivalType.LandOfDeathRevival)
        {
            return;
        }

        
        int item = (int)ItemVnums.POTION_OF_SALVATION;
        short amount = 1;
        if (!session.PlayerEntity.HasItem(item, amount))
        {
            e.Sender.PlayerEntity.UpdateRevival(DateTime.MinValue, RevivalType.DontPayRevival, ForcedType.Forced);
            return;
        }

        session.SendDialog(CharacterPacketExtension.GenerateRevivalPacket(RevivalType.TryPayRevival), CharacterPacketExtension.GenerateRevivalPacket(RevivalType.DontPayRevival),
            _languageService.GetLanguageFormat(GameDialogKey.LAND_OF_DEATH_REVIVAL_DIALOG, e.Sender.UserLanguage));
    }
}