using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Icebreaker.Commands
{
    [Name("Icebreaker")]
    [Group("icebreaker", "ib", "ice")]
    [Description("Module related to Icebreaker commands.")]
    public class IcebreakerCommandModule : SaltyModuleBase
    {
        private readonly IIcebreakerManager _icebreakerManager;

        public IcebreakerCommandModule(IIcebreakerManager icebreakerManager)
        {
            _icebreakerManager = icebreakerManager;
        }

        [Command("start")]
        [Description("Starts an Icebreaker event for the specified level bracket.")]
        [RequireAuthority(AuthorityType.GameMaster)]
        public async Task<SaltyCommandResult> StartIcebreaker(int minLevel = 1, int maxLevel = 99)
        {
            await Task.CompletedTask;

            if (minLevel < 1 || maxLevel > 99 || minLevel > maxLevel)
            {
                return new SaltyCommandResult(false, "Invalid level range. Use: $ice start [minLevel] [maxLevel]");
            }

            bool started = _icebreakerManager.StartEvent(minLevel, maxLevel);

            if (started)
            {
                Context.Player.SendSuccessChatMessage($"Icebreaker event started for levels {minLevel}-{maxLevel}!");
                return new SaltyCommandResult(true);
            }

            return new SaltyCommandResult(false, "Failed to start Icebreaker. Check configuration and logs.");
        }

        [Command("status")]
        [Description("Shows the current Icebreaker event status.")]
        public async Task<SaltyCommandResult> Status()
        {
            await Task.CompletedTask;

            bool waiting = _icebreakerManager.IsWaitingForPlayers;
            if (waiting)
            {
                Context.Player.SendChatMessage("Icebreaker is waiting for players to join!", ChatMessageColorType.Yellow);
            }
            else
            {
                Context.Player.SendChatMessage("No active Icebreaker event.", ChatMessageColorType.Yellow);
            }

            return new SaltyCommandResult(true);
        }

        [Command("join")]
        [Description("Joins the current Icebreaker event.")]
        public async Task<SaltyCommandResult> JoinEvent()
        {
            await _icebreakerManager.Join(Context.Player.PlayerEntity);
            return new SaltyCommandResult(true);
        }

        [Command("forcestart", "force")]
        [Description("Force starts the battle immediately, skipping lobby wait time. For admin testing.")]
        [RequireAuthority(AuthorityType.GameMaster)]
        public async Task<SaltyCommandResult> ForceStartBattle()
        {
            await Task.CompletedTask;

            bool forced = _icebreakerManager.ForceStartBattle();
            if (forced)
            {
                Context.Player.SendSuccessChatMessage("Icebreaker battle force started!");
                return new SaltyCommandResult(true);
            }

            return new SaltyCommandResult(false, "No active Icebreaker lobby to force start.");
        }
    }
}
