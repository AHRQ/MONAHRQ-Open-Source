using System.Windows.Controls;
using Monahrq.Infrastructure.Validation;
using ValidationError = System.Windows.Controls.ValidationError;

namespace Monahrq.Wing.NursingHomeCompare.Views
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

        
    }
}
