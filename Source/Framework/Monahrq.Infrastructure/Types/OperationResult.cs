using System;

namespace Monahrq.Infrastructure.Types
{
    public class OperationResult<T> where T : class, new()
    {
        public bool Status { get; set; }
        public T Model { get; set; }
        public Exception Exception { get; set; }
    }

    public class ProgressServiceResult<T>
    {
        public bool OperationResult { get; set; }
        public T Model { get; set; }
    }
}
