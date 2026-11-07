using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.InstantBattle;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class InstantBattleCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IMessagePublisher<InstantBattleStartMessage>>(
    //            "instant-battle",
    //            s => s.PublishAsync(new InstantBattleStartMessage(), CancellationToken.None),
    //            "55 1-23/2 * * *", // hardcoded cron for the moment
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class InstantBattleCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<InstantBattleStartMessage> _messagePublisher;

        public InstantBattleCronScheduler(IMessagePublisher<InstantBattleStartMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task Invoke()
        {
            await _messagePublisher.PublishAsync(new InstantBattleStartMessage(), CancellationToken.None);
        }
    }
}