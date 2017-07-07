using System;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    [Serializable]
    public class ZipCodeToHRRAndHSA : Entity<int>
    {
        public virtual string Zip { get; set; }
        public virtual int HRRNumber { get; set; }
        public virtual int HSANumber { get; set; }
        public virtual string State { get; set; }
        public virtual string StateFIPS { get; set; }
        /// <summary>
        /// Do not use! Only for importing of base data.
        /// </summary>
        /// <value>
        /// The state_ identifier.
        /// </value>
       // public int State_Id { get; set; }
    }
}