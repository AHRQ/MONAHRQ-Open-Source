using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Default.Views;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;

using Monahrq.Sdk.Extensions;

namespace Monahrq.Default.Controllers
{
    /// <summary>
    ///  class for stand by operations
    /// </summary>
    /// <seealso cref="Monahrq.Default.Controllers.DefaultCompositeUIEventHandler{Monahrq.Sdk.Events.PleaseStandByEventPayload}" />
    [Export]
    public class PleaseStandByHandler : DefaultCompositeUIEventHandler<PleaseStandByEventPayload>
    {
        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import]
        IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        [Import]
        IEventAggregator Events { get; set; }

        [Import(LogNames.Session)]
        public ILogWriter Logger { get; set; }

        /// <summary>
        /// Handles the specified result.
        /// </summary>
        /// <param name="result">The result.</param>
        public override void Handle(PleaseStandByEventPayload result)
        {
            var ctrl = new PleaseStandBy();            
            ctrl.Model = result;
            Events.GetEvent<PleaseStandByMessageUpdateEvent>().Subscribe(msg =>
                {
                    this.Logger.Debug($"Processing update: {msg}");
                    ctrl.Model.Message = msg;
                    ctrl.Refresh();
                });

            this.Logger.Debug("Pausing normal processing");

            IRegion region = RegionManager.Regions[RegionNames.Modal];
            if (!region.Views.Contains(ctrl))
            {
                region.Add(ctrl);
            }
            region.Activate(ctrl);
        }
    }
}
