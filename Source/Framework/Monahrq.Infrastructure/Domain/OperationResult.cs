//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Monahrq.Infrastructure.Entities.Domain
//{
//    public interface IOperationResult
//    {
//        Exception Error { get; }
//        bool Successful { get; }
//        bool Failed { get;  }
//    }

//    public interface IOperationResult<T> : IOperationResult
//    {
//        T Result { get; }
//    }

//    public class OperationResult : IOperationResult
//    {
//        protected ILogWriter Logger
//        {
//            get;
//            private set;
//        }

//        protected virtual void LogError(Exception ex)
//        {
//            if (Logger != null)
//            {
//                Logger.Write(ex);
//            }
//        }

//        public OperationResult() { }
//        public OperationResult(Exception error, ILogWriter logger)
//        {
//            Logger = logger;
//            Error = error;
//        }

//        DomainOperationException OpError { get; set; }
//        public Exception Error
//        {
//            get { return OpError; }
//            private
//            set
//            {
//                OpError = new DomainOperationException();
//                LogError(value);
//            }
//        }
//        public bool Successful { get { return Error == null; } }
//        public bool Failed { get { return !Successful; } }
//    }

//    public class OperationResult<T> : OperationResult, IOperationResult<T>
//    {
//        public OperationResult()
//            : base()
//        {

//        }
//        public OperationResult(T result)
//            : this()
//        {
//            Result = result;
//        }
//        public OperationResult(Exception error, ILogWriter logger)
//            : base(error, logger)
//        {
//        }

//        public T Result
//        {
//            get;
//            private set;
//        }

//        protected override void LogError(Exception ex)
//        {
//            Logger.Write(Error, TraceEventType.Error, new Dictionary<string, string>()
//            {
//                {"Model",  typeof(T).Name}
//            });
//        }
//    }

//    public class DomainOperationException : Exception
//    {
//        public DomainOperationException()
//            : base("An error occured during a domain repository operation" + 
//                Environment.NewLine + "Please refer to the session event log for details")
//        {
//        }
//    }

//}
