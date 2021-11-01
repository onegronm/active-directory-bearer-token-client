using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace DemoBearerTokenClientApp
{
    /// <summary>
    /// Shows how to secure a .NET Core API with JWT Bearer Authentication, using Azure Active Directory as the Identity and Access Management Layer.
    /// Also shows how to write a “secure” API client to call and authenticate to the secured API endpoint.
    /// We use the Microsoft.Identity.Client and Microsoft.AspNetCore.Authentication.JwtBearer packages amongst others.
    /// </summary>
    class Program
    {
        private static AuthConfig config;

        static void Main(string[] args)
        {
            var result = GetToken().GetAwaiter().GetResult();
            MakeSecureApiCall(result).GetAwaiter().GetResult();
        }

        private static async Task<AuthenticationResult> GetToken()
        {
            config = AuthConfig.ReadJsonFromFile("appsettings.json");

            ConfidentialClientApplicationOptions _applicationOptions;

            _applicationOptions = new ConfidentialClientApplicationOptions()
            {
                ClientSecret = config.ClientSecret,
                ClientId = config.ClientId,
                Instance = config.Instance,
                TenantId = config.TenantId
            };

            IConfidentialClientApplication app;

            app = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(_applicationOptions)
                .WithAuthority(config.Authority)
                .Build();

            // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
            // application permissions need to be set statically (in the portal or by PowerShell), and
            // then granted by a tenant administrator
            string[] ResourceIds = new string[] { config.ResourceId };

            AuthenticationResult result = null;

            try
            {
                result = await app.AcquireTokenForClient(ResourceIds).ExecuteAsync();
                Console.WriteLine("Token acquired \n");
                Console.WriteLine(result.AccessToken);

                return result;
            }
            catch (MsalClientException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static async Task MakeSecureApiCall(AuthenticationResult result)
        {
            var httpClient = new HttpClient();
            var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

            // Add the Content Type HTTP header
            if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept
                .Any(m => m.MediaType == "application/json"))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }

            // Set "bearer" as the authorization type
            defaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", result.AccessToken);

            // call the resource API
            HttpResponseMessage response = await httpClient.GetAsync(config.BaseAddress);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                Console.WriteLine(json);
            }
            else
            {
                Console.WriteLine($"Failed to call API: {response.StatusCode}");
                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Content: {content}");
            }
        }
    }
}
