using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    ///     Monahrq View Converter converter (Reference: Jounce Project/Wintellect)
    /// </summary>
    /// <remarks>
    /// Use this is generate an instance of a view bound to a view model - if you have a list of 
    /// view models, this will generate non-shared views that are bound in lists
    /// </remarks>
   public class MonahrqViewConverter : IValueConverter 
    {
     /// <summary>
        ///     Single instance for composition 
        /// </summary>
        private static readonly MonahrqViewConverter _composedConverter = new MonahrqViewConverter();

        /// <summary>
        /// The reference to the <see cref="IViewModelRouter"/>
        /// </summary>
        [Import]
        public IViewModelRouter Router { get; set; }
        
        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        /// <param name="value">The source data being passed to the target.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //todo:INGA  Port Dependency Property for In Design : can I inject DP? hmmm

            //if (DesignerProperties.IsInDesignTool)
            //{
            //    return value;
            //}

            lock (_composedConverter)
            {
                if (_composedConverter.Router == null)
                {
                    //todo: Use Castle to to staisfy imports with Composition Container
                    //todo: Delegate Composition to Bootsratper
                    //CompositionInitializer.SatisfyImports(_composedConverter);
                }
            }

            if (_composedConverter.Router == null)
            {
                return value;
            }

            // if the value is a view model and a view tag was passed, resolve them
            if (value is IViewModel)
            {
                // if tag is passed in
                if (parameter != null)
                {
                    //todo: add parameter type to use the TAG
                    return ServiceLocator.Current.GetAllInstances<IViewModel>();
                }

                // first get the view model tag 
                var exportAttribute =
                    (from c in value.GetType().GetCustomAttributes(true) where c is ExportAsViewModelAttribute select (ExportAsViewModelAttribute)c)
                        .FirstOrDefault();

                if (exportAttribute == null)
                {
                    return value;
                }

                var viewModelTag = exportAttribute.ViewModelType;
                var views = _composedConverter.Router.GetViewTagsForViewModel(viewModelTag);

                // we always assume the first view
                if (views.Length > 0)
                {
                    return _composedConverter.Router.GetNonSharedView(views[0], value);
                }
            }

            return value;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        /// <param name="value">The target data being passed to the source.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
