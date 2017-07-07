using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using NHibernate;
using NHibernate.Linq;

namespace Monahrq.DataSets.Services
{
    [Export]
    public class CategoryService
    {
        IDomainSessionFactoryProvider Provider { get; set; }
        HospitalRegistry Registry { get; set; }
        IConfigurationService ConfigurationService { get; set; }

        [ImportingConstructor]
        public CategoryService(IDomainSessionFactoryProvider provider, 
                IConfigurationService conf)
        {
            Provider = provider;
            ConfigurationService = conf;
        }
 

        public void Delete(HospitalCategory HospitalCategory)
        {
            using (var sess = Provider.SessionFactory.OpenSession())
            {
                var tx = sess.BeginTransaction();
                try
                {
                    
                    var catdelete = string.Format("delete from {0} entity where entity.Id = {1}"
                                , typeof(HospitalCategory).FullName, HospitalCategory.Id);
                    sess.CreateQuery(catdelete)
                        .SetTimeout( (int) ConfigurationService.MonahrqSettings.ShortTimeout.TotalSeconds)
                        .ExecuteUpdate();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
        
}
