using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Core;
using Monahrq.Infrastructure.Entities.Core;

namespace Monahrq.Wing.BaseData
{
    public interface IDataLoader
    {
        void LoadData();
        ReaderDefinition Reader { get; }
    }
}
