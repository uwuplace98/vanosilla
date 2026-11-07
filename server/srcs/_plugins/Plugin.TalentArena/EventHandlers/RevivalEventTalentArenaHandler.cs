using PhoenixLib.Events;
using WingsEmu.Game.Arena.TalentArena;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace Plugin.TalentArena.EventHandlers;

public class RevivalEventTalentArenaHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.TalentArenaComponent.IsInTalentArena)
        {
            return;
        }

        e.Sender.UpdateVisibility();
        await e.Sender.PlayerEntity.Restore(restoreMates: false);
        e.Sender.BroadcastRevive();
        e.Sender.SendBuffsPacket();

        foreach (IClientSession member in session.CurrentMapInstance.Sessions)
        {
            member.SendPacket(member.GenerateTaM(2));
            member.SendPacket(member.GenerateTaP(2, true));
            member.SendPacket($"taw_d {session.PlayerEntity.Id}");
            member.SendPacket(session.GenerateTaFcPacket(0));
        }
    }
}