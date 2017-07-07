using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

namespace Monahrq.Sdk.Modules.Settings
{
    public interface IHospitalRegion
    {
        bool IsDefined { get; }
        void Save();
        Type SelectedRegionType { get; set; }
        IEnumerable<State> SelectedStates { get; set; }
    }
}
