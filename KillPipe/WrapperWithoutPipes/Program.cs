using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WrapperWithoutPipes
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var proc = new Process();
            proc.StartInfo.FileName = @"C:\Source\KillPipe\KillPipe\LongRunningProcess\bin\Debug\netcoreapp3.1\LongRunningProcess.exe";
            proc.StartInfo.Arguments = "10";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            var startup = DateTimeOffset.Now;

            try
            {
                Console.WriteLine("Launching child process for 10 seconds.");
                if (!proc.Start()) return;
                Console.WriteLine($"Process started.");

                Console.WriteLine("Awaiting tasks.");
                Task.WaitAny(
                    proc.WaitForExitAsync(ct),
                    ProcessTimeout(ct),
                    ShowTimeForever(ct));
                Console.WriteLine("Wait is over.");

                if (!proc.HasExited)
                {
                    Console.WriteLine("Killing process.");
                    proc.Kill(true); // still fires process-exit (WaitForExitAsync still running)
                }
                
                Console.WriteLine("Cancelling token.");
                cts.Cancel();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wrapper catching exception {ex.GetType().Name}");
                if (ex.InnerException != null) Console.WriteLine($"Inner exception {ex.InnerException.GetType().Name}");
            }
            finally
            {
                cts?.Dispose();
                proc?.Dispose();
            }

            Console.WriteLine("Exiting app.");

            var elapsed = DateTimeOffset.Now.Subtract(startup).TotalSeconds;
            Console.WriteLine($"Elapsed time: {elapsed}");
        }

        static async Task ProcessTimeout(CancellationToken ct)
        {
            int sec = 90; // set to 5 to kill process by timeout
            Console.WriteLine($"ProcessTimeout task sleeping for {sec} seconds.");
            await Task.Delay(sec * 1000, ct);
            Console.WriteLine("ProcessTimeout task exiting.");
        }

        static async Task ShowTimeForever(CancellationToken ct)
        {
            int sec = 0;
            while(!ct.IsCancellationRequested)
            {
                await Task.Delay(1000, ct);
                Console.WriteLine($"[{++sec} sec...]");
            }
        }
    }
}
