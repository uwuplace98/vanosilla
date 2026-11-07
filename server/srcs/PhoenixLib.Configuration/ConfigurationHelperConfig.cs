using System;

namespace PhoenixLib.Configuration
{
    public class ConfigurationHelperConfig
    {
        public string ConfigurationDirectory { get; set; } = Environment.GetEnvironmentVariable("CONFIG_PATH") ?? "C:\\Users\\Wallcroft\\Documents\\Github\\WingsEmu-Managed\\server-files\\";
    }
}