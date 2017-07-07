namespace Monahrq.DataSets.ViewModels.Crosswalk
{
    /// <summary>
    /// The dataset crosswalk viewl model interface.
    /// </summary>
    public interface ICrosswalkViewModel
    {
        /// <summary>
        /// Gets or sets the candidate scopes.
        /// </summary>
        /// <value>
        /// The candidate scopes.
        /// </value>
        System.Windows.Data.ListCollectionView CandidateScopes { get; set; }
        /// <summary>
        /// Gets the crosswalk.
        /// </summary>
        /// <value>
        /// The crosswalk.
        /// </value>
        Model.CrosswalkScope Crosswalk { get; }
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
    }
}
