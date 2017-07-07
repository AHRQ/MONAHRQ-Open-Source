using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Infrastructure.Types
{
    public class ProgressService : IDisposable
    {
        private bool _showProgressbar;
        private IEventAggregator _eventAggregator;
        private const string DEFAULT_STATUS_MESSAGE = "Ready";
        private static bool _hasCompletedBeenCalled;

        public  ProgressService()
        {
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _showProgressbar = true;
            _hasCompletedBeenCalled = false;
        }

        public async void Delay(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }

        public void Wait()
        {
            while (_showProgressbar)
            {
                //
            }
        }

        public void Finish()
        {
            if (_showProgressbar)
                _showProgressbar = false;

            _hasCompletedBeenCalled = true;
        }

        public void SetProgress(string message, int progress, bool reset, bool isIndeterminate)
        {
            if (reset)
            {
                Finish();

                Delay(500);
            }

            if (_eventAggregator != null)
            {
                _eventAggregator.GetEvent<StatusbarUpdateEvent>().Publish(new StatusbarUpdateEventObject()
                {
                    ProcessingText = message,
                    ProgressText = message,
                    Progress = progress,
                    Reset = reset,
                    IsIndeterminate = isIndeterminate
                });
            }
        }

        public async Task<bool> Execute(Action operation, Action<OperationResult<object>> completeResult,
            CancellationToken cancellationToken, Action<int> progressCallback = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();

                    operation();

                    completeResult(new OperationResult<object> {Status = true});

                    return true;
                }
                catch (Exception exc)
                {
                    completeResult(new OperationResult<object> {Status = false, Exception = exc.GetBaseException()});
                    return true;
                }
            }, 
            cancellationToken);
        }

        public async Task<ProgressServiceResult<T>> ExecuteWithResult<T>(Func<T> operation, Action<OperationResult<object>> completeResult,
            CancellationToken cancellationToken, Action<int> progressCallback = null) 
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();

                    var result = operation();

                    completeResult(new OperationResult<object> { Model = result, Status = true });

                    return Task.FromResult(new ProgressServiceResult<T> { Model = result, OperationResult = true });
                }
                catch (Exception exc)
                {
                    completeResult(new OperationResult<object> { Status = false, Exception = exc.GetBaseException() });
                    return Task.FromResult(new ProgressServiceResult<T> { OperationResult = true }); 
                }

            }, cancellationToken);
        }

        public void Dispose()
        {
            Finish();

            if (!_hasCompletedBeenCalled && _eventAggregator != null)
            {
                _eventAggregator.GetEvent<StatusbarUpdateEvent>().Publish(new StatusbarUpdateEventObject
                {
                    ProcessingText = DEFAULT_STATUS_MESSAGE,
                    ProgressText = DEFAULT_STATUS_MESSAGE,
                    Progress = 0,
                    Reset = true,
                    IsIndeterminate = false
                });

                _eventAggregator = null;
            }
        }

        ~ProgressService()
        {
            Dispose();
        }
    }

    public class StatusbarUpdateEvent : CompositePresentationEvent<StatusbarUpdateEventObject> { }

    public class StatusbarUpdateEventObject
    {
        public string ProcessingText { get; set; }

        public string ProgressText { get; set; }
        public int Progress { get; set; }

        public bool Reset { get; set; }

        public bool IsIndeterminate { get; set; }
    }
}
