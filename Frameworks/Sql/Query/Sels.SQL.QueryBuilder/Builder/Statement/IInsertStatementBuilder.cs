using Sels.SQL.QueryBuilder.Builder.Expressions;
using System.Linq.Expressions;
using SqlParameterExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ParameterExpression;
using Sels.Core.Extensions;
using System.Collections.Generic;
using Sels.Core;
using System.Linq;
using System;
using Sels.Core.Extensions.Reflection;
using Sels.SQL.QueryBuilder.Expressions;
using Sels.Core.Models;
using Sels.Core.Extensions.Collections;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Exposes methods for building a sql insert query.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to delete</typeparam>
    public interface IInsertStatementBuilder<TEntity, out TDerived> : IStatementQueryBuilder<TEntity, InsertExpressionPositions, TDerived>
    {
        #region Into
        /// <summary>
        /// Defines the table to insert into.
        /// </summary>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <param name="table">The name of the table to insert into</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Into(string table, string database = null, string schema = null) => Expression(new TableExpression(database, schema, table.ValidateArgumentNotNullOrWhitespace(nameof(table)), null), InsertExpressionPositions.Into);
        /// <summary>
        /// Defines the table to insert into where the table name is taken from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type whose name to use as table name</typeparam>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Into<T>(string database = null, string schema = null) => Expression(new TableExpression(database, schema, typeof(T).Name, typeof(T)), InsertExpressionPositions.Into);
        /// <summary>
        /// Defines the table to insert into where the table name is taken from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="database">Optional database to select the table from</param>
        /// <param name="schema">Optional schema where the table is defined in</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Into(string database = null, string schema = null) => Into<TEntity>(database, schema);
        #endregion

        #region Columns
        #region Column
        /// <summary>
        /// Specifies a column to insert into.
        /// </summary>
        /// <param name="column">The name of the column to insert into</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(string column) => Expression(new ColumnExpression(null, column.ValidateArgumentNotNullOrWhitespace(nameof(column))), InsertExpressionPositions.Columns);
        /// <summary>
        /// Specifies a column to insert into by using the name of the property selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column<T>(Expression<Func<T, object>> property) => Column(property.ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Specifies a column to insert into by using the name of the property selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Column(Expression<Func<TEntity, object>> property) => Column<TEntity>(property);
        #endregion
        #region Columns
        /// <summary>
        /// Specifies the columns to insert into.
        /// </summary>
        /// <param name="columns">The columns to select</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(IEnumerable<string> columns);
        /// <summary>
        /// Specifies the columns to insert into.
        /// </summary>
        /// <param name="column">The primary column to insert into</param>
        /// <param name="columns">Additional columns to insert into</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(string column, params string[] columns) => Columns(Helper.Collection.Enumerate(column, columns));
        /// <summary>
        /// Specifies the columns to insert into by using the name of the property selected by the expressionsfrom <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="properties">Additional expressions that point to the properties to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns<T>(Expression<Func<T, object>> property, params Expression<Func<T, object>>[] properties) => Columns(Helper.Collection.Enumerate(property, properties).Select(x => x.ExtractProperty(nameof(property)).Name));
        /// <summary>
        /// Specifies the columns to insert into by using the name of the property selected by the expressionsfrom <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="properties">Additional expressions that point to the properties to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Columns(Expression<Func<TEntity, object>> property, params Expression<Func<TEntity, object>>[] properties) => Columns<TEntity>(property, properties);
        #endregion
        #region ColumnsOf
        /// <summary>
        /// Specifies the columns to insert into by selecting the names of all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the properties from</typeparam>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf<T>(params string[] excludedProperties);
        /// <summary>
        /// Specifies the columns to insert into by selecting the names of all public properties on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ColumnsOf(params string[] excludedProperties) => ColumnsOf<TEntity>(excludedProperties);
        #endregion
        #endregion

        #region Values
        #region Constants
        /// <summary>
        /// Defines the constant values to insert.
        /// </summary>
        /// <param name="values">List of values to insert</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Values(IEnumerable<object> values);
        /// <summary>
        /// Defines the constant values to insert.
        /// </summary>
        /// <param name="values">List of values to insert</param>
        /// <typeparam name="T">The type of the elements</typeparam>
        /// <returns>Current builder for method chaining</returns>
        TDerived Values<T>(T[] values) => Values(values.ValidateArgument(nameof(values)).Enumerate());
        /// <summary>
        /// Defines the constant values to insert.
        /// </summary>
        /// <param name="value">The first value to insert</param>
        /// <param name="values">Optional additional values to insert</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Values(object value, params object[] values) => Values(Helper.Collection.Enumerate(value, values));
        /// <summary>
        /// Defines the constant values to insert by taking the values from all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <param name="valueObject">The object to get the values from</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ValuesUsing<T>(T valueObject, params string[] excludedProperties);
        #endregion
        #region Parameters
        /// <summary>
        /// Defines the values to insert by defining sql parameters.
        /// </summary>
        /// <param name="parameters">The parameters to insert with</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameters(IEnumerable<string> parameters) => Values(parameters.ValidateArgumentNotNullOrEmpty(nameof(parameters)).Select(x => new SqlParameterExpression(x)));
        /// <summary>
        /// Defines the values to insert by defining sql parameters.
        /// </summary>
        /// <param name="parameter">The first parameter to insert with</param>
        /// <param name="parameters">Optional additional parameters to insert with</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameters(string parameter, params string[] parameters) => Parameters(Helper.Collection.Enumerate(parameter, parameters));
        /// <summary>
        /// Defines the values to insert by defining sql parameters where the parameters names are taken from all public properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the properties from</typeparam>
        /// <param name="suffix">Optional suffix that needs to be appended to the parameters name. When set to null the suffix is omited</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ParametersFrom<T>(string suffix = null, params string[] excludedProperties);
        /// <summary>
        /// Defines the values to insert by defining sql parameters where the parameters names are taken from all public properties on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="suffix">Optional suffix that needs to be appended to the parameters name. When set to null the suffix is omited</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ParametersFrom(string suffix = null, params string[] excludedProperties) => ParametersFrom<TEntity>(suffix, excludedProperties);

        /// <summary>
        /// Specifies the values to insert into by using the name of the property selected by the expressionsfrom <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="properties">Additional expressions that point to the properties to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameters<T>(Expression<Func<T, object>> property, params Expression<Func<T, object>>[] properties) => Parameters(Helper.Collection.Enumerate(property, properties).Select(x => x.ExtractProperty(nameof(property)).Name));
        /// <summary>
        /// Specifies the columns to insert into by using the name of the property selected by the expressionsfrom <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="properties">Additional expressions that point to the properties to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameters(Expression<Func<TEntity, object>> property, params Expression<Func<TEntity, object>>[] properties) => Parameters<TEntity>(property, properties);
        /// <summary>
        /// Specifies the values to insert into by using the name of the property selected by the expressionsfrom <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="suffix">Suffix that will be added after the parameter name. Useful when inserting multiple rows</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="properties">Additional expressions that point to the properties to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameters<T>(string suffix, Expression<Func<T, object>> property, params Expression<Func<T, object>>[] properties) => Values(Helper.Collection.Enumerate(property, properties).Select(x => x.ExtractProperty(nameof(property)).Name).Select(x => new SqlParameterExpression(x, suffix)));
        /// <summary>
        /// Specifies the columns to insert into by using the name of the property selected by the expressionsfrom <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="suffix">Suffix that will be added after the parameter name. Useful when inserting multiple rows</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="properties">Additional expressions that point to the properties to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Parameters(string suffix, Expression<Func<TEntity, object>> property, params Expression<Func<TEntity, object>>[] properties) => Parameters<TEntity>(suffix, property, properties);
        #endregion

        #region Expression
        /// <summary>
        /// Defines the values to insert by selecting the expression using the builder delegates.
        /// </summary>
        /// <typeparam name="T">The main type to create the builder for</typeparam>
        /// <param name="expression">Delegate that selects the expression to insert</param>
        /// <param name="expressions">Optional additional expressions to insert</param>
        /// <returns></returns>
        TDerived Values<T>(Action<ISharedExpressionBuilder<T, Null>> expression, params Action<ISharedExpressionBuilder<T, Null>>[] expressions) => Values(Helper.Collection.Enumerate(expression.ValidateArgument(nameof(expression)), expressions).Select(x => new ExpressionBuilder<T>(x)));
        /// <summary>
        /// Defines the values to insert by selecting the expression using the builder delegates.
        /// </summary>
        /// <param name="expression">Delegate that selects the expression to insert</param>
        /// <param name="expressions">Optional additional expressions to insert</param>
        /// <returns></returns>
        TDerived Values(Action<ISharedExpressionBuilder<TEntity, Null>> expression, params Action<ISharedExpressionBuilder<TEntity, Null>>[] expressions) => Values<TEntity>(expression, expressions);

        #endregion
        #endregion
    }

    /// <inheritdoc cref="IInsertStatementBuilder{TEntity, TDerived}"/>
    public interface IInsertStatementBuilder<TEntity> : IInsertStatementBuilder<TEntity, IInsertStatementBuilder<TEntity>>
    {

    }
}
