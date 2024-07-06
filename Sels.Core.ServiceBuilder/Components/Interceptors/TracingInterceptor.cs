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
using Sels.Core.Tracing;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace Sels.Core.ServiceBuilder.Interceptors
{
    /// <summary>
    /// Interceptor that traces method duration and/or exceptions.
    /// </summary>
    public class TracingInterceptor : BaseResultlessInterceptor, ITracingInterceptorBuilder, IMethodDurationInterceptorBuilder, IMethodLoggingScopeInterceptorBuilder
    {
        // Statics
        /// <summary>
        /// Method that took longer to execute then the offset will be logged using the <see cref="LongRunningLogLevel"/> log level. Useful for debugging.
        /// </summary>
        public static TimeSpan? LongRunningOffset { get; set; }
        /// <summary>
        /// The log elvel that will be used to trace long running method.
        /// </summary>
        public static LogLevel LongRunningLogLevel { get; set; } = LogLevel.Warning;

        // Fields
        private readonly IMemoryCache _cache;
        private readonly ILoggerFactory _factory;
        private readonly ILogger _logger;

        // State
        private List<AllMethodTracer> _allMethodTracers = new List<AllMethodTracer>();
        private List<SpecificMethodTracer> _specificMethodTracers = new List<SpecificMethodTracer>();
        private List<AllMethodScopeStarter> _allMethodScopeStarters = new List<AllMethodScopeStarter>();
        private List<SpecificMethodScopeStarter> _specificMethodScopeStarters = new List<SpecificMethodScopeStarter>();
        private ExceptionTracer _exceptionTracer;

        /// <inheritdoc cref="TracingInterceptor"/>
        /// <param name="cache">Cache used to store generate expressions</param>
        /// <param name="logger">Optionall logger for tracing</param>
        public TracingInterceptor(IMemoryCache cache, ILogger<TracingInterceptor> logger = null)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc cref="TracingInterceptor"/>
        /// <param name="cache">Cache used to store generate expressions</param>
        /// <param name="factory">Logger factory for creating loggers based on the target type</param>
        public TracingInterceptor(IMemoryCache cache, ILoggerFactory factory)
        {
            _cache = cache;
            _factory = factory;
        }

        internal void Validate()
        {
            if (((_allMethodScopeStarters.HasValue() && _allMethodScopeStarters.Any(x => x.EnableEnrichment)) || (_specificMethodScopeStarters.HasValue() && _specificMethodScopeStarters.Any(x => x.EnableEnrichment))) && _cache == null)
            {
                throw new InvalidOperationException($"IMemoryCache must be registered when using method logging scopes with enrichment enabled");
            }
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
        public IMethodLoggingScopeInterceptorBuilder WithScope => this;
        /// <inheritdoc/>
        public IAllMethodLoggingScopeInterceptorBuilder ForAll { get { var tracer = new AllMethodScopeStarter(this); _allMethodScopeStarters.Add(tracer); return tracer; } }
        /// <inheritdoc/>
        public ISpecificMethodLoggingScopeInterceptorBuilder For { get { var tracer = new SpecificMethodScopeStarter(this); _specificMethodScopeStarters.Add(tracer); return tracer; } }

        /// <inheritdoc/>
        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            var logger = GetLoggerFor(invocation);
            using var logScope = StartScopeFor(logger, invocation);

            try
            {
                var tracer = Helper.Collection.EnumerateAll<MethodTracer>(_specificMethodTracers, _allMethodTracers).FirstOrDefault(x => x.CanTrace(invocation));

                if (tracer != null)
                {
                    string method = null;
                    if (_factory != null)
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
                if (logger != null)
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

        private IDisposable StartScopeFor(ILogger logger, IInvocation invocation)
        {
            if (logger == null) return NullDisposer.Instance;

            var scopeStarter = Helper.Collection.EnumerateAll<MethodScopeStarter>(_specificMethodScopeStarters, _allMethodScopeStarters).FirstOrDefault(x => x.CanStartScope(invocation));

            if (scopeStarter == null) return NullDisposer.Instance;

            IDictionary<string, object> scope = null;

            if (scopeStarter.Enricher != null)
            {
                scope ??= new Dictionary<string, object>();
                scopeStarter.Enricher(scope, invocation.Method, invocation);
            }

            if (scopeStarter.EnableEnrichment)
            {
                var methodName = invocation.Method.GetDisplayName(MethodDisplayOptions.Full);
                _logger.Debug($"Enrichment is enabled for method <{methodName}>. Starting logging scope with parameters enriched from attributes defined on the method");
                scope ??= new Dictionary<string, object>();

                var cacheKey = $"{scopeStarter.CachePrefix}.{methodName}";

                if (!_cache.TryGetValue<Action<object, object[], IDictionary<string, object>>>(cacheKey, out var enricher))
                {
                    _logger.Debug($"First time enriching method <{methodName}>. Generating enricher");
                    if (Helper.Expressions.Tracing.TryGenerateEnrichmentDelegate(invocation.Method, out enricher, scopeStarter.ConflictHandling, true, scopeStarter.TopLevelOnly))
                    {
                        _logger.Debug($"Generated enricher for method <{methodName}>. Caching");                       
                    }
                    else
                    {
                        _logger.Debug($"Nothing traceable configured on method <{methodName}>. Ignoring");
                    }
                    _cache.Set(cacheKey, enricher, scopeStarter.CacheOptions);
                }

                if(enricher != null)
                {
                    _logger.Debug($"Enriching method <{methodName}>");
                    enricher(invocation.InvocationTarget, invocation.Arguments, scope);
                }
            }

            return logger.TryBeginScope(scope);
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
        private class SpecificMethodScopeStarter : MethodScopeStarter<ISpecificMethodLoggingScopeInterceptorBuilder>, ISpecificMethodLoggingScopeInterceptorBuilder
        {
            // Fields
            private readonly List<Predicate<IInvocation>> _selectors = new List<Predicate<IInvocation>>();

            public SpecificMethodScopeStarter(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public override ISpecificMethodLoggingScopeInterceptorBuilder Self => this;

            public override bool CanStartScope(IInvocation invocation)
            {
                return _selectors.Count != 0 && _selectors.Any(x => x(invocation));
            }

            public ISpecificMethodLoggingScopeInterceptorBuilder Methods(Predicate<IInvocation> condition)
            {
                condition = condition.ValidateArgument(nameof(condition));

                _selectors.Add(condition);
                return this;
            }
        }
        private class AllMethodScopeStarter : MethodScopeStarter<IAllMethodLoggingScopeInterceptorBuilder>, IAllMethodLoggingScopeInterceptorBuilder
        {
            // Fields
            private readonly List<Predicate<IInvocation>> _exceptions = new List<Predicate<IInvocation>>();

            public AllMethodScopeStarter(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public override IAllMethodLoggingScopeInterceptorBuilder Self => this;

            public override bool CanStartScope(IInvocation invocation)
            {
                return _exceptions.Count == 0 || !_exceptions.Any(x => x(invocation));
            }

            public IAllMethodLoggingScopeInterceptorBuilder Except(Predicate<IInvocation> condition)
            {
                condition = condition.ValidateArgument(nameof(condition));

                _exceptions.Add(condition);
                return this;
            }
        }
        private abstract class MethodScopeStarter<TDerived> : MethodScopeStarter, IMethodLoggingScopeSharedInterceptorBuilder<TDerived> where TDerived : IMethodLoggingScopeSharedInterceptorBuilder<TDerived>
        {
            protected MethodScopeStarter(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public abstract TDerived Self { get; }

            public ITracingInterceptorBuilder And => _builder;

            public TDerived WithCachePrefix(string prefix)
            {
                CachePrefix = prefix.ValidateArgumentNotNullOrWhitespace(nameof(prefix));
                return Self;
            }

            public TDerived IgnoreAttributes()
            {
                EnableEnrichment = false;
                return Self;
            }

            public TDerived WithConflictHandling(TraceableConflictHandling conflictHandling)
            {
                ConflictHandling = conflictHandling;
                return Self;
            }

            public TDerived WithParameter(Action<IDictionary<string, object>, MethodInfo, IInvocation> action)
            {
                action = action.ValidateArgument(nameof(action));

                if (Enricher == null)
                {
                    Enricher = action;
                }
                else
                {
                    Enricher += action;
                }
                return Self;
            }

            TDerived IMethodLoggingScopeSharedInterceptorBuilder<TDerived>.TopLevelOnly()
            {
                TopLevelOnly = true;
                return Self;
            }

            public TDerived WithCacheOptions(MemoryCacheEntryOptions options)
            {
                CacheOptions = options.ValidateArgument(nameof(options));
                return Self;
            }
        }
        private abstract class MethodScopeStarter : Delegator
        {
            protected MethodScopeStarter(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public string CachePrefix { get; protected set; } = typeof(TracingInterceptor).FullName;
            public MemoryCacheEntryOptions CacheOptions { get; protected set; } = new MemoryCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(5) };
            public TraceableConflictHandling ConflictHandling { get; set; } = TraceableConflictHandling.UpdateIfDefault;
            public bool EnableEnrichment { get; set; } = true;
            public bool TopLevelOnly { get; set; } = false;
            public Action<IDictionary<string, object>, MethodInfo, IInvocation> Enricher { get; set; }

            public abstract bool CanStartScope(IInvocation invocation);
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

            public IMethodLoggingScopeInterceptorBuilder WithScope => _builder.WithScope;
        }
        #endregion
    }
}
