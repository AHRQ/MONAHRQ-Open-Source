using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Measures.Events;
using Monahrq.Default.ViewModels;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Types;
using Monahrq.Measures.Service;
using Monahrq.Sdk.Events;
using PropertyChanged;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// Measure model class
    /// </summary>
    /// <seealso cref="Monahrq.Default.ViewModels.BaseViewModel" />
    [ImplementPropertyChanged]
    public class MeasureModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the measure service.
        /// </summary>
        /// <value>
        /// The measure service.
        /// </value>
        [Import]
        public IMeasureServiceSync MeasureService { get; set; }

        /// <summary>
        /// Gets or sets the measure details vm.
        /// </summary>
        /// <value>
        /// The measure details vm.
        /// </value>
        [Import]
        public MeasureDetailsViewModel MeasureDetailsVm { get; set; }

        /// <summary>
        /// Gets or sets the assign topic command.
        /// </summary>
        /// <value>
        /// The assign topic command.
        /// </value>
        public DelegateCommand AssignTopicCommand { get; set; }                 // TODO

        /// <summary>
        /// Gets or sets the measure.
        /// </summary>
        /// <value>
        /// The measure.
        /// </value>
        public Measure Measure { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the topics is updated.
        /// </summary>
        /// <value>
        /// <c>true</c> if topics is updated; otherwise, <c>false</c>.
        /// </value>
        public bool IsTopicsUpdated { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureModel"/> class.
        /// </summary>
        public MeasureModel()
        {
            IsVisible = true;
            Websites = new ObservableCollection<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureModel"/> class.
        /// </summary>
        /// <param name="measure">The measure.</param>
        public MeasureModel(Measure measure)
        {
            InitMeasure(measure);

            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "IsSelectedForTopicAssignment")
                {
                    Events.GetEvent<SelectedTopicAssigmentEvent>().Publish(false);
                }
                if (e.PropertyName == "IsTopicsUpdated")
                {
                    Events.GetEvent<SelectedTopicAssigmentEvent>().Publish(true);
                }
            };

            Reset();
        }




        /// <summary>
        /// Initializes the measure.
        /// </summary>
        /// <param name="measure">The measure.</param>
        public void InitMeasure(Measure measure)
        {
            Measure = measure;

            //if (Measure.MeasureTitle.Selected == null)
            //{
            //    Measure.MeasureTitle.Selected = Measure.MeasureTitle.Plain;
            //}

            //if (!Measure.Metadata.Any())
            //{
            //    Measure.Metadata.Add(new MetadataItem(Measure, "Reference metadata") { Value = "N/A" });
            //}

            IsVisible = true;

            Websites = GetWebsites(measure.Id);

            Url = measure.Url ?? string.Empty;
            UrlTitle = measure.UrlTitle ?? string.Empty;

            ScaleBy = measure.ScaleBy.HasValue ? measure.ScaleBy.ToString().SubStrBefore(".") : "0";
            HandleScaleByRadioButtons();
            Denominator = measure.SuppressionDenominator.HasValue ? measure.SuppressionDenominator.ToString() : "0";
            Numerator = measure.SuppressionNumerator.HasValue ? measure.SuppressionNumerator.ToString() : "0";
            PerformMarginSuppression = measure.PerformMarginSuppression;
            UpperBound = measure.UpperBound.HasValue ? measure.UpperBound.ToString() : "0";
            LowerBound = measure.LowerBound.HasValue ? measure.LowerBound.ToString() : "0";
            NationalBenchmark = measure.NationalBenchmark.HasValue && measure.NationalBenchmark.Value != 0
                                            ? measure.NationalBenchmark.ToString() : "0";
            ProvidedBenchmark = measure.StatePeerBenchmark.ProvidedBenchmark.HasValue && measure.StatePeerBenchmark.ProvidedBenchmark.Value != 0
                                            ? measure.StatePeerBenchmark.ProvidedBenchmark.ToString() : "0";

            //BUG: should other titles also be init here? this code isn't running for the correct measure when the measure is re-edited
            PolicyTitle = measure.MeasureTitle.Policy;

            Reset();
        }

        /// <summary>
        /// Handles the scale by radio buttons.
        /// </summary>
        private void HandleScaleByRadioButtons()
        {
            if (IsScaleMeasure)
            {
                IsMinScaleByRadioButtonChecked = false;
                IsMediumScaleByRadioButtonChecked = false;
                IsMaxScaleByRadioButtonChecked = false;
                switch (ScaleBy)
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

        //before saving measure get all selected topics from parent view model and add them to the measure
        /// <summary>
        /// Reconciles the topics before save.
        /// </summary>
        public void ReconcileTopicsBeforeSave()
        {
            var topicsvm = MeasureDetailsVm.TopicViewModels;
            var topics = new List<Topic>();

            foreach (TopicViewModel topicViewModel in topicsvm)
            {
                if (!topicViewModel.ChildrenCollectionView.Any()) continue;

                //var selectedTopics = topicViewModel.ChildrenCollectionView.OfType<SubTopicViewModel>()
                //                                   .Where(subtopic => subtopic.IsSelected)
                //                                   .Select(subtopic => subtopic.Entity);

                var selectedTopics = topicViewModel.SelectedTopics;

                topics.AddRange(selectedTopics);
            }

            Measure.ClearTopics();

            foreach (var topic in topics)
            {
                Measure.AddTopic(topic);
            }
        }

        /// <summary>
        /// Committs measure.
        /// </summary>
        public async void Committ()
        {
           
            await OnCommitted();
        }

        /// <summary>
        /// Use this to perform whatever action is needed when committed
        /// </summary>
        /// <returns></returns>
        protected async override Task<bool> OnCommitted()
        {
            ReconcileTopicsBeforeSave();

            Measure.IsLibEdit = true;

            var errorOccurred = false;
            var progressService = new ProgressService();

            progressService.SetProgress("Saving measure", 0, false, true);

            // await Task.Delay(500);

            var operationComplete = await progressService.Execute(() =>
            {
                MeasureService.SaveMeasure(Measure, (o, e) =>
                {
                    if (e != null)
                    {
						progressService.SetProgress("Failed", 100, true, false);
						throw e;
                    }
                });
            },
            opResult =>
            {
                if (!opResult.Status || opResult.Exception != null)
                {
                        SynchronizationContext.Current.Post(x => {
                        errorOccurred = true;
                        var ex = opResult.Exception;
                        Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
                    }, null);
                }

                progressService.SetProgress("Completed", 100, true, false);
            },
            new CancellationToken());

            if (operationComplete && !errorOccurred)
            {
                Events.GetEvent<TopicsUpdatedEvent>().Publish(Measure.Id);
                var msg = string.Format("Measure \"{0}\" was successfully save and/or updated.", ClinicalTitle);
                Events.GetEvent<GenericNotificationEvent>().Publish(msg);
                Reset();
                return true;
            }

            if (errorOccurred)
            {
                return false;
            }
            return true;
        }

        //todo move out hard refernece of the service //crappy code alert

        /// <summary>
        /// Rollbacks this measure.
        /// </summary>
        public void Rollback()
        {
            if (MeasureService == null)
                MeasureService = ServiceLocator.Current.GetInstance<IMeasureServiceSync>();
            //  var service = ServiceLocator.Current.GetInstance<IMeasureServiceSync>();
            var updatedMeasure = MeasureService.Refresh(Measure);         //.GetById<Measure>(Measure.Id);

            //bug possible MEF might create multiple versions /ensure only one import is create or better fix the architecture
            var parentViewModel = ServiceLocator.Current.GetInstance<MeasureDetailsViewModel>();

            //todo: null reference exception FIX this
            InitMeasure(updatedMeasure);

            parentViewModel.UpdateCurrentMeasure(this);
        }

        /// <summary>
        /// Resets this measure.
        /// </summary>
        private void Reset()
        {
            Committed = true;
        }

        #region MAX LENGTH PROPERTIES

        /// <summary>
        /// Gets the maximum length of the more information.
        /// </summary>
        /// <value>
        /// The maximum length of the more information.
        /// </value>
        public int MoreInformationMaxLength
        {
            get { return 500; }
        }
        /// <summary>
        /// Gets the maximum length of the measure name.
        /// </summary>
        /// <value>
        /// The maximum length of the measure name.
        /// </value>
        public int MeasureNameMaxLength
        {
            get { return 200; }            // max length for the user to type in the Alternate measure names textboxes
        }
        /// <summary>
        /// Gets the maximum length of the footnotes.
        /// </summary>
        /// <value>
        /// The maximum length of the footnotes.
        /// </value>
        public int FootnotesMaxLength
        {
            get { return 500; }
        }
        /// <summary>
        /// Gets the maximum length of the URL.
        /// </summary>
        /// <value>
        /// The maximum length of the URL.
        /// </value>
        public int UrlMaxLength
        {
            get { return 300; }
        }
        /// <summary>
        /// Gets the maximum length of the URL title.
        /// </summary>
        /// <value>
        /// The maximum length of the URL title.
        /// </value>
        public int UrlTitleMaxLength
        {
            get { return 100; }
        }

        #endregion

        #region Measure Fields

        /// <summary>
        /// Gets or sets the websites.
        /// </summary>
        /// <value>
        /// The websites.
        /// </value>
        public ObservableCollection<string> Websites { get; set; }

        /// <summary>
        /// Gets the name of the data set.
        /// </summary>
        /// <value>
        /// The name of the data set.
        /// </value>
        public string DataSetName
        {
            get { return Measure.Owner.Name; }
        }

        /// <summary>
        /// Gets or sets the clinical title.
        /// </summary>
        /// <value>
        /// The clinical title.
        /// </value>
        public string ClinicalTitle
        {
            get { return Measure.MeasureTitle.Clinical; }
            set
            {
                //if (value == Measure.MeasureTitle.Clinical) return;
                Measure.MeasureTitle.Clinical = value;
                _ValidateTitle(ExtractPropertyName(() => ClinicalTitle), value);
                Committed = false;
                RaisePropertyChanged(() => ClinicalTitle);
            }
        }

        /// <summary>
        /// Validates the title.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="value">The value.</param>
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

        /// <summary>
        /// Gets or sets the plain title.
        /// </summary>
        /// <value>
        /// The plain title.
        /// </value>
        public string PlainTitle
        {
            get { return Measure.MeasureTitle.Plain; }
            set
            {
                //if (value == Measure.MeasureTitle.Plain) return;
                Measure.MeasureTitle.Plain = value;
                Committed = false;
                RaisePropertyChanged(() => PlainTitle);
                _ValidateTitle(ExtractPropertyName(() => PlainTitle), value);
            }
        }

        /// <summary>
        /// Gets or sets the policy title.
        /// </summary>
        /// <value>
        /// The policy title.
        /// </value>
        public string PolicyTitle
        {
            get { return Measure.MeasureTitle.Policy; }
            set
            {
                //if (value == Measure.MeasureTitle.Policy) return;
                Measure.MeasureTitle.Policy = value;
                Committed = false;
                RaisePropertyChanged(() => PolicyTitle);
                _ValidateTitle(ExtractPropertyName(() => PolicyTitle), value);
            }
        }

        /// <summary>
        /// Gets or sets the calculation method.
        /// </summary>
        /// <value>
        /// The calculation method.
        /// </value>
        public StatePeerBenchmarkCalculationMethod CalculationMethod
        {
            get { return Measure.StatePeerBenchmark.CalculationMethod; }
            set
            {
                Measure.StatePeerBenchmark.CalculationMethod = value;
                Committed = false;
                RaisePropertyChanged(() => CalculationMethod);

            }
        }

        // BUG: this field isn't running validation on every char typed (PropertyChanged), only on FocusChanged event
        string _ProvidedBenchmark;
        /// <summary>
        /// Gets or sets the provided benchmark.
        /// </summary>
        /// <value>
        /// The provided benchmark.
        /// </value>
        public string ProvidedBenchmark
        {
            get { return _ProvidedBenchmark; }
            set
            {
                //if (value == _ProvidedBenchmark) return;
                _ProvidedBenchmark = value;
                Committed = false;
                RaisePropertyChanged(() => ProvidedBenchmark);

                var propertyName = ExtractPropertyName(() => ProvidedBenchmark);
                _ValidateNumeric(propertyName, value);

                if (GetErrors(propertyName) == null)
                    Measure.StatePeerBenchmark.ProvidedBenchmark = decimal.Parse(value);

            }
        }

        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code
        {
            get { return Measure.MeasureCode; }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get { return Measure.Description; }
            set
            {
                Measure.Description = value;
                Committed = false;
                RaisePropertyChanged(() => Description);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [higher scores are better].
        /// </summary>
        /// <value>
        /// <c>true</c> if [higher scores are better]; otherwise, <c>false</c>.
        /// </value>
        public bool HigherScoresAreBetter
        {
            get { return Measure.HigherScoresAreBetter; }
            set
            {
                Measure.HigherScoresAreBetter = value;
                Committed = false;
                RaisePropertyChanged(() => HigherScoresAreBetter);
            }
        }

        /// <summary>
        /// Gets the higher scores text.
        /// </summary>
        /// <value>
        /// The higher scores text.
        /// </value>
        public string HigherScoresText
        {
            get
            {
                return HigherScoresAreBetter ? "Higher the better" : "Lower the better";
            }
        }

        /// <summary>
        /// Gets a value indicating whether [lower scores are better].
        /// </summary>
        /// <value>
        /// <c>true</c> if [lower scores are better]; otherwise, <c>false</c>.
        /// </value>
        public bool LowerScoresAreBetter
        {
            get { return !Measure.HigherScoresAreBetter; }
        }

        /// <summary>
        /// Gets or sets the more information.
        /// </summary>
        /// <value>
        /// The more information.
        /// </value>
        public string MoreInformation
        {
            get { return Measure.MoreInformation; }
            set
            {
                Measure.MoreInformation = value;
                Committed = false;
                RaisePropertyChanged(() => MoreInformation);
            }
        }

        /// <summary>
        /// Gets or sets the conumer plain title.
        /// </summary>
        /// <value>
        /// The conumer plain title.
        /// </value>
        public string ConumerPlainTitle
        {
            get { return Measure.ConsumerPlainTitle; }
            set
            {
                Measure.ConsumerPlainTitle = value;
                RaisePropertyChanged(() => ConumerPlainTitle);
            }
        }

        /// <summary>
        /// Gets or sets the consumer description.
        /// </summary>
        /// <value>
        /// The consumer description.
        /// </value>
        public string ConsumerDescription
        {
            get { return Measure.ConsumerDescription; }
            set
            {
                Measure.ConsumerDescription = value;
                RaisePropertyChanged(() => ConsumerDescription);
            }
        }

        #region URL_URLTitle

        /// <summary>
        /// Gets or sets the URL title.
        /// </summary>
        /// <value>
        /// The URL title.
        /// </value>
        public string UrlTitle
        {
            get { return Measure.UrlTitle; }
            set
            {
                //if (value == Measure.UrlTitle) return;
                Measure.UrlTitle = value;
                RaisePropertyChanged(() => UrlTitle);
                _ValidateUrlTitle(ExtractPropertyName(() => UrlTitle), value);
                _ValidateUrl(ExtractPropertyName(() => Url), Url);
            }
        }

        /// <summary>
        /// Validates the URL title.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="value">The value.</param>
        private void _ValidateUrlTitle(string p, string value)
        {
            ClearErrors(p);
            if (string.IsNullOrWhiteSpace(Url) && string.IsNullOrWhiteSpace(UrlTitle)) return;
            if (string.IsNullOrWhiteSpace(value))
            {
                SetError(p, "Please provide a URL Title.");
            }
        }

        private string _Url;
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                //if (value == _Url) return;
                _Url = value;
                RaisePropertyChanged(() => Url);
                _ValidateUrl(ExtractPropertyName(() => Url), value);
                _ValidateUrlTitle(ExtractPropertyName(() => UrlTitle), UrlTitle);
            }
        }

        /// <summary>
        /// The valid URL message
        /// </summary>
        const string ValidUrlMessage = "Please provide a valid URL, beginning with http:// or https://.";

        /// <summary>
        /// Validates the URL.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="value">The value.</param>
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
                    Measure.Url = new Uri(value).AbsoluteUri;
                }
                catch (Exception)
                {
                    SetError(p, ValidUrlMessage);
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the footnotes.
        /// </summary>
        /// <value>
        /// The footnotes.
        /// </value>
        public string Footnotes
        {
            get { return Measure.Footnotes; }
            set { Measure.Footnotes = value; }
        }

        string _ScaleBy;
        /// <summary>
        /// Gets or sets the scale by.
        /// </summary>
        /// <value>
        /// The scale by.
        /// </value>
        public string ScaleBy
        {
            get { return _ScaleBy; }
            set
            {
                //if (value == _ScaleBy) return;
                _ScaleBy = value;
                RaisePropertyChanged(() => ScaleBy);

                var propertyName = ExtractPropertyName(() => ScaleBy);
                _ValidateNumeric(propertyName, value);
                if (GetErrors(propertyName) == null)
                    Measure.ScaleBy = decimal.Parse(value);
            }
        }

        /// <summary>
        /// The is minimum scale by RadioButton checked
        /// </summary>
        private bool _isMinScaleByRadioButtonChecked;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is minimum scale by RadioButton checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is minimum scale by RadioButton checked; otherwise, <c>false</c>.
        /// </value>
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
                    ScaleBy = "1000";
                }
            }
        }

        /// <summary>
        /// The is medium scale by RadioButton checked
        /// </summary>
        private bool _isMediumScaleByRadioButtonChecked;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is medium scale by RadioButton checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is medium scale by RadioButton checked; otherwise, <c>false</c>.
        /// </value>
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
                    ScaleBy = "10000";
                }
            }
        }

        /// <summary>
        /// The is maximum scale by RadioButton checked
        /// </summary>
        private bool _isMaxScaleByRadioButtonChecked;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is maximum scale by RadioButton checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is maximum scale by RadioButton checked; otherwise, <c>false</c>.
        /// </value>
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
                    ScaleBy = "100000";
                }
            }
        }

        /// <summary>
        /// Gets the is scale by RadioButton fuctionality visible.
        /// </summary>
        /// <value>
        /// The is scale by RadioButton fuctionality visible.
        /// </value>
        public Visibility IsScaleByRadioButtonFuctionalityVisible
        {
            get
            {
                return IsScaleMeasure ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Gets the is scale by text box fuctionality visible.
        /// </summary>
        /// <value>
        /// The is scale by text box fuctionality visible.
        /// </value>
        public Visibility IsScaleByTextBoxFuctionalityVisible
        {
            get
            {
                return IsScaleMeasure ? Visibility.Collapsed : (Measure is NursingHomeMeasure) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is scale measure.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is scale measure; otherwise, <c>false</c>.
        /// </value>
        private bool IsScaleMeasure
        {
            get
            {
                return Code == "IP-11" || Code == "IP-15";
            }
        }

        string _NationalBenchmark;
        /// <summary>
        /// Gets or sets the national benchmark.
        /// </summary>
        /// <value>
        /// The national benchmark.
        /// </value>
        public string NationalBenchmark
        {
            get { return _NationalBenchmark; }
            set
            {
                //if (value == _NationalBenchmark) return;
                _NationalBenchmark = value;
                RaisePropertyChanged(() => NationalBenchmark);

                var propertyName = ExtractPropertyName(() => NationalBenchmark);
                _ValidateNumeric(propertyName, value);

                if (GetErrors(propertyName) == null)
                    Measure.NationalBenchmark = decimal.Parse(value);
            }
        }

        string _UpperBound;
        /// <summary>
        /// Gets or sets the upper bound.
        /// </summary>
        /// <value>
        /// The upper bound.
        /// </value>
        public string UpperBound
        {
            get { return _UpperBound; }
            set
            {
                if (value == _UpperBound) return;
                _UpperBound = value;
                Committed = false;
                RaisePropertyChanged(() => UpperBound);

                var lowerPropertyName = ExtractPropertyName(() => LowerBound);
                var upperPropertyName = ExtractPropertyName(() => UpperBound);

                _ValidateNumeric(upperPropertyName, value);

                if (GetErrors(upperPropertyName) == null)
                    _ValidateLess(lowerPropertyName, LowerBound, upperPropertyName, value, upperPropertyName);

                if (GetErrors(upperPropertyName) == null)
                {
                    ClearRangeError(lowerPropertyName);
                    Measure.UpperBound = decimal.Parse(value);
                }
            }
        }

        string _LowerBound;
        /// <summary>
        /// Gets or sets the lower bound.
        /// </summary>
        /// <value>
        /// The lower bound.
        /// </value>
        public string LowerBound
        {
            get { return _LowerBound; }
            set
            {
                if (value == _LowerBound) return;
                _LowerBound = value;
                Committed = false;
                RaisePropertyChanged(() => LowerBound);

                var lowerPropertyName = ExtractPropertyName(() => LowerBound);
                var upperPropertyName = ExtractPropertyName(() => UpperBound);

                _ValidateNumeric(lowerPropertyName, value);

                if (GetErrors(lowerPropertyName) == null)
                    _ValidateLess(lowerPropertyName, value, upperPropertyName, UpperBound, lowerPropertyName);

                if (GetErrors(lowerPropertyName) == null)
                {
                    ClearRangeError(upperPropertyName);
                    Measure.LowerBound = decimal.Parse(value);
                }
            }
        }

        // If lower/upper just passed ValidateLess(), then so does the other field, but the other field might
        // still have a range error left over from previous LostFocus. So clear it now too.
        // We MUST set only 1 validation error at a time for upper/lower bound. It can't be non-numeric and range error.
        /// <summary>
        /// Clears the range error.
        /// </summary>
        /// <param name="PropertyName">Name of the property.</param>
        private void ClearRangeError(string PropertyName)
        {
            // This uses foreach because it's IEnumerable, but calls ClearErrors below if a single match is found.
            // Copy the errors collection because we might clear it in the loop.
            var errors = GetErrors(PropertyName) as IEnumerable<string>;
            if (errors == null) return;
            if (errors.Any(err => !err.EqualsIgnoreCase(ValidateLessMessage)))
            {
                ClearErrors(PropertyName);
            }
        }

        string _Numerator;
        /// <summary>
        /// Gets or sets the numerator.
        /// </summary>
        /// <value>
        /// The numerator.
        /// </value>
        public string Numerator
        {
            get { return _Numerator; }
            set
            {
                //if (value == _Numerator) return;
                _Numerator = value;
                Committed = false;
                RaisePropertyChanged(() => Numerator);

                var propertyName = ExtractPropertyName(() => Numerator);
                _ValidateNumeric(propertyName, value);
                if (GetErrors(propertyName) == null)
                    Measure.SuppressionNumerator = decimal.Parse(value);
            }
        }

        string _Denominator;
        /// <summary>
        /// Gets or sets the denominator.
        /// </summary>
        /// <value>
        /// The denominator.
        /// </value>
        public string Denominator
        {
            get { return _Denominator; }
            set
            {
                //if (value == _Denominator) return;
                _Denominator = value;
                Committed = false;
                RaisePropertyChanged(() => Denominator);

                var propertyName = ExtractPropertyName(() => Denominator);
                _ValidateNumeric(propertyName, value);
                if (GetErrors(propertyName) == null)
                    Measure.SuppressionDenominator = decimal.Parse(value);
            }
        }

        bool _PerformMarginSuppression;
        /// <summary>
        /// Gets or sets a value indicating whether [perform margin suppression].
        /// </summary>
        /// <value>
        /// <c>true</c> if [perform margin suppression]; otherwise, <c>false</c>.
        /// </value>
        public bool PerformMarginSuppression
        {
            get { return _PerformMarginSuppression; }
            set
            {
                _PerformMarginSuppression = value;
                Committed = false;
                RaisePropertyChanged(() => PerformMarginSuppression);

                Measure.PerformMarginSuppression = _PerformMarginSuppression;
            }
        }

        #endregion

        #region UI
        /// <summary>
        /// Gets or sets a value indicating whether measure is selected for topic assignment.
        /// </summary>
        /// <value>
        /// <c>true</c> if measure is selected for topic assignment; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelectedForTopicAssignment { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether measure is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if measure is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether measure is changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if measure is changed; otherwise, <c>false</c>.
        /// </value>
        public bool IsChanged { get; set; }
        #endregion

        /// <summary>
        /// Gets or sets the calculated options.
        /// </summary>
        /// <value>
        /// The calculated options.
        /// </value>
        public ObservableCollection<string> CalculatedOptions { get; set; }
        /// <summary>
        /// Gets or sets the assigned calculated option.
        /// </summary>
        /// <value>
        /// The assigned calculated option.
        /// </value>
        public string AssignedCalculatedOption { get; set; }

        /// <summary>
        /// Gets the better high or low.
        /// </summary>
        /// <value>
        /// The better high or low.
        /// </value>
        public string BetterHighOrLow
        {
            get
            {
                return HigherScoresAreBetter ? "Higher is better" : "Lower is better";
            }
        }

        #region Methods

        /// <summary>
        /// Gets the websites.
        /// </summary>
        /// <param name="measureId">The measure identifier.</param>
        /// <returns></returns>
        private ObservableCollection<string> GetWebsites(int measureId)
        {
            var service = ServiceLocator.Current.GetInstance<IMeasureServiceSync>();
            return service.GetWebsitesForMeasure(measureId).ToObservableCollection();
        }

        #endregion

        #region VALIDATION_HELPERS

        string ValidateLessMessage = "Please ensure Lower Bound is less than Upper Bound.";

        /// <summary>
        /// Validates the less.
        /// </summary>
        /// <param name="lowerName">Name of the lower.</param>
        /// <param name="lowerValue">The lower value.</param>
        /// <param name="upperName">Name of the upper.</param>
        /// <param name="upperValue">The upper value.</param>
        /// <param name="propertyName">Name of the property.</param>
        private void _ValidateLess(string lowerName, string lowerValue, string upperName, string upperValue, string propertyName)
        {
            ClearErrors(propertyName);
            if (!string.IsNullOrWhiteSpace(LowerBound) && !string.IsNullOrWhiteSpace(UpperBound))
            {
                if (IsNumeric(lowerValue) && IsNumeric(upperValue))
                {
                    if (double.Parse(lowerValue) > double.Parse(upperValue))
                    {
                        SetError(propertyName, ValidateLessMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Validates the numeric.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="value">The value.</param>
        private void _ValidateNumeric(string p, string value)
        {
            ClearErrors(p);
            if (!IsNumeric(value))
            {
                SetError(p, "Please provide a valid numeric value.");
                return;
            }
        }

        /// <summary>
        /// Determines whether the specified value is numeric.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is numeric; otherwise, <c>false</c>.
        /// </returns>
        bool IsNumeric(string value)
        {
            // valid format is:
            // ^ = start of text
            // - (negative sign optional)
            // 0-9 digits
            // decimal point (optional)
            // 0-4 digits after decimal point
            // $ = end of text
            var regexNumeric = new Regex(@"^(0|(-?(((0|[1-9]\d{0,9})\.\d{0,9})|([1-9]\d{0,9}))))$");
            return !string.IsNullOrEmpty(value) && regexNumeric.IsMatch(value);
        }

        /// <summary>
        /// Use this to validate any late-bound fields before committing
        /// </summary>
        protected override void ValidateAll()
        {
            // This validates all 3 titles so at least 1 is present, doesn't matter which
            _ValidateTitle(ExtractPropertyName(() => ClinicalTitle), ClinicalTitle);

            _ValidateUrl(ExtractPropertyName(() => Url), Url);
            _ValidateUrlTitle(ExtractPropertyName(() => UrlTitle), UrlTitle);
            _ValidateNumeric(ExtractPropertyName(() => ScaleBy), ScaleBy);
            _ValidateNumeric(ExtractPropertyName(() => UpperBound), UpperBound);
            _ValidateNumeric(ExtractPropertyName(() => LowerBound), LowerBound);
            _ValidateNumeric(ExtractPropertyName(() => Numerator), Numerator);
            _ValidateNumeric(ExtractPropertyName(() => Denominator), Denominator);
            _ValidateNumeric(ExtractPropertyName(() => NationalBenchmark), NationalBenchmark);

            // only do numeric validation if benchmark # is provided, not calculated mean
            if (CalculationMethod == StatePeerBenchmarkCalculationMethod.Provided) _ValidateNumeric(ExtractPropertyName(() => ProvidedBenchmark), ProvidedBenchmark);
        }

        #endregion
    }
}
