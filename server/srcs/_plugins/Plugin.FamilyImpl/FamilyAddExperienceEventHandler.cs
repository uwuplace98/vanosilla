using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;

namespace Plugin.FamilyImpl
{
    public class FamilyAddExperienceEventHandler : IAsyncEventProcessor<FamilyAddExperienceEvent>
    {
        private readonly IFamilyManager _familyManager;
        private readonly EventConfiguration _eventConfiguration;

        public FamilyAddExperienceEventHandler(IFamilyManager familyManager, EventConfiguration eventConfiguration)
        {
            _familyManager = familyManager;
            _eventConfiguration = eventConfiguration;
        }

        public async Task HandleAsync(FamilyAddExperienceEvent e, CancellationToken cancellation) =>
            _familyManager.SendExperienceToFamilyServer(new ExperienceGainedSubMessage(e.Sender.PlayerEntity.Id, _eventConfiguration.FamilyExpEvent ? e.ExperienceGained * 2 : e.ExperienceGained, e.FamXpObtainedFromType, DateTime.UtcNow));
    }
}