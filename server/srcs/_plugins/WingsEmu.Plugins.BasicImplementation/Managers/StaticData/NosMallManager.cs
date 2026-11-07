using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsAPI.Data.NosMall;
using WingsEmu.Game.Managers.StaticData;

namespace WingsEmu.Plugins.BasicImplementations.Managers.StaticData
{
    public class NosMallManager : INosMallManager
    {
        private readonly IResourceLoader<NosMallItemDto> _loader;
        private readonly Dictionary<int, NosMallItemDto> _items = new();

        public NosMallManager(IResourceLoader<NosMallItemDto> loader)
        {
            _loader = loader;
        }

        public void Initialize()
        {
            IEnumerable<NosMallItemDto> items = _loader.LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            foreach (var item in items)
            {
                _items[item.Id] = item;
            }
            Log.Info($"[NOSMALL] Loaded {_items.Count} items.");
        }

        public NosMallItemDto GetItem(int id) => _items.GetValueOrDefault(id);

        public IEnumerable<NosMallItemDto> GetItems() => _items.Values;
    }
}
