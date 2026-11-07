using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;

namespace Plugin.Fishing.Commands
{
    [Name("Fishing")]
    [Group("fishing", "fish")]
    [Description("Module related to Fishing debug commands.")]
    [RequireAuthority(AuthorityType.GameMaster)] // Debug only for now
    public class FishingCommandModule : SaltyModuleBase
    {
        private readonly IFishingManager _fishingManager;

        public FishingCommandModule(IFishingManager fishingManager)
        {
            _fishingManager = fishingManager;
        }

        [Command("start")]
        [Description("Force start fishing")]
        public async Task<SaltyCommandResult> StartFishing()
        {
            await _fishingManager.StartFishing(Context.Player);
            return new SaltyCommandResult(true);
        }

        [Command("reel")]
        [Description("Force reel in")]
        public async Task<SaltyCommandResult> ReelIn()
        {
            await _fishingManager.ReelIn(Context.Player);
            return new SaltyCommandResult(true);
        }

        [Command("stop")]
        [Description("Force stop fishing")]
        public async Task<SaltyCommandResult> StopFishing()
        {
            _fishingManager.StopFishing(Context.Player);
            return new SaltyCommandResult(true);
        }
    }
}
