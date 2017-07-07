using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Entities.Core.Import
{
    public partial class CSVDataReaderDictionary : Component
    {
        public CSVDataReaderDictionary()
        {
            InitializeComponent();
            OnInitialized();
        }

        public IEnumerable<IDataLoader> DataLoaders { get; set; }

        public CSVDataReaderDictionary(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            OnInitialized();
        }

    }
}
