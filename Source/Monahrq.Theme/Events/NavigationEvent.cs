using Microsoft.Practices.Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Theme.Events
{
	//This event notifies of Navigation event has occured, passing view name as a string
	//
	/// <summary>
	/// Event sent on a navigation request.
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.String}" />
	public class NavigationEvent : CompositePresentationEvent<string> { }


	/// <summary>
	/// Event sent on a navigation request for current view model.
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.Type}" />
	public class NavigationEventCurrentViewModel : CompositePresentationEvent<Type> { }
}
