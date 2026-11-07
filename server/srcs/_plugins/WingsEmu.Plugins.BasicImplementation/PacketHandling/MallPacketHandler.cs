using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Packets.Handling;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Items;
using WingsEmu.Game._i18n;
using WingsEmu.Packets.Enums;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game.Managers;

namespace WingsEmu.Plugins.BasicImplementation.PacketHandling
{
    public class MallPacketHandler : GenericGamePacketHandlerBase<MallPacket>
    {
        private readonly INosMallManager _nosMallManager;
        private readonly IGameItemInstanceFactory _itemFactory;
        private readonly IGameLanguageService _language;
        private readonly ISessionManager _sessionManager;

        public MallPacketHandler(INosMallManager nosMallManager, IGameItemInstanceFactory itemFactory, IGameLanguageService language, ISessionManager sessionManager)
        {
            _nosMallManager = nosMallManager;
            _itemFactory = itemFactory;
            _language = language;
            _sessionManager = sessionManager;
        }
        protected override async Task HandlePacketAsync(IClientSession session, MallPacket packet)
        {
            switch (packet.Type)
            {
                case 0: // Refresh coins
                    session.SendPacket(session.GenerateCashPacket());
                    break;
                case 10: // Buy
                    await HandleBuy(session, packet);
                    break;
                case 12: // Gift/Reward
                    await HandleGift(session, packet);
                    break;
                case 15: // Open Shop
                    session.SendPacket(session.GenerateMblsPacket());
                    session.SendPacket(session.GenerateZzimPacket()); // Send wishlist
                    break;
            }
        }

        private async Task HandleBuy(IClientSession session, MallPacket packet)
        {
            if (!int.TryParse(packet.Arg1?.ToString(), out int mallItemId))
            {
                Log.Warn($"[NOSMALL] Failed to parse mallItemId from Arg1={packet.Arg1}");
                return;
            }
            Log.Info($"[NOSMALL] Character {session.PlayerEntity.Name} tried to buy item: {mallItemId}");
            Log.Debug($"[NOSMALL] Total loaded items: {System.Linq.Enumerable.Count(_nosMallManager.GetItems())}");

            var mallItem = _nosMallManager.GetItem(mallItemId);
            if (mallItem == null)
            {
                Log.Warn($"[NOSMALL] Item {mallItemId} NOT FOUND in NosMall dictionary.");
                session.SendSMemo(SmemoType.Error, "Item not found in NosMall.");
                return;
            }
            Log.Info($"[NOSMALL] Found item: Id={mallItem.Id}, Name={mallItem.Name}, ItemVnum={mallItem.ItemVnum}, Price={mallItem.Price}");

            if (session.Account.Coins < mallItem.Price)
            {
                session.SendSMemo(SmemoType.Error, _language.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage));
                return;
            }

            if (!session.PlayerEntity.HasSpaceFor(mallItem.ItemVnum, (short)mallItem.Amount))
            {
                session.SendSMemo(SmemoType.Error, _language.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage));
                return;
            }

            // Deduct Coins
            session.Account.Coins -= mallItem.Price;

            // Create and Add Item
            var newItem = _itemFactory.CreateItem(mallItem.ItemVnum, mallItem.Amount);
            await session.AddNewItemToInventory(newItem);

            // Notify
            session.SendSMemo(SmemoType.Balance, $"Purchased {mallItem.Name} for {mallItem.Price} Coins.");
            session.SendPacket(session.GenerateCashPacket()); // Update UI
        }

        private async Task HandleGift(IClientSession session, MallPacket packet)
        {
            // mall 12 ReceiverName nosmallid DescriptionText
            string receiverName = packet.Arg1?.ToString();
            if (string.IsNullOrEmpty(receiverName))
            {
                session.SendSMemo(SmemoType.Error, "Invalid receiver name.");
                return;
            }

            if (!int.TryParse(packet.Arg2?.ToString(), out int mallItemId))
            {
                session.SendSMemo(SmemoType.Error, "Invalid item.");
                return;
            }

            string description = packet.Arg3?.ToString() ?? "";

            var mallItem = _nosMallManager.GetItem(mallItemId);
            if (mallItem == null)
            {
                session.SendSMemo(SmemoType.Error, "Item not found in NosMall.");
                return;
            }

            if (session.Account.Coins < mallItem.Price)
            {
                session.SendSMemo(SmemoType.Error, _language.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage));
                return;
            }

            // Find receiver
            var receiver = _sessionManager.GetSessionByCharacterName(receiverName);
            if (receiver == null)
            {
                session.SendSMemo(SmemoType.Error, "Receiver not found or offline.");
                return;
            }

            if (!receiver.PlayerEntity.HasSpaceFor(mallItem.ItemVnum, (short)mallItem.Amount))
            {
                session.SendSMemo(SmemoType.Error, "Receiver doesn't have enough inventory space.");
                return;
            }

            // Deduct coins from sender
            session.Account.Coins -= mallItem.Price;

            // Give item to receiver
            var newItem = _itemFactory.CreateItem(mallItem.ItemVnum, mallItem.Amount);
            await receiver.AddNewItemToInventory(newItem);

            // Notify sender
            session.SendSMemo(SmemoType.Balance, $"Gift sent to {receiverName}!");
            session.SendPacket(session.GenerateCashPacket());

            // Notify receiver
            receiver.SendSMemo(SmemoType.Balance, $"You received a gift from {session.PlayerEntity.Name}: {mallItem.Name}");

            Log.Info($"[NOSMALL] {session.PlayerEntity.Name} gifted {mallItem.Name} to {receiverName}");
        }
    }
}

