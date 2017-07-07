using System;

namespace Monahrq.Infrastructure.Services
{
    /// <summary>
    /// A custom abstract data domain service. This class handles the disposable methods/functionality.
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    /// <seealso cref="Monahrq.Infrastructure.Services.IDataServiceBase" />
    /// <seealso cref="System.IDisposable" />
    public abstract partial class DataServiceBase : IDisposable
    {
        // Track whether Dispose has been called. 
        private bool disposed = false;
       
        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if(!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if(disposing)
                {
                    ReleaseSessions();
                }

                disposed = true;

            }
        }

        /// <summary>
        /// Releases the sessions.
        /// </summary>
        private void ReleaseSessions()
        {
            //// Dispose managed resources.
            //try
            //{
            //    LazySession.Value.Dispose();
            //}
            //finally
            //{
            //    try
            //    {
            //        LazyStatelessSession.Value.Dispose();
            //    }
            //    finally
            //    {
            //        try
            //        {
            //            LazySession = null;
            //        }
            //        finally
            //        {
            //            LazyStatelessSession = null;
            //        }
            //    }
            //}
        }
 
        // Use C# destructor syntax for finalization code. 
        // This destructor will run only if the Dispose method 
        // does not get called. 
        // It gives your base class the opportunity to finalize. 
        // Do not provide destructors in types derived from this class.
        ~DataServiceBase()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability.
            Dispose(false);
        }
    }
}
