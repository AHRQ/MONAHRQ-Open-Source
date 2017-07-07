using System.Linq;
using System.Xml;
using Monahrq.Infrastructure.Utility;
using System.Dynamic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Monahrq.Infrastructure.Extensions
{
	public static class TaskExtensions
	{
		public static void WaitWithPumping(this Task task)
		{
			if (task == null) throw new ArgumentNullException("task");
			var nestedFrame = new DispatcherFrame();
			task.ContinueWith(_ => nestedFrame.Continue = false);
			Dispatcher.PushFrame(nestedFrame);
			task.Wait();
		}
	}
}

