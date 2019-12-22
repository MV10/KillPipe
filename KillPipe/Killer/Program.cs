using System;
using System.IO.Pipes;
using System.Threading.Tasks;

// Solution is configured to run Wrapper and Killer at startup.

namespace Killer
{
    class Program
    {
        static public async Task Main(string[] args)
        {
            Console.WriteLine("Press any key to connect to the kill-command pipe server.");
            Console.ReadKey(true);

            NamedPipeClientStream client = null;
            int connectTimeoutMS = 1000; // locally probably only ~10ms is actually fine
            try
            {
                client = new NamedPipeClientStream(".", "ProcessKiller", PipeDirection.Out, PipeOptions.Asynchronous);
                await client.ConnectAsync(connectTimeoutMS);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.GetType().Name}");
            }
            finally
            {
                client?.Dispose();
            }

            Console.WriteLine("Exiting app");
        }
    }
}
