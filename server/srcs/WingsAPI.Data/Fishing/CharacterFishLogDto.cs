namespace WingsAPI.Data.Fishing
{
    /// <summary>
    /// Tracks a player's fishing log for a specific fish type.
    /// </summary>
    public class CharacterFishLogDto
    {
        /// <summary>
        /// Character ID this log belongs to.
        /// </summary>
        public long CharacterId { get; set; }

        /// <summary>
        /// VNum of the fish caught.
        /// </summary>
        public int FishVnum { get; set; }

        /// <summary>
        /// Total count of this fish caught.
        /// </summary>
        public int CatchCount { get; set; }

        /// <summary>
        /// Maximum length of this fish caught (in cm).
        /// </summary>
        public int MaxLength { get; set; }
    }
}
