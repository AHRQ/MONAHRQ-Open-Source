using PropertyChanged;
using System;

namespace Monahrq.Sdk.Types
{
    [ImplementPropertyChanged]
    public class SelectListItem : IComparable<SelectListItem>
    {
        public event EventHandler ValueChanged;
        private void RaiseValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, new EventArgs());
            }
        }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; set; }
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text { get; set; }
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public object Model { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is selected].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is selected]; otherwise, <c>false</c>.
        /// </value>
        /// 
        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                RaiseValueChanged();
            }
        }

        public int CompareTo(SelectListItem obj)
        {
            if (Value == obj.Value || (obj.Value == null || Value == null))
            {
                return string.Compare(Text, obj.Text, StringComparison.InvariantCultureIgnoreCase);
            }
            return string.Compare(Value.ToString(), obj.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}