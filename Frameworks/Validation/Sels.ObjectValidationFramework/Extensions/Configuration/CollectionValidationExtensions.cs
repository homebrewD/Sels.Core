using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Linq;
using Sels.ObjectValidationFramework.Configurators;
using Sels.ObjectValidationFramework.Rules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Profile
{
    /// <summary>
    /// Contains generic configuration extensions for collection types.
    /// </summary>
    public static class CollectionValidationExtensions
    {
        /// <summary>
        /// Value is only valid when it contains at least 1 element.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> CannotBeEmpty<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.GetCount() > 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it contains at least 1 element.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> CannotBeEmpty<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.GetCount() > 0, info => $"Must contain at least 1 element. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it doesn't contain any elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeEmpty<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.GetCount() == 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it doesn't contain any elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeEmpty<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.GetCount() == 0, info => $"Must be empty. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it contains <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">How many elements the collection must contain</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustContainExactly<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, int count, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            count.ValidateArgumentLarger(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() == count, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it contains <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">How many elements the collection must contain</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustContainExactly<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, int count)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            count.ValidateArgumentLarger(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() == count, info => $"Must contain {count} elements. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it only contains a max of <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">Max allowed count of elements</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustContainAtMax<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, int count, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            count.ValidateArgumentLargerOrEqual(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() <= count, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it only contains a max of <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">Max allowed count of elements</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustContainAtMax<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, int count)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            count.ValidateArgumentLargerOrEqual(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() <= count, info => $"Can only contain a maximum of {count} elements. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it only contains a min of <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">Max allowed count of elements</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustContainAtLeast<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, int count, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            count.ValidateArgumentLargerOrEqual(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() >= count, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it only contains a min of <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">Max allowed count of elements</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustContainAtLeast<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, int count)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            count.ValidateArgumentLargerOrEqual(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() >= count, info => $"Can only contain a minimum of {count} elements. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it contains more or equal to <paramref name="minCount"/> and less or equal to <paramref name="maxCount"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minCount">Min allowed count of elements</param>
        /// <param name="maxCount">Max allowed count of elements</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustContainInRange<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, int minCount, int maxCount, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minCount.ValidateArgumentLargerOrEqual(nameof(minCount), 0);
            maxCount.ValidateArgumentLarger(nameof(maxCount), minCount);

            return configurator.ValidIf(info => info.Value.GetCount() >= minCount && info.Value.GetCount() <= maxCount, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it contains more or equal to <paramref name="minCount"/> and less or equal to <paramref name="maxCount"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minCount">Min allowed count of elements</param>
        /// <param name="maxCount">Max allowed count of elements</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustContainInRange<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, int minCount, int maxCount)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            minCount.ValidateArgumentLargerOrEqual(nameof(minCount), 0);
            maxCount.ValidateArgumentLarger(nameof(maxCount), minCount);

            return configurator.ValidIf(info => info.Value.GetCount() >= minCount && info.Value.GetCount() <= maxCount, info => $"Can only contain a minimum of {minCount} and a maximum of {maxCount} elements. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it contains between <paramref name="minCount"/> and <paramref name="maxCount"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minCount">Min allowed count of elements</param>
        /// <param name="maxCount">Max allowed count of elements</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustContainInBetween<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, int minCount, int maxCount, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minCount.ValidateArgumentLargerOrEqual(nameof(minCount), 0);
            maxCount.ValidateArgumentLarger(nameof(maxCount), minCount+1);

            return configurator.ValidIf(info => info.Value.GetCount() > minCount && info.Value.GetCount() < maxCount, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it contains more or equal to <paramref name="minCount"/> and less or equal to <paramref name="maxCount"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minCount">Min allowed count of elements</param>
        /// <param name="maxCount">Max allowed count of elements</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustContainInBetween<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, int minCount, int maxCount)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            minCount.ValidateArgumentLargerOrEqual(nameof(minCount), 0);
            maxCount.ValidateArgumentLarger(nameof(maxCount), minCount+1);

            return configurator.ValidIf(info => info.Value.GetCount() > minCount && info.Value.GetCount() < maxCount, info => $"Can only contain between {minCount} and {maxCount} elements. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when all elements are unique. Values are compared using <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> AllMustBeUnique<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor) 
            where TTargetContext : TBaseContext 
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.Enumerate().AreAllUnique(), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when all elements are unique. Values are compared using <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> AllMustBeUnique<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.Enumerate().AreAllUnique(), info => $"Must all be unique");
        }

        /// <summary>
        /// Value is only valid when all elements are unique. Values are compared using <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <typeparam name="TElement">The type of elements to compare</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparer">The comparer to use when checking for equality</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> AllMustBeUnique<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue, TElement>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, IEqualityComparer<TElement> comparer, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable<TElement>
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.AreAllUnique(comparer), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when all elements are unique. Values are compared using <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <typeparam name="TElement">The type of elements to compare</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparer">The comparer to use when checking for equality</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> AllMustBeUnique<TEntity, TBaseContext, TInfo, TTargetContext, TValue, TElement>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, IEqualityComparer<TElement> comparer)
            where TTargetContext : TBaseContext
            where TValue : IEnumerable<TElement>
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.AreAllUnique(comparer), info => $"Must all be unique");
        }
    }
}
