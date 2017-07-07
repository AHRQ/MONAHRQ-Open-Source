using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Theme.PopupDialog;
using Monahrq.TestSupport;

namespace Monahrq.Default.Test.PopupDialog
{
    [TestClass]
    public class PopupDialogViewModelTest : MefTestFixture
    {

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
        }

        [TestMethod]
        public void CtorTest()
        {
            var viewModel = new PopupDialogViewModel();
            var expected = PopupDialogButtons.OK | PopupDialogButtons.Cancel;
            var actual = viewModel.Buttons;
            Assert.AreEqual(expected, actual);
        }
    }
}