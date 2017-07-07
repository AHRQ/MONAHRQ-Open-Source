using Monahrq.Sdk.Services.Contracts;
using Monahrq.Theme.Controls.Wizard.Models;
using System;
using Monahrq.Infrastructure.Entities.Events;
namespace Monahrq.DataSets.ViewModels.Validation
{
    /// <summary>
    /// The dataset import wizard validation viewmodel interface.
    /// </summary>
    public interface IValidationViewModel
    {
        /// <summary>
        /// Gets the button caption.
        /// </summary>
        /// <value>
        /// The button caption.
        /// </value>
        string ButtonCaption { get; }
        /// <summary>
        /// Gets the current file.
        /// </summary>
        /// <value>
        /// The current file.
        /// </value>
        string CurrentFile { get; }
        /// <summary>
        /// Gets the current progress.
        /// </summary>
        /// <value>
        /// The current progress.
        /// </value>
        IProgressState CurrentProgress { get; }
        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        string DisplayName { get; }
        /// <summary>
        /// Gets the feedback.
        /// </summary>
        /// <value>
        /// The feedback.
        /// </value>
        string Feedback { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        bool IsRunning { get; }
        /// <summary>
        /// Occurs when [notify progress].
        /// </summary>
        event EventHandler<ExtendedEventArgs<Action>> NotifyProgress;
        /// <summary>
        /// Gets the results summary.
        /// </summary>
        /// <value>
        /// The results summary.
        /// </value>
        IValidationResultsSummary ResultsSummary { get; }
        /// <summary>
        /// Gets the results view.
        /// </summary>
        /// <value>
        /// The results view.
        /// </value>
        System.Windows.Data.ListCollectionView ResultsView { get; }
        /// <summary>
        /// Gets the start command.
        /// </summary>
        /// <value>
        /// The start command.
        /// </value>
        System.Windows.Input.ICommand StartCommand { get; }
        /// <summary>
        /// Occurs when [validation complete].
        /// </summary>
        event EventHandler ValidationComplete;
        /// <summary>
        /// Occurs when [validation failed].
        /// </summary>
        event EventHandler ValidationFailed;
        /// <summary>
        /// Occurs when [validation started].
        /// </summary>
        event EventHandler ValidationStarted;
        /// <summary>
        /// Occurs when [validation succeeded].
        /// </summary>
        event EventHandler ValidationSucceeded;

        /// <summary>
        /// Loads the model.
        /// </summary>
        void LoadModel();
    }
}
