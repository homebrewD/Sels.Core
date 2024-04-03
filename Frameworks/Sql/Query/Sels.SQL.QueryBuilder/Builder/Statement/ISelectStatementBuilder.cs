using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using System.Linq.Expressions;
using System.Text;
using SqlConstantExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ConstantExpression;
using SqlParameterExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ParameterExpression;
using Sels.Core.Extensions;
using System.Linq;
using System;
using Sels.Core.Extensions.Reflection;
using System.Collections.Generic;
using Sels.Core;
using Sels.Core.Models;
using Sels.SQL.QueryBuilder.Expressions.Select;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Exposes methods for building a sql select query.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to select from</typeparam>
    public interface ISelectStatementBuilder<TEntity, out TDerived> : 
        IStatementQueryBuilder<TEntity, SelectExpressionPositions, TDerived>, 
        IStatementConditionBuilder<TEntity, TDerived>,
        IStatementHavingBuilder<TEntity, TDerived>,
        IStatementJoinBuilder<TEntity, TDerived>,
        IStatementOrderByBuilder<TEntity, TDerived>
    {
        #region Keywords
        /// <summary>
        /// Select only distinct rows.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TDerived Distinct() => InnerExpressions.Any(x => x is DistintExpression) ? Instance : Expression(DistintExpression.Instance, SelectExpressionPositions.Before);
        #endregion

        #region Select
        #region All
        /// <summary>
        /// Selects all columns.
        /// </summary>
        /// <param name="dataset">Optional dataset to select from. Is the alias assigned to tables / sub queries</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived All(object dataset = null) => Expression(new AllColumnsExpression(dataset), SelectExpressionPositions.Column);
        /// <summary>
        /// Select all columns from the dataset alias defined by <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived AllOf<T>(object dataset = null) => All(dataset ?? typeof(T));
        /// <summary>
        /// Select all columns from the dataset alias defined by <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived AllOf(object dataset = null) => AllOf<TEntity>(dataset);
        #endregion
        #region Column
        /// <summary>
        /// Specifies a column to select.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The name of the column to select</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Column(object dataset, string column) => ColumnExpression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))));
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Column<T>(object dataset, Expression<Func<T, object>> property) => Column(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Specifies a column to select.
        /// </summary>
        /// <param name="column">The name of the column to select</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Column(string column) => Column(null, column);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Column<T>(Expression<Func<T, object>> property) => Column<T>(typeof(T), property);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Column(Expression<Func<TEntity, object>> property) => Column<TEntity>(property);
        /// <summary>
        /// Specifies a column to select by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Column(object dataset, Expression<Func<TEntity, object>> property) => Column<TEntity>(dataset, property);
        #endregion
        #region Columns
        /// <summary>
        /// Specifies the columns to select.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="columns"/> from</param>
        /// <param name="columns">The columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(object dataset, IEnumerable<string> columns);
        /// <summary>
        /// Specifies the columns to select.
        /// </summary>
        /// <param name="column">The primary column to select</param>
        /// <param name="columns">Additional columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(string column, params string[] columns) => Columns(Helper.Collection.Enumerate(column, columns));
        /// <summary>
        /// Specifies the columns to select.
        /// </summary>
        /// <param name="columns">The columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(IEnumerable<string> columns) => Columns(null, columns);
        /// <summary>
        /// Specifies the columns to select.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> and <paramref name="columns"/> from</param>
        /// <param name="column">The primary column to select</param>
        /// <param name="columns">Additional columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(object dataset, string column, params string[] columns) => Columns(dataset, Helper.Collection.Enumerate(column, columns));
        #endregion
        #region ColumnsOf
        /// <summary>
        /// Specifies the columns to select by selecting the names of all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the properties from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf<T>(object dataset, params string[] excludedProperties);
        /// <summary>
        /// Specifies the columns to select by selecting the names of all public properties on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf(params string[] excludedProperties) => ColumnsOf<TEntity>(excludedProperties);
        /// <summary>
        /// Specifies the columns to select by selecting the names of all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the properties from</typeparam>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf<T>(params string[] excludedProperties) => ColumnsOf<T>(typeof(T), excludedProperties);
        /// <summary>
        /// Specifies the columns to select by selecting the names of all public properties on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf(object dataset, params string[] excludedProperties) => ColumnsOf<TEntity>(dataset, excludedProperties);
        #endregion
        #region Value
        /// <summary>
        /// Selects <paramref name="value"/> as a constant sql value.
        /// </summary>
        /// <param name="value">The value to select</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Value(object value) => ColumnExpression(new SqlConstantExpression(value));
        #endregion
        #region Expression
        /// <summary>
        /// Adds a new expression in the <see cref="SelectExpressionPositions.Column"/> position and returns a builder for configuring said value.
        /// </summary>
        /// <param name="expression">The expression that will be selected</param>
        /// <param name="order">Optional order for <paramref name="expression"/>. A lower order means it will be compiled first. Can be used to sort custom expressions</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> ColumnExpression(IExpression expression, int order = 0)
        {
            expression.ValidateArgument(nameof(expression));
            var builder = new SelectedValueExpression<TEntity, TDerived>(expression, this);
            Expression(builder, SelectExpressionPositions.Column, order);
            return builder;
        }
        /// <summary>
        /// Selects a raw sql expression defined by the <see cref="object.ToString()"/> on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Object containing the raw sql expression</param>
        /// <param name="order">Optional order for the created expression. A lower order means it will be compiled first. Can be used to sort custom expressions</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> ColumnExpression(object value, int order = 0) => ColumnExpression(new RawExpression(value.ValidateArgument(nameof(value))), order);
        /// <summary>
        /// Defines a value to select using <paramref name="builder"/>.
        /// </summary>
        /// <typeparam name="T">The main entity to create the expression for</typeparam>
        /// <param name="builder">The builder to create the expression</param>
        /// <param name="order">Optional order for the created expression. A lower order means it will be compiled first. Can be used to sort custom expressions</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> ColumnExpression<T>(Action<ISharedExpressionBuilder<T, Null>> builder, int order = 0) => ColumnExpression(new ExpressionBuilder<T>(builder), order);
        /// <summary>
        /// Defines a value to select using <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to create the expression</param>
        /// <param name="order">Optional order for the created expression. A lower order means it will be compiled first. Can be used to sort custom expressions</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> ColumnExpression(Action<ISharedExpressionBuilder<TEntity, Null>> builder, int order = 0) => ColumnExpression(new ExpressionBuilder<TEntity>(builder), order);

        /// <summary>
        /// Adds a new expression in the <see cref="SelectExpressionPositions.Column"/> position and returns a builder for configuring said value.
        /// Expression should return an aggregated value (From functions, ...)
        /// </summary>
        /// <param name="expression">The expression that will be selected</param>
        /// <param name="order">Optional order for <paramref name="expression"/>. A lower order means it will be compiled first. Can be used to sort custom expressions</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> AggregatedColumnExpression(IExpression expression, int order = 0)
        {
            expression.ValidateArgument(nameof(expression));
            var builder = new AggregateSelectedValueExpression<TEntity, TDerived>(expression, this);
            Expression(builder, SelectExpressionPositions.Column, order);
            return builder;
        }
        /// <summary>
        /// Selects a raw sql expression defined by the <see cref="object.ToString()"/> on <paramref name="value"/>.
        /// Expression should return an aggregated value (From functions, ...)
        /// </summary>
        /// <param name="value">Object containing the raw sql expression</param>
        /// <param name="order">Optional order for the created expression. A lower order means it will be compiled first. Can be used to sort custom expressions</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> AggregatedColumnExpression(object value, int order = 0) => AggregatedColumnExpression(new RawExpression(value.ValidateArgument(nameof(value))), order);
        #endregion
        #region Case
        /// <summary>
        /// Select a value using a case expression.
        /// </summary>
        /// <param name="caseBuilder">Delegate that configures the case expression</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Case(Action<ICaseExpressionRootBuilder<TEntity>> caseBuilder) => Case<TEntity>(caseBuilder);
        /// <summary>
        /// Select a value using a case expression.
        /// </summary>
        /// <typeparam name="T">The main type to create the case expression with</typeparam>
        /// <param name="caseBuilder">Delegate that configures the case expression</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Case<T>(Action<ICaseExpressionRootBuilder<T>> caseBuilder) => ColumnExpression(new WrappedExpression(new CaseExpression<T>(caseBuilder)));
        #endregion
        #region Parameter
        /// <summary>
        /// Select the value from an SQL parameter.
        /// </summary>
        /// <param name="parameter">The name of the sql parameter</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Parameter(string parameter) => ColumnExpression(new SqlParameterExpression(parameter.ValidateArgumentNotNullOrWhitespace(nameof(parameter))));
        /// <summary>
        /// Select the value from an SQL parameter where the parameter name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Parameter<T>(Expression<Func<T, object>> property) => Parameter(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Select the value from an SQL parameter where the parameter name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Parameter(Expression<Func<TEntity, object>> property) => Parameter<TEntity>(property);
        #endregion
        #region Variable
        /// <summary>
        /// Select the value from an SQL variable.
        /// </summary>
        /// <param name="variable">The name of the sql variable</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Variable(string variable) => ColumnExpression(new VariableExpression(variable.ValidateArgumentNotNullOrWhitespace(nameof(variable))));
        /// <summary>
        /// Select the value from an SQL parameter where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Variable<T>(Expression<Func<T, object>> property) => Variable(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Select the value from an SQL parameter where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Variable(Expression<Func<TEntity, object>> property) => Variable<TEntity>(property);
        #endregion
        #region Query
        /// <summary>
        /// Selects a value from a sub query.
        /// </summary>
        /// <param name="query">Delegate that adds the query to the supplied builder</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Query(Action<StringBuilder, ExpressionCompileOptions> query) => ColumnExpression(new SubQueryExpression(null, query.ValidateArgument(nameof(query))));
        /// <summary>
        /// Selects a value from a sub query.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Query(string query) => Query((b, o) => b.Append(query.ValidateArgumentNotNullOrWhitespace(nameof(query))));
        /// <summary>
        /// Selects a value from a sub query.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementSelectedValueBuilder<TEntity, TDerived> Query(IQueryBuilder builder) => ColumnExpression(new SubQueryExpression(null, builder.ValidateArgument(nameof(builder))));

        #endregion
        #endregion

        #region From
        /// <summary>
        /// Defines the table to select from.
        /// </summary>
        /// <param name="table">The table to select from</param>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From(string table, object datasetAlias = null, string database = null, string schema = null) => Expression(new TableExpression(database, schema, table.ValidateArgumentNotNullOrWhitespace(nameof(table)), datasetAlias), SelectExpressionPositions.From);
        /// <summary>
        /// Defines the table to select from by using the name of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the table name from</typeparam>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From<T>(object datasetAlias = null, string database = null, string schema = null) => From(typeof(T).Name, datasetAlias ?? typeof(T), database, schema);
        /// <summary>
        /// Defines the table to select from by using the name of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="datasetAlias">Optional alias for the dataset. If a type is used the alias defined for the type is taken</param>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived From(object datasetAlias = null, string database = null, string schema = null) => From<TEntity>(datasetAlias, database, schema);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <param name="query">Delegate that adds the query to the supplied builder</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery(Action<StringBuilder, ExpressionCompileOptions> query, object datasetAlias) => Expression(new SubQueryExpression(datasetAlias.ValidateArgument(nameof(datasetAlias)), query.ValidateArgument(nameof(query))), SelectExpressionPositions.From);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <param name="query">The sub query</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery(string query, object datasetAlias) => FromQuery((b, o) => b.Append(query.ValidateArgumentNotNullOrWhitespace(nameof(query))), datasetAlias);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <param name="builder">Builder that creates the sub query</param>
        /// <param name="datasetAlias">Alias for the sub query dataset. If a type is used the alias defined for the type is taken</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery(IQueryBuilder builder, object datasetAlias) => Expression(new SubQueryExpression(datasetAlias.ValidateArgument(nameof(datasetAlias)), builder.ValidateArgument(nameof(builder))), SelectExpressionPositions.From);
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="query">Delegate that adds the query to the supplied builder</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery<T>(Action<StringBuilder, ExpressionCompileOptions> query) => FromQuery(query, typeof(T));
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="query">The sub query</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery<T>(string query) => FromQuery(query, typeof(T));
        /// <summary>
        /// Defines the sub query to select from.
        /// </summary>
        /// <typeparam name="T">The type to get the dataset alias from</typeparam>
        /// <param name="builder">Builder that creates the sub query</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived FromQuery<T>(IQueryBuilder builder) => FromQuery(builder, typeof(T));
        #endregion

        #region Functions
        #region Count
        /// <summary>
        /// Counts the total amount of rows returned.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> CountAll(object dataset = null) => AggregatedColumnExpression(new ColumnFunctionExpression(Functions.Count, new ColumnExpression(dataset, Sql.All.ToString())));
        /// <summary>
        /// Counts the total amount of rows where <paramref name="column"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to count</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Count(object dataset, string column) => AggregatedColumnExpression(new ColumnFunctionExpression(Functions.Count, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)))));
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="T"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Count<T>(object dataset, Expression<Func<T, object>> property) => Count(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Count(object dataset, Expression<Func<TEntity, object>> property) => Count<TEntity>(dataset, property);
        /// <summary>
        /// Counts the total amount of rows where <paramref name="column"/> is not null.
        /// </summary>
        /// <param name="column">The column to count</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Count(string column) => Count(null, column);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="T"/> is not null.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Count<T>(Expression<Func<T, object>> property) => Count<T>(typeof(T), property);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is not null.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Count(Expression<Func<TEntity, object>> property) => Count<TEntity>(property);

        #endregion
        #region Avg
        /// <summary>
        /// Calculates the average of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the average from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Average(object dataset, string column = null) => AggregatedColumnExpression(new ColumnFunctionExpression(Functions.Avg, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)))));
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Average<T>(object dataset, Expression<Func<T, object>> property = null) => Average(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Average(object dataset, Expression<Func<TEntity, object>> property = null) => Average<TEntity>(dataset, property);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Average(Expression<Func<TEntity, object>> property = null) => Average<TEntity>(property);
        /// <summary>
        /// Calculates the average of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the average from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Average(string column = null) => Average(null, column);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Average<T>(Expression<Func<T, object>> property = null) => Average<T>(typeof(T), property);
        #endregion
        #region Sum
        /// <summary>
        /// Calculates the sum of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the average from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Sum(object dataset, string column = null) => AggregatedColumnExpression(new ColumnFunctionExpression(Functions.Sum, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)))));
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Sum<T>(object dataset, Expression<Func<T, object>> property = null) => Sum(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Sum(object dataset, Expression<Func<TEntity, object>> property = null) => Sum<TEntity>(dataset, property);
        /// <summary>
        /// Calculates the sum of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the average from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Sum(string column = null) => Sum(null, column);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Sum<T>(Expression<Func<T, object>> property = null) => Sum<T>(typeof(T), property);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Sum(Expression<Func<TEntity, object>> property = null) => Sum<TEntity>(property);
        #endregion
        #region Max
        /// <summary>
        /// Returns the largest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the max from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Max(object dataset, string column = null) => AggregatedColumnExpression(new ColumnFunctionExpression(Functions.Max, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)))));
        /// <summary>
        /// Returns the largest value of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Max<T>(object dataset, Expression<Func<T, object>> property = null) => Max(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Max(object dataset, Expression<Func<TEntity, object>> property = null) => Max<TEntity>(dataset, property);
        /// <summary>
        /// Returns the largest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the max from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Max(string column = null) => Max(null, column);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Max<T>(Expression<Func<T, object>> property = null) => Max<T>(typeof(T), property);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Max(Expression<Func<TEntity, object>> property = null) => Max<TEntity>(property);
        #endregion
        #region Min
        /// <summary>
        /// Returns the smallest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the max from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Min(object dataset, string column = null) => AggregatedColumnExpression(new ColumnFunctionExpression(Functions.Min, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)))));
        /// <summary>
        /// Returns the smallest value of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Min<T>(object dataset, Expression<Func<T, object>> property = null) => Min(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Min(object dataset, Expression<Func<TEntity, object>> property = null) => Min<TEntity>(dataset, property);
        /// <summary>
        /// Returns the smallest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the max from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Min(string column = null) => Min(null, column);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Min<T>(Expression<Func<T, object>> property = null) => Min<T>(typeof(T), property);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Min(Expression<Func<TEntity, object>> property = null) => Min<TEntity>(property);
        #endregion
        #endregion

        #region Window Functions
        #region Row Number
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.RowNumber"/>
        /// </summary>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> RowNumber() => AggregatedColumnExpression(new FunctionExpression(new EnumExpression<WindowFunctions>(WindowFunctions.RowNumber)));
        #endregion
        #region Dense
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Dense"/>
        /// </summary>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Dense() => AggregatedColumnExpression(new FunctionExpression(new EnumExpression<WindowFunctions>(WindowFunctions.Dense)));
        #endregion
        #region DenseRank
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.DenseRank"/>
        /// </summary>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> DenseRank() => AggregatedColumnExpression(new FunctionExpression(new EnumExpression<WindowFunctions>(WindowFunctions.DenseRank)));
        #endregion
        #region Ntile
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Ntile"/>
        /// </summary>
        /// <param name="buckets">How many buckets to divide the result set into</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Ntile(uint buckets) => AggregatedColumnExpression(new FunctionExpression(new EnumExpression<WindowFunctions>(WindowFunctions.Ntile), new SqlConstantExpression(buckets.ValidateArgumentLargerOrEqual(nameof(buckets), 1u))));
        #endregion
        #region Lag
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lag"/>
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The name of the column to select</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lag(object dataset, string column, uint rows) => AggregatedColumnExpression(new FunctionExpression(new EnumExpression<WindowFunctions>(WindowFunctions.Lag), new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))), new SqlConstantExpression(rows.ValidateArgumentLargerOrEqual(nameof(rows), 1u))));
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lag"/>
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lag<T>(object dataset, Expression<Func<T, object>> property, uint rows) => Lag(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, rows);
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lag"/>
        /// </summary>
        /// <param name="column">The name of the column to select</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lag(string column, uint rows) => Lag(null, column, rows);
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lag"/>
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lag<T>(Expression<Func<T, object>> property, uint rows) => Lag<T>(typeof(T), property, rows);
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lag"/>
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lag(Expression<Func<TEntity, object>> property, uint rows) => Lag<TEntity>(property, rows);
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lag"/>
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lag(object dataset, Expression<Func<TEntity, object>> property, uint rows) => Lag<TEntity>(dataset, property, rows);
        #endregion
        #region Lead
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lead"/>
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The name of the column to select</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lead(object dataset, string column, uint rows) => AggregatedColumnExpression(new FunctionExpression(new EnumExpression<WindowFunctions>(WindowFunctions.Lead), new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))), new SqlConstantExpression(rows.ValidateArgumentLargerOrEqual(nameof(rows), 1u))));
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lead"/>
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lead<T>(object dataset, Expression<Func<T, object>> property, uint rows) => Lead(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, rows);
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lead"/>
        /// </summary>
        /// <param name="column">The name of the column to select</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lead(string column, uint rows) => Lead(null, column, rows);
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lead"/>
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lead<T>(Expression<Func<T, object>> property, uint rows) => Lead<T>(typeof(T), property, rows);
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lead"/>
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lead(Expression<Func<TEntity, object>> property, uint rows) => Lead<TEntity>(property, rows);
        /// <summary>
        /// <inheritdoc cref="WindowFunctions.Lead"/>
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="rows">How many rows away to pull from</param>
        /// <returns>Builder for configuring the selected value</returns>
        ISelectStatementAggregatedValueBuilder<TEntity, TDerived> Lead(object dataset, Expression<Func<TEntity, object>> property, uint rows) => Lead<TEntity>(dataset, property, rows);
        #endregion
        #endregion

        #region GroupBy
        /// <summary>
        /// Groups query results by <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column to group by</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(object dataset, string column) => Expression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))), SelectExpressionPositions.GroupBy);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/> is used as the column name.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/></param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy<T>(object dataset, Expression<Func<T, object>> property) => GroupBy(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Groups query results by <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to group by</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(string column) => GroupBy(null, column);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/> is used as the column name.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        ///<param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy<T>(Expression<Func<T, object>> property) => GroupBy<T>(typeof(T), property);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is used as the column name.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/></param>
        ///<param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(object dataset, Expression<Func<TEntity, object>> property) => GroupBy<TEntity>(dataset, property);
        /// <summary>
        /// Groups query results by column where the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is used as the column name.
        /// </summary>
        ///<param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived GroupBy(Expression<Func<TEntity, object>> property) => GroupBy<TEntity>(typeof(TEntity), property);
        #endregion

        #region Union
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are excluded.
        /// </summary>
        /// <param name="query">Delegate that returns the query string</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Union(Func<ExpressionCompileOptions, string> query) => Expression(new UnionExpression(query.ValidateArgument(nameof(query))), SelectExpressionPositions.After);
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are excluded.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Union(string query) => Union(x => query.ValidateArgumentNotNullOrWhitespace(nameof(query)));
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are excluded.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Union(IQueryBuilder builder) => Union(x => builder.ValidateArgument(nameof(builder)).Build(x));
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are included.
        /// </summary>
        /// <param name="query">Delegate that returns the query string</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived UnionAll(Func<ExpressionCompileOptions, string> query) => Expression(new UnionExpression(query.ValidateArgument(nameof(query)), false), SelectExpressionPositions.After);
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are included.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived UnionAll(string query) => UnionAll(x => query.ValidateArgumentNotNullOrWhitespace(nameof(query)));
        /// <summary>
        /// Adds a sub query expression whose results will be added to the result set of the current query. Duplicate rows are included.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived UnionAll(IQueryBuilder builder) => UnionAll(x => builder.ValidateArgument(nameof(builder)).Build(x));
        #endregion
    }
    /// <inheritdoc cref="ISelectStatementBuilder{TEntity, TDerived}"/>
    public interface ISelectStatementBuilder<TEntity> : ISelectStatementBuilder<TEntity, ISelectStatementBuilder<TEntity>>
    {

    }
}
