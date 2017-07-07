using System.Collections.ObjectModel;
using Monahrq.DataSets.ViewModels;
using Monahrq.Sdk.Model;
using PropertyChanged;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The data type model used in the manage datasets view/screen.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Model.BaseNotifyBase" />
    [ImplementPropertyChanged]
    public class DataTypeModel : BaseNotifyBase
    {
        /// <summary>
        /// The records list
        /// </summary>
        private ObservableCollection<DataTypeDetailsViewModel> _recordsList;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeModel"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public DataTypeModel(Target target)
        {
            Target = target;
            RecordsList=new ObservableCollection<DataTypeDetailsViewModel>();
        }

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public Target Target { get; private set; }
        /// <summary>
        /// Gets the name of the data type.
        /// </summary>
        /// <value>
        /// The name of the data type.
        /// </value>
        public string DataTypeName { get { return Target.DisplayName; } }
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get { return Target.Description; } }
        /// <summary>
        /// Gets the display order.
        /// </summary>
        /// <value>
        /// The display order.
        /// </value>
        public int DisplayOrder { get { return Target.DisplayOrder; } }

        /// <summary>
        /// Gets or sets the records list.
        /// </summary>
        /// <value>
        /// The records list.
        /// </value>
        public ObservableCollection<DataTypeDetailsViewModel> RecordsList
        {
            get { return _recordsList; }
            set
            {
                _recordsList = value;
                RaisePropertyChanged(() => RecordsList);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return DataTypeName + " (" + RecordsList.Count + ")";
        }
    }
}
