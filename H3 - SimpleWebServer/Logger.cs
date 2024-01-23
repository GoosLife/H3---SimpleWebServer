using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3___SimpleWebServer
{
    internal class Logger
    {
        // Pretend we're logging and keeping track of this instead of just throwing it away l o l
        public static void Log(Object message)
        {
            Console.WriteLine(message.ToString());
        }
    }
}
