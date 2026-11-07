using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.LandOfDeath.Events;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace Plugin.LandOfDeath.EventHandlers;

public class RevivalLandOfDeathEventHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;

    public RevivalLandOfDeathEventHandler(IGameLanguageService gameLanguage,
        ISpPartnerConfiguration spPartnerConfiguration, IGameLanguageService languageService, IItemsManager itemsManager)
    {
        _gameLanguage = gameLanguage;
        _languageService = languageService;
        _itemsManager = itemsManager;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.LodInstance)
        {
            return;
        }

        session.PlayerEntity.DisableRevival();

        if (!session.PlayerEntity.IsInFamily())
        {
            await session.EmitEventAsync(new LandOfDeathLeaveMapEvent());
            return;
        }
        
        bool paidPenalization = false;
        
        if (e.RevivalType != RevivalType.DontPayRevival)
        {
            paidPenalization = await TryPayPenalization(session.PlayerEntity);
        }
         
        
        if (!paidPenalization )
        {
            await session.EmitEventAsync(new LandOfDeathLeaveMapEvent());
            return;
        }


        session.UpdateVisibility();
        await session.PlayerEntity.Restore(restoreMates: false);
        session.BroadcastRevive();
        session.BroadcastInTeamMembers(_gameLanguage, _spPartnerConfiguration);
        session.RefreshParty(_spPartnerConfiguration);
        await session.CheckPartnerBuff();
        session.SendBuffsPacket();
    }

    private async Task<bool> TryPayPenalization(IPlayerEntity character)
    {
        int saverItem = (int)ItemVnums.POTION_OF_SALVATION;
        short saverAmount = 1;
        string saver = _languageService.GetItemName(_itemsManager.GetItem(saverItem), character.Session);

        if (character.CountItemWithVnum(saverItem) >= saverAmount)
        {
            await character.Session.RemoveItemFromInventory(saverItem, saverAmount);
            character.Session.SendSuccessChatMessage(_languageService.GetLanguageFormat(GameDialogKey.INFORMATION_CHATMESSAGE_REQUIRED_ITEM_EXPENDED, character.Session.UserLanguage, saver, saverAmount));

            return true;
        }

        character.Session.SendErrorChatMessage(_languageService.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, character.Session.UserLanguage, saverAmount, saver));
        return false;

    }
}