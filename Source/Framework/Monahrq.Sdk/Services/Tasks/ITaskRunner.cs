using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Monahrq.Sdk.Services.Tasks
{
    interface ITaskRunner
    {
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
        CancellationTokenSource RunTasks(
            List<object> items,
            Action<object> WorkCallback,
            Action<int> ProgressCallback,
            Action<TaskServiceCompletionType, Exception> CompletionCallback, int totalProgressNotifications);
    }
}
