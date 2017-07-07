using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Monahrq.Infrastructure.Domain.Wings
{
    [Serializable,
     DebuggerStepThrough]
    public class DynamicTargetStep
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [XmlAttribute]
        public DynamicStepTypeEnum Type { get; set; }
    }

    [Serializable,Flags]
    public enum DynamicStepTypeEnum
    {
        /// <summary>
        /// The simple
        /// </summary>
        Simple,
        /// <summary>
        /// The full
        /// </summary>
        Full,
    }
}