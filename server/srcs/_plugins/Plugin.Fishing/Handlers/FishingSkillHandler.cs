using System.Threading.Tasks;
using WingsEmu.Game.Core.SkillHandling;
using WingsEmu.Game.Core.SkillHandling.Event;
using WingsEmu.Game.Networking;

namespace Plugin.Fishing.Handlers
{
    public class FishingSkillHandler : ISkillHandler
    {
        private readonly IFishingManager _fishingManager;

        public FishingSkillHandler(IFishingManager fishingManager)
        {
            _fishingManager = fishingManager;
        }

        // 797: Throw Fishing Rod
        // 798: Reel In
        public long[] SkillId => new long[] { 797, 798 };

        public async Task ExecuteAsync(IClientSession session, SkillEvent e)
        {
            if (e.SkillId == 797)
            {
                await _fishingManager.StartFishing(session);
            }
            else if (e.SkillId == 798)
            {
                await _fishingManager.ReelIn(session);
            }
        }
    }
}
