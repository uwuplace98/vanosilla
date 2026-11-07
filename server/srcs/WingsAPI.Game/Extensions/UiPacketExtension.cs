using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WingsAPI.Data.Character;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.Act4;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Relations;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Shops;
using WingsEmu.Packets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Game.Extensions;

public static class UiPacketExtension
{
    #region Generate Packets

    // family
    
    public static string GenerateStpM(this IClientSession session) =>
        "stpm "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Evolve75DifferentPetFrom1And2StarTo5)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Evolve1PetTo6Star)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Evolve2PetTo6Star)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Evolve3PetTo6Star)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Evolve10PetTo6Star)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Evolve25PetTo6Star)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.ReachTrainingLevel60With200Pets)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.EvolveASandDwarfTo6Stars)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.EvolveAMothTo6Stars)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.EvolveHappyWoolyTo6Stars)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Own50Unique1StarPet)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Own50Unique2StarPet)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Own25Unique3StarPet)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Own10Unique4StarPet)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Own5Unique5StarPet)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Evolve25PetFrom1StarTo4)} "
        + $"{session.GetTrainerQuestAchievement(PetTrainerMissionType.Evolve40PetFrom2StarTo4)} "
        + "0 "
        + "0 "
        + "0";
    
    public static string GenerateStpS(this IClientSession session, IMateEntity mateEntity) => $"stp_s {mateEntity.MonsterVNum} {mateEntity.Stars} {mateEntity.HeroLevel}";

    public static string GeneratePetBasketPacket(this IClientSession session, bool isOn) => $"ib 1278 {(isOn ? 1 : 0)}";

    public static string GenerateQna(this IClientSession session, string packet, string message) => $"qna #{packet.Replace(' ', '^')} {message}";
    
    public static string GenerateQnai(this IClientSession session, string packet, Game18NConstString message) => $"qnai #{packet.Replace(' ', '^')} {(int)message} 0 0 0";

    public static string GenerateMapClear(this IMapInstance mapInstance) => "mapclear";

    public static string GenerateAct6EmptyPacket(this IClientSession session) => "act6";

    public static string GenerateEmptyRcScalc(this IClientSession session) => "rc_scalc 0 -1 -1 -1 -1 -1 ";

    public static string GenerateRcScalc(this IClientSession session, string name, byte type, long price, int amount, int bzAmount, long taxes, long priceTaxes)
        => $"rc_scalc {type} {price} {amount} {bzAmount} {taxes} {priceTaxes} {name ?? ""}";

    public static string GenerateBlinit(this IClientSession session)
    {
        string result = "blinit";

        foreach (CharacterRelationDTO relation in session.PlayerEntity.GetBlockedRelations())
        {
            result += $" {relation.RelatedCharacterId}|{relation.RelatedName}";
        }

        return result;
    }

    public static string GenerateFinit(this IClientSession session, ISessionManager sessionManager)
    {
        string result = "finit";

        foreach (CharacterRelationDTO relation in session.PlayerEntity.GetRelations().Where(x => x.RelationType != CharacterRelationType.Blocked))
        {
            bool isOnline = sessionManager.IsOnline(relation.RelatedCharacterId);
            result += $" {relation.RelatedCharacterId}|{(short)relation.RelationType}|{(isOnline ? 1 : 0)}|{relation.RelatedName}";
        }

        return result;
    }

    public static string GenerateDir(this IBattleEntity entity) => $"dir {(byte)entity.Type} {entity.Id} {entity.Direction}";
    public static string GenerateDamage(this IBattleEntity entity, int damage, DmType type) => $"dm {(byte)entity.Type} {entity.Id} {damage} {(byte)type}";
    public static string GenerateHeal(this IBattleEntity entity, int heal) => $"rc {(byte)entity.Type} {entity.Id} {heal} 0";
    public static string GenerateSmemo(this IClientSession session, SmemoType type, string message) => $"s_memo {(byte)type} {message}";

    public static string GenerateGb(this IClientSession session, BankType type, IReputationConfiguration reputationConfiguration, IBankReputationConfiguration bankReputationConfiguration,
        IReadOnlyList<CharacterDTO> topReputation) =>
        $"gb {(byte)type} {session.Account.BankMoney / 1000} {session.PlayerEntity.Gold} {(byte)session.PlayerEntity.GetBankRank(reputationConfiguration, bankReputationConfiguration, topReputation)} {session.PlayerEntity.GetBankPenalty(reputationConfiguration, bankReputationConfiguration, topReputation)}";

    public static string GenerateRcPacket(this IBattleEntity entity, int health) => $"rc {(byte)entity.Type} {entity.Id} {health} 0";
    public static string GenerateSpectatorWindow(this IClientSession session) => "taw_open";
    public static string GenerateMovement(this IBattleEntity entity) => $"mv {(byte)entity.Type} {entity.Id} {entity.PositionX} {entity.PositionY} {entity.Speed}";
    public static string GenerateEffectObject(this IBattleEntity entity, bool first, EffectType effect) => $"eff_ob {(byte)entity.Type} {entity.Id} {(first ? 1 : 0)} {(int)effect}";

    public static string GenerateEffectGround(this IBattleEntity entity, EffectType effectType, short x, short y, bool remove)
        => $"eff_g {(short)effectType} {entity.Id} {x} {y} {(remove ? 1 : 0)}";

    public static string GenerateEffectTarget(this IBattleEntity entity, IBattleEntity target, EffectType effectType)
        => $"eff_t {(byte)entity.Type} {entity.Id} {(byte)target.Type} {target.Id} {(short)effectType}";

    public static string GenerateSayPacket(this IClientSession session, string msg, ChatMessageColorType color) =>
        $"say {(byte)session.PlayerEntity.Type} {session.PlayerEntity.Id} {(byte)color} {msg}";

    public static string GenerateSayNoIdPacket(string msg, ChatMessageColorType color) =>
        $"say 1 -1 {((byte)color).ToString()} {msg}";

    public static string GenerateCancelPacket(this IClientSession session, CancelType cancelType, int id) => $"cancel {(byte)cancelType} {id} 1";

    public static string GenerateInfoPacket(this IClientSession session, string message) => $"info {message}";

    public static string GenerateMsgPacket(this IClientSession session, string message, MsgMessageType type) => $"msg {(byte)type} {message}";

    
    public static string GenerateInfoiPacket(this IClientSession session, Game18NConstString message, byte argumentType, int firstData, int secondData) => $"infoi {(int)message} {argumentType} {firstData} {secondData}";

    public static string GenerateModaliPacket(this IClientSession session, Game18NConstString message, byte argument, int firstData, int secondData) => $"modali 1 {(int)message} {argument} {firstData} {secondData}";

    public static string GenerateMsgiPacket(this IClientSession session, MsgiMessageType type, Game18NConstString message, byte argument, int value) => $"msgi {(byte)type} {(short)message} {argument} {value} 0 0 0";

    public static string GenerateSayiPacket(this IClientSession session, ChatMessageColorType color, Game18NConstString message, byte argumentType, int firstData, int secondData) => $"sayi {(byte)session.PlayerEntity.Type} {session.PlayerEntity.Id} {(byte)color} {(short)message} {argumentType} {firstData} {secondData} 0 0";

    public static string GenerateSpkPacket(this IClientSession session, string message, SpeakType type) => $"spk 1 {session.PlayerEntity.Id} {(byte)type} {session.PlayerEntity.Name} {message}";

    public static string GenerateSpkPacket(long senderId, string senderName, string message, SpeakType type) => $"spk 1 {senderId.ToString()} {(byte)type} {senderName} {message}";

    public static string GenerateGuriPacket(this IClientSession session, byte type, short argument = 0, long value = 0, int secondValue = 0)
    {
        switch (type)
        {
            case 2:
                return $"guri 2 {argument} {session.PlayerEntity.Id} 0";

            case 4:
                return $"guri 4 {session.PlayerEntity.AdditionalHp} {session.PlayerEntity.AdditionalMp}";

            case 6:
                return $"guri 6 {argument} {value} {secondValue} 0";

            case 10:
                return $"guri 10 {argument} {value} {session.PlayerEntity.Id}";

            case 12:
                return $"guri 12 1 {session.PlayerEntity.Id} {value} 0";

            case 15:
                return $"guri 15 {argument} 0 0";

            case (int)GuriType.ShellEffect:
                return $"guri {type} 0 0 {argument}";

            case 19:
                return $"guri 19 0 0 {value}";

            case 25:
                return "guri 25";

            default:
                return $"guri {type} {argument} {value} {session.PlayerEntity.Id}";
        }
    }

    public static string GenerateRestPacket(this IClientSession session) => $"rest 1 {session.PlayerEntity.Id} {(session.PlayerEntity.IsSitting ? 1 : 0)}";
    public static string GenerateDfPacket(this IClientSession session) => "df 2";

    public static string GenerateFcPacket(FactionType faction, Act4Status act4Status) =>
        $"fc {((byte)faction).ToString()} {((int)act4Status.TimeBeforeReset.TotalMinutes).ToString()} {GenerateSubFcPacket(FactionType.Angel, act4Status)} {GenerateSubFcPacket(FactionType.Demon, act4Status)}";

    public static string GenerateGuriFactionOverridePacket(this IClientSession session) =>
        $"guri 5 1 {session.PlayerEntity.Id} {(session.PlayerEntity.Faction == FactionType.Angel ? 3 : 4).ToString()}";

    public static string GenerateEndDancingGuriPacket(this IPlayerEntity playerEntity) => $"guri 6 1 {playerEntity.Id} 0 0";

    private static string GenerateSubFcPacket(FactionType faction, Act4Status act4Status)
    {
        if (faction == act4Status.RelevantFaction)
        {
            return $"{(faction == FactionType.Angel ? act4Status.AngelPointsPercentage : act4Status.DemonPointsPercentage).ToString()} " + //percentage
                $"{((byte)act4Status.FactionStateType).ToString()} " + //mode
                $"{((int)act4Status.CurrentTimeBeforeMukrajuDespawn.TotalSeconds).ToString()} " + //currentTime
                $"{((int)act4Status.TimeBeforeMukrajuDespawn.TotalSeconds).ToString()} " + //totalTime
                "0 " + //$"{(act4Status.DungeonType == DungeonType.Morcos ? 1 : 0).ToString()} " + //morcos
                "0 " + //$"{(act4Status.DungeonType == DungeonType.Hatus ? 1 : 0).ToString()} " + //hatus
                "0 " + //$"{(act4Status.DungeonType == DungeonType.Calvinas ? 1 : 0).ToString()} " +  //calvina
                "0 " + //$"{(act4Status.DungeonType == DungeonType.Berios ? 1 : 0).ToString()} " +  //berios
                "0"; //no idea
        }

        return $"{(faction == FactionType.Angel ? act4Status.AngelPointsPercentage : act4Status.DemonPointsPercentage).ToString()} 0 0 0 0 0 0 0 0";
    }

    public static string GenerateDungeonPacket(this IClientSession session, DungeonInstance dungeonInstance, DungeonSubInstance dungeonSubInstance, IAct4DungeonManager act4DungeonManager,
        DateTime currentTime)
    {
        DungeonEventType dungeonEventType = AssertDungeonEventType(dungeonInstance, dungeonSubInstance);
        int secondsBeforeEnd = (int)(act4DungeonManager.DungeonEnd - currentTime).TotalSeconds;
        return $"dg {(byte)dungeonInstance.DungeonType} {(byte)dungeonEventType} {secondsBeforeEnd.ToString()} 0";
    }

    private static DungeonEventType AssertDungeonEventType(DungeonInstance dungeonInstance, DungeonSubInstance dungeonSubInstance)
    {
        //quick win
        if (dungeonInstance.FinishSlowMoDate != null)
        {
            return DungeonEventType.BossRoomFinished;
        }

        if (dungeonSubInstance.Bosses.Count > 0)
        {
            return DungeonEventType.InBossRoom;
        }

        if (dungeonInstance.SpawnInstance.PortalGenerators.Count < 1)
        {
            return DungeonEventType.BossRoomOpen;
        }

        return DungeonEventType.BossRoomClosed;
    }

    public static string GenerateEvtb(EventConfiguration eventConfiguration) =>
        "evtb " +
        $"{(eventConfiguration.UpgradeEqEvent ? "50" : "0")} " +
        $"{(eventConfiguration.GambleEqEvent ? "50" : "0")} " +
        $"{(eventConfiguration.UpgradeSpEvent ? "50" : "0")} " +
        $"{(eventConfiguration.UpgradePerfectionEvent ? "50" : "0")} " +
        $"{(eventConfiguration.FamilyExpEvent ? "2" : "0")} " +
        $"0 " +
        $"{(eventConfiguration.ExpEvent ? "2" : "0")} " +
        $"{(eventConfiguration.GoldEvent ? "2" : "0")} " +
        $"{(eventConfiguration.ReputationEvent ? "2" : "0")} " +
        $"{(eventConfiguration.DropEvent ? "2" : "0")} " +
        $"{(eventConfiguration.RuneEvent ? "50" : "0")} " +
        $"{(eventConfiguration.TattooEvent ? "50" : "0")} " +
        "0 0 " +
        $"{(eventConfiguration.BonusRaidBoxEvent ? "50" : "0")} " +
        "0 0 0 0 0 0 0 ";

    public static string GenerateNullAct6Packet() => "act6";

    public static string GenerateCHDM(int maxhp, int angeldmg, int demondmg, int time) =>
        $"ch_dm {maxhp} {angeldmg} {demondmg} {time}";

    public static string GenerateAct6PacketUi(Act6Status status) =>
        $"act6 1 0 {status.AngelPointsPercentage} " +
        $"{Convert.ToByte(status.AngelMode)} " +
        $"{status.AngelCurrentTime} " +
        $"{status.AngelTotalTime} " +
        $"{status.DemonPointsPercentage} " +
        $"{Convert.ToByte(status.DemonMode)} " +
        $"{status.DemonCurrentTime} " +
        $"{status.DemonTotalTime}";

    public static string GenerateDlgPacket(this IClientSession session, string yesPacket, string noPacket, string message) =>
        $"dlg #{yesPacket.Replace(' ', '^')} #{noPacket.Replace(' ', '^')} {message}";

    public static string GenerateRpPacket(this IClientSession session, int mapId, int x, int y, string param) => $"rp {mapId} {x} {y} {param}";

    public static string GenerateSpPointPacket(this IClientSession session) =>
        $"sp {session.PlayerEntity.SpPointsBonus} {StaticServerManager.Instance.MaxAdditionalSpPoints} {session.PlayerEntity.SpPointsBasic} {StaticServerManager.Instance.MaxBasicSpPoints}";
    
    public static string GenerateSpFtptPacket(this IClientSession session)
    {
        switch (session.PlayerEntity.Morph)
        {
            case (byte)MorphType.HolyMage:
                return $"ftpt {(byte)FtptType.Holy} {session.PlayerEntity.EnergyBar} {(byte)FtptBarType.Holy}";

            case (byte)MorphType.WaterfallBerserker:
                return $"ftpt {(byte)FtptType.WaterfallBerserker} {session.PlayerEntity.EnergyBar} {(byte)FtptBarType.WaterfallBerserker}";

            case (byte)MorphType.DragonKnight:
                return $"ftpt {(byte)FtptType.DragonKnight} {session.PlayerEntity.EnergyBar} {session.PlayerEntity.SecondEnergyBar}";

            case (byte)MorphType.Blaster:
                return $"ftpt {(byte)FtptType.Blaster} {session.PlayerEntity.EnergyBar} {session.PlayerEntity.SecondEnergyBar}";

            case (byte)MorphType.Gravity:
                return $"ftpt {(byte)FtptType.Gravity} {session.PlayerEntity.EnergyBar} {session.PlayerEntity.SecondEnergyBar}";

            default:
                return "ftpt -1";
        }

    }

    public static string RemoveSpFtptPacket(this IClientSession session) => "ftpt -1";
    
    public static string GenerateEsfPacket(this IClientSession session, byte type) => $"esf {type}";

    public static string GenerateDeletePost(this IClientSession session, byte type, int id) => $"post {type} {id}";

    public static string GenerateNpcDialogSession(this IClientSession session, int value) => GenerateNpcDialog(session.PlayerEntity.Id, value);

    public static string GenerateNpcDialog(long characterId, int value) => $"npc_req 1 {characterId.ToString()} {value}";

    public static string GenerateItemSpeaker(this IClientSession session, GameItemInstance item, string message, IItemsManager itemsManager, ICharacterAlgorithm algorithm)
    {
        string itemInfo = item.Type switch
        {
            ItemInstanceType.BoxInstance => $"{item.GenerateEInfo(itemsManager, algorithm)}",
            ItemInstanceType.SpecialistInstance => $"{(item.GameItem.IsPartnerSpecialist ? item.GeneratePslInfo() : session.GenerateSlInfo(item, algorithm))}",
            ItemInstanceType.WearableInstance => $"{item.GenerateEInfo(itemsManager, algorithm)}",
            _ => $"IconInfo {item.ItemVNum}"
        };

        return $"sayitemt 1 {session.PlayerEntity.Id} 17 1 {item.ItemVNum} {session.PlayerEntity.Name} {message} {itemInfo}";
    }

    public static string GenerateInboxPacket(this IClientSession session, string message) => $"inbox {message}";

    public static string GenerateMsCPacket(this IClientSession session, byte type) => $"ms_c {type}";

    public static string GenerateMSlotPacket(this IClientSession session, int slot) => $"mslot {slot} -1";

    public static string GenerateScpPacket(this IClientSession session, byte type) => $"scp {type}";

    public static string GenerateObArPacket(this IClientSession session) => "ob_ar";

    public static string GenerateClockPacket(this IClientSession session, ClockType type, sbyte subType, TimeSpan time1, TimeSpan time2) =>
        $"evnt {(byte)type} {subType} {(int)time1.TotalMilliseconds / 100} {(int)time2.TotalMilliseconds / 100}";

    public static string GenerateTsClockPacket(this IClientSession session, TimeSpan time1, bool isVisible) =>
        $"evnt {(byte)ClockType.TimeSpaceClock} {(isVisible ? 0 : -1)} {(int)time1.TotalMilliseconds / 100} 1";

    public static string GenerateRemoveClockPacket(this IClientSession session) => "evnt 10 0 -1 -1";

    public static string GenerateRemoveRedClock(this IClientSession session) => "evnt 3 1 -1 -1";

    public static string GenerateInvisible(this IClientSession session) =>
        $"cl {session.PlayerEntity.Id} {(session.PlayerEntity.Invisible || session.PlayerEntity.CheatComponent.IsInvisible ? 1 : 0)} {(session.PlayerEntity.CheatComponent.IsInvisible ? 1 : 0)}";

    public static string GenerateOppositeMove(this IClientSession session, bool enabled) => $"rv_m {session.PlayerEntity.Id} 1 {(enabled ? 1 : 0)}";

    public static string GenerateBubble(this IClientSession session, string message) => $"csp {session.PlayerEntity.Id} {message.Replace(' ', (char)0xB)}";

    public static string GenerateIncreaseRange(this IClientSession session, short range, bool enabled) => $"bf_d {range} {(enabled ? 1 : 0)}";

    public static string GenerateGenderPacket(this IClientSession session) => $"p_sex {(byte)session.PlayerEntity.Gender}";

    //pflag packet's argument doesn't seem useful as it only makes the client do "npc_req", without this argument that theoretically represents the dialog the server should return
    public static string GeneratePlayerFlag(this IClientSession session, long flag) => $"pflag 1 {session.PlayerEntity.Id} {flag.ToString()}";

    public static string GenerateShopPacket(this IClientSession session)
    {
        IEnumerable<ShopPlayerItem> items = session.PlayerEntity.ShopComponent.Items;
        return
            $"shop {(byte)session.PlayerEntity.Type} {session.PlayerEntity.Id} {(items == null ? 0 : 1)} {(items == null ? 0 : 3)} {(items == null ? string.Empty : 0.ToString())} {(items == null ? string.Empty : session.PlayerEntity.ShopComponent.Name)}";
    }

    public static string GenerateGbexPacket(this IClientSession session, IReputationConfiguration reputationConfiguration, IBankReputationConfiguration bankReputationConfiguration,
        IReadOnlyList<CharacterDTO> topReputation) =>
        $"gbex {session.Account.BankMoney / 1000} {session.PlayerEntity.Gold} {(byte)session.PlayerEntity.GetBankRank(reputationConfiguration, bankReputationConfiguration, topReputation)} {session.PlayerEntity.GetBankPenalty(reputationConfiguration, bankReputationConfiguration, topReputation)}";

    private static string GenerateScene(this IClientSession session, byte type, bool skip) => $"scene {type} {(skip ? 1 : 0)}";

    public static string GenerateDragonPacket(this IBattleEntity entity, byte amountOfDragons) => $"eff_d 2 {amountOfDragons} ";
    public static string GenerateEmptyHatusHeads(this IClientSession session) => "bc 0 0 0";

    public static string GenerateArenaStatistics(this IClientSession session, bool leavingArena, PlayerGroup playerGroup)
    {
        CharacterLifetimeStatsDto lifetimeStats = session.PlayerEntity.LifetimeStats;

        var stringBuilder = new StringBuilder($"ascr  {lifetimeStats.TotalArenaKills} {lifetimeStats.TotalArenaDeaths} 0 {session.PlayerEntity.ArenaKills} {session.PlayerEntity.ArenaDeaths} 0");

        if (playerGroup == null)
        {
            stringBuilder.Append($" 0 0 {(leavingArena ? -1 : 0)}");
            return stringBuilder.ToString();
        }

        stringBuilder.Append($" {playerGroup.ArenaKills} {playerGroup.ArenaDeaths} {(leavingArena ? -1 : 1)}");
        return stringBuilder.ToString();
    }

    #endregion

    #region Send Packets

    /// <summary>
    ///     Qna packet is supposed to trigger a dialog box on the client side, which, once confirmed, will make the client send
    ///     the packet given in parameter
    /// </summary>
    /// <param name="session"></param>
    /// <param name="packet">Packet you want the client to send when he will confirm the dialog box</param>
    /// <param name="message"></param>
    public static void SendQnaPacket(this IClientSession session, string packet, string message) => session.SendPacket(session.GenerateQna(packet, message));
    
    public static void SendQnaiPacket(this IClientSession session, string packet, Game18NConstString message) => session.SendPacket(session.GenerateQnai(packet, message));

    public static void SendPlayerShopTitle(this IClientSession packetReceiver, IClientSession shopOwner) => packetReceiver.SendPacket(shopOwner.GenerateShopPacket());
    public static void SendPlayerFlag(this IClientSession receiverSession, IClientSession targetSession, long flag) => receiverSession.SendPacket(targetSession.GeneratePlayerFlag(flag));
    public static void SendInboxPacket(this IClientSession session, string message) => session.SendPacket(session.GenerateInboxPacket(message));

    public static void SendGuriPacket(this IClientSession session, byte type, short argument = 0, long value = 0, int secondValue = 0) =>
        session.SendPacket(session.GenerateGuriPacket(type, argument, value, secondValue));

    public static void SendEsfPacket(this IClientSession session, byte type) => session.SendPacket(session.GenerateEsfPacket(type));
    public static void RefreshSpPoint(this IClientSession session) => session.SendPacket(session.GenerateSpPointPacket());
    public static void SendSpFtptPacket(this IClientSession session) => session.SendPacket(session.GenerateSpFtptPacket());
    public static void SendRemoveSpFtptPacket(this IClientSession session) => session.SendPacket(session.RemoveSpFtptPacket());
    public static void SendRpPacket(this IClientSession session, int mapId, int x, int y, string param) => session.SendPacket(session.GenerateRpPacket(mapId, x, y, param));
    public static void SendEsfPacket(this IClientSession session) => session.SendPacket("esf 4");
    public static void SendDialog(this IClientSession session, string yesPacket, string noPacket, string dialog) => session.SendPacket(session.GenerateDlgPacket(yesPacket, noPacket, dialog));
    public static void SendSpeak(this IClientSession session, string message, SpeakType type) => session.SendPacket(session.GenerateSpkPacket(message, type));
    public static void SendSpeakToTarget(this IClientSession session, IClientSession target, string message, SpeakType type) => target.SendPacket(session.GenerateSpkPacket(message, type));
    public static void SendEvtb(this IClientSession session, EventConfiguration eventConfiguration) => session.SendPacket(GenerateEvtb(eventConfiguration));
    
    public static void ReceiveSpeakWhisper(this IClientSession receiver, long senderId, string senderName, string message, SpeakType type) =>
        receiver.SendPacket(GenerateSpkPacket(senderId, senderName, message, type));

    public static void BroadcastRest(this IClientSession session) => session.Broadcast(session.GenerateRestPacket());
    public static void BroadcastRevive(this IClientSession session) => session.Broadcast(session.PlayerEntity.GenerateRevive());

    public static void BroadcastGuri(this IClientSession session, byte type, byte argument, long value = 0, params IBroadcastRule[] rules) =>
        session.Broadcast(session.GenerateGuriPacket(type, argument, value), rules);

    public static void BroadcastIn(this IClientSession session, IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation, params IBroadcastRule[] rules) =>
        session.Broadcast(session.GenerateInPacket(reputationConfiguration, topReputation), rules);

    public static void BroadcastOut(this IClientSession session, params IBroadcastRule[] rules) => session.Broadcast(session.GenerateOutPacket(), rules);
    public static void BroadcastMateOut(this IMateEntity mateEntity) => mateEntity.MapInstance?.Broadcast(mateEntity.GenerateOut());

    public static void BroadcastMateTeleport(this IClientSession session, IMateEntity mateEntity, params IBroadcastRule[] rules) =>
        session.Broadcast(mateEntity.GenerateTeleportPacket(mateEntity.PositionX, mateEntity.PositionY), rules);

    /// <summary>
    ///     By default it will send a TeleportPacket to where the character is, you can also define the coords manually.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="rules"></param>
    public static void BroadcastTeleportPacket(this IClientSession session, short? x = null, short? y = null, params IBroadcastRule[] rules)
    {
        short teleportX = session.PlayerEntity.PositionX;
        short teleportY = session.PlayerEntity.PositionY;
        if (x != null)
        {
            teleportX = (short)x;
        }

        if (y != null)
        {
            teleportY = (short)y;
        }

        session.Broadcast(session.PlayerEntity.GenerateTeleportPacket(teleportX, teleportY), rules);
    }

    public static void BroadcastSpeak(this IClientSession session, string message, SpeakType type, params IBroadcastRule[] rules) =>
        session.PlayerEntity.MapInstance.Broadcast(session.GenerateSpkPacket(message, type), rules);

    public static void BroadcastTitleInfo(this IClientSession session) => session.CurrentMapInstance.Broadcast(session.GenerateTitInfoPacket());
    public static void BroadcastEffect(this IClientSession session, EffectType effectType, params IBroadcastRule[] rules) => session.Broadcast(session.GenerateEffectPacket(effectType), rules);

    public static void BroadcastEffectInRange(this IClientSession session, EffectType effectType) =>
        session.Broadcast(session.GenerateEffectPacket(effectType), new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));

    public static void BroadcastEffectInRange(this IClientSession session, int effectId) =>
        session.Broadcast(session.GenerateEffectPacket(effectId), new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));

    public static void BroadcastEffect(this IClientSession session, int effectId, params IBroadcastRule[] rules) => session.Broadcast(session.GenerateEffectPacket(effectId), rules);
    public static void BroadcastCMode(this IClientSession session, params IBroadcastRule[] rules) => session.Broadcast(session.GenerateCModePacket(), rules);
    public static void BroadcastEq(this IClientSession session, params IBroadcastRule[] rules) => session.Broadcast(session.GenerateEqPacket(), rules);
    public static void BroadcastPairy(this IClientSession session, params IBroadcastRule[] rules) => session.Broadcast(session.GeneratePairyPacket(), rules);
    public static void BroadcastMinipet(this IClientSession session, params IBroadcastRule[] rules) => session.Broadcast(session.GenerateMinipetPacket(), rules);

    public static void BroadcastTargetConstBuffEffects(this IClientSession session, IMateEntity mateEntity, params IBroadcastRule[] rules)
        => session.CurrentMapInstance?.Broadcast(mateEntity.GenerateConstBuffEffects(), rules);

    public static void SendTargetInPacket(this IClientSession session, IClientSession target, IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation,
        bool foe = false, bool showInEffect = false)
        => session.SendPacket(target.GenerateInPacket(reputationConfiguration, topReputation, foe, showInEffect));

    public static void BroadcastMovement(this IClientSession session, IBattleEntity entity, params IBroadcastRule[] rules) => session.Broadcast(GenerateMovement(entity), rules);
    public static void Broadcast(this IClientSession session, string packet, params IBroadcastRule[] rules) => session.CurrentMapInstance?.Broadcast(packet, rules);
    public static void Broadcast<T>(this IClientSession session, T packet, params IBroadcastRule[] rules) where T : IServerPacket => session.CurrentMapInstance?.Broadcast(packet, rules);
    public static void SendChatMessage(this IClientSession session, string msg, ChatMessageColorType color) => session.SendPacket(session.GenerateSayPacket(msg, color));

    public static void SendChatMessageNoPlayer(this IClientSession session, string msg, ChatMessageColorType color) =>
        session.SendPacket($"say {(byte)session.PlayerEntity.Type} 0 {(byte)color} {msg}");

    public static void SendChatMessageNoId(this IClientSession session, string msg, ChatMessageColorType color) => session.SendPacket(GenerateSayNoIdPacket(msg, color));
    public static void SendInformationChatMessage(this IClientSession session, string msg) => session.SendChatMessage(msg, ChatMessageColorType.Yellow);
    public static void SendSuccessChatMessage(this IClientSession session, string msg) => session.SendChatMessage(msg, ChatMessageColorType.Green);
    public static void SendErrorChatMessage(this IClientSession session, string msg) => session.SendChatMessage(msg, ChatMessageColorType.Red);

    public static void SendSpCooldownUi(this IClientSession session, int seconds) => session.SendPacket(session.GenerateSpCooldownPacket(seconds));
    public static void ResetSpCooldownUi(this IClientSession session) => session.SendPacket(session.GenerateSpCooldownPacket(0));
    public static string GenerateSpCooldownPacket(this IClientSession session, int seconds) => $"sd {seconds}";

    public static void SendDebugMessage(this IClientSession session, string msg, ChatMessageColorType color = ChatMessageColorType.Yellow)
    {
        if (!session.DebugMode)
        {
            return;
        }

        session.SendChatMessage($"[DEBUG] {msg}", color);
    }

    public static void SendCancelPacket(this IClientSession session, CancelType cancelType, int id = 0)
    {
        session.SendPacket(session.GenerateCancelPacket(cancelType, id));
        session.SendDebugMessage("Battle cancel");
    }

    public static void SendGuriFactionOverridePacket(this IClientSession session) => session.SendPacket(session.GenerateGuriFactionOverridePacket());

    public static void SendDungeonPacket(this IClientSession session, DungeonInstance dungeonInstance, DungeonSubInstance dungeonSubInstance, IAct4DungeonManager act4DungeonManager,
        DateTime currentTime)
        => session.SendPacket(session.GenerateDungeonPacket(dungeonInstance, dungeonSubInstance, act4DungeonManager, currentTime));

    public static void SendStpM(this IClientSession session) => session.SendPacket(session.GenerateStpM());
    public static void SendStpS(this IClientSession session, IMateEntity mateEntity) => session.SendPacket(session.GenerateStpS(mateEntity));
    public static void SendInfo(this IClientSession session, string msg) => session.SendPacket(session.GenerateInfoPacket(msg));
    public static void SendInfo(this IClientSession session, GameDialogKey msg) => session.SendPacket(session.GenerateInfoPacket(session.GetLanguage(msg)));
    public static void SendInfo(this IClientSession session, GameDialogKey msg, params object[] formatParams) => session.SendPacket(session.GenerateInfoPacket(session.GetLanguageFormat(msg)));
    public static void SendInfoi(this IClientSession session, Game18NConstString message, byte argument = 0, int firstData = 0, int secondData = 0) => session.SendPacket(session.GenerateInfoiPacket(message, argument, firstData, secondData));
    public static void SendModali(this IClientSession session, Game18NConstString message, byte argument = 0, int firstData = 0, int secondData = 0) => session.SendPacket(session.GenerateModaliPacket(message, argument, firstData, secondData));
    public static void SendMsgi(this IClientSession session, MsgiMessageType type, Game18NConstString message, byte argument = 0, int value = 0) => session.SendPacket(session.GenerateMsgiPacket(type, message, argument, value));

    public static void SendSayi(this IClientSession session, ChatMessageColorType color, Game18NConstString message, byte argumentType = 0, int firstData = 0, int secondData = 0) =>
        session.SendPacket(session.GenerateSayiPacket(color, message, argumentType, firstData, secondData));
    public static void SendMsg(this IClientSession session, string msg, MsgMessageType type) => session.SendPacket(session.GenerateMsgPacket(msg, type));
    public static void SendMsg(this IClientSession session, GameDialogKey msg, MsgMessageType type) => session.SendPacket(session.GenerateMsgPacket(session.GetLanguage(msg), type));

    public static void BroadcastHeal(this IBattleEntity entity, int heal) => entity.MapInstance.Broadcast(entity.GenerateRcPacket(heal));
    public static void BroadcastDamage(this IBattleEntity entity, int damage, DmType type = DmType.RedDamage) => entity.MapInstance.Broadcast(entity.GenerateDamage(damage, type));
    public static void SendPost(this IClientSession session, byte type, int id) => session.SendPacket(session.GenerateDeletePost(type, id));
    public static void SendSMemo(this IClientSession session, SmemoType type, string message) => session.SendPacket(session.GenerateSmemo(type, message));

    public static void SendRcScalcPacket(this IClientSession session, byte type, long price, int amount, int bzAmount, long taxes, long priceTaxes, string name)
        => session.SendPacket(session.GenerateRcScalc(name, type, price, amount, bzAmount, taxes, priceTaxes));

    public static void SendEmptyRcScalcPacket(this IClientSession session) => session.SendPacket(session.GenerateEmptyRcScalc());
    public static void SendNpcDialog(this IClientSession session, int value) => session.SendPacket(session.GenerateNpcDialogSession(value));
    public static void SendTargetNpcDialog(this IClientSession session, long targetCharacterId, int value) => session.SendPacket(GenerateNpcDialog(targetCharacterId, value));
    public static void SendSpectatorWindow(this IClientSession session) => session.SendPacket(session.GenerateSpectatorWindow());
    public static void SendPslInfoPacket(this IClientSession session, GameItemInstance item) => session.SendPacket(item.GeneratePslInfo());
    public static void SendMsCPacket(this IClientSession session, byte type) => session.SendPacket(session.GenerateMsCPacket(type));
    public static void SendMSlotPacket(this IClientSession session, int slot) => session.SendPacket(session.GenerateMSlotPacket(slot));
    public static void SendScpPacket(this IClientSession session, byte type) => session.SendPacket(session.GenerateScpPacket(type));
    public static void SendObArPacket(this IClientSession session) => session.SendPacket(session.GenerateObArPacket());
    public static void SendEffectEntity(this IClientSession session, IBattleEntity battleEntity, EffectType effectId) => session.SendPacket(battleEntity.GenerateEffectPacket(effectId));

    public static void SendClockPacket(this IClientSession session, ClockType type, sbyte subType, TimeSpan time1, TimeSpan time2) =>
        session.SendPacket(session.GenerateClockPacket(type, subType, time1, time2));

    public static void SendTsClockPacket(this IClientSession session, TimeSpan time, bool isVisible) => session.SendPacket(session.GenerateTsClockPacket(time, isVisible));
    public static void SendRemoveClockPacket(this IClientSession session) => session.SendPacket(session.GenerateRemoveClockPacket());
    public static void SendRemoveRedClockPacket(this IClientSession session) => session.SendPacket(session.GenerateRemoveRedClock());

    public static void SendEffectObject(this IClientSession session, IBattleEntity entity, bool first, EffectType effect) => session.SendPacket(entity.GenerateEffectObject(first, effect));

    public static void RefreshFriendList(this IClientSession session, ISessionManager sessionManager) =>
        session.SendPacket(session.GenerateFinit(sessionManager));

    public static void RefreshBlackList(this IClientSession session) =>
        session.SendPacket(session.GenerateBlinit());

    public static void SendOppositeMove(this IClientSession session, bool enabled) => session.SendPacket(session.GenerateOppositeMove(enabled));
    public static void BroadcastBubbleMessage(this IClientSession session, string message) => session.Broadcast(session.GenerateBubble(message));

    public static void SendIncreaseRange(this IClientSession session)
    {
        int range = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.FearSkill,
            (byte)AdditionalTypes.FearSkill.AttackRangedIncreased, session.PlayerEntity.Level).firstData;

        session.SendPacket(session.GenerateIncreaseRange((short)range, range > 0));

        if (session.PlayerEntity.UseSp && session.PlayerEntity.Specialist != null)
        {
        }
    }

    public static void BroadcastEffectGround(this IBattleEntity entity, EffectType effectType, short x, short y, bool remove) =>
        entity.MapInstance.Broadcast(entity.GenerateEffectGround(effectType, x, y, remove));

    public static void SendGenderPacket(this IClientSession session) => session.SendPacket(session.GenerateGenderPacket());
    public static void BroadcastPlayerShopFlag(this IClientSession session, long flag) => session.Broadcast(session.GeneratePlayerFlag(flag), new ExceptSessionBroadcast(session));
    public static void BroadcastShop(this IClientSession session) => session.Broadcast(session.GenerateShopPacket());

    public static void BroadcastEffectTarget(this IBattleEntity entity, IBattleEntity target, EffectType effectType)
        => entity.MapInstance.Broadcast(entity.GenerateEffectTarget(target, effectType));

    public static void SendGbexPacket(this IClientSession session, IReputationConfiguration reputationConfiguration, IBankReputationConfiguration bankReputationConfiguration,
        IReadOnlyList<CharacterDTO> topReputation)
        => session.SendPacket(session.GenerateGbexPacket(reputationConfiguration, bankReputationConfiguration, topReputation));

    public static void SendScene(this IClientSession session, byte type, bool skip) => session.SendPacket(session.GenerateScene(type, skip));

    public static void SendEmptyHatusHeads(this IClientSession session) => session.SendPacket(session.GenerateEmptyHatusHeads());

    public static void SendPetBasketPacket(this IClientSession session, bool isOn) => session.SendPacket(session.GeneratePetBasketPacket(isOn));

    public static void BroadcastEndDancingGuriPacket(this IPlayerEntity playerEntity) => playerEntity.MapInstance.Broadcast(playerEntity.GenerateEndDancingGuriPacket());

    public static void SendMapClear(this IClientSession session) => session.SendPacket(session.CurrentMapInstance.GenerateMapClear());

    public static void SendArenaStatistics(this IClientSession session, bool leavingArena, PlayerGroup playerGroup = null) =>
        session.SendPacket(session.GenerateArenaStatistics(leavingArena, playerGroup));

    #endregion
}