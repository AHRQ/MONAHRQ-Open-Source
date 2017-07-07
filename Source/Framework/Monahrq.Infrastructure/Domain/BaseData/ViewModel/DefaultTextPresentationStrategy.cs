using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Domain.BaseData.ViewModel
{

    public interface ITextPresentationStrategy 
    {
        string Present(object data);
    }

    public class DefaultTextPresentationStrategy : ITextPresentationStrategy
    {
        public string Present(object data)
        {
            return data == null ? string.Empty : data.ToString();
        }
    }
}
