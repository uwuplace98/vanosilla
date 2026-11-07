using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.AdminRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServerController : ControllerBase
    {
        private readonly ISessionManager _sessionManager;

        public ServerController(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        /// <summary>
        /// Broadcast shout message to all players
        /// </summary>
        [HttpPost("shout")]
        public IActionResult BroadcastShout([FromBody] ShoutRequest request)
        {
            string message = request.Message;
            int count = 0;

            foreach (var session in _sessionManager.Sessions.Where(s => s?.PlayerEntity != null))
            {
                session.SendChatMessage($"[ADMIN]: {message}", ChatMessageColorType.Yellow);
                count++;
            }

            return Ok(new
            {
                success = true,
                message = "Shout broadcasted",
                recipients = count
            });
        }

        /// <summary>
        /// Send announcement popup to all players
        /// </summary>
        [HttpPost("announce")]
        public IActionResult SendAnnouncement([FromBody] AnnounceRequest request)
        {
            int count = 0;

            foreach (var session in _sessionManager.Sessions.Where(s => s?.PlayerEntity != null))
            {
                session.SendInformationChatMessage(request.Message);
                count++;
            }

            return Ok(new
            {
                success = true,
                message = "Announcement sent",
                recipients = count
            });
        }

        /// <summary>
        /// Get server statistics
        /// </summary>
        [HttpGet("stats")]
        public IActionResult GetServerStats()
        {
            var sessions = _sessionManager.Sessions.ToList();

            return Ok(new
            {
                onlinePlayers = sessions.Count(s => s?.PlayerEntity != null),
                totalSessions = sessions.Count,
                uptime = System.Diagnostics.Process.GetCurrentProcess().StartTime,
                memoryUsageMB = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024
            });
        }

        /// <summary>
        /// Kick all players (for maintenance)
        /// </summary>
        [HttpPost("kick-all")]
        public async Task<IActionResult> KickAll([FromBody] KickAllRequest request)
        {
            string message = request.Message ?? "Server maintenance in progress";
            int count = 0;

            // Send warning first
            foreach (var session in _sessionManager.Sessions.Where(s => s != null).ToList())
            {
                session.SendInformationChatMessage(message);
            }

            // Wait 5 seconds
            await Task.Delay(5000);

            // Kick all
            foreach (var session in _sessionManager.Sessions.Where(s => s != null).ToList())
            {
                session.ForceDisconnect();
                count++;
            }

            return Ok(new
            {
                success = true,
                message = "All players kicked",
                count
            });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("/api/health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = System.DateTime.UtcNow
            });
        }
    }

    public class ShoutRequest
    {
        public string Message { get; set; }
    }

    public class AnnounceRequest
    {
        public string Message { get; set; }
        public string Type { get; set; } = "middle"; // "top", "middle", "white"
    }

    public class KickAllRequest
    {
        public string Message { get; set; }
    }
}
