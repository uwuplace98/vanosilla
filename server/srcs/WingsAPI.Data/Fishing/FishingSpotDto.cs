namespace WingsAPI.Data.Fishing
{
    /// <summary>
    /// Represents a fishing spot on a map where players can fish.
    /// </summary>
    public class FishingSpotDto
    {
        /// <summary>
        /// Map ID where the fishing spot is located.
        /// </summary>
        public int MapId { get; set; }

        /// <summary>
        /// X coordinate of the fishing spot.
        /// </summary>
        public short X { get; set; }

        /// <summary>
        /// Y coordinate of the fishing spot.
        /// </summary>
        public short Y { get; set; }

        /// <summary>
        /// Direction the player should face when fishing.
        /// </summary>
        public short Direction { get; set; }

        /// <summary>
        /// Region/Zone ID for drop table association.
        /// </summary>
        public int RegionId { get; set; }

        /// <summary>
        /// Minimum fishing level required to use this spot.
        /// </summary>
        public int MinLevel { get; set; }
    }
}
