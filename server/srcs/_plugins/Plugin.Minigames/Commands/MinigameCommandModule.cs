using System.Threading.Tasks;
using Qmmands;
using Plugin.Minigames.Managers;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Extensions;

namespace Plugin.Minigames.Commands
{
    [Name("Minigames")]
    [Group("minigame", "mg")]
    [Description("Module related to Minigame commands.")]
    // [RequireAuthority(AuthorityType.GameMaster)] <-- Removed class level
    public class MinigamesCommandModule : SaltyModuleBase
    {
        private readonly ISheepFarmManager _sheepFarmManager;
        private readonly IMeteoriteGameManager _meteoriteManager;

        public MinigamesCommandModule(ISheepFarmManager sheepFarmManager, IMeteoriteGameManager meteoriteManager)
        {
            _sheepFarmManager = sheepFarmManager;
            _meteoriteManager = meteoriteManager;
        }

        [Command("join", "start")]
        [Description("Joins a minigame (sheep, meteor)")]
        public async Task<SaltyCommandResult> JoinMinigame(string type)
        {
            if (type.ToLower() == "sheep")
            {
                await _sheepFarmManager.JoinGame(Context.Player);
                return new SaltyCommandResult(true);
            }

            if (type.ToLower() == "meteor" || type.ToLower() == "meteorite")
            {
                await _meteoriteManager.JoinGame(Context.Player);
                return new SaltyCommandResult(true);
            }

            return new SaltyCommandResult(false, "Unknown minigame type. Try 'sheep' or 'meteor'.");
        }

        [Command("seals")]
        [Description("Gives Minigame Raid Seals")]
        [RequireAuthority(AuthorityType.GameMaster)]
        public async Task<SaltyCommandResult> GetSeals()
        {
            await Context.Player.AddItemToInventory(5854, 1); // Sheep Farm Raid Seal
            await Context.Player.AddItemToInventory(5855, 1); // Dodge Meteor Raid Seal
            Context.Player.SendSuccessChatMessage("Received Minigame Seals.");
            return new SaltyCommandResult(true);
        }
    }
}
