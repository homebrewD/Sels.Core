using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.SQL.Test
{
    public class SqlLockingProvider_TryLockAsync
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
            var resource = "Resource";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = requester,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryLockAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
            });
            await using var provider = new SqlLockingProvider(TestHelper.GetNotifierMock().Object, TestHelper.GetSubscriberMock().Object, TestHelper.GetManagerMock().Object, repositoryMock.Object, options);

            // Act
            var lockResult = await provider.TryLockAsync(resource, requester);

            // Assert
            Assert.IsTrue(lockResult.Success);
            Assert.IsNotNull(requester);
            Assert.IsNotNull(lockResult.AcquiredLock);
            Assert.That(lockResult.AcquiredLock.Resource, Is.EqualTo(resource));
            Assert.That(lockResult.AcquiredLock.LockedBy, Is.EqualTo(requester));
            Assert.That(lockResult.AcquiredLock.ExpiryDate, Is.Null);
        }

        [Test, Timeout(60000)]
        public async Task ShouldNotAcquireLockTwice()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var resource = "Resource";
            var holder = "Holder";
            var requester = "Requester";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = holder,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryLockAsync(It.IsAny<IRepositoryTransaction>(), resource, It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
            });
            await using var provider = new SqlLockingProvider(TestHelper.GetNotifierMock().Object, TestHelper.GetSubscriberMock().Object, TestHelper.GetManagerMock().Object, repositoryMock.Object, options);

            // Act
            var lockResult = await provider.TryLockAsync(resource, holder);
            var lockResultTwo = await provider.TryLockAsync(resource, requester);

            // Assert
            Assert.IsTrue(lockResult.Success);
            Assert.IsNotNull(lockResult.AcquiredLock);
            Assert.IsFalse(lockResultTwo.Success);
            Assert.IsNull(lockResultTwo.AcquiredLock);
        }

        [Test, Timeout(60000)]
        public async Task TrueIsReturnedWhenRequesterLocksResourceItAlreadyHolds()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var resource = "Resource";
            var requester = "Requester";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = requester,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.TryLockAsync(It.IsAny<IRepositoryTransaction>(), resource, requester, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLock);
            });
            await using var provider = new SqlLockingProvider(TestHelper.GetNotifierMock().Object, TestHelper.GetSubscriberMock().Object, TestHelper.GetManagerMock().Object, repositoryMock.Object, options);

            // Act
            var lockResult = await provider.TryLockAsync(resource, requester);
            var lockResultTwo = await provider.TryLockAsync(resource, requester);

            // Assert
            Assert.IsTrue(lockResult.Success);
            Assert.IsNotNull(lockResult.AcquiredLock);
            Assert.IsTrue(lockResultTwo.Success);
            Assert.IsNotNull(lockResultTwo.AcquiredLock);
        }
    }
}
