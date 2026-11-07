using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.AdminRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiftController : ControllerBase
    {
        private readonly ISessionManager _sessionManager;

        public GiftController(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        /// <summary>
        /// Send gold to player
        /// </summary>
        [HttpPost("send-gold")]
        public IActionResult SendGold([FromBody] SendGoldRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Player not found or offline" });
            }

            // Player is online - give directly
            long oldGold = session.PlayerEntity.Gold;
            session.PlayerEntity.Gold = oldGold + request.Amount;
            session.RefreshGold();
            session.SendChatMessage($"You received {request.Amount} gold from Admin Panel!", ChatMessageColorType.Green);

            return Ok(new
            {
                success = true,
                message = $"Gold sent to online player {request.CharacterName}",
                oldGold,
                newGold = session.PlayerEntity.Gold
            });
        }

        /// <summary>
        /// Send gold to all online players
        /// </summary>
        [HttpPost("send-bulk-gold")]
        public IActionResult SendBulkGold([FromBody] SendBulkGoldRequest request)
        {
            int count = 0;

            foreach (var session in _sessionManager.Sessions.Where(s => s?.PlayerEntity != null))
            {
                session.PlayerEntity.Gold += request.Amount;
                session.RefreshGold();
                session.SendChatMessage($"You received {request.Amount} gold from Admin Panel!", ChatMessageColorType.Green);
                count++;
            }

            return Ok(new
            {
                success = true,
                message = $"Sent {request.Amount} gold to {count} players",
                recipients = count
            });
        }

        /// <summary>
        /// NOTE: Item gifting via mail requires complex gRPC service integration
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                message = "Gift system active - gold gifting available",
                note = "Item gifting via mail requires additional server-specific mail service implementation"
            });
        }
    }

    public class SendGoldRequest
    {
        public string CharacterName { get; set; }
        public long Amount { get; set; }
    }

    public class SendBulkGoldRequest
    {
        public long Amount { get; set; }
    }
}
