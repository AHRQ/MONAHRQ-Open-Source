namespace Monahrq.Sdk.Model
{
	public interface IBaseNotifyInterceptor
	{
		/// <summary>
		/// The intercepted BaseNotify object will call this method before performing any RaisePropertyChange event handling.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>
		///		True  - Continue with normal processing.
		///		False - The event has been handled and normal processing should be avoided.
		/// </returns>
		bool OnRaisePropertyChanged(BaseNotifyBase baseNotify,string propertyName);
	}
}
