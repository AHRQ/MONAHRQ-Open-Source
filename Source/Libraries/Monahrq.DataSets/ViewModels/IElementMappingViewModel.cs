using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using Monahrq.DataSets.Model;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// A hastily extracted interface intended to provide common methods, properties, and commands for field mapping UI
    /// </summary>
    public interface IElementMappingViewModel
    {
        /// <summary>
        /// Gets or sets the remove mapping command.
        /// </summary>
        /// <value>
        /// The remove mapping command.
        /// </value>
        ICommand RemoveMappingCommand { get; set; }

        /// <summary>
        /// Gets or sets the show sample values command.
        /// </summary>
        /// <value>
        /// The show sample values command.
        /// </value>
        ICommand ShowSampleValuesCommand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show mapped values popup].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show mapped values popup]; otherwise, <c>false</c>.
        /// </value>
        bool ShowMappedValuesPopup { get; set; }

        /// <summary>
        /// Gets or sets the selected unmapped field.
        /// </summary>
        /// <value>
        /// The selected unmapped field.
        /// </value>
        MOriginalField SelectedUnmappedField { get; set; }

        /// <summary>
        /// Gets or sets the source fields.
        /// </summary>
        /// <value>
        /// The source fields.
        /// </value>
        ListCollectionView SourceFields { get; set; }

        /// <summary>
        /// Gets or sets the selected mapped field.
        /// </summary>
        /// <value>
        /// The selected mapped field.
        /// </value>
        MTargetField SelectedMappedField { get; set; }

        /// <summary>
        /// Gets or sets the selected target field.
        /// </summary>
        /// <value>
        /// The selected target field.
        /// </value>
        MTargetField SelectedTargetField { get; set; }

        /// <summary>
        /// Gets or sets the selected original field.
        /// </summary>
        /// <value>
        /// The selected original field.
        /// </value>
        MOriginalField SelectedOriginalField { get; set; }

        /// <summary>
        /// Gets or sets the mapped fields count.
        /// </summary>
        /// <value>
        /// The mapped fields count.
        /// </value>
        int MappedFieldsCount { get; set; }

        /// <summary>
        /// Gets or sets the mapped fields for values.
        /// </summary>
        /// <value>
        /// The mapped fields for values.
        /// </value>
        ObservableCollection<MOriginalField> MappedFieldsForValues { get; set; }

        /// <summary>
        /// Gets the required field count.
        /// </summary>
        /// <value>
        /// The required field count.
        /// </value>
        int RequiredFieldCount { get; }

        /// <summary>
        /// Gets the optional field count.
        /// </summary>
        /// <value>
        /// The optional field count.
        /// </value>
        int OptionalFieldCount { get; }

        /// <summary>
        /// Gets or sets the total mapped fields count.
        /// </summary>
        /// <value>
        /// The total mapped fields count.
        /// </value>
        int TotalMappedFieldsCount { get; set; }

        /// <summary>
        /// Gets or sets the total source fields.
        /// </summary>
        /// <value>
        /// The total source fields.
        /// </value>
        int TotalSourceFields { get; set; }

        /// <summary>
        /// Executes the remove mapping.
        /// </summary>
        /// <param name="mappedField">The mapped field.</param>
        void ExecuteRemoveMapping(MOriginalField mappedField);
    }
}