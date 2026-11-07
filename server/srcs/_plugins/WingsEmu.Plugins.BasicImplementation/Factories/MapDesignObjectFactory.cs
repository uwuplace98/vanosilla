using System;
using WingsAPI.Data.Miniland;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.Factories;

public interface IMapDesignObjectFactory
{
    MapDesignObject CreateGameObject(long characterId, CharacterMinilandObjectDto dto);

    MapDesignObject CreateTalentArenaGameObject(long characterId, short itemVNum, byte slot, short x, short y);

}

public class MapDesignObjectFactory : IMapDesignObjectFactory
{
    private readonly ISessionManager _sessionManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;

    public MapDesignObjectFactory(ISessionManager sessionManager, IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _sessionManager = sessionManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    public MapDesignObject CreateGameObject(long characterId, CharacterMinilandObjectDto dto)
    {
        IClientSession session = _sessionManager.GetSessionByCharacterId(characterId);

        InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(dto.InventorySlot, InventoryType.Miniland);
        if (item == null)
        {
            return null;
        }

        return new MapDesignObject
        {
            Id = dto.Id,
            CharacterId = characterId,
            InventorySlot = item.Slot,
            InventoryItem = item,
            Level1BoxAmount = dto.Level1BoxAmount,
            Level2BoxAmount = dto.Level2BoxAmount,
            Level3BoxAmount = dto.Level3BoxAmount,
            Level4BoxAmount = dto.Level4BoxAmount,
            Level5BoxAmount = dto.Level5BoxAmount,
            MapX = dto.MapX,
            MapY = dto.MapY
        };
    }

    public MapDesignObject CreateTalentArenaGameObject(long characterId, short itemVNum, byte slot, short x, short y)
    {
        GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(itemVNum, 1);
        newItem.Design = itemVNum;
        var item = new InventoryItem
        {
            ItemInstance = newItem,
        };

        return new MapDesignObject
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            InventorySlot = slot,
            InventoryItem = item,
            MapX = x,
            MapY = y
        };
    }
}