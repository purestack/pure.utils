using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pure.Utils
{
    public static class TaskHelper
    {
        public static Task Completed = FromResult(new AsyncVoid());

        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            var completionSource = new TaskCompletionSource<TResult>();
            completionSource.SetResult(result);
            return completionSource.Task;
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct AsyncVoid { }



        public static async Task DelayedAsync(TimeSpan delay, Func<Task> action)
        {
            await Task.Run(async () => {
                await Task.Delay(delay).AnyContext();
                await action().AnyContext();
            }).AnyContext();
        }

        public static Task InParallel(int iterations, Func<int, Task> work)
        {
            return Task.WhenAll(Enumerable.Range(1, iterations).Select(i => Task.Run(() => work(i))));
        }

        public static async Task WithRetriesAsync(Func<Task> action, int maxAttempts = 5, TimeSpan? retryInterval = null, CancellationToken cancellationToken = default(CancellationToken) )
        {
            await  WithRetriesAsync(async () => {
                await action().AnyContext();
                return TaskHelper.Completed;
            }, maxAttempts, retryInterval, cancellationToken).AnyContext();
        }

        public static async Task<T> WithRetriesAsync<T>(Func<Task<T>> action, int maxAttempts = 5, TimeSpan? retryInterval = null, CancellationToken cancellationToken = default(CancellationToken) )
        {
            if (action == null)
                throw new ArgumentNullException("action");

            int attempts = 1;
            var startTime = DateTime.UtcNow;
            do
            {
                //if (attempts > 1)
                //    logger.DebugFormat("Retrying {0} attempt after {1}ms...", attempts, DateTime.UtcNow.Subtract(startTime).TotalMilliseconds);

                try
                {
                    return await action().AnyContext();
                }
                catch (Exception ex)
                {
                    if (attempts >= maxAttempts)
                        throw;

                   // logger.Error("Retry error", ex);
                    //await Task.Delay(retryInterval ?? TimeSpan.FromMilliseconds(attempts * 100), cancellationToken).AnyContext();
                }

                attempts++;
            } while (attempts <= maxAttempts && !cancellationToken.IsCancellationRequested);

            throw new TaskCanceledException("Should not get here.");
        }
    }


    internal static class TaskExtensions
    {
        //public static Task WaitAsync(this AsyncCountdownEvent countdownEvent, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    return Task.WhenAny(countdownEvent.WaitAsync(), cancellationToken.AsTask());
        //}

        //public static Task WaitAsync(this AsyncCountdownEvent countdownEvent, TimeSpan timeout)
        //{
        //    return countdownEvent.WaitAsync(timeout.ToCancellationToken());
        //}

        //public static Task WaitAsync(this AsyncManualResetEvent resetEvent, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    return Task.WhenAny(resetEvent.WaitAsync(), cancellationToken.AsTask());
        //}

        //public static Task WaitAsync(this AsyncManualResetEvent resetEvent, TimeSpan timeout)
        //{
        //    return resetEvent.WaitAsync(timeout.ToCancellationToken());
        //}

        //public static Task WaitAsync(this AsyncAutoResetEvent resetEvent, TimeSpan timeout)
        //{
        //    return resetEvent.WaitAsync(timeout.ToCancellationToken());
        //}

        public static Task IgnoreExceptions(this Task task)
        {
            task.ContinueWith(c => { var ignored = c.Exception; },
                TaskContinuationOptions.OnlyOnFaulted |
                TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        public static Task<T> IgnoreExceptions<T>(this Task<T> task)
        {
            task.ContinueWith(c => { var ignored = c.Exception; },
                TaskContinuationOptions.OnlyOnFaulted |
                TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        public static void SetFromTask<TResult>(this TaskCompletionSource<TResult> resultSetter, Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion: resultSetter.SetResult(task is Task<TResult> ? ((Task<TResult>)task).Result : default(TResult)); break;
                case TaskStatus.Faulted: resultSetter.SetException(task.Exception.InnerExceptions); break;
                case TaskStatus.Canceled: resultSetter.SetCanceled(); break;
                default: throw new InvalidOperationException("The task was not completed.");
            }
        }

        public static void SetFromTask<TResult>(this TaskCompletionSource<TResult> resultSetter, Task<TResult> task)
        {
            SetFromTask(resultSetter, (Task)task);
        }

        public static bool TrySetFromTask<TResult>(this TaskCompletionSource<TResult> resultSetter, Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion: return resultSetter.TrySetResult(task is Task<TResult> ? ((Task<TResult>)task).Result : default(TResult));
                case TaskStatus.Faulted: return resultSetter.TrySetException(task.Exception.InnerExceptions);
                case TaskStatus.Canceled: return resultSetter.TrySetCanceled();
                default: throw new InvalidOperationException("The task was not completed.");
            }
        }

        public static bool TrySetFromTask<TResult>(this TaskCompletionSource<TResult> resultSetter, Task<TResult> task)
        {
            return TrySetFromTask(resultSetter, (Task)task);
        }

        [DebuggerStepThrough]
        public static ConfiguredTaskAwaitable<TResult> AnyContext<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(continueOnCapturedContext: false);
        }

        [DebuggerStepThrough]
        public static ConfiguredTaskAwaitable AnyContext(this Task task)
        {
            return task.ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
