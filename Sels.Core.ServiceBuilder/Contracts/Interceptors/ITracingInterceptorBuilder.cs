using Castle.DynamicProxy;
using System.Reflection;
using Sels.Core.Extensions.Reflection;
using Microsoft.Extensions.Logging;
using System;
using Sels.Core.Extensions;
using System.Linq;
using System.Collections.Generic;
using Sels.Core.Tracing;
using Sels.Core.Extensions.Collections;
using Microsoft.Extensions.Caching.Memory;

namespace Sels.Core.ServiceBuilder.Interceptors
{
    /// <summary>
    /// Builder for creating an interceptor that traces method duration and/or exceptions.
    /// </summary>
    public interface ITracingInterceptorBuilder
    {
        /// <summary>
        /// Traces the duration of method execution.
        /// </summary>
        IMethodDurationInterceptorBuilder Duration { get; }
        /// <summary>
        /// Traces any thrown exceptions.
        /// </summary>
        IExceptionTracingInterceptorBuilder Exceptions { get; }
        /// <summary>
        /// Starts a logging scope for the selected method(s).
        /// </summary>
        IMethodLoggingScopeInterceptorBuilder WithScope { get; }
    }

    /// <summary>
    /// Builder for creating an interceptor that traces method duration.
    /// </summary>
    public interface IMethodDurationInterceptorBuilder
    {
        /// <summary>
        /// Traces the duration of all methods.
        /// </summary>
        IAllMethodDurationInterceptorBuilder OfAll { get; }
        /// <summary>
        /// Traces the duration of specific methods.
        /// </summary>
        ISpecificMethodDurationInterceptorBuilder Of { get; }
    }
    /// <summary>
    /// Builder for creating an interceptor that traces all method duration.
    /// </summary>
    public interface IAllMethodDurationInterceptorBuilder : IMethodDurationSharedInterceptorBuilder<IAllMethodDurationInterceptorBuilder>
    {
        /// <summary>
        /// Returns the parent builder for defining more tracing configuration.
        /// </summary>
        ITracingInterceptorBuilder And { get; }

        /// <summary>
        /// All methods matching <paramref name="condition"/> will not be traced.
        /// </summary>
        /// <param name="condition">Delegate that dictates if a method should not be traced</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodDurationInterceptorBuilder Except(Predicate<IInvocation> condition);
        /// <summary>
        /// <paramref name="method"/> will not be traced.
        /// </summary>
        /// <param name="method">The method not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodDurationInterceptorBuilder ExceptMethod(MethodInfo method) => Except(x => x.Method.AreEqual(method.ValidateArgument(nameof(method))));
        /// <summary>
        /// Methods with <paramref name="methodname"/> will not be traced.
        /// </summary>
        /// <param name="methodname">The name of the methods not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodDurationInterceptorBuilder ExceptMethod(string methodname) => Except(x => x.Method.Name.Equals(methodname.ValidateArgumentNotNullOrWhitespace(methodname)));
        /// <summary>
        /// Methods with <paramref name="methodNames"/> will not be traced.
        /// </summary>
        /// <param name="methodNames">The names of the methods not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodDurationInterceptorBuilder ExceptMethods(params string[] methodNames) => Except(x => methodNames.ValidateArgument(nameof(methodNames)).Contains(x.Method.Name));
    }
    /// <summary>
    /// Builder for creating an interceptor that traces the duration of specific methods.
    /// </summary>
    public interface ISpecificMethodDurationInterceptorBuilder : IMethodDurationSharedInterceptorBuilder<ISpecificMethodDurationInterceptorBuilder>
    {
        /// <summary>
        /// Returns the parent builder for defining more tracing configuration.
        /// </summary>
        ITracingInterceptorBuilder And { get; }

        /// <summary>
        /// All methods matching <paramref name="condition"/> will be traced.
        /// </summary>
        /// <param name="condition">Delegate that dictates if a method should be traced</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodDurationInterceptorBuilder Methods(Predicate<IInvocation> condition);
        /// <summary>
        /// <paramref name="method"/> will be traced.
        /// </summary>
        /// <param name="method">The method to trace</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodDurationInterceptorBuilder Method(MethodInfo method) => Methods(x => x.Method.AreEqual(method.ValidateArgument(nameof(method))));
        /// <summary>
        /// Methods with <paramref name="methodname"/> will be traced.
        /// </summary>
        /// <param name="methodname">The name of the methods not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodDurationInterceptorBuilder Method(string methodname) => Methods(x => x.Method.Name.Equals(methodname.ValidateArgumentNotNullOrWhitespace(methodname)));
        /// <summary>
        /// Methods with <paramref name="methodNames"/> will be traced.
        /// </summary>
        /// <param name="methodNames">The names of the methods to trace</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodDurationInterceptorBuilder Methods(params string[] methodNames) => Methods(x => methodNames.ValidateArgument(nameof(methodNames)).Contains(x.Method.Name));
    }
    /// <summary>
    /// Builder for configuring extra options when tracing the duration of methods.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IMethodDurationSharedInterceptorBuilder<TDerived> where TDerived : IMethodDurationSharedInterceptorBuilder<TDerived>
    {
        /// <summary>
        /// If method duration is above or equal to <paramref name="duration"/>, the duration will be traced using log level <paramref name="logLevel"/>.
        /// </summary>
        /// <param name="duration">The threshold above which to trace with <paramref name="duration"/></param>
        /// <param name="logLevel">THe log level to trace with</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WhenDurationAbove(TimeSpan duration, LogLevel logLevel);
        /// <summary>
        /// If method duration is above or equal to <paramref name="duration"/>ms, the duration will be traced using log level <paramref name="logLevel"/>.
        /// </summary>
        /// <param name="duration">The threshold (in ms) above which to trace with <paramref name="duration"/></param>
        /// <param name="logLevel">THe log level to trace with</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WhenDurationAbove(uint duration, LogLevel logLevel) => WhenDurationAbove(TimeSpan.FromMilliseconds(duration), logLevel);

        /// <summary>
        /// Sets the duration threshold above which to trace with <see cref="LogLevel.Warning"/> and <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="warningDuration">The threshold above which to trace with log level <see cref="LogLevel.Warning"/></param>
        /// <param name="errorDuration">The threshold above which to trace with log level <see cref="LogLevel.Error"/></param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithDurationThresholds(TimeSpan warningDuration, TimeSpan errorDuration) => WhenDurationAbove(warningDuration, LogLevel.Warning).WhenDurationAbove(errorDuration, LogLevel.Error);
        /// <summary>
        /// Sets the duration threshold above which to trace with <see cref="LogLevel.Warning"/> and <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="warningDuration">The threshold (in ms) above which to trace with log level <see cref="LogLevel.Warning"/></param>
        /// <param name="errorDuration">The threshold (in ms) above which to trace with log level <see cref="LogLevel.Error"/></param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithDurationThresholds(uint warningDuration, uint errorDuration) => WhenDurationAbove(warningDuration, LogLevel.Warning).WhenDurationAbove(errorDuration, LogLevel.Error);
        /// <summary>
        /// Sets the duration threshold above which to trace with <see cref="LogLevel.Warning"/>, <see cref="LogLevel.Error"/> and <see cref="LogLevel.Critical"/>.
        /// </summary>
        /// <param name="warningDuration">The threshold above which to trace with log level <see cref="LogLevel.Warning"/></param>
        /// <param name="errorDuration">The threshold above which to trace with log level <see cref="LogLevel.Error"/></param>
        /// <param name="criticalDuration">The threshold above which to trace with log level <see cref="LogLevel.Critical"/></param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithDurationThresholds(TimeSpan warningDuration, TimeSpan errorDuration, TimeSpan criticalDuration) => WithDurationThresholds(warningDuration, errorDuration).WhenDurationAbove(criticalDuration, LogLevel.Critical);
        /// <summary>
        /// Sets the duration threshold above which to trace with <see cref="LogLevel.Warning"/>, <see cref="LogLevel.Error"/> and <see cref="LogLevel.Critical"/>.
        /// </summary>
        /// <param name="warningDuration">The threshold (in ms) above which to trace with log level <see cref="LogLevel.Warning"/></param>
        /// <param name="errorDuration">The threshold (in ms) above which to trace with log level <see cref="LogLevel.Error"/></param>
        /// <param name="criticalDuration">The threshold (in ms) above which to trace with log level <see cref="LogLevel.Critical"/></param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithDurationThresholds(uint warningDuration, uint errorDuration, uint criticalDuration) => WithDurationThresholds(TimeSpan.FromMilliseconds(warningDuration), TimeSpan.FromMilliseconds(errorDuration), TimeSpan.FromMilliseconds(criticalDuration));

        /// <summary>
        /// Traces with <see cref="LogLevel.Warning"/> when method duration goes above 250ms and with <see cref="LogLevel.Error"/> when duration goes above 1000ms.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithDefaultThresholds() => WithDurationThresholds(250, 1000);
    }

    /// <summary>
    /// Builder for creating an interceptor that traces any exception thrown.
    /// </summary>
    public interface IExceptionTracingInterceptorBuilder
    {
        /// <summary>
        /// Returns the parent builder for defining more tracing configuration.
        /// </summary>
        ITracingInterceptorBuilder And { get; }

        /// <summary>
        /// Defines when to trace exception. Default is all exceptions if no condition is set.
        /// </summary>
        /// <param name="condition">Delegate that dictates when to trace an exception</param>
        /// <returns>Current builder for method chaining</returns>
        IExceptionTracingInterceptorBuilder When(Predicate<Exception> condition);
        /// <summary>
        /// Traces exception assignable to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the exception to trace</typeparam>
        /// <returns>Current builder for method chaining</returns>
        IExceptionTracingInterceptorBuilder OfType<T>() where T : Exception => When(x => x.IsAssignableTo<T>());
        /// <summary>
        /// Defines the log level to use for specific exceptions. If no delegates are set the default is <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="selector">Delegate that returns the log level for the provided exception. If null is returned then the default log level is used</param>
        /// <returns>Current builder for method chaining</returns>
        IExceptionTracingInterceptorBuilder WithLevel(Func<Exception, LogLevel?> selector);
        /// <summary>
        /// Defines a custom delegate for logging the exception.
        /// </summary>
        /// <param name="logger">Delegate that logs the exceptions using the provided loggers</param>
        /// <returns>Current builder for method chaining</returns>
        IExceptionTracingInterceptorBuilder Using(Action<IInvocation, ILogger, LogLevel, Exception> logger);
    }

    /// <summary>
    /// Builder for creating an interceptor that starts a logging scope for the selected method.
    /// </summary>
    public interface IMethodLoggingScopeInterceptorBuilder
    {
        /// <summary>
        /// Starts a logging scope for all methods.
        /// </summary>
        IAllMethodLoggingScopeInterceptorBuilder ForAll { get; }
        /// <summary>
        /// Starts a logging scope for a few methods.
        /// </summary>
        ISpecificMethodLoggingScopeInterceptorBuilder For { get; }
    }
    /// <summary>
    /// Builder for creating an interceptor that starts a logging scope for a few methods.
    /// </summary>
    public interface ISpecificMethodLoggingScopeInterceptorBuilder : IMethodLoggingScopeSharedInterceptorBuilder<ISpecificMethodLoggingScopeInterceptorBuilder>
    {
        /// <summary>
        /// All methods matching <paramref name="condition"/> will be traced.
        /// </summary>
        /// <param name="condition">Delegate that dictates if a method should be traced</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodLoggingScopeInterceptorBuilder Methods(Predicate<IInvocation> condition);
        /// <summary>
        /// <paramref name="method"/> will be traced.
        /// </summary>
        /// <param name="method">The method to trace</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodLoggingScopeInterceptorBuilder Method(MethodInfo method) => Methods(x => x.Method.AreEqual(method.ValidateArgument(nameof(method))));
        /// <summary>
        /// Methods with <paramref name="methodname"/> will be traced.
        /// </summary>
        /// <param name="methodname">The name of the methods not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodLoggingScopeInterceptorBuilder Method(string methodname) => Methods(x => x.Method.Name.Equals(methodname.ValidateArgumentNotNullOrWhitespace(methodname)));
        /// <summary>
        /// Methods with <paramref name="methodNames"/> will be traced.
        /// </summary>
        /// <param name="methodNames">The names of the methods to trace</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodLoggingScopeInterceptorBuilder Methods(params string[] methodNames) => Methods(x => methodNames.ValidateArgument(nameof(methodNames)).Contains(x.Method.Name));
    }
    /// <summary>
    /// Builder for creating an interceptor that starts a logging scope for all methods.
    /// </summary>
    public interface IAllMethodLoggingScopeInterceptorBuilder : IMethodLoggingScopeSharedInterceptorBuilder<IAllMethodLoggingScopeInterceptorBuilder>
    {
        /// <summary>
        /// All methods matching <paramref name="condition"/> will not be traced.
        /// </summary>
        /// <param name="condition">Delegate that dictates if a method should not be traced</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodLoggingScopeInterceptorBuilder Except(Predicate<IInvocation> condition);
        /// <summary>
        /// <paramref name="method"/> will not be traced.
        /// </summary>
        /// <param name="method">The method not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodLoggingScopeInterceptorBuilder ExceptMethod(MethodInfo method) => Except(x => x.Method.AreEqual(method.ValidateArgument(nameof(method))));
        /// <summary>
        /// Methods with <paramref name="methodname"/> will not be traced.
        /// </summary>
        /// <param name="methodname">The name of the methods not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodLoggingScopeInterceptorBuilder ExceptMethod(string methodname) => Except(x => x.Method.Name.Equals(methodname.ValidateArgumentNotNullOrWhitespace(methodname)));
        /// <summary>
        /// Methods with <paramref name="methodNames"/> will not be traced.
        /// </summary>
        /// <param name="methodNames">The names of the methods not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodLoggingScopeInterceptorBuilder ExceptMethods(params string[] methodNames) => Except(x => methodNames.ValidateArgument(nameof(methodNames)).Contains(x.Method.Name));
    }

    /// <summary>
    /// Builder for configuring extra options when starting a logging scope for methods.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IMethodLoggingScopeSharedInterceptorBuilder<TDerived> where TDerived : IMethodLoggingScopeSharedInterceptorBuilder<TDerived>
    {
        /// <summary>
        /// Returns the parent builder for defining more tracing configuration.
        /// </summary>
        ITracingInterceptorBuilder And { get; }

        /// <summary>
        /// The cache prefix that will be used when creating the key for the IMemoryCache. Used to store the generates expressions for the methods.
        /// </summary>
        /// <param name="prefix">The prefix to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithCachePrefix(string prefix);
        /// <summary>
        /// Defines the cache options to use for the IMemoryCache when storing the generated expressions for the methods.
        /// </summary>
        /// <param name="options">DThe options to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithCacheOptions(MemoryCacheEntryOptions options);
        /// <summary>
        /// Disables the enrichments that happends based on <see cref="TraceableAttribute"/>(s) defined on the method arguments.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TDerived IgnoreAttributes();
        /// <summary>
        /// If only the <see cref="TraceableAttribute"/>(s) defined on the method arguments and it's properties. Hierarchy will not be traversed.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TDerived TopLevelOnly();
        /// <summary>
        /// Overwrites the default conflict handling when gathering log parameters.
        /// The default is <see cref="TraceableConflictHandling.UpdateIfDefault"/>.
        /// </summary>
        /// <param name="conflictHandling">The conflict handling to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithConflictHandling(TraceableConflictHandling conflictHandling);
        /// <summary>
        /// Defines a log parameter that will be used when starting the logging scope.
        /// Value will always override log parameters enriched from the <see cref="TraceableAttribute"/>(s) defined on the method arguments and it's properties.
        /// </summary>
        /// <param name="name">The name of the log parameter</param>
        /// <param name="value">The value for the log parameter</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithParameter(string name, object value)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            return WithParameter((l, m, i) => l.AddOrUpdate(name, value));
        }
        /// <summary>
        /// Registers a delegate that can be used to add log parameters to the logging scope.
        /// </summary>
        /// <param name="action">Delegate that will be called to enrich the log parameters</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithParameter(Action<IDictionary<string, object>, MethodInfo, IInvocation> action);
        /// <summary>
        /// Registers a delegate that can be used to add log parameters to the logging scope.
        /// </summary>
        /// <param name="action">Delegate that will be called to enrich the log parameters</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithParameter(Action<IDictionary<string, object>, MethodInfo> action)
        {
            action = action.ValidateArgument(nameof(action));

            return WithParameter((l, m, i) => action(l, m));
        }
        /// <summary>
        /// Registers a delegate that can be used to add log parameters to the logging scope.
        /// </summary>
        /// <param name="action">Delegate that will be called to enrich the log parameters</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived WithParameter(Action<IDictionary<string, object>> action)
        {
            action = action.ValidateArgument(nameof(action));

            return WithParameter((l, m, i) => action(l));
        }
    }
}
