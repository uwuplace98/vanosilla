using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Icebreaker;

namespace WingsEmu.ClusterScheduler.Service
{
    public class IcebreakerCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<IcebreakerStartMessage> _messagePublisher;

        public IcebreakerCronScheduler(IMessagePublisher<IcebreakerStartMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task Invoke()
        {
            await _messagePublisher.PublishAsync(new IcebreakerStartMessage(), CancellationToken.None);
        }
    }
}
