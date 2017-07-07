using System;
using System.Windows.Input;

namespace Monahrq.Infrastructure
{
    /// <summary>
    /// The wait cursor class. Handles custom UI wait cursor.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class WaitCursor : IDisposable
    {
        /// <summary>
        /// The previous cursor
        /// </summary>
        private Cursor _previousCursor;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitCursor"/> class.
        /// </summary>
        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }

        #endregion
    }
}
