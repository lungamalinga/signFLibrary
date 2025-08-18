using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SigniflowMiddlewareCSharp.Loggings;
using SigniFlowMiddlewareLibrary.SageService;
using System.Text.Json;

namespace SigniFlowMiddlewareApiTrigger;

public class SageFunction
{
    private readonly MyLogs myLogs;

    public SageFunction(ILogger<SageFunction> logger)
    {
        myLogs = new MyLogs();
    }

    [Function("employees")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        SageServices sageServices = new SageServices();
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