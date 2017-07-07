using System.ComponentModel;
using System.Windows;
using Microsoft.Practices.Prism.Regions;
using System;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Declares the Attached Properties and Behaviors for implementing Popup regions.
	/// </summary>
	/// <remarks>
	/// Although the fastest way is to create a RegionAdapter for a Window and register it with the RegionAdapterMappings,
	/// this would be conceptually incorrect because we want to create a new popup window everytime a view is added
	/// (instead of having a Window as a host control and replacing its contents everytime Views are added, as other adapters do).
	/// This is why we have a different class for this behavior, instead of reusing the <see cref="RegionManager.RegionNameProperty" /> attached property.
	/// </remarks>
	public static class RegionPopupBehaviors
    {
		/// <summary>
		/// The name of the Popup <see cref="IRegion" />.
		/// </summary>
		public static readonly DependencyProperty CreatePopupRegionWithNameProperty =
            DependencyProperty.RegisterAttached("CreatePopupRegionWithName", typeof(string), typeof(RegionPopupBehaviors), new PropertyMetadata(CreatePopupRegionWithNamePropertyChanged));

		/// <summary>
		/// The <see cref="Style" /> to set to the Popup.
		/// </summary>
		public static readonly DependencyProperty ContainerWindowStyleProperty =
          DependencyProperty.RegisterAttached("ContainerWindowStyle", typeof(Style), typeof(RegionPopupBehaviors), null);

		/// <summary>
		/// Gets the name of the Popup <see cref="IRegion" />.
		/// </summary>
		/// <param name="owner">Owner of the Popup.</param>
		/// <returns>
		/// The name of the Popup <see cref="IRegion" />.
		/// </returns>
		/// <exception cref="ArgumentNullException">owner</exception>
		public static string GetCreatePopupRegionWithName(DependencyObject owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            return owner.GetValue(CreatePopupRegionWithNameProperty) as string;
        }

		/// <summary>
		/// Sets the name of the Popup <see cref="IRegion" />.
		/// </summary>
		/// <param name="owner">Owner of the Popup.</param>
		/// <param name="value">Name of the Popup <see cref="IRegion" />.</param>
		/// <exception cref="ArgumentNullException">owner</exception>
		public static void SetCreatePopupRegionWithName(DependencyObject owner, string value)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            owner.SetValue(CreatePopupRegionWithNameProperty, value);
        }

		/// <summary>
		/// Gets the <see cref="Style" /> for the Popup.
		/// </summary>
		/// <param name="owner">Owner of the Popup.</param>
		/// <returns>
		/// The <see cref="Style" /> for the Popup.
		/// </returns>
		/// <exception cref="ArgumentNullException">owner</exception>
		public static Style GetContainerWindowStyle(DependencyObject owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            return owner.GetValue(ContainerWindowStyleProperty) as Style;
        }

		/// <summary>
		/// Sets the <see cref="Style" /> for the Popup.
		/// </summary>
		/// <param name="owner">Owner of the Popup.</param>
		/// <param name="style"><see cref="Style" /> for the Popup.</param>
		/// <exception cref="ArgumentNullException">owner</exception>
		public static void SetContainerWindowStyle(DependencyObject owner, Style style)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            owner.SetValue(ContainerWindowStyleProperty, style);
        }

		/// <summary>
		/// Creates a new <see cref="IRegion" /> and registers it in the default <see cref="IRegionManager" />
		/// attaching to it a <see cref="DialogActivationBehavior" /> behavior.
		/// </summary>
		/// <param name="owner">The owner of the Popup.</param>
		/// <param name="regionName">The name of the <see cref="IRegion" />.</param>
		/// <remarks>
		/// This method would typically not be called directly, instead the behavior
		/// should be set through the Attached Property <see cref="CreatePopupRegionWithNameProperty" />.
		/// </remarks>
		public static void RegisterNewPopupRegion(DependencyObject owner, string regionName)
        {
            // Creates a new region and registers it in the default region manager.
            // Another option if you need the complete infrastructure with the default region behaviors
            // is to extend DelayedRegionCreationBehavior overriding the CreateRegion method and create an 
            // instance of it that will be in charge of registering the Region once a RegionManager is
            // set as an attached property in the Visual Tree.
            var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            if (regionManager != null)
            {
                IRegion region = new SingleActiveRegion();
                DialogActivationBehavior behavior = new WindowDialogActivationBehavior { HostControl = owner};

                region.Behaviors.Add(DialogActivationBehavior.BEHAVIOR_KEY, behavior);
                regionManager.Regions.Add(regionName, region);
            }
        }

		/// <summary>
		/// Creates the popup region with name property changed.
		/// </summary>
		/// <param name="hostControl">The host control.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void CreatePopupRegionWithNamePropertyChanged(DependencyObject hostControl, DependencyPropertyChangedEventArgs e)
        {
            if (IsInDesignMode(hostControl))
            {
                return;
            }

            RegisterNewPopupRegion(hostControl, e.NewValue as string);
        }

		/// <summary>
		/// Determines whether [is in design mode] [the specified element].
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>
		///   <c>true</c> if [is in design mode] [the specified element]; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsInDesignMode(DependencyObject element)
        {
            // Due to a known issue in Cider, GetIsInDesignMode attached property value is not enough to know if it's in design mode.
            return DesignerProperties.GetIsInDesignMode(element) || Application.Current == null
                   || Application.Current.GetType() == typeof(Application);
        }
    }
}
