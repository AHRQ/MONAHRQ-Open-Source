

using System.Runtime.InteropServices;

namespace Monahrq.Infrastructure.Types
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[ComVisible(true)]
	public interface IDeepAssignable<T>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		void DeepAssignmentFrom(T other);
	}
}
