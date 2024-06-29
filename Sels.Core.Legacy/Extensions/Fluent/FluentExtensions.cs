using System;
using System.Collections.Generic;

namespace Sels.Core.Extensions.Fluent
{
    /// <summary>
    /// Contains static extension methods for working with api's that use a fluent syntax.
    /// </summary>
    public static class FluentExtensions
    {
        #region ForEach
        /// <summary>
        /// Executes <paramref name="action"/> for each item in <paramref name="items"/> on <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of the fluent object</typeparam>
        /// <typeparam name="TItem">Type of the items to perform the action with</typeparam>
        /// <param name="source">The fluent object to perform the actions on</param>
        /// <param name="items">Enumerator returning the items to perform <paramref name="action"/> with. Can be null</param>
        /// <param name="action">Delegate that executes the action. First arg is the last returned fluent object, second arg is the counter and last arg is the item to perform the action with.
        /// The object returned will be the used for the next item. When null is returned the last non-null object is returned</param>
        /// <returns><paramref name="source"/> or the last object returned by <paramref name="action"/></returns>
        public static T ForEach<T, TItem>(this T source, IEnumerable<TItem> items, Func<T, int, TItem, T> action)
        {
            source.ValidateArgument(nameof(source));
            action.ValidateArgument(nameof(action));

            var counter = 0;
            if(items != null)
            {
                foreach (var item in items)
                {
                    var result = action(source, counter, item);
                    if (result == null) return source;
                    source = result;
                    counter++;
                }
            }         

            return source;
        }
        #endregion

        #region When
        /// <summary>
        /// Executes <paramref name="action"/> when <paramref name="condition"/> is set to true.
        /// </summary>
        /// <typeparam name="T">Type of the fluent object</typeparam>
        /// <param name="source">The fluent object to perform the actions on</param>
        /// <param name="condition">Boolean that indicates if we can execute <paramref name="action"/></param>
        /// <param name="action">The action to execute with <paramref name="source"/></param>
        /// <returns><paramref name="source"/> or the return value from <paramref name="action"/> when <paramref name="condition"/> was true</returns>
        public static T When<T>(this T source, bool condition, Func<T, T> action)
        {
            source.ValidateArgument(nameof(source));
            action.ValidateArgument(nameof(action));

            if (condition)
            {
                return action(source);
            }

            return source;
        }
        /// <summary>
        /// Executes <paramref name="action"/> when <paramref name="condition"/> is set to true for each item in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of the fluent object</typeparam>
        /// <param name="source">The fluent object to perform the actions on</param>
        /// <param name="condition">Boolean that indicates if we can execute <paramref name="action"/></param>
        /// <param name="action">The action to execute with <paramref name="source"/></param>
        /// <returns><paramref name="source"/> or the return value from <paramref name="action"/> when <paramref name="condition"/> was true</returns>
        public static IEnumerable<T> WhenForEach<T>(this IEnumerable<T> source, bool condition, Func<T, T> action)
        {
            source.ValidateArgument(nameof(source));
            action.ValidateArgument(nameof(action));

            if (condition)
            {
                foreach(var item in source)
                {
                   yield return action(item);
                }
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }
        /// <summary>
        /// Executes <paramref name="action"/> when <paramref name="condition"/> is set to true.
        /// </summary>
        /// <typeparam name="T">Type of the fluent object</typeparam>
        /// <typeparam name="TReturn">The type to return</typeparam>
        /// <param name="source">The fluent object to perform the actions on</param>
        /// <param name="condition">Boolean that indicates if we can execute <paramref name="action"/></param>
        /// <param name="action">The action to execute with <paramref name="source"/> if <paramref name="condition"/> is set true</param>
        /// <param name="elseAction">The action to execute with <paramref name="source"/> if <paramref name="condition"/> is set false</param>
        /// <returns>The value returned from <paramref name="action"/> when <paramref name="condition"/> is true, otherwise the return from <paramref name="elseAction"/></returns>
        public static TReturn When<T, TReturn>(this T source, bool condition, Func<T, TReturn> action, Func<T, TReturn> elseAction)
        {
            source.ValidateArgument(nameof(source));
            action.ValidateArgument(nameof(action));
            elseAction.ValidateArgument(nameof(elseAction));

            if (condition)
            {
                return action(source);
            }
            else
            {
                return elseAction(source);
            }

        }
        #endregion
    }
}
