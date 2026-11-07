using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Data.Account;
using WingsEmu.Game.Managers;

namespace WingsEmu.Plugins.AdminRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ISessionManager _sessionManager;
        private readonly IAccountService _accountService;

        public AccountController(ISessionManager sessionManager, IAccountService accountService)
        {
            _sessionManager = sessionManager;
            _accountService = accountService;
        }

        /// <summary>
        /// Get account information by character name
        /// </summary>
        [HttpGet("{characterName}")]
        public async Task<IActionResult> GetAccount(string characterName)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            var response = await _accountService.LoadAccountById(new AccountLoadByIdRequest
            {
                AccountId = session.Account.Id
            });

            if (response.ResponseType != RpcResponseType.SUCCESS || response.AccountDto == null)
            {
                return BadRequest(new { error = "Failed to load account" });
            }

            return Ok(new
            {
                success = true,
                account = new
                {
                    id = response.AccountDto.Id,
                    name = response.AccountDto.Name,
                    email = response.AccountDto.Email,
                    authority = response.AccountDto.Authority,
                    languageType = response.AccountDto.Language,
                    bankGold = response.AccountDto.BankMoney
                }
            });
        }

        /// <summary>
        /// Ban an account by character name
        /// </summary>
        [HttpPost("ban")]
        public async Task<IActionResult> BanAccount([FromBody] BanAccountRequest request)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            long accountId = session.Account.Id;
            DateTime startDate = DateTime.UtcNow;
            DateTime? endDate = request.DurationHours.HasValue
                ? startDate.AddHours(request.DurationHours.Value)
                : null; // null = permanent ban

            var banDto = new AccountBanDto
            {
                AccountId = accountId,
                JudgeName = request.AdminName ?? "Admin Panel",
                TargetName = session.Account.Name,
                Start = startDate,
                End = endDate,
                Reason = request.Reason ?? "Banned by admin"
            };

            var response = await _accountService.SaveAccountBan(new AccountBanSaveRequest
            {
                AccountBanDto = banDto
            });

            if (response.ResponseType != RpcResponseType.SUCCESS)
            {
                return BadRequest(new { error = "Failed to save ban" });
            }

            // Kick the player
            session.ForceDisconnect();

            return Ok(new
            {
                success = true,
                message = $"Account banned for {request.CharacterName}",
                ban = new
                {
                    accountId,
                    accountName = session.Account.Name,
                    startDate,
                    endDate,
                    duration = request.DurationHours.HasValue ? $"{request.DurationHours} hours" : "Permanent",
                    reason = request.Reason
                }
            });
        }

        /// <summary>
        /// Unban an account by character name
        /// </summary>
        [HttpPost("unban")]
        public async Task<IActionResult> UnbanAccount([FromBody] UnbanAccountRequest request)
        {
            // First try to find online character
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(request.CharacterName, StringComparison.OrdinalIgnoreCase));

            long accountId;
            string accountName;

            if (session != null)
            {
                accountId = session.Account.Id;
                accountName = session.Account.Name;
            }
            else
            {
                // If offline, we need to load character to get account ID
                // For now, return error - admin should provide account ID directly
                return NotFound(new
                {
                    error = "Character not online. Use /api/account/unban-by-id instead with account ID"
                });
            }

            // Get existing ban
            var banResponse = await _accountService.GetAccountBan(new AccountBanGetRequest
            {
                AccountId = accountId
            });

            if (banResponse.ResponseType != RpcResponseType.SUCCESS || banResponse.AccountBanDto == null)
            {
                return NotFound(new { error = "No ban found for this account" });
            }

            // Set end date to now to effectively unban
            banResponse.AccountBanDto.End = DateTime.UtcNow.AddSeconds(-1);

            var saveResponse = await _accountService.SaveAccountBan(new AccountBanSaveRequest
            {
                AccountBanDto = banResponse.AccountBanDto
            });

            if (saveResponse.ResponseType != RpcResponseType.SUCCESS)
            {
                return BadRequest(new { error = "Failed to unban account" });
            }

            return Ok(new
            {
                success = true,
                message = $"Account unbanned for {request.CharacterName}",
                accountId,
                accountName
            });
        }

        /// <summary>
        /// Unban an account by account ID
        /// </summary>
        [HttpPost("unban-by-id")]
        public async Task<IActionResult> UnbanAccountById([FromBody] UnbanByIdRequest request)
        {
            // Get existing ban
            var banResponse = await _accountService.GetAccountBan(new AccountBanGetRequest
            {
                AccountId = request.AccountId
            });

            if (banResponse.ResponseType != RpcResponseType.SUCCESS || banResponse.AccountBanDto == null)
            {
                return NotFound(new { error = "No ban found for this account" });
            }

            // Set end date to now to effectively unban
            banResponse.AccountBanDto.End = DateTime.UtcNow.AddSeconds(-1);

            var saveResponse = await _accountService.SaveAccountBan(new AccountBanSaveRequest
            {
                AccountBanDto = banResponse.AccountBanDto
            });

            if (saveResponse.ResponseType != RpcResponseType.SUCCESS)
            {
                return BadRequest(new { error = "Failed to unban account" });
            }

            return Ok(new
            {
                success = true,
                message = $"Account ID {request.AccountId} has been unbanned",
                accountId = request.AccountId
            });
        }

        /// <summary>
        /// Get account ban status
        /// </summary>
        [HttpGet("ban/{characterName}")]
        public async Task<IActionResult> GetBanStatus(string characterName)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            var banResponse = await _accountService.GetAccountBan(new AccountBanGetRequest
            {
                AccountId = session.Account.Id
            });

            if (banResponse.ResponseType != RpcResponseType.SUCCESS || banResponse.AccountBanDto == null)
            {
                return Ok(new
                {
                    success = true,
                    banned = false,
                    message = "Account is not banned"
                });
            }

            bool isCurrentlyBanned = banResponse.AccountBanDto.End == null ||
                                     banResponse.AccountBanDto.End > DateTime.UtcNow;

            return Ok(new
            {
                success = true,
                banned = isCurrentlyBanned,
                ban = new
                {
                    accountId = banResponse.AccountBanDto.AccountId,
                    judgeName = banResponse.AccountBanDto.JudgeName,
                    targetName = banResponse.AccountBanDto.TargetName,
                    startDate = banResponse.AccountBanDto.Start,
                    endDate = banResponse.AccountBanDto.End,
                    reason = banResponse.AccountBanDto.Reason,
                    isPermanent = banResponse.AccountBanDto.End == null,
                    remainingTime = banResponse.AccountBanDto.End.HasValue
                        ? (banResponse.AccountBanDto.End.Value - DateTime.UtcNow).ToString()
                        : "Permanent"
                }
            });
        }

        /// <summary>
        /// Get account penalties (mutes, warnings)
        /// </summary>
        [HttpGet("penalties/{characterName}")]
        public async Task<IActionResult> GetPenalties(string characterName)
        {
            var session = _sessionManager.Sessions.FirstOrDefault(s =>
                s?.PlayerEntity != null &&
                s.PlayerEntity.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return NotFound(new { error = "Character not found or offline" });
            }

            var response = await _accountService.GetAccountPenalties(new AccountPenaltyGetRequest
            {
                AccountId = session.Account.Id
            });

            if (response.ResponseType != RpcResponseType.SUCCESS)
            {
                return BadRequest(new { error = "Failed to get penalties" });
            }

            var penalties = response.AccountPenaltyDtos.Select(p => new
            {
                id = p.Id,
                accountId = p.AccountId,
                penaltyType = p.PenaltyType,
                reason = p.Reason,
                start = p.Start,
                remainingTime = p.RemainingTime,
                judgeName = p.JudgeName,
                targetName = p.TargetName
            }).ToList();

            return Ok(new
            {
                success = true,
                count = penalties.Count,
                penalties
            });
        }
    }

    public class BanAccountRequest
    {
        public string CharacterName { get; set; }
        public int? DurationHours { get; set; } // null = permanent
        public string Reason { get; set; }
        public string AdminName { get; set; }
    }

    public class UnbanAccountRequest
    {
        public string CharacterName { get; set; }
    }

    public class UnbanByIdRequest
    {
        public long AccountId { get; set; }
    }
}
