using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Rules;
using System;
using Sels.Core.Extensions.Reflection;
using System.Linq;
using Sels.ObjectValidationFramework.Configurators;
using Sels.Core.Extensions.Text;
using System.Collections;
using System.Collections.Generic;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Profile
{
    /// <summary>
    /// Contains generic configuration extensions. 
    /// </summary>
    public static class GenericValidationExtensions
    {
        /// <summary>
        /// Value is only valid when it is not null.
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
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> CannotBeNull<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.InvalidIf(info => info.Value == null, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is not null.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> CannotBeNull<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.InvalidIf(info => info.Value == null, info => $"Cannot be null");
        }

        /// <summary>
        /// Value is only valid when it is not the default value of the type.
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
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> CannotBeDefault<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.InvalidIf(info => info.Value.IsDefault(), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is not the default value of the type.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> CannotBeDefault<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.InvalidIf(info => info.Value.IsDefault(), info => $"Cannot be the default value. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is null.
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
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeNull<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value == null, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is null.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeNull<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value == null, info => $"Must be null");
        }

        /// <summary>
        /// Value is only valid when it is the default value of the type.
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
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeDefault<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.IsDefault(), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is not the default value of the type.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeDefault<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.IsDefault(), info => $"Must be the default value. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is in <paramref name="validValues"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="validValues">Array of valid values</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeIn<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, IEnumerable<TValue> validValues, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            validValues.ValidateArgument(nameof(validValues));

            return configurator.ValidIf(info => validValues.Contains(info.Value), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is in <paramref name="validValues"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="validValues">Array of valid values</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeIn<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, IEnumerable<TValue> validValues)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            validValues.ValidateArgument(nameof(validValues));

            var validValuesString = validValues.JoinString(", ");
            return configurator.MustBeIn(validValues, info => $"Value not in the list of valid values. Was <{info.Value}>. Must be in <{validValuesString}>");
        }

        /// <summary>
        /// Value is only valid when it is in <paramref name="validValues"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="validValues">Array of valid values</param>
        /// <param name="comparer">The comparer to use when checking for equality</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeIn<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, IEnumerable<TValue> validValues, IEqualityComparer<TValue> comparer, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            validValues.ValidateArgument(nameof(validValues));
            comparer.ValidateArgument(nameof(comparer));

            return configurator.ValidIf(info => validValues.Contains(info.Value, comparer), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is in <paramref name="validValues"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="validValues">Array of valid values</param>
        /// <param name="comparer">The comparer to use when checking for equality</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeIn<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(
            this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, 
            IEnumerable<TValue> validValues, 
            IEqualityComparer<TValue> comparer)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            validValues.ValidateArgument(nameof(validValues));
            comparer.ValidateArgument(nameof(comparer));

            var validValuesString = validValues.JoinString(", ");
            return configurator.MustBeIn(validValues, comparer, info => $"Value not in the list of valid values. Was <{info.Value}>. Must be in <{validValuesString}>");
        }


        /// <summary>
        /// Value is only valid when it is not in <paramref name="invalidValues"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="invalidValues">Array of invalid values</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustNotBeIn<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(
            this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, 
            IEnumerable<TValue> invalidValues, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            invalidValues.ValidateArgument(nameof(invalidValues));

            return configurator.ValidIf(info => !invalidValues.Contains(info.Value), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is not in <paramref name="invalidValues"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="invalidValues">Array of invalid values</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustNotBeIn<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(
            this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, 
            IEnumerable<TValue> invalidValues)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            invalidValues.ValidateArgument(nameof(invalidValues));

            var validValuesString = invalidValues.JoinString(", ");
            return configurator.MustNotBeIn(invalidValues, info => $"Value in the list of invalid values. Was <{info.Value}>. Must not be in <{validValuesString}>");
        }

        /// <summary>
        /// Value is only valid when it is not in <paramref name="invalidValues"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="invalidValues">Array of invalid values</param>
        /// <param name="comparer">The comparer to use when checking for equality</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustNotBeIn<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(
            this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, 
            IEnumerable<TValue> invalidValues, 
            IEqualityComparer<TValue> comparer, 
            Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            invalidValues.ValidateArgument(nameof(invalidValues));
            comparer.ValidateArgument(nameof(comparer));

            return configurator.ValidIf(info => !invalidValues.Contains(info.Value, comparer), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is not in <paramref name="invalidValues"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="invalidValues">Array of invalid values</param>
        /// <param name="comparer">The comparer to use when checking for equality</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustNotBeIn<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(
            this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, 
            IEnumerable<TValue> invalidValues, 
            IEqualityComparer<TValue> comparer)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            invalidValues.ValidateArgument(nameof(invalidValues));
            comparer.ValidateArgument(nameof(comparer));

            var validValuesString = invalidValues.JoinString(", ");
            return configurator.MustNotBeIn(invalidValues, comparer, info => $"Value in the list of invalid values. Was <{info.Value}>. Must not be in <{validValuesString}>");
        }

        #region Conditions
        /// <summary>
        /// Validation rules created after this method call will only be executed when value is not null.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <returns>Current configurator</returns>
        public static IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> NextWhenNotNull<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.NextWhen(info => info.Value != null);
        }

        /// <summary>
        /// Validation rules created after this method call will only be executed when value is not the default value for it's type.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <returns>Current configurator</returns>
        public static IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> NextWhenNotDefault<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.NextWhen(info => !info.Value.IsDefault());
        }

        /// <summary>
        /// Validation rules created after this method call will only be executed when value is null, empty or whitespace.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <returns>Current configurator</returns>
        public static IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> NextWhenNotNullOrWhitespace<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.NextWhen(info => !string.IsNullOrWhiteSpace(info.Value));
        }

        /// <summary>
        /// Validation rules created after this method call will only be executed when value is null or empty.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <returns>Current configurator</returns>
        public static IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> NextWhenNotNullOrEmpty<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.NextWhen(info => !string.IsNullOrEmpty(info.Value));
        }
        #endregion
    }
}
