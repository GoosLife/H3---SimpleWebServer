using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3___SimpleWebServer
{
    internal static class FileRepository
    {
        // Supported content types
        public static readonly Dictionary<string, string> extensions = new Dictionary<string, string>()
        {
            { "htm", "text/html" },
            { "html", "text/html" },
            { "xml", "text/xml" },
            { "txt", "text/plain" },
            { "css", "text/css" },
            { "png", "image/png" },
            { "gif", "image/gif" },
            { "jpg", "image/jpg" },
            { "jpeg", "image/jpeg" },
            { "zip", "application/zip"}
        };

        public static bool FileExists(string filePath, string contentPath)
        {
            return File.Exists(Path.Combine(contentPath, filePath));
        }

        public static byte[]? GetFileContent(string filePath, string contentPath)
        {
            string fullPath = Path.Combine(contentPath, filePath);
            return File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : throw new FileNotFoundException("File not found");
        }

        public static string? GetMimeType(string extension)
        {
            // Assuming a dictionary in RequestHandler that maps extensions to MIME types.
            return extensions.TryGetValue(extension, out var mimeType) ? mimeType : null!;
        }
    }
}
