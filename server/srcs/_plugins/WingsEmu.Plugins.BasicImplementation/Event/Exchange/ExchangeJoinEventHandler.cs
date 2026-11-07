using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Exchange.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.BasicImplementations.Event.Exchange;

public class ExchangeJoinEventHandler : IAsyncEventProcessor<ExchangeJoinEvent>
{
    private readonly IBankReputationConfiguration _bankReputationConfiguration;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;

    public ExchangeJoinEventHandler(IReputationConfiguration reputationConfiguration, IBankReputationConfiguration bankReputationConfiguration, IRankingManager rankingManager)
    {
        _reputationConfiguration = reputationConfiguration;
        _bankReputationConfiguration = bankReputationConfiguration;
        _rankingManager = rankingManager;
    }

    public async Task HandleAsync(ExchangeJoinEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IClientSession target = e.Target;

        if (session.PlayerEntity.IsInExchange())
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, "Tried to open exchange while being in exchange");
            return;
        }

        if (target.PlayerEntity.IsInExchange())
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, "Tried to open exchange with a target already in exchange");
            return;
        }

        var senderExchange = new PlayerExchange(session.PlayerEntity.Id, target.PlayerEntity.Id);
        var targetExchange = new PlayerExchange(target.PlayerEntity.Id, session.PlayerEntity.Id);

        session.PlayerEntity.SetExchange(senderExchange);
        target.PlayerEntity.SetExchange(targetExchange);

        session.SendEmptyExchangeWindow(target.PlayerEntity.Id);
        target.SendEmptyExchangeWindow(session.PlayerEntity.Id);

        session.SendGbexPacket(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
        target.SendGbexPacket(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
    }
}