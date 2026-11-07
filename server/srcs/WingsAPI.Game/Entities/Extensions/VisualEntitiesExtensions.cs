// WingsEmu
// 
// Developed by NosWings Team

using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.Entities.Extensions;

public static class VisualEntitiesExtensions
{
    public static string GenerateIn(this IMonsterEntity monsterEntity, bool inEffect = false) => monsterEntity.IsStillAlive ? $"in 3 {monsterEntity.MonsterVNum} {monsterEntity.Id} {monsterEntity.PositionX} {monsterEntity.PositionY} {monsterEntity.Direction} {monsterEntity.GetHpPercentage()} {monsterEntity.GetMpPercentage()} 0 0 0 -1 {(inEffect ? 0 : 1)} 0 {monsterEntity.Morph} - 0 -1 0 0 0 0 0 0 0 0 0 0 0 0" : string.Empty;

    public static string GenerateIn(this INpcEntity entity, bool inEffect = false)
    {
        if (entity.IsStillAlive)
        {
            string timeSpaceOwnerName = entity.TimeSpaceOwnerId.HasValue ? entity.MapInstance.GetCharacterById(entity.TimeSpaceOwnerId.Value)?.Name : entity.MinilandOwner?.Name;

            // in 2 1431 2751 102 70 5 100 100 462 0 0 -1 1 0 -1 - 0 -1 0 0 0 0 0 0 0 0 0 0 0 0
            
            return
                "in 2 " +
                $"{entity.NpcVNum} " +
                $"{entity.Id} " +
                $"{entity.PositionX} " +
                $"{entity.PositionY} " +
                $"{entity.Direction} " +
                $"{entity.GetHpPercentage()} " +
                $"{entity.GetMpPercentage()} " +
                $"{(entity.TimeSpaceInfo != null ? 10002 : entity.Dialog)} " +
                "0 " +
                $"{(entity.CharacterPartnerId.HasValue ? 3 : 0)} " +
                $"{entity.CharacterPartnerId ?? -1} " +
                $"{(inEffect ? 0 : 1)} " +
                $"{(entity.IsSitting ? 1 : 0)} " +
                "-1 " +
                $"- " +
                "0 " +
                "-1 " +
                "0 " +
                "0 " +
                "0 " +
                "0 " +
                "0 " +
                "0 " +
                "0 " +
                "0 " +
                $"{(entity.TimeSpaceInfo != null ? 647 : entity.MinilandOwner != null ? 595 : 0)} " +
                $"{(string.IsNullOrEmpty(timeSpaceOwnerName) ? "0" : timeSpaceOwnerName)} " +
                "0 0";
            // Add PetRank
        }

        return string.Empty;
    }

    public static string GenerateOut(this IEntity entity) => $"out {(byte)entity.Type} {entity.Id}";
    public static void BroadcastChatBubble(this IEntity entity, string message, ChatMessageColorType type) => entity.MapInstance.Broadcast(entity.GenerateSayPacket(message, type));
    public static string GenerateSayPacket(this IEntity entity, string message, ChatMessageColorType type) => $"say {(byte)entity.Type} {entity.Id} {(byte)type} {message}";
}