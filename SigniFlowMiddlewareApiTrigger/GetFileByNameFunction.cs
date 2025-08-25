using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SigniFlowMiddlewareLibrary.FilesService;
using System.IO;

namespace SigniFlowMiddlewareApiTrigger;

public class GetFileByNameFunction
{
    private readonly ILogger<GetFileByNameFunction> _logger;

    public GetFileByNameFunction(ILogger<GetFileByNameFunction> logger)
    {
        _logger = logger;
    }
    [Function("download")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "download/{filename}")] HttpRequest req)
    {
        string filename = req.RouteValues["filename"]?.ToString();
        string folderPath = "uploads";

        DocumentServices documentService = new DocumentServices();
        return await DocumentServices.DownloadPdfFileAsync(filename, folderPath);
    }
}