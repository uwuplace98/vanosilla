using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Player;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class SpecialistPointsRefreshCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IMessagePublisher<SpecialistPointsRefreshMessage>>(
    //            "specialist-points-refresh",
    //            s => s.PublishAsync(new SpecialistPointsRefreshMessage(), CancellationToken.None),
    //            "1 0 * * *", // hardcoded cron for the moment
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class SpecialistPointsRefreshCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<SpecialistPointsRefreshMessage> _messagePublisher;

        public SpecialistPointsRefreshCronScheduler(IMessagePublisher<SpecialistPointsRefreshMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task Invoke()
        {
            await _messagePublisher.PublishAsync(new SpecialistPointsRefreshMessage
            {
                Force = true
            }, CancellationToken.None);
        }
    }
}