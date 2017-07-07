using Monahrq.DataSets.Model;
using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.Theme.Controls.Wizard.Models;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// The default dataset import wizard data crosswalk view model.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.DataSets.Model.DatasetContext}" />
    public class DefaultWizardCrosswalkViewModel : WizardStepViewModelBase<DatasetContext>
    {
        /// <summary>
        /// Gets or sets the data set date.
        /// </summary>
        /// <value>
        /// The data set date.
        /// </value>
        public string DataSetDate { get; set; }
        /// <summary>
        /// Gets or sets the name of the data set.
        /// </summary>
        /// <value>
        /// The name of the data set.
        /// </value>
        public string DataSetName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardCrosswalkViewModel"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        public DefaultWizardCrosswalkViewModel(DatasetContext c)
            : base(c)
        {
            MappedValues = new HybridDictionary();
        }

        /// <summary>
        /// Gets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        public string Caption
        {
            get
            {
                return "Map each value to a meaning:";
            }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get
            {
                return "The value of the following variables have specific meanings. Choose the description that indicates the meaning of each value in your input file.";
            }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get
            {
                return "Map Values";
            }
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return MappedFieldModels.SelectMany(model => model.FieldEntry.Bin).All(item => item.ScopeValue == null);
        }

        /// <summary>
        /// Gets the mapped field models.
        /// </summary>
        /// <value>
        /// The mapped field models.
        /// </value>
        IEnumerable<MappedFieldEntryViewModel> MappedFieldModels
        {
            get
            {
                return MappedFieldEntries.OfType<MappedFieldEntryViewModel>();
            }
        }

        /// <summary>
        /// Gets or sets the mapped field entries.
        /// </summary>
        /// <value>
        /// The mapped field entries.
        /// </value>
        ListCollectionView MappedFieldEntries
        {
            get;
            set;
        }

        // private HybridDictionary _mappedValues = new HybridDictionary();
        /// <summary>
        /// Gets or sets the mapped values.
        /// </summary>
        /// <value>
        /// The mapped values.
        /// </value>
        public HybridDictionary MappedValues
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            var typeProps = DataContextObject.TargetProperties;
            var histogram = DataContextObject.Histogram;
            var mappedFields = DataContextObject.RequiredMappings.Concat(DataContextObject.OptionalMappings)
                                                                 .Select(kvp =>
                                                                    {
                                                                        if (string.IsNullOrEmpty(kvp.Key) || string.IsNullOrEmpty(kvp.Value))
                                                                            return null;

                                                                        var element = DataContextObject.TargetElements.FirstOrDefault(elem => elem.Scope != null && 
                                                                                                                                              elem.Name == kvp.Key);
                                                                        var field = histogram[kvp.Value];

                                                                        MappedValues.Add(element.Name.ToUpper(), field);

                                                                        if (element == null || field == null)
                                                                            return null;

                                                                        return new MappedFieldEntryViewModel(typeProps[element.Name.ToUpper()], element, field);
                                                                    })
                                                                 .Where(item => item != null)
                                                                 .ToList();

            MappedFieldEntries = (ListCollectionView)CollectionViewSource.GetDefaultView(new ObservableCollection<MappedFieldEntryViewModel>(mappedFields));
        }
    }
}
