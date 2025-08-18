using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace SigniFlowMiddlewareLibrary.Auth
{

    // sage cookie and toke response [kinda type]
    public class SageAuthResponse
    {
        public string token { get; set; }
        public string cookie { get; set; }
    }

    // Token Authentication
    public static class SageAuth
    {
        private static readonly string SAGE_URL = "https://online.sage.co.za/M91423"; // TODO: Environment.GetEnvironmentVariable("SAGE_URL");
        private static readonly string API_KEY = "6598ea86-b185-49ee-bcab-df19d5c4f4cd"; // TODO: Environment.GetEnvironmentVariable("SAGE_API_KEY");

        public static async Task<SageAuthResponse> GetTokenAndCookie()
        {
            using (var client = new HttpClient())
            {
                // Prepare body
                var body = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("scope", $"apiKey={API_KEY}")
            });

                //  post to sage
                var response = await client.PostAsync($"{SAGE_URL}/token", body);
                response.EnsureSuccessStatusCode();

                // get token and cookie from sage
                var json = await response.Content.ReadAsStringAsync();
                dynamic tokenData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);
                string token = tokenData?.GetProperty("access_token").GetString();

                // Extract cookies
                IEnumerable<string> cookieHeaders;
                response.Headers.TryGetValues("Set-Cookie", out cookieHeaders);
                string cookie = cookieHeaders != null ? string.Join("; ", cookieHeaders) : string.Empty;

                return new SageAuthResponse
                {
                    token = token,
                    cookie = cookie
                };
            }
        }
    }
}
