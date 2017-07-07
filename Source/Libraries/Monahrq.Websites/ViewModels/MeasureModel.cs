using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Websites.Services;
using PropertyChanged;
using Monahrq.Default.ViewModels;
using System.Text.RegularExpressions;
using Microsoft.Practices.Prism.Commands;
using System.Windows;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Websites.ViewModels
{
    [ImplementPropertyChanged]
    public class MeasureModel : BaseViewModel, ISelectable
    {
        #region Commands

        public DelegateCommand OverrideMeasureCommand { get; set; }                 // TODO

        #endregion

        #region Constructor

        public MeasureModel()
        {
            WebsiteDataService = ServiceLocator.Current.GetInstance<IWebsiteDataService>();

            IsVisible = true;
            Websites = new ObservableCollection<string>();
        }

        public MeasureModel(WebsiteMeasure websiteMeasure)
            : this(websiteMeasure.OriginalMeasure, websiteMeasure.OverrideMeasure)
        {

            WebsiteMeasure = websiteMeasure;
        }
        public MeasureModel(Measure measure, Measure measureOverride = null)
            : this()
        {
            InitMeasure(measure, measureOverride);

            //var committedProp = ExtractPropertyName(() => Committed);

            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "IsSelectedForWebsiteAssignment")
                {
                    //EventAggregator.GetEvent<SelectedTopicAssigmentEvent>().Publish(false);
                }
                //if (e.PropertyName == "IsTopicsUpdated")
                //{
                //    EventAggregator.GetEvent<SelectedTopicAssigmentEvent>().Publish(true);
                //}
            };

            Reset();
        }

        #endregion

        #region Fields and Constants

        private bool _isMinScaleByRadioButtonChecked;
        private bool _isMediumScaleByRadioButtonChecked;
        private bool _isMaxScaleByRadioButtonChecked;
        private string _Numerator;
        private string _isCustomized;
        private string _Denominator;
        private string _LowerBound;
        private string _UpperBound;
        private string _NationalBenchmark;
        private string _scaleBy;
        private string _ProvidedBenchmark;
        private bool _performMarginSuppressionOverride;
        private bool _performMarginSuppression;
        public int MoreInformationMaxLength { get { return 500; } }
        public int MeasureNameMaxLength { get { return 200; } }            // max length for the user to type in the Alternate measure names textboxes
        public int FootnotesMaxLength { get { return 500; } }
        public int UrlMaxLength { get { return 300; } }
        public int UrlTitleMaxLength { get { return 100; } }
        private string ValidateLessMessage = "Please ensure Lower Bound is less than Upper Bound.";

        private string _idOverride;
        private string _providedBenchmarkOverride;
        private string _codeOverride;
        private string _scaleByOverride;
        private string _nationalBenchmarkOverride;
        private string _urlOverride;
        private string _denominatorOverride;
        private string _numeratorOverride;
        private string _lowerBoundOverride;
        private string _upperBoundOverride;
        private bool _isSelectedForWebsiteAssignment;
        private string _url;

        #endregion

        #region Properties

        public Measure Measure { get; set; }

        public Measure MeasureOverwrite { get; set; }

        public WebsiteMeasure WebsiteMeasure { get; set; }

        public bool IsTopicsUpdated { get; set; }

        public IWebsiteDataService WebsiteDataService { get; set; }

        /// <summary>
        /// Gets or sets the websites.
        /// </summary>
        /// 
        /// <value>
        /// The websites.
        /// </value>
        public ObservableCollection<string> Websites { get; set; }

        public ObservableCollection<string> Topics { get; set; }

        public ObservableCollection<string> TopicsOverride { get; set; }

        public string Id { get { return Measure.Id.ToString(); } }

        public bool PerformMarginSuppression
        {
            get { return _performMarginSuppression; }
            set
            {
                _performMarginSuppression = value;
                Committed = false;
                RaisePropertyChanged(() => PerformMarginSuppression);

                Measure.PerformMarginSuppression = _performMarginSuppression;
            }
        }

        public bool PerformMarginSuppressionOverride
        {
            get { return _performMarginSuppressionOverride; }
            set
            {
                _performMarginSuppressionOverride = value;
                Committed = false;
                RaisePropertyChanged(() => PerformMarginSuppressionOverride);

                Measure.PerformMarginSuppression = _performMarginSuppressionOverride;
            }
        }

        public string DataSetName
        {
            get { return Measure.Owner.Name; }
            set
            {
                if (value == Measure.Owner.Name) return;
                Measure.Owner.Name = value;
                RaisePropertyChanged(() => DataSetName);
            }
        }

        public string ClinicalTitle
        {
            get { return Measure.MeasureTitle.Clinical; }
            set
            {
                if (value == Measure.MeasureTitle.Clinical) return;
                Measure.MeasureTitle.Clinical = value;
                RaisePropertyChanged(() => ClinicalTitle);
                _ValidateTitle(ExtractPropertyName(() => ClinicalTitle), value);
            }
        }

        private void _ValidateTitle(string p, string value)
        {
            ClearErrors(p);
            if (string.IsNullOrWhiteSpace(ClinicalTitle)
                && string.IsNullOrWhiteSpace(PlainTitle)
                && string.IsNullOrWhiteSpace(PolicyTitle)
                )
            {
                SetError(p, "Please make sure at least one title is provided.");
            }
        }

        public string PlainTitle
        {
            get { return Measure.MeasureTitle.Plain; }
            set
            {
                if (value == Measure.MeasureTitle.Plain) return;
                Measure.MeasureTitle.Plain = value;
                RaisePropertyChanged(() => PlainTitle);
                _ValidateTitle(ExtractPropertyName(() => PlainTitle), value);
            }
        }

        public string PolicyTitle
        {
            get { return Measure.MeasureTitle.Policy; }
            set
            {
                if (value == Measure.MeasureTitle.Policy) return;
                Measure.MeasureTitle.Policy = value;
                RaisePropertyChanged(() => PolicyTitle);
                _ValidateTitle(ExtractPropertyName(() => PolicyTitle), value);
            }
        }

        public StatePeerBenchmarkCalculationMethod CalculationMethod
        {
            get { return Measure.StatePeerBenchmark.CalculationMethod; }
            set { Measure.StatePeerBenchmark.CalculationMethod = value; }
        }

        // BUG: this field isn't running validation on every char typed (PropertyChanged), only on FocusChanged event
        public string ProvidedBenchmark
        {
            get { return _ProvidedBenchmark; }
            set
            {
                if (value == _ProvidedBenchmark) return;
                _ProvidedBenchmark = value;
                RaisePropertyChanged(() => ProvidedBenchmark);

                var propertyName = ExtractPropertyName(() => ProvidedBenchmark);
                ValidateNumeric(propertyName, value);

                if (GetErrors(propertyName) == null)
                    Measure.StatePeerBenchmark.ProvidedBenchmark = decimal.Parse(value);
            }
        }

        public string Code
        {
            get { return Measure.MeasureCode; }
        }

        public string Description
        {
            get { return Measure.Description; }
            set { Measure.Description = value; }
        }

        public bool HigherScoresAreBetter
        {
            get { return Measure.HigherScoresAreBetter; }
            set { Measure.HigherScoresAreBetter = value; }
        }

        public bool LowerScoresAreBetter
        {
            get { return !Measure.HigherScoresAreBetter; }
        }

        public string MoreInformation
        {
            get { return Measure.MoreInformation; }
            set { Measure.MoreInformation = value; }
        }


        public string UrlTitle
        {
            get { return Measure.UrlTitle; }
            set
            {
                if (value == Measure.UrlTitle) return;
                Measure.UrlTitle = value;
                RaisePropertyChanged(() => UrlTitle);
                _ValidateUrlTitle(ExtractPropertyName(() => UrlTitle), value);
                _ValidateUrl(ExtractPropertyName(() => Url), Url);
            }
        }

        private void _ValidateUrlTitle(string p, string value)
        {
            ClearErrors(p);
            if (string.IsNullOrWhiteSpace(Url) && string.IsNullOrWhiteSpace(UrlTitle)) return;
            if (string.IsNullOrWhiteSpace(value))
            {
                SetError(p, "Please provide a URL Title.");
            }
        }

        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                if (value == _url) return;
                _url = value;
                RaisePropertyChanged(() => Url);
                _ValidateUrl(ExtractPropertyName(() => Url), value);
                _ValidateUrlTitle(ExtractPropertyName(() => UrlTitle), UrlTitle);
            }
        }

        const string ValidUrlMessage = "Please provide a valid URL, beginning with http:// or https://.";

        private void _ValidateUrl(string p, string value)
        {
            ClearErrors(p);
            if (string.IsNullOrWhiteSpace(UrlTitle) && string.IsNullOrWhiteSpace(Url)) return;

            if (string.IsNullOrWhiteSpace(value))
            {
                SetError(p, "Please provide a URL.");
                return;
            }
            // user MUST type http: or https:
            var regexUrl = new Regex(@"^(http|https)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$");

            if (!regexUrl.IsMatch(value))
            {
                SetError(p, ValidUrlMessage);
            }
            else
            {
                // our regex might not catch every problem that .NET considers invalid, so let .NET finally decide if the user text is valid.
                // but using only try/catch would allow non-http: and unusual uri's
                try
                {
                    Measure.Url = value; //new Uri(value);
                }
                catch (Exception)
                {
                    SetError(p, ValidUrlMessage);
                }
            }
        }

        public string Footnotes
        {
            get { return Measure.Footnotes; }
            set { Measure.Footnotes = value; }
        }

        public string ScaleBy
        {
            get { return _scaleBy; }
            set
            {
                if (value == _scaleBy) return;
                _scaleBy = value;
                RaisePropertyChanged(() => ScaleBy);

                var propertyName = ExtractPropertyName(() => ScaleBy);
                ValidateNumeric(propertyName, value);
                if (GetErrors(propertyName) == null)
                    Measure.ScaleBy = decimal.Parse(value);
            }
        }

        public bool IsMinScaleByRadioButtonChecked
        {
            get
            {
                return _isMinScaleByRadioButtonChecked;
            }
            set
            {
                _isMinScaleByRadioButtonChecked = value;
                if (_isMinScaleByRadioButtonChecked)
                {
                    ScaleByOverride = "1000";
                }
            }
        }

        public bool IsMediumScaleByRadioButtonChecked
        {
            get
            {
                return _isMediumScaleByRadioButtonChecked;
            }
            set
            {
                _isMediumScaleByRadioButtonChecked = value;
                if (_isMediumScaleByRadioButtonChecked)
                {
                    ScaleByOverride = "10000";
                }
            }
        }

        public bool IsMaxScaleByRadioButtonChecked
        {
            get
            {
                return _isMaxScaleByRadioButtonChecked;
            }
            set
            {
                _isMaxScaleByRadioButtonChecked = value;
                if (_isMaxScaleByRadioButtonChecked)
                {
                    ScaleByOverride = "100000";
                }
            }
        }

        public Visibility IsScaleByRadioButtonFuctionalityVisible
        {
            get
            {
                return IsScaleMeasure ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility IsScaleByTextBoxFuctionalityVisible
        {
            get
            {
                return IsScaleMeasure ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private bool IsScaleMeasure
        {
            get
            {
                return Code == "IP-11" || Code == "IP-15";
            }
        }

        public string NationalBenchmark
        {
            get { return _NationalBenchmark; }
            set
            {
                if (value == _NationalBenchmark) return;
                _NationalBenchmark = value;
                RaisePropertyChanged(() => NationalBenchmark);

                var propertyName = ExtractPropertyName(() => NationalBenchmark);
                ValidateNumeric(propertyName, value);

                if (GetErrors(propertyName) == null)
                    Measure.NationalBenchmark = decimal.Parse(value);
            }
        }

        public string UpperBound
        {
            get { return _UpperBound; }
            set
            {
                if (value == _UpperBound) return;
                _UpperBound = value;
                RaisePropertyChanged(() => UpperBound);

                var lowerPropertyName = ExtractPropertyName(() => LowerBound);
                var upperPropertyName = ExtractPropertyName(() => UpperBound);

                ValidateNumeric(upperPropertyName, value);

                if (GetErrors(upperPropertyName) == null)
                    ValidateLess(lowerPropertyName, LowerBound, upperPropertyName, value, upperPropertyName);

                if (GetErrors(upperPropertyName) == null)
                {
                    ClearRangeError(lowerPropertyName);
                    Measure.UpperBound = decimal.Parse(value);
                }
            }
        }

        public string LowerBound
        {
            get { return _LowerBound; }
            set
            {
                if (value == _LowerBound) return;
                _LowerBound = value;
                RaisePropertyChanged(() => LowerBound);

                var lowerPropertyName = ExtractPropertyName(() => LowerBound);
                var upperPropertyName = ExtractPropertyName(() => UpperBound);

                ValidateNumeric(lowerPropertyName, value);

                if (GetErrors(lowerPropertyName) == null)
                    ValidateLess(lowerPropertyName, value, upperPropertyName, UpperBound, lowerPropertyName);

                if (GetErrors(lowerPropertyName) == null)
                {
                    ClearRangeError(upperPropertyName);
                    Measure.LowerBound = decimal.Parse(value);
                }
            }
        }

        public string Numerator
        {
            get { return _Numerator; }
            set
            {
                if (value == _Numerator) return;
                _Numerator = value;
                RaisePropertyChanged(() => Numerator);

                var propertyName = ExtractPropertyName(() => Numerator);
                ValidateNumeric(propertyName, value);
                if (GetErrors(propertyName) == null)
                    Measure.SuppressionNumerator = decimal.Parse(value);
            }
        }

        public string Denominator
        {
            get { return _Denominator; }
            set
            {
                if (value == _Denominator) return;
                _Denominator = value;
                RaisePropertyChanged(() => Denominator);

                var propertyName = ExtractPropertyName(() => Denominator);
                ValidateNumeric(propertyName, value);
                if (GetErrors(propertyName) == null)
                    Measure.SuppressionDenominator = decimal.Parse(value);
            }
        }

        public ObservableCollection<string> CalculatedOptions { get; set; }

        public string AssignedCalculatedOption { get; set; }

        public string BetterHighOrLow
        {
            get
            {
                return HigherScoresAreBetter ? "Higher is better" : "Lower is better";
            }
        }

        public string IsCustomized
        {
            get
            {
                _isCustomized = (MeasureOverwrite != null) ? "Yes" : "No";
                return _isCustomized;
            }
            set
            {
                _isCustomized = value;
                RaisePropertyChanged(() => IsCustomized);
            }
        }

        public bool IsSelectedForWebsiteAssignment { get; set; }

        public bool IsVisible { get; set; }

        public bool IsChanged { get; set; }

        #endregion

        #region Methods

        private void HandleScaleByRadioButtons()
        {
            if (IsScaleMeasure)
            {
                IsMinScaleByRadioButtonChecked = false;
                IsMediumScaleByRadioButtonChecked = false;
                IsMaxScaleByRadioButtonChecked = false;
                switch (ScaleByOverride)
                {
                    case "1000":
                        IsMinScaleByRadioButtonChecked = true;
                        break;
                    case "10000":
                        IsMediumScaleByRadioButtonChecked = true;
                        break;
                    case "100000":
                        IsMaxScaleByRadioButtonChecked = true;
                        break;
                    default:
                        break;
                }
            }
        }

        public void InitMeasure(Measure measure, Measure measureOverride = null)
        {
            Measure = measure;
            //if (!Measure.Metadata.Any())
            //{
            //    Measure.Metadata.Add(new MetadataItem(Measure, "Reference metadata") { Value = "N/A" });
            //}

            IsVisible = true;

            Websites = GetWebsites();
            Topics = GetTopics();

            Url = string.IsNullOrWhiteSpace(measure.Url) ? string.Empty : measure.Url;
            UrlTitle = measure.UrlTitle ?? string.Empty;

            ScaleBy = measure.ScaleBy.HasValue ? measure.ScaleBy.ToString().SubStrBefore(".") : string.Empty;
            Denominator = measure.SuppressionDenominator.ToString();
            Numerator = measure.SuppressionNumerator.ToString();
            PerformMarginSuppression = measure.PerformMarginSuppression;
            UpperBound = measure.UpperBound.HasValue ? measure.UpperBound.ToString() : "0";
            LowerBound = measure.LowerBound.HasValue ? measure.LowerBound.ToString() : "0";
            NationalBenchmark = measure.NationalBenchmark.HasValue && measure.NationalBenchmark.Value != 0
                                            ? measure.NationalBenchmark.ToString() : "0";
            ProvidedBenchmark = measure.StatePeerBenchmark.ProvidedBenchmark.HasValue && measure.StatePeerBenchmark.ProvidedBenchmark.Value != 0
                                            ? measure.StatePeerBenchmark.ProvidedBenchmark.ToString() : "0";

            //BUG: should other titles also be init here? this code isn't running for the correct measure when the measure is re-edited
            PolicyTitle = measure.MeasureTitle.Policy;

            // Initiate override
            if (measureOverride != null && measureOverride.Id > 0)
                InitMeasureOverride(measureOverride);

            Reset();
        }

        public void InitMeasureOverride(Measure measure)
        {
            MeasureOverwrite = measure.IsOverride ? measure : measure.Clone(true);

            //if (!MeasureOverwrite.Metadata.Any())
            //{
            //    MeasureOverwrite.Metadata.Add(new MetadataItem(MeasureOverwrite, "Reference metadata") { Value = "N/A" });
            //}

            IsVisible = true;

            IdOverride = MeasureOverwrite.Id.ToString();

            TopicsOverride = MeasureOverwrite.Topics != null && MeasureOverwrite.Topics.Any()
                                    ? MeasureOverwrite.Topics.Select(t => t.Name).ToObservableCollection()
                                    : Measure.Topics.Select(t => t.Name).ToObservableCollection();

            DescriptionOverride = MeasureOverwrite.Description;
            ConsumerDescriptionOverride = MeasureOverwrite.ConsumerDescription;
            ConsumerPlainTitleOverride = MeasureOverwrite.ConsumerPlainTitle;

            UrlOverride = MeasureOverwrite.Url ?? string.Empty;
            UrlTitleOverride = MeasureOverwrite.UrlTitle ?? string.Empty;

            //ScaleByOverride = MeasureOverwrite.ScaleBy.HasValue
            //                      ? MeasureOverwrite.ScaleBy.Value.ToString()
            //                      : string.Empty;
            //DenominatorOverride = MeasureOverwrite.SuppressionDenominator.HasValue ? MeasureOverwrite.SuppressionDenominator.Value.ToString() : string.Empty;
            //NumeratorOverride = MeasureOverwrite.SuppressionNumerator.HasValue ? MeasureOverwrite.SuppressionNumerator.Value.ToString() : string.Empty;
            //UpperBoundOverride = MeasureOverwrite.UpperBound.HasValue ? MeasureOverwrite.UpperBound.Value.ToString() : string.Empty;
            //LowerBoundOverride = MeasureOverwrite.LowerBound.HasValue ? MeasureOverwrite.LowerBound.Value.ToString() : string.Empty;
            //NationalBenchmarkOverride = MeasureOverwrite.NationalBenchmark.HasValue ? MeasureOverwrite.NationalBenchmark.Value.ToString() : string.Empty;
            //ProvidedBenchmarkOverride = MeasureOverwrite.StatePeerBenchmark != null && MeasureOverwrite.StatePeerBenchmark.ProvidedBenchmark.HasValue ? MeasureOverwrite.StatePeerBenchmark.ProvidedBenchmark.ToString() : string.Empty;

            ////BUG: should other titles should also be init here? this code isn't running for the correct measure the measure is re-edited
            //PolicyTitleOverride = MeasureOverwrite.MeasureTitle.Policy;

			ScaleByOverride = MeasureOverwrite.ScaleBy.HasValue ? MeasureOverwrite.ScaleBy.ToString().SubStrBefore(".") : null;// "0";
            HandleScaleByRadioButtons();
            DenominatorOverride = MeasureOverwrite.SuppressionDenominator.ToString();
            NumeratorOverride = MeasureOverwrite.SuppressionNumerator.ToString();
            PerformMarginSuppression = MeasureOverwrite.PerformMarginSuppression;
            UpperBoundOverride = MeasureOverwrite.UpperBound.HasValue && !string.IsNullOrWhiteSpace(MeasureOverwrite.UpperBound.HasValue.ToString()) ? ((double)MeasureOverwrite.UpperBound).ToString() : "0";
            LowerBoundOverride = MeasureOverwrite.LowerBound.HasValue && !string.IsNullOrWhiteSpace(MeasureOverwrite.UpperBound.HasValue.ToString()) ? ((double)MeasureOverwrite.LowerBound).ToString() : "0";
            NationalBenchmarkOverride = MeasureOverwrite.NationalBenchmark.HasValue && MeasureOverwrite.NationalBenchmark.Value != 0
                                            ? MeasureOverwrite.NationalBenchmark.ToString().EndsWith(".0000000") ? MeasureOverwrite.NationalBenchmark.ToString().Replace(".0000000", "") : MeasureOverwrite.NationalBenchmark.ToString()
                                            : "0";
            ProvidedBenchmarkOverride = MeasureOverwrite.StatePeerBenchmark.ProvidedBenchmark.HasValue && MeasureOverwrite.StatePeerBenchmark.ProvidedBenchmark.Value != 0
                                       ? MeasureOverwrite.StatePeerBenchmark.ProvidedBenchmark.ToString().EndsWith(".0000000") ? MeasureOverwrite.StatePeerBenchmark.ProvidedBenchmark.ToString().Replace(".0000000", "") : MeasureOverwrite.StatePeerBenchmark.ProvidedBenchmark.ToString()
                                       : "0";

            CalculationMethodOverride = MeasureOverwrite.StatePeerBenchmark.CalculationMethod;
            //BUG: should other titles also be init here? this code isn't running for the correct measure when the measure is re-edited
            PolicyTitleOverride = MeasureOverwrite.MeasureTitle.Policy;
            PerformMarginSuppressionOverride = Measure.PerformMarginSuppression;
        }

        public void UpdateForMeasureOverride()
        {
            // Map values to override object
            MeasureOverwrite.Description = DescriptionOverride;
            MeasureOverwrite.ConsumerDescription = ConsumerDescriptionOverride;
            MeasureOverwrite.ConsumerPlainTitle = ConsumerPlainTitleOverride;
            MeasureOverwrite.Url = UrlOverride;
            MeasureOverwrite.UrlTitle = UrlTitleOverride;
			MeasureOverwrite.ScaleBy = !String.IsNullOrEmpty(ScaleByOverride) ? Convert.ToDecimal(ScaleByOverride) : (decimal?)null;	// (decimal?)0M;
            MeasureOverwrite.SuppressionDenominator = String.IsNullOrEmpty(DenominatorOverride) ? 0M : Convert.ToDecimal(DenominatorOverride);
            MeasureOverwrite.SuppressionNumerator = String.IsNullOrEmpty(NumeratorOverride) ? 0M : Convert.ToDecimal(NumeratorOverride);
            MeasureOverwrite.PerformMarginSuppression = PerformMarginSuppression;
            MeasureOverwrite.UpperBound = !String.IsNullOrEmpty(UpperBoundOverride) ? Convert.ToDecimal(UpperBoundOverride) : (decimal?)0M;
            MeasureOverwrite.LowerBound = !String.IsNullOrEmpty(LowerBoundOverride) ? Convert.ToDecimal(LowerBoundOverride) : (decimal?)0M;
            MeasureOverwrite.NationalBenchmark = !String.IsNullOrEmpty(NationalBenchmarkOverride) ? Convert.ToDecimal(NationalBenchmarkOverride) : (decimal?)0M;
            MeasureOverwrite.StatePeerBenchmark = !String.IsNullOrEmpty(ProvidedBenchmarkOverride) ?
                                                  new StatePeerBenchmark() { ProvidedBenchmark = Convert.ToDecimal(ProvidedBenchmarkOverride), CalculationMethod = CalculationMethodOverride }
                                                : new StatePeerBenchmark() { ProvidedBenchmark = (decimal?)0M };
            MeasureOverwrite.PerformMarginSuppression = PerformMarginSuppressionOverride;
            this.Measure.Topics.ForEach(t => MeasureOverwrite.AddTopic(t));
        }

        private ObservableCollection<string> GetTopics()
        {
            if (Measure != null)
            {
                return Measure.Topics.Select(t => t.Name).ToObservableCollection();
            }
            return new ObservableCollection<string>();
        }

        //before saving measure get all selected topics from parent view model and add them to the measure
        public void ReconcileTopics(WebsiteMeasuresViewModel websiteMeasuresViewModel, Measure measure)
        {
            if (measure == null)
            {
                return;
            }



            var topicViewModels = websiteMeasuresViewModel.TopicsCollectionView.OfType<TopicViewModel>();
            var topics = new List<Topic>();

            foreach (var t in topicViewModels)
            {
                if (!t.ChildrenCollectionView.Any())
                {
                    continue;
                }
                topics.AddRange(t.SelectedTopics);
            }

            measure.ClearTopics();
            if (topics.Any())
            {
                foreach (var topic in topics)
                {
                    measure.AddTopic(topic);
                }
            }
        }

        public void Rollback()
        {
            var service = ServiceLocator.Current.GetInstance<IMeasureService>();
            var updatedMeasure = service.Refresh(Measure);
            //var parentViewModel = ServiceLocator.Current.GetInstance<MeasureDetailsViewModel>();
            InitMeasure(updatedMeasure);
            //parentViewModel.UpdateCurrentMeasure(this);
        }

        private void Reset()
        {
            Committed = true;
        }

        public ObservableCollection<string> GetWebsites()
        {
            //// Inga: I changed this in changeset 539 per Vivek
            //return new ObservableCollection<string> 
            //    { 
            //        "2012 Q1 AZ Hospital Compare",
            //        "2012 Q2 AZ Hospital Compare",
            //        "2012 Q3 AZ Hospital Compare"
            //    };
            var measureId = int.Parse(Id);
            return WebsiteDataService.GetWebsiteNamesForMeasure(measureId).ToObservableCollection();
        }

        // If lower/upper just passed ValidateLess(), then so does the other field, but the other field might
        // still have a range error left over from previous LostFocus. So clear it now too.
        // We MUST set only 1 validation error at a time for upper/lower bound. It can't be non-numeric and range error.
        private void ClearRangeError(string PropertyName)
        {
            // This uses foreach because it's IEnumerable, but calls ClearErrors below if a single match is found.
            // Copy the errors collection because we might clear it in the loop.
            var errors = GetErrors(PropertyName) as IEnumerable<string>;
            if (errors == null) return;
            foreach (var err in errors)
            {
                if (string.Compare(err, ValidateLessMessage) == 0)
                {
                    ClearErrors(PropertyName);
                    break;
                }
            }
        }

        private void ValidateLess(string lowerName, string lowerValue, string upperName, string upperValue, string propertyName)
        {
            ClearErrors(propertyName);
            if (!string.IsNullOrWhiteSpace(lowerValue) && !string.IsNullOrWhiteSpace(upperValue))
            {
                if (IsNumeric(lowerValue) && IsNumeric(lowerValue))
                {
                    if (double.Parse(lowerValue) > double.Parse(upperValue))
                    {
                        SetError(propertyName, ValidateLessMessage);
                    }
                }
            }
        }

        private void ValidateNumeric(string p, string value)
        {
            ClearErrors(p);
            if (!IsNumeric(value))
            {
                SetError(p, "Please provide a valid numeric value.");
            }
        }

        private bool IsNumeric(string value)
        {
            // valid format is:
            // ^ = start of text
            // - (negative sign optional)
            // 0-9 digits
            // decimal point (optional)
            // 0-4 digits after decimal point
            // $ = end of text
            var regexNumeric = new Regex(@"^(0|(-?(((0|[1-9]\d{0,9})\.\d{0,8})|([1-9]\d{0,9}))))$");
            return !string.IsNullOrEmpty(value) && regexNumeric.IsMatch(value);
        }

        protected override void ValidateAll()
        {
            // This validates all 3 titles so at least 1 is present, doesn't matter which
            _ValidateTitle(ExtractPropertyName(() => ClinicalTitle), ClinicalTitle);

            _ValidateUrl(ExtractPropertyName(() => Url), Url);
            _ValidateUrlTitle(ExtractPropertyName(() => UrlTitle), UrlTitle);
            ValidateNumeric(ExtractPropertyName(() => ScaleBy), ScaleBy);
            ValidateNumeric(ExtractPropertyName(() => UpperBound), UpperBound);
            ValidateNumeric(ExtractPropertyName(() => LowerBound), LowerBound);
            ValidateNumeric(ExtractPropertyName(() => Numerator), Numerator);
            ValidateNumeric(ExtractPropertyName(() => Denominator), Denominator);
            ValidateNumeric(ExtractPropertyName(() => NationalBenchmark), NationalBenchmark);

            // only do numeric validation if benchmark # is provided, not calculated mean
            if (CalculationMethod == StatePeerBenchmarkCalculationMethod.Provided) ValidateNumeric(ExtractPropertyName(() => ProvidedBenchmark), ProvidedBenchmark);
        }

        #endregion

        #region Override Measure Fields

        public string ConsumerDescriptionOverride { get; set; }

		private string _consumerPlainTitleOverride;
		public string ConsumerPlainTitleOverride
		{
			get { return _consumerPlainTitleOverride; }
			set
			{
				_consumerPlainTitleOverride = value;
				System.Windows.Input.CommandManager.InvalidateRequerySuggested();
			}
		}

        ///// <summary>
        ///// Gets or sets the websites.
        ///// </summary>
        ///// 
        ///// <value>
        ///// The websites.
        ///// </value>
        //public ObservableCollection<string> WebsitesMe { get; set; }

        //public ObservableCollection<string> Topics { get; set; }

        public string IdOverride
        {
            get
            {
                _idOverride = MeasureOverwrite != null ? MeasureOverwrite.Id.ToString() : 0.ToString();
                return _idOverride;
            }
            set
            {
                //if (value == _idOverride) return;
                _idOverride = value;
                RaisePropertyChanged(() => IdOverride);

                if (MeasureOverwrite != null)
                    MeasureOverwrite.Id = int.Parse(value);
            }
        }

        public bool WasMeasureOverrideEdited { get; set; }

        public string DataSetNameOverride
        {
            get { return MeasureOverwrite != null ? MeasureOverwrite.Owner.Name : string.Empty; }
            set
            {
                // if (value == MeasureOverwrite.Owner.Name) return;
                MeasureOverwrite.Owner.Name = value;
                RaisePropertyChanged(() => DataSetNameOverride);
            }
        }

        public string ClinicalTitleOverride
        {
            get { return MeasureOverwrite != null && MeasureOverwrite.MeasureTitle != null ? MeasureOverwrite.MeasureTitle.Clinical : string.Empty; }
            set
            {
                //if (value == MeasureOverwrite.MeasureTitle.Clinical) return;
                MeasureOverwrite.MeasureTitle.Clinical = value;
                RaisePropertyChanged(() => ClinicalTitleOverride);
				System.Windows.Input.CommandManager.InvalidateRequerySuggested();
				_ValidateTitleOverride("ClinicalTitleOverride", value);
			}
        }

        private void _ValidateTitleOverride(string p, string value)
        {
            ClearErrors(p);
            if (string.IsNullOrWhiteSpace(ClinicalTitleOverride)
                && string.IsNullOrWhiteSpace(PlainTitleOverride)
                && string.IsNullOrWhiteSpace(PolicyTitleOverride)
                )
            {
                SetError(p, "Please make sure at least one title is provided.");
            }
        }

        public bool IsPlainTitleSelected
        {
            get;
            set;
        }

        public string PlainTitleOverride
        {
            get { return MeasureOverwrite != null && MeasureOverwrite.MeasureTitle != null ? MeasureOverwrite.MeasureTitle.Plain : string.Empty; }
            set
            {
                //if (value == MeasureOverwrite.MeasureTitle.Plain) return;
                MeasureOverwrite.MeasureTitle.Plain = value;
                RaisePropertyChanged(() => PlainTitleOverride);
				System.Windows.Input.CommandManager.InvalidateRequerySuggested();
				_ValidateTitleOverride("PlainTitleOverride", value);
            }
        }

        public string PolicyTitleOverride
        {
            get { return MeasureOverwrite != null && MeasureOverwrite.MeasureTitle != null ? MeasureOverwrite.MeasureTitle.Policy : string.Empty; }
            set
            {
                //if (value == MeasureOverwrite.MeasureTitle.Policy) return;
                MeasureOverwrite.MeasureTitle.Policy = value;
                RaisePropertyChanged(() => PolicyTitleOverride);
                _ValidateTitleOverride("PolicyTitleOverride", value);
            }
        }

        public StatePeerBenchmarkCalculationMethod CalculationMethodOverride
        {
            get { return MeasureOverwrite != null ? MeasureOverwrite.StatePeerBenchmark.CalculationMethod : StatePeerBenchmarkCalculationMethod.Calculated_Mean; }
            set { MeasureOverwrite.StatePeerBenchmark.CalculationMethod = value; }
        }

        // BUG: this field isn't running validation on every char typed (PropertyChanged), only on FocusChanged event

        public string ProvidedBenchmarkOverride
        {
            get { return _providedBenchmarkOverride; }
            set
            {
                //if (value == _providedBenchmarkOverride) return;
                _providedBenchmarkOverride = value;
                RaisePropertyChanged(() => ProvidedBenchmarkOverride);

                var propertyName = "ProvidedBenchmarkOverride";
                ValidateNumeric(propertyName, value);

                if (GetErrors(propertyName) == null)
                    MeasureOverwrite.StatePeerBenchmark.ProvidedBenchmark = decimal.Parse(value);
            }
        }

        public string CodeOverride
        {
            get
            {
                _codeOverride = MeasureOverwrite != null
                                           ? MeasureOverwrite.MeasureCode
                                           : string.Empty;

                return _codeOverride;
            }
            set
            {

                //if (value == _codeOverride) return;
                _codeOverride = value;
                RaisePropertyChanged(() => CodeOverride);
                if (MeasureOverwrite != null)
                {
                    MeasureOverwrite.Name = _codeOverride;
                }
            }
        }

        public string DescriptionOverride
        {
            get;
            set;
        }

        public bool HigherScoresAreBetterOverride
        {
            get
            {
                if (MeasureOverwrite != null)
                {
                    return MeasureOverwrite.HigherScoresAreBetter;
                }
                else
                {
                    return Measure.HigherScoresAreBetter;
                }
            }
            set
            {
                if (MeasureOverwrite != null)
                {
                    MeasureOverwrite.HigherScoresAreBetter = value;
                }
                else
                {
                    Measure.HigherScoresAreBetter = value;
                }
            }
        }

        public string HigherScoresText
        {
            get
            {
                return HigherScoresAreBetterOverride ? "Higher the better" : "Lower the better";
            }
        }

        public bool LowerScoresAreBetterOverride
        {
            get
            {
                if (MeasureOverwrite != null)
                {
                    return !MeasureOverwrite.HigherScoresAreBetter;
                }
                else
                {
                    return !Measure.HigherScoresAreBetter;
                }
            }
        }

        public string MoreInformationOverride
        {
            get { return MeasureOverwrite != null ? MeasureOverwrite.MoreInformation : string.Empty; }
            set { MeasureOverwrite.MoreInformation = value; }
        }

        public string UrlTitleOverride
        {
            get { return MeasureOverwrite != null ? MeasureOverwrite.UrlTitle : string.Empty; }
            set
            {
                //if (value == Measure.UrlTitle) return;
                MeasureOverwrite.UrlTitle = value;
                RaisePropertyChanged(() => UrlTitleOverride);
                _ValidateUrlTitleOverride("UrlOverride", value);
                _ValidateUrlOverride("UrlOverride", UrlOverride);
            }
        }

        private void _ValidateUrlTitleOverride(string p, string value)
        {
            ClearErrors(p);
            if (string.IsNullOrWhiteSpace(UrlOverride) && string.IsNullOrWhiteSpace(UrlTitleOverride)) return;
            if (string.IsNullOrWhiteSpace(value))
            {
                SetError(p, "Please provide a URL Title.");
            }
        }

        public string UrlOverride
        {
            get
            {
                return _urlOverride;
            }
            set
            {
                //if (value == _urlOverride) return;
                _urlOverride = value;
                RaisePropertyChanged(() => UrlOverride);
                _ValidateUrlOverride("UrlOverride", value);
                _ValidateUrlTitleOverride("UrlTitleOverride", UrlTitleOverride);
            }
        }

        //const string ValidUrlMessage = "Please provide a valid URL, beginning with http:// or https://.";

        private void _ValidateUrlOverride(string p, string value)
        {
            ClearErrors(p);
            if (string.IsNullOrWhiteSpace(UrlTitleOverride) && string.IsNullOrWhiteSpace(UrlOverride)) return;

            if (string.IsNullOrWhiteSpace(value))
            {
                SetError(p, "Please provide a URL.");
                return;
            }
            // user MUST type http: or https:
            var regexUrl = new Regex(@"^(http|https)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$");

            if (!regexUrl.IsMatch(value))
            {
                SetError(p, ValidUrlMessage);
            }
            else
            {
                // our regex might not catch every problem that .NET considers invalid, so let .NET finally decide if the user text is valid.
                // but using only try/catch would allow non-http: and unusual uri's
                try
                {
                    Measure.Url = value;
                }
                catch (Exception)
                {
                    SetError(p, ValidUrlMessage);
                }
            }
        }

        public string FootnotesOverride
        {
            get { return MeasureOverwrite != null ? MeasureOverwrite.Footnotes : string.Empty; }
            set { MeasureOverwrite.Footnotes = value; }
        }

        public string ScaleByOverride
        {
            get { return _scaleByOverride; }
            set
            {
                //if (value == _scaleByOverride) return;
                _scaleByOverride = value;
                RaisePropertyChanged(() => ScaleByOverride);

                var propertyName = "ScaleByOverride";
                ValidateNumeric(propertyName, value);
                if (GetErrors(propertyName) == null)
                    MeasureOverwrite.ScaleBy = decimal.Parse(value);
            }
        }

        public string NationalBenchmarkOverride
        {
            get { return _nationalBenchmarkOverride; }
            set
            {
                //if (value == _nationalBenchmarkOverride) return;
                _nationalBenchmarkOverride = value;
                RaisePropertyChanged(() => NationalBenchmarkOverride);

                //var propertyName = ExtractPropertyName(() => NationalBenchmarkOverride);
                ValidateNumeric("NationalBenchmarkOverride", value);

                if (GetErrors("NationalBenchmarkOverride") == null)
                    MeasureOverwrite.NationalBenchmark = decimal.Parse(value);
            }
        }

        public string UpperBoundOverride
        {
            get { return _upperBoundOverride; }
            set
            {
                //if (value == _upperBoundOverride) return;
                _upperBoundOverride = value;
                RaisePropertyChanged(() => UpperBoundOverride);

                const string lowerPropertyName = "LowerBoundOverride"; // ExtractPropertyName(() => LowerBoundOverride);
                const string upperPropertyName = "UpperBoundOverride"; // ExtractPropertyName(() => UpperBoundOverride);

                ValidateNumeric(upperPropertyName, value);

                if (GetErrors(upperPropertyName) == null)
                    ValidateLess(lowerPropertyName, LowerBoundOverride, upperPropertyName, value, upperPropertyName);

                if (GetErrors(upperPropertyName) == null)
                {
                    ClearRangeError(lowerPropertyName);
                    MeasureOverwrite.UpperBound = decimal.Parse(value);
                }
            }
        }

        public string LowerBoundOverride
        {
            get { return _lowerBoundOverride; }
            set
            {
                //if (value == _lowerBoundOverride) return;
                _lowerBoundOverride = value;
                RaisePropertyChanged(() => LowerBoundOverride);

                var lowerPropertyName = "LowerBoundOverride"; // ExtractPropertyName(() => LowerBoundOverride);
                var upperPropertyName = "UpperBoundOverride"; // ExtractPropertyName(() => UpperBoundOverride);

                ValidateNumeric(lowerPropertyName, value);

                if (GetErrors(lowerPropertyName) == null)
                    ValidateLess(lowerPropertyName, value, upperPropertyName, UpperBoundOverride, lowerPropertyName);

                if (GetErrors(lowerPropertyName) == null)
                {
                    ClearRangeError(upperPropertyName);
                    MeasureOverwrite.LowerBound = decimal.Parse(value);
                }
            }
        }

        //// If lower/upper just passed ValidateLess(), then so does the other field, but the other field might
        //// still have a range error left over from previous LostFocus. So clear it now too.
        //// We MUST set only 1 validation error at a time for upper/lower bound. It can't be non-numeric and range error.
        //private void ClearRangeError(string PropertyName)
        //{
        //    // This uses foreach because it's IEnumerable, but calls ClearErrors below if a single match is found.
        //    // Copy the errors collection because we might clear it in the loop.
        //    var errors = GetErrors(PropertyName) as IEnumerable<string>;
        //    if (errors == null) return;
        //    foreach (var err in errors)
        //    {
        //        if (string.Compare(err, ValidateLessMessage) == 0)
        //        {
        //            ClearErrors(PropertyName);
        //            break;
        //        }
        //    }
        //}

        public string NumeratorOverride
        {
            get { return _numeratorOverride; }
            set
            {
                //if (value == _numeratorOverride) return;
                _numeratorOverride = value;
                RaisePropertyChanged(() => NumeratorOverride);

                const string propertyName = "NumeratorOverride"; // ExtractPropertyName(() => NumeratorOverride);
                ValidateNumeric(propertyName, value);
                if (GetErrors(propertyName) == null)
                    MeasureOverwrite.SuppressionNumerator = decimal.Parse(value);
            }
        }

        public string DenominatorOverride
        {
            get { return _denominatorOverride; }
            set
            {
                //if (value == _denominatorOverride) return;
                _denominatorOverride = value;
                RaisePropertyChanged(() => DenominatorOverride);

                var propertyName = "DenominatorOverride"; // ExtractPropertyName(() => DenominatorOverride);
                ValidateNumeric(propertyName, value);
                if (GetErrors(propertyName) == null)
                    MeasureOverwrite.SuppressionDenominator = decimal.Parse(value);
            }
        }

        public ObservableCollection<string> CalculatedOptionsOverride { get; set; }

        public string AssignedCalculatedOptionOverride { get; set; }

        public string BetterHighOrLowOverride
        {
            get
            {
                return HigherScoresAreBetterOverride ? "Higher is better" : "Lower is better";
            }
        }

        #endregion

        public bool IsSelected { get; set; }
    }
}
