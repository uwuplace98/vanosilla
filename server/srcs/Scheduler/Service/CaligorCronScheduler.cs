using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Caligor;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class CaligorCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IMessagePublisher<CaligorStartMessage>>(
    //            "caligor.start",
    //            s => s.PublishAsync(new CaligorStartMessage(), CancellationToken.None),
    //            "0 17,20 * * 6,0", // hardcoded cron for the moment (17 and 20 on Saturday and Sunday)
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class CaligorCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<CaligorStartMessage> _messagePublisher;

        public CaligorCronScheduler(IMessagePublisher<CaligorStartMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public Task Invoke()
        {
            return _messagePublisher.PublishAsync(new CaligorStartMessage
            {
            }, CancellationToken.None);
        }
    }
}