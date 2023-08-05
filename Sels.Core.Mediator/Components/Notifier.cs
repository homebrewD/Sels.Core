﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using Sels.Core.Mediator.Event;
using Sels.Core.Mediator.Request;
using Sels.Core.Models.Disposables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Mediator
{
    /// <inheritdoc cref="INotifier"/>
    public class Notifier : INotifier
    {
        // Fields
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly IRequestSubscriptionManager _requestSubscriptionManager;
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc cref="Notifier"/>
        /// <param name="eventSubscriber">Manager used to get global listeners</param>
        /// <param name="requestSubscriptionManager">Manager used to get the runtime request handlers</param>
        /// <param name="serviceProvider">Provider used to resolve typed event and request subscribers</param>
        /// <param name="loggerFactory">Optional factory to create loggers for child instances</param>
        /// <param name="logger">Optional logger for tracing</param>
        public Notifier(IEventSubscriber eventSubscriber, IRequestSubscriptionManager requestSubscriptionManager, IServiceProvider serviceProvider, ILoggerFactory loggerFactory = null, ILogger<Notifier> logger = null)
        {
            _eventSubscriber = eventSubscriber.ValidateArgument(nameof(eventSubscriber));
            _serviceProvider = serviceProvider.ValidateArgument(nameof(serviceProvider));
            _requestSubscriptionManager = requestSubscriptionManager.ValidateArgument(nameof(requestSubscriptionManager));
            _loggerFactory = loggerFactory;
            _logger = logger;
        }
        
        /// <inheritdoc/>
        public async Task<int> RaiseEventAsync<TEvent>(object sender, TEvent @event, Action<INotifierEventOptions<TEvent>> eventOptions, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            sender.ValidateArgument(nameof(sender));
            eventOptions.ValidateArgument(nameof(eventOptions));
            @event.ValidateArgument(nameof(@event));

            _logger.Log($"Raising event <{@event}> created by <{sender}>");
            await using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var provider = scope.ServiceProvider;
                var orchestrator = new EventOrchestrator(this, provider, _loggerFactory?.CreateLogger<EventOrchestrator>());
                var options = new NotifierEventOptions<TEvent>(this, orchestrator, sender, @event);
                // Enlist main listeners first
                Enlist(orchestrator, sender, @event);
                // Configure options
                eventOptions(options);

                // Run fire and forget
                if (options.Options.HasFlag(EventOptions.FireAndForget))
                {
                    _logger.Debug($"Fire and forget enabled for event <{@event}> raised by <{sender}>. Starting task");
                    // TODO: Use task manager to gracefully wait for tasks to complete when IoC containers gets disposed.
                    _ = Task.Run(async () => await RaiseEventAsync(orchestrator, sender, @event, options, token).ConfigureAwait(false));
                    return 0;
                }

                // Execute
                return await RaiseEventAsync(orchestrator, sender, @event, options, token).ConfigureAwait(false);
            }
        }

        private async Task<int> RaiseEventAsync<TEvent>(EventOrchestrator orchestrator, object sender, TEvent @event, NotifierEventOptions<TEvent> eventOptions, CancellationToken token)
        {
            using var methodLogger = _logger.TraceMethod(this);
            sender.ValidateArgument(nameof(sender));
            eventOptions.ValidateArgument(nameof(eventOptions));
            @event.ValidateArgument(nameof(@event));

            var ignoreExceptions = eventOptions.Options.HasFlag(EventOptions.IgnoreExceptions);
            var allowParallelExecution = eventOptions.Options.HasFlag(EventOptions.AllowParallelExecution);

            try
            {
                _logger.Log($"Executing event transaction for event <{@event}> created by <{sender}> with any enlisted event listeners");
                return await orchestrator.ExecuteAsync(allowParallelExecution, token).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                _logger.Log($"Something went wrong while raising event <{@event}> created by <{sender}>", ex);

                if (!ignoreExceptions) throw;
                return 0;
            }
        }
        /// <summary>
        /// Enlists all event listeners to <paramref name="orchestrator"/> if there are any listening for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to enlist</typeparam>
        /// <param name="orchestrator">The orchestrator to add the listeners to</param>
        /// <param name="sender">The object that raised <paramref name="event"/></param>
        /// <param name="event">The event that was raised</param>
        /// <returns>Task that will complete when all listeners have been enlisted to <paramref name="orchestrator"/></returns>
        public void Enlist<TEvent>(EventOrchestrator orchestrator, object sender, TEvent @event)
        {
            using var methodLogger = _logger.TraceMethod(this);
            orchestrator.ValidateArgument(nameof(orchestrator));
            sender.ValidateArgument(nameof(sender));
            @event.ValidateArgument(nameof(@event));

            var provider = orchestrator.ServiceProvider;
            // Get listeners
            var runtimeGlobalListeners = _eventSubscriber.GetAllListeners(provider);
            _logger.Debug($"Got <{runtimeGlobalListeners?.Length ?? 0}> listeners subscribed at runtime to all events");
            runtimeGlobalListeners.Execute(x => orchestrator.Enlist(sender, x, @event));

            var runtimeListeners = provider.GetRequiredService<IEventSubscriber<TEvent>>().GetAllListeners();
            _logger.Debug($"Got <{runtimeListeners?.Length ?? 0}> listeners subscribed at runtime to event of type <{typeof(TEvent)}>");
            runtimeListeners.Execute(x => orchestrator.Enlist(sender, x, @event));

            var injectedGlobalListeners = provider.GetServices<IEventListener>()?.ToArray();
            _logger.Debug($"Got <{injectedGlobalListeners?.Length ?? 0}> injected listeners subscribed to all events");
            injectedGlobalListeners.Execute(x => orchestrator.Enlist(sender, x, @event));

            var injectedListeners = provider.GetServices<IEventListener<TEvent>>()?.ToArray();
            _logger.Debug($"Got <{injectedListeners?.Length ?? 0}> injected listeners subscribed to event of type <{typeof(TEvent)}>");
            injectedListeners.Execute(x => orchestrator.Enlist(sender, x, @event));
        }

        /// <inheritdoc/>
        public async Task<RequestResponse<TResponse>> RequestAsync<TRequest, TResponse>(object sender, TRequest request, Action<INotifierRequestOptions<TRequest>> requestOptions, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            sender.ValidateArgument(nameof(sender));
            requestOptions.ValidateArgument(nameof(requestOptions));
            request.ValidateArgument(nameof(request));

            _logger.Log($"Raising request <{request}> created by <{sender}>");
            var options = new NotifierRequestOptions<TRequest>();
            requestOptions(options);
            var executionChain = new RequestExecutionChain<TRequest, TResponse>(_loggerFactory?.CreateLogger<RequestExecutionChain<TRequest, TResponse>>());

            await using var scope = _serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;

            // Get handlers
            var runtimeHandlers = _requestSubscriptionManager.GetHandlers<TRequest, TResponse>();
            _logger.Debug($"Got <{runtimeHandlers?.Length ?? 0}> handlers subscribed at runtime to requests of type <{typeof(TRequest)}> to which they can reply with <{typeof(TResponse)}>");
            runtimeHandlers.Execute(x => executionChain.Enlist(x));

            var injectedHandlers = provider.GetServices<IRequestHandler<TRequest, TResponse>>()?.ToArray();
            _logger.Debug($"Got <{injectedHandlers?.Length ?? 0}> injected handlers subscribed to requests of type <{typeof(TRequest)}> to which they can reply with <{typeof(TResponse)}>");
            injectedHandlers.Execute(x => executionChain.Enlist(x));

            // Execute
            _logger.Debug($"Executing request chain for <{request}> raised by <{sender}>");
            var response = await executionChain.ExecuteAsync(sender, request, token);

            if (response.Completed)
            {
                _logger.Log($"Got a response for <{request}> created by <{sender}>");
            }
            else
            {
                _logger.Log($"Could not get a response for <{request}> created by <{sender}>");
                if (options.ExceptionFactory != null) throw options.ExceptionFactory(request);
            }
            return response;
        }
        /// <inheritdoc/>
        public async Task<RequestAcknowledgement> RequestAcknowledgementAsync<TRequest>(object sender, TRequest request, Action<INotifierRequestOptions<TRequest>> requestOptions, CancellationToken token = default)
        {
            using var methodLogger = _logger.TraceMethod(this);
            sender.ValidateArgument(nameof(sender));
            requestOptions.ValidateArgument(nameof(requestOptions));
            request.ValidateArgument(nameof(request));

            _logger.Log($"Raising request <{request}> created by <{sender}>");
            var options = new NotifierRequestOptions<TRequest>();
            requestOptions(options);
            var executionChain = new RequestExecutionChain<TRequest>(_loggerFactory?.CreateLogger<RequestExecutionChain<TRequest>>());

            await using var scope = _serviceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;

            // Get handlers
            var runtimeHandlers = _requestSubscriptionManager.GetHandlers<TRequest>();
            _logger.Debug($"Got <{runtimeHandlers?.Length ?? 0}> handlers subscribed at runtime to requests of type <{typeof(TRequest)}> that they can acknowledge");
            runtimeHandlers.Execute(x => executionChain.Enlist(x));

            var injectedHandlers = provider.GetServices<IRequestHandler<TRequest>>()?.ToArray();
            _logger.Debug($"Got <{injectedHandlers?.Length ?? 0}> injected handlers subscribed to requests of type <{typeof(TRequest)}> that they can acknowledge");
            injectedHandlers.Execute(x => executionChain.Enlist(x));

            // Execute
            _logger.Debug($"Executing request chain for <{request}> raised by <{sender}>");
            var response = await executionChain.ExecuteAsync(sender, request, token);

            if (response.Acknowledged)
            {
                _logger.Log($"Got an acknowledgment for <{request}> created by <{sender}>");
            }
            else
            {
                _logger.Log($"Could not get an acknowledgment for <{request}> created by <{sender}>");
                if (options.ExceptionFactory != null) throw options.ExceptionFactory(request);
            }
            return response;
        }

        /// <inheritdoc cref="INotifierEventOptions{TEvent}"/>
        /// <typeparam name="TEvent"></typeparam>
        private class NotifierEventOptions<TEvent> : INotifierEventOptions<TEvent>
        {
            // Fields
            private readonly Notifier _parent;
            private readonly EventOrchestrator _orchestrator;
            private readonly object _sender;
            private readonly TEvent _event;

            // Properties
            /// <summary>
            /// The configured options.
            /// </summary>
            public EventOptions Options { get; private set; }

            public NotifierEventOptions(Notifier parent, EventOrchestrator eventOrchestrator, object sender, TEvent @event)
            {
                _parent = parent.ValidateArgument(nameof(parent));
                _orchestrator = eventOrchestrator.ValidateArgument(nameof(eventOrchestrator));
                _sender = sender.ValidateArgument(nameof(sender));
                _event = @event.ValidateArgument(nameof(@event));
            }

            /// <inheritdoc/>
            public INotifierEventOptions<TEvent> WithOptions(EventOptions options)
            {
                Options = options;
                return this;
            }
            /// <inheritdoc/>
            public INotifierEventOptions<TEvent> AlsoRaiseAs<T>()
            {
                if (!(_event is T)) throw new ArgumentException($"Event <{_event}> is not assignable to type <{typeof(T)}>");

                _parent.Enlist(_orchestrator, _sender, _event.CastTo<T>());

                return this;
            }
            /// <inheritdoc/>
            public INotifierEventOptions<TEvent> Enlist<T>(T @event)
            {
                @event.ValidateArgument(nameof(@event));
                _parent.Enlist(_orchestrator, _sender, @event);

                return this;
            }
        }

        /// <inheritdoc cref="INotifierRequestOptions{TRequest}"/>
        private class NotifierRequestOptions<TRequest> : INotifierRequestOptions<TRequest>
        {
            // Properties
            /// <summary>
            /// Custom exception factory to throw an exception when a raised requests is not replied to. When set to null no exception will be thrown.
            /// </summary>
            public Func<TRequest, Exception> ExceptionFactory { get; private set; }

            /// <inheritdoc/>
            public INotifierRequestOptions<TRequest> ThrowOnUnhandled(Func<TRequest, Exception> exceptionFactory)
            {
                ExceptionFactory = exceptionFactory.ValidateArgument(nameof(exceptionFactory));
                return this;
            }
        }
    }
}
