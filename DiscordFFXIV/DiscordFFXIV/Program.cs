using System;
using System.Threading.Tasks;
using System.Timers;

namespace DiscordFFXIV
{
    class Program
    {
        public static void Main(string[] args)
        {
            Startup.RunAsync(args).GetAwaiter().GetResult();
        }
       

       
    }
}
