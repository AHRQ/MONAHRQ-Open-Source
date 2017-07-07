using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Monahrq.Theme.Controls {
	/// <summary>
	/// Overrides the current cursor shown to user.  A history of cursors is tracked to allow
	/// previous cursor to be restored when done.
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class OverrideCursor : IDisposable {
		/// <summary>
		/// The s stack
		/// </summary>
		static Stack<Cursor> s_Stack = new Stack<Cursor>();

		/// <summary>
		/// Initializes a new instance of the <see cref="OverrideCursor"/> class.
		/// </summary>
		public OverrideCursor() {

        }
		/// <summary>
		/// Initializes a new instance of the <see cref="OverrideCursor"/> class.
		/// </summary>
		/// <param name="newCursor">The new cursor.</param>
		public OverrideCursor(Cursor newCursor) {
            PushCursor(newCursor);
        }

		/// <summary>
		/// Pushes the cursor.
		/// </summary>
		/// <param name="newCursor">The new cursor.</param>
		public void PushCursor(Cursor newCursor) {
            s_Stack.Push(newCursor);

            if (Mouse.OverrideCursor != newCursor)
                Mouse.OverrideCursor = newCursor;
        }
		/// <summary>
		/// Pops the cursor.
		/// </summary>
		public void PopCursor() {
            s_Stack.Pop();

            Cursor cursor = s_Stack.Count > 0 ? s_Stack.Peek() : null;

            if (cursor != Mouse.OverrideCursor)
                Mouse.OverrideCursor = cursor;
        }
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
            PopCursor();
        }

    }
}
