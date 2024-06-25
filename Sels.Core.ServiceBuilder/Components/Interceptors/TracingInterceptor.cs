using Castle.DynamicProxy;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Models.Disposables;
using Sels.Core.ServiceBuilder.Template.Interceptors;
using Sels.Core.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Linq;

namespace Sels.Core.ServiceBuilder.Interceptors
{
    /// <summary>
    /// Interceptor that traces method duration and/or exceptions.
    /// </summary>
    public class TracingInterceptor : BaseResultlessInterceptor, ITracingInterceptorBuilder, IMethodDurationInterceptorBuilder
    {
        // Statics
        /// <summary>
        /// Method that took longer to execute then the offset will be logged using the <see cref="LongRunningLogLevel"/> log level. Usefull for debugging.
        /// </summary>
        public static TimeSpan? LongRunningOffset { get; set; }
        /// <summary>
        /// The log elvel that will be used to trace long running method.
        /// </summary>
        public static LogLevel LongRunningLogLevel { get; set; } = LogLevel.Warning;
        
        // Fields
        private readonly ILoggerFactory _factory;
        private readonly ILogger _logger;

        // State
        private List<AllMethodTracer> _allMethodTracers = new List<AllMethodTracer>();
        private List<SpecificMethodTracer> _specificMethodTracers = new List<SpecificMethodTracer>();
        private ExceptionTracer _exceptionTracer;

        /// <inheritdoc cref="TracingInterceptor"/>
        /// <param name="logger">Optionall logger for tracing</param>
        public TracingInterceptor(ILogger<TracingInterceptor> logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc cref="TracingInterceptor"/>
        /// <param name="factory">Logger factory for creating loggers based on the target type</param>
        public TracingInterceptor(ILoggerFactory factory)
        {
            _factory = factory;
        }
        /// <inheritdoc/>
        public IMethodDurationInterceptorBuilder Duration => this;
        /// <inheritdoc/>
        public IExceptionTracingInterceptorBuilder Exceptions { get { _exceptionTracer = new ExceptionTracer(this); return _exceptionTracer; } }
        /// <inheritdoc/>
        public IAllMethodDurationInterceptorBuilder OfAll { get { var tracer = new AllMethodTracer(this); _allMethodTracers.Add(tracer); return tracer; } }
        /// <inheritdoc/>
        public ISpecificMethodDurationInterceptorBuilder Of { get { var tracer = new SpecificMethodTracer(this); _specificMethodTracers.Add(tracer); return tracer; } }

        /// <inheritdoc/>
        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            var logger = GetLoggerFor(invocation);

            try
            {
                var tracer = Helper.Collection.EnumerateAll<MethodTracer>(_specificMethodTracers, _allMethodTracers).FirstOrDefault(x => x.CanTrace(invocation));

                if(tracer != null)
                {
                    string method = null;
                    if(_factory != null)
                    {
                        method = invocation.Method.GetDisplayName(MethodDisplayOptions.MethodOnly);
                    }
                    else
                    {
                        method = $"{invocation.TargetType.Name}.{invocation.Method.GetDisplayName(MethodDisplayOptions.MethodOnly)}";
                    }

                    logger.Trace($"Executing method <{method}>");
                    using (Helper.Time.CaptureDuration(x =>
                    {
                        var logLevel = LogLevel.Trace;
                        tracer.DurationLogLevels?.OrderBy(x => x.Value)?.Where(d => x >= d.Value).Execute(x =>
                        {
                            logLevel = x.Key;
                        });
                        logger.LogMessage(logLevel, $"Executed method <{method}> in <{x}>");
                        if (LongRunningOffset.HasValue && LongRunningOffset.Value <= x) logger.LogMessage(LongRunningLogLevel, $"Long running method: {method}[{x}]");
                    }))
                    {
                        await proceed(invocation, proceedInfo);
                    }
                }
                else
                {
                    await proceed(invocation, proceedInfo);
                }                   
            }
            catch (Exception ex)
            {
                if(logger != null)
                {
                    _exceptionTracer?.Trace(invocation, logger, ex);
                }

                throw;
            }
 
        }

        private ILogger GetLoggerFor(IInvocation invocation)
        {
            if (_factory != null)
            {
                return _factory.CreateLogger(invocation.TargetType);
            }

            return _logger;
        }

        #region Helper classes
        private class ExceptionTracer : Delegator, IExceptionTracingInterceptorBuilder
        {
            // Fields
            private List<Predicate<Exception>> _conditions = new List<Predicate<Exception>>();
            private Func<Exception, LogLevel?> _logLevelSelector;
            private Action<IInvocation, ILogger, LogLevel, Exception> _logger;

            // Properties
            /// <inheritdoc/>
            public ITracingInterceptorBuilder And => _builder;

            public ExceptionTracer(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public void Trace(IInvocation invocation, ILogger loggers, Exception exception)
            {
                if (_conditions.Count == 0 || _conditions.Any(x => x(exception)))
                {
                    var logLevel = _logLevelSelector?.Invoke(exception) ?? LogLevel.Error;

                    if (_logger != null) _logger(invocation, loggers, logLevel, exception); else loggers.LogException(logLevel, exception);
                } 
            }

            public IExceptionTracingInterceptorBuilder Using(Action<IInvocation, ILogger, LogLevel, Exception> logger)
            {
               _logger = logger.ValidateArgument(nameof(logger));
                return this;
            }

            public IExceptionTracingInterceptorBuilder When(Predicate<Exception> condition)
            {
                condition.ValidateArgument(nameof(condition));

                _conditions.Add(condition);
                return this;
            }

            public IExceptionTracingInterceptorBuilder WithLevel(Func<Exception, LogLevel?> selector)
            {
                _logLevelSelector = selector.ValidateArgument(nameof(selector));
                return this;
            }
        }
        private class AllMethodTracer : MethodTracer, IAllMethodDurationInterceptorBuilder
        {
            // Fields
            private readonly List<Predicate<IInvocation>> _exceptions = new List<Predicate<IInvocation>>();
            // Properties
            /// <inheritdoc/>
            public ITracingInterceptorBuilder And => _builder;

            public AllMethodTracer(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public override bool CanTrace(IInvocation invocation)
            {
                return _exceptions.Count == 0 || !_exceptions.Any(x => x(invocation));
            }

            public IAllMethodDurationInterceptorBuilder Except(Predicate<IInvocation> condition)
            {
                condition.ValidateArgument(nameof(condition));

                _exceptions.Add(condition);
                return this;
            }

            public IAllMethodDurationInterceptorBuilder WhenDurationAbove(TimeSpan duration, LogLevel logLevel)
            {
                DurationLogLevels ??= new Dictionary<LogLevel, TimeSpan>();

                DurationLogLevels.AddOrUpdate(logLevel, duration);

                return this;
            }
        }
        private class SpecificMethodTracer : MethodTracer, ISpecificMethodDurationInterceptorBuilder
        {
            // Fields
            private readonly List<Predicate<IInvocation>> _selectors = new List<Predicate<IInvocation>>();

            // Properties
            /// <inheritdoc/>
            public ITracingInterceptorBuilder And => _builder;

            public SpecificMethodTracer(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public override bool CanTrace(IInvocation invocation)
            {
                return _selectors.Count != 0 && _selectors.Any(x => x(invocation));
            }

            public ISpecificMethodDurationInterceptorBuilder Methods(Predicate<IInvocation> condition)
            {
                condition.ValidateArgument(nameof(condition));

                _selectors.Add(condition);
                return this;
            }

            public ISpecificMethodDurationInterceptorBuilder WhenDurationAbove(TimeSpan duration, LogLevel logLevel)
            {
                DurationLogLevels ??= new Dictionary<LogLevel, TimeSpan>();

                DurationLogLevels.AddOrUpdate(logLevel, duration);

                return this;
            }
        }
        private abstract class MethodTracer : Delegator
        {
            protected MethodTracer(ITracingInterceptorBuilder builder) : base(builder)
            {
            }


            public Dictionary<LogLevel, TimeSpan> DurationLogLevels { get; protected set; }

            public abstract bool CanTrace(IInvocation invocation);
        }
        private abstract class Delegator : ITracingInterceptorBuilder
        {
            // Fields
            protected readonly ITracingInterceptorBuilder _builder;

            public Delegator(ITracingInterceptorBuilder builder)
            {
                _builder = builder.ValidateArgument(nameof(builder));
            }

            public IMethodDurationInterceptorBuilder Duration => _builder.Duration;

            public IExceptionTracingInterceptorBuilder Exceptions => _builder.Exceptions;
        }
        #endregion
    }
}
