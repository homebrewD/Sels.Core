using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sels.Core.Extensions
{
    /// <summary>
    /// Contains extensions for validating the argumetns supplied to methods/constructors.
    /// </summary>
    public static class ArgumentValidationExtensions
    {
        /// <summary>
        /// Validates argument to see if it passes the condition. Throws exception from validationFailedExceptionFunc when it fails validation.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="condition">Condition that argument needs to pass</param>
        /// <param name="validationFailedExceptionFunc">Func that creates exception when validation fails. First arg is supplied argument</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgument<T>(this T argument, Predicate<T> condition, Func<T, Exception> validationFailedExceptionFunc)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (validationFailedExceptionFunc == null) throw new ArgumentNullException(nameof(validationFailedExceptionFunc));

            if (!condition(argument))
            {
                throw validationFailedExceptionFunc(argument);
            }

            return argument;
        }

        /// <summary>
        /// Validates argument to see if it passes the condition. Throws ArgumentException with validationFailedMessage when it fails validation.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="condition">Condition that argument needs to pass</param>
        /// <param name="validationFailedMessage">Message for ArgumentException when validation fails</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgument<T>(this T argument, Predicate<T> condition, string validationFailedMessage)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (string.IsNullOrWhiteSpace(validationFailedMessage)) throw new ArgumentException($"{nameof(validationFailedMessage)} cannot be null, empty or whitespace");

            return argument.ValidateArgument(condition, x => new ArgumentException(validationFailedMessage));
        }

        /// <summary>
        /// Validates if argument is not null. Throws ArgumentNullException when argument is null.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgument<T>(this T argument, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => x != null, x => throw new ArgumentNullException(parameterName));
        }

        #region String
        /// <summary>
        /// Validates if argument is not null, empty or whitespace. Throws ArgumentException when it is.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <returns><paramref name="argument"/></returns>
        public static string ValidateArgumentNotNullOrEmpty(this string argument, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => !string.IsNullOrEmpty(x), $"{parameterName} cannot be null or empty");
        }

        /// <summary>
        /// Validates if argument is not null or empty. Throws ArgumentException when it is.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <returns><paramref name="argument"/></returns>
        public static string ValidateArgumentNotNullOrWhitespace(this string argument, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => !string.IsNullOrWhiteSpace(x), $"{parameterName} cannot be null, empty or whitespace");
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> is not null and ends with <paramref name="comparator"/>.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="comparator">What <paramref name="argument"/> must end with</param>
        /// <param name="option">How the strings should be compared</param>
        /// <returns><paramref name="argument"/></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ValidateArgumentEndsWith(this string argument, string parameterName, string comparator, StringComparison option = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));
            if (string.IsNullOrWhiteSpace(comparator)) throw new ArgumentException($"{nameof(comparator)} cannot be null, empty or whitespace", nameof(comparator));

            return argument.ValidateArgument(x => x != null && x.EndsWith(comparator, option), $"{parameterName} must end with {comparator}");
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> is not null and does not end with <paramref name="comparator"/>.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="comparator">What <paramref name="argument"/> can't end with</param>
        /// <param name="option">How the strings should be compared</param>
        /// <returns><paramref name="argument"/></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ValidateArgumentDoesNotEndWith(this string argument, string parameterName, string comparator, StringComparison option = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));
            if (string.IsNullOrWhiteSpace(comparator)) throw new ArgumentException($"{nameof(comparator)} cannot be null, empty or whitespace", nameof(comparator));

            return argument.ValidateArgument(x => x != null && !x.EndsWith(comparator, option), $"{parameterName} can't end with {comparator}");
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> is not null and starts with <paramref name="comparator"/>.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="comparator">What <paramref name="argument"/> must start with</param>
        /// <param name="option">How the strings should be compared</param>
        /// <returns><paramref name="argument"/></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ValidateArgumentStartsWith(this string argument, string parameterName, string comparator, StringComparison option = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));
            if (string.IsNullOrWhiteSpace(comparator)) throw new ArgumentException($"{nameof(comparator)} cannot be null, empty or whitespace", nameof(comparator));

            return argument.ValidateArgument(x => x != null && x.StartsWith(comparator, option), $"{parameterName} must start with {comparator}");
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> is not null and does not start with <paramref name="comparator"/>.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="comparator">What <paramref name="argument"/> can't start with</param>
        /// <param name="option">How the strings should be compared</param>
        /// <returns><paramref name="argument"/></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ValidateArgumentDoesNotStartWith(this string argument, string parameterName, string comparator, StringComparison option = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));
            if (string.IsNullOrWhiteSpace(comparator)) throw new ArgumentException($"{nameof(comparator)} cannot be null, empty or whitespace", nameof(comparator));

            return argument.ValidateArgument(x => x != null && !x.StartsWith(comparator, option), $"{parameterName} can't start with {comparator}");
        }
        #endregion

        #region Comparable
        /// <summary>
        /// Validates if argument is larger than comparator. Throws ArgumentException when it is not.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="comparator">Value to compare argument to</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgumentLarger<T>(this T argument, string parameterName, T comparator)
            where T : IComparable
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => argument.CompareTo(comparator) > 0, $"{parameterName} must be larger than <{comparator}>. Was <{argument}>");
        }

        /// <summary>
        /// Validates if argument is larger or equal to comparator. Throws ArgumentException when it is not.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="comparator">Value to compare argument to</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgumentLargerOrEqual<T>(this T argument, string parameterName, T comparator)
            where T : IComparable
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => argument.CompareTo(comparator) >= 0, $"{parameterName} must be larger or equal to <{comparator}>. Was <{argument}>");
        }

        /// <summary>
        /// Validates if argument is smaller than comparator. Throws ArgumentException when it is not.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="comparator">Value to compare argument to</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgumentSmaller<T>(this T argument, string parameterName, T comparator)
            where T : IComparable
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => argument.CompareTo(comparator) < 0, $"{parameterName} must be larger or equal to <{comparator}>. Was <{argument}>");
        }

        /// <summary>
        /// Validates if argument is smaller or equal to comparator. Throws ArgumentException when it is not.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="comparator">Value to compare argument to</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgumentSmallerOrEqual<T>(this T argument, string parameterName, T comparator)
            where T : IComparable
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => argument.CompareTo(comparator) <= 0, $"{parameterName} must be larger or equal to <{comparator}>. Was <{argument}>");
        }

        /// <summary>
        /// Validates if argument in range of startRange and endRange. Throws ArgumentException when it is not.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="startRange">Start value of range</param>
        /// <param name="endRange">End value of range</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgumentInRange<T>(this T argument, string parameterName, T startRange, T endRange)
            where T : IComparable
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => argument.CompareTo(startRange) >= 0 && argument.CompareTo(endRange) <= 0, $"{parameterName} must be in range of <{startRange}> and <{endRange}>. Was <{argument}>");
        }
        #endregion

        #region Collection
        /// <summary>
        /// Validates if argument is not null and contains at least 1 item. Throws ArgumentException when it does not.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <typeparam name="TItem">Type of item in collection</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgumentNotNullOrEmpty<T, TItem>(this T argument, string parameterName) where T : IEnumerable<TItem>
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => x.HasValue(), $"{parameterName} cannot be null and must contain at least 1 item");
        }

        /// <summary>
        /// Validates if argument is not null and contains at least 1 item. Throws ArgumentException when it does not.
        /// </summary>
        /// <typeparam name="T">Type of item in collection</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <returns><paramref name="argument"/></returns>
        public static IEnumerable<T> ValidateArgumentNotNullOrEmpty<T>(this IEnumerable<T> argument, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => x.HasValue(), $"{parameterName} cannot be null and must contain at least 1 item");
        }

        /// <summary>
        /// Validates if argument is not null and contains at least 1 item. Throws ArgumentException when it does not.
        /// </summary>
        /// <typeparam name="T">Type of item in collection</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <returns><paramref name="argument"/></returns>
        public static T[] ValidateArgumentNotNullOrEmpty<T>(this T[] argument, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => x.HasValue(), $"{parameterName} cannot be null and must contain at least 1 item");
        }
        #endregion

        #region Type
        /// <summary>
        /// Validates if argument is not null and is assignable from assignableType. Throws ArgumentException when it is not.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="assignableType">Type to check against</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgumentAssignableFrom<T>(this T argument, string parameterName, Type assignableType)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));
            if (assignableType == null) throw new ArgumentException($"{nameof(assignableType)} cannot be null");

            return argument.ValidateArgument(x => argument != null && argument.GetType().IsAssignableFrom(assignableType), $"{parameterName} cannot be null && must be assignable from Type <{assignableType}>");
        }

        /// <summary>
        /// Validates if argument is not null and assignableType is assignable from the type of argument. Throws ArgumentException when it is not.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="assignableType">Type to check against</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgumentAssignableTo<T>(this T argument, string parameterName, Type assignableType)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));
            if (assignableType == null) throw new ArgumentException($"{nameof(assignableType)} cannot be null");

            return argument.ValidateArgument(x => argument != null && assignableType.IsAssignableFrom(argument.GetType()), $"{parameterName} cannot be null && Type <{assignableType}> must be assignable from type <{argument.GetType()}>");
        }

        /// <summary>
        /// Validates if argument is not null and is assignable from assignableType. Throws ArgumentException when it is not.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="assignableType">Type to check against</param>
        /// <returns><paramref name="argument"/></returns>
        public static Type ValidateArgumentAssignableFrom(this Type argument, string parameterName, Type assignableType)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));
            if (assignableType == null) throw new ArgumentException($"{nameof(assignableType)} cannot be null");

            return argument.ValidateArgument(x => argument != null && argument.IsAssignableFrom(assignableType), $"{parameterName} cannot be null && <{argument}> must be assignable from Type <{assignableType}>");
        }

        /// <summary>
        /// Validates if argument is not null and assignableType is assignable from the argument. Throws ArgumentException when it is not.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="assignableType">Type to check against</param>
        /// <returns><paramref name="argument"/></returns>
        public static Type ValidateArgumentAssignableTo(this Type argument, string parameterName, Type assignableType)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));
            if (assignableType == null) throw new ArgumentException($"{nameof(assignableType)} cannot be null");

            return argument.ValidateArgument(x => argument != null && assignableType.IsAssignableFrom(argument), $"{parameterName} cannot be null && Type <{assignableType}> must be assignable from the type of {argument}");
        }
        /// <summary>
        /// Validates if argument is not null and is not an interface type.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <returns><paramref name="argument"/></returns>
        public static Type ValidateArgumentNotInterface(this Type argument, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => argument != null && !argument.IsInterface, $"{parameterName} cannot be null and can't be an interface");
        }

        /// <summary>
        /// Validates if argument is not null and is not an interface or abstract class.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <returns><paramref name="argument"/></returns>
        public static Type ValidateArgumentInstanceable(this Type argument, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => argument != null && !argument.IsInterface && !argument.IsAbstract, $"{parameterName} cannot be null, can't be an interface or abstract class");
        }

        /// <summary>
        /// Validates if an instance can be constructed from <paramref name="argument"/> using the supplied <paramref name="parameterTypes"/>.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="parameterTypes">Contructor argument types in order</param>
        /// <returns><paramref name="argument"/></returns>
        public static Type ValidateArgumentCanBeContructedWith(this Type argument, string parameterName, params Type[] parameterTypes)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => argument != null && argument.CanConstructWith(parameterTypes), $"{parameterName} cannot be null && must contain {(parameterTypes.HasValue() ? "a constructor that has the following parameters: " + parameterTypes.JoinString(", ") : "a no-arg constructor")}");
        }

        /// <summary>
        /// Validates if an instance can be constructed from <paramref name="argument"/> using the supplied <paramref name="arguments"/>.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="parameterName">Method/Constructor parameter name</param>
        /// <param name="arguments">Contructor arguments in order</param>
        /// <returns><paramref name="argument"/></returns>
        public static Type ValidateArgumentCanBeContructedWithArguments(this Type argument, string parameterName, params object[] arguments)
        {
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentException($"{nameof(parameterName)} cannot be null, empty or whitespace", nameof(parameterName));

            return argument.ValidateArgument(x => argument != null && argument.CanConstructWithArguments(arguments), $"{parameterName} cannot be null && must contain {(arguments.HasValue() ? "a constructor that has the following parameters: " + arguments.Select(x => x.GetTypeOrDefault()).JoinString(", ") : "a no-arg constructor")}");
        }
        #endregion

        #region FileSystem
        /// <summary>
        /// Validates if <paramref name="argument"/> is not null and exists on the file system.
        /// </summary>
        /// <param name="argument">Method/Constructor argument</param>
        /// <param name="validationFailedMessage">Message for ArgumentException when validation fails</param>
        /// <returns><paramref name="argument"/></returns>
        public static T ValidateArgumentExists<T>(this T argument, string validationFailedMessage) where T : FileSystemInfo
        {
            if (string.IsNullOrWhiteSpace(validationFailedMessage)) throw new ArgumentException($"{nameof(validationFailedMessage)} cannot be null, empty or whitespace");

            return argument.ValidateArgument(x => x.HasValue() && x.Exists, validationFailedMessage);
        }
        #endregion
    }
}
