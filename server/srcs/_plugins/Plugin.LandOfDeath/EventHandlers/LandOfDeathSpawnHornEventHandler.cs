using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.LandOfDeath.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.LandOfDeath.EventHandlers;

public class LandOfDeathSpawnHornEventHandler : IAsyncEventProcessor<LandOfDeathSpawnHornEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly LandOfDeathConfiguration _landOfDeathConfiguration;
    public LandOfDeathSpawnHornEventHandler(IGameLanguageService languageService, IAsyncEventPipeline eventPipeline, LandOfDeathConfiguration landOfDeathConfiguration)
    {
        _languageService = languageService;
        _eventPipeline = eventPipeline;
        _landOfDeathConfiguration = landOfDeathConfiguration;
    }

    public async Task HandleAsync(LandOfDeathSpawnHornEvent e, CancellationToken cancellation)
    {
        if (e.ShouldSpawn)
        {
            await SpawnDarkHorn(e.MapInstance);
            return;
        }

        DespawnDarkHorn(e.MapInstance);
    }

    private async Task SpawnDarkHorn(IMapInstance mapInstance)
    {
        IClientSession lastPlayerEntity = mapInstance.Sessions.MaxBy(s => s.PlayerEntity.RegisterMapInstanceTime);
        Position spawnPos = mapInstance.GetRandomPosition();
        ;

        if (lastPlayerEntity != null)
        {
            spawnPos = lastPlayerEntity.PlayerEntity.Position;
        }

        var summons = new List<ToSummon>();
        var summon = new ToSummon
        {
            VNum = _landOfDeathConfiguration.DhVnum,
            SpawnCell = spawnPos,
            IsMoving = true,
            IsHostile = true,
            Target = lastPlayerEntity?.PlayerEntity
        };

        summons.Add(summon);

        await _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(mapInstance, summons));

        mapInstance.Broadcast(x => x.GenerateDfPacket());
        mapInstance.Broadcast(x =>
            x.GenerateMsgPacket(_languageService.GetLanguage(GameDialogKey.LAND_OF_DEATH_HORN_APPEAR_MESSAGE, x.UserLanguage), MsgMessageType.Middle));
    }

    private void DespawnDarkHorn(IMapInstance mapInstance)
    {
        IMonsterEntity darkHorn = mapInstance.GetAliveMonsters(s => s.MonsterVNum.Equals(_landOfDeathConfiguration.DhVnum)).FirstOrDefault();

        if (darkHorn != null)
        {
            mapInstance.DespawnMonster(darkHorn);
            mapInstance.Broadcast(x => x.GenerateMsgPacket(_languageService.GetLanguage(GameDialogKey.LAND_OF_DEATH_HORN_HORN_DISAPPEAR_MESSAGE, x.UserLanguage), MsgMessageType.Middle));
        }
    }
}