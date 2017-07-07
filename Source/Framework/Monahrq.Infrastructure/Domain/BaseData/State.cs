using System;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping;
using PropertyChanged; 
using System.Collections.Generic;
using System.Windows;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    [Serializable]
    public class State : Entity<int>
    {

        protected override void Initialize()
        {
            base.Initialize();
            //HospitalServiceAreas = new List<HospitalServiceArea>();
            //HealthReferralRegions = new List<HealthReferralRegion>();
            //CustomRegions = new List<CustomRegion>();
            Hospitals = new List<Hospital>();
        }

        public virtual string FIPSState { get; set; }
        public virtual string Abbreviation { get; set; }
        public virtual double MaxX { get; set; }
        public virtual double MaxY { get; set; }
        public virtual double MinX { get; set; }
        public virtual double MinY { get; set; }
        public virtual double X0 { get; set; }
        public virtual double Y0 { get; set; }

        public virtual Rect BoundingRegion 
        {
            get
            {
                return new Rect(new Point(MinX, MaxY), new Point(MaxX, MinY));
            }
            set
            {
                MinX = value.Left;
                MinY = value.Bottom;
                MaxX = value.Right;
                MaxY = value.Top;
            }
        }

        public virtual Point Centroid
        {
            get
            {
                return  new Point(X0, Y0);
            }
            set
            {
                X0 = value.X;
                Y0 = value.Y;
            }
        }

        //public virtual IList<HospitalServiceArea> HospitalServiceAreas { get; private set; }
        //public virtual IList<HealthReferralRegion> HealthReferralRegions { get; private set; }
        //public virtual IList<CustomRegion> CustomRegions { get; private set; }
        public virtual IList<Hospital> Hospitals { get; private set; }
    }
}
