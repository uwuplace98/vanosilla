using System.Collections.Generic;
using System.Linq;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Packets.Enums.TalentArena;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Arena.TalentArena;

public static class TalentArenaExtensions
{
    public static string GenerateTaP(this IClientSession character, byte tatype, bool showOponent)
    {
        ITalentArenaComponent member = character.PlayerEntity.TalentArenaComponent;
        ArenaTeamType type = member.TeamType;
        string groups = "";

        TalentArenaTeam characterTeam = type == ArenaTeamType.ERENIA ? member.Instance.EreniaTeam : member.Instance.ZenasTeam;
        TalentArenaTeam enemyTeam = type == ArenaTeamType.ERENIA ? member.Instance.ZenasTeam : member.Instance.EreniaTeam;
        for (int i = 0; i < 3; i++)
        {
            if (characterTeam.Members.FirstOrDefault(s => s.PlayerEntity.TalentArenaComponent.Order != -1 && s.PlayerEntity.TalentArenaComponent.Order == i) is { } memberSession)
            {
                groups +=
                    $"{(memberSession.PlayerEntity.TalentArenaComponent.IsDead ? 0 : 1)}.{memberSession.PlayerEntity.Id}.{(byte)memberSession.PlayerEntity.Class}.{(byte)memberSession.PlayerEntity.Gender}.{(byte)memberSession.PlayerEntity.Morph} ";
            }
            else
            {
                groups += "-1.-1.-1.-1.-1 ";
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (enemyTeam.Members.FirstOrDefault(s => s.PlayerEntity.TalentArenaComponent.Order != -1 && s.PlayerEntity.TalentArenaComponent.Order == i) is { } memberSession && showOponent)
            {
                groups +=
                    $"{(memberSession.PlayerEntity.TalentArenaComponent.IsDead ? 0 : 1)}.{memberSession.PlayerEntity.Id}.{(byte)memberSession.PlayerEntity.Class}.{(byte)memberSession.PlayerEntity.Gender}.{(byte)memberSession.PlayerEntity.Morph} ";
            }
            else
            {
                groups += "-1.-1.-1.-1.-1 ";
            }
        }

        return
            $"ta_p {tatype} {(byte)type} {5 - characterTeam.Calls} {5 - enemyTeam.Calls} {groups.TrimEnd(' ')}";

    }

    public static string GenerateTaPs(this IClientSession character)
    {
        ITalentArenaComponent member = character.PlayerEntity.TalentArenaComponent;
        TalentArenaTeam characterTeam = member.TeamType == ArenaTeamType.ERENIA ? member.Instance.EreniaTeam : member.Instance.ZenasTeam;
        TalentArenaTeam enemyTeam = member.TeamType == ArenaTeamType.ERENIA ? member.Instance.ZenasTeam : member.Instance.EreniaTeam;

        string groups = "";
        if (characterTeam == null)
        {
            return $"ta_ps {groups.TrimEnd(' ')}";
        }

        for (int i = 0; i < 3; i++)
        {
            if (characterTeam.Members.FirstOrDefault(s => s.PlayerEntity.TalentArenaComponent.Order == i) is { } arenaMember)
            {
                groups +=
                      $"{arenaMember.PlayerEntity.Id}.{arenaMember.PlayerEntity.HpPercentage}.{arenaMember.PlayerEntity.MpPercentage}.0 ";
            }
            else
            {
                groups += $"-1.-1.-1.-1.-1 ";
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (enemyTeam.Members.FirstOrDefault(s => s.PlayerEntity.TalentArenaComponent.Order == i) is { } arenaMember)
            {
                groups +=
                    $"{arenaMember.PlayerEntity.Id}.{arenaMember.PlayerEntity.HpPercentage}.{arenaMember.PlayerEntity.MpPercentage}.0 ";
            }
            else
            {
                groups += $"-1.-1.-1.-1.-1 ";
            }
        }

        return $"ta_ps {groups.TrimEnd(' ')}";
    }


    public static string GenerateTaM(this IClientSession member, int type, short? seconds = null)
    {
        ITalentArenaComponent arenaComponent = member.PlayerEntity.TalentArenaComponent;
        int score1 = arenaComponent.Instance.ZenasTeam.Score;
        int score2 = arenaComponent.Instance.EreniaTeam.Score;

        return $"ta_m {type} {score1} {score2} {(type == 3 ? seconds : 0)} 0";
    }

    public static string GenerateTaF(this IClientSession session, byte victoriousTeam)
    {
        ITalentArenaComponent talentArenaComponent = session.PlayerEntity.TalentArenaComponent;
        ArenaTeamType teamType = talentArenaComponent.TeamType;
        TalentArenaInstance instance = talentArenaComponent.Instance;
        TalentArenaTeam memberTeam = teamType == ArenaTeamType.ERENIA ? instance.EreniaTeam : instance.ZenasTeam;
        TalentArenaTeam enemyTeam = teamType == ArenaTeamType.ERENIA ? instance.ZenasTeam : instance.EreniaTeam;
        int score1 = memberTeam.Score;
        int score2 = enemyTeam.Score;
        int life1 = 3 - memberTeam.Members.Count(s => s.PlayerEntity.TalentArenaComponent.IsDead);
        int life2 = 3 - enemyTeam.Members.Count(s => s.PlayerEntity.TalentArenaComponent.IsDead);
        int call1 = 5 - memberTeam.Calls;
        int call2 = 5 - enemyTeam.Calls;

        return $"ta_f 0 {victoriousTeam} {(byte)teamType} {score1} {life1} {call1} {score2} {life2} {call2}";
    }

    public static string GenerateTaClose() => "ta_close";

    public static string GenerateTaSv(byte type) => $"ta_sv {type}";
    public static string GenerateTaSt(TalentArenaOptionType watch) => $"ta_st {(byte)watch}";
}