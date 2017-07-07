using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// The interface for tab vi.ew models
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    /// <seealso cref="Microsoft.Practices.Prism.IActiveAware" />
    public interface ITabViewModel : IPartImportsSatisfiedNotification, IActiveAware
    {
        /// <summary>
        /// Called when [save].
        /// </summary>
        void OnSave();
        /// <summary>
        /// Called when [cancel].
        /// </summary>
        void OnCancel();
        /// <summary>
        /// Called when [reset].
        /// </summary>
        void OnReset();

        /// <summary>
        /// Called when [is active changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        void OnIsActiveChanged(object sender, EventArgs eventArgs);
    }
}