using System;

namespace Monahrq.Theme.Controls
{
	/// <summary>
	/// Checks if event exists before raising it.
	/// </summary>
	internal static class SafeRaise
    {
		/// <summary>
		/// Raises the specified event to raise.
		/// </summary>
		/// <param name="eventToRaise">The event to raise.</param>
		/// <param name="sender">The sender.</param>
		public static void Raise(EventHandler eventToRaise, object sender)
        {
            if (eventToRaise != null)
            {
                eventToRaise(sender, EventArgs.Empty);
            }
        }

		/// <summary>
		/// Raises the specified event to raise.
		/// </summary>
		/// <param name="eventToRaise">The event to raise.</param>
		/// <param name="sender">The sender.</param>
		public static void Raise(EventHandler<EventArgs> eventToRaise, object sender)
        {
            Raise(eventToRaise, sender, EventArgs.Empty);
        }

		/// <summary>
		/// Raises the specified event to raise.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="eventToRaise">The event to raise.</param>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The arguments.</param>
		public static void Raise<T>(EventHandler<T> eventToRaise, object sender, T args) where T : EventArgs
        {
            if (eventToRaise != null)
            {
                eventToRaise(sender, args);
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public delegate T GetEventArgs<T>() where T : EventArgs;

		/// <summary>
		/// Raises the specified event to raise.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="eventToRaise">The event to raise.</param>
		/// <param name="sender">The sender.</param>
		/// <param name="getEventArgs">The <see cref="GetEventArgs{T}"/> instance containing the event data.</param>
		public static void Raise<T>(EventHandler<T> eventToRaise, object sender, GetEventArgs<T> getEventArgs) where T : EventArgs
        {
            if (eventToRaise != null)
            {
                eventToRaise(sender, getEventArgs());
            }
        }
    }
}