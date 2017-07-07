using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Services.Tasks
{
    public enum TaskServiceCompletionType
    {
        Undefined,
        Success,
        Cancelled,
        Exception
    };

    public abstract class AbstractTaskRunner : ITaskRunner, IDisposable
    {
        protected CountdownEvent countdown;
        protected CancellationTokenSource cancelSource;
        object lockthis = new object();

        int numDone;

        /// <summary>
        /// RunTasks is implemented in ParallelTaskRunner and SequentialTaskRunner. It processes each item in the data list by making separate
        /// calls to the WorkCallback function. If the client needs to accumulate results, the caller should initialize an object before calling
        /// this method, and then update it (inside a lock, or with other thread-sync protection) inside WorkCallback.
        /// If any calls to WorkCallback get an exception, or if the client cancels, the process will abort as soon as possible and CompletionCallback
        /// will be called likely before all items are processed.
        /// </summary>
        /// <param name="items">List of objects the client needs to process. Parallel runner does each on a separate thread.</param>
        /// <param name="WorkCallback">Client function that will process each item in the list.</param>
        /// <param name="ProgressCallback">Client function that will update the UI to show progress.</param>
        /// <param name="CompletionCallback">Client function called when done, or an exception, or runner is cancelled.</param>
        /// <param name="totalProgressNotifications">How many times should ProgressCallback be called?</param>
        /// <returns></returns>
        public CancellationTokenSource RunTasks(
            List<object> items,
            Action<object> WorkCallback,
            Action<int> ProgressCallback,
            Action<TaskServiceCompletionType, Exception> CompletionCallback, int totalProgressNotifications)
        {
            if (items != null && (WorkCallback == null || CompletionCallback == null))
                throw new ArgumentNullException();

            cancelSource = new CancellationTokenSource();

            // start a task separate from the caller, so the caller isn't blocked while this waits before calling CompletionCallback
            var outer = Task.Factory.StartNew(() =>
            {
                countdown = new CountdownEvent(items.Count);

                if (totalProgressNotifications > items.Count)
                    totalProgressNotifications = items.Count;

                numDone = 0;

                try
                {
                    inner_RunTasks(items, WorkCallback, ProgressCallback, CompletionCallback, totalProgressNotifications);
                }
                catch (OperationCanceledException ex)
                {
                    CompletionCallback(TaskServiceCompletionType.Cancelled, ex);

                    // abort further processing and don't signal success below
                    return;
                }
                catch (Exception ex)
                {
                    CompletionCallback(TaskServiceCompletionType.Exception, ex);

                    // abort further processing and don't signal success below
                    return;
                }

                // wait inside this "starter" task for all the sub-tasks to finish so we can reply Success to client
                countdown.Wait();
                CompletionCallback(TaskServiceCompletionType.Success, null);
            },
            cancelSource.Token);

            return cancelSource;
        }

        protected void ShowProgress(Action<int> ProgressCallback, int totalProgressNotifications)
        {
            // Allow clients to use TaskRunner without a progress function
            if (ProgressCallback == null)
                return;

            lock (lockthis)
            {
                // TODO: Interlocked shouldn't be needed here, but wasn't working with only lock
                // TODO: not sure why, but I had to use numDone instead of checking countdown.CurrentCount
                Interlocked.Increment(ref numDone);
                int percent = (int)(numDone * 100.0 / (float)countdown.InitialCount);

                // TODO: this calls back to the UI for EVERY item in the list; implement totalProgressNotifications to reduce thread-context switches!
                ProgressCallback(percent);
            }
        }

        // This method is implemented in concrete child classes
        abstract protected void inner_RunTasks(
            List<object> items,
            Action<object> workCallback,
            Action<int> progressCallback,
            Action<TaskServiceCompletionType, Exception> completionCallback, int totalProgressNotifications);

        public void Dispose()
        {
            if (cancelSource != null)
            {
                cancelSource.Dispose();
                cancelSource = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
