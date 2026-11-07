using WingsEmu.DTOs.Maps;

namespace WingsEmu.Game.Networking.Broadcasting
{
    public class InMapFlagBroadcast : IBroadcastRule
    {
        private readonly MapFlags _mapFlags;

        public InMapFlagBroadcast(MapFlags mapFlags) => _mapFlags = mapFlags;

        public bool Match(IClientSession session) => session.CurrentMapInstance != null && session.CurrentMapInstance.HasMapFlag(_mapFlags);
    }
}