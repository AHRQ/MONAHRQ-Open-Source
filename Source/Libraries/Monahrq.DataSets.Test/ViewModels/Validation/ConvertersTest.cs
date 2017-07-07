using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Monahrq.DataSets.ViewModels.Validation;
using System.Globalization;
using System.Windows;

namespace Monahrq.DataSets.Test.ViewModels.Validation
{
    [TestClass]
    public class ConvertersTest
    {
        [TestMethod]
        public void ValidationViewModelCollapseWhenRunningConverterPositiveTest()
        {

            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.IsRunning).Return(true);
            var convert = new BooleanCollapseConverter();
            var result = convert.Convert(model.IsRunning, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Collapsed, result);
        }


        [TestMethod]
        public void ValidationViewModelCollapseWhenRunningConverterNegativeTest()
        {

            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.IsRunning).Return(false);
            var convert = new BooleanCollapseConverter();
            var result = convert.Convert(model.IsRunning, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Visible, result);
        }


        [TestMethod]
        public void ValidationViewModelCollapseWhenRunningConverterInvertPositiveTest()
        {

            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.IsRunning).Return(true);
            var convert = new BooleanCollapseConverter();
            var result = convert.Convert(model.IsRunning, null, "invert", CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Visible, result);
        }


        [TestMethod]
        public void ValidationViewModelCollapseWhenRunningConverterInvertNegativeTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.IsRunning).Return(false);
            var convert = new BooleanCollapseConverter();
            var result = convert.Convert(model.IsRunning, null, "invert", CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [TestMethod]
        public void ValidationViewModelHiddenWhenRunningConverterPositiveTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.IsRunning).Return(true);
            var convert = new BooleanHiddenConverter();
            var result = convert.Convert(model.IsRunning, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Hidden, result);
        }


        [TestMethod]
        public void ValidationViewModelHiddenWhenRunningConverterNegativeTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.IsRunning).Return(false);
            var convert = new BooleanHiddenConverter();
            var result = convert.Convert(model.IsRunning, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Visible, result);
        }


        [TestMethod]
        public void ValidationViewModelHiddenWhenRunningConverterInvertPositiveTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.IsRunning).Return(true);
            var convert = new BooleanHiddenConverter();
            var result = convert.Convert(model.IsRunning, null, "invert", CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Visible, result);
        }


        [TestMethod]
        public void ValidationViewModelHiddenWhenRunningConverterInvertNegativeTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.IsRunning).Return(false);
            var convert = new BooleanHiddenConverter();
            var result = convert.Convert(model.IsRunning, null, "invert", CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Hidden, result);
        }

        IValidationResultsSummary summary = MockRepository.GenerateStub<IValidationResultsSummary>();
        [TestMethod]
        public void ValidationViewModelCollapseHasResultsConverterPositiveTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.ResultsSummary).Return(summary);
            var convert = new NullCollapseConverter();
            var result = convert.Convert(model.ResultsSummary, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Visible, result);
        }


        [TestMethod]
        public void ValidationViewModelCollapseHasResultsConverterNegativeTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.ResultsSummary).Return(null);
            var convert = new NullCollapseConverter();
            var result = convert.Convert(model.ResultsSummary, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Collapsed, result);
        }


        [TestMethod]
        public void ValidationViewModelCollapseHasResultsConverterInvertPositiveTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.ResultsSummary).Return(summary);
            var convert = new NullCollapseConverter();
            var result = convert.Convert(model.ResultsSummary, null, "invert", CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Collapsed, result);
        }


        [TestMethod]
        public void ValidationViewModelCollapseHasResultsConverterInvertNegativeTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.ResultsSummary).Return(null);
            var convert = new NullCollapseConverter();
            var result = convert.Convert(model.ResultsSummary, null, "invert", CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Visible, result);
        }

        [TestMethod]
        public void ValidationViewModelHiddenHasResultsConverterPositiveTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.ResultsSummary).Return(summary);
            var convert = new NullHiddenConverter();
            var result = convert.Convert(model.ResultsSummary, null, "invert", CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Hidden, result);
        }


        [TestMethod]
        public void ValidationViewModelHiddenHasResultsConverterNegativeTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.ResultsSummary).Return(null);
            var convert = new NullHiddenConverter();
            var result = convert.Convert(model.ResultsSummary, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Hidden, result);
        }


        [TestMethod]
        public void ValidationViewModelHiddenHasResultsConverterInvertPositiveTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.ResultsSummary).Return(summary);
            var convert = new NullHiddenConverter();
            var result = convert.Convert(model.ResultsSummary, null, "invert", CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Hidden, result);
        }


        [TestMethod]
        public void ValidationViewModelHiddenHasResultsConverterInvertNegativeTest()
        {
            var model = MockRepository.GenerateStub<IValidationViewModel>();
            model.Stub(m => m.ResultsSummary).Return(null);
            var convert = new NullHiddenConverter();
            var result = convert.Convert(model.ResultsSummary, null, "invert", CultureInfo.CurrentCulture);
            Assert.AreEqual(Visibility.Visible, result);
        }
    }

}
