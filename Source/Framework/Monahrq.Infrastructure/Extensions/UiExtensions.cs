using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Monahrq.Sdk.Extensions
{
    public static class UiExtensions
    {

        private static Action EmptyDelegate = delegate () { };
        public static void Refresh(this UIElement uiElement)
        {
            // Blair changed this from BeginInvoke to Invoke on 11/29/2013
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        public static void DoEvents(this Application application)
        {
            if (application == null) return;

            // I don't know why Blair changed this from BeginInvoke to Invoke on 11/29/2013. It could be important, but it crashes Hospital Compare import.
            // I'm changing it back to BeginInvoke to fix HC, but I'm not changing Invoke to BeginInvoke in Refresh method above.
            application.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                          new Action(delegate { }));
        }

        public static void DoEventsUI(this Application application)
        {
            System.Windows.Forms.Application.DoEvents();
        }

        public static bool IsDesignMode(this DependencyObject dependencyObject)
        {
            return DesignerProperties.GetIsInDesignMode(dependencyObject);
        }

        public static DependencyObject GetParent(UIElement element, Type parentType)
        {
            if (element == null || parentType == null) return null;

            var parent = VisualTreeHelper.GetParent(element);

            while (parent != null && parent.GetType() != parentType)
            {
                parent = VisualTreeHelper.GetParent(element);
            }
            return parent;
        }

        public static DependencyObject GetChild(UIElement element, Type childType)
        {
            if (element == null || childType == null) return null;

            var child = VisualTreeHelper.GetChild(element, 0);

            while (child != null && child.GetType() != childType)
            {
                child = VisualTreeHelper.GetChild(element, 0);
            }
            return child;
        }
    }
}
