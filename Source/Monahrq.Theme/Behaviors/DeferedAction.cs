using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace Monahrq.Theme.Behaviors
{

	/// <summary>
	/// Represents a timer which performs an action on the UI thread when time elapses.  Rescheduling is supported.
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class DeferredAction : IDisposable
    {
		/// <summary>
		/// The timer
		/// </summary>
		private Timer timer;

		/// <summary>
		/// Creates a new DeferredAction.
		/// </summary>
		/// <param name="action">The action that will be deferred.  It is not performed until after <see cref="Defer" /> is called.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">action</exception>
		public static DeferredAction Create(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return new DeferredAction(action);
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DeferredAction"/> class.
		/// </summary>
		/// <param name="action">The action.</param>
		private DeferredAction(Action action)
        {
            this.timer = new Timer(new TimerCallback(delegate
            {
                Application.Current.Dispatcher.Invoke(action);
            }));
        }

		/// <summary>
		/// Defers performing the action until after time elapses.  Repeated calls will reschedule the action
		/// if it has not already been performed.
		/// </summary>
		/// <param name="delay">The amount of time to wait before performing the action.</param>
		public void Defer(TimeSpan delay)
        {
            // Fire action when time elapses (with no subsequent calls).
            this.timer.Change(delay, TimeSpan.FromMilliseconds(-1));
        }

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }

        #endregion
    }
}


