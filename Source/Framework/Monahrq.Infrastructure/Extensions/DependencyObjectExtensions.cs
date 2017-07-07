using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.ComponentModel;

namespace Monahrq.Infrastructure.Extensions
{
    /// <summary>
    /// A set of useful extensions for the DependencyObject class.
    /// </summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// Gets the first parent found of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of parent to find.</typeparam>
        /// <param name="child">The child.</param>
        /// <returns>The first parent found of type 'T' or null if no parent of type 'T' is found.</returns>
        public static T GetParent<T>(this DependencyObject child) where T : DependencyObject
        {
            //  Get the visual parent.
            DependencyObject dependencyObject = VisualTreeHelper.GetParent(child);

            //  If we've got the parent, return it if it is the correct type - otherwise
            //  continue up the tree.
            if (dependencyObject != null)
                return dependencyObject is T ? dependencyObject as T : GetParent<T>(dependencyObject);
            else
                return null;
        }

        /// <summary>
        /// Gets the top level parent.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <returns></returns>
        public static DependencyObject GetTopLevelParent(this DependencyObject child)
        {
            DependencyObject tmp = child;
            DependencyObject parent = null;
            while ((tmp = VisualTreeHelper.GetParent(tmp)) != null)
            {
                parent = tmp;
            }
            return parent;
        }

        /// <summary>
        /// Gets all children of a specified type, through the visual tree.
        /// This function recurses.
        /// </summary>
        /// <typeparam name="T">The type of child to get.</typeparam>
        /// <param name="me">The dependency object to get children of.</param>
        /// <returns>All children of type T of the dependency object.</returns>
        public static IEnumerable<T> GetVisualChildren<T>(this DependencyObject me) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(me); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(me, i);
                if (child != null && child is T)
                    yield return (T)child;

                foreach (T childOfChild in child.GetVisualChildren<T>())
                    yield return childOfChild;
            }
        }

        /// <summary>
        /// Gets all children of a specified type, through the logical tree.
        /// This function recurses.
        /// </summary>
        /// <typeparam name="T">The type of child to get.</typeparam>
        /// <param name="me">The dependency object to get children of.</param>
        /// <returns>All children of type T of the dependency object.</returns>
        public static IEnumerable<T> GetLogicalChildren<T>(this DependencyObject me) where T : DependencyObject
        {
            foreach (var child in LogicalTreeHelper.GetChildren(me))
            {
                //  If the child is not a dependency object, we can't use it.
                var childDependencyObject = child as DependencyObject;
                if (childDependencyObject == null)
                    continue;

                if (childDependencyObject != null && childDependencyObject is T)
                    yield return (T)childDependencyObject;

                foreach (T childOfChild in childDependencyObject.GetLogicalChildren<T>())
                    yield return childOfChild;
            }
        }

        /// <summary>
        /// Finds a child element of a specified type with a specified name.
        /// </summary>
        /// <typeparam name="T">The type of child element to find.</typeparam>
        /// <param name="me">The dependency object.</param>
        /// <param name="childName">Name of the child.</param>
        /// <returns>The first child of type T with the specified name, or null of no
        /// children are found.</returns>
        public static T FindChild<T>(this DependencyObject me, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (me == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(me);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(me, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Gets the binding objects.
        /// </summary>
        /// <param name="me">The dependency object.</param>
        /// <returns>All bindings for the dependency object.</returns>
        public static IEnumerable<Binding> GetBindingObjects(this DependencyObject me)
        {
            //  Get the dependency properties.
            var dpList = GetDependencyProperties(me);

            //  Select all of the non-null bindings.
            return dpList.Select(dp => BindingOperations.GetBinding(me, dp)).Where(b => b != null);
        }

        /// <summary>
        /// Gets the dependency properties of a dependency object.
        /// </summary>
        /// <param name="me">The dependency object.</param>
        /// <returns>The dependency properties of a dependency object.</returns>
        public static IEnumerable<DependencyProperty> GetDependencyProperties(this DependencyObject me)
        {
            //  Get appropriate properties.
            var properties = TypeDescriptor.GetProperties(me,
                                                          new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) });

            //  Return all non null dependency properties.
            return from PropertyDescriptor pd in properties
                   select DependencyPropertyDescriptor.FromProperty(pd) into dpd
                   where dpd != null
                   select dpd.DependencyProperty;
        }
    }
}