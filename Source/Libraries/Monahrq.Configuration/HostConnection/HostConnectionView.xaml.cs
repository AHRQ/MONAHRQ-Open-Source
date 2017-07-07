using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monahrq.Configuration.HostConnection
{
    /// <summary>
    /// Interaction logic for HostConnectionView.xaml
    /// </summary>
    public partial class HostConnectionView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostConnectionView"/> class.
        /// </summary>
        public HostConnectionView()
        {
            InitializeComponent();
            password.PasswordChanged+=(o,e)=> Model.Password = password.Password;
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        [Import]
        public HostConnectionViewModel Model 
        {
            get { return DataContext as HostConnectionViewModel; }

            set { DataContext = value; } 
        }
    }
}
