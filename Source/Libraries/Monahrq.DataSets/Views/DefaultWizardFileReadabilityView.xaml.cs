using System;
using System.Windows;
using System.Windows.Controls;
using Monahrq.DataSets.ViewModels;
using System.Windows.Threading;

using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Events;
using Monahrq.Infrastructure.Entities.Events;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for DefaultWizardFileReadabilityView.xaml
    /// </summary>
    public partial class DefaultWizardFileReadabilityView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardFileReadabilityView"/> class.
        /// </summary>
        public DefaultWizardFileReadabilityView()
        {
            InitializeComponent();
            DataContextChanged += (o, e) =>
                {
                    var model = e.OldValue as DefaultWizardFileReadabilityViewModel;
                    if (model != null)
                    {
                        model.NotifyProgress -= Model_NotifyProgress;
                        model.Verified -= Model_Verified;
                        model.Failed += Model_Failed;
                    }
                    model = e.NewValue as DefaultWizardFileReadabilityViewModel;
                    if (model != null)
                    {
                        model.NotifyProgress += Model_NotifyProgress;
                        model.Verified += Model_Verified;
                        model.Failed += Model_Failed;
                    }
                };
        }

        /// <summary>
        /// Handles the Failed event of the Model control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Model_Failed(object sender, EventArgs e)
        {
            MessageBox.Show(Application.Current.MainWindow, "Unable to read data file", "MONAHRQ", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Handles the Verified event of the Model control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Model_Verified(object sender, EventArgs e)
        {
            //SZ: commented this out to save user mouse-clicks now that the "Next button not enabled" issue is fixed
            //MessageBox.Show(Application.Current.MainWindow, "Datafile verified", "MONAHRQ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Handles the NotifyProgress event of the Model control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExtendedEventArgs{Action}"/> instance containing the event data.</param>
        private void Model_NotifyProgress(object sender, ExtendedEventArgs<Action> e)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, e.Data);
            this.Refresh();
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        DefaultWizardFileReadabilityViewModel Model
        {
            get { return DataContext as DefaultWizardFileReadabilityViewModel; }
        }

        /// <summary>
        /// Handles the Loaded event of the UserControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Model.Loaded();
        }
    }
}
