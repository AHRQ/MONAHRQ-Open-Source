using System.Windows;

namespace Monahrq.Behaviors
{
    /// <summary>
    /// Class for SplashBehaviour.
    /// </summary>
    public class SplashBehaviour
    {
        #region Dependency Properties
        /// <summary>
        /// The enabled property
        /// </summary>
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
          "Enabled", typeof(bool), typeof(SplashBehaviour), new PropertyMetadata(OnEnabledChanged));

        /// <summary>
        /// Gets the where splash behaviour enabled.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        /// <summary>
        /// Sets the enabled property for the splash behaviour.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Called when enabled property is changed.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="args">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var splash = obj as Window;
            if (splash != null && args.NewValue is bool && (bool)args.NewValue)
            {
                splash.Closed += (s, e) =>
                {
                    splash.DataContext = null;
                    splash.Dispatcher.InvokeShutdown();
                };
                splash.MouseDoubleClick += (s, e) => splash.Close();
                splash.MouseLeftButtonDown += (s, e) => splash.DragMove();
            }
        }
        #endregion
    }
}
