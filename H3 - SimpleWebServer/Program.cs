namespace H3___SimpleWebServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server serv = new Server();
            serv.Start(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }), 6789, 1, "C:/Dev/Skole/H3/H3 - SimpleWebServer/Content");
        }
    }
}