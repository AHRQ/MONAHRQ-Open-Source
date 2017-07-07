using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Infrastructure.Validation
{
    public class XmlValidator : ValidationRuleEx
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }
        /// <summary>
        /// Gets or sets the XML schema path. This path needs to be a relative path to the Xml Schema file.
        /// </summary>
        /// <value>
        /// The XML schema path.
        /// </value>
        public string XmlSchemaPath { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()) || string.IsNullOrEmpty(XmlSchemaPath)) return new ValidationResult(true, null);

            if (!XmlSchemaPath.StartsWith("\\"))
                XmlSchemaPath = string.Format("\\{0}", XmlSchemaPath);

            var filePath = value.ToString();
            var schemaPath = string.Format("{0}{1}", MonahrqContext.BinFolderPath,XmlSchemaPath);
            return XmlHelper.IsValidXml(filePath, schemaPath) 
                ? new ValidationResult(true, null) 
                : new ValidationResult(false, string.Format("{0} is not a valid Monahrq data mapping file. Please select the correct file and try again.", PropertyName));
        }
    }
}
