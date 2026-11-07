using Plugin.TalentArena.Command;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;

namespace Plugin.TalentArena;

public class TalentArenaPlugin : IGamePlugin
{
    private readonly ICommandContainer _commandContainer;

    public TalentArenaPlugin(ICommandContainer commandContainer) => _commandContainer = commandContainer;

    public string Name => nameof(TalentArenaPlugin);

    public void OnLoad()
    {
        _commandContainer.AddModule<TalentArenaCommandModule>();
    }
}