using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigniFlowMiddlewareLibrary.FilesService
{
    public class DocumentServices
    {
        public DocumentServices() { }

        // save document in the "uploads" folder
        public async Task saveDocLocally(string customFileName, string base64String)
        {
            var filePath = Path.Combine("uploads", customFileName);
            byte[] fileBytes = Convert.FromBase64String(base64String);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await fs.WriteAsync(fileBytes, 0, fileBytes.Length);
            }
        }


        /**
         * Delete file from the "uploads" folder
         */
        public async Task<bool> deleteFile(string fileName, string folderPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(folderPath))
                {
                    Console.WriteLine("File name or folder path is null or empty.");
                    return false;
                }

                // Sanitize the file name to prevent directory traversal attacks
                string safeFileName = Path.GetFileName(fileName);
                string filePath = Path.Combine(folderPath, safeFileName);

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    Console.WriteLine($"Deleted file: {filePath}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"File not found: {filePath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete file: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> getAllFiles()
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            var files = Directory.GetFiles(uploadsFolder).Select(Path.GetFileName).ToList();
            return files;
        }

        // get file by name from the "uploads" folder
        public static async Task<IActionResult> DownloadPdfFileAsync(string filename, string folderPath)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), folderPath);
            var filePath = Path.Combine(uploadsPath, filename);

            // Protect against path traversal
            if (filename.Contains("..") || !Path.GetFullPath(filePath).StartsWith(uploadsPath))
            {
                return new BadRequestObjectResult("Invalid path");
            }

            if (!File.Exists(filePath))
            {
                return new NotFoundObjectResult("File not found");
            }

            try
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }

                memory.Position = 0;

                return new FileStreamResult(memory, "application/pdf")
                {
                    FileDownloadName = filename
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Download error: " + ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


    }

}
