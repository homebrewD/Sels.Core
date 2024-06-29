using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Sels.Core
{
    /// <summary>
    /// Contains static helper methods for validating method/constructor parameters.
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Checks if <paramref name="condition"/> passes. If it doesn't an <see cref="ArgumentException"/> will be thrown with the message created from <paramref name="errorMessageConstructor"/>.
        /// </summary>
        /// <param name="condition">The condition that must pass</param>
        /// <param name="errorMessageConstructor">Delegate that creates the error message</param>
        public static void Is(Func<bool> condition, Func<string> errorMessageConstructor)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (errorMessageConstructor == null) throw new ArgumentException(nameof(errorMessageConstructor));

            if (!condition()) throw new ArgumentException(errorMessageConstructor());
        }

        /// <summary>
        /// Checks if <paramref name="condition"/> passes. If it doesn't the exception created by <paramref name="errorMessageConstructor"/> will be thrown.
        /// </summary>
        /// <param name="condition">The condition that must pass</param>
        /// <param name="errorMessageConstructor">Delegate that creates the exception to throw</param>
        public static void Is(Func<bool> condition, Func<Exception> errorMessageConstructor)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (errorMessageConstructor == null) throw new ArgumentException(nameof(errorMessageConstructor));

            if (!condition()) throw errorMessageConstructor();
        }

        /// <summary>
        /// Values that <paramref name="argument"/> passes <paramref name="condition"/>. If it doesn't an <see cref="ArgumentException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the argument being validated</typeparam>
        /// <param name="argument">The argument being validated</param>
        /// <param name="condition">The predicate that checks if <paramref name="argument"/> is valid</param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/> if it's valid</returns>
        public static T Is<T>(T argument, Expression<Predicate<T>> condition, [CallerArgumentExpression("argument")] string expression = "")
        {
            condition.ValidateArgument(nameof(condition));
            expression.ValidateArgumentNotNullOrWhitespace(nameof(expression));

            if (!condition.Compile().Invoke(argument)) throw new ArgumentException($"{expression} did not pass condition <{condition}>");
            return argument;
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> is not null, if it is a <see cref="ArgumentNullException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">Type of the argument to check</typeparam>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return : NotNull]
        public static T IsNotNull<T>(T argument, [CallerArgumentExpression("argument")] string expression = "")
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");

            if (argument == null) throw new ArgumentNullException(expression);
            return argument;
        }

        #region String
        /// <summary>
        /// Validates that <paramref name="argument"/> is not null or empty, if it is an <see cref="ArgumentException"/> will be thrown.
        /// </summary>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return: NotNull]
        public static string IsNotNullOrEmpty(string argument, [CallerArgumentExpression("argument")] string expression = "")
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");

            if (string.IsNullOrEmpty(argument)) throw new ArgumentException($"{expression} cannot be null or empty");
            return argument;
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> is not null or empty, if it is an <see cref="ArgumentException"/> will be thrown.
        /// </summary>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return: NotNull]
        public static string IsNotNullOrWhitespace(string argument, [CallerArgumentExpression("argument")] string expression = "")
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");

            if (string.IsNullOrWhiteSpace(argument)) throw new ArgumentException($"{expression} cannot be null ,empty or whitespace");
            return argument;
        }
        #endregion

        #region Comparable
        /// <summary>
        /// Validates that <paramref name="argument"/> is larger than <paramref name="comparator"/>, if it is not, a <see cref="ArgumentOutOfRangeException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">Type of the argument to check</typeparam>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="comparator">The value to compare to <paramref name="argument"/></param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return: NotNull]
        public static T IsLarger<T>(T argument, T comparator, [CallerArgumentExpression("argument")] string expression = "") where T : IComparable
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");

            IsNotNull(argument, expression);
            Is(() => argument.CompareTo(comparator) > 0, () => new ArgumentOutOfRangeException($"{expression} must be larger than {comparator}"));
            return argument;
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> is larger or equal to <paramref name="comparator"/>, if it is not, a <see cref="ArgumentOutOfRangeException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">Type of the argument to check</typeparam>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="comparator">The value to compare to <paramref name="argument"/></param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return: NotNull]
        public static T IsLargerOrEqual<T>(T argument, T comparator, [CallerArgumentExpression("argument")] string expression = "") where T : IComparable
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");

            IsNotNull(argument, expression);
            Is(() => argument.CompareTo(comparator) >= 0, () => new ArgumentOutOfRangeException($"{expression} must be larger or equal to {comparator}"));
            return argument;
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> is larger than <paramref name="comparator"/>, if it is not, a <see cref="ArgumentOutOfRangeException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">Type of the argument to check</typeparam>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="comparator">The value to compare to <paramref name="argument"/></param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return: NotNull]
        public static T IsSmaller<T>(T argument, T comparator, [CallerArgumentExpression("argument")] string expression = "") where T : IComparable
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");

            IsNotNull(argument, expression);
            Is(() => argument.CompareTo(comparator) < 0, () => new ArgumentOutOfRangeException($"{expression} must be larger than {comparator}"));
            return argument;
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> is larger or equal to <paramref name="comparator"/>, if it is not, a <see cref="ArgumentOutOfRangeException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">Type of the argument to check</typeparam>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="comparator">The value to compare to <paramref name="argument"/></param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return: NotNull]
        public static T IsSmallerOrEqual<T>(T argument, T comparator, [CallerArgumentExpression("argument")] string expression = "") where T : IComparable
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");

            IsNotNull(argument, expression);
            Is(() => argument.CompareTo(comparator) <= 0, () => new ArgumentOutOfRangeException($"{expression} must be larger or equal to {comparator}"));
            return argument;
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> falls in the range of <paramref name="low"/> and <paramref name="high"/>, if it is a <see cref="ArgumentOutOfRangeException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">Type of the argument to check</typeparam>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="low">The lowest value of the range</param>
        /// <param name="high">The highest value of the range</param>
        /// <param name="inclusive">If the value can be equal to <paramref name="low"/> or <paramref name="high"/>. If set to false it can't be</param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return: NotNull]
        public static T IsInRange<T>(T argument, T low, T high, bool inclusive = true, [CallerArgumentExpression("argument")] string expression = "") where T : IComparable
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");
            if (low.CompareTo(high) >= 0) throw new ArgumentException($"{nameof(low)} must be lower than {nameof(high)}");

            IsNotNull(argument, expression);
            Is(() => inclusive ? argument.CompareTo(low) >= 0 && argument.CompareTo(high) <= 0 : argument.CompareTo(low) > 0 && argument.CompareTo(high) < 0, () => new ArgumentOutOfRangeException($"{expression} must be {(inclusive ? "in range of" : "between")} {low} and {high}"));
            return argument;
        }
        #endregion

        #region Collection
        /// <summary>
        /// Validates that <paramref name="argument"/> is not null and contains at least 1 element, if it is an <see cref="ArgumentException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">Type of the argument to check</typeparam>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return: NotNull]
        public static T IsNotNullOrEmpty<T>(T argument, [CallerArgumentExpression("argument")] string expression = "") where T : IEnumerable
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");

            Is(() => argument != null && argument.Enumerate().HasValue(), () => $"{expression} cannot be null and must contain at least 1 element");
            return argument;
        }
        #endregion

        #region FileSystem
        /// <summary>
        /// Validates that <paramref name="argument"/> is not null and exists on the filesystem, if it doesn't exists a <see cref="FileNotFoundException"/> will be thrown.
        /// </summary>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return: NotNull]
        public static FileInfo MustExist(FileInfo argument, [CallerArgumentExpression("argument")] string expression = "")
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");

            IsNotNull(argument, expression);
            Is(() => argument.Exists, () => new FileNotFoundException($"{nameof(expression)} could not be found", argument.FullName));
            return argument;
        }
        /// <summary>
        /// Validates that <paramref name="argument"/> is not null and exists on the filesystem, if it doesn't exists a <see cref="DirectoryNotFoundException"/> will be thrown.
        /// </summary>
        /// <param name="argument">The argument instance to check</param>
        /// <param name="expression">The expression of the argument being checked. Compiler will fill it out automatically so no need to provide it</param>
        /// <returns><paramref name="argument"/></returns>
        [return: NotNull]
        public static DirectoryInfo MustExist(DirectoryInfo argument, [CallerArgumentExpression("argument")] string expression = "")
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException($"{nameof(expression)} cannot be null, empty or whitespace");

            IsNotNull(argument, expression);
            Is(() => argument.Exists, () => new DirectoryNotFoundException($"{nameof(expression)} could not be found. Path was <{argument.FullName}>"));
            return argument;
        }
        #endregion
    }
}
