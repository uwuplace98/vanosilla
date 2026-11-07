using System.Runtime.Serialization;

namespace WingsEmu.Plugins.AdminRestApi.Configuration
{
    [DataContract]
    public class AdminApiConfigurationFile
    {
        [DataMember(Name = "enabled")]
        public bool Enabled { get; set; } = true;

        [DataMember(Name = "port")]
        public int Port { get; set; } = 17500;

        [DataMember(Name = "api_key")]
        public string ApiKey { get; set; } = "test12345";

        [DataMember(Name = "allow_cors")]
        public bool AllowCors { get; set; } = true;

        [DataMember(Name = "cors_origins")]
        public string[] CorsOrigins { get; set; } = { "*" };

        [DataMember(Name = "enable_swagger")]
        public bool EnableSwagger { get; set; } = true;

        [DataMember(Name = "rate_limit_requests_per_minute")]
        public int RateLimitRequestsPerMinute { get; set; } = 60;
    }
}
