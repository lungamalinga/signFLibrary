using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
//using SigniflowMiddlewareCSharp.Auth;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using SigniflowMiddlewareCSharp.Loggings;
using Serilog;
using SigniFlowMiddlewareLibrary.Auth;

namespace SigniFlowMiddlewareLibrary.SageService
{
    public class SageServices
    {
        // Change the field initializer to assign myLogs in the constructor instead
        private readonly string SAGE_URL;
        private readonly string connectionString;
        private readonly MyLogs myLogs;
        private readonly string SAGE_API_KEY;

        public SageServices(string SAGE_URL, string SAGE_API_KEY, string LogDbConnectionString)
        {
            this.SAGE_URL = SAGE_URL;
            this.SAGE_API_KEY = SAGE_API_KEY;
            this.connectionString = LogDbConnectionString;
            this.myLogs = new MyLogs(this.connectionString);
        }

        /** 
         * Sage - get all employees
         * **/
        public async Task<object> GetEmployees()
        {
            try
            {
                // auth
                SageAuth sageAuth = new SageAuth(SAGE_URL, SAGE_API_KEY);
                var auth = await sageAuth.GetTokenAndCookie();
                string token = auth.token;
                string cookie = auth.cookie;
                string EMPS_URL = SAGE_URL + "/api/apibase/GenericGet/EMPINFO"; // Environment.GetEnvironmentVariable("GET_EMP_URL");

                HttpContent config = new StringContent("");
                string sageResponseFailure;

                using (HttpClient client = new HttpClient()) // note: always generate a new request
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    client.DefaultRequestHeaders.Add("Cookie", $"{cookie}");

                    //post to sage
                    HttpResponseMessage sageResponse = await client.PostAsync(EMPS_URL, config);
                    myLogs.LogInfo("Sage Response: " + sageResponse.StatusCode);
                    if (sageResponse.IsSuccessStatusCode)
                    {
                        var responseBody = sageResponse.Content.ReadAsStringAsync().Result;
                        var structuredEmployeeList = new JsonObject
                        {
                            ["employees"] = JsonNode.Parse(responseBody),
                        };
                        myLogs.LogInfo("Successfully retrieved employees");
                        return structuredEmployeeList;
                    }
                    else
                    {
                        myLogs.LogError($"Error: {sageResponse.StatusCode} - {sageResponse.ReasonPhrase}");
                        sageResponseFailure = sageResponse.ReasonPhrase;
                        return new JsonObject
                        {
                            ["Code"] = sageResponse.StatusCode.ToString(),
                            ["SageResponse"] = sageResponseFailure,
                        };
                    }
                }
            }
            catch (Exception e)
            {
                myLogs.LogError("Error : " + e.Message);
                throw;
            }
        }

        /**
         * Get/ generate the document header to post it to sage
         */
        public async Task<string> GetSageDocumentHeader()
        {
            SageAuth sageAuth = new SageAuth(SAGE_URL, SAGE_API_KEY);
            var auth = await sageAuth.GetTokenAndCookie();
            string token = auth.token;
            string cookie = auth.cookie;

            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Cookie", $"{cookie}");

            var request = new HttpRequestMessage(HttpMethod.Post, SAGE_URL + "/api/apibase/Header/DOCUMENT");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var SageResponse = await response.Content.ReadAsStringAsync();
            //Console.WriteLine("Sage response: " + SageResponse);

            var SageHeader = (string)JObject.Parse(SageResponse)["data"]["apiObjectID"];
            Console.WriteLine("Returned header: " + SageHeader);
            return SageHeader;
        }

        // todo: Post Document to Sage
        public async Task<object> PostDocumentToDage(string filePath, string APIHeaderID, string companyRuleCode, string employeeCode, string fileName)
        {


            string SAGE_URL = Environment.GetEnvironmentVariable("SAGE_URL");
            string SAGE_API_KEY = Environment.GetEnvironmentVariable("SAGE_API_KEY");

            SageAuth sageAuth = new SageAuth(SAGE_URL, SAGE_API_KEY);
            var auth = await sageAuth.GetTokenAndCookie();
            string token = auth.token;
            string cookie = auth.cookie;

            DateTime expiryDate = DateTime.Today.AddMonths(6);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                client.DefaultRequestHeaders.Add("Cookie", cookie); // No need to format like Set-Cookie

                var requestUrl = $"{SAGE_URL}/api/apibase/LineImage/{APIHeaderID}";
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

                var content = new MultipartFormDataContent();

                // JSON payload as string content with explicit media type
                var jsonPayload = new StringContent($@"{{
            ""companyCode"": ""SPARK_SCHOOLS"",
            ""CompanyRuleCode"": ""{companyRuleCode}"",
            ""employeeCode"": ""{employeeCode}"",
            ""contentCategoryTypeID"": 508,
            ""ContentCategoryTypeCode"": ""{fileName}"",
            ""subject"": ""ALL FIELDS - FINAL Doc1"",
            ""ZoneCode"": ""EA_IT"",
            ""ExpiryDate"": ""{expiryDate:yyyy-MM-dd}""
        }}", System.Text.Encoding.UTF8, "application/json");

                content.Add(jsonPayload, "model");

                // Properly dispose file stream after use
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                    content.Add(fileContent, "file0", Path.GetFileName(filePath));

                    request.Content = content;

                    // Execute request
                    var response = await client.SendAsync(request);

                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Response content -> " + responseContent);
                        return responseContent;
                    }
                    else
                    {
                        Console.WriteLine("Error content -> " + responseContent);
                        return $"Error: {response.StatusCode} - {responseContent}";
                    }
                }
            }
        }
    }
}