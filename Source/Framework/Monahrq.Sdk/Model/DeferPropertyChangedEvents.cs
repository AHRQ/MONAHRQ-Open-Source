using System;
using System.Collections.Generic;
using System.Linq;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Sdk.Model
{
    /// <summary>
    /// Class will accumulate all property changed events that occur to a Model while this object is active
    /// and only process them once this object is disposed of.
    /// </summary>
    public class DeferPropertyChangedEvents : IBaseNotifyInterceptor, IDisposable
	{
        #region Methods.
        /// <summary>
        /// Initializes a new instance of the <see cref="DeferPropertyChangedEvents"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public DeferPropertyChangedEvents(BaseNotifyBase model)
		{
			deferredModelList = new Dictionary<BaseNotifyBase, List<string>>();
			deferredModelList.Add(model,new List<string>());
			deferredModelList.ForEach(m => m.Key.SetInterceptor(this));
		}
        /// <summary>
        /// Initializes a new instance of the <see cref="DeferPropertyChangedEvents"/> class.
        /// </summary>
        /// <param name="models">The models.</param>
        public DeferPropertyChangedEvents(IEnumerable<BaseNotify> models)
		{
			deferredModelList = new Dictionary<BaseNotifyBase, List<string>>();
			models.ForEach(m => deferredModelList.Add(m, new List<string>()));
			deferredModelList.ForEach(m => m.Key.SetInterceptor(this));
		}

        /// <summary>
        /// The intercepted BaseNotify object will call this method before performing any RaisePropertyChange event handling.
        /// </summary>
        /// <param name="baseNotify"></param>
        /// <param name="propertyName"></param>
        /// <returns>
        /// True  - Continue with normal processing.
        /// False - The event has been handled and normal processing should be avoided.
        /// </returns>
        public bool OnRaisePropertyChanged(BaseNotifyBase baseNotify,string propertyName)
		{
			//	Only add this propertyName if it is not already present.
			if (!deferredModelList[baseNotify].Any(pn => pn == propertyName))
				deferredModelList[baseNotify].Add(propertyName);
			return false;
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
		{
			if (deferredModelList == null) return;
			deferredModelList.ForEach(dm => dm.Key.SetInterceptor(null));

			//  Run all deferred events.
			foreach (var deferredModel in deferredModelList)
			{
				deferredModel.Value.ForEach(evt => deferredModel.Key.RaisePropertyChanged(evt));
			}
			deferredModelList.ForEach(md => md.Value.Clear());
			deferredModelList.Clear();
		}
        #endregion

        #region Variables.
        /// <summary>
        /// The deferred model list
        /// </summary>
        private Dictionary<BaseNotifyBase,List<String>> deferredModelList;
		#endregion
	}
}
