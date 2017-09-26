using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Default.Views;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;

namespace Monahrq.Default.Controllers
{
    /// <summary>
    /// class to resume the processing
    /// </summary>
    /// <seealso cref="Empty" />
    [Export]
    public class ResumeNormalProcessingHandler : DefaultCompositeUIEventHandler<Empty>
    {

        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import]
        IRegionManager RegionManager { get; set; }

        [Import(LogNames.Session)]
        public ILogWriter Logger { get; set; }

        /// <summary>
        /// Handles the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        public override void Handle(Empty payload)
        {
            var deactivateThese = RegionManager.Regions[RegionNames.Modal].Views.OfType<PleaseStandBy>();
            foreach (var ctrl in deactivateThese)
            {
                this.Logger.Debug("Resuming normal processing");
                RegionManager.Regions[RegionNames.Modal].Deactivate(ctrl);
                RegionManager.Regions[RegionNames.Modal].Remove(ctrl);
            }
        }

       
    }
}
