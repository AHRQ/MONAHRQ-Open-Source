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
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Microsoft.Practices.Prism.MefExtensions
{
    ///<summary>
    /// A very simple custom <see href="ComposablePartCatalog" /> that takes an enumeration 
    /// of parts and returns them when requested.
    ///</summary>
    internal class PrismDefaultsCatalog : ComposablePartCatalog
    {
        private readonly IEnumerable<ComposablePartDefinition> parts;

        ///<summary>
        /// Creates a PrismDefaultsCatalog that will return the provided parts when requested.
        ///</summary>
        ///<param name="parts">Parts to add to the catalog</param>
        ///<exception cref="ArgumentNullException">Thrown if the parts parameter is null.</exception>
        public PrismDefaultsCatalog(IEnumerable<ComposablePartDefinition> parts)
        {
            if (parts == null) throw new ArgumentNullException("parts");
            this.parts = parts;
        }

        /// <summary>
        /// Gets the parts contained in the catalog.
        /// </summary>
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return parts.AsQueryable(); }
        }
    }
}