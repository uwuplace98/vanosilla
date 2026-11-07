using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Compliments;

namespace WingsEmu.ClusterScheduler.Service
{
    public class ComplimentsMonthlyRefreshCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<ComplimentsMonthlyRefreshMessage> _messagePublisher;

        public ComplimentsMonthlyRefreshCronScheduler(IMessagePublisher<ComplimentsMonthlyRefreshMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public Task Invoke()
        {
            return _messagePublisher.PublishAsync(new ComplimentsMonthlyRefreshMessage
            {
                Force = true
            }, CancellationToken.None);
        }
    }
}