using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SigniflowMiddlewareCSharp.Loggings;
using SigniFlowMiddlewareLibrary.Models;
using SigniFlowMiddlewareLibrary.SageService;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace SigniFlowMiddlewareApiTrigger;

public class SageFunction
{
     MyLogs myLogs;

    public SageFunction(ILogger<SageFunction> logger)
    {
        string connectionString = Environment.GetEnvironmentVariable("MySQLConnectionString");
        myLogs = new MyLogs( connectionString );
    }

    [Function("employees")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        // auth check
        string authToken = Environment.GetEnvironmentVariable("BearerToken");
        var authHelper = new AuthHelper(authToken);
        var authResult = authHelper.ValidateToken(req, myLogs);
        if (authResult != null)
        {
            return authResult;
        }
        else {
            myLogs.LogInfo("Authenticated successfully!");
        }

        // Your main function logic here
        // auth end
        string SAGE_URL = Environment.GetEnvironmentVariable("SAGE_URL");
        string SAGE_API_KEY = Environment.GetEnvironmentVariable("SAGE_API_KEY");
        string LogDbConnString = Environment.GetEnvironmentVariable("MySQLConnectionString");
        SageServices sageServices = new SageServices( SAGE_URL, SAGE_API_KEY, LogDbConnString );
        Object finalResult = null;
        Object EmployeeData = null;
        await sageServices.GetEmployees().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var result = task.Result;
                myLogs.LogInfo("Sage employees retrieved successfully.");
                finalResult = result;

                Root rootEmployeeData = JsonSerializer.Deserialize<Root>(finalResult.ToString());
                EmployeeData = rootEmployeeData.employees.success.ToString();
                //myLogs.LogInfo("Sage Response " + rootEmployeeData.employees.success.ToString());
            }
            else
            {
                myLogs.LogError("Failed to retrieve Sage employees: " + task.Exception?.Message);
                finalResult = task.Exception?.Message;
                EmployeeData = task.Exception?.Message;
                new BadRequestObjectResult("Failed to retrieve Sage employees : " + finalResult);
            }
        });
        myLogs.LogInfo("Sage Response: " + EmployeeData);
        return new OkObjectResult(finalResult);
    }
}