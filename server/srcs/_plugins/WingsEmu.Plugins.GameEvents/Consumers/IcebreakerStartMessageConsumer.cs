using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Icebreaker;
using WingsEmu.Game.GameEvent;
using WingsEmu.Plugins.GameEvents.Event.Global;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class IcebreakerStartMessageConsumer : IMessageConsumer<IcebreakerStartMessage>
    {
        private readonly IAsyncEventPipeline _eventPipeline;

        public IcebreakerStartMessageConsumer(IAsyncEventPipeline eventPipeline) => _eventPipeline = eventPipeline;

        public async Task HandleAsync(IcebreakerStartMessage notification, CancellationToken token)
        {
            await _eventPipeline.ProcessEventAsync(new GameEventPrepareEvent(GameEventType.Icebreaker));
        }
    }
}
