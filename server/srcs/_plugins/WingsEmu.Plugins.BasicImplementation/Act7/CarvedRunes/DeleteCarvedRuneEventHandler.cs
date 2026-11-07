using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.CarvedRune;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game.Act7.CarvedRunes;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.Act7.CarvedRunes;

public class DeleteCarvedEventHandler : IAsyncEventProcessor<RemoveCarvedRuneEvent>
{
    public async Task HandleAsync(RemoveCarvedRuneEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        GameItemInstance item = e.Equipment.ItemInstance;

        if (item.GameItem.EquipmentSlot != EquipmentType.MainWeapon && item.GameItem.EquipmentSlot != EquipmentType.Armor)
        {
            // Cannot delete if it's not MainWeapon or Armor.
            return;
        }

        // if (item.GameItem.LevelMinimum < 80)
        // {
        //     // Cannot delete if equipment is under Level 80.
        //     return;
        // }

        if (item.CarvedRunes.BCards == null)
        {
            session.SendMsgi(MsgiMessageType.Default, Game18NConstString.WeaponHaveNoRunes);
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }

        if (!session.PlayerEntity.HasItem((short)ItemVnums.RUNE_REMOVAL_HAMMER))
        {
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }

        item.CarvedRunes = new CarvedRunesDTO();
        await session.RemoveItemFromInventory((short)ItemVnums.RUNE_REMOVAL_HAMMER);
        session.SendMsgi(MsgiMessageType.Default, Game18NConstString.RuneRemoved);
        session.SendInventoryAddPacket(e.Equipment);
        session.SendShopEndPacket(ShopEndType.Npc);
    }
}
