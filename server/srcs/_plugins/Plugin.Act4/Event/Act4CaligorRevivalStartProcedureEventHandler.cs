using PhoenixLib.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4.Event
{
    public class Act4CaligorRevivalStartProcedureEventHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
    {
        private readonly IGameLanguageService _languageService;
        private readonly GameMinMaxConfiguration _minMaxConfiguration;
        private readonly IRankingManager _rankingManager;
        private readonly IReputationConfiguration _reputationConfiguration;
        private readonly PlayerRevivalConfiguration _revivalConfiguration;
        private readonly ISessionManager _sessionManager;

        public Act4CaligorRevivalStartProcedureEventHandler(GameRevivalConfiguration gameRevivalConfiguration,
            GameMinMaxConfiguration minMaxConfiguration, IGameLanguageService languageService, ISessionManager sessionManager, IReputationConfiguration reputationConfiguration,
            IRankingManager rankingManager)
        {
            _minMaxConfiguration = minMaxConfiguration;
            _languageService = languageService;
            _sessionManager = sessionManager;
            _reputationConfiguration = reputationConfiguration;
            _rankingManager = rankingManager;
            _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;
        }

        public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IBattleEntity killer = e.Killer;
            if (session.PlayerEntity.IsAlive())
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.Caligor)
            {
                return;
            }

            if (session.PlayerEntity.IsOnVehicle)
            {
                await session.EmitEventAsync(new RemoveVehicleEvent());
            }

            await session.PlayerEntity.RemoveBuffsOnDeathAsync();
            session.RefreshStat();

            if (killer?.Faction != session.PlayerEntity.Faction)
            {
                IPlayerEntity playerEntity = killer switch
                {
                    IPlayerEntity player => player,
                    IMateEntity mateEntity => mateEntity.Owner,
                    IMonsterEntity monsterEntity => monsterEntity.SummonerType is VisualType.Player && monsterEntity.SummonerId.HasValue
                        ? monsterEntity.MapInstance.GetCharacterById(monsterEntity.SummonerId.Value)
                        : null,
                    _ => null
                };

                if (playerEntity != null)
                {
                    switch (session.PlayerEntity.Faction)
                    {
                        case FactionType.Angel:
                            _sessionManager.Broadcast(x => session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_PVP_KILLER, x.UserLanguage,
                                playerEntity.Name), ChatMessageColorType.Green), new FactionBroadcast(FactionType.Demon));

                            _sessionManager.Broadcast(x => session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_PVP_DEATH, x.UserLanguage,
                                session.PlayerEntity.Name), ChatMessageColorType.Red), new FactionBroadcast(FactionType.Angel));

                            break;

                        case FactionType.Demon:

                            _sessionManager.Broadcast(x => session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_PVP_DEATH, x.UserLanguage,
                                session.PlayerEntity.Name), ChatMessageColorType.Red), new FactionBroadcast(FactionType.Demon));

                            _sessionManager.Broadcast(x => session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_PVP_KILLER, x.UserLanguage,
                                playerEntity.Name), ChatMessageColorType.Green), new FactionBroadcast(FactionType.Angel));

                            break;
                    }
                }
            }

            DateTime currentTime = DateTime.UtcNow;
            e.Sender.PlayerEntity.UpdateRevival(currentTime + _revivalConfiguration.Act4CaligorRevivalDelay, RevivalType.DontPayRevival, ForcedType.Reconnect);
            e.Sender.PlayerEntity.UpdateAskRevival(currentTime + _revivalConfiguration.RevivalDialogDelay, AskRevivalType.CaligorRevival);
        }
    }
}