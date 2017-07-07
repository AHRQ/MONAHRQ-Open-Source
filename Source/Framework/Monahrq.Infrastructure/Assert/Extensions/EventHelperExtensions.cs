using System;
using System.Diagnostics;

namespace Monahrq.Infrastructure.Assert.Extensions
{
    /// <summary>
    /// Defines extension methods for the <see cref="EventHelper"/> class.
    /// </summary>
    /// <remarks>
    /// This class defines extensions methods for the <see cref="EventHelper"/>. All extension methods simply delegate to the
    /// appropriate member of the <see cref="EventHelper"/> class.
    /// </remarks>
    /// <example>
    /// The following example shows how a generic event can be raised:
    /// <code>
    /// public event EventHandler&lt;EventArgs&gt; Changed;
    /// 
    /// protected void OnChanged()
    /// {
    ///     Changed.Raise(this, EventArgs.Empty);
    /// }
    /// </code>
    /// </example>
    public static class EventHelperExtensions
    {
        /// <summary>
        /// Raises the specified sender.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="sender">The sender.</param>
        /// <include file="../EventHelper.doc.xml" path="doc/member[@name=&quot;Raise(EventHandler,object)&quot;]/*" />
        [DebuggerHidden]
        public static void Raise(this EventHandler handler, object sender)
        {
            EventHelper.Raise(handler, sender);
        }

        /// <summary>
        /// Raises the specified sender.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <include file="../EventHelper.doc.xml" path="doc/member[@name=&quot;Raise{T}(EventHandler{T},object,T)&quot;]/*" />
        [DebuggerHidden]
        public static void Raise<T>(this EventHandler<T> handler, object sender, T e)
            where T : EventArgs
        {
            EventHelper.Raise(handler, sender, e);
        }

        /// <summary>
        /// Raises the specified sender.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="createEventArguments">The create event arguments.</param>
        /// <include file="../EventHelper.doc.xml" path="doc/member[@name=&quot;Raise{T}(EventHandler{T},object,Func{T})&quot;]/*" />
        [DebuggerHidden]
        public static void Raise<T>(this EventHandler<T> handler, object sender, Func<T> createEventArguments)
            where T : EventArgs
        {
            EventHelper.Raise(handler, sender, createEventArguments);
        }

        /// <summary>
        /// Raises the specified sender.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <include file="../EventHelper.doc.xml" path="doc/member[@name=&quot;Raise(Delegate,object,EventArgs)&quot;]/*" />
        [DebuggerHidden]
        public static void Raise(this Delegate handler, object sender, EventArgs e)
        {
            EventHelper.Raise(handler, sender, e);
        }

        /// <summary>
        /// Begins the raise.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="asyncState">State of the asynchronous.</param>
        /// <include file="../EventHelper.doc.xml" path="doc/member[@name=&quot;BeginRaise(EventHandler,object,AsyncCallback,object)&quot;]/*" />
        [DebuggerHidden]
        public static void BeginRaise(this EventHandler handler, object sender, AsyncCallback callback, object asyncState)
        {
            EventHelper.BeginRaise(handler, sender, callback, asyncState);
        }

        /// <summary>
        /// Begins the raise.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="asyncState">State of the asynchronous.</param>
        /// <include file="../EventHelper.doc.xml" path="doc/member[@name=&quot;BeginRaise{T}(EventHandler{T},object,T,AsyncCallback,object)&quot;]/*" />
        [DebuggerHidden]
        public static void BeginRaise<T>(this EventHandler<T> handler, object sender, T e, AsyncCallback callback, object asyncState)
            where T : EventArgs
        {
            EventHelper.BeginRaise(handler, sender, e, callback, asyncState);
        }

        /// <summary>
        /// Begins the raise.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="createEventArguments">The create event arguments.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="asyncState">State of the asynchronous.</param>
        /// <include file="../EventHelper.doc.xml" path="doc/member[@name=&quot;BeginRaise{T}(EventHandler{T},object,Func{T},AsyncCallback,object)&quot;]/*" />
        [DebuggerHidden]
        public static void BeginRaise<T>(this EventHandler<T> handler, object sender, Func<T> createEventArguments, AsyncCallback callback, object asyncState)
            where T : EventArgs
        {
            EventHelper.BeginRaise(handler, sender, createEventArguments, callback, asyncState);
        }

        /// <summary>
        /// Begins the raise.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="asyncState">State of the asynchronous.</param>
        /// <include file="../EventHelper.doc.xml" path="doc/member[@name=&quot;BeginRaise(Delegate,object,EventArgs,AsyncCallback,object)&quot;]/*" />
        [DebuggerHidden]
        public static void BeginRaise(this Delegate handler, object sender, EventArgs e, AsyncCallback callback, object asyncState)
        {
            EventHelper.BeginRaise(handler, sender, e, callback, asyncState);
        }
    }
}