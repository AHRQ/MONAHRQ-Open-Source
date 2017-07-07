using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using System.Threading;

namespace Monahrq.Sdk.Services.Tasks.Test
{
    [TestClass]
    public class TaskServiceTest
    {
        CancellationTokenSource cancellationTokenSource; 
        TaskServiceCompletionType taskCompletionFlag;
        CountdownEvent caller_countdown;

        // this list is the data to be processed in separate threads
        List<object> items = new List<object>() { 3, 6, 9, 12, 15, 18, 21 };

        #region PARALLEL_TESTS

        [TestMethod]
        public void TestParallelSuccess()
        {
            taskCompletionFlag = TaskServiceCompletionType.Undefined;
            caller_countdown = new CountdownEvent(1);

            var service = new ParallelTaskRunner();
            cancellationTokenSource = service.RunTasks(items, MyWork, MyProgress, MyCompletion, 4);

            caller_countdown.Wait();
            Assert.IsTrue(taskCompletionFlag == TaskServiceCompletionType.Success, string.Format("Completion flag is {0}", taskCompletionFlag)); 
        }

        [TestMethod]
        public void TestParallelException()
        {
            taskCompletionFlag = TaskServiceCompletionType.Undefined;
            caller_countdown = new CountdownEvent(1);

            var service = new ParallelTaskRunner();
            cancellationTokenSource = service.RunTasks(items, MyWorkException, MyProgress, MyCompletion, 4);

            caller_countdown.Wait();
            Assert.IsTrue(taskCompletionFlag == TaskServiceCompletionType.Exception, string.Format("Completion flag is {0}", taskCompletionFlag));
        }

        [TestMethod]
        public void TestParallelCancelled()
        {
            taskCompletionFlag = TaskServiceCompletionType.Undefined;
            caller_countdown = new CountdownEvent(1);

            var service = new ParallelTaskRunner();
            cancellationTokenSource = service.RunTasks(items, MyWorkCancelled, MyProgress, MyCompletion, 4);

            caller_countdown.Wait();
            Assert.IsTrue(taskCompletionFlag == TaskServiceCompletionType.Cancelled, string.Format("Completion flag is {0}", taskCompletionFlag));
        }

        #endregion

        #region SEQUENTIAL_TESTS

        [TestMethod]
        public void TestSequentialSuccess()
        {
            taskCompletionFlag = TaskServiceCompletionType.Undefined;

            // wait for the completion function to be called
            caller_countdown = new CountdownEvent(1);

            var service = new SequentialTaskRunner();
            cancellationTokenSource = service.RunTasks(items, MyWork, MyProgress, MyCompletion, 4);

            caller_countdown.Wait();
            Assert.IsTrue(taskCompletionFlag == TaskServiceCompletionType.Success, string.Format("Completion flag is {0}", taskCompletionFlag));
        }

        [TestMethod]
        public void TestSequentialException()
        {
            taskCompletionFlag = TaskServiceCompletionType.Undefined;

            // wait for the completion function to be called
            caller_countdown = new CountdownEvent(1);

            var service = new SequentialTaskRunner();
            cancellationTokenSource = service.RunTasks(items, MyWorkException, MyProgress, MyCompletion, 4);

            caller_countdown.Wait();
            Assert.IsTrue(taskCompletionFlag == TaskServiceCompletionType.Exception, string.Format("Completion flag is {0}", taskCompletionFlag));
        }

        [TestMethod]
        public void TestSequentialCancelled()
        {
            taskCompletionFlag = TaskServiceCompletionType.Undefined;

            // wait for the completion function to be called
            caller_countdown = new CountdownEvent(1);

            var service = new SequentialTaskRunner();
            cancellationTokenSource = service.RunTasks(items, MyWorkCancelled, MyProgress, MyCompletion, 4);

            caller_countdown.Wait();
            Assert.IsTrue(taskCompletionFlag == TaskServiceCompletionType.Cancelled, string.Format("Completion flag is {0}", taskCompletionFlag));
        }

        #endregion

        #region CALLBACK_FUNCTIONS

        void MyWork(object myType)
        {
        }

        void MyWorkException(object myType)
        {
            throw new Exception("anything");
        }

        void MyWorkCancelled(object myType)
        {
            cancellationTokenSource.Cancel();
        }

        void MyProgress(int percent)
        {
        }

        void MyCompletion(TaskServiceCompletionType flag, Exception ex)
        {
            // set this *before* unblocking the countdown
            taskCompletionFlag = flag;

            // when the task runner calls here, this will unblock the countdown in the unit test function so it can test the expected completion type
            caller_countdown.Signal();
        }

        #endregion
    }
}
