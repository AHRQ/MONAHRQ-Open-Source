using System;
using System.Windows;
using System.Windows.Threading;

using Monahrq.Sdk.Extensions;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Wing.Dynamic.ViewModels;

namespace Monahrq.Wing.Dynamic.Views
{
    /// <summary>
    /// Interaction logic for FullWizardFileReadabilityView.xaml
    /// </summary>
    public partial class FullWizardFileReadabilityView
    {
        public FullWizardFileReadabilityView()
        {
            InitializeComponent();
            DataContextChanged += (o, e) =>
                {
                    var model = e.OldValue as FullWizardFileReadabilityViewModel;
                    if (model != null)
                    {
                        model.NotifyProgress -= Model_NotifyProgress;
                        model.Verified -= Model_Verified;
                        model.Failed += Model_Failed;
                    }
                    model = e.NewValue as FullWizardFileReadabilityViewModel;
                    if (model != null)
                    {
                        model.NotifyProgress += Model_NotifyProgress;
                        model.Verified += Model_Verified;
                        model.Failed += Model_Failed;
                    }
                };
        }

        private void Model_Failed(object sender, EventArgs e)
        {
            MessageBox.Show(Application.Current.MainWindow, "Unable to read data file", "MONAHRQ", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Model_Verified(object sender, EventArgs e)
        {
            //SZ: commented this out to save user mouse-clicks now that the "Next button not enabled" issue is fixed
            //MessageBox.Show(Application.Current.MainWindow, "Datafile verified", "MONAHRQ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Model_NotifyProgress(object sender, ExtendedEventArgs<Action> e)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, e.Data);
            this.Refresh();
        }

        FullWizardFileReadabilityViewModel Model
        {
            get { return DataContext as FullWizardFileReadabilityViewModel; }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Model.Loaded();
        }
    }
}
