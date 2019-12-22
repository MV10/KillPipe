using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wrapper
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
                    MonitorProcess(proc, ct),
                    WaitForPipeConnection(ct),
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

        static async Task MonitorProcess(Process proc, CancellationToken ct)
        {
            await proc.WaitForExitAsync(ct);
            Console.WriteLine($"Process monitor exiting, is process running? {!proc.HasExited}");
            if(proc.HasExited)
            {
                Console.WriteLine($"Process exit code: {proc.ExitCode}");
            }
        }

        static async Task WaitForPipeConnection(CancellationToken ct)
        {
            NamedPipeServerStream server = null;
            try
            {
                server = new NamedPipeServerStream("ProcessKiller", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                Console.WriteLine("Pipe server waiting for connection.");
                await server.WaitForConnectionAsync(ct);
                Console.WriteLine("Pipe server disconnecting.");
                server.Disconnect();
            }
            finally
            {
                server?.Dispose();
            }
            Console.WriteLine("Pipe server exiting.");
        }

        static async Task ShowTimeForever(CancellationToken ct)
        {
            int sec = 0;
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(1000, ct);
                Console.WriteLine($"[{++sec} sec...]");
            }
        }
    }
}
