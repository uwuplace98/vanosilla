namespace WingsEmu.Game.Configurations;

public interface IInitialQuestConfig
{
    int InitialQuestId { get; }
}

public class InitialQuestConfig : IInitialQuestConfig
{
    private readonly InitialQuestConfiguration _configuration;

    public InitialQuestConfig(InitialQuestConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int InitialQuestId => _configuration.InitialQuestId;
}

public class InitialQuestConfiguration
{
    public int InitialQuestId { get; set; } = 1001;
}
