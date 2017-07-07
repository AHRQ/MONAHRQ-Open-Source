using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Attributes;

namespace Monahrq.Sdk.DataProvider.Builder
{
    /// <summary>
    /// Interaction logic for ConnectinStringBuilderView.xaml
    /// </summary>
    [ViewExport(typeof(IConnectionStringBuilderView))]
    public partial class ConnectionStringBuilderView : UserControl, IConnectionStringBuilderView
    {
     
        [Import]
        public IConnectionStringBuilderViewModel Model 
        {
            get { return DataContext as IConnectionStringBuilderViewModel; }
            private set { DataContext = value; } 
        }

        public ConnectionStringBuilderView()
        {
            InitializeComponent();
            this.Loaded+=(o,e) =>
            {
                if (!Model.ProviderExports.IsEmpty)
                {
                    Model.ProviderExports.MoveCurrentToFirst();
                }
            };
        }
    }
}
