using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Theme.Controls.Wizard.Models.Data;
using PropertyChanged;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The dataset targeted field.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.Data.ModelBase" />
    [ImplementPropertyChanged]
    public class MTargetField : ModelBase
    {
        /// <summary>
        /// Occurs when [mapping changed].
        /// </summary>
        public event EventHandler<ExtendedEventArgs<MTargetField>> MappingChanged = delegate { };

        /// <summary>
        /// Raises the <see cref="E:MappingChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnMappingChanged(EventArgs e)
        {
            MappingChanged(this, new ExtendedEventArgs<MTargetField>(this));
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
        /// Initializes a new instance of the <see cref="MTargetField"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="originalFields">The original fields.</param>
        public MTargetField(Element element, ObservableCollection<MOriginalField> originalFields)
        {
            IsMapped = false;
            Name  = element.Name;
            Caption = element.Description;
            Description = element.LongDescription; 
            FieldVisibility = Visibility.Visible;
            OriginalFields = originalFields;
            MappingHints = element.MappingHints.ToList();
            Element = element;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        public string Caption
        {
            get;set;
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public int Position
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        public Element Element
        {
            get;
            private set;
        }

        /// <summary>
        /// The mapped field
        /// </summary>
        private MOriginalField _mappedField;
        /// <summary>
        /// Gets or sets the mapped field.
        /// </summary>
        /// <value>
        /// The mapped field.
        /// </value>
        public MOriginalField MappedField
        {
            get { return _mappedField; }
            set
            {
                _mappedField = value;

                if (value == null || MappedField.Name == "UNMAPPED")
                {
                    IsMapped = false;
                }
                else
                {
                    IsMapped = true;
                }

                OnMappingChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the original fields.
        /// </summary>
        /// <value>
        /// The original fields.
        /// </value>
        public ObservableCollection<MOriginalField> OriginalFields
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is mapped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is mapped; otherwise, <c>false</c>.
        /// </value>
        public bool IsMapped
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired
        {
            get;
            set;
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

        /// <summary>
        /// Gets the mapping hints.
        /// </summary>
        /// <value>
        /// The mapping hints.
        /// </value>
        public List<string> MappingHints { get; private set; }

        /// <summary>
        /// Gets or sets the current dragged over item.
        /// </summary>
        /// <value>
        /// The current dragged over item.
        /// </value>
        public object CurrentDraggedOverItem { get; set; }

        /// <summary>
        /// Gets or sets the index of the drop.
        /// </summary>
        /// <value>
        /// The index of the drop.
        /// </value>
        public int DropIndex { get; set; }
        
        /// <summary>
        /// Gets or sets the current dragged item.
        /// </summary>
        /// <value>
        /// The current dragged item.
        /// </value>
        public object CurrentDraggedItem { get; set; }

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
    }
}
