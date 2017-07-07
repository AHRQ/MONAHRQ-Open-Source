﻿using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.BaseData
{
    [ImplementPropertyChanged]
    [EntityTableName("Base_RateProviders")]
    public class RateProvider : EnumLookupEntity<int>
    {}
}