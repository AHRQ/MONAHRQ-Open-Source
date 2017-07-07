using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Sdk.Regions;

namespace Monahrq.Sdk.Behaviors
{
    /// <summary>
    /// This is the auto populate exported wpf views custom behavior.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Regions.RegionBehavior" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    [Export(typeof(AutoPopulateExportedViewsBehavior))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AutoPopulateExportedViewsBehavior : RegionBehavior, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Override this method to perform the logic after the behavior has been attached.
        /// </summary>
        protected override void OnAttach()
        {
            AddRegisteredViews();
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            AddRegisteredViews();
        }

        /// <summary>
        /// Adds the registered views.
        /// </summary>
        private void AddRegisteredViews()
        {
            if (Region == null) return;

            foreach (var view in from viewEntry in 
                                     RegisteredViews where viewEntry.Metadata.RegionName == 
                                     Region.Name select viewEntry.Value into view where 
                                     !Region.Views.Contains(view) select view)
            {
                Region.Add(view);
            }
        }

        /// <summary>
        /// Gets or sets the registered views.
        /// </summary>
        /// <value>
        /// The registered views.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "MEF injected values")]
        [ImportMany(typeof(UserControl), AllowRecomposition = true)]
        public Lazy<object, IViewRegionRegistration>[] RegisteredViews { get; set; }
    }
}
