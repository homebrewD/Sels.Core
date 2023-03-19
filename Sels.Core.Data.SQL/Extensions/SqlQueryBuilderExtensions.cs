﻿using Dapper;
using Microsoft.Extensions.Logging;
using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Statement;
using Sels.Core.Data.SQL.SearchCriteria;

namespace Sels.Core.Data.SQL
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

        #region Search Criteria
        /// <summary>
        /// Converts <paramref name="searchCriteria"/> to SQL conditions.
        /// </summary>
        /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
        /// <typeparam name="TSearchCriteria">Type of the search criteria</typeparam>
        /// <param name="builder">The builder to create the conditions with</param>
        /// <param name="searchCriteria">THe object to convert to SQL conditions</param>
        /// <param name="configurator">Optional delegate for configuring how to convert <paramref name="searchCriteria"/></param>
        /// <param name="parameters">Optional parameter bag that can be provided. Implicit conditions that use parameters will automatically add the values to the bag</param>
        /// <param name="logger">Optional logger for debugging</param>
        /// <returns>The final builder after the creating the conditions or null if no conditions were created</returns>
        public static IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>? FromSearchCriteria<TEntity, TSearchCriteria>(this IStatementConditionExpressionBuilder<TEntity> builder, TSearchCriteria searchCriteria, Action<ISearchCriteriaConverterBuilder<TEntity, TSearchCriteria>>? configurator = null, DynamicParameters? parameters = null, ILogger? logger = null)
        {
            Guard.IsNotNull(builder);
            Guard.IsNotNull(searchCriteria);

            var converter = new SearchCriteriaConverter<TEntity, TSearchCriteria>(configurator, logger);
            return converter.Build(builder, searchCriteria, parameters);
        }

        /// <summary>
        /// Converts <paramref name="searchCriteria"/> to SQL conditions.
        /// </summary>
        /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
        /// <typeparam name="TDerived">The type of the builder to create the conditions for</typeparam>
        /// <typeparam name="TSearchCriteria">Type of the search criteria</typeparam>
        /// <param name="builder">The builder to create the conditions with</param>
        /// <param name="searchCriteria">THe object to convert to SQL conditions</param>
        /// <param name="configurator">Optional delegate for configuring how to convert <paramref name="searchCriteria"/></param>
        /// <param name="parameters">Optional parameter bag that can be provided. Implicit conditions that use parameters will automatically add the values to the bag</param>
        /// <param name="logger">Optional logger for debugging</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived FromSearchCriteria<TEntity, TDerived, TSearchCriteria>(this IStatementConditionBuilder<TEntity, TDerived> builder, TSearchCriteria searchCriteria, Action<ISearchCriteriaConverterBuilder<TEntity, TSearchCriteria>>? configurator = null, DynamicParameters? parameters = null, ILogger? logger = null)
        {
            Guard.IsNotNull(builder);
            Guard.IsNotNull(searchCriteria);

            return builder.Where(x => x.FromSearchCriteria(searchCriteria, configurator, parameters, logger));
        }
        #endregion
    }
}
