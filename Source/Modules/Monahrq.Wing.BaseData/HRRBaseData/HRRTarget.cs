using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Sdk.Attributes.Wings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Monahrq.Wing.BaseData.HRRBaseData
{
    [Serializable]
    [WingTarget("HRR Base Data", "77fc5bf0-dcd2-44ad-85e6-6f47cbd319b7", "Mapping target for HRR Base Data", true)]
    public class HRRTarget : ContentPartRecord
    {
        int _HRRNumber;
        [WingTargetElement("HRRNumber", false, 1)]
        public virtual int HRRNumber
        {
            get { return _HRRNumber; }
            set { _HRRNumber = value; }
        }

        string _HRRCity;
        [StringLength(25, MinimumLength = 1)]
        [WingTargetElement("HRRCity", false, 2)]
        public virtual string HRRCity
        {
            get { return _HRRCity; }
            set { _HRRCity = value; }
        }

        string _HRRState;
        [StringLength(2, MinimumLength = 1)]
        [WingTargetElement("HRRState", false, 3)]
        public virtual string HRRState
        {
            get { return _HRRState; }
            set { _HRRState = value; }
        }
    }
}
