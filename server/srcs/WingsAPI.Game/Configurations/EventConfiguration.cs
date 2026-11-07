using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class EventConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool ExpEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool UpgradeEqEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool GambleEqEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool UpgradeSpEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool UpgradePerfectionEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool FamilyExpEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool GoldEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool ReputationEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool DropEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool RuneEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool TattooEvent { get; set; }
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool BonusRaidBoxEvent { get; set; }
}