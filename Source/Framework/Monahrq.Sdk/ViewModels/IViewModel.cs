using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    ///     View model interface
    /// </summary>
    public interface IViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     Go to visual state action
        /// </summary>
        Action<string, bool> GoToVisualState { get; set; }

        /// <summary>
        ///     List of view tags registered to the current view model
        /// </summary>
        List<string> RegisteredViews { get; }

        /// <summary>
        ///     Register the visual state transition
        /// </summary>
        /// <param name="view">The tag of the view being bound</param>
        /// <param name="action">The delegate to set a visual state on the view</param>
        void RegisterVisualState(string view, Action<string, bool> action);

        /// <summary>
        ///     Visual state for a specific view
        /// </summary>
        /// <param name="view">The view</param>
        /// <param name="state">The state</param>
        /// <param name="useTransitions">Use transitions?</param>
        /// <returns>True if the view is registered</returns>
        bool GoToVisualStateForView(string view, string state, bool useTransitions);

        /// <summary>
        ///     Called first time the view model is created
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Called whenever the view model has a corresponding view come into focus
        /// </summary>
        void Activate(string viewName);

        /// <summary>
        ///     Called whenever the view model has a corresponding view come into focus
        /// </summary>
        void Activate(string viewName, IDictionary<string, object> parameters);

        /// <summary>
        ///     Called whenever a corresponding view goes out of focus
        /// </summary>
        void Deactivate(string viewName);

        /// <summary>
        ///     Returns true when in design mode
        /// </summary>
        bool InDesigner { get; }

        /// <summary>
        ///     Sets the title, whether in the browser or in Out of Browser
        /// </summary>
        /// <param name="title">The title to set</param>
        void SetTitle(string title);
    }
}
