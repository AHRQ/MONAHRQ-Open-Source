using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Integration.Castle.Aspects;
using Monahrq.TestSupport;
using Rhino.Mocks;


namespace Monahrq.Infrastructure.Test.Integration

{
    [TestClass]
    public class LogAspectTest:MefTestFixture   
    {
        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get { return new Type[] {typeof (LogAspectTarget), typeof (HoldsLogAspectTarget)}; }
        }

        protected override void ComposeFixtureInstances()
        {
           base.ComposeFixtureInstances();
       
        }

        [TestMethod]
        public void TestMethod1()
        {
            var Logger = MockRepository.GenerateMock<ILogWriter>();

    


            Container.ComposeExportedValue<ILogWriter>(Logger);
            var logger = Container.GetExportedValue<ILogWriter>();

            LogAspectAttribute.SetLogger(logger);

            var target = Container.GetExportedValue<HoldsLogAspectTarget>();

            target.InvokeLogedMethod();

            
            Assert.AreSame(logger,Logger);
            Logger.AssertWasCalled(l=>l.Write(string.Empty));
        }


    }

    [AspectsStatusExport(typeof(LogAspectTarget), AreAspectsEnabled = true)]
    [LogAspect]
    public class LogAspectTarget
    {
        private int test = 1;

        public LogAspectTarget ()
        {
            test = 5;
        }

        [Log]
        public void LogMethod()
        {
            var i = 0;
        }
    }

    [Export]
    public class HoldsLogAspectTarget
    {
        [Import]
        public virtual LogAspectTarget target { get; set; }

        public virtual void InvokeLogedMethod()
        {
            target.LogMethod();
        }
    }
}
