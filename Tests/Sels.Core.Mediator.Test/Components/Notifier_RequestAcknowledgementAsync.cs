using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Mediator.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Test.Components
{
    public class Notifier_RequestAcknowledgementAsync
    {
        public class NullRequest : IRequest
        {

        }

        [Test]
        [Timeout(60000)]
        public async Task RequestIsRaisedToGlobalHandler()
        {
            // Arrange
            var request = new NullRequest();
            var handlerMock = new Mock<IRequestHandler<NullRequest>>();
            handlerMock.Setup(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.IsAny<NullRequest>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.FromResult(RequestAcknowledgement.Acknowledge()));
            var provider = TestHelper.GetTestContainer(x => x.AddRequestHandler(handlerMock.Object));
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAcknowledgementAsync(this, request);

            // Assert
            Assert.That(result.Acknowledged, Is.True);
            handlerMock.Verify(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.Is<NullRequest>(x => x.Equals(request)), It.IsAny<CancellationToken>()));
        }

        [Test]
        [Timeout(60000)]
        public async Task RequestIsRaisedToRuntimeHandler()
        {
            // Arrange
            var request = new NullRequest();
            var handlerMock = new Mock<IRequestHandler<NullRequest>>();
            handlerMock.Setup(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.IsAny<NullRequest>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.FromResult(RequestAcknowledgement.Acknowledge()));
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();

            // Act
            using var subscription = subscriber.Subscribe(handlerMock.Object);
            var result = await notifier.RequestAcknowledgementAsync(this, request);

            // Assert
            Assert.That(result.Acknowledged, Is.True);
            handlerMock.Verify(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.Is<NullRequest>(x => x.Equals(request)), It.IsAny<CancellationToken>()));
        }

        [Test]
        [Timeout(60000)]
        public async Task UnsubscribingStopsRuntimeHandlerFromReceivingRequests()
        {
            // Arrange
            var request = new NullRequest();
            var handlerMock = new Mock<IRequestHandler<NullRequest>>();
            handlerMock.Setup(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.IsAny<NullRequest>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.FromResult(RequestAcknowledgement.Acknowledge()));
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();

            // Act
            var subscription = subscriber.Subscribe(handlerMock.Object);
            subscription.Dispose();
            var result = await notifier.RequestAcknowledgementAsync(this, request);

            // Assert
            Assert.That(result.Acknowledged, Is.False);
            handlerMock.Verify(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.Is<NullRequest>(x => x.Equals(request)), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Timeout(60000)]
        public async Task CancellingTokenCancelsRequest()
        {
            // Arrange
            var request = new NullRequest();
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();
            Exception exception = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Act
            using var subscription = subscriber.Subscribe<NullRequest>(async (x, e, t) =>
            {
                await Task.Delay(5000, t);
                return RequestAcknowledgement.Reject();
            });
            var result = await notifier.RequestAcknowledgementAsync(this, request);
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RequestAcknowledgementAsync(this, request, tokenSource.Token);
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
        public async Task RequestHandlersAreCalledInTheCorrectOrder()
        {
            // Arrange
            var priorities = new byte?[] { null, 10, 3, 4, 0, 1, null };
            var expected = new uint?[] { 0, 1, 3, 4, 10, null, null };
            var results = new List<uint?>();
            var request = new NullRequest();
            var provider = TestHelper.GetTestContainer(x => {
                foreach (var priority in priorities)
                {
                    var handlerPriority = priority;
                    x.AddRequestHandler<NullRequest>((c, e, t) =>
                    {
                        results.Add(handlerPriority);
                        return Task.FromResult(RequestAcknowledgement.Reject());
                    }, handlerPriority);
                }
            });
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAcknowledgementAsync(this, request);

            // Assert
            Assert.That(result.Acknowledged, Is.False);
            Assert.That(results, Is.EqualTo(expected).AsCollection);
        }

        [Test]
        [Timeout(60000)]
        public async Task OnlyOneHandlerCanRespondToRequest()
        {
            // Arrange
            var respondedCount = 0;
            var request = new NullRequest();
            var provider = TestHelper.GetTestContainer(x => {
                x.AddRequestHandler<NullRequest>((c, e, t) =>
                {
                    var response = RequestAcknowledgement.Acknowledge();
                    respondedCount++;
                    return Task.FromResult(response);
                }, null)
                .AddRequestHandler<NullRequest>((c, e, t) =>
                {
                    var response = RequestAcknowledgement.Acknowledge();
                    respondedCount++;
                    return Task.FromResult(response);
                }, 5)
                .AddRequestHandler<NullRequest>((c, e, t) =>
                {
                    var response = RequestAcknowledgement.Acknowledge();
                    respondedCount++;
                    return Task.FromResult(response);
                }, 1);
            });
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAcknowledgementAsync(this, request);

            // Assert
            Assert.That(result.Acknowledged, Is.True);
            Assert.That(respondedCount, Is.EqualTo(1));
        }
    }
}
