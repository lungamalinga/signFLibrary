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
    public class SageAuth
    {
        string SAGE_URL;
        string API_KEY; 
        public SageAuth( string SAGE_URL, string API_KEY) {
            this.SAGE_URL = SAGE_URL;
            this.API_KEY = API_KEY;
        }
        public async Task<SageAuthResponse> GetTokenAndCookie()
        {
            using (var client = new HttpClient())
            {
                // Prepare body
                var body = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("scope", $"apiKey={this.API_KEY}")
            });

                //  post to sage
                var response = await client.PostAsync($"{this.SAGE_URL}/token", body);
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
