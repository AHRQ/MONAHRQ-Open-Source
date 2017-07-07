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
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;

namespace Microsoft.Practices.Prism.MefExtensions
{
    /// <summary>
    /// Provides service location utilizing the Managed Extensibility Framework container.
    /// </summary>
    public class MefServiceLocatorAdapter : ServiceLocatorImplBase
    {
        private readonly CompositionContainer compositionContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MefServiceLocatorAdapter"/> class.
        /// </summary>
        /// <param name="compositionContainer">The MEF composition container.</param>
        public MefServiceLocatorAdapter(CompositionContainer compositionContainer)
        {
            this.compositionContainer = compositionContainer;
        }

        /// <summary>
        /// Resolves the instance of the requested service.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <returns>The requested service instance.</returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            List<object> instances = new List<object>();

            IEnumerable<Lazy<object, object>> exports = this.compositionContainer.GetExports(serviceType, null, null);
            if (exports != null)
            {
                instances.AddRange(exports.Select(export => export.Value));
            }

            return instances;
        }

        /// <summary>
        /// Resolves all the instances of the requested service.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <param name="key">Name of registered service you want. May be null.</param>
        /// <returns>Sequence of service instance objects.</returns>
        protected override object DoGetInstance(Type serviceType, string key)
        {
            IEnumerable<Lazy<object, object>> exports = this.compositionContainer.GetExports(serviceType, null, key);
            if ((exports != null) && (exports.Count() > 0))
            {
                // If there is more than one value, this will throw an InvalidOperationException, 
                // which will be wrapped by the base class as an ActivationException.
                return exports.Single().Value;
            }

            throw new ActivationException(
                this.FormatActivationExceptionMessage(new CompositionException("Export not found"), serviceType, key));
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns></returns>
        public override TService GetInstance<TService>()
        {
            try
            {
                if (typeof (TService) == typeof (CompositionContainer))
                {
                    object temp = compositionContainer;
                    return (TService) temp;
                }
                return base.GetInstance<TService>();
            }
            catch (ReflectionTypeLoadException exc)
            {
                var msg = string.Empty;
                foreach(var loadException in exc.LoaderExceptions.ToList())
                {
                    msg += loadException.GetBaseException().Message + Environment.NewLine;
                    
                }

                string types = exc.Types != null && exc.Types.Any() ? string.Join(",",exc.Types.Select(t => t.FullName).ToList()) : null;

                throw new ApplicationException(string.Format("LoaderMessages: {1}{0}{0}Types: {2}", Environment.NewLine, msg, types));
            }
            catch (Exception exc)
            {
                throw exc.GetBaseException();
            }
        }
    }
}