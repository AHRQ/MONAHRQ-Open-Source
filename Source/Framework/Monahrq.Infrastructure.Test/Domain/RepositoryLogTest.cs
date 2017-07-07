using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.TestSupport;
using Rhino.Mocks;
using NHibernate;
using NHibernate.Linq;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;

namespace Monahrq.Infrastructure.Test.Domain
{
    [TestClass]
    public class RepositoryLogTest: MefTestFixture
    {

        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get
            {
                return
                    new[]
                    {
                        typeof(WingRepository),
                    };
            }
        }
         
       
    }

   
}
