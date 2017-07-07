using Microsoft.Practices.Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Events
{
    /// <summary>
    /// Class for Help , Open and Close events
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.Boolean}" />
    public class HelpOpenCloseEvent : CompositePresentationEvent<bool> { }
}
