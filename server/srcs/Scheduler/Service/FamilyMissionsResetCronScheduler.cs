using Coravel.Invocable;
using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Families;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class FamilyMissionsResetCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IMessagePublisher<FamilyMissionsResetMessage>>(
    //            "family-missions-reset",
    //            s => s.PublishAsync(new FamilyMissionsResetMessage(), CancellationToken.None),
    //            "1 0 * * *",
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class FamilyMissionsResetCronScheduler : IInvocable
    {
        private readonly IMessagePublisher<FamilyMissionsResetMessage> _messagePublisher;

        public FamilyMissionsResetCronScheduler(IMessagePublisher<FamilyMissionsResetMessage> messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task Invoke()
        {
            await _messagePublisher.PublishAsync(new FamilyMissionsResetMessage
            {
                //Force = true
            }, CancellationToken.None);
        }
    }
}