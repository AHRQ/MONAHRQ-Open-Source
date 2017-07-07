using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Commands;
using PropertyChanged;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// Measrues UI model class
    /// </summary>
    [ImplementPropertyChanged]
    public class MeasuresUIModel
    {
        /// <summary>
        /// The spliter width on
        /// </summary>
        public const string SPLITER_WIDTH_ON="7";
        /// <summary>
        /// The detail view width on
        /// </summary>
        public const string DETAIL_VIEW_WIDTH_ON = "0.3*";
        /// <summary>
        /// The data view width on
        /// </summary>
        public const string DATA_VIEW_WIDTH_ON = "0.7*";
        /// <summary>
        /// The zero width
        /// </summary>
        public const string ZERO_WIDTH = "0";


        /// <summary>
        /// Initializes a new instance of the <see cref="MeasuresUIModel"/> class.
        /// </summary>
        public MeasuresUIModel()
        {
            DetailsViewCommand = new DelegateCommand(ExecuteDetailsViewCommand);
            IsDetailsViewOn = false;
        }

        /// <summary>
        /// Gets or sets the details view command.
        /// </summary>
        /// <value>
        /// The details view command.
        /// </value>
        public DelegateCommand DetailsViewCommand { get; set; }
        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public ObservableCollection<Exception> Errors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Measures UI is busy.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Measures UI is busy; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the details view is on.
        /// </summary>
        /// <value>
        /// <c>true</c> if the details view is on; otherwise, <c>false</c>.
        /// </value>
        public bool IsDetailsViewOn { get; set; }
        /// <summary>
        /// Gets or sets the width of the detail view.
        /// </summary>
        /// <value>
        /// The width of the detail view.
        /// </value>
        public string DetailViewWidth { get; set; }
        /// <summary>
        /// Gets or sets the width of the spliter.
        /// </summary>
        /// <value>
        /// The width of the spliter.
        /// </value>
        public string SpliterWidth { get; set; }
        /// <summary>
        /// Gets or sets the width of the data view.
        /// </summary>
        /// <value>
        /// The width of the data view.
        /// </value>
        public string DataViewWidth { get; set; }
        /// <summary>
        /// Gets or sets the progress percentage.
        /// </summary>
        /// <value>
        /// The progress percentage.
        /// </value>
        public int ProgressPercentage { get; set; }

        /// <summary>
        /// Executes the details view command.
        /// </summary>
        public void ExecuteDetailsViewCommand()
        {
            if (!IsDetailsViewOn)
            {
                DetailViewWidth = DETAIL_VIEW_WIDTH_ON;
                SpliterWidth = SPLITER_WIDTH_ON;
                DataViewWidth = DATA_VIEW_WIDTH_ON;
                IsDetailsViewOn = true;
            }
            else
            {
                DetailViewWidth = ZERO_WIDTH;
                SpliterWidth = ZERO_WIDTH;
                DataViewWidth = DATA_VIEW_WIDTH_ON;
                IsDetailsViewOn = false;
            }
        }
    }
}