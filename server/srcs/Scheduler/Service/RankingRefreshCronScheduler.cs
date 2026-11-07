using Coravel.Invocable;
using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.ClusterScheduler.Ranking;

namespace WingsEmu.ClusterScheduler.Service
{
    //public class RankingRefreshCronScheduler : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        RecurringJob.AddOrUpdate<IAsyncEventPipeline>(
    //            "ranking-refresh",
    //            s => s.ProcessEventAsync(new RankingRefreshEvent(), CancellationToken.None),
    //            "1 0 * * *",
    //            TimeZoneInfo.Utc
    //        );
    //    }
    //}

    public class RankingRefreshCronScheduler : IInvocable
    {
        private readonly IAsyncEventPipeline _eventPipeline;

        public RankingRefreshCronScheduler(IAsyncEventPipeline eventPipeline)
        {
            _eventPipeline = eventPipeline;
        }

        public async Task Invoke()
        {
            await _eventPipeline.ProcessEventAsync(new RankingRefreshEvent(), CancellationToken.None);
        }
    }
}