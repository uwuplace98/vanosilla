using PhoenixLib.Events;
using WingsEmu.Game.Arena.Events;
using WingsEmu.Game.Arena.TalentArena;

namespace Plugin.TalentArena.EventHandlers;

public class TalentArenaStartEventHandler : IAsyncEventProcessor<TalentArenaStartEvent>
{
    private readonly ITalentArenaManager _talentArenaManager;

    public TalentArenaStartEventHandler(ITalentArenaManager talentArenaManager) => _talentArenaManager = talentArenaManager;

    public async Task HandleAsync(TalentArenaStartEvent e, CancellationToken cancellation)
    {
        _talentArenaManager.TalentArenaProcessTime = DateTime.UtcNow;
        _talentArenaManager.IsActive = true;
    }
}