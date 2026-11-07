using System.Collections.Generic;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Arena.TalentArena;

public class TalentArenaTeam
{
    public List<IClientSession> Members { get; } = new();
    public int Score { get; set; }
    public int Calls { get; set; }
}