using System;
namespace Monahrq.DataSets.ViewModels.Crosswalk
{
    /// <summary>
    /// The mapped field entry view model interface.
    /// </summary>
    public interface IMappedFieldEntryViewModel
    {
        /// <summary>
        /// Gets the count mapped crosswalks.
        /// </summary>
        /// <value>
        /// The count mapped crosswalks.
        /// </value>
        int CountMappedCrosswalks { get; }
        /// <summary>
        /// Gets the count total crosswalks.
        /// </summary>
        /// <value>
        /// The count total crosswalks.
        /// </value>
        Lazy<int> CountTotalCrosswalks { get; }
        /// <summary>
        /// Gets the count unmapped crosswalks.
        /// </summary>
        /// <value>
        /// The count unmapped crosswalks.
        /// </value>
        int CountUnmappedCrosswalks { get; }
        /// <summary>
        /// Gets the crosswalk models.
        /// </summary>
        /// <value>
        /// The crosswalk models.
        /// </value>
        System.Collections.Generic.IEnumerable<ICrosswalkViewModel> CrosswalkModels { get; }
        /// <summary>
        /// Gets the crosswalks.
        /// </summary>
        /// <value>
        /// The crosswalks.
        /// </value>
        System.Windows.Data.ListCollectionView Crosswalks { get; }
        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        string DisplayName { get; }
        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        Infrastructure.Entities.Domain.Wings.Element Element { get; }
        /// <summary>
        /// Gets or sets the element scope values.
        /// </summary>
        /// <value>
        /// The element scope values.
        /// </value>
        System.Windows.Data.ListCollectionView ElementScopeValues { get; set; }
        /// <summary>
        /// Gets the field entry.
        /// </summary>
        /// <value>
        /// The field entry.
        /// </value>
        Model.FieldEntry FieldEntry { get; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is changed; otherwise, <c>false</c>.
        /// </value>
        bool IsChanged { get; set; }
        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        bool IsValid { get; }
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        System.Windows.Data.ListCollectionView Scope { get; }
        /// <summary>
        /// Gets the total entries.
        /// </summary>
        /// <value>
        /// The total entries.
        /// </value>
        int TotalEntries { get; }
        /// <summary>
        /// Gets the valid entries.
        /// </summary>
        /// <value>
        /// The valid entries.
        /// </value>
        int ValidEntries { get; }
    }
}
