using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.ViewModels;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for InstallDynamicWingTargetView.xaml
    /// </summary>
    [Export(typeof(ManageWingsFluttersView))] 
    public partial class ManageWingsFluttersView : UserControl
    { 
        /// <summary>
        /// Initializes a new instance of the <see cref="ManageWingsFluttersView"/> class.
        /// </summary>
        public ManageWingsFluttersView()
        {
            try
            {
                InitializeComponent();
                
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        ///// <summary>
        ///// Sets the model.
        ///// </summary>
        ///// <value>
        ///// The model.
        ///// </value>
        //[Import]
        //protected ManageWingFluttersViewModel Model
        //{
        //    set
        //    {
        //        DataContext = value;
        //    }
        //}
    }
}
