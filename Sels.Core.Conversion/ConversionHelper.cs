using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Converters.Simple;
using Sels.Core.Conversion.Extensions;
using Sels.Core.Extensions;
using Sels.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sels.Core.Conversion
{
    /// <summary>
    /// Contains static helper methods for conversion related operations.
    /// </summary>
    public static class ConversionHelper
    {
        private static ConcurrentDictionary<TypeMapping, Delegate> Copiers = new ConcurrentDictionary<TypeMapping, Delegate>();

        /// <summary>
        /// Copies the properties of the source object to a new instance of <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object</typeparam>
        /// <typeparam name="TTarget">The type of the target object</typeparam>
        /// <param name="source">The object to copy the properties from</param>
        /// <param name="converter">The converter to use. When set to null <see cref="GenericConverter.DefaultJsonConverter"/> will be used</param>
        /// <param name="conversionArguments">Optional arguments that can be passed to <paramref name="converter"/> during conversion</param>
        /// <param name="forceConvert">True if properties should always be converted, false if only when the value can be converted</param>
        /// <returns>A new instance with the properties copied from <paramref name="source"/></returns>
        public static TTarget CopyTo<TSource, TTarget>(TSource source, ITypeConverter converter = null, IReadOnlyDictionary<string, object> conversionArguments = null, bool forceConvert = false) where TTarget : new()
        {
            var target = new TTarget();
            CopyTo(source, target, converter, conversionArguments, forceConvert);
            return target;
        }
        /// <summary>
        /// Copies the properties of the source object to the target object.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object</typeparam>
        /// <typeparam name="TTarget">The type of the target object</typeparam>
        /// <param name="source">The object to copy the properties from</param>
        /// <param name="target">The object to copy the properties to</param>
        /// <param name="converter">The converter to use. When set to null <see cref="GenericConverter.DefaultJsonConverter"/> will be used</param>
        /// <param name="conversionArguments">Optional arguments that can be passed to <paramref name="converter"/> during conversion</param>
        /// <param name="forceConvert">True if properties should always be converted, false if only when the value can be converted</param>
        public static void CopyTo<TSource, TTarget>(TSource source, TTarget target, ITypeConverter converter = null, IReadOnlyDictionary<string, object> conversionArguments = null, bool forceConvert = false)
        {
            source.ValidateArgument(nameof(source));
            target.ValidateArgument(nameof(target));

            var typeMapping = new TypeMapping() { Source = typeof(TSource), Target = typeof(TTarget), IsForced = forceConvert };

            Action<ITypeConverter, IReadOnlyDictionary<string, object>, TSource, TTarget> copier = null;

            copier = Copiers.GetOrAdd(typeMapping, x => GetCopier<TSource, TTarget>(forceConvert)) as Action<ITypeConverter, IReadOnlyDictionary<string, object>, TSource, TTarget>;

            copier(converter ?? GenericConverter.DefaultJsonConverter, conversionArguments, source, target);
        }

        private static Action<ITypeConverter, IReadOnlyDictionary<string, object>, TSource, TTarget> GetCopier<TSource, TTarget>(bool forceConversion)
        {
            var properties = typeof(TSource).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(x => x.CanRead && x.GetIndexParameters().Length == 0).ToArray();

            var typeConverterParameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(ITypeConverter), "c");
            var conversionArgumentsParameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(IReadOnlyDictionary<string, object>), "a");
            var sourceParameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TSource), "s");
            var targetParameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TTarget), "t");

            var canConvertMethod = Helper.Expressions.Method.GetMethod<ITypeConverter>(x => x.CanConvert(null, null, null));
            var convertMethod = Helper.Expressions.Method.GetMethod<ITypeConverter>(x => x.ConvertTo(null, null, null));

            var expressions = new List<System.Linq.Expressions.Expression>();
            foreach (var property in properties)
            {
                var targetProperty = typeof(TTarget).GetProperty(property.Name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

                if (targetProperty == null || !targetProperty.CanWrite || targetProperty.GetIndexParameters().Length > 0) continue;

                var sourcePropertyExpression = System.Linq.Expressions.Expression.Property(sourceParameterExpression, property);
                var targetPropertyExpression = System.Linq.Expressions.Expression.Property(targetParameterExpression, targetProperty);

                System.Linq.Expressions.Expression conditionExpression = null;
                System.Linq.Expressions.Expression convertToExpression = null;
                var defaultExpression = System.Linq.Expressions.Expression.Default(targetProperty.PropertyType);
                if (forceConversion)
                {
                    conditionExpression = System.Linq.Expressions.Expression.NotEqual(sourcePropertyExpression, System.Linq.Expressions.Expression.Constant(null));
                    convertToExpression = System.Linq.Expressions.Expression.Call(typeConverterParameterExpression, convertMethod, System.Linq.Expressions.Expression.Convert(sourcePropertyExpression, typeof(object)), System.Linq.Expressions.Expression.Constant(targetProperty.PropertyType), conversionArgumentsParameterExpression);
                }
                else
                {
                    conditionExpression = System.Linq.Expressions.Expression.Call(typeConverterParameterExpression, canConvertMethod, System.Linq.Expressions.Expression.Convert(sourcePropertyExpression, typeof(object)), System.Linq.Expressions.Expression.Constant(targetProperty.PropertyType), conversionArgumentsParameterExpression);
                    convertToExpression = System.Linq.Expressions.Expression.Call(typeConverterParameterExpression, convertMethod, System.Linq.Expressions.Expression.Convert(sourcePropertyExpression, typeof(object)), System.Linq.Expressions.Expression.Constant(targetProperty.PropertyType), conversionArgumentsParameterExpression);
                }

                var castExpression = System.Linq.Expressions.Expression.Convert(convertToExpression, targetProperty.PropertyType);
                var convertExpression = System.Linq.Expressions.Expression.Condition(conditionExpression, castExpression, defaultExpression);
                var assignmentExpression = System.Linq.Expressions.Expression.Assign(targetPropertyExpression, convertExpression);
                expressions.Add(assignmentExpression);
            }

            var blockExpression = System.Linq.Expressions.Expression.Block(expressions);
            var lambda = System.Linq.Expressions.Expression.Lambda<Action<ITypeConverter, IReadOnlyDictionary<string, object>, TSource, TTarget>>(blockExpression, typeConverterParameterExpression, conversionArgumentsParameterExpression, sourceParameterExpression, targetParameterExpression);
            return lambda.Compile();
        }

        private class TypeMapping : ValueObject
        {
            public Type Source { get; set; }
            public Type Target { get; set; }
            public bool IsForced { get; set; }
        }
    }
}
