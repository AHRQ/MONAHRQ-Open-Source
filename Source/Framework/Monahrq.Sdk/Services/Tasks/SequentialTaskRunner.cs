using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Services.Tasks
{
    public class SequentialTaskRunner : AbstractTaskRunner
    {
        protected override void inner_RunTasks(
            List<object> items,
            Action<object> workCallback,
            Action<int> progressCallback,
            Action<TaskServiceCompletionType, Exception> completionCallback, int totalProgressNotifications)
        {
            // aggregate exceptions from user code
            var exceptions = new ConcurrentQueue<Exception>();

            for (var k = 0; k < items.Count; k++)
            {
                cancelSource.Token.ThrowIfCancellationRequested();

                try
                {
                    workCallback(items[k]);
                }
                catch (Exception ex)
                {
                    // catch all exceptions from user code and then throw the collection as AggregateException
                    exceptions.Enqueue(ex);
                }
                finally
                {
                    // always decrement countdown and update progress bar, even if an exception occurred on the current item
                    ShowProgress(progressCallback, totalProgressNotifications);
                    countdown.Signal();
                }

                if (exceptions.Count > 0)
                    throw new AggregateException(exceptions);
            }
        }
    }
}
