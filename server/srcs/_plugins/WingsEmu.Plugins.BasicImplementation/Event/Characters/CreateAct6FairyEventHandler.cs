using PhoenixLib.Events;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.Event.Characters
{
    public class CreateAct6FairyEventHandler : IAsyncEventProcessor<CreateAct6FairyEvent>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IGameItemInstanceFactory _gameItemInstance;
        private readonly CreateFairyAct6Configuration _conf;

        public CreateAct6FairyEventHandler(IGameLanguageService gameLanguage, IRandomGenerator randomGenerator, CreateFairyAct6Configuration conf, IGameItemInstanceFactory gameItemInstance)
        {
            _gameLanguage = gameLanguage;
            _randomGenerator = randomGenerator;
            _gameItemInstance = gameItemInstance;
            _conf = conf;
        }

        public async Task HandleAsync(CreateAct6FairyEvent e, CancellationToken cancellation)
        {
            InventoryItem fairy = e.Inv;
            IClientSession session = e.Sender;
            FairyConfig conf = _conf.FirstOrDefault(s => s.FairyType == e.FairyType);
            if (conf == null)
            {
                session.SendShopEndPacket(ShopEndType.Npc);
                session.SendShopEndPacket(ShopEndType.SpecialistHolder);
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
                return;
            }

            if (conf.AllowedFairyVnum.All(s => s != fairy.ItemInstance.ItemVNum))
            {
                session.SendShopEndPacket(ShopEndType.Npc);
                session.SendShopEndPacket(ShopEndType.SpecialistHolder);
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
                return;
            }

            if (!conf.SpecialItemsNeeded.Any(item => session.PlayerEntity.HasItem(item.ItemVnum, (short)item.Amount)))
            {
                session.SendShopEndPacket(ShopEndType.Npc);
                session.SendShopEndPacket(ShopEndType.SpecialistHolder);
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
            }

            if (!session.HasEnoughGold(conf.GoldPrice))
            {
                session.SendShopEndPacket(ShopEndType.Npc);
                session.SendShopEndPacket(ShopEndType.SpecialistHolder);
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
                return;
            }

            int rnd = _randomGenerator.RandomNumber();
            bool saySucces = false;
            if (rnd < conf.SucessRate)
            {
                int itemVnum = (int)(conf.FairyVnumCreated + (fairy.ItemInstance.GameItem.Element - 1));
                GameItemInstance newItem = _gameItemInstance.CreateItem(itemVnum, 1);
                newItem.ElementRate = fairy.ItemInstance.ElementRate;
                await session.AddNewItemToInventory(newItem);
                await session.RemoveItemFromInventory(item: fairy);
                saySucces = true;
                session.SendPacket($"pdti 11 {itemVnum} 1 29 0 0");
            }

            session.SendChatMessage(_gameLanguage.GetLanguage(saySucces ? GameDialogKey.ITEM_CRAFT_SUCCESS : GameDialogKey.ITEM_CRAFT_FAILED, session.UserLanguage), ChatMessageColorType.Yellow);
            foreach (SpecialItem item in conf.SpecialItemsNeeded)
            {
                await session.RemoveItemFromInventory((short)item.ItemVnum, (short)item.Amount);
            }
            session.PlayerEntity.RemoveGold(conf.GoldPrice);
            session.SendShopEndPacket(ShopEndType.Npc);
            session.SendShopEndPacket(ShopEndType.SpecialistHolder);
        }
    }
}