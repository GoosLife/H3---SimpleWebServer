using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace H3___SimpleWebServer
{
    internal static class RequestHandler
    {
        // I don't have time to refactor this right now; I realize that I am violating the Single Responsibility Principle here.
        public static HttpResponse HandleTheRequest(Socket clientSocket, string contentPath, Encoding charEncoder)
        {
            try
            {
                string requestString = ReceiveRequest(clientSocket, charEncoder);

                // Parse the request
                (string httpMethod, string requestedUrl) = ParseRequest(requestString, contentPath);

                string requestedFile;
                if (!httpMethod.Equals("GET") || httpMethod.Equals("HEAD"))
                {
                    return HttpResponse.NotImplemented();
                }
                else
                {
                    requestedFile = ResolveRequestedFile(requestedUrl, contentPath);
                }

                try
                {
                    // Convert to local file path and check if it exists. It is not allowed to go outside the content folder.
                    if (!FileRepository.FileExists(requestedFile, contentPath))
                    {
                        return HttpResponse.NotFound();
                    }
                }
                // File not found but not gracefully
                catch (FileNotFoundException e)
                {
                    Logger.Log(e);
                    return HttpResponse.NotFound();
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                    return HttpResponse.ServerError();
                }

                string extension = Path.GetExtension(requestedFile).TrimStart('.').ToLower();
                byte[] fileContent = FileRepository.GetFileContent(requestedFile, contentPath);
                string mimeType = FileRepository.GetMimeType(extension);

                return fileContent != null && mimeType != null ?
                    new HttpResponse(fileContent, "200 OK", mimeType) :
                    HttpResponse.Forbidden();
            }
            catch (Exception e)
            {
                Logger.Log(e);
                return HttpResponse.ServerError();
            }
        }

        private static string ResolveRequestedFile(string requestedUrl, string contentPath)
        {
            string filePath = Uri.UnescapeDataString(requestedUrl).TrimStart('/');
            filePath = filePath.Replace('/', Path.DirectorySeparatorChar).Replace("..", string.Empty);
            if (!Path.HasExtension(filePath))
            {
                filePath = Path.Combine(filePath, "index.html");
            }
            return Path.Combine(contentPath, filePath);
        }

        private static string ReceiveRequest(Socket clientSocket, Encoding charEncoder)
        {
            byte[] buffer = new byte[10240]; // Setup incoming data buffer
            int receivedByteCount = clientSocket.Receive(buffer); // Receive the request and keep track of the size
            return charEncoder.GetString(buffer, 0, receivedByteCount);
        }

        private static (string, string) ParseRequest(string requestString, string contentPath)
        {
            // Find HTTP method
            string httpMethod = requestString.Substring(0, requestString.IndexOf(" "));

            // Find requested URL
            int start = requestString.IndexOf(httpMethod) + httpMethod.Length + 1;
            int length = requestString.LastIndexOf("HTTP") - start - 1;
            string requestedUrl = requestString.Substring(start, length);

            // Return method and URL
            return (httpMethod, requestedUrl);
        }
    }
}
