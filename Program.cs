using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsumingODataWebServices
{
    public class Program
    {
        private static void Main()
        {
            Credentials credentials = new Credentials() { Username = AdminUsername, Password = AdminPassword };
            HttpClient client = new HttpClient();

            //Task.Run(() => Login(client, credentials))
            //    .ContinueWith(t => AddServiceHeaders(client, t.Result))
            //    .ContinueWith(t => GetNewsItems(client).Wait())
            //    .ContinueWith(t => Dispose(client));

            Task.Run(() => Login(client, credentials))
                .ContinueWith(login => AddServiceHeaders(client, login.Result))
                .ContinueWith(login => GetNewsItems(client) // Use .Wait() if not using the GetNewsItems Result, to complete it
                    .ContinueWith(getNewsItems => PrintNewsItems(getNewsItems.Result))
                    .ContinueWith(getNewsItems => Dispose(client))
                        .ContinueWith(getNewsItems => Console.WriteLine("Done...")));

            Console.WriteLine("Started...");
            Console.ReadLine();
        }

        internal static async Task<string> Login(HttpClient client, Credentials credentials)
        {
            string credentialsJson = JsonConvert.SerializeObject(credentials);

            using (HttpResponseMessage response = await client.PostAsync(SiteApiUrl + LoginUrl, new StringContent(credentialsJson, Encoding.UTF8, JsonContentType)))
            {
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Logged in...");
                using (HttpContent content = response.Content)
                {
                    string result = await content.ReadAsStringAsync();

                    Dictionary<string, string> loginResult = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                    string token = loginResult[TokenValueKey];
                    return token;
                }
            }
        }

        internal static async Task<List<NewsItemModel>> GetNewsItems(HttpClient client)
        {
            using (HttpResponseMessage response = await client.GetAsync(SiteApiUrl + NewsItemServiceUrl))
            {
                response.EnsureSuccessStatusCode();
                using (HttpContent content = response.Content)
                {
                    string result = await content.ReadAsStringAsync();

                    var odataResult = JsonConvert.DeserializeObject<ODataResult<NewsItemModel>>(result);

                    Console.WriteLine("News items queried...");
                    return odataResult.Value;
                }
            }
        }

        internal static void AddServiceHeaders(HttpClient client, string token)
        {
            // Otherwise use SendAsync to create HttpRequestMessage with the headers to send for the current request
            client.DefaultRequestHeaders.Add("Authorization", token);
            client.DefaultRequestHeaders.Add("X-SF-Service-Request", true.ToString());
        }

        internal static void Dispose(HttpClient client)
        {
            Console.WriteLine("Disposed...");
            client.Dispose();
        }

        internal static void PrintNewsItems(List<NewsItemModel> models)
        {
            string outputString = JsonConvert.SerializeObject(models);
            Console.WriteLine(outputString);
        }

        protected static readonly string SiteUrl = "http://localhost/";
        protected static readonly string SiteApiUrl = SiteUrl + "api/default/";
        protected static readonly string LoginUrl = "login";
        protected static readonly string NewsItemServiceUrl = "newsitems";

        protected static readonly string AdminUsername = "admin";
        protected static readonly string AdminPassword = "password";

        internal const string TokenValueKey = "value";
        internal const string JsonContentType = "application/json";
    }
}
