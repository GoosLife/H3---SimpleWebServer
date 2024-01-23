namespace H3___SimpleWebServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // This can't be the best way to get the solution path, right? Right?!
            string solutionPath = AppDomain.CurrentDomain.BaseDirectory;
            for (int i = 0; i < 5; i++)
            {
                solutionPath = System.IO.Directory.GetParent(solutionPath).FullName;
            }
            string contentPath = System.IO.Path.Combine(solutionPath, "Content");

            Server serv = new Server(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }), 6789, 1, contentPath);
            Logger.Log(serv.Start());
        }
    }
}