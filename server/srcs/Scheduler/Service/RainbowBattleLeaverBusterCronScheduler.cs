using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.RainbowBattle;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class RainbowBattleLeaverBusterCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IMessagePublisher<RainbowBattleLeaverBusterResetMessage>>(
    //            "rainbow-battle-leaver-buster-reset",
    //            s => s.PublishAsync(new RainbowBattleLeaverBusterResetMessage
    //            {
    //                Force = true
    //            }, CancellationToken.None),
    //            "1 0 1 * *",
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class RainbowBattleLeaverBusterCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<RainbowBattleLeaverBusterResetMessage> _messagePublisher;

        public RainbowBattleLeaverBusterCronScheduler(IMessagePublisher<RainbowBattleLeaverBusterResetMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task Invoke()
        {
            await _messagePublisher.PublishAsync(new RainbowBattleLeaverBusterResetMessage
            {
                Force = true
            }, CancellationToken.None);
        }
    }
}