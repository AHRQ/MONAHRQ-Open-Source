using System.Windows.Controls;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    ///  A route binds a view to a view model
    /// </summary>
    /// <remarks>
    /// Export the route to indicate what view model should be used and bound when a view is navigated to
    /// </remarks>
    public class ViewModelRoute
    {
        /// <summary>
        /// No public instantation
        /// </summary>
        private ViewModelRoute()
        {
        }

        /// <summary>
        ///     Safely typed creation - uses the full names for the types
        /// </summary>
        /// <typeparam name="TViewModel">The view model</typeparam>
        /// <typeparam name="TView">The view</typeparam>
        /// <returns>The route</returns>
        public static ViewModelRoute Create<TViewModel, TView>()
            where TViewModel : IViewModel
            where TView : UserControl
        {
            return new ViewModelRoute { ViewModelType = typeof(TViewModel).FullName, ViewType = typeof(TView).FullName };
        }

        /// <summary>
        /// Create using tags 
        /// </summary>
        /// <param name="viewModelType">The view model tag</param>
        /// <param name="viewType">The view tag</param>
        /// <returns>A new instance of the route</returns>
        public static ViewModelRoute Create(string viewModelType, string viewType)
        {
            return new ViewModelRoute { ViewModelType = viewModelType, ViewType = viewType };
        }

        /// <summary>
        /// Tag for the view model
        /// </summary>
        public string ViewModelType { get; private set; }

        /// <summary>
        /// Tag for the view type
        /// </summary>
        public string ViewType { get; private set; }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            return string.Format(Resources.ViewModelRoute_ToString_ViewModelRoute, ViewModelType, ViewType);
        }
    }
}
