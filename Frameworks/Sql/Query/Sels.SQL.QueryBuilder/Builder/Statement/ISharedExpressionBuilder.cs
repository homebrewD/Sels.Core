using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using System;
using System.Linq.Expressions;
using System.Text;
using SqlConstantExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ConstantExpression;
using SqlParameterExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ParameterExpression;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Models;
using System.Collections.Generic;
using Sels.Core;
using Sels.Core.Extensions.Conversion;
using System.Linq;
namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder that adds common sql expressions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TReturn">The type to return for the fluent syntax</typeparam>
    public interface ISharedExpressionBuilder<TEntity, out TReturn>
    {
        #region Expression
        /// <summary>
        /// Adds a sql expression to the builder.
        /// </summary>
        /// <param name="expression">The sql expression to add</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expression(IExpression expression);
        /// <summary>
        /// Adds a raw sql expression to the builder.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql expression</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expression(string sqlExpression) => Expression(new RawExpression(sqlExpression == null ? string.Empty : sqlExpression));
        /// <summary>
        /// Adds a sql expression to the builder.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql expression to the provided string builder</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expression(Action<StringBuilder, ExpressionCompileOptions> sqlExpression) => Expression(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        /// <summary>
        /// Adds an expression that consists of other expressions optionally joined together using a expression or object.
        /// Useful for example adding a minus before another expression.
        /// </summary>
        /// <param name="joinValue">Optional object containing the value to join together <paramref name="expressions"/> with. Can be null or <see cref="IExpression"/></param>
        /// <param name="expressions">Enumerator containing the expressions to join together</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expressions(object joinValue, IEnumerable<IExpression> expressions) => Expression(new ExpressionContainer(expressions, joinValue));       
        /// <summary>
        /// Adds an expression that consists of other expressions optionally joined together using a expression or object.
        /// Useful for example adding a minus before another expression.
        /// </summary>
        /// <param name="joinValue">Optional object containing the value to join together the expressions. Can be null or <see cref="IExpression"/></param>
        /// <param name="firstExpression">The first expression to join</param>
        /// <param name="secondExpression">The second expression to join</param>
        /// <param name="additionalExpressions">Any additional expressions to join</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expressions(object joinValue, object firstExpression, object secondExpression, params object[] additionalExpressions)
        {           
            var expressions = Helper.Collection.EnumerateAll(firstExpression.AsEnumerable(), secondExpression.AsEnumerable(), additionalExpressions).Where(x => x != null);

            return Expression(new ExpressionContainer(expressions.Select(x => x is IExpression expression ? expression : new SqlConstantExpression(x)), joinValue));
        }
        /// <summary>
        /// Adds an expression that consists of other expressions optionally joined together using a expression or object.
        /// Useful for example adding a minus before another expression.
        /// </summary>
        /// <param name="joinValueBuilder">Optional builder that will create the expression to join with</param>
        /// <param name="expressionBuilders">Enumerator containing the builders that will create the expressions to join with</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expressions(Action<ISharedExpressionBuilder<TEntity, Null>> joinValueBuilder, IEnumerable<Action<ISharedExpressionBuilder<TEntity, Null>>> expressionBuilders) => Expression(new ExpressionContainer(expressionBuilders != null ? expressionBuilders.Select(e => new ExpressionBuilder<TEntity>(e)) : null, joinValueBuilder != null ? new ExpressionBuilder<TEntity>(joinValueBuilder) : null));

        /// <summary>
        /// Adds an expression that consists of other expressions optionally joined together using a expression or object.
        /// Useful for example adding a minus before another expression.
        /// </summary>
        /// <param name="joinValueBuilder">Optional builder that will create the expression to join with</param>
        /// <param name="firstExpressionBuilder">Builder that will create the first expression to join</param>
        /// <param name="secondExpressionBuilder">Builder that will create the second expression to join</param>
        /// <param name="additionalExpressionsBuilders">Any additional builders that will create expressions to join</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Expressions(Action<ISharedExpressionBuilder<TEntity, Null>> joinValueBuilder, Action<ISharedExpressionBuilder<TEntity, Null>> firstExpressionBuilder, Action<ISharedExpressionBuilder<TEntity, Null>> secondExpressionBuilder, params Action<ISharedExpressionBuilder<TEntity, Null>>[] additionalExpressionsBuilders)
        {
            var expressions = Helper.Collection.EnumerateAll(firstExpressionBuilder.AsEnumerable(), secondExpressionBuilder.AsEnumerable(), additionalExpressionsBuilders).Where(x => x != null);

            return Expression(new ExpressionContainer(expressions.Select(x => new ExpressionBuilder<TEntity>(x)), joinValueBuilder != null ? new ExpressionBuilder<TEntity>(joinValueBuilder) : null));
        }
        #endregion

        #region Column
        /// <summary>
        /// Adds a column expression.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select <paramref name="column"/> from</param>
        /// <param name="column">The column to create the condition for</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(object dataset, string column) => Expression(new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column))));
        /// <summary>
        /// Adds a column expression.
        /// </summary>
        /// <param name="column">The column to create the condition for</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(string column) => Column(null, column);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="T"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column<T>(object dataset, Expression<Func<T, object>> property) => Column(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column<T>(Expression<Func<T, object>> property) => Column<T>(typeof(T), property);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(Expression<Func<TEntity, object>> property) => Column<TEntity>(property);
        /// <summary>
        /// Adds a column expression where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Overwrites the default dataset name defined for type <typeparamref name="TEntity"/>. If a type is used the alias defined for the type is taken. Set to an empty string to omit the dataset alias</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Column(object dataset, Expression<Func<TEntity, object>> property) => Column<TEntity>(dataset, property);
        #endregion

        #region Value
        /// <summary>
        /// Adds a SQL null value.
        /// </summary>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Null() => Value(DBNull.Value);
        /// <summary>
        /// Adds a constant sql value expression.
        /// </summary>
        /// <param name="constantValue">Object containing the constant sql value to compare to</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Value(object constantValue) => Expression(new SqlConstantExpression(constantValue.ValidateArgument(nameof(constantValue))));
        #endregion

        #region Parameter
        /// <summary>
        /// Adds a sql parameter expression.
        /// </summary>
        /// <param name="parameter">The name of the sql parameter</param>
        /// <param name="suffix">Optional suffix to appends after the name of the parameter. Useful when using multiple entities in the same query or when columns share the same name.</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Parameter(string parameter, string suffix = null) => Expression(new SqlParameterExpression(parameter.ValidateArgumentNotNullOrWhitespace(nameof(parameter)), suffix));
        /// <summary>
        /// Adds a sql parameter expression where the parameter name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="suffix">Optional suffix to appends after the name of the parameter. Useful when using multiple entities in the same query or when columns share the same name.</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Parameter<T>(Expression<Func<T, object>> property, string suffix = null) => Parameter(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, suffix);
        /// <summary>
        /// Adds a sql parameter expression where the parameter name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="suffix">Optional suffix to appends after the name of the parameter. Useful when using multiple entities in the same query or when columns share the same name.</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Parameter(Expression<Func<TEntity, object>> property, string suffix = null) => Parameter<TEntity>(property, suffix);
        #endregion

        #region Variable
        /// <summary>
        /// Adds a sql variable expression.
        /// </summary>
        /// <param name="variable">The name of the sql variable</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Variable(string variable) => Expression(new VariableExpression(variable.ValidateArgumentNotNullOrWhitespace(nameof(variable))));
        /// <summary>
        /// Adds a sql variable expression where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Variable<T>(Expression<Func<T, object>> property) => Variable(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Adds a sql variable expression where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Variable(Expression<Func<TEntity, object>> property) => Variable<TEntity>(property);
        #endregion

        #region VariableAssignment
        /// <summary>
        /// Assigns a new value to a SQL variable.
        /// </summary>
        /// <param name="variable">The name of the sql variable</param>
        /// <param name="constant">The constant value to assign to the variable</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn AssignVariable(string variable, object constant) => AssignVariable(variable, e => e.Value(constant));
        /// <summary>
        /// Assigns a new value to a SQL variable.
        /// </summary>
        /// <typeparam name="T">The main entity for build the query for</typeparam>
        /// <param name="variable">The name of the sql variable</param>
        /// <param name="builder">Builder to select the value to assign</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn AssignVariable<T>(string variable, Action<ISharedExpressionBuilder<T, Null>> builder) => Expression(new VariableInlineAssignmentExpression(new VariableExpression(variable.ValidateArgumentNotNullOrWhitespace(nameof(variable))), new ExpressionBuilder<T>(builder)));
        /// <summary>
        /// Assigns a new value to a SQL variable.
        /// </summary>
        /// <param name="variable">The name of the sql variable</param>
        /// <param name="builder">Builder to select the value to assign</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn AssignVariable(string variable, Action<ISharedExpressionBuilder<TEntity, Null>> builder) => AssignVariable<TEntity>(variable, builder);
        /// <summary>
        /// Assigns a new value to a SQL variable where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The main entity for build the query for</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        ///  <param name="builder">Builder to select the value to assign</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn AssignVariable<T>(Expression<Func<T, object>> property, Action<ISharedExpressionBuilder<T, Null>> builder) => AssignVariable(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, builder);
        /// <summary>
        /// Assigns a new value to a SQL variable where the variable name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <param name="builder">Builder to select the value to assign</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn AssignVariable(Expression<Func<TEntity, object>> property, Action<ISharedExpressionBuilder<TEntity, Null>> builder) => AssignVariable<TEntity>(property, builder);
        #endregion

        #region Functions
        #region Count
        /// <summary>
        /// Counts the total amount of rows returned.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn CountAll(object dataset = null) => Expression(new ColumnFunctionExpression(Functions.Count, new ColumnExpression(dataset, Sql.All.ToString())));
        /// <summary>
        /// Counts the total amount of rows where <paramref name="column"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to count</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Count(object dataset, string column) => Expression(new ColumnFunctionExpression(Functions.Count, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)))));
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="T"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Count<T>(object dataset, Expression<Func<T, object>> property) => Count(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is not null.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Count(object dataset, Expression<Func<TEntity, object>> property) => Count<TEntity>(dataset, property);
        /// <summary>
        /// Counts the total amount of rows where <paramref name="column"/> is not null.
        /// </summary>
        /// <param name="column">The column to count</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Count(string column) => Count(null, column);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="T"/> is not null.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Count<T>(Expression<Func<T, object>> property) => Count<T>(typeof(T), property);
        /// <summary>
        /// Counts the total amount of rows where column selected by <paramref name="property"/> from <typeparamref name="TEntity"/> is not null.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Count(Expression<Func<TEntity, object>> property) => Count<TEntity>(property);

        #endregion
        #region Avg
        /// <summary>
        /// Calculates the average of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the average from</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Average(object dataset, string column) => Expression(new ColumnFunctionExpression(Functions.Avg, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)))));
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Average<T>(object dataset, Expression<Func<T, object>> property) => Average(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Average(object dataset, Expression<Func<TEntity, object>> property) => Average<TEntity>(dataset, property);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Average(Expression<Func<TEntity, object>> property) => Average<TEntity>(property);
        /// <summary>
        /// Calculates the average of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the average from</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Average(string column) => Average(null, column);
        /// <summary>
        ///  Calculates the average of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Average<T>(Expression<Func<T, object>> property) => Average<T>(typeof(T), property);
        #endregion
        #region Sum
        /// <summary>
        /// Calculates the sum of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the average from</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Sum(object dataset, string column) => Expression(new ColumnFunctionExpression(Functions.Sum, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)))));
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Sum<T>(object dataset, Expression<Func<T, object>> property) => Sum(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Sum(object dataset, Expression<Func<TEntity, object>> property) => Sum<TEntity>(dataset, property);
        /// <summary>
        /// Calculates the sum of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the average from</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Sum(string column) => Sum(null, column);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Sum<T>(Expression<Func<T, object>> property) => Sum<T>(typeof(T), property);
        /// <summary>
        ///  Calculates the sum of the column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Sum(Expression<Func<TEntity, object>> property) => Sum<TEntity>(property);
        #endregion
        #region Max
        /// <summary>
        /// Returns the largest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the max from</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Max(object dataset, string column) => Expression(new ColumnFunctionExpression(Functions.Max, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)))));
        /// <summary>
        /// Returns the largest value of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Max<T>(object dataset, Expression<Func<T, object>> property) => Max(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Max(object dataset, Expression<Func<TEntity, object>> property) => Max<TEntity>(dataset, property);
        /// <summary>
        /// Returns the largest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the max from</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Max(string column) => Max(null, column);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Max<T>(Expression<Func<T, object>> property) => Max<T>(typeof(T), property);
        /// <summary>
        /// Returns the largest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Max(Expression<Func<TEntity, object>> property) => Max<TEntity>(property);
        #endregion
        #region Min
        /// <summary>
        /// Returns the smallest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="column">The column to get the max from</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Min(object dataset, string column) => Expression(new ColumnFunctionExpression(Functions.Min, new ColumnExpression(dataset, column.ValidateArgumentNotNullOrWhitespace(nameof(column)))));
        /// <summary>
        /// Returns the smallest value of the column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Min<T>(object dataset, Expression<Func<T, object>> property) => Min(dataset, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="dataset">Optional dataset alias to select column from</param>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Min(object dataset, Expression<Func<TEntity, object>> property) => Min<TEntity>(dataset, property);
        /// <summary>
        /// Returns the smallest value of <paramref name="column"/>.
        /// </summary>
        /// <param name="column">The column to get the max from</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Min(string column) => Min(null, column);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Min<T>(Expression<Func<T, object>> property) => Min<T>(typeof(T), property);
        /// <summary>
        /// Returns the smallest value of column selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Min(Expression<Func<TEntity, object>> property) => Min<TEntity>(property);
        #endregion
        #endregion

        #region Query
        /// <summary>
        /// Adds a sub query expression.
        /// </summary>
        /// <param name="query">Delegate that adds the query to the supplied builder</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Query(Action<StringBuilder, ExpressionCompileOptions> query) => Expression(new SubQueryExpression(null, query.ValidateArgument(nameof(query))));
        /// <summary>
        /// Adds a sub query expression.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Query(string query) => Query((b, o) => b.Append(query.ValidateArgumentNotNullOrWhitespace(nameof(query))));
        /// <summary>
        /// Adds a sub query expression.
        /// </summary>
        /// <param name="builder">Builder for creating the sub query</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Query(IQueryBuilder builder) => Expression(new SubQueryExpression(null, builder.ValidateArgument(nameof(builder))));
        #endregion

        #region Case
        /// <summary>
        /// Adds a case expression.
        /// </summary>
        /// <param name="caseBuilder">Delegate that configures the case expression</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Case(Action<ICaseExpressionRootBuilder<TEntity>> caseBuilder) => Case<TEntity>(caseBuilder);
        /// <summary>
        /// Adds a case expression.
        /// </summary>
        /// <typeparam name="T">The main type to create the case expression with</typeparam>
        /// <param name="caseBuilder">Delegate that configures the case expression</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn Case<T>(Action<ICaseExpressionRootBuilder<T>> caseBuilder) => Expression(new WrappedExpression(new CaseExpression<T>(caseBuilder)));
        #endregion

        #region Date
        /// <summary>
        /// Adds an expression that returns the current date.
        /// </summary>
        /// <param name="type">Determines which date is returned</param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn CurrentDate(DateType type = DateType.Utc) => Expression(new CurrentDateExpression(type));

        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <param name="date">String that contains the date to modify</param>
        /// <param name="amount">The amount to add/substract to/from the date</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate(string date, double amount, DateInterval interval = DateInterval.Millisecond) => Expression(new ModifyDateExpression(new SqlConstantExpression(date.ValidateArgumentNotNullOrWhitespace(nameof(date))), new SqlConstantExpression(amount), interval));
        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <param name="date">The date to modify</param>
        /// <param name="amount">The amount to add/substract to/from the date</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate(DateTimeOffset date, double amount, DateInterval interval = DateInterval.Millisecond) => Expression(new ModifyDateExpression(new SqlConstantExpression(date), new SqlConstantExpression(amount), interval));
        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <typeparam name="T">The main entity to build the expression for</typeparam>
        /// <param name="dateExpressionBuilder">Delegate that selects the date expression to modify</param>
        /// <param name="amount">The amount to add/substract to/from the date</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate<T>(Action<ISharedExpressionBuilder<T, Null>> dateExpressionBuilder, double amount, DateInterval interval = DateInterval.Millisecond) => Expression(new ModifyDateExpression(new ExpressionBuilder<T>(dateExpressionBuilder.ValidateArgument(nameof(dateExpressionBuilder))), new SqlConstantExpression(amount), interval));
        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <param name="dateExpressionBuilder">Delegate that selects the date expression to modify</param>
        /// <param name="amount">The amount to add/substract to/from the date</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate(Action<ISharedExpressionBuilder<TEntity, Null>> dateExpressionBuilder, double amount, DateInterval interval = DateInterval.Millisecond) => ModifyDate<TEntity>(dateExpressionBuilder, amount, interval);
        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <typeparam name="T">The main entity to build the expression for</typeparam>
        /// <param name="dateExpressionBuilder">Delegate that selects the date expression to modify</param>
        /// <param name="amountExpressionBuilder">Delegate that selects the value expression</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate<T>(Action<ISharedExpressionBuilder<T, Null>> dateExpressionBuilder, Action<ISharedExpressionBuilder<T, Null>> amountExpressionBuilder, DateInterval interval = DateInterval.Millisecond) => Expression(new ModifyDateExpression(new ExpressionBuilder<T>(dateExpressionBuilder.ValidateArgument(nameof(dateExpressionBuilder))), new ExpressionBuilder<T>(amountExpressionBuilder.ValidateArgument(nameof(amountExpressionBuilder))), interval));
        /// <summary>
        /// Adds an expression where a date is modified.
        /// </summary>
        /// <param name="dateExpressionBuilder">Delegate that selects the date expression to modify</param>
        /// <param name="amountExpressionBuilder">Delegate that selects the value expression</param>
        /// <param name="interval"><inheritdoc cref="DateInterval"/></param>
        /// <returns>Builder for creating more expressions</returns>
        TReturn ModifyDate(Action<ISharedExpressionBuilder<TEntity, Null>> dateExpressionBuilder, Action<ISharedExpressionBuilder<TEntity, Null>> amountExpressionBuilder, DateInterval interval = DateInterval.Millisecond) => ModifyDate<TEntity>(dateExpressionBuilder, amountExpressionBuilder, interval);
        #endregion
    }
}
