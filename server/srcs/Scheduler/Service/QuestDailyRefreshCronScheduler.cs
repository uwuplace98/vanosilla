using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Quests;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class QuestDailyRefreshCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IMessagePublisher<QuestDailyRefreshMessage>>(
    //            "quest-daily-refresh",
    //            s => s.PublishAsync(new QuestDailyRefreshMessage
    //            {
    //                Force = true
    //            }, CancellationToken.None),
    //            "1 0 * * *", // hardcoded cron for the moment (every day at 12am)
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class QuestDailyRefreshCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<QuestDailyRefreshMessage> _messagePublisher;

        public QuestDailyRefreshCronScheduler(IMessagePublisher<QuestDailyRefreshMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task Invoke()
        {
            await _messagePublisher.PublishAsync(new QuestDailyRefreshMessage { Force = true }, CancellationToken.None);
        }
    }
}