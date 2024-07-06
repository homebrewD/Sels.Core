using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sels.Core.Extensions.Collections
{
    /// <summary>
    /// Contains extension methods for working with the various .NET collections.
    /// </summary>
    public static class CollectionExtensions
    {
        #region IEnumerable
        /// <summary>
        /// Yield return any object in <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Collection to get items from</param>
        /// <returns>Objects in <paramref name="source"/></returns>
        public static IEnumerable<object> Enumerate(this IEnumerable source)
        {
            source.ValidateArgument(nameof(source));

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Creates a new list from the elements in <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T">Collection element type</typeparam>
        /// <param name="collection">Collection to get elements from</param>
        /// <returns>List containing all elements in <paramref name="collection"/></returns>
        public static List<T> Copy<T>(this IEnumerable<T> collection)
        {
            collection.ValidateArgument(nameof(collection));

            return new List<T>(collection);
        }
        /// <summary>
        /// Gets the element at index <paramref name="index"/> in <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in <paramref name="collection"/></typeparam>
        /// <param name="collection">The collection to get the element from</param>
        /// <param name="index">The index of the element to get</param>
        /// <returns>The element at index <paramref name="index"/> in <paramref name="collection"/></returns>
        public static T GetAtIndex<T>(this IEnumerable<T> collection, int index)
        {
            collection.ValidateArgument(nameof(collection));
            index.ValidateArgumentLargerOrEqual(nameof(index), 0);

            var currentIndex = 0;
            foreach (var item in collection)
            {
                if (currentIndex == index)
                {
                    return item;
                }

                currentIndex++;
            }

            throw new IndexOutOfRangeException($"No element at index {index}");
        }
        /// <summary>
        /// Checks if all elements are unique in <paramref name="collection"/> by comparing with <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <typeparam name="T">Collection element type</typeparam>
        /// <param name="collection">Collection to check</param>
        /// <returns>Boolean indicating if all elements in <paramref name="collection"/> are unique</returns>
        public static bool AreAllUnique<T>(this IEnumerable<T> collection)
        {
            collection.ValidateArgument(nameof(collection));

            foreach (var item in collection)
            {
                var occuranceAmount = 0;
                foreach (var itemToCompare in collection)
                {
                    if (item.Equals(itemToCompare))
                    {
                        occuranceAmount++;
                    }

                    // Has to be 2 because an item counts itself at least once
                    if (occuranceAmount > 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if all element values selected by <paramref name="valueSelector"/> are unique in <paramref name="collection"/> by comparing with <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <typeparam name="T">Collection element type</typeparam>
        /// <param name="collection">Collection to check</param>
        /// <param name="valueSelector">Func that selects the value from <typeparamref name="T"/> to compare</param>
        /// <returns>Boolean indicating if all elements in <paramref name="collection"/> are unique</returns>
        public static bool AreAllUnique<T>(this IEnumerable<T> collection, Func<T, object> valueSelector)
        {
            collection.ValidateArgument(nameof(collection));
            valueSelector.ValidateArgument(nameof(valueSelector));

            foreach (var item in collection)
            {
                var itemValue = valueSelector(item);

                var occuranceAmount = 0;
                foreach (var itemToCompare in collection)
                {
                    var itemToCompareValue = valueSelector(itemToCompare);

                    if (itemValue.Equals(itemToCompareValue))
                    {
                        occuranceAmount++;
                    }

                    // Has to be 2 because an item counts itself at least once
                    if (occuranceAmount > 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if <paramref name="source"/> contains the same elements as <paramref name="other"/> regardless of order.
        /// </summary>
        /// <typeparam name="T">Type of the elements</typeparam>
        /// <param name="source">The source collection to compare to <paramref name="other"/></param>
        /// <param name="other">The collection to compare to <paramref name="source"/></param>
        /// <returns>True if <paramref name="source"/> and <paramref name="other"/> contains the same elements regardless of order</returns>
        public static bool UnorderedEquals<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            source.ValidateArgument(nameof(source));
            if (other == null) return false;
            if(source.GetCount() != other.GetCount()) return false;

            var checkedElements = new HashSet<T>();

            foreach(var element in source)
            {
                // We already checked so we skip
                if (checkedElements.Contains(element)) continue;

                // Compare source count to target count
                var sourceCount = source.Count(x => x.Equals(element));
                var matching = other.Count(x => x.Equals(element));
                if(sourceCount != matching) return false;

                // Count matches se we cache that value was already checked to avoid 
                checkedElements.Add(element);
            }

            return true;
        }

        #region Random
        /// <summary>
        /// Gets the index of a random element in <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in <paramref name="collection"/></typeparam>
        /// <param name="collection">The collection to get the random element index from</param>
        /// <returns>The index of a random element in <paramref name="collection"/> or -1 when <paramref name="collection"/> is empty</returns>
        public static int GetRandomIndex<T>(this IEnumerable<T> collection)
        {
            collection.ValidateArgument(nameof(collection));

            var count = collection.GetCount();
            return count > 0 ? Helper.Random.Instance.Next(0, count) : -1;
        }
        /// <summary>
        /// Gets a random element in <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in <paramref name="collection"/></typeparam>
        /// <param name="collection">The collection to get the random element from</param>
        /// <returns>A random element in <paramref name="collection"/></returns>
        public static T GetRandomItem<T>(this IEnumerable<T> collection)
        {
            collection.ValidateArgument(nameof(collection));

            var randomIndex = collection.GetRandomIndex();
            if(randomIndex < 0) throw new IndexOutOfRangeException("Collection was empty");
            return collection.GetAtIndex(randomIndex);
        }
        #endregion
        #endregion

        #region Grid
        /// <summary>
        /// Gets an array from all the elements in row <paramref name="rowIndex"/> in grid <paramref name="grid"/>.
        /// </summary>
        /// <param name="grid">The grid to get the row from</param>
        /// <param name="rowIndex">The index of the row to get</param>
        /// <returns>The columns of row at index <paramref name="rowIndex"/></returns>
        public static string[] GetRow(this string[,] grid, int rowIndex = 0)
        {
            grid.ValidateArgument(nameof(grid));
            rowIndex.ValidateArgumentLargerOrEqual(nameof(rowIndex), 0);

            var columnLength = grid.GetLength(1);

            var columns = new string[columnLength];

            for (int i = 0; i < columnLength; i++)
            {
                columns[i] = grid[rowIndex, i];
            }

            return columns;
        }
        #endregion

        #region List
        /// <summary>
        /// Inserts <paramref name="value"/> in <paramref name="list"/> before the first item matching <paramref name="predicate"/>. If no matching item is found it added to <paramref name="list"/>. 
        /// </summary>
        /// <typeparam name="T">Element type of list</typeparam>
        /// <param name="list">List to add element in</param>
        /// <param name="predicate">Predicate for finding object to insert before</param>
        /// <param name="value">Object to insert</param>
        public static void InsertBefore<T>(this IList<T> list, Predicate<T> predicate, T value)
        {
            list.ValidateArgument(nameof(list));
            predicate.ValidateArgument(nameof(predicate));

            var index = 0;
            foreach (var item in list)
            {
                if (predicate(item))
                {
                    list.Insert(index, value);
                    return;
                }
                index++;
            }

            list.Add(value);
        }
        #endregion

        #region Dictionary
        /// <summary>
        /// Adds all values in <paramref name="dictionaryToMerge"/> to <paramref name="dictionary"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="dictionary"/></typeparam>
        /// <typeparam name="TItem">Type of the elements in <paramref name="dictionary"/></typeparam>
        /// <param name="dictionary">The dictionary to add the values to</param>
        /// <param name="dictionaryToMerge">The dictionary with the values to add</param>
        public static void Merge<TKey, TItem>(this IDictionary<TKey, List<TItem>> dictionary, IDictionary<TKey, List<TItem>> dictionaryToMerge)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            dictionaryToMerge.ValidateArgument(nameof(dictionaryToMerge));

            foreach(var pair in dictionaryToMerge)
            {
                dictionary.AddValues(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Returns the value with key <paramref name="key"/> if it exists in <paramref name="dictionary"/>. Otherwise it will return the value returned by <paramref name="defaultValueConstructor"/> or the default of <typeparamref name="TValue"/> if no delegate is provided.
        /// </summary>
        /// <typeparam name="TKey">Type of the dictionary key</typeparam>
        /// <typeparam name="TValue">Type of the dictionary</typeparam>
        /// <param name="dictionary">Dictionary to get the value from</param>
        /// <param name="key">The key of the value to get</param>
        /// <param name="defaultValueConstructor">Optional delegate to create the value to return if <paramref name="dictionary"/> doesn't contain <paramref name="key"/></param>
        /// <returns>The value with key <paramref name="key"/> or the default value if <paramref name="dictionary"/> doesn't contain the provided key</returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueConstructor = null)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if(dictionary.ContainsKey(key)) return dictionary[key];

            return defaultValueConstructor != null ? defaultValueConstructor() : default(TValue);
        }

        /// <summary>
        /// Returns the value with key <paramref name="key"/> if it exists in <paramref name="dictionary"/>. Otherwise it will return the value returned by <paramref name="defaultValueConstructor"/> or the default of <typeparamref name="TValue"/> if no delegate is provided.
        /// </summary>
        /// <typeparam name="TKey">Type of the dictionary key</typeparam>
        /// <typeparam name="TValue">Type of the dictionary</typeparam>
        /// <param name="dictionary">Dictionary to get the value from</param>
        /// <param name="key">The key of the value to get</param>
        /// <param name="defaultValueConstructor">Optional delegate to create the value to return if <paramref name="dictionary"/> doesn't contain <paramref name="key"/></param>
        /// <returns>The value with key <paramref name="key"/> or the default value if <paramref name="dictionary"/> doesn't contain the provided key</returns>
        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueConstructor = null)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (dictionary.ContainsKey(key)) return dictionary[key];

            return defaultValueConstructor != null ? defaultValueConstructor() : default(TValue);
        }

        /// <summary>
        /// Returns the value with key <paramref name="key"/> if it exists in <paramref name="dictionary"/>. Otherwise it will return the value returned by <paramref name="defaultValueConstructor"/> or the default of <typeparamref name="T"/> if no delegate is provided.
        /// </summary>
        /// <typeparam name="T">The expected type of the value</typeparam>
        /// <param name="dictionary">Dictionary to get the value from</param>
        /// <param name="key">The key of the value to get</param>
        /// <param name="defaultValueConstructor">Optional delegate to create the value to return if <paramref name="dictionary"/> doesn't contain <paramref name="key"/></param>
        /// <returns>The value with key <paramref name="key"/> or the default value if <paramref name="dictionary"/> doesn't contain the provided key</returns>
        public static T GetOrDefault<T>(this IDictionary<string, object> dictionary, string key, Func<T> defaultValueConstructor = null)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (dictionary.ContainsKey(key)) return dictionary[key].CastTo<T>();

            return defaultValueConstructor != null ? defaultValueConstructor() : default(T);
        }

        /// <summary>
        /// Returns the value with key <paramref name="key"/> if it exists in <paramref name="dictionary"/>. Otherwise it will return the value returned by <paramref name="defaultValueConstructor"/> or the default of <typeparamref name="T"/> if no delegate is provided.
        /// </summary>
        /// <typeparam name="T">The expected type of the value</typeparam>
        /// <param name="dictionary">Dictionary to get the value from</param>
        /// <param name="key">The key of the value to get</param>
        /// <param name="defaultValueConstructor">Optional delegate to create the value to return if <paramref name="dictionary"/> doesn't contain <paramref name="key"/></param>
        /// <returns>The value with key <paramref name="key"/> or the default value if <paramref name="dictionary"/> doesn't contain the provided key</returns>
        public static T GetOrDefault<T>(this IReadOnlyDictionary<string, object> dictionary, string key, Func<T> defaultValueConstructor = null)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (dictionary.ContainsKey(key)) return dictionary[key].CastTo<T>();

            return defaultValueConstructor != null ? defaultValueConstructor() : default(T);
        }

        /// <summary>
        /// Tries to get the value of <paramref name="key"/> in <paramref name="dictionary"/> if it exists and is castable to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the value</typeparam>
        /// <param name="dictionary">THe dictionary to search</param>
        /// <param name="key">The key of the value to get</param>
        /// <param name="value">The value if it exists</param>
        /// <returns>True if <paramref name="dictionary"/> contains a value with <paramref name="key"/> that can be casted to <typeparamref name="T"/>, otherwise false</returns>
        public static bool TryGetValue<T>(this IReadOnlyDictionary<string, object> dictionary, string key, out T value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            value = default(T);

            if(dictionary.TryGetValue(key, out var existing) && existing is T castedValue)
            {
                value = castedValue;
                return true;
            }

            return false;
        }
        /// <summary>
        /// Tries to get the value of <paramref name="key"/> in <paramref name="dictionary"/> if it exists and is castable to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the value</typeparam>
        /// <param name="dictionary">THe dictionary to search</param>
        /// <param name="key">The key of the value to get</param>
        /// <param name="value">The value if it exists</param>
        /// <returns>True if <paramref name="dictionary"/> contains a value with <paramref name="key"/> that can be casted to <typeparamref name="T"/>, otherwise false</returns>
        public static bool TryGetValue<T>(this IDictionary<string, object> dictionary, string key, out T value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            value = default(T);

            if (dictionary.TryGetValue(key, out var existing) && existing is T castedValue)
            {
                value = castedValue;
                return true;
            }

            return false;
        }

        #region AddValue
        /// <summary>
        /// Adds <paramref name="value"/> to <paramref name="dictionary"/> if no entry with <paramref name="key"/> exists, otherwise the value is updated using <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="dictionary"/></typeparam>
        /// <typeparam name="TValue">Type of the value in <paramref name="dictionary"/></typeparam>
        /// <param name="dictionary">The dictionary to add the value to</param>
        /// <param name="key">The key to add to</param>
        /// <param name="value">The value to add</param>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
        /// <summary>
        /// Adds <paramref name="value"/> to <paramref name="dictionary"/> if no entry with <paramref name="key"/> exists.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="dictionary"/></typeparam>
        /// <typeparam name="TValue">Type of the value in <paramref name="dictionary"/></typeparam>
        /// <param name="dictionary">The dictionary to add the value to</param>
        /// <param name="key">The key to add to</param>
        /// <param name="value">The value to add</param>
        public static void AddIfMissing<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
        }
        /// <summary>
        /// Adds <paramref name="value"/> to <paramref name="dictionary"/> if no entry with <paramref name="key"/> exists or if the existing value is null.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="dictionary"/></typeparam>
        /// <typeparam name="TValue">Type of the value in <paramref name="dictionary"/></typeparam>
        /// <param name="dictionary">The dictionary to add the value to</param>
        /// <param name="key">The key to add to</param>
        /// <param name="value">The value to add</param>
        public static void AddIfMissingOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
            else if (dictionary[key] == null)
            {

               dictionary[key] = value;
            }
        }
        /// <summary>
        /// Adds <paramref name="value"/> to <paramref name="dictionary"/> if no entry with <paramref name="key"/> exists or if the existing value is the default value for the type.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="dictionary"/></typeparam>
        /// <typeparam name="TValue">Type of the value in <paramref name="dictionary"/></typeparam>
        /// <param name="dictionary">The dictionary to add the value to</param>
        /// <param name="key">The key to add to</param>
        /// <param name="value">The value to add</param>
        public static void AddIfMissingOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
            else if (dictionary[key] == null)
            {
                dictionary[key] = value;
            }
            else if (dictionary[key].Equals(dictionary[key].GetType().GetDefaultValue()))
            {
                dictionary[key] = value;
            }
        }
        /// <summary>
        /// Adds <paramref name="value"/> to <paramref name="dictionary"/> if no entry with <paramref name="key"/> exists, otherwise the value is updated using <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="dictionary"/></typeparam>
        /// <typeparam name="TValue">Type of the value in <paramref name="dictionary"/></typeparam>
        /// <param name="dictionary">The dictionary to add the value to</param>
        /// <param name="key">The key to add to</param>
        /// <param name="value">The value to add</param>
        /// <returns><paramref name="dictionary"/> for method chaining</returns>
        public static IDictionary<TKey, TValue> AddOrUpdateFluently<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            dictionary.AddOrUpdate(key, value);
            return dictionary;
        }
        /// <summary>
        /// Adds <paramref name="value"/> to <paramref name="dictionary"/> if no entry with <paramref name="key"/> exists.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="dictionary"/></typeparam>
        /// <typeparam name="TValue">Type of the value in <paramref name="dictionary"/></typeparam>
        /// <param name="dictionary">The dictionary to add the value to</param>
        /// <param name="key">The key to add to</param>
        /// <param name="value">The value to add</param>
        /// <returns><paramref name="dictionary"/> for method chaining</returns>
        public static IDictionary<TKey, TValue> AddIfMissingFluently<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            dictionary.AddIfMissing(key, value);
            return dictionary;
        }
        /// <summary>
        /// Adds <paramref name="value"/> to <paramref name="dictionary"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="dictionary"/></typeparam>
        /// <typeparam name="TValue">Type of the value in <paramref name="dictionary"/></typeparam>
        /// <param name="dictionary">The dictionary to add the value to</param>
        /// <param name="key">The key to add to</param>
        /// <param name="value">The value to add</param>
        /// <returns><paramref name="dictionary"/> for method chaining</returns>
        public static IDictionary<TKey, TValue> AddFluently<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            dictionary.Add(key, value);
            return dictionary;
        }

        /// <summary>
        /// Adds <paramref name="item"/> to the list of <paramref name="key"/> if <paramref name="key"/> exists in <paramref name="dictionary"/>, otherwise create new list and add <paramref name="item"/> to it.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TItem">Collection type</typeparam>
        /// <param name="dictionary">Dictionary to add item to</param>
        /// <param name="key">Key for list</param>
        /// <param name="item">Item to add</param>
        public static void AddValueToList<TKey, TItem>(this IDictionary<TKey, List<TItem>> dictionary, TKey key, TItem item)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(item);
            }
            else
            {
                var newList = new List<TItem>();
                newList.Add(item);
                dictionary.Add(key, newList);
            }
        }

        /// <summary>
        /// Adds <paramref name="items"/> to the list of <paramref name="key"/> if <paramref name="key"/> exists in <paramref name="dictionary"/>, otherwise create new list and add <paramref name="items"/> to it.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TItem">Collection type</typeparam>
        /// <param name="dictionary">Dictionary to add item to</param>
        /// <param name="key">Key for list</param>
        /// <param name="items">Items to add</param>
        public static void AddValues<TKey, TItem>(this IDictionary<TKey, List<TItem>> dictionary, TKey key, IEnumerable<TItem> items)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            items.ValidateArgument(nameof(items));

            if (dictionary.ContainsKey(key))
            {
                dictionary[key].AddRange(items);
            }
            else
            {
                dictionary.Add(key, new List<TItem>(items));
            }
        }
        #endregion

        #region TryGetOrSet
        /// <summary>
        /// Gets the value with key <paramref name="key"/> from <paramref name="dictionary"/> if it exists, otherwise <paramref name="value"/> is added and returned.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="dictionary"/></typeparam>
        /// <typeparam name="TValue">Type of the value in <paramref name="dictionary"/></typeparam>
        /// <param name="dictionary">The dictionary to get the value from</param>
        /// <param name="key">The key of the value to get</param>
        /// <param name="value">The value to add if no value is stored under key <paramref name="key"/></param>
        /// <returns>The value under key <paramref name="key"/>, otherwise <paramref name="value"/></returns>
        public static TValue TryGetOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            return dictionary.TryGetOrSet(key, () => value);
        }
        /// <summary>
        /// Gets the value with key <paramref name="key"/> from <paramref name="dictionary"/> if it exists, otherwise the value created using <paramref name="valueFunc"/> is added and returned.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in <paramref name="dictionary"/></typeparam>
        /// <typeparam name="TValue">Type of the value in <paramref name="dictionary"/></typeparam>
        /// <param name="dictionary">The dictionary to get the value from</param>
        /// <param name="key">The key of the value to get</param>
        /// <param name="valueFunc">The function that creates the value to add if no value is stored under key <paramref name="key"/></param>
        /// <returns>The value under key <paramref name="key"/>, otherwise the value created from <paramref name="valueFunc"/></returns>
        public static TValue TryGetOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFunc)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            valueFunc.ValidateArgument(nameof(valueFunc));

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                var value = valueFunc();
                dictionary.Add(key, value);
                return value;
            }
        }
        #endregion

        #region ContainsItem
        /// <summary>
        /// Checks if the enumerator at key <paramref name="key"/> in <paramref name="dictionary"/> contains <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TItem">Type of enumerator element</typeparam>
        /// <param name="dictionary">The dictionary to check</param>
        /// <param name="key">The key to check</param>
        /// <param name="item">The item to check for</param>
        /// <returns>True if the enumerator at key <paramref name="key"/> in <paramref name="dictionary"/> contains <paramref name="item"/>, otherwise false</returns>
        public static bool ContainsItem<TKey, TItem>(this IDictionary<TKey, IEnumerable<TItem>> dictionary, TKey key, TItem item)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            item.ValidateArgument(nameof(item));

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key]?.Contains(item) ?? false;
            }

            return false;
        }
        /// <summary>
        /// Checks if the list at key <paramref name="key"/> in <paramref name="dictionary"/> contains <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TItem">Type of list element</typeparam>
        /// <param name="dictionary">The dictionary to check</param>
        /// <param name="key">The key to check</param>
        /// <param name="item">The item to check for</param>
        /// <returns>True if the list at key <paramref name="key"/> in <paramref name="dictionary"/> contains <paramref name="item"/>, otherwise false</returns>
        public static bool ContainsItem<TKey, TItem>(this IDictionary<TKey, List<TItem>> dictionary, TKey key, TItem item)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            item.ValidateArgument(nameof(item));

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key]?.Contains(item) ?? false;
            }

            return false;
        }
        /// <summary>
        /// Checks if the collection at key <paramref name="key"/> in <paramref name="dictionary"/> contains <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TItem">Type of list element</typeparam>
        /// <param name="dictionary">The dictionary to check</param>
        /// <param name="key">The key to check</param>
        /// <param name="item">The item to check for</param>
        /// <returns>True if the collection at key <paramref name="key"/> in <paramref name="dictionary"/> contains <paramref name="item"/>, otherwise false</returns>
        public static bool ContainsItem<TKey, TItem>(this Dictionary<TKey, Collection<TItem>> dictionary, TKey key, TItem item)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            item.ValidateArgument(nameof(item));

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key]?.Contains(item) ?? false;
            }

            return false;
        }
        #endregion
        #endregion
    }
}
