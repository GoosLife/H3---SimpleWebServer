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
        private bool running = false; // Runtime flag for the while loop

        private int timeout = 8; // Timeout for requests in milliseconds
        private Encoding charEncoder = Encoding.UTF8; // Specifies the character encoding for the request

        private Socket? serverSocket;

        private string contentPath; // Path to the folder the server will serve from

        // Store the IP address and port number for logging purposes
        private IPAddress ipAddress;
        private int port;

        // List of connection threads
        private List<Thread> connectionThreads = new List<Thread>();

        public Server(IPAddress ipAddress, int port, int maxConnections, string contentPath)
        {
            this.ipAddress = ipAddress;
            this.port = port;

            try
            {
                CreateSocket(ipAddress, port, maxConnections, contentPath);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }

        public bool Start()
        {
            if (running) return false;

            running = true;

            try
            {
                // Create a new thread that will run the server and listen for requests
                Thread requestListenerThread = new Thread(() =>
                {
                    // TODO: Log this
                    Logger.Log("Server running on " + ipAddress + ":" + port + "...");

                    Run();
                });

                // Start the thread and wait for it to finish
                requestListenerThread.Start();
                requestListenerThread.Join();
            }
            // Unknown error; assume critical and stop running immediately
            catch (Exception e)
            {
                // Log error
                Logger.Log(e.Message);

                // Ensure server is stopped
                Stop();

                // Stop running entirely
                running = false;
                return false;
            }

            // If we get here, the server stopped running gracefully, so return true
            return true;
        }

        // Run loop for the server
        private void Run()
        {
            while (running)
            {
                try
                {
                    // Process the incoming request
                    Socket clientSocket = serverSocket.Accept();

                    Thread requestHandlerThread = new Thread(() => ProcessClient(clientSocket));

                    requestHandlerThread.Start();
                    connectionThreads.Add(requestHandlerThread);
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
                }
            }

            // End the server
            Stop();
        }

        public void Stop()
        {
            // Pick up all remaining connection threads and wait for them to finish
            if (connectionThreads.Count > 0)
            {
                foreach (Thread t in connectionThreads)
                {
                    t.Join();
                }

                connectionThreads.Clear();
            }

            // Stop the server socket
            if (running)
            {
                running = false;
                try { serverSocket!.Close(); }
                catch (Exception e) { Logger.Log(e.Message); }
                serverSocket = null;
            }
        }

        private void ProcessClient(Socket clientSocket)
        {
            clientSocket.ReceiveTimeout = timeout;
            clientSocket.SendTimeout = timeout;
            HttpResponse response = RequestHandler.HandleTheRequest(clientSocket, contentPath, charEncoder);
            SendResponse(clientSocket, response);
        }

        private void CreateSocket(IPAddress ipAddress, int port, int maxConnections, string contentPath)
        {
            // Create socket
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ipAddress, port));
            serverSocket.Listen(maxConnections);
            serverSocket.ReceiveTimeout = timeout;
            serverSocket.SendTimeout = timeout;
            this.contentPath = contentPath;
        }

        private void SendResponse(Socket clientSocket, HttpResponse response)
        {
            try
            {
                byte[] responseHeader = charEncoder.GetBytes(
                    "HTTP/1.1 " + response.Status + "\r\n" +
                    "Server: Atasoy Simple Web Server\r\n" +
                    "Content-Length: " + response.Content.Length.ToString() + "\r\n" +
                    "Connection: close\r\n" +
                    "Content-Type: " + response.ContentType + "\r\n\r\n");

                clientSocket.Send(responseHeader);
                clientSocket.Send(response.Content);
                clientSocket.Close();
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }
    }
}