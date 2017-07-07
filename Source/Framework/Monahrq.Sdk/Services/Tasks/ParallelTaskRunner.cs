using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Services.Tasks
{
    public class ParallelTaskRunner : AbstractTaskRunner
    {
        protected override void inner_RunTasks(
            List<object> items,
            Action<object> workCallback,
            Action<int> progressCallback,
            Action<TaskServiceCompletionType, Exception> completionCallback, int totalProgressNotifications)
        {
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            po.CancellationToken = cancelSource.Token;

            // check for cancellation before starting the loop
            cancelSource.Token.ThrowIfCancellationRequested();

            // aggregate exceptions from user code
            var exceptions = new ConcurrentQueue<Exception>();

            // launch a parallel task for each item
            Parallel.For(0, items.Count, po, (k, loopstate) =>
            {
                try
                {
                    // Not really sure how this gets fired...
                    if (loopstate.ShouldExitCurrentIteration || loopstate.IsExceptional)
                        loopstate.Stop();

                    workCallback(items[k]);
                }
                catch (Exception ex)
                {
                    // catch all exceptions from user code and then throw the collection as AggregateException
                    exceptions.Enqueue(ex);
                }
                finally
                {
                    // update progress bar even if an exception occurred on the current item
                    ShowProgress(progressCallback, totalProgressNotifications);
                    countdown.Signal();
                }
            });

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }
    }
}
