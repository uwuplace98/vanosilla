using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.TalentArena;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class TalentArenaCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IMessagePublisher<TalentArenaStartMessage>>(
    //            "talent-arena",
    //            s => s.PublishAsync(new TalentArenaStartMessage(), CancellationToken.None),
    //            "55 */2 * * *", // hardcoded cron for the moment
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class TalentArenaCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<TalentArenaStartMessage> _messagePublisher;

        public TalentArenaCronScheduler(IMessagePublisher<TalentArenaStartMessage> messagePublisher) => _messagePublisher = messagePublisher;

        public async Task Invoke()
        {
            await _messagePublisher.PublishAsync(new TalentArenaStartMessage(), CancellationToken.None);
        }
    }
}