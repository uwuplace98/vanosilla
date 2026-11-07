using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.GameEvents.Configuration.InstantBattle;
using WingsEmu.Plugins.GameEvents.Event.InstantBattle;

namespace WingsEmu.Plugins.GameEvents.InstantBattle
{
    public class InstantBattleDropEventHandler : IAsyncEventProcessor<InstantBattleDropEvent>
    {
        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly IGameItemInstanceFactory _gameItemInstance;

        public InstantBattleDropEventHandler(IAsyncEventPipeline eventPipeline, IGameItemInstanceFactory gameItemInstance)
        {
            _eventPipeline = eventPipeline;
            _gameItemInstance = gameItemInstance;
        }

        public async Task HandleAsync(InstantBattleDropEvent e, CancellationToken cancellation)
        {
            IMapInstance map = e.Instance.MapInstance;

            if (0 < e.Instance.MapInstance.GetAliveMonsters(x => x.IsInstantBattle).Count || e.Wave.Drops == null || e.Wave.Drops.Count < 1)
            {
                return;
            }

            foreach (IClientSession session in map.Sessions)
            {
                if (session.PlayerEntity.DealtDamages < e.Instance.InternalConfiguration.RequiredDamages)
                {
                    continue;
                }

                foreach (InstantBattleDrop drop in e.Wave.Drops)
                {
                    if (drop.ItemVnum == (short)ItemVnums.GOLD)
                    {
                        await session.EmitEventAsync(new GenerateGoldEvent(drop.AmountPerBunch));
                    }
                    else
                    {
                        GameItemInstance itemInstance = _gameItemInstance.CreateItem(drop.ItemVnum, drop.AmountPerBunch);
                        await session.AddNewItemToInventory(itemInstance, true);
                    }
                }
            }
        }
    }
}