using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Sdk.Extensions;
using Monahrq.Wing.NursingHomeCompare.NHC.ViewModel;

namespace Monahrq.Wing.NursingHomeCompare.NHC.Views
{
    /// <summary>
    /// Interaction logic for ProcessFileView.xaml
    /// </summary>
    [Export("ProcessFileView")]
    public partial class ProcessFileView : UserControl
    {
        public bool ControlIsLoaded { get; set; }

        [ImportingConstructor]
        public ProcessFileView()
        {
            InitializeComponent();
            DataContextChanged += (o, e) =>
            {
                var model = e.OldValue as ProcessFileViewModel;
                if (model != null)
                {
                    model.NotifyUi -= Model_CountUpdated;
                }
                model = e.NewValue as ProcessFileViewModel;
                if (model != null)
                {
                    model.NotifyUi += Model_CountUpdated;
                }
            };

            Loaded += delegate
            {
                ImportData();
            };
        }
        
        ProcessFileViewModel Model
        {
            get { return DataContext as ProcessFileViewModel; }
        }

        void Model_CountUpdated(object sender, ExtendedEventArgs<Action> e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                e.Data();
                this.Refresh();
            }));
            Application.Current.DoEvents();
        }

        public void ImportData()
        {
            var model = Model;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
           {
                model.StartImport();
               //model.StartOldImport();

           }));
            

        }

    }

}
