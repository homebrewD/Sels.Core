﻿using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Converters;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.ServiceBuilder.Contracts.Interceptors.Caching;
using System.Text;

namespace Sels.Core.ServiceBuilder.Interceptors.Caching
{
    /// <inheritdoc cref="IInterceptorCachingProvider{TOptions}"/>
    /// <typeparam name="TEncoding">The encoding to use for the serialized strings</typeparam>
    internal class DistributedCachingProvider<TEncoding> : IInterceptorCachingProvider<IDistributedCacheOptions> where TEncoding : Encoding, new()
    {
        // Fields
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCachingProvider<TEncoding>>? _logger;

        /// <inheritdoc cref="DistributedCachingProvider{TEncoding}"/>
        /// <param name="cache">The cache to use</param>
        /// <param name="logger">Optional logger for tracing</param>
        public DistributedCachingProvider(IDistributedCache cache, ILogger<DistributedCachingProvider<TEncoding>>? logger = null)
        {
            this._cache = Guard.IsNotNull(cache);
            this._logger = logger;
        }

        /// <inheritdoc/>
        public IDistributedCacheOptions CreateNewOptions() => new DistributedCacheOptions();
        /// <inheritdoc/>
        public async Task<T> GetOrSetAsync<T>(IInvocation target, string key, IDistributedCacheOptions? options, Delegates.Async.AsyncFunc<CancellationToken, T> cacheGetter, CancellationToken token = default)
        {
            Guard.IsNotNullOrWhitespace(key);
            Guard.IsNotNull(cacheGetter);

            var cacheOptions = options?.CastOrDefault<DistributedCacheOptions>() ?? new DistributedCacheOptions();
            _logger.Debug($"Checking distributed cache if an object with key <{key}> is cached");

            var bytes = await _cache.GetAsync(key, token);

            if (bytes.HasValue())
            {
                _logger.Debug($"Found cached object <{key}> in the distributed cache");
                var serializedString = bytes.ToString<TEncoding>();
                return cacheOptions.Deserializer(serializedString, typeof(T)).CastOrDefault<T>();
            }
            else
            {
                _logger.Debug($"No cached object with key <{key}>. Generating value");
                var valueToSet = await cacheGetter(token);

                if (valueToSet == null)
                {
                    _logger.Warning($"Null was returned so can't cache");
                    return valueToSet;
                }

                var cachingOptions = cacheOptions.Build(target);
                var serializedString = cacheOptions.Serializer(valueToSet);
                bytes = serializedString.GetBytes<TEncoding>();

                if (cachingOptions != null)
                {
                    _cache.Set(key, bytes, cachingOptions);
                    _logger.Trace($"Object <{serializedString}> was cached in the distributed cache with options <{cachingOptions}>");
                }
                else
                {
                    _cache.Set(key, bytes);
                    _logger.Trace($"Object <{serializedString}> was cached in the distributed cache");
                }

                return valueToSet;
            }
        }
    }

    internal class DistributedCacheOptions : IDistributedCacheOptions
    {
        // Fields
        private readonly List<Action<IInvocation, DistributedCacheEntryOptions>> _optionBuilders = new List<Action<IInvocation, DistributedCacheEntryOptions>>();

        // Properties
        public Func<object, string> Serializer { get; private set; }
        public Func<string, Type, object> Deserializer { get; private set; }

        /// <inheritdoc cref="DistributedCacheOptions"/>
        public DistributedCacheOptions()
        {
            this.Cast<IDistributedCacheOptions>().ConvertUsing(GenericConverter.DefaultJsonConverter);
        }

        /// <inheritdoc/>
        public IDistributedCacheOptions WithOptions(Action<IInvocation, DistributedCacheEntryOptions> optionBuilder)
        {
            Guard.IsNotNull(optionBuilder);
            _optionBuilders.Add(optionBuilder);
            return this;
        }

        /// <inheritdoc/>
        public IDistributedCacheOptions ConvertUsing(Func<object, string> serializeFunc, Func<string, Type, object> deserializeFunc)
        {
            Serializer = Guard.IsNotNull(serializeFunc);
            Deserializer = Guard.IsNotNull(deserializeFunc);

            return this;
        }

        /// <summary>
        /// Builds the options for the target method using the current configuration.
        /// </summary>
        /// <param name="invocation">The target to cache for</param>
        /// <returns>The cache options for <paramref name="invocation"/></returns>
        public DistributedCacheEntryOptions? Build(IInvocation invocation)
        {
            if (_optionBuilders.HasValue())
            {
                var options = new DistributedCacheEntryOptions();
                _optionBuilders.Execute(x => x(invocation, options));

                return options;
            }
            return null;
        }
    }
}
