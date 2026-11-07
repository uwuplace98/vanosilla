using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Plugin.Icebreaker.Configuration;
using WingsEmu.Game.Managers; // IIcebreakerManager
using WingsEmu.Game.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Icebreaker.Event.Icebreaker
{
    public class IcebreakerSystem : BackgroundService
    {
        private readonly IcebreakerManager _manager;
        private readonly IcebreakerConfiguration _config;
        private readonly IMapManager _mapManager;
        private readonly IGameLanguageService _language;

        // Simple scheduling: check every second
        private readonly TimeSpan _tickRate = TimeSpan.FromSeconds(1);

        public IcebreakerSystem(IIcebreakerManager manager, IcebreakerConfiguration config, IMapManager mapManager, IGameLanguageService language)
        {
            _manager = manager as IcebreakerManager; // Cast to concrete to access internal methods? Or expose Process()?
            _config = config;
            _mapManager = mapManager;
            _language = language;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Logic to start event if not running
                    // Logic to match InstantBattle schedules could be complex.
                    // For now, let's assume we start one manually or periodically.

                    // For MVP: Let's focus on processing existing instances first.
                    // The Manager should probably have a "Process" method.

                    _manager?.Process();
                }
                catch (Exception ex)
                {
                    // Log error
                    Console.WriteLine($"[Icebreaker] Error in loop: {ex.Message}");
                }

                await Task.Delay(_tickRate, stoppingToken);
            }
        }
    }
}
