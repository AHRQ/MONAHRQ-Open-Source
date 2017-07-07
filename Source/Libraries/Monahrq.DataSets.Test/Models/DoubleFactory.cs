using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Validation;
using Monahrq.Sdk.Attributes.Wings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Core.Attributes;

namespace Monahrq.DataSets.Test.Models
{
    class DoubleFactory
    {
        public static Monahrq.Infrastructure.Entities.Domain.Wings.Wing DummyWing 
        { 
            get 
            { 
                return WingRepository.New(Guid.NewGuid().ToString());
            }
        }
        public static Element CreateElementDouble(Target target, string name)
        {
            var result = new Element(target, name);
            return result;
        }
    }

    public enum Something { value1, value2, value3 };

    class FooTarget: DatasetRecord
    {
        [RequiredWarningAttribute]
        [WingTargetElementAttribute("SomeEnum",  true, 1)]
        public Something? SomeEnum { get; set; }

        public int SomeInt { get; set; }

        public string SomeString { get; set; }
    }
}
