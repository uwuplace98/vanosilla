namespace WingsEmu.Game.Helpers.Damages;

public class DamageAlgorithmResult
{
    public DamageAlgorithmResult(int damages, HitType hitMode, bool onyxEffect, bool softDamageEffect, bool zephyrEffect)
    {
        Damages = damages;
        HitType = hitMode;
        OnyxEffect = onyxEffect;
        SoftDamageEffect = softDamageEffect;
        ZephyrEffect = zephyrEffect;
    }

    public bool SoftDamageEffect { get; }
    public int Damages { get; set; }
    public HitType HitType { get; }
    public bool OnyxEffect { get; }
    public bool ZephyrEffect { get; }
}

public enum HitType
{
    Normal,
    Critical,
    Miss
}