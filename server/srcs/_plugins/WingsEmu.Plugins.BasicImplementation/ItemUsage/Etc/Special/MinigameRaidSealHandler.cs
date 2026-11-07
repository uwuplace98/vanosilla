using System.Threading.Tasks;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.ItemUsage.Etc.Special
{
    public class MinigameRaidSealHandler : IItemHandler
    {
        public ItemType ItemType => ItemType.Special;
        public long[] Effects => new long[] { 1717 };

        public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
        {
            int effectValue = e.Item.ItemInstance.GameItem.EffectValue;

            GameType? gameType = null;
            byte level = 0;

            switch (effectValue)
            {
                case 1: // Bushi King Defence
                    gameType = GameType.BushiKingDefend;
                    level = 3;
                    break;
                case 2: // Sheep Farm
                    gameType = GameType.SheepFarm;
                    level = 4;
                    break;
                case 3: // Dodge Meteor
                    gameType = GameType.DodgeMeteor;
                    level = 10;
                    break;
            }

            if (gameType == null)
            {
                return;
            }

            session.SendPacket($"bsinfo {(byte)gameType.Value} {level} 0 0");
        }
    }
}
