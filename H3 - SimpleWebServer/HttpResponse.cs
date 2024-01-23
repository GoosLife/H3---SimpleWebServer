using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace H3___SimpleWebServer
{
    internal class HttpResponse
    {
        public int StatusCode { get => GetStatusCode(); }
        public string Status { get; }
        public byte[] Content { get; }
        public string ContentType { get; }

        public HttpResponse(byte[] content, string statusCode, string contentType)
        {
            this.Status = statusCode;
            this.Content = content;
            this.ContentType = contentType;
        }

        public HttpResponse(string content, string statusCode, string contentType)
        {
            this.Status = statusCode;
            this.Content = Encoding.UTF8.GetBytes(content);
            this.ContentType = contentType;
        }

        public static HttpResponse OK(byte[] content, string contentType)
        {
            return new HttpResponse(content, "200 OK", contentType);
        }

        public static HttpResponse NotFound()
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
                        <h2>GoosLife Simple Web Server</h2>
                        <div>404 - Not Found</div>
                    </body>
                </html>
                """;

            string responseCode = "404 Not Found";

            string contentType = "text/html";

            return new HttpResponse(Encoding.UTF8.GetBytes(body), responseCode, contentType);
        }

        public static HttpResponse NotImplemented()
        {
            string body =
                """
                <html>
                    <head>
                        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
                    </head>

                    <body>
                        <h2>GoosLife Simple Web Server</h2>
                        <div>501 - Not Implemented</div>
                    </body>
                </html>
                """;

            string responseCode = "501 Not Implemented";

            string contentType = "text/html";

            return new HttpResponse(Encoding.UTF8.GetBytes(body), responseCode, contentType);
        }

        public static HttpResponse Forbidden()
        {
            string body =
                """
                <html>
                    <head>
                        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
                    </head>

                    <body>
                        <h2>GoosLife Simple Web Server</h2>
                        <div>403 - Forbidden</div>
                    </body>
                </html>
                """;

            string responseCode = "403 Forbidden";

            string contentType = "text/html";

            return new HttpResponse(Encoding.UTF8.GetBytes(body), responseCode, contentType);
        }

        public static HttpResponse ServerError()
        {
            string body =
                """
                <html>
                    <head>
                        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
                    </head>

                    <body>
                        <h2>GoosLife Simple Web Server</h2>
                        <div>500 - Internal Server Error</div>
                    </body>
                </html>
                """;

            string responseCode = "500 Internal Server Error";

            string contentType = "text/html";

            return new HttpResponse(Encoding.UTF8.GetBytes(body), responseCode, contentType);
        }

        private int GetStatusCode()
        {
            try
            {
                return int.Parse(Status.Split(" ")[0]);
            }
            catch (Exception)
            {
                Logger.Log("Could not parse status code from response:\n" + ToString());
                return 500;
            }
        }

        public override string ToString()
        {
            // Print the properties of the HTTP Response object
            return
                "Status: " + Status + "\n" +
                "Content: " + Encoding.UTF8.GetString(Content) + "\n" +
                "Content Type: " + ContentType;
        }

        public string ToHttpString()
        {
            // Print the properties of the HTTP Response object in a format that can be sent over the network
            return
                "HTTP/1.1 " + Status + "\n" +
                "Content-Type: " + ContentType + "\n" +
                "Content-Length: " + Content.Length + "\n" +
                "\n" +
                Encoding.UTF8.GetString(Content);
        }
    }
}
