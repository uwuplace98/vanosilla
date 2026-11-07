using System.Threading.Tasks;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class ReqInfoPacketHandler : GenericGamePacketHandlerBase<ReqInfoPacket>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public ReqInfoPacketHandler(INpcMonsterManager npcMonsterManager, IGameLanguageService gameLanguage,
        IReputationConfiguration reputationConfiguration, ISpPartnerConfiguration spPartnerConfiguration, IRankingManager rankingManager)
    {
        _npcMonsterManager = npcMonsterManager;
        _gameLanguage = gameLanguage;
        _reputationConfiguration = reputationConfiguration;
        _spPartnerConfiguration = spPartnerConfiguration;
        _rankingManager = rankingManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, ReqInfoPacket packet)
    {
        switch (packet.Type)
        {
            case 12:
                InventoryItem item = session.PlayerEntity.GetItemBySlotAndType((short)packet.TargetVNum, InventoryType.Equipment);
                if (item == null)
                {
                    return;
                }

                if (item.ItemInstance.Type != ItemInstanceType.WearableInstance)
                {
                    return;
                }

                if (item.ItemInstance.Rarity <= 0)
                {
                    session.SendPacket($"r_info {item.ItemInstance.ItemVNum} 0");
                    return;
                }

                if (item.ItemInstance.BoundCharacterId == session.PlayerEntity.Id)
                {
                    session.SendPacket($"r_info {item.ItemInstance.ItemVNum} 1");
                    return;
                }

                session.SendPacket($"r_info {item.ItemInstance.ItemVNum} 2");
                break;

            case 13:
                InventoryItem fairy = session.PlayerEntity.GetItemBySlotAndType((short)packet.TargetVNum, InventoryType.Equipment);
                if (fairy == null)
                {
                    return;
                }

                if (fairy.ItemInstance.Type != ItemInstanceType.WearableInstance)
                {
                    return;
                }

                if (fairy.ItemInstance.FairyEnchantmentLevel >= 0)
                {
                    if (fairy.ItemInstance.FairyEnchantments != null)
                    {
                        var enchantments = fairy.ItemInstance.FairyEnchantments;
                        string effectsStr = string.Join(" ", System.Linq.Enumerable.Select(enchantments, x => $"{x.EffectId}.{x.EffectValue}.{x.EffectLevel}"));
                        session.SendPacket($"pa_suc 1 {enchantments.Count} {effectsStr}");
                    }

                }

                break;
            case 14:
                InventoryItem shell = session.PlayerEntity.GetItemBySlotAndType((short)packet.TargetVNum, InventoryType.Equipment);
                if (shell == null)
                {
                    return;
                }

                if (shell.ItemInstance.Type != ItemInstanceType.WearableInstance)
                {
                    return;
                }

                if (shell.ItemInstance.EquipmentOptions.Count >= 0)
                {
                    if (shell.ItemInstance.EquipmentOptions != null)
                    {
                        var enchantments = shell.ItemInstance.EquipmentOptions;
                        var enchantcount = shell.ItemInstance.EquipmentOptions.Count;
                        string effectsStr = string.Join(" ", System.Linq.Enumerable.Select(enchantments, x => $"{x.EffectVnum}.{x.Type}.{x.Type}.1"));
                        session.SendPacket($"su_suc 1 0 1 {enchantcount} {effectsStr} 100");
                        //su_suc  1 0 1 6 1.17.100.0 1.16.50.0 2.7.3.0 2.25.142.1 3.24.190.0 7.31.26.0 100

                    }

                }
                break;

            case 6:
                if (!packet.MateVNum.HasValue)
                {
                    return;
                }

                // Check if it's an NPC (non-enemy)
                IMonsterData npcPartner = session.CurrentMapInstance.GetNpcById(packet.MateVNum.Value);
                if (npcPartner != null)
                {
                    session.SendNpcInfo(npcPartner, _gameLanguage);
                    return;
                }

                // Check if it's a Monster (Enemy) → First, get the actual spawned entity
                IMonsterEntity targetMonster = session.CurrentMapInstance.GetMonsterById(packet.MateVNum.Value);
                if (targetMonster != null)
                {
                    // Retrieve the correct monster template data
                    IMonsterData mon = _npcMonsterManager.GetNpc((short)targetMonster.MonsterVNum);
                    if (mon != null)
                    {
                        session.SendNpcInfo(mon, _gameLanguage);
                        return;
                    }
                }

                // Check if it's a Mate (Pet/Partner)
                IMateEntity targetMate = session.CurrentMapInstance.GetMateById(packet.MateVNum.Value);
                if (targetMate == null)
                {
                    return;
                }

                if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) &&
                    session.PlayerEntity.Faction != targetMate.Faction)
                {
                    return;
                }

                session.SendPacket(targetMate.GenerateEInfo(_gameLanguage, session.UserLanguage, _spPartnerConfiguration));
                break;
            case 5:
                IMonsterData npc = _npcMonsterManager.GetNpc((short)packet.TargetVNum);
                if (npc == null)
                {
                    return;
                }

                session.SendNpcInfo(npc, _gameLanguage);

                break;
            default:
                IPlayerEntity target = session.CurrentMapInstance.GetCharacterById(packet.TargetVNum);
                if (target == null)
                {
                    return;
                }

                if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    if (session.PlayerEntity.Faction != target.Faction)
                    {
                        return;
                    }
                }

                if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle && target.RainbowBattleComponent.IsInRainbowBattle)
                {
                    if (session.PlayerEntity.RainbowBattleComponent.Team != target.RainbowBattleComponent.Team)
                    {
                        return;
                    }
                }

                session.SendPacket(target.Session.GenerateReqInfo(_reputationConfiguration, _rankingManager.TopReputation));
                break;
        }
    }
}