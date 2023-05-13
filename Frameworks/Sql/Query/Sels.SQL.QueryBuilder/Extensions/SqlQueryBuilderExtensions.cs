﻿using Microsoft.Extensions.Logging;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;

namespace Sels.SQL.QueryBuilder.Extensions
{
    /// <summary>
    /// Contains extension methods for working with the sql builders.
    /// </summary>
    public static class SqlQueryBuilderExtensions
    {
        /// <summary>
        /// Turns <paramref name="parameter"/> into a <see cref="ParameterExpression"/>.
        /// </summary>
        /// <param name="parameter">The string containing the parameter name</param>
        /// <returns><paramref name="parameter"/> as <see cref="ParameterExpression"/></returns>
        public static ParameterExpression AsParameterExpression(this string parameter)
        {
            parameter.ValidateArgumentNotNullOrWhitespace(nameof(parameter));

            return new ParameterExpression(parameter);
        }
        /// <summary>
        /// Set an expression to NULL.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to select</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <returns>Current builder for method chaining</returns>
        public static IUpdateStatementBuilder<TEntity, TDerived> Null<TEntity, TDerived>(this ISharedExpressionBuilder<TEntity, IUpdateStatementBuilder<TEntity, TDerived>> builder)
        {
            builder.ValidateArgument(nameof(builder));

            return builder.Value(DBNull.Value);
        }
    }
}
