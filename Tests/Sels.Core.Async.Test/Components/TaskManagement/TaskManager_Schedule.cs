﻿using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Threading;
using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.Test.Components.TaskManagement
{
    public class TaskManager_Schedule
    {
        [Test, Timeout(10000)]
        public async Task TaskIsScheduledAndExecuted()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool executed = false;

            // Act
            var scheduledTask = taskManager.Schedule(this, () => executed = true);

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.IsTrue(executed);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(69)]
        [TestCase(420)]
        [Timeout(10000)]
        public async Task CorrectOutputIsReturnedFromTask(int output)
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.Schedule(this, () => output);

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<int>();
            Assert.AreEqual(output, actual);
        }

        [Test,Timeout(60000)]
        public async Task CorrectExceptionIsReturnedFromTask()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => throw new DivideByZeroException());

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.IsNotNull(actual);
            Assert.That(actual, Is.AssignableTo<DivideByZeroException>());
        }

        [Test, Timeout(10000)]
        public async Task TaskIsCancelledWhenTokenGetsCancelled()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            var tokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.Delay(TimeSpan.FromMinutes(1), tokenSource.Token), token: tokenSource.Token);
            tokenSource.Cancel();

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            exception = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.IsNotNull(exception);
            Assert.That(exception, Is.AssignableTo<OperationCanceledException>());
        }

        [Test, Timeout(10000)]
        public async Task PreAndPostExecutionAreExecuted()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            bool preExecuted = false;
            bool executed = false;
            bool postExecuted = false;

            // Act
            var scheduledTask = taskManager.Schedule(this, () => executed = true, x => x.ExecuteFirst(() => preExecuted = true).ExecuteAfter(() => postExecuted = true));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.IsTrue(preExecuted);
            Assert.IsTrue(executed);
            Assert.IsTrue(postExecuted);
        }

        [Test, Timeout(10000)]
        public async Task ExceptionThrownInPreActionIsCaptured()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.ExecuteFirst(() => throw new AbandonedMutexException()));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.IsNotNull(actual);
            Assert.That(actual, Is.AssignableTo<AbandonedMutexException>());
        }

        [Test, Timeout(10000)]
        public async Task ExceptionThrownInPostActionIsCaptured()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.ExecuteAfter(() => throw new AbandonedMutexException()));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            var actual = scheduledTask.Result.CastToOrDefault<Exception>();
            Assert.IsNotNull(actual);
            Assert.That(actual, Is.AssignableTo<AbandonedMutexException>());
        }

        [TestCase(ManagedTaskOptions.GracefulCancellation)]
        [TestCase(ManagedTaskOptions.AutoRestart | ManagedTaskOptions.KeepAlive)]
        [TestCase(ManagedTaskOptions.None)]
        [Timeout(10000)]
        public async Task TaskIsScheduledWithExpectedManagedTaskOptions(ManagedTaskOptions taskOptions)
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.WithManagedOptions(taskOptions));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.AreEqual(taskOptions, scheduledTask.Options);
        }

        [Test, Timeout(10000)]
        public async Task ManagedContinuationIsTriggered()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.ScheduleAction(this, () => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.That(scheduledTask.Continuations.Length, Is.EqualTo(1));
            Assert.That(scheduledTask.Continuations[0], Is.EqualTo(managedTask));
        }

        [Test, Timeout(10000)]
        public async Task ManagedNamedContinuationIsTriggered()
        {
            // Arrange
            const string TaskName = "Hello";
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedTask managedTask = null;

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.ContinueWith(async (s, m, r, c) =>
            {
                managedTask = await s.ScheduleActionAsync(this, TaskName, false, () => Task.CompletedTask, x => x.WithPolicy(NamedManagedTaskPolicy.WaitAndStart));
                return managedTask;
            }));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.That(scheduledTask.Continuations.Length, Is.EqualTo(1));
            Assert.That(scheduledTask.Continuations[0], Is.EqualTo(managedTask));
            Assert.That(scheduledTask.Continuations[0].Name, Is.EqualTo(TaskName));
        }

        [Test, Timeout(10000)]
        public async Task AnonymousContinuationIsTriggered()
        {
            // Arrange
            await using var serviceProvider = TestHelper.GetTaskManagerContainer();
            await using var scope = serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var taskManager = provider.GetRequiredService<ITaskManager>();
            IManagedAnonymousTask managedTask = null;

            // Act
            var scheduledTask = taskManager.ScheduleAction(this, () => Task.CompletedTask, x => x.ContinueWith((s, m, r, c) =>
            {
                managedTask = s.ScheduleAnonymousAction(() => Task.CompletedTask);
                return managedTask;
            }));

            // Assert
            Assert.IsNotNull(scheduledTask);
            Assert.IsNotNull(scheduledTask.OnExecuted);
            await scheduledTask.OnExecuted;
            Assert.That(scheduledTask.AnonymousContinuations.Length, Is.EqualTo(1));
            Assert.That(scheduledTask.AnonymousContinuations[0], Is.EqualTo(managedTask));
        }
    }
}
