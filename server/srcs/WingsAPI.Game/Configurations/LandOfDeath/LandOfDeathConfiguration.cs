namespace WingsEmu.Game.Configurations;

public class LandOfDeathConfiguration
{
    public byte MinLevel { get; init; }
    public int MapId { get; init; }
    public int ExitMapId { get; init; }
    public int NpcPortalVnum { get; init; }
    public LandOfDeathPosition SpawnPos { get; init; }
    public LandOfDeathPosition ExitPos { get; init; }
    public int LodLifeTimeSeconds { get; init; }
    public int ClosePortalTimeSeconds { get; init; }
    public int DhVnum { get; init; }
    public int DhSpawnTimeSeconds { get; init; }
    public int DhLifeSeconds { get; init; }
}

public class LandOfDeathPosition
{
    public short X { get; init; }
    public short Y { get; init; }
}