using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PropertyValidator.Test.Extensions
{
    public static class TaskExtension
    {
        public static void FireAndForget(this Task task, Action<Exception> exceptionHandler = null)
        {
            _ = task.ContinueWith(t =>
            {
                ReportException(t, exceptionHandler);
            });
        }

        public static void FireAndForget(this Task task, Action completion, Action<Exception> exceptionHandler = null)
        {
            _ = task.ContinueWith(t =>
            {
                completion();
                ReportException(t, exceptionHandler);
            });
        }

        public static void FireAndForget<T>(this Task<T> task, Action<Exception> exceptionHandler = null)
        {
            _ = task.ContinueWith(t =>
            {
                ReportException(t, exceptionHandler);
            });
        }

        public static void FireAndForget<T>(this Task<T> task, Action completion, Action<Exception> exceptionHandler = null)
        {
            _ = task.ContinueWith(t =>
            {
                completion();
                ReportException(t, exceptionHandler);
            });
        }

        public static Task TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            return TimeoutAfterImpl(task, timeout);
        }

        public static Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            return TimeoutAfterImpl(task, timeout);
        }

        private static async Task TimeoutAfterImpl(Task task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
            if (completedTask == task)
            {
                cts.Cancel();
                await task;  // Very important in order to propagate exceptions
            }
        }

        private static void ReportException(Task task, Action<Exception> exceptionHandler)
        {
            if (!task.IsFaulted)
                return;

            Exception ex = task.Exception;
            while (ex.InnerException != null)
                ex = ex.InnerException;
            exceptionHandler?.Invoke(ex);
        }
    }
}
