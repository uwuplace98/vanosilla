using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Raid;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class RaidRestrictionRefreshCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IMessagePublisher<RaidRestrictionRefreshMessage>>(
    //            "raid-restriction-refresh",
    //            s => s.PublishAsync(new RaidRestrictionRefreshMessage(), CancellationToken.None),
    //            "1 0 * * *", // hardcoded cron for the moment
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class RaidRestrictionRefreshCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<RaidRestrictionRefreshMessage> _messagePublisher;

        public RaidRestrictionRefreshCronScheduler(IMessagePublisher<RaidRestrictionRefreshMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task Invoke()
        {
            await _messagePublisher.PublishAsync(new RaidRestrictionRefreshMessage() /*{ Force = true }*/, CancellationToken.None);
        }
    }
}