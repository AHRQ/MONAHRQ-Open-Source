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
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.Prism.Interactivity.Tests
{
    [TestClass]
    public class InteractionRequestFixture
    {
        [TestMethod]
        public void WhenANotificationIsRequested_ThenTheEventIsRaisedWithTheSuppliedContext()
        {
            var request = new InteractionRequest<Notification>();
            object suppliedContext = null;
            request.Raised += (o, e) => suppliedContext = e.Context;

            var context = new Notification();

            request.Raise(context, c => { });

            Assert.AreSame(context, suppliedContext);
        }

        [TestMethod]
        public void WhenTheEventCallbackIsExecuted_ThenTheNotifyCallbackIsInvokedWithTheSuppliedContext()
        {
            var request = new InteractionRequest<Notification>();
            Action eventCallback = null;
            request.Raised += (o, e) => eventCallback = e.Callback;

            var context = new Notification();
            object suppliedContext = null;

            request.Raise(context, c => { suppliedContext = c; });

            eventCallback();

            Assert.AreSame(context, suppliedContext);
        }
    }
}
