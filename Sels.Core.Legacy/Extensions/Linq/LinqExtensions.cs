using Sels.Core.Extensions.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sels.Core.Extensions.Linq
{
    /// <summary>
    /// Extra extension methods that follow the same setup as Linq.
    /// </summary>
    public static class LinqExtensions
    {
        #region Count
        /// <summary>
        /// Checks how many items are in <paramref name="source"/>. Checks common collection types first to avoid having to enumerate <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of items in <paramref name="source"/></typeparam>
        /// <param name="source">Enumerator to check</param>
        /// <returns>Item count of <paramref name="source"/></returns>
        public static int GetCount<T>(this IEnumerable<T> source)
        {
            source.ValidateArgument(nameof(source));

            if (source is T[] array)
            {
                return array.Length;
            }

            if (source is IList<T> list)
            {
                return list.Count;
            }

            if (source is IReadOnlyCollection<T> collection)
            {
                return collection.Count;
            }

            return source.Count();
        }
        /// <summary>
        /// Checks how many items are in <paramref name="source"/>. Checks common collection types first to avoid having to enumerate <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of items in <paramref name="source"/></typeparam>
        /// <param name="source">Enumerator to check</param>
        /// <returns>Item count of <paramref name="source"/></returns>
        public static int GetCount(this IEnumerable source)
        {
            source.ValidateArgument(nameof(source));

            if (source is Array array)
            {
                return array.Length;
            }

            if (source is IList list)
            {
                return list.Count;
            }

            if (source is ICollection collection)
            {
                return collection.Count;
            }

            return source.Enumerate().Count();
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects a new value of type <typeparamref name="TSelect"/> from <typeparamref name="T"/> for each element in <paramref name="source"/>. If <paramref name="source"/> is null an empty enumerator is returned.
        /// </summary>
        /// <typeparam name="T">The type to select a new value from</typeparam>
        /// <typeparam name="TSelect">The type selected from <typeparamref name="T"/></typeparam>
        /// <param name="source">Enumerator with the elements to select a new value from</param>
        /// <param name="selector">The func that selects the new value of type <typeparamref name="TSelect"/> from <typeparamref name="T"/></param>
        /// <returns>An enumerator returning all selected values of type <typeparamref name="TSelect"/> from all elements of type  <typeparamref name="T"/> in <paramref name="source"/></returns>
        public static IEnumerable<TSelect> SelectOrDefault<T, TSelect>(this IEnumerable<T> source, Func<T, TSelect> selector)
        {
            selector.ValidateArgument(nameof(selector));

            if (source.HasValue())
            {
                foreach (var item in source)
                {
                    yield return selector(item);
                }
            }
        }

        /// <summary>
        /// Selects a new value of type <typeparamref name="TSelect"/> from <typeparamref name="T"/> for each element in <paramref name="source"/>. If <paramref name="source"/> is null an empty enumerator is returned.
        /// </summary>
        /// <typeparam name="T">The type to select a new value from</typeparam>
        /// <typeparam name="TSelect">The type selected from <typeparamref name="T"/></typeparam>
        /// <param name="source">Enumerator with the elements to select a new value from</param>
        /// <param name="selector">The func that selects the new value of type <typeparamref name="TSelect"/> from <typeparamref name="T"/>. First arg is the index, second arg is the element</param>
        /// <returns>An enumerator returning all selected values of type <typeparamref name="TSelect"/> from all elements of type  <typeparamref name="T"/> in <paramref name="source"/></returns>
        public static IEnumerable<TSelect> SelectOrDefault<T, TSelect>(this IEnumerable<T> source, Func<int, T, TSelect> selector)
        {
            selector.ValidateArgument(nameof(selector));

            if (source.HasValue())
            {
                var counter = 0;
                foreach (var item in source)
                {
                    yield return selector(counter, item);
                    counter++;
                }
            }
        }
        #endregion

        #region ForceSelect
        /// <summary>
        /// Projects each element of a sequence into a new form. Exception are caught so execution doesn't stop.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TSelect">The type of the value returned by selector.</typeparam>
        /// <param name="source"> A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="exceptionHandler">Optional action for handling exceptions. First argument is the element that caused the exception en the second arg is the thrown exception</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 whose elements are the result of invoking the transform function on each element of source.</returns>
        public static IEnumerable<TSelect> ForceSelect<TSource, TSelect>(this IEnumerable<TSource> source, Func<TSource, TSelect> selector, Action<TSource, Exception> exceptionHandler = null)
        {
            if (source.HasValue())
            {
                foreach(var item in source)
                {

                    TSelect selectedItem;
                    try
                    {
                        selectedItem = selector(item);
                    }
                    catch(Exception ex)
                    {
                        try
                        {
                            if (exceptionHandler.HasValue())
                            {
                                exceptionHandler(item, ex);
                            }
                        }
                        catch { }

                        continue;
                    }

                    yield return selectedItem;
                }
            }
        }
        #endregion

        #region Modify 
        /// <summary>
        /// Modifies the elements in <paramref name="source"/> matching <paramref name="condition"/> using <paramref name="modifier"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="source"/></typeparam>
        /// <param name="source">The array to check</param>
        /// <param name="condition">The conditions that elements must pass before they are modified</param>
        /// <param name="modifier">The delegate that modifies the matching elements</param>
        /// <returns><paramref name="source"/> with the modified elements</returns>
        public static T[] Modify<T>(this T[] source, Predicate<T> condition, Func<T,T> modifier)
        {
            condition.ValidateArgument(nameof(condition));
            modifier.ValidateArgument(nameof(modifier));

            if (source.HasValue())
            {
                for(int i = 0; i < source.Length; i++)
                {
                    var item = source[i];

                    if (condition(item))
                    {
                        source[i] = modifier(item);
                    }
                }
            }

            return source;
        }
        #endregion

        #region Execution
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <returns><paramref name="source"/></returns>
        public static IEnumerable<T> Execute<T>(this IEnumerable<T> source, Action<T> action)
        {
            action.ValidateArgument(nameof(action));
            if (source != null)
            {
                foreach (var item in source)
                {
                    action(item);
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
        public static IEnumerable<T> Execute<T>(this IEnumerable<T> source, Action<T> action, Action<T, Exception> exceptionHandler)
        {
            action.ValidateArgument(nameof(action));
            exceptionHandler.ValidateArgument(nameof(exceptionHandler));

            if (source != null)
            {
                foreach (var item in source)
                {
                    try
                    {
                        action(item);
                    }
                    catch (Exception ex)
                    {
                        exceptionHandler(item, ex);
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
        /// <returns><paramref name="source"/></returns>
        public static IEnumerable<T> ForceExecute<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source != null)
            {
                foreach (var item in source)
                {
                    try
                    {
                        action(item);
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
        public static IEnumerable<T> ForceExecute<T>(this IEnumerable<T> source, Action<T> action, Action<T, Exception> exceptionHandler)
        {
            if (source != null)
            {
                foreach (var item in source)
                {
                    try
                    {
                        action(item);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            exceptionHandler(item, ex);
                        }
                        catch { }
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
        /// <returns><paramref name="source"/></returns>
        public static IEnumerable<T> Execute<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            action.ValidateArgument(nameof(action));

            if (source != null)
            {
                var counter = 0;
                foreach (var item in source)
                {
                    action(counter, item);
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
        public static IEnumerable<T> Execute<T>(this IEnumerable<T> source, Action<int, T> action, Action<int, T, Exception> exceptionHandler)
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
                        action(counter, item);
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        exceptionHandler(counter, item, ex);
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
        /// <returns><paramref name="source"/></returns>
        public static IEnumerable<T> ForceExecute<T>(this IEnumerable<T> source, Action<int, T> action)
        {

            if (source != null)
            {
                var counter = 0;
                foreach (var item in source)
                {
                    try
                    {
                        action(counter, item);
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
        public static IEnumerable<T> ForceExecute<T>(this IEnumerable<T> source, Action<int, T> action, Action<int, T, Exception> exceptionHandler)
        {
            if (source != null)
            {
                var counter = 0;
                foreach (var item in source)
                {
                    try
                    {
                        action(counter, item);
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            exceptionHandler(counter, item, ex);
                        }
                        catch { }
                    }

                }
            }

            return source;
        }
        #endregion

        #region Trim
        /// <summary>
        /// Returns an enumerator that trims the values in <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Strings to trim</param>
        /// <returns>An enumerator that trims each element</returns>
        public static IEnumerable<string> Trim(this IEnumerable<string> source)
        {
            source.ValidateArgument(nameof(source));

            return source.Select(x => x.Trim());
        }
        #endregion

        #region TryGetFirst
        /// <summary>
        /// Tries to get the first instance in <paramref name="source"/> matching <paramref name="condition"/>.
        /// </summary>
        /// <typeparam name="T">Type of instance to get</typeparam>
        /// <param name="source">Enumerator which returns the instances to check</param>
        /// <param name="condition">What condition an instance in <paramref name="source"/> must pass before it is returned</param>
        /// <param name="first">The first instance in <paramref name="source"/> matching <paramref name="condition"/> or null if no instance matched <paramref name="condition"/></param>
        /// <returns>If an instance ;atching <paramref name="condition"/> was found</returns>
        public static bool TryGetFirst<T>(this IEnumerable<T> source, Predicate<T> condition, out T first)
        {
            first = default;

            if(source != null)
            {
                foreach (var item in source)
                {
                    if (condition(item))
                    {
                        first = item;
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region IndexOf
        /// <summary>
        /// Returns the index of the first item matching <paramref name="condition"/> in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The type of the items to search</typeparam>
        /// <param name="source">The enumerator containing the items to search</param>
        /// <param name="condition">The condition that will be checked against the items returned by <paramref name="condition"/>. The index of the first matching will be returned</param>
        /// <returns>The index of the first item matching <paramref name="condition"/> in <paramref name="source"/> or -1 if nothing matches</returns>
        public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> condition)
        {
            source.ValidateArgument(nameof(source));
            condition.ValidateArgument(nameof(condition));

            var index = 0;
            foreach (var item in source)
            {
                if (condition(item))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        /// <summary>
        /// The return the index of <paramref name="item"/> in <paramref name="source"/>. Uses <see cref="object.Equals(object)"/> to compare items.
        /// </summary>
        /// <typeparam name="T">The type of the items to search</typeparam>
        /// <param name="source">The enumerator containing the items to search</param>
        /// <param name="item">The item to get the index for</param>
        /// <returns>The index of <paramref name="item"/> in <paramref name="source"/> or -1 if <paramref name="item"/> is not in <paramref name="source"/></returns>
        public static int IndexOf<T>(this IEnumerable<T> source, T item)
        {
            source.ValidateArgument(nameof(source));

            return source.IndexOf(x => x.Equals(item));
        }
        #endregion
    }
}
