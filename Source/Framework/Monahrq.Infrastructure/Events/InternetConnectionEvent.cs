using Microsoft.Practices.Prism.Events;

namespace Monahrq.Infrastructure.Events
{
    public class InternetConnectionEvent : CompositePresentationEvent<ConnectionState> { }

   public enum ConnectionState
   {
       /// <summary>
       /// <para>  Connection state is Offline</para>
       /// </summary>
       OffLine = 0,
       /// <summary>
       /// <para> Connection state is Online</para>
       /// </summary>
       OnLine = 1,
       /// <summary>
       /// <para> Connection state is Unknown</para>
       /// </summary>
       Unknown = -1
   }
}
