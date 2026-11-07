using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.AdminRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterController : ControllerBase
    {
        private readonly ISessionManager _sessionManager;
        private readonly ICharacterAlgorithm _characterAlgorithm;

        public CharacterController(ISessionManager sessionManager, ICharacterAlgorithm characterAlgorithm)
        {
            _sessionManager = sessionManager;
            _characterAlgorithm = characterAlgorithm;
        }

        /// <summary>
        /// Set character level
        /// </summary>
        [HttpPost("level")]
        public IActionResult SetLevel([FromBody] SetLevelRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            byte oldLevel = session.PlayerEntity.Level;
            byte oldJobLevel = session.PlayerEntity.JobLevel;
            byte oldHeroLevel = session.PlayerEntity.HeroLevel;

            // Set levels
            if (request.Level.HasValue && request.Level.Value != oldLevel)
            {
                session.PlayerEntity.Level = request.Level.Value;
                session.PlayerEntity.LevelXp = 0; // Reset XP when setting level
            }

            if (request.JobLevel.HasValue && request.JobLevel.Value != oldJobLevel)
            {
                session.PlayerEntity.JobLevel = request.JobLevel.Value;
                session.PlayerEntity.JobLevelXp = 0;
            }

            if (request.HeroLevel.HasValue && request.HeroLevel.Value != oldHeroLevel)
            {
                session.PlayerEntity.HeroLevel = request.HeroLevel.Value;
                session.PlayerEntity.HeroXp = 0;
            }

            // Refresh character display
            session.RefreshLevel(_characterAlgorithm);
            session.RefreshStatChar();
            session.SendChatMessage("Your levels have been updated by an administrator!", ChatMessageColorType.Green);

            return Ok(new
            {
                success = true,
                message = $"Levels updated for {request.CharacterName}",
                changes = new
                {
                    level = new { old = oldLevel, @new = session.PlayerEntity.Level },
                    jobLevel = new { old = oldJobLevel, @new = session.PlayerEntity.JobLevel },
                    heroLevel = new { old = oldHeroLevel, @new = session.PlayerEntity.HeroLevel }
                }
            });
        }

        /// <summary>
        /// Add experience points
        /// </summary>
        [HttpPost("experience")]
        public IActionResult AddExperience([FromBody] AddExperienceRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            long oldXp = session.PlayerEntity.LevelXp;
            long oldJobXp = session.PlayerEntity.JobLevelXp;
            long oldHeroXp = session.PlayerEntity.HeroXp;

            // Add experience
            if (request.Experience.HasValue && request.Experience.Value != 0)
            {
                session.PlayerEntity.LevelXp += request.Experience.Value;
                if (session.PlayerEntity.LevelXp < 0) session.PlayerEntity.LevelXp = 0;
            }

            if (request.JobExperience.HasValue && request.JobExperience.Value != 0)
            {
                session.PlayerEntity.JobLevelXp += request.JobExperience.Value;
                if (session.PlayerEntity.JobLevelXp < 0) session.PlayerEntity.JobLevelXp = 0;
            }

            if (request.HeroExperience.HasValue && request.HeroExperience.Value != 0)
            {
                session.PlayerEntity.HeroXp += request.HeroExperience.Value;
                if (session.PlayerEntity.HeroXp < 0) session.PlayerEntity.HeroXp = 0;
            }

            // Refresh display
            session.RefreshLevel(_characterAlgorithm);
            session.SendChatMessage("You received experience from an administrator!", ChatMessageColorType.Green);

            return Ok(new
            {
                success = true,
                message = $"Experience added for {request.CharacterName}",
                changes = new
                {
                    experience = new { old = oldXp, @new = session.PlayerEntity.LevelXp, added = request.Experience ?? 0 },
                    jobExperience = new { old = oldJobXp, @new = session.PlayerEntity.JobLevelXp, added = request.JobExperience ?? 0 },
                    heroExperience = new { old = oldHeroXp, @new = session.PlayerEntity.HeroXp, added = request.HeroExperience ?? 0 }
                }
            });
        }

        /// <summary>
        /// Modify character reputation (compliment count)
        /// </summary>
        [HttpPost("reputation")]
        public IActionResult ModifyReputation([FromBody] ModifyReputationRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            int oldCompliment = session.PlayerEntity.Compliment;

            switch (request.Operation?.ToLower())
            {
                case "add":
                    session.PlayerEntity.Compliment += (short)request.Amount;
                    break;
                case "remove":
                    session.PlayerEntity.Compliment = (short)Math.Max(0, session.PlayerEntity.Compliment - (short)request.Amount);
                    break;
                case "set":
                    session.PlayerEntity.Compliment = (short)request.Amount;
                    break;
                default:
                    return BadRequest(new { error = "Invalid operation. Use 'add', 'remove', or 'set'" });
            }

            session.RefreshStatInfo();
            session.SendChatMessage("Your reputation has been modified by an administrator!", ChatMessageColorType.Green);

            return Ok(new
            {
                success = true,
                message = $"Reputation {request.Operation}ed for {request.CharacterName}",
                oldCompliment,
                newCompliment = session.PlayerEntity.Compliment,
                change = session.PlayerEntity.Compliment - oldCompliment
            });
        }

        /// <summary>
        /// Modify character dignity
        /// </summary>
        [HttpPost("dignity")]
        public IActionResult ModifyDignity([FromBody] ModifyDignityRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            float oldDignity = session.PlayerEntity.Dignity;

            switch (request.Operation?.ToLower())
            {
                case "add":
                    session.PlayerEntity.Dignity += request.Amount;
                    break;
                case "remove":
                    session.PlayerEntity.Dignity -= request.Amount;
                    break;
                case "set":
                    session.PlayerEntity.Dignity = request.Amount;
                    break;
                default:
                    return BadRequest(new { error = "Invalid operation. Use 'add', 'remove', or 'set'" });
            }

            // Clamp dignity between -1000 and 100
            if (session.PlayerEntity.Dignity > 100) session.PlayerEntity.Dignity = 100;
            if (session.PlayerEntity.Dignity < -1000) session.PlayerEntity.Dignity = -1000;

            session.RefreshStatInfo();
            session.SendChatMessage("Your dignity has been modified by an administrator!", ChatMessageColorType.Green);

            return Ok(new
            {
                success = true,
                message = $"Dignity {request.Operation}ed for {request.CharacterName}",
                oldDignity = (int)oldDignity,
                newDignity = (int)session.PlayerEntity.Dignity,
                change = (int)(session.PlayerEntity.Dignity - oldDignity)
            });
        }

        /// <summary>
        /// Modify character HP/MP
        /// </summary>
        [HttpPost("health")]
        public IActionResult ModifyHealth([FromBody] ModifyHealthRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            int oldHp = session.PlayerEntity.Hp;
            int oldMp = session.PlayerEntity.Mp;

            if (request.Hp.HasValue)
            {
                session.PlayerEntity.Hp = Math.Clamp(request.Hp.Value, 0, session.PlayerEntity.MaxHp);
            }

            if (request.Mp.HasValue)
            {
                session.PlayerEntity.Mp = Math.Clamp(request.Mp.Value, 0, session.PlayerEntity.MaxMp);
            }

            if (request.RestoreFull == true)
            {
                session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
                session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
            }

            session.RefreshStat();

            return Ok(new
            {
                success = true,
                message = $"Health modified for {request.CharacterName}",
                hp = new { old = oldHp, @new = session.PlayerEntity.Hp, max = session.PlayerEntity.MaxHp },
                mp = new { old = oldMp, @new = session.PlayerEntity.Mp, max = session.PlayerEntity.MaxMp }
            });
        }

        /// <summary>
        /// Change character faction
        /// </summary>
        [HttpPost("faction")]
        public IActionResult ChangeFaction([FromBody] ChangeFactionRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            var oldFaction = session.PlayerEntity.Faction;

            FactionType newFaction = request.Faction.ToLower() switch
            {
                "angel" => FactionType.Angel,
                "demon" => FactionType.Demon,
                "neutral" => FactionType.Neutral,
                _ => FactionType.Neutral
            };

            session.PlayerEntity.SetFaction(newFaction);
            session.RefreshStatInfo();
            session.SendChatMessage($"Your faction has been changed to {newFaction} by an administrator!", ChatMessageColorType.Green);

            return Ok(new
            {
                success = true,
                message = $"Faction changed for {request.CharacterName}",
                oldFaction = oldFaction.ToString(),
                newFaction = newFaction.ToString()
            });
        }

        /// <summary>
        /// Get character full stats
        /// </summary>
        [HttpGet("stats/{characterName}")]
        public IActionResult GetCharacterStats(string characterName)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            return Ok(new
            {
                success = true,
                character = new
                {
                    name = session.PlayerEntity.Name,
                    level = session.PlayerEntity.Level,
                    jobLevel = session.PlayerEntity.JobLevel,
                    heroLevel = session.PlayerEntity.HeroLevel,
                    experience = session.PlayerEntity.LevelXp,
                    jobExperience = session.PlayerEntity.JobLevelXp,
                    heroExperience = session.PlayerEntity.HeroXp,
                    hp = session.PlayerEntity.Hp,
                    maxHp = session.PlayerEntity.MaxHp,
                    mp = session.PlayerEntity.Mp,
                    maxMp = session.PlayerEntity.MaxMp,
                    gold = session.PlayerEntity.Gold,
                    reputation = session.PlayerEntity.Reput,
                    dignity = (int)session.PlayerEntity.Dignity,
                    compliment = session.PlayerEntity.Compliment,
                    faction = session.PlayerEntity.Faction.ToString(),
                    @class = session.PlayerEntity.Class.ToString(),
                    gender = session.PlayerEntity.Gender.ToString(),
                    mapId = session.PlayerEntity.MapInstance?.MapId ?? 0,
                    position = new { x = session.PlayerEntity.PositionX, y = session.PlayerEntity.PositionY }
                }
            });
        }
    }

    public class SetLevelRequest
    {
        public string CharacterName { get; set; }
        public byte? Level { get; set; }
        public byte? JobLevel { get; set; }
        public byte? HeroLevel { get; set; }
    }

    public class AddExperienceRequest
    {
        public string CharacterName { get; set; }
        public long? Experience { get; set; }
        public long? JobExperience { get; set; }
        public long? HeroExperience { get; set; }
    }

    public class ModifyReputationRequest
    {
        public string CharacterName { get; set; }
        public string Operation { get; set; } // "add", "remove", "set"
        public long Amount { get; set; }
    }

    public class ModifyDignityRequest
    {
        public string CharacterName { get; set; }
        public string Operation { get; set; } // "add", "remove", "set"
        public float Amount { get; set; }
    }

    public class ModifyHealthRequest
    {
        public string CharacterName { get; set; }
        public int? Hp { get; set; }
        public int? Mp { get; set; }
        public bool? RestoreFull { get; set; }
    }

    public class ChangeFactionRequest
    {
        public string CharacterName { get; set; }
        public string Faction { get; set; } // "angel", "demon", "neutral"
    }
}
