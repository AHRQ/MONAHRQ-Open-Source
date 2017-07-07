using System;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    [Serializable]
    public class ZipCodeToLatLong : Entity<int>
    {
        public virtual string Zip { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
    }
}