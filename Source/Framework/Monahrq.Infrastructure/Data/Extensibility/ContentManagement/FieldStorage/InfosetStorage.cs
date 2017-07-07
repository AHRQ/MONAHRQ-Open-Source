using System.Xml.Linq;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.FieldStorage
{
    /// <summary>
    /// 
    /// </summary>
    public class Infoset
    {
        /// <summary>
        /// The element
        /// </summary>
        private XElement _element;

        /// <summary>
        /// Sets the element.
        /// </summary>
        /// <param name="value">The value.</param>
        private void SetElement(XElement value)
        {
            _element = value;
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        public XElement Element
        {
            get
            {
                return _element ?? (_element = new XElement("Data"));
            }
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public string Data
        {
            get
            {
                return _element == null ? null : Element.ToString(SaveOptions.DisableFormatting);
            }
            set
            {
                SetElement(string.IsNullOrEmpty(value) ? null : XElement.Parse(value, LoadOptions.PreserveWhitespace));
            }
        }
    }
}
