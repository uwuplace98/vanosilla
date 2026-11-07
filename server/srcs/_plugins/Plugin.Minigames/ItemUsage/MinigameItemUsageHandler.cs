using System.Threading.Tasks;
using WingsEmu.Game._enum;
using WingsEmu.Game.Extensions;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace Plugin.Minigames.ItemUsage
{
    public class MinigameItemUsageHandler : IItemUsageByVnumHandler
    {
        private const int BushiKingSealVnum = 5853;
        private const int SheepFarmRaidSealVnum = 5854;
        private const int MeteoriteGameRaidSealVnum = 5855;

        public long[] Vnums => new long[] {BushiKingSealVnum, SheepFarmRaidSealVnum, MeteoriteGameRaidSealVnum };

        public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
        {
            var itemVnum = e.Item.ItemInstance.ItemVNum;

            if (itemVnum == SheepFarmRaidSealVnum)
            {
                session.SendPacket($"bsinfo {(byte)GameType.SheepFarm} 4 0 0");
                return;
            }

            if (itemVnum == MeteoriteGameRaidSealVnum)
            {
                PhoenixLib.Logging.Log.Info($"[MINIGAME] Using Meteorite Seal. Sending bsinfo {(byte)GameType.DodgeMeteor} 10 0 0");
                session.SendPacket($"bsinfo {(byte)GameType.DodgeMeteor} 10 0 0");
                return;
            }

            if (itemVnum == BushiKingSealVnum)
            {
                session.SendPacket($"bsinfo {(byte)GameType.BushiKingDefend} 3 0 0");
                return;
            }
        }
    }
}
