using Coravel.Pro.Features.Metrics;
using System.Threading.Tasks;
using WingsAPI.Communication;
using WingsAPI.Communication.Player;
using WingsEmu.Game.Managers;

namespace WingsEmu.ClusterScheduler.Metrics
{
    public class OnlineCountMetric : IMetricCard
    {
        private readonly ISessionManager _sessionManager;
        private readonly IClusterCharacterService _clusterCharacterService;


        // Inject dbContext from service provider.
        public OnlineCountMetric(ISessionManager sessionManager, IClusterCharacterService clusterCharacterService)
        {
            _sessionManager = sessionManager;
            _clusterCharacterService = clusterCharacterService;

        }

        // Fetch and assign values in this method.  
        public async Task ExecuteAsync()
        {
            this.Value = $"{GetOnlinePlayerCount()} Players";
            this.Title = "Online Players";
            this.IsPositiveMetric = true;
            this.IncludeArrow = true;
        }

        private async Task<int> GetOnlinePlayerCount()
        {
            try
            {
                var allCharacters = await _clusterCharacterService.GetAllCharacters(new EmptyRpcRequest());
                return allCharacters.ResponseType == RpcResponseType.SUCCESS
                       ? allCharacters.ClusterCharacterInfo.Count
                       : 0;
            }
            catch
            {
                return 0;
            }
        }

        // These are implemented from IMetric Card.
        public string Value { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public bool IsPositiveMetric { get; set; }
        public bool IncludeArrow { get; set; }
    }
}

