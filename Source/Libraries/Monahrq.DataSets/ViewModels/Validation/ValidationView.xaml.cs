using System;
using System.Collections.Generic;
using System.Globalization;
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
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Sdk.Extensions;
using PropertyChanged;
using Monahrq.DataSets.ViewModels.Validation;

namespace Monahrq.DataSets.ViewModels.Validation
{
    /// <summary>
    /// Interaction logic for ValidationView.xaml
    /// </summary>
    [ImplementPropertyChanged]
    public partial class ValidationView : UserControl
    {
        public ValidationView()
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
            Model_NotifyProgress(sender, new ExtendedEventArgs<Action>(new Action(() => { })));
        }

        void Model_NotifyProgress(object sender, ExtendedEventArgs<Action> e)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
           {
               e.Data();
           }));
        }

        void Model_ValidationStarted(object sender, EventArgs e)
        {

        }
    }

    public class ImportSucessfullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var obj = value as ValidationResultsSummary;
            if (obj == null) return Visibility.Collapsed;

            return obj.CountInvalid > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

