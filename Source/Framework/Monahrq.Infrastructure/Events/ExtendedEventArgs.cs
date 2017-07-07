using System;

namespace Monahrq.Infrastructure.Entities.Events
{
    public class ExtendedEventArgs<T>: EventArgs
    {
        public T Data { get; set; }
        public ExtendedEventArgs(){}
        public ExtendedEventArgs(T modelData)
        {
            Data = modelData;
        }
    }

    public class ExtendedEventArgs<T, TId> : EventArgs
    {
        public TId DataId { get; set; }
        public T Data { get; set; }
        public ExtendedEventArgs() { }
        public ExtendedEventArgs(T modelData, TId dataId)
        {
            Data = modelData;
        }
    }
}
