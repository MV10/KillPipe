using System.Threading;
using System.Threading.Tasks;

namespace System.Diagnostics
{
    public static class ProcessWaitForExitAsyncExtension
    {
        public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Starting WaitForExitAsync");

            var tcs = new TaskCompletionSource<bool>();

            process.EnableRaisingEvents = true;
            process.Exited += CompleteTaskOnExit;

            try
            {
                if (process.HasExited) return;
                using (cancellationToken.Register(() => Task.Run(() => tcs.SetCanceled())))
                {
                    await tcs.Task;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Extension catching {ex.GetType().Name}");
            }
            finally
            {
                process.Exited -= CompleteTaskOnExit;
            }

            Console.WriteLine("Leaving WaitForExitAsync");

            void CompleteTaskOnExit(object sender, EventArgs e)
            {
                Console.WriteLine("Process fired Exited event.");
                Task.Run(() => tcs.TrySetResult(true));
            }
        }
    }
}
