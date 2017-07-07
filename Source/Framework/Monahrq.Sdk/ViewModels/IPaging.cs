
namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// The inteface used with list view models that support pagination.
    /// </summary>
    public interface IPaging
    {
        /// <summary>
        /// Fetches this instance.
        /// </summary>
        void Fetch();
    }
}
