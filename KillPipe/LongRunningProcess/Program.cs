using System;
using System.Threading;
using System.Threading.Tasks;

namespace LongRunningProcess
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int seconds = 0;
            if (args.Length != 1 || !int.TryParse(args[0], out seconds))
            {
                Environment.ExitCode = -1;
                Console.WriteLine("LongRunningProcess requires one argument defining number of seconds to sleep.");
            }
            Console.WriteLine($"LongRunningProcess sleeping for {seconds} secs");
            await Task.Delay(seconds * 1000);
            Environment.ExitCode = 12345;
            Console.WriteLine($"LongRunningProcess set exit code {Environment.ExitCode}");
        }
    }
}
