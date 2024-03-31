using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Test.Components
{
    public class Notifier_RaiseEventAsync
    {
        [Test, Timeout(60000)]
        public async Task EventIsRaisedToGlobalGenericListener()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listenerMock = new Mock<IEventListener<string>>();
            listenerMock.Setup(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var provider = TestHelper.GetTestContainer(x => x.AddEventListener(listenerMock.Object));
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            listenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<string>(x => x.Equals(@event)), It.IsAny<CancellationToken>()));
        }

        [Test]
        [Timeout(60000)]
        public async Task EventIsRaisedToGlobalListener()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listenerMock = new Mock<IEventListener>();
            listenerMock.Setup(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var provider = TestHelper.GetTestContainer(x => x.AddEventListener(listenerMock.Object));
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            listenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<object>(x => x.Equals(@event)), It.IsAny<CancellationToken>()));
        }

        [Test]
        [Timeout(60000)]
        public async Task EventIsRaisedToRuntimeGenericListener()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listenerMock = new Mock<IEventListener<string>>();
            listenerMock.Setup(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber<string>>();

            // Act
            using var subscription = subscriber.Subscribe(listenerMock.Object);
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            listenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<string>(x => x.Equals(@event)), It.IsAny<CancellationToken>()));
        }

        [Test]
        [Timeout(60000)]
        public async Task EventIsRaisedToRuntimeListener()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listenerMock = new Mock<IEventListener>();
            listenerMock.Setup(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber>();

            // Act
            using var subscription = subscriber.Subscribe(listenerMock.Object);
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            listenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<object>(x => x.Equals(@event)), It.IsAny<CancellationToken>()));
        }

        [Test]
        [Timeout(60000)]
        public async Task UnsubscribingStopsGenericRuntimeListenerFromReceivingEvents()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listenerMock = new Mock<IEventListener<string>>();
            listenerMock.Setup(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber<string>>();

            // Act
            var subscription = subscriber.Subscribe(listenerMock.Object);
            subscription.Dispose();
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.That(result, Is.EqualTo(0));
            listenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<string>(x => x.Equals(@event)), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Timeout(60000)]
        public async Task UnsubscribingStopsRuntimeListenerFromReceivingEvents()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listenerMock = new Mock<IEventListener>();
            listenerMock.Setup(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber>();

            // Act
            var subscription = subscriber.Subscribe(listenerMock.Object);
            subscription.Dispose();
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.That(result, Is.EqualTo(0));
            listenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<string>(x => x.Equals(@event)), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Timeout(60000)]
        public async Task EnlistedEventIsAlsoRaised()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var secondEvent = 1;
            var listenerMock = new Mock<IEventListener>();
            listenerMock.Setup(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var provider = TestHelper.GetTestContainer(x => x.AddEventListener(listenerMock.Object));
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event, x => x.Enlist(secondEvent));

            // Assert
            Assert.That(result, Is.EqualTo(2));
            listenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<string>(x => x.Equals(@event)), It.IsAny<CancellationToken>()), Times.Once);
            listenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<int>(x => x.Equals(secondEvent)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [Timeout(60000)]
        public async Task EnlistedEventRaisedByListenerIsAlsoRaised()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var secondEvent = 1;
            var listenerMock = new Mock<IEventListener<int>>();
            listenerMock.Setup(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var provider = TestHelper.GetTestContainer(x => x.AddEventListener((c, e, t) =>
            {
                c.EnlistEvent(secondEvent);
                return Task.CompletedTask;
            }).AddEventListener(listenerMock.Object));
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.That(result, Is.EqualTo(2));
            listenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<int>(x => x.Equals(secondEvent)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [Timeout(60000)]
        public async Task EventIsAlsoRaisedAsBaseClass()
        {
            // Arrange
            var @event = new List<string>() { "Hello I'm an event" };
            var listenerMock = new Mock<IEventListener<List<string>>>();
            listenerMock.Setup(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var baseListenerMock = new Mock<IEventListener<IEnumerable<string>>>();
            baseListenerMock.Setup(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var provider = TestHelper.GetTestContainer(x => x.AddEventListener(listenerMock.Object).AddEventListener(baseListenerMock.Object));
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event, x => x.AlsoRaiseAs<IEnumerable<string>>());

            // Assert
            Assert.That(result, Is.EqualTo(2));
            listenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<List<string>>(x => x.Equals(@event)), It.IsAny<CancellationToken>()), Times.Once);
            baseListenerMock.Verify(x => x.HandleAsync(It.IsAny<IEventListenerContext>(), It.Is<IEnumerable<string>>(x => x.Equals(@event)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [Timeout(60000)]
        public async Task CancellingTokenCancelsEvent()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber>();
            Exception exception = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Act
            using var subscription = subscriber.Subscribe((x, e, t) => Task.Delay(5000, t));
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RaiseEventAsync(this, @event, tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
        }

        [Test]
        [Timeout(60000)]
        public async Task ExceptionsAreIgnoredWhenOptionIsEnabled()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber>();
            Exception exception = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Act
            using var subscription = subscriber.Subscribe((x, e, t) => Task.Delay(5000, t));
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RaiseEventAsync(this, @event, x => x.WithOptions(EventOptions.IgnoreExceptions), tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
        }

        [Test]
        [Timeout(60000)]
        public async Task FireAndForgetTaskIsLaunchedWhenOptionIsEnabled()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber>();
            Exception exception = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Act
            using var subscription = subscriber.Subscribe((x, e, t) => Task.Delay(5000, t));
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RaiseEventAsync(this, @event, x => x.WithOptions(EventOptions.FireAndForget), tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
        }

        [Test]
        [Timeout(60000)]
        public async Task EventListenersAreCalledInTheCorrectOrder()
        {
            // Arrange
            var priorities = new byte?[] { null, 10, 3, 4, 0, 1, null };
            var expected = new uint?[] { 0, 1, 3, 4, 10, null, null };
            var @event = "Hi I'm an event";
            var results = new List<uint?>();
            var provider = TestHelper.GetTestContainer(x =>
            {
                foreach (var priority in priorities)
                {
                    var eventPriority = priority;
                    x.AddEventListener((c, e, t) =>
                    {
                        results.Add(eventPriority);
                        return Task.CompletedTask;
                    }, eventPriority);
                }
            });
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.That(result, Is.EqualTo(priorities.Length));
            Assert.That(results, Is.EqualTo(expected).AsCollection);
        }

        [TestCase(1)]
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(100)]
        [Timeout(60000)]
        public async Task CorrectAmountOfListenersAreExecutedInParallel(int amount)
        {
            // Arrange
            var @event = "Hi I'm an event";
            var provider = TestHelper.GetTestContainer(x =>
            {
                foreach (var i in Enumerable.Range(1, amount))
                {
                    x.AddEventListener((c, e, t) => c.WaitForCommitAsync());
                }
            });
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event, x => x.WithOptions(EventOptions.AllowParallelExecution));

            // Assert
            Assert.That(result, Is.EqualTo(amount));
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(10)]
        [Timeout(60000)]
        public async Task EventGetsRaisedCorrectlyWhenListenersWaitForTransaction(int amount)
        {
            // Arrange
            var @event = "Hi I'm an event";
            var provider = TestHelper.GetTestContainer(x =>
            {
                foreach (var i in Enumerable.Range(1, amount))
                {
                    var waitForTransaction = i % 2 == 0;
                    x.AddEventListener(async (c, e, t) => {
                        if (waitForTransaction) await c.WaitForCommitAsync();
                    });
                }
            });
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event, x => x.WithOptions(EventOptions.AllowParallelExecution));

            // Assert
            Assert.That(result, Is.EqualTo(amount));
        }

        [Test]
        [Timeout(60000)]
        public async Task EventTransactionGetsCancelledWhenAnyEventListenerFails()
        {
            // Arrange
            var @event = "Hi I'm an event";
            bool completedAfterTransaction = false;
            var provider = TestHelper.GetTestContainer(x =>
            {
                x.AddEventListener(async (c, e, t) =>
                {
                    await c.WaitForCommitAsync();
                    completedAfterTransaction = true;
                }, 0)
                .AddEventListener((c, e, t) =>
                {
                    throw new InvalidOperationException();
                }, 1).AddEventListener(async (c, e, t) =>
                {
                    await c.WaitForCommitAsync();
                    completedAfterTransaction = true;
                }, null);
            });
            var notifier = provider.GetRequiredService<INotifier>();
            Exception exception = null;

            // Act
            try
            {
                await notifier.RaiseEventAsync(this, @event, x => x.WithOptions(EventOptions.AllowParallelExecution));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsFalse(completedAfterTransaction);
        }
    }
}
