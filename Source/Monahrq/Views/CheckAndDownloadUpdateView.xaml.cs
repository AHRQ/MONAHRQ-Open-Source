using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Monahrq.Sdk.Extensions;
using Monahrq.ViewModels;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for CheckAndDownloadUpdateView.xaml
    /// </summary>
    [Export(typeof(CheckAndDownloadUpdateView))]
    public partial class CheckAndDownloadUpdateView : IPartImportsSatisfiedNotification
    {
        public CheckAndDownloadUpdateView()
        {
            InitializeComponent();
        }

        CheckAndDownloadUpdateViewModel CheckAndDownloadUpdateViewModel
        {
            get
            {
                return DataContext as CheckAndDownloadUpdateViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        IEventAggregator Events { get; set; }

        [ImportingConstructor]
        public CheckAndDownloadUpdateView(CheckAndDownloadUpdateViewModel checkAndDownloadUpdateViewModel, IEventAggregator events)
        {
            InitializeComponent();
            Events = events;
            
            if (checkAndDownloadUpdateViewModel != null)
            {
                DataContext = checkAndDownloadUpdateViewModel;
            }
            checkAndDownloadUpdateViewModel.PropertyChanged += delegate
            {
                this.Refresh();
            };
        }

        public void OnImportsSatisfied()
        {
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            CheckAndDownloadUpdateViewModel.CancelInstallation();

            base.OnClosing(e);
        }
    }
}
