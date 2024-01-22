using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Net.WebSockets;

namespace H3___SimpleWebServer
{
    internal class Server
    {
        private bool running = false;

        private int timeout = 8;
        private Encoding charEncoder = Encoding.UTF8;
        private Socket? serverSocket;
        private string contentPath;

        // Supported content types
        private Dictionary<string, string> extensions = new Dictionary<string, string>()
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

        public bool Start(IPAddress ipAddress, int port, int maxConnections, string contentPath)
        {
            if (running) return false;

            try
            {
                // Create socket
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(ipAddress, port));
                serverSocket.Listen(maxConnections);
                serverSocket.ReceiveTimeout = timeout;
                serverSocket.SendTimeout = timeout;
                running = true;
                this.contentPath = contentPath;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Thread requestListenerThread = new Thread(() =>
            {
                while (running)
                {
                    Console.WriteLine("Server running on" + ipAddress + ":" + port + "...");

                    Socket clientSocket;

                    try
                    {
                        clientSocket = serverSocket.Accept();

                        Thread requestHandlerThread = new Thread(() =>
                        {
                            clientSocket.ReceiveTimeout = timeout;
                            clientSocket.SendTimeout = timeout;

                            try
                            {
                                HandleTheRequest(clientSocket);
                            }
                            catch (Exception e) { Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"); }
                        });
                        requestHandlerThread.Start();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });

            return true;
        }

        public void Stop()
        {
            if (running)
            {
                running = false;
                try { serverSocket!.Close(); }
                catch (Exception e) { Console.WriteLine(e.Message); }
                serverSocket = null;
            }
        }

        private void HandleTheRequest(Socket clientSocket)
        {
            byte[] buffer = new byte[10240]; // Incoming data buffer
            int receivedByteCount = clientSocket.Receive(buffer); // Receive the request and keep track of the size
            string requestString = charEncoder.GetString(buffer, 0, receivedByteCount);

            // Parse the request
            string httpMethod = requestString.Substring(0, requestString.IndexOf(" "));

            int start = requestString.IndexOf(httpMethod) + httpMethod.Length + 1;
            int length = requestString.LastIndexOf("HTTP") - start - 1;
            string requestedUrl = requestString.Substring(start, length);

            string requestedFile;
            if (httpMethod.Equals("GET") || httpMethod.Equals("HEAD"))
            {
                requestedFile = requestedUrl.Split('?')[0];
            }
            else
            {
                NotImplemented(clientSocket);
                return;
            }

            // Convert to local file path and check if it exists. It is not allowed to go outside the content folder.
            requestedFile = requestedFile.Replace("/", @"\").Replace("\\..", "");

            start = requestedFile.LastIndexOf('.') + 1;
            if (start > 0)
            {
                length = requestedFile.Length - start;

                string extension = requestedFile.Substring(start, length);

                if (extensions.ContainsKey(extension))
                {
                    if (File.Exists(contentPath + requestedFile))
                    {
                        OK(clientSocket, File.ReadAllBytes(contentPath + requestedFile), extensions[extension]);
                    }
                    else
                    {
                        NotFound(clientSocket);
                    }
                }
                else
                {
                    Forbidden(clientSocket);
                }
            }
            else
            {
                // Default to index.html
                if (requestedFile.Substring(length - 1, 1) != @"\")
                {
                    requestedFile += @"\";
                }

                // Check if index.html exists, then send it
                if (File.Exists(contentPath + requestedFile + "index.html"))
                {
                    OK(clientSocket, File.ReadAllBytes(contentPath + requestedFile + "index.html"), extensions["html"]);
                }
                else
                {
                    NotFound(clientSocket);
                }
            }
        }

        private void OK(Socket clientSocket, byte[] content, string contentType)
        {
            SendResponse(clientSocket, content, "200 OK", contentType);
        }

        private void NotFound(Socket clientSocket)
        {
            string body =
                """
                <html>
                    <head>
                        <meta 
                        http-equiv="Content-Type"
                        content="text/html; 
                                    charset=utf-8">
                    </head>
                    <body>
                        <h2>Atasoy Simple Web Server</h2>
                        <div>404 - Not Found</div>
                    </body>
                </html>
                """;

            string responseCode = "404 Not Found";

            string contentType = "text/html";

            SendResponse(clientSocket, body, responseCode, contentType);
        }

        private void NotImplemented(Socket clientSocket)
        {
            string body =
                """
                <html>
                    <head>
                        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
                    </head>

                    <body>
                        <h2>Atasoy Simple Web Server</h2>
                        <div>501 - Not Implemented</div>
                    </body>
                </html>
                """;

            string responseCode = "501 Not Implemented";

            string contentType = "text/html";

            SendResponse(clientSocket, body, responseCode, contentType);
        }

        private void Forbidden(Socket clientSocket)
        {
            string body =
                """
                <html>
                    <head>
                        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
                    </head>

                    <body>
                        <h2>Atasoy Simple Web Server</h2>
                        <div>403 - Forbidden</div>
                    </body>
                </html>
                """;

            string responseCode = "403 Forbidden";

            string contentType = "text/html";

            SendResponse(clientSocket, body, responseCode, contentType);
        }

        private void SendResponse(Socket clientSocket, string content, string responseCode, string contentType)
        {
            byte[] contentBytes = charEncoder.GetBytes(content);
            SendResponse(clientSocket, contentBytes, responseCode, contentType);
        }

        private void SendResponse(Socket clientSocket, byte[] content, string responseCode, string contentType)
        {
            try
            {
                byte[] responseHeader = charEncoder.GetBytes(
                    "HTTP/1.1 " + responseCode + "\r\n" +
                    "Server: Atasoy Simple Web Server\r\n" +
                    "Content-Length: " + content.Length.ToString() + "\r\n" +
                    "Connection: close\r\n" +
                    "Content-Type: " + contentType + "\r\n\r\n");

                clientSocket.Send(responseHeader);
                clientSocket.Send(content);
                clientSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}