using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Sels.Core.Models
{
    /// <summary>
    /// Objects who's equality is based on it's properties rather then it's reference.
    /// </summary>
    public abstract class ValueObject
    {
        // Statics
        private static ConcurrentDictionary<Type, Func<object, object[]>> Getters = new ConcurrentDictionary<Type, Func<object, object[]>>();

        /// <summary>
        /// Enumerable of properties that are used to calculate equality.
        /// </summary>
        /// <returns>Enumerable of properties that are used to calculate equality</returns>
        protected virtual IEnumerable<object> GetEqualityComponents()
        {
            var type = GetType();

            var getter = Getters.GetOrAdd(type, t =>
            {
                var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(x => x.CanRead && x.GetIndexParameters().Length == 0).ToArray();

                if (properties.Length == 0)
                {
                    throw new InvalidOperationException("Value object must have at least one public property");
                }

                var instanceParameter = Expression.Parameter(typeof(object), "o");
                var instanceVariable = Expression.Variable(type, "i");
                var castVariable = Expression.Convert(instanceParameter, type);
                var assignVariable = Expression.Assign(instanceVariable, castVariable);
                var createArray = Expression.NewArrayInit(typeof(object), properties.Select(p => Expression.Convert(Expression.Property(instanceVariable, p), typeof(object))));

                var lambda = Expression.Lambda<Func<object, object[]>>(Expression.Block(new[] { instanceVariable }, assignVariable, createArray), instanceParameter);
                return lambda.Compile();
            });

            return getter(this);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if(base.Equals(obj))
            {
                return true;
            }
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = obj as ValueObject;
            var components = GetEqualityComponents().ToArray();
            var otherComponents = other.GetEqualityComponents().ToArray();

            if(components.Length != otherComponents.Length)
            {
                return false;
            }

            for (int i = 0; i < components.Length; i++)
            {
                if (!components[i].Equals(otherComponents[i]))
                {
                    return false;
                }
            }

            return true;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return GetEqualityComponents().Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + (obj?.GetHashCode() ?? 0);
                }
            });
        }
    }
}
