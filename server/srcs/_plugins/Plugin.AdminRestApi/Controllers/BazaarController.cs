using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Communication;
using WingsAPI.Communication.Bazaar;
using WingsEmu.Game.Managers;

namespace WingsEmu.Plugins.AdminRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BazaarController : ControllerBase
    {
        private readonly IBazaarService _bazaarService;
        private readonly ISessionManager _sessionManager;

        public BazaarController(IBazaarService bazaarService, ISessionManager sessionManager)
        {
            _bazaarService = bazaarService;
            _sessionManager = sessionManager;
        }

        /// <summary>
        /// Search bazaar items (with optional filters)
        /// </summary>
        [HttpGet("items")]
        public async Task<IActionResult> SearchBazaarItems(
            [FromQuery] int? itemVnum = null,
            [FromQuery] int index = 0,
            [FromQuery] int pageSize = 50)
        {
            var searchContext = new BazaarSearchContext
            {
                Index = index,
                AmountOfItemsPerIndex = pageSize,
                ItemVNumFilter = itemVnum.HasValue ? new List<int> { itemVnum.Value } : new List<int>()
            };

            var response = await _bazaarService.SearchBazaarItems(new BazaarSearchBazaarItemsRequest
            {
                BazaarSearchContext = searchContext
            });

            if (response.ResponseType != RpcResponseType.SUCCESS || response.BazaarItemDtos == null)
            {
                return Ok(new
                {
                    success = true,
                    count = 0,
                    items = new List<object>()
                });
            }

            var items = response.BazaarItemDtos.Select(item => new
            {
                bazaarItemId = item.Id,
                characterId = item.CharacterId,
                itemVnum = item.ItemInstance?.ItemVNum ?? 0,
                amount = item.Amount,
                soldAmount = item.SoldAmount,
                pricePerItem = item.PricePerItem,
                totalPrice = item.PricePerItem * (item.Amount - item.SoldAmount),
                isPackage = item.IsPackage,
                usedMedal = item.UsedMedal,
                saleFee = item.SaleFee,
                upgrade = item.ItemInstance?.Upgrade ?? 0,
                rare = item.ItemInstance?.Rarity ?? 0
            }).ToList();

            return Ok(new
            {
                success = true,
                count = items.Count,
                items
            });
        }

        /// <summary>
        /// Get bazaar items by character name
        /// </summary>
        [HttpGet("items/seller/{characterName}")]
        public async Task<IActionResult> GetBazaarItemsBySeller(string characterName)
        {
            // Find character session to get ID
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            var response = await _bazaarService.GetItemsByCharacterIdFromBazaar(new BazaarGetItemsByCharIdRequest
            {
                CharacterId = session.PlayerEntity.Id
            });

            if (response.ResponseType != RpcResponseType.SUCCESS)
            {
                return BadRequest(new { error = "Failed to get bazaar items" });
            }

            var items = response.BazaarItems.Select(item => new
            {
                bazaarItemId = item.Id,
                itemVnum = item.ItemInstance?.ItemVNum ?? 0,
                amount = item.Amount,
                soldAmount = item.SoldAmount,
                pricePerItem = item.PricePerItem,
                totalPrice = item.PricePerItem * (item.Amount - item.SoldAmount),
                isPackage = item.IsPackage
            }).ToList();

            return Ok(new
            {
                success = true,
                characterName,
                characterId = session.PlayerEntity.Id,
                count = items.Count,
                items
            });
        }

        /// <summary>
        /// Get bazaar items by character ID
        /// </summary>
        [HttpGet("items/character/{characterId}")]
        public async Task<IActionResult> GetBazaarItemsByCharacterId(long characterId)
        {
            var response = await _bazaarService.GetItemsByCharacterIdFromBazaar(new BazaarGetItemsByCharIdRequest
            {
                CharacterId = characterId
            });

            if (response.ResponseType != RpcResponseType.SUCCESS)
            {
                return BadRequest(new { error = "Failed to get bazaar items" });
            }

            var items = response.BazaarItems.Select(item => new
            {
                bazaarItemId = item.Id,
                itemVnum = item.ItemInstance?.ItemVNum ?? 0,
                amount = item.Amount,
                soldAmount = item.SoldAmount,
                pricePerItem = item.PricePerItem,
                totalPrice = item.PricePerItem * (item.Amount - item.SoldAmount),
                isPackage = item.IsPackage
            }).ToList();

            return Ok(new
            {
                success = true,
                characterId,
                count = items.Count,
                items
            });
        }

        /// <summary>
        /// Remove all bazaar items from a character by name
        /// </summary>
        [HttpDelete("items/seller/{characterName}")]
        public async Task<IActionResult> RemoveBazaarItemsBySeller(string characterName)
        {
            // Find character session to get ID
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            var response = await _bazaarService.RemoveItemsByCharacterIdFromBazaar(new BazaarRemoveItemsByCharIdRequest
            {
                CharacterId = session.PlayerEntity.Id
            });

            if (response.ResponseType != RpcResponseType.SUCCESS)
            {
                return BadRequest(new { error = "Failed to remove bazaar items" });
            }

            return Ok(new
            {
                success = true,
                message = $"Removed all bazaar items for {characterName}",
                characterId = session.PlayerEntity.Id
            });
        }

        /// <summary>
        /// Remove all bazaar items from a character by ID
        /// </summary>
        [HttpDelete("items/character/{characterId}")]
        public async Task<IActionResult> RemoveBazaarItemsByCharacterId(long characterId)
        {
            var response = await _bazaarService.RemoveItemsByCharacterIdFromBazaar(new BazaarRemoveItemsByCharIdRequest
            {
                CharacterId = characterId
            });

            if (response.ResponseType != RpcResponseType.SUCCESS)
            {
                return BadRequest(new { error = "Failed to remove bazaar items" });
            }

            return Ok(new
            {
                success = true,
                message = $"Removed all bazaar items for character {characterId}",
                characterId
            });
        }

        /// <summary>
        /// Unlist bazaar items by item vnum
        /// </summary>
        [HttpPost("unlist-by-vnum")]
        public async Task<IActionResult> UnlistItemsByVnum([FromBody] UnlistByVnumRequest request)
        {
            var response = await _bazaarService.UnlistItemsFromBazaarWithVnumAsync(new UnlistItemFromBazaarRequest
            {
                Vnum = request.ItemVnums
            });

            return Ok(new
            {
                success = true,
                message = $"Unlisted {response.UnlistedItems} items",
                unlistedCount = response.UnlistedItems
            });
        }

        /// <summary>
        /// Unlist all bazaar items for a character by name
        /// </summary>
        [HttpPost("unlist-character/{characterName}")]
        public async Task<IActionResult> UnlistCharacterItems(string characterName)
        {
            // Find character session to get ID
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            var response = await _bazaarService.UnlistCharacterItemsFromBazaarAsync(new UnlistCharacterItemsFromBazaarRequest
            {
                Id = (int)session.PlayerEntity.Id
            });

            return Ok(new
            {
                success = true,
                message = $"Unlisted {response.UnlistedItems} items for {characterName}",
                unlistedCount = response.UnlistedItems
            });
        }

        /// <summary>
        /// Get bazaar service status
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "operational",
                message = "Bazaar management API is active",
                availableEndpoints = new[]
                {
                    "GET /api/bazaar/items - Search bazaar items with filters",
                    "GET /api/bazaar/items/seller/{name} - Get items by seller name",
                    "GET /api/bazaar/items/character/{id} - Get items by character ID",
                    "DELETE /api/bazaar/items/seller/{name} - Remove all items by seller name",
                    "DELETE /api/bazaar/items/character/{id} - Remove all items by character ID",
                    "POST /api/bazaar/unlist-by-vnum - Unlist items by item vnum",
                    "POST /api/bazaar/unlist-character/{name} - Unlist all character items"
                }
            });
        }
    }

    public class UnlistByVnumRequest
    {
        public List<int> ItemVnums { get; set; }
    }
}
