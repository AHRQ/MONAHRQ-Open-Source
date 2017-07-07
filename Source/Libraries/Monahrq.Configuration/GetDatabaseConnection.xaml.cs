using Microsoft.Practices.ServiceLocation;
using Monahrq.Configuration.HostConnection;
using System.Windows;
using System.ComponentModel.Composition;
using System;

namespace Monahrq.Configuration
{
    /// <summary>
    /// Interaction logic for GetDatabaseConnection.xaml
    /// </summary>
    [Export(typeof(GetDatabaseConnection))]
    public partial class GetDatabaseConnection : IPartImportsSatisfiedNotification  //: Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetDatabaseConnection"/> class.
        /// </summary>
        public GetDatabaseConnection()
        {
            InitializeComponent();
            //DataContext = new HostConnectionViewModel();
            Model = new HostConnectionViewModel();
            Model.Connected += (o, e) => this.IsEnabled = true;
            Model.Connecting += (o, e) => this.IsEnabled = false;
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>

        public HostConnectionViewModel Model
        {
            get { return DataContext as HostConnectionViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        /// Handles the Click event of the Cancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        /// <summary>
        /// Handles the Click event of the OK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!DialogResult.GetValueOrDefault()) return;

            base.OnClosing(e);

            // If the model isn't dirty, it means the user already created or tested the database and hasn't changed any settings since then.
            // So in that case, we shouldn't display another "Connection succeeded" message box.
            if (!Model.IsDirty)
                return;

            bool testFailed = false;
            EventHandler handler = (o, ev) => testFailed = true;
            Model.TestFailed += handler;
            Model.TestCommand.Execute();
            Model.TestFailed -= handler;
            e.Cancel = testFailed;
            if (testFailed)
            {
                MessageBox.Show(this, "Please specify a valid connection.", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void OnImportsSatisfied()
        {
        }
    }
}
