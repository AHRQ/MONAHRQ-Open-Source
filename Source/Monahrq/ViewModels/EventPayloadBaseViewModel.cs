namespace Monahrq.ViewModels
{
    /// <summary>
    /// Class foor event pay load
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventPayloadBaseViewModel<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventPayloadBaseViewModel{T}"/> class.
        /// </summary>
        /// <param name="payload">The payload.</param>
        public EventPayloadBaseViewModel(T payload)
        {
            Payload = payload;
        }
        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        public T Payload { get; private set; }
    }
}
