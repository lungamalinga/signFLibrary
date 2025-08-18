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
        public async Task<bool> saveDocLocally(string customFileName, string base64String)
        {
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                // Build the full file path
                var filePath = Path.Combine(uploadsFolder, customFileName);

                // Convert base64 to bytes
                byte[] fileBytes = Convert.FromBase64String(base64String);

                // Save to file
                File.WriteAllBytes(filePath, fileBytes);

                Console.WriteLine($"Saved file to: {filePath}");
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to save file: " + e);
                return false;
            }
        }

        /**
         * Delete file from the "uploads" folder
         */
        public async void deleteFile(string fileName)
        {
            string filePath = Path.Combine(".uploads", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"Deleted file: {filePath}");
            }
            else
            {
                Console.WriteLine($"File not found: {filePath}");
            }
        }
    }

}
