using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsAPI.Packets.Handling;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.BasicImplementation.Extensions;

namespace WingsEmu.Plugins.BasicImplementation.BattlePass
{
    public class BpOpenPacketHandler : GenericGamePacketHandlerBase<BpOpenPacket>
    {
        private readonly IBattlePassManager _manager;

        public BpOpenPacketHandler(IBattlePassManager manager)
        {
            _manager = manager;
        }

        protected override async Task HandlePacketAsync(IClientSession session, BpOpenPacket packet)
        {
            await Task.CompletedTask;

            var bpm = _manager.GenerateBpm(session.PlayerEntity);
            if (bpm == null)
            {
                PhoenixLib.Logging.Log.Warn("[BATTLEPASS_OPEN] Configuration not loaded, cannot open BattlePass.");
                return;
            }

            PhoenixLib.Logging.Log.Debug($"[BATTLEPASS_OPEN] Sending BPM: {bpm}");
            session.SendPacket(bpm);

            // Use the manager-aware version that includes tier data
            string bpp = _manager.GenerateBpp(session.PlayerEntity);
            PhoenixLib.Logging.Log.Debug($"[BATTLEPASS_OPEN] Sending BPP: {bpp}");
            session.SendPacket(bpp);

            string bpt = _manager.GenerateBpt();
            PhoenixLib.Logging.Log.Debug($"[BATTLEPASS_OPEN] Sending BPT: {bpt}");
            session.SendPacket(bpt);

            session.SendPacket("bpo");
        }
    }
}
