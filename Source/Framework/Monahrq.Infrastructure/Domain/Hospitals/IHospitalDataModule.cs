using System;
using Monahrq.Infrastructure.Core;
using Monahrq.Infrastructure.Entities.Core;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals
{
    public interface IHospitalDataModule : IDataReaderDictionary
    {
        string Title { get; }
        void Load();
    }

}
