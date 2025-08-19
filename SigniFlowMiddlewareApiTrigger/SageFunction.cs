using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SigniflowMiddlewareCSharp.Loggings;
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
        string authHeader = req.Headers["Authorization"];

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return new UnauthorizedResult();
        }

        string token = authHeader.Substring("Bearer ".Length).Trim();

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key")),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Check if the token matches
        string expectedToken = Environment.GetEnvironmentVariable("BearerToken");
        if (token != expectedToken)
        {
            myLogs.LogWarning("Invalid token provided.");
            return new UnauthorizedResult();
        }
        // auth end
        string SAGE_URL = Environment.GetEnvironmentVariable("SAGE_URL");
        SageServices sageServices = new SageServices( SAGE_URL );
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