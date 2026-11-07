using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.TalentArena;
using WingsEmu.Game.Arena.Events;

namespace Plugin.TalentArena.Managers;

public class TalentArenaStartMessageConsumer : IMessageConsumer<TalentArenaStartMessage>
{
    private readonly IAsyncEventPipeline _eventPipeline;

    public TalentArenaStartMessageConsumer(IAsyncEventPipeline eventPipeline) => _eventPipeline = eventPipeline;

    public async Task HandleAsync(TalentArenaStartMessage notification, CancellationToken token)
    {
        await _eventPipeline.ProcessEventAsync(new TalentArenaStartEvent());
    }
}