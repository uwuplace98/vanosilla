using System.Collections.Generic;
using WingsEmu.DTOs.Skills;

namespace WingsEmu.Plugins.PacketHandling.Customization;

public class BaseBrawlerSkill
{
    public BaseBrawlerSkill() => Skills = new List<CharacterSkillDTO>
    {
        new() { SkillVNum = 1525 },
        new() { SkillVNum = 1529 },
        new() { SkillVNum = 1526 },
        new() { SkillVNum = 1527 },
        new() { SkillVNum = 1528 },
        new() { SkillVNum = 1530 },
        new() { SkillVNum = 1531 },
        new() { SkillVNum = 1532 },
        new() { SkillVNum = 1533 },
        new() { SkillVNum = 1534 },
        new() { SkillVNum = 1535 },
        new() { SkillVNum = 1536 },
        new() { SkillVNum = 1537 },
        new() { SkillVNum = 1538 },
        new() { SkillVNum = 1539 },
        new() { SkillVNum = 1539 },
        new() { SkillVNum = 209 }
    };

    public List<CharacterSkillDTO> Skills { get; set; }
}