namespace WingsAPI.Data.Fishing
{
    /// <summary>
    /// Represents a possible fish drop when fishing.
    /// </summary>
    public class FishingDropDto
    {
        /// <summary>
        /// Unique identifier for the drop entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Item VNum of the fish or item that can be caught.
        /// </summary>
        public int ItemVnum { get; set; }

        /// <summary>
        /// Chance to catch this item (out of 10000).
        /// </summary>
        public int Chance { get; set; }

        /// <summary>
        /// Region/Zone ID this drop belongs to.
        /// </summary>
        public int RegionId { get; set; }

        /// <summary>
        /// Minimum fishing level required to catch this.
        /// </summary>
        public int MinLevel { get; set; }

        /// <summary>
        /// Maximum fishing level for this drop (0 = no limit).
        /// </summary>
        public int MaxLevel { get; set; }

        /// <summary>
        /// Minimum length of the fish.
        /// </summary>
        public short MinFishLength { get; set; }

        /// <summary>
        /// Maximum length of the fish.
        /// </summary>
        public short MaxFishLength { get; set; }
    }
}
