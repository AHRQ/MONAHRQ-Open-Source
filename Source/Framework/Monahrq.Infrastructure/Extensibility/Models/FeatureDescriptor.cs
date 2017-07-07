using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Extensibility.Models
{
    public class FeatureDescriptor
    {
        public FeatureDescriptor()
        {
            Dependencies = Enumerable.Empty<string>();
        }

        public ExtensionDescriptor Extension { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Priority { get; set; }
        public IEnumerable<string> Dependencies { get; set; }

        public override string ToString()
        {
            var desc = new List<string>()
            {
                  string.Format("Id: {0}", Id)
                 , string.Format("Name: {0}", Name)
                 , string.Format("Description: {0}", Description)
                 , string.Format("Category: {0}", Category)
                 , string.Format("Priority: {0}", Priority)
                 , string.Format("Dependencies: {0}", string.Join("\n\t", Dependencies))
            };
            desc.Add(Extension.ToString());
            return string.Join("\n", desc);
        }

    }
}
