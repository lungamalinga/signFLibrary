using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SigniFlowMiddlewareLibrary.FilesService;

namespace SigniFlowMiddlewareApiTrigger;

public class RemoveFileByNameFunction
{
    private readonly ILogger<RemoveFileByNameFunction> _logger;

    public RemoveFileByNameFunction(ILogger<RemoveFileByNameFunction> logger)
    {
        _logger = logger;
    }

    [Function("remove")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "remove/{fileName}")] HttpRequest req)
    {
        try {
            DocumentServices documentService = new DocumentServices();
            string folderPath = "uploads";
            string fileName = req.RouteValues["filename"]?.ToString();
            documentService.deleteFile( fileName, folderPath);
            return new OkObjectResult($"File {req.Query["fileName"]} removed successfully");
        } catch (Exception ex) 
        {
            Console.WriteLine($"Error in RemoveFileByNameFunction: {ex.Message}");
            return new BadRequestObjectResult("An error occurred while processing the request.");
        }
    }
}