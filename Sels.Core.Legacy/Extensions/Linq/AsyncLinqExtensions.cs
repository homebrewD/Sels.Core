using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Extensions.Linq
{
    /// <summary>
    /// Contains exension method similar to linq but for async operations.
    /// </summary>
    public static class AsyncLinqExtensions
    {
        #region Execution
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <returns><paramref name="source"/></returns>
        public static async Task<IEnumerable<T>> ExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<T> action)
        {
            action.ValidateArgument(nameof(action));
            if (source != null)
            {
                foreach (var item in source)
                {
                    await action(item).ConfigureAwait(false);
                }
            }

            return source;
        }
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <param name="exceptionHandler">Delegate that handles exceptions thrown by <paramref name="action"/> before the exception is rethrown</param>
        /// <returns><paramref name="source"/></returns>
        public static async Task<IEnumerable<T>> ExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<T> action, AsyncAction<T, Exception> exceptionHandler)
        {
            action.ValidateArgument(nameof(action));
            exceptionHandler.ValidateArgument(nameof(exceptionHandler));

            if (source != null)
            {
                foreach (var item in source)
                {
                    try
                    {
                        await action(item).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await exceptionHandler(item, ex).ConfigureAwait(false);
                        throw;
                    }
                }
            }

            return source;
        }
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <param name="exceptionHandler">Delegate that handles exceptions thrown by <paramref name="action"/> before the exception is rethrown</param>
        /// <returns><paramref name="source"/></returns>
        public static Task<IEnumerable<T>> ExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<T> action, Action<T, Exception> exceptionHandler)
        => ExecuteAsync(source, action, (item, ex) => { exceptionHandler(item, ex); return Task.CompletedTask; });
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <returns><paramref name="source"/></returns>
        public static async Task<IEnumerable<T>> ForceExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<T> action)
        {
            if (source != null)
            {
                foreach (var item in source)
                {
                    try
                    {
                        await action(item).ConfigureAwait(false);
                    }
                    catch { }
                }
            }

            return source;
        }
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>. Any exceptions thrown are caught and not rethrown.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <param name="exceptionHandler">Delegate that handles exceptions thrown by <paramref name="action"/> before the exception is rethrown</param>
        /// <returns><paramref name="source"/></returns>
        public static async Task<IEnumerable<T>> ForceExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<T> action, AsyncAction<T, Exception> exceptionHandler)
        {
            if (source != null)
            {
                foreach (var item in source)
                {
                    try
                    {
                        await action(item).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            await exceptionHandler(item, ex).ConfigureAwait(false);
                        }
                        catch { }
                    }

                }
            }

            return source;
        }
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>. Any exceptions thrown are caught and not rethrown.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <param name="exceptionHandler">Delegate that handles exceptions thrown by <paramref name="action"/> before the exception is rethrown</param>
        /// <returns><paramref name="source"/></returns>
        public static Task<IEnumerable<T>> ForceExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<T> action, Action<T, Exception> exceptionHandler)
        => ForceExecuteAsync(source, action, (item, ex) => { exceptionHandler(item, ex); return Task.CompletedTask; });
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <returns><paramref name="source"/></returns>
        public static async Task<IEnumerable<T>> ExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<int, T> action)
        {
            action.ValidateArgument(nameof(action));

            if (source != null)
            {
                var counter = 0;
                foreach (var item in source)
                {
                    await action(counter, item).ConfigureAwait(false);
                    counter++;
                }
            }

            return source;
        }
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <param name="exceptionHandler">Delegate that handles exceptions thrown by <paramref name="action"/> before the exception is rethrown</param>
        /// <returns><paramref name="source"/></returns>
        public static async Task<IEnumerable<T>> ExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<int, T> action, AsyncAction<int, T, Exception> exceptionHandler)
        {
            action.ValidateArgument(nameof(action));
            exceptionHandler.ValidateArgument(nameof(exceptionHandler));

            if (source != null)
            {
                var counter = 0;
                foreach (var item in source)
                {
                    try
                    {
                        await action(counter, item).ConfigureAwait(false);
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        await exceptionHandler(counter, item, ex).ConfigureAwait(false);
                        throw;
                    }
                }
            }

            return source;
        }
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <param name="exceptionHandler">Delegate that handles exceptions thrown by <paramref name="action"/> before the exception is rethrown</param>
        /// <returns><paramref name="source"/></returns>
        public static Task<IEnumerable<T>> ExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<int, T> action, Action<int, T, Exception> exceptionHandler)
        => ExecuteAsync(source, action, (counter, item, ex) => { exceptionHandler(counter, item, ex); return Task.CompletedTask; });
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <returns><paramref name="source"/></returns>
        public static async Task<IEnumerable<T>> ForceExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<int, T> action)
        {

            if (source != null)
            {
                var counter = 0;
                foreach (var item in source)
                {
                    try
                    {
                        await action(counter, item).ConfigureAwait(false);
                    }
                    catch { }
                    counter++;
                }
            }

            return source;
        }
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>. Any exceptions thrown are caught and not rethrown.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <param name="exceptionHandler">Delegate that handles exceptions thrown by <paramref name="action"/> before the exception is rethrown</param>
        /// <returns><paramref name="source"/></returns>
        public static async Task<IEnumerable<T>> ForceExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<int, T> action, AsyncAction<int, T, Exception> exceptionHandler)
        {
            if (source != null)
            {
                var counter = 0;
                foreach (var item in source)
                {
                    try
                    {
                        await action(counter, item).ConfigureAwait(false);
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            await exceptionHandler(counter, item, ex).ConfigureAwait(false);
                        }
                        catch { }
                    }

                }
            }

            return source;
        }
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>. Any exceptions thrown are caught and not rethrown.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <param name="exceptionHandler">Delegate that handles exceptions thrown by <paramref name="action"/> before the exception is rethrown</param>
        /// <returns><paramref name="source"/></returns>
        public static Task<IEnumerable<T>> ForceExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<int, T> action, Action<int, T, Exception> exceptionHandler)
        => ForceExecuteAsync(source, action, (counter, item, ex) => { exceptionHandler(counter, item, ex); return Task.CompletedTask; });
        #endregion
    }
}
