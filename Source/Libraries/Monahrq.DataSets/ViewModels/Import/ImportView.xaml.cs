using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Monahrq.Sdk.Extensions;

namespace Monahrq.DataSets.ViewModels.Import
{
    /// <summary>
    /// Interaction logic for ImportView.xaml
    /// </summary>
    public partial class ImportView : UserControl
    {
        public ImportView()
        {
            InitializeComponent();
            if (this.IsDesignMode()) return;
            Loaded += (o, e) =>
            {
                (DataContext as ImportViewModel).LoadModel();
            };
            DataContextChanged += (o, e) =>
                {
                    Unwire(e.OldValue as ImportViewModel);
                    Wire(e.NewValue as ImportViewModel);
                };
        }


        private void Unwire(ImportViewModel model)
        {
            if (model == null) return;
            model.ImportFailed -= ProcessEvents;
            model.ImportSucceeded -= ProcessEvents;
            model.NotifyProgress -= Model_NotifyProgress;
            model.ImportComplete -= model_ValidationComplete;
        }

        private void Wire(ImportViewModel model)
        {
            //Presentation = new Presenter(model);
            if (model == null) return;
            model.ImportFailed += ProcessEvents;
            model.ImportSucceeded += ProcessEvents;
            model.NotifyProgress += Model_NotifyProgress;
            model.ImportComplete += model_ValidationComplete;
        }

        void model_ValidationComplete(object sender, EventArgs e)
        {

        }

        void ProcessEvents(object sender, EventArgs e)
        {
            Model_NotifyProgress(sender, new Sdk.Events.EventArgsBase<Action>(new Action(() => { })));
        }

        void Model_NotifyProgress(object sender, Sdk.Events.EventArgsBase<Action> e)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, e.Data);
            Application.Current.DoEvents();
        }

        void Model_ValidationStarted(object sender, EventArgs e)
        {

        }

    }
}
