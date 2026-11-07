using PhoenixLib.ServiceBus;
using WingsAPI.Communication.LandOfDeath;
using WingsEmu.Game.LandOfDeath;

namespace Plugin.LandOfDeath.Managers;

public class LandOfDeathStartMessageConsumer : IMessageConsumer<LandOfDeathStartMessage>
{
    private readonly ILandOfDeathManager _landOfDeathManager;

    public LandOfDeathStartMessageConsumer(ILandOfDeathManager landOfDeathManager) => _landOfDeathManager = landOfDeathManager;

    public async Task HandleAsync(LandOfDeathStartMessage notification, CancellationToken token)
    {
        _landOfDeathManager.LandOfDeathProcessTime = DateTime.UtcNow;
    }
}