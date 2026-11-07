using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Miniland;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class MinigameProductionRefreshCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IMessagePublisher<MinigameRefreshProductionPointsMessage>>(
    //            "minigame-refresh-free-production-points",
    //            s => s.PublishAsync(new MinigameRefreshProductionPointsMessage(), CancellationToken.None),
    //            "1 0 * * *", // hardcoded cron for the moment
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class MinigameProductionRefreshCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<MinigameRefreshProductionPointsMessage> _messagePublisher;

        public MinigameProductionRefreshCronScheduler(IMessagePublisher<MinigameRefreshProductionPointsMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task Invoke()
        {
            await _messagePublisher.PublishAsync(new MinigameRefreshProductionPointsMessage { Force = true }, CancellationToken.None);
        }
    }
}