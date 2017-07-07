using System;
using System.Windows.Threading;
using Monahrq.DataSets.ViewModels.Validation;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Sdk.Extensions;
using PropertyChanged;

namespace Monahrq.Wing.Dynamic.Views
{
    /// <summary>
    /// Interaction logic for FullWizardValidationView.xaml
    /// </summary>
    [ImplementPropertyChanged]
    public partial class FullWizardValidationView
    {
        public FullWizardValidationView()
        {
            InitializeComponent();
            
            if (this.IsDesignMode()) return;
            Loaded += (o, e) =>
            {
                (DataContext as IValidationViewModel).LoadModel();
                (DataContext as IValidationViewModel).StartCommand.Execute(null);
            };
            DataContextChanged += (o, e) =>
                {
                    Unwire(e.OldValue as IValidationViewModel);
                    Wire(e.NewValue as IValidationViewModel);
                };
        }


        private void Unwire(IValidationViewModel model)
        {
            //Presentation = new Presenter(model);
            if (model == null) return;
            model.ValidationFailed -= ProcessEvents;
            model.ValidationSucceeded -= ProcessEvents;
            model.NotifyProgress -= Model_NotifyProgress;
            model.ValidationComplete -= model_ValidationComplete;
        }

        private void Wire(IValidationViewModel model)
        {
            //Presentation = new Presenter(model);
            if (model == null) return;
            model.ValidationFailed += ProcessEvents;
            model.ValidationSucceeded += ProcessEvents;
            model.NotifyProgress += Model_NotifyProgress;
            model.ValidationComplete += model_ValidationComplete;
        }

        void model_ValidationComplete(object sender, EventArgs e)
        {
         
        }

        void ProcessEvents(object sender, EventArgs e)
        {
            Model_NotifyProgress(sender, new ExtendedEventArgs<Action>(() => { }));
        }

        void Model_NotifyProgress(object sender, ExtendedEventArgs<Action> e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action( () =>
            {
                e.Data();
            }));
        }
    }
}
