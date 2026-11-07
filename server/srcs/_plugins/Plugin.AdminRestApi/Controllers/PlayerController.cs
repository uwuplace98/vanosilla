using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;

namespace WingsEmu.Plugins.AdminRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly ISessionManager _sessionManager;
        private readonly IMapManager _mapManager;

        public PlayerController(ISessionManager sessionManager, IMapManager mapManager)
        {
            _sessionManager = sessionManager;
            _mapManager = mapManager;
        }

        /// <summary>
        /// Get all online players
        /// </summary>
        [HttpGet("online")]
        public IActionResult GetOnlinePlayers()
        {
            var players = _sessionManager.Sessions
                .Where(s => s?.PlayerEntity != null)
                .Select(s => new
                {
                    characterId = s.PlayerEntity.Id,
                    characterName = s.PlayerEntity.Name,
                    accountId = s.Account.Id,
                    level = s.PlayerEntity.Level,
                    @class = (int)s.PlayerEntity.Class,
                    gender = (int)s.PlayerEntity.Gender,
                    mapId = s.PlayerEntity.MapInstance?.MapId ?? 0,
                    positionX = s.PlayerEntity.PositionX,
                    positionY = s.PlayerEntity.PositionY,
                    gold = s.PlayerEntity.Gold,
                    jobLevel = s.PlayerEntity.JobLevel,
                    heroLevel = s.PlayerEntity.HeroLevel,
                    dignity = (int)s.PlayerEntity.Dignity,
                    reputation = s.PlayerEntity.Compliment,
                    hp = s.PlayerEntity.Hp,
                    maxHp = s.PlayerEntity.MaxHp,
                    mp = s.PlayerEntity.Mp,
                    maxMp = s.PlayerEntity.MaxMp
                })
                .ToList();

            return Ok(new
            {
                count = players.Count,
                players
            });
        }

        /// <summary>
        /// Get specific player by name
        /// </summary>
        [HttpGet("{characterName}")]
        public IActionResult GetPlayer(string characterName)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Player not found or offline" });
            }

            return Ok(new
            {
                characterId = session.PlayerEntity.Id,
                characterName = session.PlayerEntity.Name,
                accountId = session.Account.Id,
                level = session.PlayerEntity.Level,
                @class = (int)session.PlayerEntity.Class,
                gender = (int)session.PlayerEntity.Gender,
                mapId = session.PlayerEntity.MapInstance?.MapId ?? 0,
                positionX = session.PlayerEntity.PositionX,
                positionY = session.PlayerEntity.PositionY,
                gold = session.PlayerEntity.Gold,
                jobLevel = session.PlayerEntity.JobLevel,
                heroLevel = session.PlayerEntity.HeroLevel,
                dignity = (int)session.PlayerEntity.Dignity,
                reputation = session.PlayerEntity.Compliment
            });
        }

        /// <summary>
        /// Kick player from server
        /// </summary>
        [HttpPost("kick")]
        public IActionResult KickPlayer([FromBody] KickPlayerRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Player not found or offline" });
            }

            // Disconnect the player
            session.ForceDisconnect();

            return Ok(new
            {
                success = true,
                message = $"Player {request.CharacterName} has been kicked"
            });
        }

        /// <summary>
        /// Teleport player to coordinates
        /// </summary>
        [HttpPost("teleport")]
        public IActionResult TeleportPlayer([FromBody] TeleportPlayerRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Player not found or offline" });
            }

            var map = _mapManager.GetBaseMapInstanceByMapId(request.MapId);
            if (map == null)
            {
                return BadRequest(new { error = "Invalid map ID" });
            }

            // Teleport player
            session.ChangeMap(request.MapId, (short)request.X, (short)request.Y);

            return Ok(new
            {
                success = true,
                message = $"Player {request.CharacterName} teleported to map {request.MapId} ({request.X}, {request.Y})"
            });
        }

        /// <summary>
        /// Modify player gold
        /// </summary>
        [HttpPost("modify-gold")]
        public IActionResult ModifyPlayerGold([FromBody] ModifyGoldRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Player not found or offline" });
            }

            long oldGold = session.PlayerEntity.Gold;
            long newGold = oldGold;

            switch (request.Operation?.ToLower())
            {
                case "add":
                    newGold = oldGold + request.Amount;
                    break;
                case "remove":
                    newGold = Math.Max(0, oldGold - request.Amount);
                    break;
                case "set":
                    newGold = request.Amount;
                    break;
                default:
                    return BadRequest(new { error = "Invalid operation. Use 'add', 'remove', or 'set'" });
            }

            session.PlayerEntity.Gold = newGold;
            session.RefreshGold();

            return Ok(new
            {
                success = true,
                message = $"Gold {request.Operation}ed for {request.CharacterName}",
                oldGold,
                newGold,
                change = newGold - oldGold
            });
        }
    }

    public class KickPlayerRequest
    {
        public string CharacterName { get; set; }
    }

    public class TeleportPlayerRequest
    {
        public string CharacterName { get; set; }
        public int MapId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class ModifyGoldRequest
    {
        public string CharacterName { get; set; }
        public string Operation { get; set; } // "add", "remove", "set"
        public long Amount { get; set; }
    }
}
