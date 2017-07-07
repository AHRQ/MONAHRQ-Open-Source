using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Test.Domain
{
    [Export]
    public class MockEntity : Entity<int> { }

    public interface IMockRepo : IRepository<MockEntity, int> { } 


    public partial interface IMockService
    {
    }

    [Export(typeof(IMockService))]
	public partial class MockService: IMockService
    {
    }

    
    
   
  
}
