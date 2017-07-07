namespace Monahrq.Sdk.Events
{
    /// <summary>
    /// The Empty entity/object
    /// </summary>
    public class Empty
    {
        private static readonly Empty _value = new Empty();

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public static Empty Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Empty"/> class from being created.
        /// </summary>
        private Empty()
        {
        }
    }

}
