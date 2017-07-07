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
using Microsoft.Practices.Prism.Interactivity;
using Microsoft.Practices.Prism.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace Prism.Interactivity.Tests
{
    [TestClass]
    public class InvokeCommandActionFixture
    {
        [TestMethod]
        public void WhenCommandPropertyIsSet_ThenHooksUpCommandBehavior()
        {
            var someControl = new TextBox();
            var commandAction = new InvokeCommandAction();
            var command = new MockCommand();
            commandAction.Attach(someControl);
            commandAction.Command = command;

            Assert.IsFalse(command.ExecuteCalled);

            commandAction.Invoke();

            Assert.IsTrue(command.ExecuteCalled);
            Assert.AreSame(command, commandAction.GetValue(InvokeCommandAction.CommandProperty));
        }

        [TestMethod]
        public void WhenAttachedAfterCommandPropertyIsSetAndInvoked_ThenInvokesCommand()
        {
            var someControl = new TextBox();
            var commandAction = new InvokeCommandAction();
            var command = new MockCommand();
            commandAction.Command = command;
            commandAction.Attach(someControl);

            Assert.IsFalse(command.ExecuteCalled);

            commandAction.Invoke();

            Assert.IsTrue(command.ExecuteCalled);
            Assert.AreSame(command, commandAction.GetValue(InvokeCommandAction.CommandProperty));
        }

        [TestMethod]
        public void WhenChangingProperty_ThenUpdatesCommand()
        {
            var someControl = new TextBox();
            var oldCommand = new MockCommand();
            var newCommand = new MockCommand();
            var commandAction = new InvokeCommandAction();
            commandAction.Attach(someControl);
            commandAction.Command = oldCommand;
            commandAction.Command = newCommand;
            commandAction.Invoke();

            Assert.IsTrue(newCommand.ExecuteCalled);
            Assert.IsFalse(oldCommand.ExecuteCalled);
        }

        [TestMethod]
        public void WhenInvokedWithCommandParameter_ThenPassesCommandParaeterToExecute()
        {
            var someControl = new TextBox();
            var command = new MockCommand();
            var parameter = new object();
            var commandAction = new InvokeCommandAction();
            commandAction.Attach(someControl);
            commandAction.Command = command;
            commandAction.CommandParameter = parameter;

            Assert.IsNull(command.ExecuteParameter);

            commandAction.Invoke();

            Assert.IsTrue(command.ExecuteCalled);
            Assert.IsNotNull(command.ExecuteParameter);
            Assert.AreSame(parameter, command.ExecuteParameter);
        }

        [TestMethod]
        public void WhenCommandParameterChanged_ThenUpdatesIsEnabledState()
        {
            var someControl = new TextBox();
            var command = new MockCommand();
            var parameter = new object();
            var commandAction = new InvokeCommandAction();
            commandAction.Attach(someControl);
            commandAction.Command = command;

            Assert.IsNull(command.CanExecuteParameter);
            Assert.IsTrue(someControl.IsEnabled);

            command.CanExecuteReturnValue = false;
            commandAction.CommandParameter = parameter;

            Assert.IsNotNull(command.CanExecuteParameter);
            Assert.AreSame(parameter, command.CanExecuteParameter);
            Assert.IsFalse(someControl.IsEnabled);
        }

        [TestMethod]
        public void WhenCanExecuteChanged_ThenUpdatesIsEnabledState()
        {
            var someControl = new TextBox();
            var command = new MockCommand();
            var parameter = new object();
            var commandAction = new InvokeCommandAction();
            commandAction.Attach(someControl);
            commandAction.Command = command;
            commandAction.CommandParameter = parameter;

            Assert.IsTrue(someControl.IsEnabled);

            command.CanExecuteReturnValue = false;
            command.RaiseCanExecuteChanged();

            Assert.IsNotNull(command.CanExecuteParameter);
            Assert.AreSame(parameter, command.CanExecuteParameter);
            Assert.IsFalse(someControl.IsEnabled);
        }

        [TestMethod]
        public void WhenDetatched_ThenSetsCommandAndCommandParameterToNull()
        {
            var someControl = new TextBox();
            var command = new MockCommand();
            var parameter = new object();
            var commandAction = new InvokeCommandAction();
            commandAction.Attach(someControl);
            commandAction.Command = command;
            commandAction.CommandParameter = parameter;

            Assert.IsNotNull(commandAction.Command);
            Assert.IsNotNull(commandAction.CommandParameter);

            commandAction.Detach();

            Assert.IsNull(commandAction.Command);
            Assert.IsNull(commandAction.CommandParameter);
        }

        [TestMethod]
        public void WhenCommandIsSetAndThenBehaviorIsAttached_ThenCommandsCanExecuteIsCalledOnce()
        {
            var someControl = new TextBox();
            var command = new MockCommand();
            var commandAction = new InvokeCommandAction();
            commandAction.Command = command;
            commandAction.Attach(someControl);

            Assert.AreEqual(1, command.CanExecuteTimesCalled);
        }

        [TestMethod]
        public void WhenCommandAndCommandParameterAreSetPriorToBehaviorBeingAttached_ThenCommandIsExecutedCorrectlyOnInvoke()
        {
            var someControl = new TextBox();
            var command = new MockCommand();
            var parameter = new object();
            var commandAction = new InvokeCommandAction();
            commandAction.Command = command;
            commandAction.CommandParameter = parameter;
            commandAction.Attach(someControl);

            commandAction.Invoke();
           
            Assert.IsTrue(command.ExecuteCalled);
        }
    }
}