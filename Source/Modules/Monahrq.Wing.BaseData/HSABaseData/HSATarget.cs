using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Sdk.Attributes.Wings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Monahrq.Wing.BaseData.HSABaseData
{
    [Serializable]
    [WingTarget("HSA Base Data", "b5116093-a6c5-417e-8d6d-1c687e47df4f", "Mapping target for HSA Base Data", true)]
    public class HSATarget : ContentPartRecord
    {
        int _HSANumber;
        [WingTargetElement("HSANumber", false, 1)]
        public virtual int HSANumber
        {
            get { return _HSANumber; }
            set { _HSANumber = value; }
        }

        string _HSACity;
        [StringLength(25, MinimumLength = 1)]
        [WingTargetElement("HSACity", false, 2)]
        public virtual string HSACity
        {
            get { return _HSACity; }
            set { _HSACity = value; }
        }

        string _HSAState;
        [StringLength(2, MinimumLength = 1)]
        [WingTargetElement("HSAState", false, 3)]
        public virtual string HSAState
        {
            get { return _HSAState; }
            set { _HSAState = value; }
        }
    }
}
