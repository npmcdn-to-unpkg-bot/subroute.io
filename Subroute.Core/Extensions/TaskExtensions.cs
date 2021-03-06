using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Subroute.Core.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Monitor will block until the task has completed or faulted, and will run a specific action
        /// at the specified interval until the task has completed.
        /// </summary>
        /// <param name="task">Asynchronous task that is configured to run on a new thread. Example: Task.Run(() => SomeAction()).</param>
        /// <param name="seconds">Interval in seconds of when the specified action will repeat until task has completed.</param>
        /// <param name="action">Arbitrary code-block to be executed during each elapsed interval.</param>
        /// <param name="error"></param>
        /// <returns>Original task that is being monitored.</returns>
        public static Task Monitor(this Task task, double seconds, Action action, Func<Exception, bool> error = null)
        {
            // Use a watchdog timer to determine if the specified action needs to be executed.
            // We will execute the action once the total duration of the task equals or
            // exceeds the specified interval. We will restart the watchdog timer and continue
            // this process until the task has completed (faults are completed).
            var watchdog = Stopwatch.StartNew();
            while (!task.IsCompleted)
            {
                if (watchdog.Elapsed.TotalSeconds >= seconds)
                {
                    watchdog.Restart();

                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        // Give the calling code a chance to handle the exception withou disturbing
                        // the task that is running. Monitor will throw original exception if
                        // calling code chose not to handle the exception or returned false.
                        if (error == null || !error(ex))
                            throw;
                    }
                }

                // We only need to probe the task at most, twice a second.
                Thread.Sleep(500);
            }

            // We need to expose any exceptions that occurred in the task, otherwise it will eat the 
            // exception and we'll never know about it.
            if (!task.IsFaulted)
                return task;

            if (task.Exception != null)
                throw task.Exception;

            throw new Exception("Task did not complete successfully.");
        }

        public static readonly Task CompletedTask = Task.FromResult(false);
    }
}