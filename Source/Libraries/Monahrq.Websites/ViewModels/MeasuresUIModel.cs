using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Commands;
using PropertyChanged;

namespace Monahrq.Websites.ViewModels
{
    [ImplementPropertyChanged]
    public class MeasuresUIModel
    {
        public const string SPLITER_WIDTH_ON="7";
        public const string DETAIL_VIEW_WIDTH_ON = "0.3*";
        public const string DATA_VIEW_WIDTH_ON = "0.7*";
        public const string ZERO_WIDTH = "0";


        public MeasuresUIModel()
        {
            DetailsViewCommand = new DelegateCommand(ExecuteDetailsViewCommand);
            IsDetailsViewOn = false;
        }

        public DelegateCommand DetailsViewCommand { get; set; }
        public ObservableCollection<Exception> Errors { get; set; }

        public bool IsBusy { get; set; }
        public bool IsDetailsViewOn { get; set; }
        public string DetailViewWidth { get; set; }
        public string SpliterWidth { get; set; }
        public string DataViewWidth { get; set; }
        public int ProgressPercentage { get; set; }

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