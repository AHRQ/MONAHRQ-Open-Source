using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;
using PropertyChanged;
using Monahrq.Sdk.DataProvider.Builder;

using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Default.DataProvider.Administration
{
    /// <summary>
    /// View model class for data provider
    /// </summary>
    /// <seealso cref="Monahrq.Default.DataProvider.Administration.IDataProviderAdministratorViewModel" />
    [ImplementPropertyChanged]
    [Export(typeof(IDataProviderAdministratorViewModel))]
    public class DataProviderAdministratorViewModel : Monahrq.Default.DataProvider.Administration.IDataProviderAdministratorViewModel
    {
        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        /// <value>
        /// The connections.
        /// </value>
        public ICollectionView Connections { get; set; }
        /// <summary>
        /// Gets or sets the current connection
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public NamedConnectionModel Current { get; set; }
    }

    /// <summary>
    /// class for connection
    /// </summary>
    public class NamedConnectionModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedConnectionModel"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        public NamedConnectionModel(NamedConnectionElement element)
        {
            Configuration = element;
            LazyProviderExport = new Lazy<IDataProviderControllerExportAttribute>(() =>
                {
                    return Configuration.GetExportAttribute();
                });
        }

        /// <summary>
        /// Gets or sets the lazy provider export.
        /// </summary>
        /// <value>
        /// The lazy provider export.
        /// </value>
        Lazy<IDataProviderControllerExportAttribute> LazyProviderExport { get; set; }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public NamedConnectionElement Configuration { get; set; }
        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get
            {
                return LazyProviderExport.Value.ControllerName;
            }
        }

        /// <summary>
        /// Gets the details of the data provider
        /// </summary>
        /// <value>
        /// The details of the data provider
        /// </value>
        public string Details
        {
            get
            {
                var builder = LazyProviderExport.Value.CreateConnectionStringBuilder();
                builder.ConnectionString = Configuration.ConnectionString;
                Dictionary<string, string> items = new Dictionary<string, string>();
                items.Add("Name", Configuration.Name);
                return items.Concat(
                    builder.Keys.OfType<string>()
                        .Where(key => builder.ContainsKey(key))
                        .Select(
                            (key) =>
                            {
                                var obj = builder[key] as string;
                                var val = obj == null ? string.Empty : obj.ToString();
                                return new KeyValuePair<string, string>(key, val);
                            }
                            )
                        .Where(kvp => !string.IsNullOrEmpty(kvp.Value))).ToString();
            }
        }
    }
}
