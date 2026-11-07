using System.Collections.Generic;
using WingsAPI.Data.NosMall;

namespace WingsEmu.Game.Managers.StaticData
{
    public interface INosMallManager
    {
        void Initialize();
        NosMallItemDto GetItem(int id);
        IEnumerable<NosMallItemDto> GetItems();
    }
}
