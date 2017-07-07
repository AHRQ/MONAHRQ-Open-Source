// TODO: Delete once totally convinced not needed - jason
//using System;
//using System.Collections.ObjectModel;
//using System.IO;
//using System.Linq;
//using System.Windows;
//using System.Windows.Media.Imaging;
//using Microsoft.Practices.Prism.Commands;
//using Microsoft.Practices.ServiceLocation;
//using Monahrq.Default.ViewModels;
//using Monahrq.Infrastructure.Entities.Domain.Reports;
//using Monahrq.Infrastructure.Entities.Domain.Reports.Attributes;
//using Monahrq.Infrastructure.Services;
//using Monahrq.Reports.Model;
//using Monahrq.Sdk.Events;

//namespace Monahrq.Reports.ViewModels
//{
//    public class ReportViewModel : BaseViewModel
//    {
//        public DelegateCommand DeleteCommand { get; set; }
//        public DelegateCommand CancelCommand { get; set; }

//        //[Import(RequiredCreationPolicy = CreationPolicy.Shared)]
//        //protected IReportService ReportService { get; set; }

//        public ReportViewModel(Report report)
//        {
//            CancelCommand = new DelegateCommand(OnCancel, CanCancel);

//            var committedProp = ExtractPropertyName(() => Committed);

//            PropertyChanged += (o, e) =>
//            {
//                if (e.PropertyName == committedProp)
//                {
//                    CancelCommand.RaiseCanExecuteChanged();
//                }
//            };

//            _initReportModel(report);
//        }


//        private void _initReportModel(Report report)
//        {
//            Report = report;
//            Name = Report.Name;
//            Type = Report.SourceTemplate.Name;
//            Category = Report.Category.GetDescription();
//            Audience = new AudiencesViewModel(Report);
//            IsDefault = Report.IsDefaultReport;
//            //Websites = _getWebsites(report.Id);
//            Datasets = report.Datasets.Select(d => d.Name).ToObservableCollection();
//            Footnote = Report.Footnote;
//            Description = Report.Description;
//            DeleteCommand = new DelegateCommand(OnDeleteReport, CanDelete);
//            DeleteCommand.RaiseCanExecuteChanged();

//            AttributeCollectionSet = AttributeViewModelFactory.Get(Report);

//            PreviewImage = _getBitmapImage(Report);
//            Committed = true;
//            Report.PropertyChanged += _reportPropertyChanged;

//            // Disable the Save button if user clicks all the checkboxes off
//            Audience.AttributeSet.PropertyChanged += _validateAudience;
//        }

//        public void _reportPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
//        {
//            this.RaisePropertyChanged();
//        }

//        #region Helper Methods

//        //private ObservableCollection<string> _getWebsites(Guid reportId)
//        //{
//        //    var service = ServiceLocator.Current.GetInstance<IReportService>();
//        //    return service.GetWebsitesForReport(reportId).ToObservableCollection();
//        //}

//        private BitmapImage _getBitmapImage(Report report, BitmapCacheOption bitmapCacheOption = BitmapCacheOption.Default)
//        {
//            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Domain\Reports\Data");
//            var filepath = Path.Combine(dir, report.SourceTemplate.PreviewImage ?? @"missing_template.png");

//            if (string.IsNullOrEmpty(report.SourceTemplate.PreviewImage) || !new FileInfo(filepath).Exists)
//            {
//                filepath = Path.Combine(dir, @"missing_template.png");
//            }

//            var uri = new Uri(filepath, UriKind.RelativeOrAbsolute);
//            var image = new BitmapImage();
//            image.BeginInit();
//            image.CacheOption = bitmapCacheOption;
//            image.UriSource = uri;
//            image.EndInit();
//            image.Freeze();
//            return image;
//        }

//        #endregion

//        #region Properties
//        private ReportViewModel _oldreport;
//        private BitmapImage _previewImage;
//        public BitmapImage PreviewImage
//        {
//            get { return _previewImage; }
//            set
//            {
//                if (Equals(_previewImage, value)) return;
//                _previewImage = value;
//                RaisePropertyChanged(() => PreviewImage);
//            }
//        }


//        public bool HasAttributes { get { return Report != null && Report.ReportAttributes != ReportAttributeOption.None; } }

//        private Report _report;
//        public Report Report
//        {
//            get { return _report; }
//            set
//            {
//                _report = value;
//                _oldreport = this;
//                RaisePropertyChanged();
//            }
//        }

//        private AttributeCollectionSet _attributeViewModels;
//        public AttributeCollectionSet AttributeCollectionSet
//        {
//            get { return _attributeViewModels; }
//            set
//            {
//                _attributeViewModels = value;
//                RaisePropertyChanged(() => AttributeCollectionSet);
//            }
//        }

//        private string _footnote;
//        public string Footnote
//        {
//            get { return _footnote; }
//            set
//            {
//                if (value.Length > FootnotesMaxLength)
//                {
//                    MessageBox.Show(string.Format("Please enter 'Foot Notes' using fewer than {0} characters.", FootnotesMaxLength),
//                    "Text length restriction",
//                    MessageBoxButton.OK,
//                    MessageBoxImage.Information);
//                    return;
//                }
//                _footnote = value;
//                RaisePropertyChanged(() => Footnote);
//                Report.Footnote = Footnote;
//                Committed = false;
//            }
//        }


//        private string _description;
//        public string Description
//        {
//            get { return _description; }
//            set
//            {
//                _description = value;
//                RaisePropertyChanged(() => Description);
//                Report.Description = _description;
//                Committed = false;
//            }
//        }

//        private string _name;
//        public string Name
//        {
//            get { return _name; }
//            set
//            {
//                _name = value;
//                RaisePropertyChanged(() => Name);
//                _validateTitle(ExtractPropertyName(() => Name), Name);
//                if (GetErrors(ExtractPropertyName(() => Name)) == null)
//                {
//                    Report.Name = Name;
//                    Committed = false;
//                }
//            }
//        }

//        public string Type { get; set; }

//        public ObservableCollection<string> Websites { get; set; }

//        public ObservableCollection<string> Datasets { get; set; }

//        public string Category { get; set; }

//        private AudiencesViewModel _audience;
//        public AudiencesViewModel Audience
//        {
//            get { return _audience; }
//            set
//            {
//                _audience = value;
//                RaisePropertyChanged(() => Audience);
//                _validateAudience(Audience);
//                if (GetErrors(ExtractPropertyName(() => Audience)) == null)
//                {
//                    //TODO
//                }
//            }
//        }

//        private bool _isDefault;
//        public bool IsDefault
//        {
//            get { return _isDefault; }
//            set
//            {
//                _isDefault = value;
//                RaisePropertyChanged(() => IsDefault);
//                RaisePropertyChanged(() => IsUserCreatedInstance);
//            }
//        }

//        public bool IsUserCreatedInstance
//        {
//            get { return !IsDefault; }
//        }

//        public bool IsCustomAndShowInterpretation
//        {
//            get { return !IsDefault && Report.ShowInterpretationText; }
//        }

//        #endregion

//        #region Commands

//        //Save Report

//        protected override void OnCommitted()
//        {
//            //var service = ServiceLocator.Current.GetInstance<IReportService>();
//            //Report.FilterItems = Report.Filter.ToReportFilterList();
//            //if (service.SaveReport(Report))
//            //{
//            //    //MessageBox.Show("Your changes have been saved.");
//            //    Committed = true;

//            //    Events.GetEvent<GenericNotificationEvent>().Publish(string.Format("Report '{0}' was successfully saved.", this.Name));
//            //    //Events.GetEvent<ReportSavedEvent>().Publish(this);
//            //}
//        }

//        private bool CanCancel()
//        {
//            return HasErrors || !Committed;
//        }

//        private void OnCancel()
//        {
//            _reset();
//        }
//        public void Reset()
//        {
//            _reset();

//        }

//        //todo : semi crappy code alert do not copy. (inga)
//        private void _reset()
//        {
//            if (!Committed)
//            {
//                Report = _oldreport.Report;
//                Committed = true;
//            }
//        }

//        private bool CanDelete()
//        {
//            // return false if this is a default report or if it is used in ANY website
//            return !IsDefault && Websites.Count == 0;
//        }

//        private void OnDeleteReport()
//        {
//            //if (MessageBox.Show("Are you sure you want to delete this report?", "Delete Report", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
//            //{
//            //    var service = ServiceLocator.Current.GetInstance<IReportService>();
//            //    var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
//            //    service.Delete(Report, (o, e) =>
//            //        {
//            //            if (e == null)
//            //            {
//            //                eventAggregator.GetEvent<ReportDeletedEvent>().Publish(this);
//            //            }
//            //            else
//            //            {
//            //                Events.GetEvent<ErrorNotificationEvent>().Publish(e);
//            //            }
//            //        });
//            //}
//        }

//        #endregion

//        #region Validation

//        // this overload is for the checkbox propertychanged
//        private void _validateAudience(object sender, System.ComponentModel.PropertyChangedEventArgs e)
//        {
//            ClearErrors("AudienceSetCollection");

//            var audienceSetCollection = sender as IEnumerationSetCollection<Audience>;
//            if (audienceSetCollection.Value == Infrastructure.Entities.Domain.Reports.Audience.None)
//            {
//                SetError("AudienceSetCollection", "At least one audience type must be selected");

//            }

//            RaisePropertyChanged(() => Report);
//        }

//        private void _validateAudience(AudiencesViewModel audienceSetCollection)
//        {
//            ClearErrors("AudienceSetCollection");
//            if (audienceSetCollection.Value == Infrastructure.Entities.Domain.Reports.Audience.None)
//            {
//                SetError("AudienceSetCollection", "At least one audience type must be selected");
//            }
//        }

//        private void _validateTitle(string p, string value)
//        {
//            ClearErrors(p);
//            if (string.IsNullOrWhiteSpace(value))
//            {
//                SetError(p, "Report name cannot be empty");

//            }
//            else if (!string.IsNullOrWhiteSpace(value) && value.Count() > 250)
//            {
//                SetError(p, "Report name length can not be more than 250 characters");
//            }
//        }

//        protected override void ValidateAll()
//        {
//            _validateAudience(Audience);
//        }

//        #endregion

//        #region MAX_LENGTH
//        public int FootnotesMaxLength { get { return 1000; } }
//        #endregion
//    }
//}
