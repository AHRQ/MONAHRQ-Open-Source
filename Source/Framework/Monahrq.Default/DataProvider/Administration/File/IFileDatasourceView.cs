using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Default.DataProvider.Administration.File
{

    /// <summary>
    /// Interface for file data view
    /// </summary>
    public interface IFileDatasourceView
    {
        IFileDatasourceViewModel Model { get; set; }
    }
}
