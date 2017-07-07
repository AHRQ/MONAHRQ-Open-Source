using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Entities.Domain
{
    public class RepositoryException: Exception
    {
        public RepositoryException(Exception innerException
            , string stringFormat
            , params object[] formatArgs)
            : base(string.Format(stringFormat, formatArgs), innerException)
        {
        }
    }
}
