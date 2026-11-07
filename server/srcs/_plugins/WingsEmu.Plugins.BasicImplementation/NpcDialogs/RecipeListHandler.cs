using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.Game;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.NpcDialogs;

public class RecipeListHandler : INpcDialogAsyncHandler
{
    private readonly IRecipeManager _recipeManager;

    public RecipeListHandler(IRecipeManager recipeManager)
    {
        _recipeManager = recipeManager;
        Console.WriteLine("RecipeListHandler initialized.");
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.OPEN_CRAFTING };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        Console.WriteLine($"Execute called with NpcId: {e.NpcId}");

        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            Console.WriteLine($"NPC with ID {e.NpcId} not found.");
            return;
        }

        Console.WriteLine($"Found NPC with ID {e.NpcId}, VNum: {npcEntity.NpcVNum}");

        IReadOnlyList<Recipe> recipes = _recipeManager.GetRecipesByNpcId(e.NpcId) ?? _recipeManager.GetRecipesByNpcMonsterVnum(npcEntity.NpcVNum);
        if (recipes == null)
        {
            Console.WriteLine($"No recipes found for NPC with ID {e.NpcId}.");
            return;
        }

        Console.WriteLine($"Found {recipes.Count} recipes for NPC with ID {e.NpcId}.");

        session.SendWopenPacket((byte)WindowType.CRAFTING_RANDOM_ITEMS_RARITY);
        Console.WriteLine("Sent Wopen packet for crafting.");

        session.SendRecipeNpcList(recipes);
        Console.WriteLine("Sent recipe NPC list.");
    }
}