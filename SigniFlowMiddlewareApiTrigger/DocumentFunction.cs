using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Serilog;
using SigniflowMiddlewareCSharp.Loggings;
using SigniFlowMiddlewareLibrary.FilesService;
using SigniFlowMiddlewareLibrary.SageService;
using System.Text.Json;

namespace SigniFlowMiddlewareApiTrigger;

public class DocumentFunction
{
    private readonly ILogger<DocumentFunction> _logger;
    private readonly MyLogs myLogs;


    public DocumentFunction(ILogger<DocumentFunction> logger)
    {
        myLogs = new MyLogs();
    }

    [Function("upload")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        //string requestBody = await new StreamReader (req.Body).ReadToEndAsync();
        var requestBody = await JsonDocument.ParseAsync(req.Body);


        try
        {
            if (!requestBody.RootElement.TryGetProperty("model", out JsonElement model_) ||
                    model_.ValueKind == JsonValueKind.Null || (model_.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(model_.GetString())))
            {
                Console.WriteLine("Invalid or missing 'model'");
                new BadRequestObjectResult("Invalid or missing 'model' value.");
                return new OkObjectResult("Request body logged.");
            }

            var model = requestBody.RootElement.GetProperty("model");

            // With the following code to properly check for the property and its value:
            if (!model.TryGetProperty("documentBase64", out var documentBase64Element) ||
                documentBase64Element.ValueKind != JsonValueKind.String ||
                string.IsNullOrEmpty(documentBase64Element.GetString()))
            {
                Console.WriteLine("Missing or invalid base64-encoded document.");
                return new BadRequestObjectResult("Missing base64-encoded document.");
            }

            string documentName = model.GetProperty("documentName").ToString();
            string signingDate = model.GetProperty("signingDate").ToString();
            string empName = model.GetProperty("name").ToString().Split(" ")[0];
            string empSurname = model.GetProperty("name").ToString().Split(" ")[1];
            string email = model.GetProperty("email").ToString();
            string base64String = model.GetProperty("documentBase64").ToString();
            string employeeCode = model.GetProperty("metadata").GetProperty("EmployeeCode").ToString();
            string companyRuleCode = model.GetProperty("metadata").GetProperty("CompanyRuleCode").ToString();

            DocumentServices documentServices = new DocumentServices();
            var uuid = Guid.NewGuid().ToString();
            string customFileName = $"{employeeCode}_{empName}_{empSurname}_{documentName.Replace(".pdf", "")}_{signingDate}_{uuid}.pdf";

            await documentServices.saveDocLocally(customFileName, base64String);
            myLogs.LogInfo("Received payload :: " + requestBody.RootElement.GetRawText());

            // TODO : look here...
            string filePath = "uploads/" + customFileName;

            // todo: get document header from Sage
            var sageService = new SageServices();
            var header = await sageService.GetSageDocumentHeader();

            // post to sage - if statement here
            var sageDocResponse = (string)await sageService.PostDocumentToDage(filePath, header, companyRuleCode, employeeCode, customFileName);
            var sageResponse = JsonSerializer.Deserialize<SageResponse>(sageDocResponse);

            if (sageResponse.success)
            {
                documentServices.deleteFile(customFileName);
                return new OkObjectResult("Succesfull uploaded");
            }
            else
            {
                // docService.deleteFile(customFileName);
                documentServices.deleteFile(customFileName);
                return new BadRequestObjectResult(sageResponse.message);
            }
        } catch (Exception ex) {
            myLogs.LogError("Error reading request body: " + ex.Message);
            return new BadRequestObjectResult("Error reading request body: " + ex.Message);
        }
    }
}