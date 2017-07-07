//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Transactions;
//using Monahrq.Infrastructure;
//using System.ComponentModel.Composition;

//namespace Monahrq.Sdk.Extensibility.Data
//{
    //public interface ITransactionManager 
    //{
    //    void Demand();
    //    void Cancel();
    //}

    //public class TransactionManager : ITransactionManager, IDisposable
    //{
    //    private TransactionScope _scope;
    //    private bool _cancelled;

    //    [ImportingConstructor]
    //    public TransactionManager([Import(LogNames.Operations)] ILogWriter logger)
    //    {
    //        Logger = logger;
    //    }
        
    //    public TransactionManager()
    //    {
    //        Logger = NullLogger.Instance;
    //    }
        
    //    [Import(LogNames.Operations)]
    //    public ILogWriter Logger { get; set; }

    //    void ITransactionManager.Demand()
    //    {
    //        if (_cancelled)
    //        {
    //            try
    //            {
    //                _scope.Dispose();
    //            }
    //            catch
    //            {
    //                // swallowing the exception
    //            }

    //            _scope = null;
    //        }

    //        if (_scope == null)
    //        {
    //            Logger.Debug("Creating transaction on Demand");
    //            _scope = new TransactionScope(
    //                TransactionScopeOption.Required,
    //                new TransactionOptions
    //                {
    //                    IsolationLevel = IsolationLevel.ReadCommitted
    //                });
    //        }
    //    }

    //    void ITransactionManager.Cancel()
    //    {
    //        Logger.Debug("Transaction cancelled flag set");
    //        _cancelled = true;
    //    }

    //    public void Dispose()
    //    {
    //        if (_scope != null)
    //        {
    //            if (!_cancelled)
    //            {
    //                Logger.Debug("Marking transaction as complete");
    //                _scope.Complete();
    //            }

    //            Logger.Debug("Final work for transaction being performed");
    //            try
    //            {
    //                _scope.Dispose();
    //            }
    //            catch
    //            {
    //                // swallowing the exception
    //            }
    //            Logger.Debug("Transaction disposed");
    //        }
    //    }

    //}

    //[Export(typeof(ITransactionManager))]
    //[PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    //public class NullTransactionManager : ITransactionManager
    //{
    //    private static ITransactionManager NullInstance = new NullTransactionManager();
    //    public static ITransactionManager Instance { get { return NullInstance; } }

    //    public NullTransactionManager()
    //    { 
    //    }
    //    public void Demand()
    //    {
    //    }

    //    public void Cancel()
    //    {
    //    }
    //}
//}
