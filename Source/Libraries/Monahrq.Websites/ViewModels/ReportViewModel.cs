using Microsoft.Practices.ServiceLocation;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Domain.Reports.Attributes;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Websites.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Websites.Services;
using PropertyChanged;
using EnumAttributeExtension = Monahrq.Reports.Model.EnumAttributeExtension;


namespace Monahrq.Websites.ViewModels
{
    [ImplementPropertyChanged]
    public class ReportViewModel : BaseViewModel, IWebsiteReportValidableSelectable
    {
        #region Fields and Constants

        private BitmapImage _professionalPreviewImage;
        private BitmapImage _consumerPreviewImage;
        private Report _report;
        private AttributeCollectionSet _attributeViewModels;
        private string _footnote;
        private string _name;
        private string _type;
        private ObservableCollection<string> _websites;
        private ObservableCollection<string> _datasets;
        private string _category;
        private AudiencesViewModel _audience;
        private bool _isDefault;
        private bool _isSelected;

        #endregion

        #region Constructor

        //public ReportViewModel()
        //{}

        public ReportViewModel(Report report) // : this()
        {
            //CancelCommand = new DelegateCommand(OnCancel, CanCancel);
            WebsiteDataService = ServiceLocator.Current.GetInstance<IWebsiteDataService>();

            _initReportModel(report);
        }

        #endregion

        #region Helper Methods
        private void _initReportModel(Report report)
        {
            Report = report;
            Id = report.Id;
            Name = report.Name;
            Type = report.SourceTemplate.Name;
            Category = report.Category.GetDescription();
            AudienceForDisplay = report.Audiences != null ? string.Join(", ", report.Audiences.Select(EnumAttributeExtension.GetDescription)) : string.Empty;
            //Audience = new AudiencesViewModel(report);

            IsDefault = report.IsDefaultReport;
            //Websites = _getWebsites();
            Datasets = report.Datasets.ToObservableCollection();
            Footnote = Report.Footnote;
            IsQuarterlyTrendingEnabled = true;

            //DeleteCommand = new DelegateCommand(OnDeleteReport, CanDelete);
            //DeleteCommand.RaiseCanExecuteChanged();

            //AttributeCollectionSet = AttributeViewModelFactory.Get(Report);
            var images = report.GetPreviewImages();
            ProfessionalPreviewImage = images["Professional"];
            ConsumerPreviewImage = images["Consumer"];

            Committed = true;
            Report.PropertyChanged += _reportPropertyChanged;

            // Disable the Save button if user clicks all the checkboxes off
            //Audience.AttributeSet.PropertyChanged += _validateAudience;
        }

        private void _reportPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged();
        }

        private ObservableCollection<string> _getWebsites()
        {
            return WebsiteDataService.GetWebsiteNamesForReport(Id).ToObservableCollection();
        }

        private void OnValueChnaged()
        {
            if (IsValueChanged != null) IsValueChanged(this, new EventArgs());
        }

        #endregion

        #region Properties

        public IList<Audience> WebsiteAudiences { get; set; }

        public bool IsQuarterlyTrendingEnabled { get; set; }

        public IWebsiteDataService WebsiteDataService { get; set; }

        public bool IsTrendingEnabled { get; set; }

        public int Id { get; set; }

        public string WebsitesForDisplay { get; set; }

        public BitmapImage ProfessionalPreviewImage
        {
            get { return _professionalPreviewImage; }
            set
            {
                if (Equals(_professionalPreviewImage, value)) return;
                _professionalPreviewImage = value;
                RaisePropertyChanged(() => ProfessionalPreviewImage);
            }
        }

        public BitmapImage ConsumerPreviewImage
        {
            get { return _consumerPreviewImage; }
            set
            {
                if (Equals(_consumerPreviewImage, value)) return;
                _consumerPreviewImage = value;
                RaisePropertyChanged(() => ConsumerPreviewImage);
            }
        }

        public Report Report
        {
            get { return _report; }
            set
            {
                _report = value;
                RaisePropertyChanged();
            }
        }

        public AttributeCollectionSet AttributeCollectionSet
        {
            get { return _attributeViewModels; }
            set
            {
                _attributeViewModels = value;
                RaisePropertyChanged(() => AttributeCollectionSet);
            }
        }

        public string Footnote
        {
            get { return _footnote; }
            set
            {
                _footnote = value;
                RaisePropertyChanged(() => Footnote);
                Report.Footnote = Footnote;
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
                _validateTitle(ExtractPropertyName(() => Name), Name);
                if (GetErrors(ExtractPropertyName(() => Name)) == null)
                {
                    Report.Name = Name;
                }
            }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                RaisePropertyChanged(() => Type);
            }
        }

        public ObservableCollection<string> Websites
        {
            get { return _websites; }
            set
            {
                _websites = value;
                RaisePropertyChanged(() => Websites);
            }
        }

        public ObservableCollection<string> Datasets
        {
            get { return _datasets; }
            set
            {
                _datasets = value;
                RaisePropertyChanged(() => Datasets);
            }
        }

        public Visibility ShowWebsiteToolTip
        {
            get { return Websites != null && Websites.Any() ? Visibility.Visible : Visibility.Hidden; }
        }

        public string Category
        {
            get { return _category; }
            set
            {
                _category = value;
                RaisePropertyChanged(() => Category);
            }
        }

        public AudiencesViewModel Audience
        {
            get { return _audience; }
            set
            {
                _audience = value;
                RaisePropertyChanged(() => Audience);
                _validateAudience(Audience);
                if (GetErrors(ExtractPropertyName(() => Audience)) == null)
                {
                    //TODO
                }
            }
        }

        public bool IsDefault
        {
            get { return _isDefault; }
            set
            {
                _isDefault = value;
                RaisePropertyChanged(() => IsDefault);
                RaisePropertyChanged(() => IsUserCreatedInstance);
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnValueChnaged();
            }
        }

        //public bool ValidateBeforeSelection(object objNeedForValidation, bool isSelectAllCommand = false)
        //{
        //    var items = objNeedForValidation as IList<ReportViewModel>;
        //    if (items == null) return true;
        //    var distinctItems = items.Where(x => x.IsSelected).DistinctBy(x => x.Type).ToList();
        //    if (!distinctItems.Any(x => x.Type.EqualsIgnoreCase(this.Type))) return true;
        //    if (isSelectAllCommand)
        //    {
        //        MessageBox.Show("Only one report of a given type can be selected.", "Validation Error!!!", MessageBoxButton.OK);
        //    }
        //    return false;
        //}

        public bool IsUserCreatedInstance
        {
            get { return !IsDefault; }
        }

        public ObservableCollection<TrendingYear> Years { get; set; }

        public string SelectedDefaultYear { get; set; }

        public bool IsTrendingReport
        {
            get
            {
                return Report != null && !string.IsNullOrEmpty(Report.Name) && Report.Name.ContainsIgnoreCase("Trending");
            }
        }

        public string AudienceForDisplay { get; set; }

        public event EventHandler IsValueChanged;

        #endregion

        #region Commands

        ////Save Report
        //protected override void OnCommitted()
        //{
        //    //var service = ServiceLocator.Current.GetInstance<IWebsiteReportDataService>();
        //    //if (service.SaveReport(Report))
        //    //{
        //    //    MessageBox.Show("Your changes have been saved.");
        //    //    Committed = true;
        //    //}
        //}

        //private bool CanCancel()
        //{
        //    return HasErrors || !Committed;
        //}

        //private void OnCancel()
        //{
        //    _reset();
        //}

        //private void _reset()
        //{
        //    //TODO
        //}

        //private bool CanDelete()
        //{
        //    // return false if this is a default report or if it is used in ANY website
        //    return !IsDefault && Websites.Count == 0;
        //}

        //private void OnDeleteReport()
        //{
        //    if (MessageBox.Show("Are you sure you want to delete this report?", "Delete Report", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    {
        //        var service = ServiceLocator.Current.GetInstance<IWebsiteReportDataService>();
        //        var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
        //        service.Delete(Report, (o, e) =>
        //            {
        //                if (e == null)
        //                {
        //                    eventAggregator.GetEvent<ReportDeletedEvent>().Publish(this);
        //                }
        //                else
        //                {
        //                    Events.GetEvent<ErrorNotificationEvent>().Publish(e);
        //                }
        //            });
        //    }
        //}

        #endregion

        #region Validation

        // this overload is for the checkbox propertychanged
        private void _validateAudience(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ClearErrors("AudienceSetCollection");

            var audienceSetCollection = sender as IEnumerationSetCollection<Audience>;
            if (audienceSetCollection != null && audienceSetCollection.Value == Infrastructure.Entities.Domain.Reports.Audience.None)
            {
                SetError("AudienceSetCollection", "At least one audience type must be selected");
            }
        }

        private void _validateAudience(AudiencesViewModel audienceSetCollection)
        {
            ClearErrors("AudienceSetCollection");
            if (audienceSetCollection.Value == Infrastructure.Entities.Domain.Reports.Audience.None)
            {
                SetError("AudienceSetCollection", "At least one audience type must be selected");
            }
        }

        private void _validateTitle(string p, string value)
        {
            ClearErrors(p);
            if (string.IsNullOrWhiteSpace(value))
            {
                SetError(p, "Report name cannot be empty");

            }
            else if (!string.IsNullOrWhiteSpace(value) && value.Count() > 250)
            {
                SetError(p, "Report name length can not be more than 250 characters");
            }
        }

        protected override void ValidateAll()
        {
            _validateAudience(Audience);
        }

        //public bool ValidateBeforeSelection(object objNeedForValidation, bool isAllSelectedCommand = false)
        //{
        //    var selectedReports = objNeedForValidation as IList<ReportViewModel>;
        //    if (selectedReports == null || !selectedReports.Any()) return true;
        //    var reportTypes = selectedReports.Select(rpt => rpt.Type).ToList();
        //    if (IsSelected || !reportTypes.Contains(Type)) return true;
        //    if (!isAllSelectedCommand)
        //    {
        //        MessageBox.Show("Only one report of a given type can be selected.",
        //        "Validation Error!!!", MessageBoxButton.OK);
        //    }

        //    IsSelected = false;
        //    return false;
        //}

        public bool ValidateBeforSelection(object objNeedForValidation)
        {
            var args = objNeedForValidation as WebsiteReportValidableSelectableStruct; //
            if (args == null) return true;

            #region Per Jason - Delete once validation is working or we need to rollback logic
            //var selectedReports = args.Item1 as List<ReportViewModel>;
            //if (selectedReports == null || !selectedReports.Any()) return true;
            //var reportTypes = selectedReports.Select(rpt => rpt.Type).ToList();

            //if (IsSelected || !reportTypes.Contains(Type)) return true;

            //MessageBox.Show("Only one report of a given type can be selected.",
            //"Validation Error!!!", MessageBoxButton.OK);

            //IsSelected = false;
            //return false;
            #endregion

            var selectedReports = args.Items as List<object>;
            if (selectedReports == null || !selectedReports.Any()) return true;

            var validationReult = OnlyOneReportTypeSelectionValidation(selectedReports.OfType<ReportViewModel>().ToList());

            if (validationReult)
            {
                var audiences = args.WebsiteAudiences as List<Audience>;

                if (audiences != null || audiences.Any())
                {
                    validationReult = SpecificAudienceForDisplayValidation(this, audiences.ToList());
                }
            }

            return validationReult;
        }

        private bool OnlyOneReportTypeSelectionValidation(IList<ReportViewModel> selectedReports)
        {
            if (selectedReports == null || !selectedReports.Any()) return true;
            var reportTypes = selectedReports.Select(rpt => rpt.Type).ToList();

            if (IsSelected || !reportTypes.Contains(Type)) return true;

            MessageBox.Show("Only one report of a given type can be selected.",
            "Validation Error!!!", MessageBoxButton.OK);

            IsSelected = false;
            return false;
        }

        public bool SpecificAudienceForDisplayValidation(ReportViewModel currentReport, IList<Audience> currentWebsiteAudiences) 
        {
            int audienceCount = currentWebsiteAudiences.Count(audience => currentReport.AudienceForDisplay.ContainsIgnoreCase(audience.ToString()));
            if (audienceCount == 0)
            {
                MessageBox.Show("The selected report(s) are not recommended for the " + currentReport.AudienceForDisplay + ", as the data contained in the report may not be easily understood by the selected audience type. This report will not be included in the website.", "Validation Error!!!", MessageBoxButton.OK);
                IsSelected = false;
            }
            else
            {
                IsSelected = true;

            }
            return IsSelected;
        }

        #endregion
    }
}
