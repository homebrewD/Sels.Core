using Dapper;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sels.Core.Data.SQL.Extensions.Dapper
{
    /// <summary>
    /// Contains static extension methods for query builders.
    /// </summary>
    public static class QueryBuilderExtentions
    {
        /// <summary>
        /// Generates parameterized VALUES expression for each entity in <paramref name="entities"/>.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to delete</typeparam>
        /// <param name="builder">The builder to add the expressions to</param>
        /// <param name="parameters">Collection to add the parameter values to</param>
        /// <param name="entities">The entities to add to the query</param>
        /// <param name="property">A property to insert from <typeparamref name="TEntity"/></param>
        /// <param name="properties">Any adduitional properties to insert from <typeparamref name="TEntity"/></param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived From<TEntity, TDerived>(this IInsertStatementBuilder<TEntity, TDerived> builder, DynamicParameters parameters, IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> property, params Expression<Func<TEntity, object>>[] properties)
        {
            builder.ValidateArgument(nameof(builder));
            parameters.ValidateArgument(nameof(parameters));
            entities.ValidateArgumentNotNullOrEmpty(nameof(entities));
            property.ValidateArgument(nameof(property));

            var propertyGetters = Helper.Collection.Enumerate(property, properties).ToDictionary(x => x.ExtractProperty().Name, x => x.Compile());
            TDerived derived = default;

            // No insert columns defines so set them
            if (!builder.Expressions.ContainsKey(InsertExpressionPositions.Columns) || builder.Expressions[InsertExpressionPositions.Columns].Length == 0)
            {
                derived = builder.Columns(propertyGetters.Select(x => x.Key));
            }

            foreach (var (entity, i) in entities.Select((x, i) => (x, i)))
            {
                derived = builder.Parameters(i.ToString(), property, properties);

                foreach(var (name, getter) in propertyGetters)
                {
                    parameters.Add($"{name}{i}", getter(entity));
                }
            }

            return derived;
        }
    }
}
