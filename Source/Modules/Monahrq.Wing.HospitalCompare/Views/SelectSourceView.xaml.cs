using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;
using Monahrq.Infrastructure.Validation;
using ValidationError = System.Windows.Controls.ValidationError;

namespace Monahrq.Wing.HospitalCompare.Views
{
    /// <summary>
    /// Interaction logic for SelectSourceView.xaml
    /// </summary>
    public partial class SelectSourceView
    {
        public SelectSourceView()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                var monthExpr = this.SelectMonth.GetBindingExpression(ComboBox.SelectedValueProperty);
                var monthErr = new ValidationError(new RequiredRule(), monthExpr);
                Validation.MarkInvalid(monthExpr, monthErr);

                var yearExpr = this.SelectYear.GetBindingExpression(ComboBox.SelectedValueProperty);
                var yearErr = new ValidationError(new RequiredRule(), yearExpr);
                Validation.MarkInvalid(yearExpr, yearErr);
            };
        }

        void HyperlinkNavigateHandler(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
