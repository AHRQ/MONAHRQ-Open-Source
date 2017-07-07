//===================================================================================
// Microsoft patterns & practices
// Composite Application Guidance for Windows Presentation Foundation and Silverlight
//===================================================================================
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===================================================================================
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//===================================================================================
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Practices.Prism.MefExtensions
{
    ///<summary>
    /// DefaultPrismServiceRegistrationAgent allows the Prism required types to be registered if necessary.
    ///</summary>
    public static class DefaultPrismServiceRegistrar
    {
        /// <summary>
        /// Registers the required Prism types that are not already registered in the <see cref="AggregateCatalog"/>.
        /// </summary>
        ///<param name="aggregateCatalog">The <see cref="AggregateCatalog"/> to register the required types in, if they are not already registered.</param>
        public static AggregateCatalog RegisterRequiredPrismServicesIfMissing(AggregateCatalog aggregateCatalog)
        {
            if (aggregateCatalog == null) throw new ArgumentNullException("aggregateCatalog");
            IEnumerable<ComposablePartDefinition> partsToRegister =
                GetRequiredPrismPartsToRegister(aggregateCatalog);

            PrismDefaultsCatalog cat = new PrismDefaultsCatalog(partsToRegister);
            aggregateCatalog.Catalogs.Add(cat);
            return aggregateCatalog;
        }

        private static IEnumerable<ComposablePartDefinition> GetRequiredPrismPartsToRegister(AggregateCatalog aggregateCatalog)
        {
            List<ComposablePartDefinition> partsToRegister = new List<ComposablePartDefinition>();

            try
            {
                var catalogWithDefaults = GetDefaultComposablePartCatalog();
                foreach (var part in catalogWithDefaults.Parts)
                {
                    foreach (var export in part.ExportDefinitions)
                    {
                        bool exportAlreadyRegistered = false;
                        foreach (var registeredPart in aggregateCatalog.Parts)
                        {
                            foreach (var registeredExport in registeredPart.ExportDefinitions)
                            {
                                if (string.Compare(registeredExport.ContractName, export.ContractName, StringComparison.Ordinal) == 0)
                                {
                                    exportAlreadyRegistered = true;
                                    break;
                                }
                            }
                        }

                        if (exportAlreadyRegistered != false) continue;
                        if (!partsToRegister.Contains(part))
                        {
                            partsToRegister.Add(part);
                        }
                    }
                }
            }
            //catch (ReflectionTypeLoadException exc)
            //{
            //    List<Exception> list = exc.LoaderExceptions.ToList();

            //    var msg = string.Empty;

            //    foreach (var exception in list)
            //        if (!msg.Contains(exception.GetBaseException().Message))
            //            msg += exception.GetBaseException().Message + Environment.NewLine;

            //    string types;
            //    if (exc.Types != null && exc.Types.Any())
            //    {
            //        var typesList = exc.Types.Where(t => t != null).Select(t => t.FullName).ToList();
            //        types = string.Join(",", typesList.Distinct().ToList());
            //    }
            //    else
            //        types = null;

            //    throw new ApplicationException(string.Format("Loader Messages: {1}{0}{0}Types: {2}", Environment.NewLine, msg, types));
            //}
            catch (Exception exc)
            {
                //var exportTypes = partsToRegister.SelectMany(p => p.ExportDefinitions.Select(d => d.ContractName)).ToList();
                //var importTypes = partsToRegister.SelectMany(p => p.ImportDefinitions.Select(d => d.ContractName)).ToList();
                //var msg = string.Format("Error occurred in GetRequiredPrismParts. Message: {3}{0}{0}Import Types: {1}{0}Export Type: {2}", Environment.NewLine, string.Join(",", importTypes), string.Join(",", exportTypes), exc.GetBaseException().Message);
                throw new Exception(exc.GetBaseException().Message, exc.GetBaseException());
            }
            
            return partsToRegister;
        }

        /// <summary>
        /// Returns an <see cref="AssemblyCatalog" /> for the current assembly
        /// </summary>
        /// <remarks>
        /// Due to the security restrictions in Silverlight, you cannot call
        /// <code>Assembly.GetAssembly(typeof(MefBootstrapper))</code>
        /// So, we are forced to use <code> Assembly.GetCallingAssembly()</code> instead.
        /// To ensure that the calling assembly is this one, the call is in this
        /// private helper method.
        /// </remarks>
        /// <returns>
        /// Returns an <see cref="AssemblyCatalog" /> for the current assembly
        /// </returns>
        private static ComposablePartCatalog GetDefaultComposablePartCatalog()
        {
#if SILVERLIGHT
            return new AssemblyCatalog(Assembly.GetCallingAssembly());
#else
            return new AssemblyCatalog(Assembly.GetAssembly(typeof(MefBootstrapper)));
#endif
        }
    }
}