using Microsoft.Practices.Prism.Regions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Theme.PopupDialog;
using Monahrq.Sdk.Regions;
using Monahrq.TestSupport;
using Rhino.Mocks;
using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Monahrq.Default.Test.PopupDialog
{
    [TestClass]
    public class PopupDialogServiceTest:MefTestFixture
    {

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();

            var regionManager = MockRepository.GenerateMock<IRegionManager>();
            var region = MockRepository.GenerateStub<IRegion>();
            var regions = MockRepository.GenerateStub<IRegionCollection>();
            regions.Stub(r => r.ContainsRegionWithName(RegionNames.MainContent)).Return(true);
            regions.Stub(r => r[RegionNames.MainContent]).Return(region);
            regionManager.Stub(r => r.Regions).Return(regions);
            Container.ComposeExportedValue(regionManager);

            var view = MockRepository.GenerateStub<IPopupDialogView>();
            Container.ComposeExportedValue(view);
            
            var viewModel = MockRepository.GenerateStub<IPopupDialogViewModel>();
            view.Stub(v=>v.Model).Return(viewModel);
        }

        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new[] { typeof(PopupDialogService) };
            }
        }

        [TestMethod]
        public void TestHowToConstruct()
        {
            var service = new PopupDialogService();
            PrivateObject priv = new PrivateObject(service);
            var expected = Container.GetExportedValue<IPopupDialogView>();
            priv.SetProperty("PopupView", expected);
            var actual = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            Assert.IsNotNull(actual);
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void TestDependency()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            Assert.IsNotNull(view);
        }

        [TestMethod]
        public void TestSetOKButtonText()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = Guid.NewGuid().ToString();
            service.ButtonOKText = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.OKButtonText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSetCancelButtonText()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = Guid.NewGuid().ToString();
            service.ButtonCancelText = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.CancelButtonText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSetYesButtonText()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = Guid.NewGuid().ToString();
            service.ButtonYesText = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.YesButtonText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSetNoButtonText()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = Guid.NewGuid().ToString();
            service.ButtonNoText = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.NoButtonText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSetAbortButtonText()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = Guid.NewGuid().ToString();
            service.ButtonAbortText = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.AbortButtonText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSetRetryButtonText()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = Guid.NewGuid().ToString();
            service.ButtonRetryText = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.RetryButtonText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSetIgnoreButtonText()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = Guid.NewGuid().ToString();
            service.ButtonIgnoreText = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.IgnoreButtonText;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSetMessage()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = Guid.NewGuid().ToString();
            service.Message = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Message;
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TestSetTitle()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = Guid.NewGuid().ToString();
            service.Title = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Title;
            Assert.AreEqual(expected, actual);
        }

        // TODO: Move to View Model test?
        [TestMethod]
        [Ignore]
        public void TestDefaultButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.OK | PopupDialogButtons.Cancel;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOkButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.OK;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCancelButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.Cancel;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestYesButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.Yes;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNoButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.No;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAbortButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.Abort;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestRetryButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.Retry;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIgnoreButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.Ignore;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOkCancelButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.OK | PopupDialogButtons.Cancel;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void TestYesNoButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.Yes | PopupDialogButtons.No;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAbortRetryIgnoreButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.Abort | PopupDialogButtons.Retry | PopupDialogButtons.Ignore;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAllButtonStatus()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = PopupDialogButtons.OK | PopupDialogButtons.Cancel |
                           PopupDialogButtons.Yes | PopupDialogButtons.No |
                           PopupDialogButtons.Abort | PopupDialogButtons.Retry | PopupDialogButtons.Ignore;
            service.Buttons = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.Buttons;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestContentControl()
        {
            var service = Container.GetExportedValue<IPopupDialogService>();
            var expected = new ContentControl();
            service.ControlContent = expected;
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;
            var actual = view.Model.ControlContent;
            Assert.AreEqual(expected, actual);
        }

        // TODO: Need to fix test.
        [TestMethod]
        [Ignore]
        public void TestShowMessageView()
        {
            var regions = Container.GetExportedValue<IRegionManager>().Regions;
            regions.Stub(r => r.ContainsRegionWithName(string.Empty));

            var service = Container.GetExportedValue<IPopupDialogService>();
            PrivateObject priv = new PrivateObject(service);
            var view = priv.GetFieldOrProperty("PopupView") as IPopupDialogView;

            service.ShowMessage();

            var args = regions.GetArgumentsForCallsMadeOn(r => r.ContainsRegionWithName(string.Empty));
            var regionName = args[0][0] as string;
            Assert.AreEqual(RegionNames.Modal, regionName, "The wrong region was loaded.");

        }

        // TODO: Not Tested
        //void ShowMessage();
        //void ShowMessage(string message);
        //void ShowMessage(string message, string title);
        //void ShowMessage(string message, string title, PopupDialogButtons buttons);
        //event EventHandler Closed;

    }
}