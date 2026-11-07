using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;

namespace WingsEmu.Plugins.AdminRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly ISessionManager _sessionManager;
        private readonly IGameItemInstanceFactory _itemFactory;
        private readonly IItemsManager _itemsManager;

        public ItemController(
            ISessionManager sessionManager,
            IGameItemInstanceFactory itemFactory,
            IItemsManager itemsManager)
        {
            _sessionManager = sessionManager;
            _itemFactory = itemFactory;
            _itemsManager = itemsManager;
        }

        /// <summary>
        /// Give item to character's inventory
        /// </summary>
        [HttpPost("give")]
        public async Task<IActionResult> GiveItem([FromBody] GiveItemRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            // Validate item exists
            var itemData = _itemsManager.GetItem(request.ItemVnum);
            if (itemData == null)
            {
                return BadRequest(new { error = $"Item vnum {request.ItemVnum} does not exist" });
            }

            // Create item instance
            GameItemInstance itemInstance;
            if (request.Upgrade.HasValue && request.Rare.HasValue)
            {
                itemInstance = _itemFactory.CreateItem(request.ItemVnum, request.Amount, request.Upgrade.Value, request.Rare.Value);
            }
            else if (request.Upgrade.HasValue)
            {
                itemInstance = _itemFactory.CreateItem(request.ItemVnum, request.Amount, request.Upgrade.Value);
            }
            else
            {
                itemInstance = _itemFactory.CreateItem(request.ItemVnum, request.Amount);
            }

            if (itemInstance == null)
            {
                return BadRequest(new { error = "Failed to create item instance" });
            }

            // Add to inventory
            var inventoryItem = await session.AddNewItemToInventory(itemInstance, showMessage: true, colorType: ChatMessageColorType.Green);

            if (inventoryItem == null)
            {
                return BadRequest(new
                {
                    error = "Failed to add item to inventory. Inventory might be full."
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Gave {request.Amount}x {itemData.Name} to {request.CharacterName}",
                item = new
                {
                    vnum = request.ItemVnum,
                    name = itemData.Name,
                    amount = request.Amount,
                    upgrade = request.Upgrade ?? 0,
                    rare = request.Rare ?? 0,
                    slot = inventoryItem.Slot,
                    inventoryType = inventoryItem.InventoryType.ToString()
                }
            });
        }

        /// <summary>
        /// Remove item from character's inventory
        /// </summary>
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveItem([FromBody] RemoveItemRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            // Find item in inventory
            var inventoryItem = session.PlayerEntity.GetFirstItemByVnum(request.ItemVnum);

            if (inventoryItem == null)
            {
                return NotFound(new { error = $"Character does not have item vnum {request.ItemVnum}" });
            }

            short amountToRemove = (short)(request.Amount ?? inventoryItem.ItemInstance.Amount);

            // Remove the item
            await session.RemoveItemFromInventory(
                itemVnum: request.ItemVnum,
                amount: amountToRemove
            );

            return Ok(new
            {
                success = true,
                message = $"Removed {amountToRemove}x item vnum {request.ItemVnum} from {request.CharacterName}",
                removed = amountToRemove
            });
        }

        /// <summary>
        /// View character's inventory
        /// </summary>
        [HttpGet("inventory/{characterName}")]
        public IActionResult GetInventory(string characterName, [FromQuery] string type = null)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            var inventory = session.PlayerEntity.GetAllPlayerInventoryItems();

            // Filter by type if specified
            if (!string.IsNullOrEmpty(type))
            {
                InventoryType inventoryType = type.ToLower() switch
                {
                    "equipment" => InventoryType.Equipment,
                    "main" => InventoryType.Main,
                    "etc" => InventoryType.Etc,
                    "specialist" => InventoryType.Specialist,
                    "costume" => InventoryType.Costume,
                    _ => InventoryType.Main
                };

                inventory = inventory.Where(i => i.InventoryType == inventoryType).ToList();
            }

            var items = inventory.Select(i => new
            {
                slot = i.Slot,
                inventoryType = i.InventoryType.ToString(),
                vnum = i.ItemInstance.ItemVNum,
                amount = i.ItemInstance.Amount,
                upgrade = i.ItemInstance.Upgrade,
                rare = i.ItemInstance.Rarity,
                isBound = i.ItemInstance.BoundCharacterId.HasValue
            }).ToList();

            return Ok(new
            {
                success = true,
                characterName,
                count = items.Count,
                items
            });
        }

        /// <summary>
        /// Search for items in database
        /// </summary>
        [HttpGet("search")]
        public IActionResult SearchItems([FromQuery] string name = null, [FromQuery] int? vnum = null)
        {
            if (string.IsNullOrEmpty(name) && !vnum.HasValue)
            {
                return BadRequest(new { error = "Provide either 'name' or 'vnum' parameter" });
            }

            if (vnum.HasValue)
            {
                var item = _itemsManager.GetItem(vnum.Value);
                if (item == null)
                {
                    return NotFound(new { error = $"Item vnum {vnum} not found" });
                }

                return Ok(new
                {
                    success = true,
                    count = 1,
                    items = new[]
                    {
                        new
                        {
                            vnum = item.Id,
                            name = item.Name,
                            type = item.Type.ToString(),
                            equipmentSlot = item.EquipmentSlot.ToString(),
                            levelMin = item.LevelMinimum,
                            classType = item.Class,
                            isSoldable = item.IsSoldable,
                            isTradable = item.IsTradable,
                            isDroppable = item.IsDroppable
                        }
                    }
                });
            }
            else
            {
                var foundItems = _itemsManager.GetItem(name);
                var items = foundItems
                    .Take(50) // Limit results
                    .Select(item => new
                    {
                        vnum = item.Id,
                        name = item.Name,
                        type = item.Type.ToString(),
                        equipmentSlot = item.EquipmentSlot.ToString()
                    })
                    .ToList();

                return Ok(new
                {
                    success = true,
                    count = items.Count,
                    items,
                    note = foundItems.Count > 50 ? "Results limited to 50 items" : null
                });
            }
        }

        /// <summary>
        /// Get detailed item information
        /// </summary>
        [HttpGet("{vnum}")]
        public IActionResult GetItemDetails(int vnum)
        {
            var item = _itemsManager.GetItem(vnum);
            if (item == null)
            {
                return NotFound(new { error = $"Item vnum {vnum} not found" });
            }

            return Ok(new
            {
                success = true,
                item = new
                {
                    vnum = item.Id,
                    name = item.Name,
                    type = item.Type.ToString(),
                    equipmentSlot = item.EquipmentSlot.ToString(),
                    itemSubType = item.ItemSubType,
                    levelMin = item.LevelMinimum,
                    classType = item.Class,
                    gender = item.Sex,
                    maxElementRate = item.MaxElementRate,
                    isSoldable = item.IsSoldable,
                    isTradable = item.IsTradable,
                    isDroppable = item.IsDroppable,
                    isHeroic = item.IsHeroic,
                    isColorable = item.IsColorable,
                    effect = item.Effect,
                    effectValue = item.EffectValue,
                    morph = item.Morph,
                    speed = item.Speed,
                    price = item.Price,
                    reputationPrice = item.ReputPrice,
                    reputationMinimum = item.ReputationMinimum
                }
            });
        }
    }

    public class GiveItemRequest
    {
        public string CharacterName { get; set; }
        public int ItemVnum { get; set; }
        public int Amount { get; set; } = 1;
        public byte? Upgrade { get; set; }
        public sbyte? Rare { get; set; }
    }

    public class RemoveItemRequest
    {
        public string CharacterName { get; set; }
        public int ItemVnum { get; set; }
        public int? Amount { get; set; } // null = remove all
    }
}
