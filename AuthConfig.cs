using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace DemoBearerTokenClientApp
{
    public class AuthConfig
    {
        public string Instance { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string Authority
        {
            get
            {
                return $"{Instance}{TenantId}";
            }
        }
        public string ClientSecret { get; set; }
        public string BaseAddress { get; set; }
        public string ResourceId { get; set; }

        public static AuthConfig ReadJsonFromFile(string fileName)
        {
            IConfiguration Configuration;

            var builder = new ConfigurationBuilder()
                .AddJsonFile(fileName);

            Configuration = builder.Build();

            return Configuration.Get<AuthConfig>();
        }
    }
}
