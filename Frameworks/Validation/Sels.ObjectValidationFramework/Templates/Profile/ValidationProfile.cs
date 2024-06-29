using Microsoft.Extensions.Logging;
using Sels.Core.Scope;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Validation;
using Sels.ObjectValidationFramework.Configurators;
using Sels.ObjectValidationFramework.Target;
using Sels.ObjectValidationFramework.Validators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.DateTimes;
using Sels.Core.Extensions.Fluent;

namespace Sels.ObjectValidationFramework.Profile
{
    /// <summary>
    /// Profile that can be configured using a fluent syntax to validate objects.
    /// </summary>
    /// <typeparam name="TError">Type of validation error to return</typeparam>
    public class ValidationProfile<TError>
    {
        // Static
        private static readonly Type[] _specialIgnoredCollectionTypes = new Type[]
        {
            typeof(string)
        };
        private static readonly Predicate<(object Property, PropertyInfo Info, object Context)>[] _defaultIgnoredPropertyConditions = new Predicate<(object Property, PropertyInfo Info, object Context)>[]
        {
            // Ignore all system types 
            new Predicate<(object Property, PropertyInfo Info, object Context)>(x => x.Info.PropertyType.FullName.StartsWith("System.")),
            // Ignore all microsoft types
            new Predicate<(object Property, PropertyInfo Info, object Context)>(x => x.Info.PropertyType.FullName.StartsWith("Microsoft."))
        };

        // Fields
        private readonly ILogger _logger;
        private readonly TargetExecutionOptions _defaultSettings;
        private readonly BindingFlags _propertyFlags = BindingFlags.Instance | BindingFlags.Public;
        internal readonly List<EntityValidator<TError>> _validators = new List<EntityValidator<TError>>();
        internal readonly Dictionary<IgnoreType, List<Predicate<(object Property, PropertyInfo Info, object Context)>>> _ignoredPropertyConditions = new Dictionary<IgnoreType, List<Predicate<(object Property, PropertyInfo Info, object Context)>>>();

        /// <inheritdoc cref="ValidationProfile{TError}"/>
        /// <param name="addDefaultIgnored">If the default ignored types must be added as ignore conditions</param>
        /// <param name="validateNonPublic">If non public properties also need to be checked</param>
        /// <param name="defaultSettings">The default target settings to use for all created validators</param>
        /// <param name="logger">Optional loggers for tracing</param>
        public ValidationProfile(bool addDefaultIgnored = true, bool validateNonPublic = false, TargetExecutionOptions defaultSettings = TargetExecutionOptions.None, ILogger logger = null)
        {
            _logger = logger;
            _defaultSettings = defaultSettings;

            if(validateNonPublic) _propertyFlags = _propertyFlags | BindingFlags.NonPublic;

            if (addDefaultIgnored)
            {
                _defaultIgnoredPropertyConditions.Execute(x => IgnoreFor(x, IgnoreType.Fallthrough));
                IgnoreFor(x => _specialIgnoredCollectionTypes.Contains(x.Info.PropertyType), IgnoreType.Collection);
            }
        }

        #region Configuration
        /// <summary>
        /// Creates a new configurator for creating validation for objetcs of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to create validation for</typeparam>
        /// <param name="settings">The default settings to use for all created validation targets</param>
        /// <returns>A configurator for creating validation rules</returns>
        public IValidationConfigurator<TEntity, object, TError> CreateValidationFor<TEntity>(TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                return CreateValidationFor<TEntity, object>(settings);
            }
        }

        /// <summary>
        /// Creates a new configurator for creating validation for objetcs of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to create validation for</typeparam>
        /// <typeparam name="TContext">Type of the validation context used by the created validation rules</typeparam>
        /// <param name="settings">The default settings to use for all created validation targets</param>
        /// <returns>A configurator for creating validation rules</returns>
        public IValidationConfigurator<TEntity, TContext, TError> CreateValidationFor<TEntity, TContext>(TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                var validator = new EntityValidator<TEntity, TContext, TError>(x => _validators.Add(x.ValidateArgument(nameof(x))), _defaultSettings | settings, _logger);
                _validators.Add(validator);
                return validator;
            }
        }
        #endregion

        #region Ignore For
        /// <summary>
        /// Property will be ignored when <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="condition">Predicate that dictates when a property is ignored</param>
        /// <param name="ignoreType">Indicates when the property is ignored</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnoreFor(Predicate<(object Property, PropertyInfo Info, object Context)> condition, IgnoreType ignoreType = IgnoreType.Fallthrough)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                _ignoredPropertyConditions.AddValueToList(ignoreType, condition);
                return this;
            }
        }

        /// <summary>
        /// Property will be ignored <paramref name="condition"/> returns true. Only gets executed when property can be assigned to <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of the property value</typeparam>
        /// <param name="condition">Predicate that dictates when when a property/collection is ignored for fallthrough</param>
        /// <param name="ignoreType">Indicates when the property is ignored</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnoreFor<TEntity>(Predicate<(TEntity Property, PropertyInfo Info, object Context)> condition, IgnoreType ignoreType = IgnoreType.Fallthrough)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                return IgnoreFor(x =>
                {
                    if(x.Property is TEntity typedProperty)
                    {
                        return condition((typedProperty, x.Info, x.Context));
                    }

                    return false;
                }, ignoreType);
            }
        }

        /// <summary>
        /// Property will be ignored <paramref name="condition"/> returns true. Only gets executed when context can be assigned to <typeparamref name="TContext"/>.
        /// </summary>
        /// <typeparam name="TContext">Type of the supplied context</typeparam>
        /// <param name="ignoreType">Indicates when the property is ignored</param>
        /// <param name="condition">Predicate that dictates when when a property/collection is ignored for fallthrough</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnoreForWhenContext<TContext>(Predicate<(object Property, PropertyInfo Info, TContext Context)> condition, IgnoreType ignoreType = IgnoreType.Fallthrough)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                return IgnoreFor(x =>
                {
                    if (x.Context is TContext typedContext)
                    {
                        return condition((x.Property, x.Info, typedContext));
                    }

                    return false;
                }, ignoreType);
            }
        }

        /// <summary>
        /// Property will be ignored <paramref name="condition"/> returns true. Only gets executed when context can be assigned to <typeparamref name="TContext"/> and property can be assigned to <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of the property value</typeparam>
        /// <typeparam name="TContext">Type of the supplied context</typeparam>
        /// <param name="ignoreType">Indicates when the property is ignored</param>
        /// <param name="condition">Predicate that dictates when when a property/collection is ignored for fallthrough</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnoreForWhenContext<TContext, TEntity>(Predicate<(object Property, PropertyInfo Info, TContext Context)> condition, IgnoreType ignoreType = IgnoreType.Fallthrough)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                return IgnoreFor(x =>
                {
                    if (x.Property is TEntity typedProperty && x.Context is TContext typedContext)
                    {
                        return condition((typedProperty, x.Info, typedContext));
                    }

                    return false;
                }, ignoreType);
            }
        }

        /// <summary>
        /// Property will be ignored where the property is selected by <paramref name="property"/> on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of the entity <paramref name="property"/> is from</typeparam>
        /// <param name="ignoreType">Indicates when the property is ignored</param>
        /// <param name="property">Delegate that selects the property on <typeparamref name="TEntity"/></param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnorePropertyFor<TEntity>(Expression<Func<TEntity, object>> property, IgnoreType ignoreType = IgnoreType.Fallthrough)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));

                return IgnorePropertyFor(property, x => true, ignoreType);
            }
        }

        /// <summary>
        /// Property will be ignored where the property is selected by <paramref name="property"/> on <typeparamref name="TEntity"/>. Property will only be ignored when <paramref name="contextCondition"/> returns true.
        /// </summary>
        /// <typeparam name="TEntity">Type of the entity <paramref name="property"/> is from</typeparam>
        /// <param name="property">Delegate that selects the property on <typeparamref name="TEntity"/></param>
        /// <param name="ignoreType">Indicates when the property is ignored</param>
        /// <param name="contextCondition">Condition supplied context must pass before <paramref name="property"/> is ignored.</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnorePropertyFor<TEntity>(Expression<Func<TEntity, object>> property, Predicate<object> contextCondition, IgnoreType ignoreType = IgnoreType.Fallthrough)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                contextCondition.ValidateArgument(nameof(contextCondition));

                return IgnorePropertyFor<TEntity, object>(property, contextCondition, ignoreType);
            }
        }

        /// <summary>
        /// Property will be ignored where the property is selected by <paramref name="property"/> on <typeparamref name="TEntity"/>. Property will only be ignored when content is of type <typeparamref name="TContext"/> and when <paramref name="contextCondition"/> returns true.
        /// </summary>
        /// <typeparam name="TEntity">Type of the entity <paramref name="property"/> is from</typeparam>
        /// <typeparam name="TContext">Type of the context</typeparam>
        /// <param name="property">Delegate that selects the property on <typeparamref name="TEntity"/></param>
        /// <param name="ignoreType">Indicates when the property is ignored</param>
        /// <param name="contextCondition">Condition supplied context must pass before <paramref name="property"/> is ignored.</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnorePropertyFor<TEntity, TContext>(Expression<Func<TEntity, object>> property, Predicate<TContext> contextCondition, IgnoreType ignoreType = IgnoreType.Fallthrough)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                if (!property.TryExtractProperty(out var propertyInfo) || !typeof(TEntity).IsAssignableTo(propertyInfo.ReflectedType))
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }
                contextCondition.ValidateArgument(nameof(contextCondition));

                return IgnoreFor(x =>
                {
                    var context = x.Context.CastToOrDefault<TContext>();
                    return contextCondition(context) && x.Info.Name == propertyInfo.Name && x.Info.DeclaringType == propertyInfo.DeclaringType;
                }, ignoreType);
            }
        }
        #endregion

        #region Importing
        /// <summary>
        /// Import configuration from <paramref name="profile"/> into this profile.
        /// </summary>
        /// <param name="profile">Profile to import from</param>
        /// <param name="options">What to import</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> ImportFrom(ValidationProfile<TError> profile, ImportOptions options = ImportOptions.All)
        {
            using (_logger.TraceMethod(this))
            {
                profile.ValidateArgument(nameof(profile));

                if(options.HasFlag(ImportOptions.Configuration))
                {
                    _logger.Debug($"Importing configuration from <{profile}>");
                    _validators.AddRange(profile._validators);
                }

                if (options.HasFlag(ImportOptions.IgnoredForFallthrough))
                {
                    _logger.Debug($"Importing ignore conditions from <{profile}> for ignore type <{IgnoreType.Fallthrough}>");
                    if (profile._ignoredPropertyConditions.ContainsKey(IgnoreType.Fallthrough)) _ignoredPropertyConditions.AddValues(IgnoreType.Fallthrough, profile._ignoredPropertyConditions[IgnoreType.Fallthrough]);
                }

                if (options.HasFlag(ImportOptions.IgnoredForCollections))
                {
                    _logger.Debug($"Importing ignore conditions from <{profile}> for ignore type <{IgnoreType.Collection}>");
                    if (profile._ignoredPropertyConditions.ContainsKey(IgnoreType.Collection)) _ignoredPropertyConditions.AddValues(IgnoreType.Collection, profile._ignoredPropertyConditions[IgnoreType.Collection]);
                }

                return this;
            }
        }

        /// <summary>
        /// Import configuration from <typeparamref name="TProfile"/> into this profile.
        /// </summary>
        /// <typeparam name="TProfile">Type of profile to import options from</typeparam>
        /// <param name="options">What to import</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> ImportFrom<TProfile>(ImportOptions options = ImportOptions.All) where TProfile : ValidationProfile<TError>, new()
        {
            using (_logger.TraceMethod(this))
            {
                return ImportFrom(new TProfile(), options);
            }
        }

        /// <summary>
        /// Import configuration from the profile created by <paramref name="constructor"/> into this profile.
        /// </summary>
        /// <param name="constructor">Delegate that creates the profile instance to import from</param>
        /// <param name="options">What to import</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> ImportFrom(Func<ValidationProfile<TError>> constructor, ImportOptions options = ImportOptions.All)
        {
            using (_logger.TraceMethod(this))
            {
                constructor.ValidateArgument(nameof(constructor));

                return ImportFrom(constructor(), options);
            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// Validates <paramref name="objectToValidate"/> using the configuration in the profile and returns alls validation errors.
        /// </summary>
        /// <typeparam name="T">The type of the object being validated</typeparam>
        /// <param name="objectToValidate">The object to validate</param>
        /// <param name="context">Optional context that can be used by the validation configuration</param>
        /// <param name="options"><inheritdoc cref="ProfileExecutionOptions"/></param>
        /// <returns>The validation result for <paramref name="objectToValidate"/></returns>
        public ValidationResult<T, TError> Validate<T>(T objectToValidate, object context = null, ProfileExecutionOptions options = ProfileExecutionOptions.None)
        {
            return ValidateAsync(objectToValidate, context, options).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Validates <paramref name="objectToValidate"/> using the configuration in the profile and returns alls validation errors.
        /// </summary>
        /// <typeparam name="T">The type of the object being validated</typeparam>
        /// <param name="objectToValidate">The object to validate</param>
        /// <param name="context">Optional context that can be used by the validation configuration</param>
        /// <param name="options"><inheritdoc cref="ProfileExecutionOptions"/></param>
        /// <returns>The validation result for <paramref name="objectToValidate"/></returns>
        public async Task<ValidationResult<T, TError>> ValidateAsync<T>(T objectToValidate, object context = null, ProfileExecutionOptions options = ProfileExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                objectToValidate.ValidateArgument(nameof(objectToValidate));
                var executionContext = new ExecutionContext(context, _propertyFlags, options);

                using(_logger.TraceAction($"Validating root object <{objectToValidate}>", x => $"Validated root object <{objectToValidate}> by checking {executionContext.History.Count} objects which returned {executionContext.Errors.Count} errors in {x.PrintTotalMs()}"))
                {
                    await ValidateObject(executionContext, objectToValidate, null).ConfigureAwait(false);

                    var objectType = objectToValidate.GetType();
                    if (!executionContext.IsRootCollectionFallthroughDisabled && !_specialIgnoredCollectionTypes.Any(x => objectType.IsAssignableTo(x)) && objectType.IsContainer())
                    {
                        _logger.Debug($"Root object <{objectToValidate}> is a collection. Triggering validation for the elements");
                        var counter = 0;
                        foreach (var element in objectToValidate.CastTo<IEnumerable>().Enumerate().Where(x => x != null))
                        {
                            await ValidateObject(executionContext, element, counter).ConfigureAwait(false);
                            counter++;
                        }
                    }
                }

                if (executionContext.ThrowOnErrors && executionContext.Errors.HasValue()) throw new ValidationException<T, TError>(objectToValidate, executionContext.Errors.Select(x => x.Message));
                var result = new ValidationResult<T, TError>()
                {
                    Validated = objectToValidate,
                    CallStack = executionContext.History.ToArray(),
                    Errors = executionContext.Errors.ToArrayOrDefault()
                };

                _logger.TraceObject($"Validation result for <{objectToValidate}>", result);

                return result;
            }
        }

        private async Task ValidateObject(ExecutionContext executionContext, object objectToValidate, int? elementIndex = null)
        {
            using (_logger.TraceMethod(this))
            {
                executionContext.ValidateArgument(nameof(executionContext));
                objectToValidate.ValidateArgument(nameof(objectToValidate));

                // Check if we arleady validated in an attemt to avoid duplicate validation and circular fallthrough
                if (executionContext.History.Contains(objectToValidate))
                {
                    _logger.Debug($"Already validated <{objectToValidate}>. Skipping");
                    return;
                }
                else
                {
                    _logger.Debug($"Starting validation for <{objectToValidate}>");
                    executionContext.History.Add(objectToValidate);
                }

                var currentParents = executionContext.CurrentParents.ToArray();

                // Trigger validation for the current object if we have validators for it
                var validators = _validators.Where(x => x.CanValidate(objectToValidate, executionContext.Context, elementIndex, currentParents)).ToArray();
                for(int i = 0; i < validators.Length; i++)
                {
                    var validator = validators[i];
                    _logger.Debug($"Using validator {i+1} for <{objectToValidate}>");
                    var errors = await validator.Validate(objectToValidate, executionContext.Context, elementIndex, currentParents).ConfigureAwait(false);
                    _logger.Debug($"Validator {i + 1} for <{objectToValidate}> returned {errors.Length} errors");
                    executionContext.Errors.AddRange(errors);
                }

                // Return if fallthough disabled
                if (executionContext.IsPropertyFallthroughDisabled) return;

                // Execute fallthrough
                foreach(var (property, ignoretype) in executionContext.GetProperties(objectToValidate.GetType()))
                {
                    var value = property.GetValue(objectToValidate);

                    if(value != null)
                    {
                        // Allowed if property is a collection but not any of the special collection types
                        var isAllowedForCollectionFallthough = !executionContext.IsCollectionFallthroughDisabled && !ignoretype.HasFlag(IgnoreType.Collection) && value.GetType().IsContainer() && !IsIgnoredFor(IgnoreType.Collection, value, property, executionContext.Context);
                        // Allowed if the property doesn't have a validator explicitly defined and isn't a default ignored type
                        var isAllowedForPropertyFallthough =  !ignoretype.HasFlag(IgnoreType.Fallthrough) && !IsIgnoredFor(IgnoreType.Fallthrough, value, property, executionContext.Context);

                        if(isAllowedForCollectionFallthough || isAllowedForPropertyFallthough)
                        {
                            var parent = new Parent(objectToValidate, property, elementIndex);
                            using (new ScopedAction(() => executionContext.CurrentParents.Add(parent), () => executionContext.CurrentParents.Remove(parent)))
                            {                              
                                if (isAllowedForCollectionFallthough)
                                {
                                    // Loop over elements in collection and trigger validation for the elements
                                    _logger.Debug($"Property {property.Name} on <{objectToValidate}> is a collection. Triggering validation for the elements");
                                    var counter = 0;
                                    foreach (var element in value.CastTo<IEnumerable>().Enumerate().Where(x => x != null))
                                    {
                                        await ValidateObject(executionContext, element, counter).ConfigureAwait(false);
                                        counter++;
                                    }
                                }

                                // Trigger validation for the property value
                                if (isAllowedForPropertyFallthough)
                                {
                                    _logger.Debug($"Triggering fallthough for property {property.Name} on <{objectToValidate}>.");
                                    await ValidateObject(executionContext, value).ConfigureAwait(false);
                                }
                                else
                                {
                                    _logger.Debug($"Property {property.Name} on <{objectToValidate}> is ignored. Skipping for property fallthrough");
                                }
                            }
                        }                                      
                    }
                    else
                    {
                        _logger.Debug($"Property {property.Name} on <{objectToValidate}> was null. Skipping for fallthrough");
                    }                    
                }
            }
        }

        private bool IsIgnoredFor(IgnoreType type, object value, PropertyInfo property, object context)
        {
            // Return false if no conditions exist for the requested type
            if (!_ignoredPropertyConditions.ContainsKey(type) && !_ignoredPropertyConditions[type].HasValue()) return false;

            return _ignoredPropertyConditions[type].Any(x => x((value, property, context)));
        }
        #endregion

        /// <summary>
        /// Contains the state of validation executed by a validation profile.
        /// </summary>
        private class ExecutionContext
        {
            // Fields
            private readonly Dictionary<Type, (PropertyInfo Property, IgnoreType IgnoreType)[]> _propertyCache = new Dictionary<Type, (PropertyInfo Property, IgnoreType IgnoreType)[]>();
            private readonly BindingFlags _bindingFlags;
            private readonly ProfileExecutionOptions _options;

            /// <inheritdoc cref="ExecutionContext"/>
            /// <param name="context"><inheritdoc cref="Context"/></param>
            /// <param name="bindingFlags">Flags that dictate which properties to fallthrough</param>
            /// <param name="options">The options for the current validation execution</param>
            public ExecutionContext(object context, BindingFlags bindingFlags, ProfileExecutionOptions options)
            {
                Context = context;
                _bindingFlags = bindingFlags;
                _options = options;
            }

            /// <summary>
            /// The optional validation context supplied by the consumer of the profile
            /// </summary>
            public object Context { get; }
            /// <summary>
            /// The object references that the profile already validated. Used to avoid stack overflows when dealing with circular dependencies.
            /// </summary>
            public List<object> History { get; } = new List<object>();
            /// <summary>
            /// The current validation errors.
            /// </summary>
            public List<ValidationError<TError>> Errors { get; } = new List<ValidationError<TError>>();
            /// <summary>
            /// Represents the current hierarchy of parent of the object currently being validated.
            /// </summary>
            public List<Parent> CurrentParents { get; } = new List<Parent>();
            /// <inheritdoc cref="ProfileExecutionOptions.ThrowOnError"/>
            public bool ThrowOnErrors => _options.HasFlag(ProfileExecutionOptions.ThrowOnError);
            /// <inheritdoc cref="ProfileExecutionOptions.NoPropertyFallthrough"/>
            public bool IsPropertyFallthroughDisabled => _options.HasFlag(ProfileExecutionOptions.NoPropertyFallthrough);
            /// <inheritdoc cref="ProfileExecutionOptions.NoCollectionFallthrough"/>
            public bool IsCollectionFallthroughDisabled => _options.HasFlag(ProfileExecutionOptions.NoCollectionFallthrough);
            /// <inheritdoc cref="ProfileExecutionOptions.NoRootCollectionFallthrough"/>
            public bool IsRootCollectionFallthroughDisabled => _options.HasFlag(ProfileExecutionOptions.NoRootCollectionFallthrough);

            /// <summary>
            /// Returns all properties to fallthrough on type <paramref name="type"/>.
            /// </summary>
            /// <param name="type">The type to get the properties for</param>
            /// <returns>All properties to fallthrough on type <paramref name="type"/> or an empty array when there aren't any properties</returns>
            public (PropertyInfo Property, IgnoreType IgnoreType)[] GetProperties(Type type)
            {
                type.ValidateArgument(nameof(type));
                return _propertyCache.TryGetOrSet(type, () =>
                {
                    var classAttribute = type.GetCustomAttribute<IgnoreInValidationAttribute>();

                    return type.GetProperties(_bindingFlags).Where(x => x.GetIndexParameters().Length == 0).Select(x =>
                    {
                        var attribute = x.GetCustomAttribute<IgnoreInValidationAttribute>();
                        if (attribute != null) return (Property: x, IgnoreType: attribute.IgnoreType);
                        if (classAttribute != null) return (Property: x, IgnoreType: classAttribute.IgnoreType);
                        return (Property: x, IgnoreType: IgnoreType.None);
                    }).Where(x => !x.IgnoreType.HasFlag(IgnoreType.All)).ToArray();
                });
            }
        }
    }

    /// <summary>
    /// Tells what to import from another profile.
    /// </summary>
    [Flags]
    public enum ImportOptions
    {
        /// <summary>
        /// Import everything
        /// </summary>
        All = IgnoredForFallthrough | IgnoredForCollections | Configuration,
        /// <summary>
        /// Impors all conditions for what properties to ignore for fallthough.
        /// </summary>
        IgnoredForFallthrough = 1,
        /// <summary>
        /// Impors all conditions for what properties are ignored as collections.
        /// </summary>
        IgnoredForCollections = 2,
        /// <summary>
        /// Import all configured validation rules.
        /// </summary>
        Configuration = 3
    }

    /// <summary>
    /// Indicates what is being ignored.
    /// </summary>
    [Flags]
    public enum IgnoreType
    {
        /// <summary>
        /// Ignored for nothing.
        /// </summary>
        None = 0,
        /// <summary>
        /// Property is ignored for fallthrough. 
        /// </summary>
        Fallthrough = 1,
        /// <summary>
        /// If the property is a collection the items will not be validated.
        /// </summary>
        Collection = 2,
        /// <summary>
        /// Ignored for everything.
        /// </summary>
        All = Fallthrough | Collection
    }
}
