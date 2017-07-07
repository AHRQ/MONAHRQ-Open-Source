using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Sdk.Events;
using WpfRichText.Ex.Commands;

namespace Monahrq.Default.ViewModels
{
    /// <summary>
    /// class for import result dialog
    /// </summary>
    /// <seealso cref="Monahrq.Default.ViewModels.EventPayloadBaseViewModel{Monahrq.Sdk.Events.ISimpleImportCompletedPayload}" />
    public class SimpleImportResultsDialogViewModel : EventPayloadBaseViewModel<ISimpleImportCompletedPayload>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleImportResultsDialogViewModel"/> class.
        /// </summary>
        /// <param name="payload">The payload.</param>
        public SimpleImportResultsDialogViewModel(ISimpleImportCompletedPayload payload): base(payload)
        {
        }

        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        public DelegateCommand SaveCommand { get; private set; }
        /// <summary>
        /// Gets the close command.
        /// </summary>
        /// <value>
        /// The close command.
        /// </value>
        public DelegateCommand CloseCommand { get; private set; }
    }
}
