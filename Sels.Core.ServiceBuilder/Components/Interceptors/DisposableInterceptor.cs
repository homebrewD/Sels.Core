using Castle.Core.Logging;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using Sels.Core.Dispose;
using Sels.Core.ServiceBuilder.Template.Interceptors;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.ServiceBuilder.Interceptors
{
    /// <summary>
    /// Interceptor that throws an <see cref="ObjectDisposedException"/> when the proxy target implements <see cref="IDisposableState"/> and the state indicates that the object is disposing/is disposed.
    /// </summary>
    public class DisposableInterceptor : BaseResultlessInterceptor
    {
        // Statis
        private static MethodInfo DisposeMethod = Helper.Expressions.Method.GetMethod<IDisposable>(x => x.Dispose());
        private static MethodInfo DisposeAsyncMethod = Helper.Expressions.Method.GetMethod<IAsyncDisposable>(x => x.DisposeAsync());

        // Fields
        private readonly ILogger _logger;

        /// <inheritdoc cref="DisposableInterceptor"/>
        /// <param name="logger">Optional logger for tracing</param>
        public DisposableInterceptor(ILogger<DisposableInterceptor> logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            if(invocation.InvocationTarget is IDisposableState disposable)
            {
                if(invocation.Method.AreEqual(DisposeMethod) || invocation.Method.AreEqual(DisposeAsyncMethod))
                {
                    _logger.Log($"Method target is the dispose method ({invocation.Method.Name}). Allowing execution");
                }
                else if (disposable.IsDisposed.HasValue)
                {
                    _logger.Log($"Dispose state for <{invocation.InvocationTarget.GetType().GetDisplayName()}> is <{disposable.IsDisposed}>. Throwing exception");

                    throw new ObjectDisposedException(invocation.InvocationTarget.GetType().FullName, $"Instance is {(disposable.IsDisposed.Value ? "disposed" : "disposing")}");
                }
                else
                {
                    _logger.Log($"Target <{invocation.InvocationTarget.GetType().GetDisplayName()}> is not disposed. Allowing execution");
                }
            }
            else
            {
                _logger.Log($"Target <{invocation.InvocationTarget.GetType().GetDisplayName()}> does not expose the disposable state. Allowing execution");
            }
            return proceed(invocation, proceedInfo);
        }
    }
}
