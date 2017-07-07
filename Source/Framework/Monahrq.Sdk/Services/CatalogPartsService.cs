using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Services
{
    [Export(typeof(IContainerPartsService))]
    public class CatalogPartsService : Monahrq.Sdk.Services.IContainerPartsService
    {
        CompositionContainer Container { get; set; }

        [ImportingConstructor]
        public CatalogPartsService(CompositionContainer container)
        {
            Container = container;
        }

        public IEnumerable<T> GetRegisteredExportAttributes<T>() where T: ExportAttribute
        {
            Func<ExportDefinition, bool> test =
                 (def) => 
                {
                    return def.ContractName == string.Empty;
                };
            Expression<Func<ExportDefinition, bool>> isExported = exp => test(exp);

            var impDef = new ImportDefinition(isExported, string.Empty,ImportCardinality.ZeroOrMore,true,true);

            return null;
        }
    }
}
