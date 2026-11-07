using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Caligor;
using WingsEmu.Game.Act4.Event;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class CaligorStartMessageConsumer : IMessageConsumer<CaligorStartMessage>
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;

        public CaligorStartMessageConsumer(IAsyncEventPipeline asyncEventPipeline) => _asyncEventPipeline = asyncEventPipeline;

        public async Task HandleAsync(CaligorStartMessage notification, CancellationToken token)
        {
            await _asyncEventPipeline.ProcessEventAsync(new Act4CaligorStartEvent());
        }
    }
}
