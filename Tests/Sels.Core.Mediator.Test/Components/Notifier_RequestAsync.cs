using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Internal.Execution;
using Sels.Core.Mediator.Request;
using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Test.Components
{
    public class Notifier_RequestAsync
    {
        public class NullRequest : IRequest<Null>
        {

        }

        [Test]
        [Timeout(60000)]
        public async Task RequestIsRaisedToGlobalHandler()
        {
            // Arrange
            var request = new NullRequest();
            var handlerMock = new Mock<IRequestHandler<NullRequest, Null>>();
            handlerMock.Setup(x => x.TryRespondAsync(It.IsAny<IRequestHandlerContext>(), It.IsAny<NullRequest>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.FromResult(RequestResponse<Null>.Success(Null.Value)));
            var provider = TestHelper.GetTestContainer(x => x.AddRequestHandler(handlerMock.Object));
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAsync(this, request);

            // Assert
            Assert.That(result.Completed, Is.True);
            handlerMock.Verify(x => x.TryRespondAsync(It.IsAny<IRequestHandlerContext>(), It.Is<NullRequest>(x => x.Equals(request)), It.IsAny<CancellationToken>()));
        }

        [Test]
        [Timeout(60000)]
        public async Task RequestIsRaisedToRuntimeHandler()
        {
            // Arrange
            var request = new NullRequest();
            var handlerMock = new Mock<IRequestHandler<NullRequest, Null>>();
            handlerMock.Setup(x => x.TryRespondAsync(It.IsAny<IRequestHandlerContext>(), It.IsAny<NullRequest>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.FromResult(RequestResponse<Null>.Success(Null.Value)));
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();

            // Act
            using var subscription = subscriber.Subscribe(handlerMock.Object);
            var result = await notifier.RequestAsync(this, request);

            // Assert
            Assert.That(result.Completed, Is.True);
            handlerMock.Verify(x => x.TryRespondAsync(It.IsAny<IRequestHandlerContext>(), It.Is<NullRequest>(x => x.Equals(request)), It.IsAny<CancellationToken>()));
        }

        [Test]
        [Timeout(60000)]
        public async Task UnsubscribingStopsRuntimeHandlerFromReceivingRequests()
        {
            // Arrange
            var request = new NullRequest();
            var handlerMock = new Mock<IRequestHandler<NullRequest, Null>>();
            handlerMock.Setup(x => x.TryRespondAsync(It.IsAny<IRequestHandlerContext>(), It.IsAny<NullRequest>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.FromResult(RequestResponse<Null>.Success(Null.Value)));
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();

            // Act
            var subscription = subscriber.Subscribe(handlerMock.Object);
            subscription.Dispose();
            var result = await notifier.RequestAsync(this, request);

            // Assert
            Assert.That(result.Completed, Is.False);
            handlerMock.Verify(x => x.TryRespondAsync(It.IsAny<IRequestHandlerContext>(), It.Is<NullRequest>(x => x.Equals(request)), It.IsAny<CancellationToken>()), Times.Never);
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
            using var subscription = subscriber.Subscribe<NullRequest, Null>(async (x, e, t) =>
            {
                await Task.Delay(5000, t);
                return RequestResponse<Null>.Reject();
            });
            var result = await notifier.RequestAsync(this, request);
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RequestAsync(this, request, tokenSource.Token);
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
                    x.AddRequestHandler<NullRequest, Null>((c, e, t) =>
                    {
                        results.Add(handlerPriority);
                        return Task.FromResult(RequestResponse<Null>.Reject());
                    }, handlerPriority);
                }
            });
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAsync(this, request);

            // Assert
            Assert.That(result.Completed, Is.False);
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
                x.AddRequestHandler<NullRequest, Null>((c, e, t) =>
                {
                    var response = RequestResponse<Null>.Success(Null.Value);
                    respondedCount++;
                    return Task.FromResult(response);
                }, null)
                .AddRequestHandler< NullRequest, Null>((c, e, t) =>
                {
                    var response = RequestResponse<Null>.Success(Null.Value);
                    respondedCount++;
                    return Task.FromResult(response);
                }, 5)
                .AddRequestHandler<NullRequest, Null>((c, e, t) =>
                {
                    var response = RequestResponse<Null>.Success(Null.Value);
                    respondedCount++;
                    return Task.FromResult(response);
                }, 1);
            });
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAsync(this, request);

            // Assert
            Assert.That(result.Completed, Is.True);
            Assert.That(respondedCount, Is.EqualTo(1));
        }
    }
}
