using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.RainbowBattle;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class RainbowBattleCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IMessagePublisher<RainbowBattleStartMessage>>(
    //            "rainbow-battle",
    //            s => s.PublishAsync(new RainbowBattleStartMessage(), CancellationToken.None),
    //            "55 */2 * * *", // hardcoded cron for the moment
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class RainbowBattleCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<RainbowBattleStartMessage> _messagePublisher;

        public RainbowBattleCronScheduler(IMessagePublisher<RainbowBattleStartMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task Invoke()
        {
            await _messagePublisher.PublishAsync(new RainbowBattleStartMessage(), CancellationToken.None);
        }
    }
}