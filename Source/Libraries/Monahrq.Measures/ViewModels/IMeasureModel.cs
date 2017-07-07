using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// Interface for measures model
    /// </summary>
    public interface IMeasureModel
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <returns></returns>
        string GetDescription();
        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected for topic assignment.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected for topic assignment; otherwise, <c>false</c>.
        /// </value>
        bool IsSelectedForTopicAssignment { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        bool IsVisible { get; set; }
        /// <summary>
        /// Gets or sets the edit command.
        /// </summary>
        /// <value>
        /// The edit command.
        /// </value>
        ICommand EditCommand { get; set; }
        /// <summary>
        /// Gets or sets the assign topic command.
        /// </summary>
        /// <value>
        /// The assign topic command.
        /// </value>
        ICommand AssignTopicCommand { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets the collection of websites.
        /// </summary>
        /// <value>
        /// The websites.
        /// </value>
        ObservableCollection<string> Websites { get; set; }
        /// <summary>
        /// Gets or sets the collection of topics.
        /// </summary>
        /// <value>
        /// The topics.
        /// </value>
        ObservableCollection<TopicViewModel> Topics { get; set; }
        /// <summary>
        /// Gets or sets the name of the data set.
        /// </summary>
        /// <value>
        /// The name of the data set.
        /// </value>
        string DataSetName { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; set; }
        /// <summary>
        /// Gets or sets the collection of alternate names.
        /// </summary>
        /// <value>
        /// The alternate names.
        /// </value>
        ObservableCollection<AlternateName> AlternateNames { get; set; }
        /// <summary>
        /// Gets or sets the selected alternate name.
        /// </summary>
        /// <value>
        /// The name of the selected alternate.
        /// </value>
        AlternateName SelectedAlternateName { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is higher score better.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is higher score better; otherwise, <c>false</c>.
        /// </value>
        bool IsHigherScoreBetter { get; set; }
        /// <summary>
        /// Gets or sets the URL title.
        /// </summary>
        /// <value>
        /// The URL title.
        /// </value>
        string UrlTitle { get; set; }
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        string Url { get; set; }
        /// <summary>
        /// Gets or sets the footnote.
        /// </summary>
        /// <value>
        /// The footnote.
        /// </value>
        string Footnote { get; set; }
        /// <summary>
        /// Gets or sets the scale by.
        /// </summary>
        /// <value>
        /// The scale by.
        /// </value>
        int ScaleBy { get; set; }
        /// <summary>
        /// Gets or sets the national benchamrk.
        /// </summary>
        /// <value>
        /// The national benchamrk.
        /// </value>
        int NationalBenchamrk { get; set; }
        /// <summary>
        /// Gets or sets the upper bounds.
        /// </summary>
        /// <value>
        /// The upper bounds.
        /// </value>
        int UpperBounds { get; set; }
        /// <summary>
        /// Gets or sets the lower bounds.
        /// </summary>
        /// <value>
        /// The lower bounds.
        /// </value>
        int LowerBounds { get; set; }
        /// <summary>
        /// Gets or sets the collection of calculated options.
        /// </summary>
        /// <value>
        /// The calculated options.
        /// </value>
        ObservableCollection<string> CalculatedOptions { get; set; }
        /// <summary>
        /// Gets or sets the assigned calculated option.
        /// </summary>
        /// <value>
        /// The assigned calculated option.
        /// </value>
        string AssignedCalculatedOption { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is calculated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is calculated; otherwise, <c>false</c>.
        /// </value>
        bool IsCalculated { get; set; }
        /// <summary>
        /// Gets or sets the provided value.
        /// </summary>
        /// <value>
        /// The provided value.
        /// </value>
        int ProvidedValue { get; set; }
        /// <summary>
        /// Gets or sets the numerator.
        /// </summary>
        /// <value>
        /// The numerator.
        /// </value>
        int Numerator { get; set; }
        /// <summary>
        /// Gets or sets the denumerator.
        /// </summary>
        /// <value>
        /// The denumerator.
        /// </value>
        int Denumerator { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to perform margin suppression or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if [perform margin suppression]; otherwise, <c>false</c>.
        /// </value>
        bool PerformMarginSuppression { get; set; }
        /// <summary>
        /// Gets or sets the clinical description.
        /// </summary>
        /// <value>
        /// The clinical description.
        /// </value>
        string ClinicalDescription { get; set; }
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        string Source { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether NFQ is endorsed or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [NFQ endorsed]; otherwise, <c>false</c>.
        /// </value>
        bool NFQEndorsed { get; set; }
        /// <summary>
        /// Gets or sets the nfqid.
        /// </summary>
        /// <value>
        /// The nfqid.
        /// </value>
        string NFQID { get; set; }
        /// <summary>
        /// Gets the better high or low.
        /// </summary>
        /// <value>
        /// The better high or low.
        /// </value>
        string BetterHighOrLow { get; }
    }
}