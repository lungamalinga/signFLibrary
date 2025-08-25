using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SigniFlowMiddlewareLibrary.FilesService;

namespace SigniFlowMiddlewareApiTrigger;

public class GetFilesFunction
{
    private readonly ILogger<GetFilesFunction> _logger;

    public GetFilesFunction(ILogger<GetFilesFunction> logger)
    {
        _logger = logger;
    }

    [Function("allfiles")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        DocumentServices documentServices = new DocumentServices();
        var fileList = await documentServices.getAllFiles();
        return new OkObjectResult(fileList);
    }
}