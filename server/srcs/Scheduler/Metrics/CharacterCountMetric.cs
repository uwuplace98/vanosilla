using Coravel.Pro.Features.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Database.DB;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace WingsEmu.ClusterScheduler.Metrics
{
    public class CharacterCountMetric : IMetricCard
    {
        private readonly IServiceProvider _container;

        // Inject dbContext from service provider.
        public CharacterCountMetric(IServiceProvider container)
        {
            _container = container;
        }

        // Fetch and assign values in this method.  
        public async Task ExecuteAsync()
        {
            using GameContext dbcontext = _container.GetRequiredService<IDbContextFactory<GameContext>>().CreateDbContext();

            int count = await dbcontext.Character.CountAsync();
            this.Value = $"{count} Characters";
            this.Title = "Character Count";
            this.IsPositiveMetric = true;
            this.IncludeArrow = true;
        }

        // These are implemented from IMetric Card.
        public string Value { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public bool IsPositiveMetric { get; set; }
        public bool IncludeArrow { get; set; }
    }
}
