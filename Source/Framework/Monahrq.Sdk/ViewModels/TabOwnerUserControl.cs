using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Events;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// The Update Tab Index Event used in updating the TabIndex in conjunction with Prism.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Sdk.ViewModels.TabIndexSelecteor}" />
    public class UpdateTabIndexEvent : CompositePresentationEvent<TabIndexSelecteor> { }

    /// <summary>
    /// 
    /// </summary>
    public class TabIndexSelecteor
    {
        /// <summary>
        /// The tab name
        /// </summary>
        public string TabName;
        /// <summary>
        /// The tab index
        /// </summary>
        public int TabIndex;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    public abstract class TabOwnerUserControl : UserControl, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import]
        protected IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            EventAggregator.GetEvent<UpdateTabIndexEvent>().Subscribe(OnUpdateTabIndex, true);
        }

        /// <summary>
        /// Called when [update tab index].
        /// </summary>
        /// <param name="obj">The object.</param>
        public abstract void OnUpdateTabIndex(TabIndexSelecteor obj);
    }
}