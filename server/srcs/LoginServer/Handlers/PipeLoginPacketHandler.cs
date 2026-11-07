using System;
using System.Linq;
using System.Threading.Tasks;
using LoginServer.Auth;
using LoginServer.Network;
using PhoenixLib.Logging;
using PhoenixLib.MultiLanguage;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;
using WingsAPI.Data.Account;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.DTOs.Account;
using WingsEmu.Health;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
namespace LoginServer.Handlers;

public class PipeLoginPacketHandler : GenericLoginPacketHandlerBase<Nos0577Packet>
{
    private readonly IAccountService _accountService;
    private readonly IMaintenanceManager _maintenanceManager;
    private readonly IServerApiService _serverApiService;
    private readonly ISessionService _sessionService;

    public PipeLoginPacketHandler(ISessionService sessionService, IServerApiService serverApiService, IMaintenanceManager maintenanceManager, IAccountService accountService)
    {
        _sessionService = sessionService;
        _serverApiService = serverApiService;
        _maintenanceManager = maintenanceManager;
        _accountService = accountService;
    }

    protected override async Task HandlePacketAsync(LoginClientSession session, Nos0577Packet nos0577Packet)
    {
        if (nos0577Packet == null)
        {
            return;
        }
        //but this solution should work. u can check it
        var split = nos0577Packet.ClientVersion.Split('\v');
        var region = (RegionLanguageType)byte.Parse(split[0]);
        var clientVersion = split[1];

        SessionResponse sessionResponse = null;
        try
        {
            sessionResponse = await _sessionService.GetSessionBySessionId(new GetSessionBySessionIdRequest
            {
                SessionId = nos0577Packet.SessionHex.DecodeHexString()
            });
        }
        catch (Exception e)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
            Log.Error("[NEW_PIPE_AUTH] Unexpected error: ", e);
        }
        if (sessionResponse?.ResponseType != RpcResponseType.SUCCESS)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
            Log.Debug("[NEW_PIPE_AUTH] Session not found: " + nos0577Packet.SessionHex.DecodeHexString());
            return;
        }
        if (sessionResponse.Session.State != SessionState.Disconnected)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.AlreadyConnected));
            Log.Debug("[NEW_PIPE_AUTH] Session is not disconnected: " + sessionResponse.Session.AccountName);
            return;
        }


        AccountLoadResponse accountLoadResponse = null;
        try
        {
            accountLoadResponse = await _accountService.LoadAccountByName(new AccountLoadByNameRequest
            {
                Name = sessionResponse.Session.AccountName
            });
        }
        catch (Exception e)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
            Log.Error("[NEW_PIPE_AUTH] Unexpected error: ", e);
        }

        if (accountLoadResponse?.ResponseType != RpcResponseType.SUCCESS)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
            Log.Debug("[NEW_PIPE_AUTH] Account not found: " + sessionResponse.Session.AccountName);
            return;
        }

        AccountDTO loadedAccount = accountLoadResponse.AccountDto;
        AccountBanGetResponse banResponse = null;
        try
        {
            banResponse = await _accountService.GetAccountBan(new AccountBanGetRequest
            {
                AccountId = loadedAccount.Id
            });
        }
        catch (Exception e)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
            Log.Error("[NEW_PIPE_AUTH] Unexpected error: ", e);
        }

        if (banResponse?.ResponseType != RpcResponseType.SUCCESS)
        {
            Log.Warn($"[NEW_PIPE_AUTH] Failed to get account ban for accountId: '{loadedAccount.Id.ToString()}'");
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
            session.Disconnect();
            return;
        }
        AccountBanDto characterPenalty = banResponse.AccountBanDto;
        if (characterPenalty != null)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.Banned));
            Log.Debug($"[NEW_PIPE_AUTH] ACCOUNT_BANNED : {loadedAccount.Name}");
            session.Disconnect();
            return;
        }

        switch (loadedAccount.Authority)
        {
            case AuthorityType.Banned:
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.Banned));
                Log.Debug("[NEW_PIPE_AUTH] ACCOUNT_BANNED");
                session.Disconnect();
                break;

            case AuthorityType.Unconfirmed:
            case AuthorityType.Closed:
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
                Log.Debug("[NEW_PIPE_AUTH] ACCOUNT_NOT_VERIFIED");
                session.Disconnect();
                break;

            default:
                if (_maintenanceManager.IsMaintenanceActive && loadedAccount.Authority < AuthorityType.GameMaster)
                {
                    session.SendPacket(session.GenerateFailcPacket(LoginFailType.Maintenance));
                    return;
                }
                SessionResponse connectResponse = await _sessionService.ConnectToLoginServer(new ConnectToLoginServerRequest
                {
                    AccountId = loadedAccount.Id,
                    ClientVersion = clientVersion,
                    HardwareId = nos0577Packet.Hwid
                });
                if (connectResponse.ResponseType != RpcResponseType.SUCCESS)
                {
                    Log.Warn("[NEW_PIPE_AUTH] General Error SessionId: " + session.Id);
                    session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
                    session.Disconnect();
                    return;
                }
                Session connectedSession = connectResponse.Session;
                Log.Debug($"[NEW_PIPE_AUTH] Connected : {loadedAccount.Name}:{connectedSession.EncryptionKey}:{connectedSession.HardwareId}");

                RetrieveRegisteredWorldServersResponse worldServersResponse = await _serverApiService.RetrieveRegisteredWorldServers(new RetrieveRegisteredWorldServersRequest
                {
                    RequesterAuthority = loadedAccount.Authority
                });

                if (worldServersResponse?.WorldServers is null || !worldServersResponse.WorldServers.Any())
                {
                    session.SendPacket(session.GenerateFailcPacket(LoginFailType.Maintenance));
                    session.Disconnect();
                    return;
                }

                session.SendChannelPacketList(connectedSession.EncryptionKey, loadedAccount.Name, region, worldServersResponse.WorldServers, false);
                session.Disconnect();
                break;
        }


    }
}