using System.Threading.Tasks;
using WingsEmu.Game.Characters;

namespace WingsEmu.Game.Managers
{
    public interface IIcebreakerManager
    {
        /// <summary>
        /// Checks if a player is frozen in an Icebreaker instance.
        /// </summary>
        bool IsFrozen(long characterId);

        /// <summary>
        /// Freezes a player who was eliminated.
        /// </summary>
        void Freeze(IPlayerEntity player);

        /// <summary>
        /// Unfreezes a player, merging them into the unfreezer's team.
        /// </summary>
        Task Unfreeze(IPlayerEntity target, IPlayerEntity unfreezer);

        /// <summary>
        /// Player joins an Icebreaker lobby.
        /// </summary>
        Task Join(IPlayerEntity player);

        /// <summary>
        /// Removes a player from any Icebreaker instance.
        /// </summary>
        void RemovePlayer(long characterId);

        /// <summary>
        /// Starts an Icebreaker event for a given level bracket.
        /// </summary>
        /// <param name="minLevel">Minimum level for the bracket.</param>
        /// <param name="maxLevel">Maximum level for the bracket.</param>
        /// <returns>True if event started successfully.</returns>
        bool StartEvent(int minLevel, int maxLevel);

        /// <summary>
        /// Checks if there's an active Icebreaker waiting for players.
        /// </summary>
        bool IsWaitingForPlayers { get; }

        /// <summary>
        /// Processes the event loop (state transitions).
        /// </summary>
        void Process();

        /// <summary>
        /// Force starts the battle immediately, skipping lobby wait time. For admin testing.
        /// </summary>
        /// <returns>True if a battle was force started.</returns>
        bool ForceStartBattle();
    }
}
