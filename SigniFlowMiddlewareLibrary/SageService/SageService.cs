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
            SageAuth sageAuth = new SageAuth(SAGE_URL, SAGE_API_KEY);
            var auth = await sageAuth.GetTokenAndCookie();
            string token = auth.token;
            string cookie = auth.cookie;

            DateTime expiryDate = DateTime.Today.AddMonths(6);
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Cookie", $"{cookie}");

            var request = new HttpRequestMessage(HttpMethod.Post, SAGE_URL + "/api/apibase/LineImage/" + APIHeaderID);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent($@"{{
                                            ""companyCode"": ""SPARK_SCHOOLS"",
                                            ""CompanyRuleCode"": ""{companyRuleCode}"",
                                            ""employeeCode"": ""{employeeCode}"",
                                            ""contentCategoryTypeID"": 508,
                                            ""ContentCategoryTypeCode"": ""{fileName}"",
                                            ""subject"": ""ALL FIELDS - FINAL Doc1"",
                                            ""ZoneCode"": ""EA_IT"",
                                            ""ExpiryDate"": ""{expiryDate}""}}"), "model");

            content.Add(new StreamContent(File.OpenRead(filePath)), "file0", filePath);
            request.Content = content;
            Console.WriteLine("Request content: " + request.Content.ToString());

            // uncomment to run
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response content -> " + responseContent);
                return responseContent;
            }
            else
            {
                // Handle error cases, e.g., read error message if available
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Error content -> " + errorContent);
                return $"Error: {response.StatusCode} - {errorContent}";
            }
        }
    }
}