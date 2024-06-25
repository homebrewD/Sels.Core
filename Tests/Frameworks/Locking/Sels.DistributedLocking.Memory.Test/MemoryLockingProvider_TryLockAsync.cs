using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Sels.Core.Extensions.Conversion;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class MemoryLockingProvider_TryLockAsync
    {
        [TestCase("Jens")]
        [TestCase("Thread.1")]
        [TestCase("Database.Deployer")]
        [TestCase("BackgroundJob.56")]
        [TestCase("sels.homebrewit.csautomatron")]
        public async Task NewResourceIsLockedByExpectedRequester(string requester)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using  var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", requester);

            // Assert
            Assert.IsTrue(lockResult.Success);
            Assert.IsNotNull(requester);
            Assert.IsNotNull(lockResult.AcquiredLock);
            Assert.That(lockResult.AcquiredLock.Resource, Is.EqualTo("Resource"));
            Assert.That(lockResult.AcquiredLock.LockedBy, Is.EqualTo(requester));
            Assert.That(lockResult.AcquiredLock.ExpiryDate, Is.Null);
        }

        [Test, Timeout(60000)]
        public async Task ShouldNotAcquireLockTwice()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using  var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Requester.1");
            var lockResultTwo = await provider.TryLockAsync("Resource", "Requester.2");

            // Assert
            Assert.IsTrue(lockResult.Success);
            Assert.IsNotNull(lockResult.AcquiredLock);
            Assert.IsFalse(lockResultTwo.Success);
            Assert.IsNull(lockResultTwo.AcquiredLock);
        }

        [Test, Timeout(60000)]
        public async Task OnlyOneThreadLocksResourceWhenMultipleLocksAreCalledInParallel()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using  var provider = new MemoryLockingProvider(options);
            var threads = 500;
            List<Task<bool>> _tasks = new List<Task<bool>>();

            // Act
            for(int i = 0; i < threads; i++)
            {
                var currentI = i;
                _tasks.Add(Task.Run(async () => (await provider.TryLockAsync("Resource", $"Thread.{currentI}")).Success));
            }
            var results = await Task.WhenAll(_tasks);

            // Assert
            Assert.IsTrue(results.Length == threads);
            Assert.That(results.Count(x => x), Is.EqualTo(1));
        }

        [Test, Timeout(60000)]
        public async Task TrueIsReturnedWhenRequesterLocksResourceItAlreadyHolds()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Requester.1");
            var lockResultTwo = await provider.TryLockAsync("Resource", "Requester.1");

            // Assert
            Assert.IsTrue(lockResult.Success);
            Assert.IsNotNull(lockResult.AcquiredLock);
            Assert.IsTrue(lockResultTwo.Success);
            Assert.IsNotNull(lockResultTwo.AcquiredLock);
        }

        [Test, Timeout(60000)]
        public async Task ResourceCanBeLockedAfterUnlocking()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using  var provider = new MemoryLockingProvider(options);

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Requester.1");
            await lockResult.AcquiredLock.UnlockAsync();
            var lockResultTwo = await provider.TryLockAsync("Resource", "Requester.2");

            // Assert
            Assert.IsTrue(lockResult.Success);
            Assert.IsNotNull(lockResult.AcquiredLock);
            Assert.That(lockResult.AcquiredLock.LockedBy, Is.EqualTo("Requester.1"));
            Assert.That(lockResult.AcquiredLock.Resource, Is.EqualTo("Resource"));
            Assert.That(lockResult.AcquiredLock.ExpiryDate, Is.Null);
            Assert.IsTrue(lockResultTwo.Success);
            Assert.IsNotNull(lockResultTwo.AcquiredLock);
            Assert.That(lockResultTwo.AcquiredLock.LockedBy, Is.EqualTo("Requester.2"));
            Assert.That(lockResultTwo.AcquiredLock.Resource, Is.EqualTo("Resource"));
            Assert.That(lockResultTwo.AcquiredLock.ExpiryDate, Is.Null);
        }

        [Test, Timeout(60000)]
        public async Task ResourceCanBeLockedAfterExpiring()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using  var provider = new MemoryLockingProvider(options);
            var expiryTime = 500;

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(expiryTime));
            await Helper.Async.Sleep(expiryTime * 2);
            var lockResultTwo = await provider.TryLockAsync("Resource", "Requester.2");

            // Assert
            Assert.IsTrue(lockResult.Success);
            Assert.IsNotNull(lockResult.AcquiredLock);
            Assert.That(lockResult.AcquiredLock.LockedBy, Is.EqualTo("Requester.1"));
            Assert.IsTrue(lockResultTwo.Success);
            Assert.IsNotNull(lockResultTwo.AcquiredLock);
            Assert.That(lockResultTwo.AcquiredLock.LockedBy, Is.EqualTo("Requester.2"));
        }

        [Test, Timeout(60000)]
        public async Task LockExpiryDateIsExtendedWhenKeepAliveIsEnabled()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using  var provider = new MemoryLockingProvider(options);
            var expiryTime = 100;

            // Act
            var lockResult = await provider.TryLockAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(expiryTime), true);
            var initialExpiry = lockResult.AcquiredLock.ExpiryDate;
            await Helper.Async.Sleep(expiryTime * 2);
            var currentExpiry = lockResult.AcquiredLock.ExpiryDate;

            // Assert
            Assert.That(initialExpiry, Is.Not.EqualTo(currentExpiry));
            Assert.That(currentExpiry, Is.GreaterThan(initialExpiry));
        }

        [Test, Timeout(60000)]
        public async Task LockRequestTakesPriorityOverDirectCalls()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock(x => x.ExpiryOffset = 10000000);
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var resultOne = (await lockingProvider.TryLockAsync("Resource", "Requester.1", TimeSpan.FromMilliseconds(100), false)).Success;
            var lockResultTask = lockingProvider.LockAndWaitAsync("Resource", "Requester.2");
            await Helper.Async.Sleep(100 * 2);
            var resultTwo = (await lockingProvider.TryLockAsync("Resource", "Requester.3")).Success;

            // Assert
            Assert.IsTrue(resultOne);
            Assert.IsFalse(resultTwo);
            Assert.IsNotNull(lockResultTask);
            Assert.IsTrue(lockResultTask.IsCompleted);
            var lockResult = await lockResultTask;
            Assert.IsNotNull(lockResult);
            Assert.That(lockResult.Resource, Is.EqualTo("Resource"));
            Assert.That(lockResult.LockedBy, Is.EqualTo("Requester.2"));
            Assert.IsNull(lockResult.ExpiryDate);
        }
    }
}
