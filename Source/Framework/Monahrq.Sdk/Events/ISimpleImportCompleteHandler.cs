namespace Monahrq.Sdk.Events
{
    /// <summary>
    /// Interface for the Simple Import Complete Handler
    /// </summary>
    public interface ISimpleImportCompleteHandler
    {
        /// <summary>
        /// Handles the specified import result.
        /// </summary>
        /// <param name="result">The result.</param>
        void Handle(Monahrq.Sdk.Events.ISimpleImportCompletedPayload result);
    }
}
