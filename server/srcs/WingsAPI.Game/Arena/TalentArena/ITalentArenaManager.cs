using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Arena.TalentArena;

public interface ITalentArenaManager
{
    bool IsActive { get; set; }
    bool IsRegistrationActive { get; set; }
    DateTime? TalentArenaProcessTime { get; set; }

    void AddRegisteredMember(IClientSession member);
    void ChangeGroupMemberState(long groupId, ArenaMemberStateType stateType);
    bool TryUnregisterLobbyMember(IClientSession member);
    List<IClientSession> GetRegisteredMembers(ArenaEventType eventType);
    List<IClientSession> GetRegisteredMembers(ArenaEventType eventType, Func<IClientSession, bool> predicate);

    void RemoveArenInstance(TalentArenaInstance instance);
    void AddArenaInstance(TalentArenaInstance instance);
    List<TalentArenaInstance> GetArenaInstances();
    IEnumerable<IClientSession> GetTeamMembers(TalentArenaInstance instance);
}

public class TalentArenaManager : ITalentArenaManager
{
    public bool IsActive { get; set; }

    public bool IsRegistrationActive { get; set; }

    public DateTime? TalentArenaProcessTime { get; set; }

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<ArenaEventType, List<IClientSession>> _registeredMembers = new();
    private readonly Dictionary<ArenaEventType, List<TalentArenaInstance>> _arenaInstances = new();

    public void AddRegisteredMember(IClientSession member)
    {
        if (_registeredMembers.TryGetValue(member.PlayerEntity.TalentArenaComponent.ArenaType, out List<IClientSession> gameEventInstances))
        {
            gameEventInstances.Add(member);
            return;
        }

        _registeredMembers[member.PlayerEntity.TalentArenaComponent.ArenaType] = new List<IClientSession>
        {
            member
        };
    }

    public List<IClientSession> GetRegisteredMembers(ArenaEventType eventType) => _registeredMembers.TryGetValue(eventType, out List<IClientSession> members) ? members : null;

    public List<IClientSession> GetRegisteredMembers(ArenaEventType eventType, Func<IClientSession, bool> predicate)
    {
        return _registeredMembers.TryGetValue(eventType, out List<IClientSession> members) ? members.FindAll(s => s != null && (predicate == null || predicate(s))) : null;
    }

    public bool TryUnregisterLobbyMember(IClientSession member)
    {
        if (!_registeredMembers.TryGetValue(member.PlayerEntity.TalentArenaComponent.ArenaType, out List<IClientSession> gameEventInstances))
        {
            return false;
        }

        gameEventInstances.Remove(member);
        return true;

    }

    public void ChangeGroupMemberState(long groupId, ArenaMemberStateType stateType)
    {
        foreach (IClientSession member in _registeredMembers[ArenaEventType.TalentArena].Where(s => s.PlayerEntity.TalentArenaComponent.GroupId == groupId))
        {
            member.PlayerEntity.TalentArenaComponent.Time = 300;
            member.SendBsInfoPacket(BsInfoType.OpenWindow, GameType.ArenaOfTalents, member.PlayerEntity.TalentArenaComponent.Time, QueueWindow.SearchOpposingTeam);
            member.PlayerEntity.TalentArenaComponent.StateType = stateType;
        }
    }

    public void AddArenaInstance(TalentArenaInstance instance)
    {
        if (_arenaInstances.TryGetValue(ArenaEventType.TalentArena, out List<TalentArenaInstance> gameEventInstances))
        {
            gameEventInstances.Add(instance);
            return;
        }

        _arenaInstances[ArenaEventType.TalentArena] = new List<TalentArenaInstance>
        {
            instance
        };
    }

    public List<TalentArenaInstance> GetArenaInstances() => _arenaInstances.TryGetValue(ArenaEventType.TalentArena, out List<TalentArenaInstance> instances) ? instances : null;

    public void RemoveArenInstance(TalentArenaInstance instance)
    {
        _arenaInstances[ArenaEventType.TalentArena].Remove(instance);
    }

    public IEnumerable<IClientSession> GetTeamMembers(TalentArenaInstance instance)
    {
        var members = new List<IClientSession>();
        if (!instance.ZenasTeam.Members.Any(s => s.PlayerEntity.TalentArenaComponent.IsInFightZone))
        {
            IClientSession zenasMember = instance.ZenasTeam.Members.FirstOrDefault(s => s.PlayerEntity.TalentArenaComponent.Order == (instance.Round - 1));
            if (zenasMember == null)
            {
                zenasMember = instance.ZenasTeam.Members.FirstOrDefault(s => s.PlayerEntity.TalentArenaComponent.Order == -1);
                zenasMember!.PlayerEntity.TalentArenaComponent.Order = instance.Round - 1;
            }

            zenasMember.PlayerEntity.TalentArenaComponent.IsInFightZone = true;
            members.Add(zenasMember);
        }

        if (!instance.EreniaTeam.Members.Any(s => s.PlayerEntity.TalentArenaComponent.IsInFightZone))
        {
            IClientSession ereniaMember = instance.EreniaTeam.Members.FirstOrDefault(s => s.PlayerEntity.TalentArenaComponent.Order == (instance.Round - 1));

            if (ereniaMember == null)
            {
                ereniaMember = instance.EreniaTeam.Members.FirstOrDefault(s => s.PlayerEntity.TalentArenaComponent.Order == -1);
                ereniaMember!.PlayerEntity.TalentArenaComponent.Order = instance.Round - 1;
            }
            ereniaMember.PlayerEntity.TalentArenaComponent.IsInFightZone = true;
            members.Add(ereniaMember);
        }

        return members;
    }
}