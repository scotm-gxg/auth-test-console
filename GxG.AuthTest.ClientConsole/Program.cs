using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace GxG.AuthTest.ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            RunResourceOwnerTest().Wait();
            RunClientCredentialsTest().Wait();

            Console.WriteLine("finished. press any key");
            Console.ReadKey();
        }

        private static async Task RunResourceOwnerTest()
        {
            var discoveryClient = new DiscoveryClient("http://localhost:5000");
            var doc = await discoveryClient.GetAsync();

            var tokenClient = new TokenClient(doc.TokenEndpoint, "ro.client", "my-shared-secret");
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("alice", "password", "core.api");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
            
            Console.WriteLine(tokenResponse.Json);
            Console.Write("\n\n");

            await AttemptApiCall(tokenResponse.AccessToken);
        }

        private static async Task RunClientCredentialsTest()
        {
            /*var discoveryClient = new DiscoveryClient("http://authtest");
            discoveryClient.Policy.RequireHttps = false;
            discoveryClient.Policy.ValidateIssuerName = false;
            discoveryClient.Policy.ValidateEndpoints = false;*/

            var discoveryClient = new DiscoveryClient("http://localhost:5000");
            var doc = await discoveryClient.GetAsync();

            var tokenClient = new TokenClient(doc.TokenEndpoint, "api.client", "my-shared-secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("core.api");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine("token: " + tokenResponse.Json);
            Console.WriteLine("\n\n");

            await AttemptApiCall(tokenResponse.AccessToken);
        }

        private static async Task AttemptApiCall(string token)
        {
            var client = new HttpClient();
            client.SetBearerToken(token);

            var response = await client.GetAsync("http://localhost:5001/identity");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error: " + response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
                Console.WriteLine("\n\n");
            }
        }
    }
}
