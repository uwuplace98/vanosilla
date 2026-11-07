using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Packets.Handling;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.BasicImplementation.NosMall;

namespace WingsEmu.Plugins.BasicImplementation.PacketHandling
{
    /// <summary>
    /// Handler for NosMall wishlist packets.
    /// Manages adding/removing items from the NosMall wishlist.
    /// </summary>
    public class ZzimPacketHandler : GenericGamePacketHandlerBase<ZzimPacket>
    {
        private readonly INosMallWishlistManager _wishlistManager;

        public ZzimPacketHandler(INosMallWishlistManager wishlistManager)
        {
            _wishlistManager = wishlistManager;
        }

        protected override async Task HandlePacketAsync(IClientSession session, ZzimPacket packet)
        {
            var character = session.PlayerEntity;
            
            Log.Debug($"[NOSMALL_WISHLIST] Player {character.Name} - Type: {packet.Type}, ItemVnum: {packet.ItemVnum}");

            switch (packet.Type)
            {
                case 1: // Add to wishlist
                    if (_wishlistManager.IsInWishlist(character.Id, packet.ItemVnum))
                    {
                        // Item already in wishlist - just refresh
                        SendWishlistPacket(session);
                        return;
                    }
                    
                    if (!_wishlistManager.AddToWishlist(character.Id, packet.ItemVnum))
                    {
                        // Wishlist full (max 10 items)
                        session.SendMsg("Wishlist is full (max 10 items)", MsgMessageType.Middle);
                        SendWishlistPacket(session);
                        return;
                    }
                    
                    Log.Info($"[NOSMALL_WISHLIST] {character.Name} added item {packet.ItemVnum} to wishlist");
                    break;
                    
                case 2: // Remove from wishlist
                    _wishlistManager.RemoveFromWishlist(character.Id, packet.ItemVnum);
                    Log.Info($"[NOSMALL_WISHLIST] {character.Name} removed item {packet.ItemVnum} from wishlist");
                    break;
                    
                default:
                    Log.Warn($"[NOSMALL_WISHLIST] Unknown zzim type: {packet.Type}");
                    return;
            }

            // Send updated wishlist to client
            SendWishlistPacket(session);

            await Task.CompletedTask;
        }

        private void SendWishlistPacket(IClientSession session)
        {
            var wishlist = _wishlistManager.GetWishlist(session.PlayerEntity.Id);
            
            if (wishlist.Count == 0)
            {
                session.SendPacket("zzim");
            }
            else
            {
                // Format: zzim {vnum1} {vnum2} {vnum3}...
                session.SendPacket($"zzim {string.Join(" ", wishlist)}");
            }
        }
    }
}
