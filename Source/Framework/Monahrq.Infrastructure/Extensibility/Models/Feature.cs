using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Extensibility.Models
{
    public class Feature
    {
        public FeatureDescriptor Descriptor { get; set; }
        public IEnumerable<Type> ExportedTypes { get; set; }
        public override string ToString()
        {
            var types = string.Join("\n\t", ExportedTypes.Select(t => t.ToString()));

            return string.Format("{0}\nTypes:\n{1}",
                    Descriptor.ToString(), types);
        }
    }
}
