

using System.Runtime.InteropServices;

namespace Monahrq.Infrastructure.Types
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[ComVisible(true)]
	public interface IDeepCloneable<T>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		T DeepClone();
	}
}
