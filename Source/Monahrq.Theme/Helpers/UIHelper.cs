using System.ComponentModel;
using System.Windows;

namespace Monahrq.Theme.Helpers
{
    /// <summary>
    /// Provides various common helper methods.
    /// </summary>
    public static class UIHelper
    {
        private static bool? _isInDesignMode;

        /// <summary>
        /// Determines whether the current code is executed in a design time environment such as Visual Studio or Blend.
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    _isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
                }
                return _isInDesignMode.Value;
            }
        }
    }
}
