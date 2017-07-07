using System;
using System.ComponentModel.Composition;

namespace Monahrq.Sdk.ViewModels
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class ExportAsViewModelAttribute : ExportAttribute
    {
        /// <summary>
        ///     Use the type as the tag (full name of the type)
        /// </summary>
        /// <param name="viewModelType">The view model type</param>
        public ExportAsViewModelAttribute(Type viewModelType)
            : this(viewModelType.FullName)
        {
        }

        /// <summary>
        /// Use a user-specified tag name 
        /// </summary>
        public ExportAsViewModelAttribute(string viewModelType)
            : base(typeof(IViewModel))
        {
            ViewModelType = viewModelType;
        }

        /// <summary>
        /// The tag for the view model
        /// </summary>
        public string ViewModelType { get; private set; }
    }
}
