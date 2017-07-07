using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Sdk.Extensibility.Models;

namespace Monahrq.Sdk.Extensibility.Utility
{
    public static class FeatureDescriptorUtility
    {

        public static bool Equals(FeatureDescriptor x, FeatureDescriptor y)
        {
            return x == null && y == null || (x != null && y != null) && (x.Name ?? string.Empty) == (y.Name ?? string.Empty);
        }

        public static int GetHashCode(FeatureDescriptor obj)
        {
            return obj.Name.GetHashCode();
        }

        public static bool Equals(Feature  x, Feature  y)
        {
            return x == null && y == null || (x != null && y != null) && Equals(x.Descriptor,y.Descriptor);
        }

        public static int GetHashCode(Feature  obj)
        {
            return GetHashCode(obj.Descriptor);
        }

     
    }

  
}
