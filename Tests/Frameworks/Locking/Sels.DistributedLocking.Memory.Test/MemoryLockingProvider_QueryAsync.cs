using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class MemoryLockingProvider_QueryAsync
    {
        [TestCase("System.", new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" }, 2)]
        [TestCase("RecurringJob", new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" }, 1)]
        [TestCase("Database.Deployment", new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" }, 0)]
        public async Task FilterOnResourceIsAppliedAndCorrectNumberOfResultsAreReturned(string filter, string[] locks, int expected)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, "Me");
            }

            // Act
            var result = await provider.QueryAsync(x => x.WithFilterOnResource(filter));

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(expected));
            foreach (var lockResult in result.Results)
            {
                Assert.That(lockResult, Is.Not.Null);
                Assert.Contains(lockResult.Resource, locks);
            }
        }

        [TestCase("Thread", new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" }, 2)]
        [TestCase("Thread.2", new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" }, 1)]
        [TestCase("Thread.3", new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" }, 0)]
        [TestCase("RecurringJob", new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" }, 1)]
        [TestCase("Me", new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" }, 0)]
        public async Task FilterOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned(string filter, string[] requesters, int expected)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var requester in requesters)
            {
                await provider.TryLockAsync(Guid.NewGuid().ToString(), requester);
            }

            // Act
            var result = await provider.QueryAsync(x => x.WithFilterOnLockedBy(filter));

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(expected));
            foreach (var lockResult in result.Results)
            {
                Assert.That(lockResult, Is.Not.Null);
                Assert.Contains(lockResult.LockedBy, requesters);
            }
        }

        [TestCase("Thread.1", new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", null }, 2)]
        [TestCase(null, new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", null }, 2)]
        [TestCase("Thread.3", new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", null }, 0)]
        [TestCase("BackgroundJob.2", new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", null }, 1)]
        [TestCase("Me", new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", null }, 0)]
        public async Task EqualToOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned(string? equalTo, string?[] requesters, int expected)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            foreach (var requester in requesters)
            {
                if(requester == null)
                {
                    var @lock = await lockingProvider.LockAndWaitAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                    await @lock.DisposeAsync();
                }
                else
                {
                    await provider.TryLockAsync(Guid.NewGuid().ToString(), requester);
                }
            }

            // Act
            var result = await provider.QueryAsync(x => x.WithLockedByEqualTo(equalTo));

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(expected));
            foreach (var lockResult in result.Results)
            {
                Assert.That(lockResult, Is.Not.Null);
            }
        }

        [TestCase(5, 0, 4)]
        [TestCase(3, 1, 1)]
        [TestCase(10, 5, 4)]
        public async Task FilterOnPendingRequestsIsAppliedAndCorrectNumberOfResultsAreReturned(int amountOfLocks, int threshold, int expected)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            for(int i = 0; i < amountOfLocks; i++)
            {
                await provider.LockAsync($"Resource.{i}", Guid.NewGuid().ToString());
                for (int y = 0; y < i; y++)
                {
                    _ = provider.LockAsync($"Resource.{i}", $"Requester,{y}");
                }
            }
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));

            // Act
            var result = await provider.QueryAsync(x => x.WithPendingRequestsLargerThan(threshold));

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(expected));
        }

        [TestCase(10, 5)]
        [TestCase(1, 0)]
        [TestCase(100, 10)]
        public async Task CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyExpired(int amountOfLocks, int amountOfExpiredLocks)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            for (int i = 0; i < amountOfLocks; i++)
            {
                await provider.LockAsync($"Resource.{i}", Guid.NewGuid().ToString(), i < amountOfExpiredLocks ? TimeSpan.Zero : TimeSpan.FromHours(24));
            }
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));

            // Act
            var result = await provider.QueryAsync(x => x.WithOnlyExpired());

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(amountOfExpiredLocks));
        }

        [TestCase(10, 5)]
        [TestCase(1, 0)]
        [TestCase(100, 10)]
        public async Task CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyNotExpired(int amountOfLocks, int amountOfExpiredLocks)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);

            for (int i = 0; i < amountOfLocks; i++)
            {
                await provider.LockAsync($"Resource.{i}", Guid.NewGuid().ToString(), i < amountOfExpiredLocks ? TimeSpan.Zero : TimeSpan.FromHours(24));
            }
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));

            // Act
            var result = await provider.QueryAsync(x => x.WithOnlyNotExpired());

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(amountOfLocks-amountOfExpiredLocks));
        }

        [Test, Timeout(60000)]
        public async Task ReturnsAllLocksWhenNoFilterApplied()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var locks = new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" };
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, "Me");
            }

            // Act
            var result = await provider.QueryAsync(x => { });

            // Assert
            Assert.That(result, Is.Not.Null);
            foreach (var lockResult in result.Results)
            {
                Assert.That(lockResult, Is.Not.Null);
                Assert.Contains(lockResult.Resource, locks);
            }
        }

        [TestCase(1, 2, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { "1", "2" })]
        [TestCase(1, 100, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        [TestCase(2, 5, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { "6", "7", "8", "9" })]
        [TestCase(6, 2, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }, new string[] { })]
        public async Task ReturnsCorrectLocksWhenPaginationIsApplied(int page, int pageSize, string[] locks, string[] expected)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, "Me");
            }

            // Act
            var result = await provider.QueryAsync(x => x.WithPagination(page, pageSize));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results.Select(x => x.Resource), Is.EqualTo(expected).AsCollection);
        }

        [TestCase(new string[] { "1", "2", "3", "4", "5" }, new string[] { "1","2","3","4", "5" } , false)]
        [TestCase(new string[] { "1", "2", "3", "4", "5" }, new string[] { "5", "4", "3", "2", "1" }, true)]
        public async Task CorrectSortingIsAppliedWhenSortingOnResource(string[] locks, string[] expected, bool sortDescending)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in locks)
            {
                await provider.TryLockAsync(@lock, Guid.NewGuid().ToString());
            }

            // Act
            var results = await provider.QueryAsync(x => x.OrderByResource(sortDescending));

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Results.Select(x => x.Resource), Is.EqualTo(expected).AsCollection);
        }

        [TestCase(new string?[] { "1", "2", "3", "4", "5", null }, new string?[] { null, "1", "2", "3", "4", "5" }, false)]
        [TestCase(new string?[] { "1", "2", "3", "4", "5", null }, new string?[] { "5", "4", "3", "2", "1", null }, true)]
        public async Task CorrectSortingIsAppliedWhenSortingOnLockedBy(string?[] requesters, string?[] expected, bool sortDescending)
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            foreach (var @lock in requesters)
            {
                // Unlock lock to simulate empty LockedBy property
                if(@lock == null)
                {
                    var lockResult = await provider.TryLockAsync(Guid.NewGuid().ToString(), "Nullable");
                    await lockResult.AcquiredLock.DisposeAsync();
                }
                else
                {
                    await provider.TryLockAsync(Guid.NewGuid().ToString(), @lock);
                }
                
            }

            // Act
            var results = await provider.QueryAsync(x => x.OrderByLockedBy(sortDescending));

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Results.Select(x => x.LockedBy), Is.EqualTo(expected).AsCollection);
        }
        [Test, Timeout(60000)]
        public async Task CorrectSortingIsAppliedWhenSortingOnLastLockDate()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            foreach (var i in Enumerable.Range(0, 100))
            {
                await using(await lockingProvider.LockAndWaitAsync(i.ToString(), "Random"))
                {

                }
            }

            // Act
            var results = await provider.QueryAsync(x => x.OrderByLastLockDate());

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Results.Select(x => x.Resource), Is.EqualTo(Enumerable.Range(0, 100).Select(x => x.ToString())).AsCollection);
        }
        [Test, Timeout(60000)]
        public async Task CorrectSortingIsAppliedWhenSortingOnLockDate()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            foreach (var i in Enumerable.Range(0, 100))
            {
                _ = await lockingProvider.LockAndWaitAsync(i.ToString(), "Random");
            }

            // Act
            var results = await provider.QueryAsync(x => x.OrderByLockedAt());

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Results.Select(x => x.Resource), Is.EqualTo(Enumerable.Range(0, 100).Select(x => x.ToString())).AsCollection);
        }
        [Test, Timeout(60000)]
        public async Task CorrectSortingIsAppliedWhenSortingOnExpiryDate()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            await using var provider = new MemoryLockingProvider(options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            foreach (var i in Enumerable.Range(0, 100))
            {
                _ = await lockingProvider.LockAndWaitAsync(i.ToString(), "Random", TimeSpan.FromMilliseconds(10));
                await Helper.Async.Sleep(TimeSpan.FromMilliseconds(5));
            }

            // Act
            var results = await provider.QueryAsync(x => x.OrderByLockedAt());

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Results.Select(x => x.Resource), Is.EqualTo(Enumerable.Range(0, 100).Select(x => x.ToString())).AsCollection);
        }
    }
}
