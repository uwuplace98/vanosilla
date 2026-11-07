using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.NpcDialogs;

public class OpenUpgradeWindowHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[]
    {
        NpcRunType.UPGRADE_FAIRIES,
        NpcRunType.FAIRY_ENCHANT,
        NpcRunType.SHELL_ENCHANT
    };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        int value = 0;
        if (e.NpcRunType == NpcRunType.SHELL_ENCHANT)
        {
            value = 117;
        }

        session.SendWopenPacket((byte)e.NpcRunType, 0, value);
        await Task.CompletedTask;
    }
}
