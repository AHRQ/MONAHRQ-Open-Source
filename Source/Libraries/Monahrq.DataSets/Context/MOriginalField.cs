using System;
using System.Collections.ObjectModel;
using System.Windows;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Theme.Controls.Wizard.Models.Data;
using PropertyChanged;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The original mapping field/
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.Data.ModelBase" />
    [ImplementPropertyChanged]
    public class MOriginalField : ModelBase
    {
        private MTargetField _targetField;
        private bool _isMapped;
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="MOriginalField"/> class.
        /// </summary>
        public MOriginalField()
        {
            Values = new ObservableCollection<string>();
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName
        {
            get
            {
                if(TargetField != null && TargetField.IsRequired)
                    return string.Format("{0} *", Name);

                return Name;
            }
            //set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the field visibility.
        /// </summary>
        /// <value>
        /// The field visibility.
        /// </value>
        public Visibility FieldVisibility
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public ObservableCollection<string> Values
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is automatic mapped.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is automatic mapped; otherwise, <c>false</c>.
        /// </value>
        public bool IsAutoMapped
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the field order.
        /// </summary>
        /// <value>
        /// The field order.
        /// </value>
        public int FieldOrder
        {
            get; set;
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        /// <returns></returns>
        public override bool Cancel()
        {
            return true;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public override void Dispose()
        {
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <returns></returns>
        public override string GetName()
        {
            return Name;
        }

        /// <summary>
        /// Gets the dataset item.
        /// </summary>
        /// <returns></returns>
        public override Dataset GetDatasetItem()
        {
            return null;
        }

        /// <summary>
        /// Refreshes the target.
        /// </summary>
        /// <param name="targetToRefesh">The target to refesh.</param>
        /// <returns></returns>
        public override Target RefreshTarget(Target targetToRefesh)
        {
            return null;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is custom.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is custom; otherwise, <c>false</c>.
        /// </value>
        public override bool IsCustom
        {
            get { return false; }
        }

        #region FOR_CODED_UI_TESTS 
        // This is to avoid coded UI test error: "Last action on list item was not recorded because the control does not have any good identification property"
        // see: http://blogs.msdn.com/b/gautamg/archive/2010/03/10/how-to-get-automation-working-properly-on-data-bound-wpf-list-or-combo-box.aspx
        // This function must return an id for each row
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether this instance is mapped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is mapped; otherwise, <c>false</c>.
        /// </value>
        public bool IsMapped
        {
            get { return _isMapped; }
            set
            {
                _isMapped = value;

                //if (value == false && TargetField != null)
                //{
                //    OnMappingChanged(EventArgs.Empty);
                //}
            }
        }

        /// <summary>
        /// Gets or sets the target field.
        /// </summary>
        /// <value>
        /// The target field.
        /// </value>
        public MTargetField TargetField
        {
            get { return _targetField; }
            set
            {
                _targetField = value;

                if (value == null || _targetField.Name == "UNMAPPED")
                {
                    IsMapped = false;
                }
                else
                {
                    IsMapped = true;
                }

                //OnMappingChanged(EventArgs.Empty);
                _targetField = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [remove mapping].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remove mapping]; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveMapping { get; set; }

        /// <summary>
        /// Occurs when [mapping changed].
        /// </summary>
        public event EventHandler<ExtendedEventArgs<MOriginalField>> MappingChanged = delegate { };

        /// <summary>
        /// Raises the <see cref="E:MappingChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public virtual void OnMappingChanged(EventArgs e)
        {
            MappingChanged(this, new ExtendedEventArgs<MOriginalField>(this));
        }
    }

    /// <summary>
    /// The mapping changed sutstom event arguements.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class OnMappingChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnMappingChangedEventArgs"/> class.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="unMap">if set to <c>true</c> [un map].</param>
        public OnMappingChangedEventArgs(MOriginalField field, bool unMap)
        {
            Field = field;
            UnMap = unMap;
        }

        /// <summary>
        /// Gets the field.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        public MOriginalField Field { get; private set; }
        /// <summary>
        /// Gets a value indicating whether [un map].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [un map]; otherwise, <c>false</c>.
        /// </value>
        public bool UnMap { get; private set; }
    }
}
