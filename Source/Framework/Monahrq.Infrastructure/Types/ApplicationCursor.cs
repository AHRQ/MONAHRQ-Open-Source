using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Monahrq.Infrastructure.Types
{
    public class ApplicationCursor : IDisposable
    {
        #region Singleton code

        private static readonly Stack<Cursor> _stack = new Stack<Cursor>();
        private static ApplicationCursor SingletonCursor { get; set; }
        private static readonly object _lockNewCursor = new object();

        public static ApplicationCursor CurrentCursor()
        {
            //this code implements double-check, multithread safe, Singleton implementation
            if (SingletonCursor == null)
                lock (_lockNewCursor)
                    if (SingletonCursor == null) SingletonCursor = new ApplicationCursor();

            return SingletonCursor;
        }

        public static ApplicationCursor SetCursor(Cursor newCursor)
        {
            var activeCursor = CurrentCursor();
            activeCursor.Push(newCursor);
            return activeCursor;
        }

        private ApplicationCursor()
        {
        }

        #endregion


        public void Push(Cursor newCursor)
        {
            _stack.Push(Mouse.OverrideCursor);

            Mouse.OverrideCursor = newCursor;
        }

        public void Pop()
        {
            Cursor cursor = _stack.Count > 0 ? _stack.Peek() : null;
            if (cursor != null) _stack.Pop();

            Mouse.OverrideCursor = cursor;
        }

        public void Dispose()
        {
            Pop();
        }

        ~ApplicationCursor()
        {
            _stack.Clear();
            //Dispatcher.CurrentDispatcher.Invoke(() => Mouse.OverrideCursor.Dispose());
        }

    }
}
