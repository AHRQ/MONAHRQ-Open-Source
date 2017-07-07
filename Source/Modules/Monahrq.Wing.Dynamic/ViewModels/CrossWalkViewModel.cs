using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.ViewModels.Crosswalk;
using PropertyChanged;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="Monahrq.DataSets.ViewModels.Crosswalk.ICrosswalkViewModel" />
    [ImplementPropertyChanged]
    public class CrosswalkViewModel
        : INotifyPropertyChanged, ICrosswalkViewModel
    {
        /// <summary>
        /// Gets the crosswalk.
        /// </summary>
        /// <value>
        /// The crosswalk.
        /// </value>
        public CrosswalkScope Crosswalk { get; private set; }

        /// <summary>
        /// Gets or sets the candidate scopes.
        /// </summary>
        /// <value>
        /// The candidate scopes.
        /// </value>
        public ListCollectionView CandidateScopes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrosswalkViewModel"/> class.
        /// </summary>
        /// <param name="crossWalk">The cross walk.</param>
        public CrosswalkViewModel(CrosswalkScope crossWalk)
        {

            Crosswalk = crossWalk;
            var dispPropChange = crossWalk as INotifyPropertyChanged;
            if (dispPropChange != null)
            {
                dispPropChange.PropertyChanged += (o, e) => OnPropertyChanged();
                OnPropertyChanged();
            }
        }



        /// <summary>
        /// Returns true if cross walk is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
#if DEBUG
                Debug.WriteLine("Calculating CrosswalkViewModel Validity - Source Column: {0} -> Value: {1}",
                   Crosswalk.Bin.Column.ColumnName, Crosswalk.SourceValue);
#endif
                var isValid = Crosswalk.ScopeValue != null;
#if DEBUG
                Debug.WriteLine("Source Column: {0} -> Value: {1} [ {2} ]",
                        Crosswalk.Bin.Column.ColumnName, Crosswalk.SourceValue,
                        isValid ? "Valid" : "Invalid");
#endif
                return isValid;
            }

        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is changed; otherwise, <c>false</c>.
        /// </value>
        public bool IsChanged
        {
            get;
            set;
        }

        /// <summary>
        /// Event which Occurs when  property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged(this, e);
        }

        /// <summary>
        /// Called when property is changed.
        /// </summary>
        /// <param name="prop">The property.</param>
        protected virtual void OnPropertyChanged(string prop)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Called when property is changed.
        /// </summary>
        protected virtual void OnPropertyChanged()
        {
            OnPropertyChanged(string.Empty);
        }
    }
}
