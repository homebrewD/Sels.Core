using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.SQL.QueryBuilder.Builder;
using System.Text;
using System;
using Sels.Core.Extensions.Text;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.Core.Models;
using Sels.SQL.QueryBuilder.Expressions;

namespace Sels.SQL.QueryBuilder
{
    /// <summary>
    /// Contains sql related constant values and helper methods.
    /// </summary>
    public static class Sql
    {
        #region Constants
        /// <summary>
        /// Contains the sql statements.
        /// </summary>
        public static class Statements
        {
            /// <summary>
            /// The sql statement for creating data.
            /// </summary>
            public const string Insert = "INSERT";
            /// <summary>
            /// The sql statement for reading data.
            /// </summary>
            public const string Select = "SELECT";
            /// <summary>
            /// The sql statement for updating data.
            /// </summary>
            public const string Update = "UPDATE";
            /// <summary>
            /// The sql statement for deleting data.
            /// </summary>
            public const string Delete = "DELETE";
            /// <summary>
            /// The sql statement for defining a condition that must be evaluated to true before executing other statements.
            /// </summary>
            public const string If = "IF";
            /// <summary>
            /// The sql statement for defining additional conditions after <see cref="If"/>.
            /// </summary>
            public const string ElseIf = "ELSE IF";
            /// <summary>
            /// The sql satement for defining the statements to execute when the previous <see cref="If"/> and <see cref="ElseIf"/> statements evaluated to false.
            /// </summary>
            public const string Else = "ELSE";
        }

        /// <summary>
        /// Contains the sql clauses.
        /// </summary>
        public static class Clauses
        {
            /// <summary>
            /// The sql clause for defining the columns to update to new values.
            /// </summary>
            public const string Set = "SET";
            /// <summary>
            /// Sql clause for defining what to insert into.
            /// </summary>
            public const string Into = "INTO";
            /// <summary>
            /// The sql clause for defining the values to insert.
            /// </summary>
            public const string Values = "VALUES";
            /// <summary>
            /// The sql clause for defining where to select from.
            /// </summary>
            public const string From = "FROM";
            /// <summary>
            /// The sql clause for defining conditions.
            /// </summary>
            public const string Where = "WHERE";
            /// <summary>
            /// The sql clause for defining how to order the query results.
            /// </summary>
            public const string OrderBy = "ORDER BY";
            /// <summary>
            /// The sql clause for defining conditions on aggregated results.
            /// </summary>
            public const string Having = "HAVING";
            /// <summary>
            /// The sql clause for partitioning an aggregated value.
            /// </summary>
            public const string Over = "OVER";
            /// <summary>
            /// The sql clause for defining how to group the query results.
            /// </summary>
            public const string GroupBy = "GROUP BY";
            /// <summary>
            /// The sql clause for concatenating the results of 2 select queries. Duplicate rows are excluded.
            /// </summary>
            public const string Union = "UNION";
            /// <summary>
            /// The sql clause for concatenating the results of 2 select queries. Duplicate rows are included.
            /// </summary>
            public const string UnionAll = "UNION ALL";
        }

        /// <summary>
        /// Contains SQL keywords related to the OVER clause.
        /// </summary>
        public static class Over
        {
            /// <summary>
            /// The keyword used to define the partitions for a window function.
            /// </summary>
            public const string PartitionBy = "PARTITION BY";
            /// <summary>
            /// The keyword used to define the lower and upper bound of a window frame.
            /// </summary>
            public const string Between = "BETWEEN";
            /// <summary>
            /// The ROWS clause for limiting a window frame.
            /// </summary>
            public const string Rows = "ROWS";
            /// <summary>
            /// The ROWS clause for limiting a window frame.
            /// </summary>
            public const string Range = "RANGE";
            /// <summary>
            /// The ROWS clause for limiting a window frame.
            /// </summary>
            public const string Groups = "GROUPS";
            /// <summary>
            /// The keyword used to set either the lower or upper bound of a window frame border to the current row.
            /// </summary>
            public const string CurrentRow = "CURRENT ROW";
            /// <summary>
            /// The keyword used when there's no limit on the lower or upper bound of a window frame border.
            /// </summary>
            public const string Unbounded = "UNBOUNDED";
            /// <summary>
            /// The keyword used to limit the lower bound of a window frame border.
            /// </summary>
            public const string Preceding = "PRECEDING";
            /// <summary>
            /// The keyword used to limit the upper bound of a window frame border.
            /// </summary>
            public const string Following = "FOLLOWING";
        }

        /// <summary>
        /// Contains the keywords for joins.
        /// </summary>
        public static class Joins
        {
            /// <summary>
            /// The keyword for an inner join.
            /// </summary>
            public const string InnerJoin = "INNER JOIN";
            /// <summary>
            /// The keyword for a full (outer) join.
            /// </summary>
            public const string FullJoin = "FULL JOIN";
            /// <summary>
            /// The keyword for a left (outer) join.
            /// </summary>
            public const string LeftJoin = "LEFT JOIN";
            /// <summary>
            /// The keyword for a right (outer) join.
            /// </summary>
            public const string RightJoin = "RIGHT JOIN";
        }
        /// <summary>
        /// Contains the sql function names.
        /// </summary>
        public static class Functions
        {
            /// <summary>
            /// The name of the max function.
            /// </summary>
            public const string Max = "MAX";
            /// <summary>
            /// The name of the min function.
            /// </summary>
            public const string Min = "MIN";
            /// <summary>
            /// The name of the avg function.
            /// </summary>
            public const string Avg = "AVG";
            /// <summary>
            /// The name of the sum function.
            /// </summary>
            public const string Sum = "SUM";
            /// <summary>
            /// The name of the count function.
            /// </summary>
            public const string Count = "COUNT";
        }
        /// <summary>
        /// Contains the sql window function names.
        /// </summary>
        public static class WindowFunctions
        {
            /// <summary>
            /// The name of the ROW_NUMBER function.
            /// </summary>
            public const string RowNumber = "ROW_NUMBER";
            /// <summary>
            /// The name of the DENSE function.
            /// </summary>
            public const string Dense = "DENSE";
            /// <summary>
            /// The name of the DENSE_RANK function.
            /// </summary>
            public const string DenseRank = "DENSE_RANK";
            /// <summary>
            /// The name of the NTILE function.
            /// </summary>
            public const string Ntile = "NTILE";
            /// <summary>
            /// The name of the LAG function.
            /// </summary>
            public const string Lag = "LAG";
            /// <summary>
            /// The name of the LEAD function.
            /// </summary>
            public const string Lead = "LEAD";
        }
        /// <summary>
        /// Contains the sql sort orders.
        /// </summary>
        public static class SortOrders
        {
            /// <summary>
            /// Sorts from smallest to largest.
            /// </summary>
            public const string Asc = "ASC";
            /// <summary>
            /// Sorts from largest to smallest.
            /// </summary>
            public const string Desc = "DESC";
        }
        /// <summary>
        /// Contains the sql operators for comparing values.
        /// </summary>
        public static class ConditionOperators
        {
            /// <summary>
            /// Expressions should be equal.
            /// </summary>
            public const string Equal = "=";
            /// <summary>
            /// Expressions should not be equal.
            /// </summary>
            public const string NotEqual = "!=";
            /// <summary>
            /// Expression should be greater than other expression.
            /// </summary>
            public const string Greater = ">";
            /// <summary>
            /// Expression should be lesser than other expression.
            /// </summary>
            public const string Less = "<";
            /// <summary>
            /// Expression should be greater or equal to other expression.
            /// </summary>
            public const string GreaterOrEqual = ">=";
            /// <summary>
            /// Expression should be lesser or equal to other expression.
            /// </summary>
            public const string LessOrEqual = "<=";
            /// <summary>
            /// Expression should be in list of values.
            /// </summary>
            public const string In = "IN";
            /// <summary>
            /// Expression should not be in list of values.
            /// </summary>
            public const string NotIn = Not + Constants.Strings.Space + In;
            /// <summary>
            /// Expression should be like a pattern.
            /// </summary>
            public const string Like = "LIKE";
            /// <summary>
            /// Expression should not be like a pattern.
            /// </summary>
            public const string NotLike = Not + Constants.Strings.Space + Like;
            /// <summary>
            /// Expression should exist in another expression.
            /// </summary>
            public const string Exists = "EXISTS";
            /// <summary>
            /// Expression should not exist in another expression.
            /// </summary>
            public const string NotExists = Not + Constants.Strings.Space + Exists;
            /// <summary>
            /// Expression should be between 2 values.
            /// </summary>
            public const string Between = "BETWEEN";
            /// <summary>
            /// Expression should not be between 2 values.
            /// </summary>
            public const string NotBetween = Not + Constants.Strings.Space + Between;
            /// <summary>
            /// Expression should be equal to a contant value.
            /// </summary>
            public const string Is = "IS";
            /// <summary>
            /// Expression should not be equal to a contant value.
            /// </summary>
            public const string IsNot = Is + Constants.Strings.Space + Not;
        }
        /// <summary>
        /// Contains the sql logic operators for comparing conditions.
        /// </summary>
        public static class LogicOperators
        {
            /// <summary>
            /// Current and next condition both need to be true.
            /// </summary>
            public const string And = "AND";
            /// <summary>
            /// Current and next condition either need to be true.
            /// </summary>
            public const string Or = "OR";
        }

        /// <summary>
        /// The sql value for selecting everything from a dataset.
        /// </summary>
        public const char All = '*';
        /// <summary>
        /// The sql keyword for defining an alias.
        /// </summary>
        public const string As = "AS";
        /// <summary>
        /// Sql keyword for defining cte's or sql locks.
        /// </summary>
        public const string With = "WITH";
        /// <summary>
        /// The sql keyword for defining what columns to join on.
        /// </summary>
        public const string On = "ON";
        /// <summary>
        /// The sql keyword for null.
        /// </summary>
        public const string Null = "NULL";
        /// <summary>
        /// The sql keyword for inverting a condition result.
        /// </summary>
        public const string Not = "NOT";
        /// <summary>
        /// Expression for starting a Sql CASE expression.
        /// </summary>
        public const string Case = "CASE";
        /// <summary>
        /// Expression for defining a condition in a CASE expression.
        /// </summary>
        public const string When = "WHEN";
        /// <summary>
        /// Expression for defining the value to return after a CASE WHEN expression.
        /// </summary>
        public const string Then = "THEN";
        /// <summary>
        /// Expression for defining the value to return when the other conditions fail in a CASE expression.
        /// </summary>
        public const string Else = "ELSE";
        /// <summary>
        /// Expression to close a statement (e.g. IF, CASE, ...)
        /// </summary>
        public const string End = "END";
        /// <summary>
        /// The prefix to place in front of parameter names.
        /// </summary>
        public const char ParameterPrefix = '@';
        /// <summary>
        /// The default prefix for defining variables.
        /// </summary>
        public const char VariablePrefix = '@';
        /// <summary>
        /// The default operator used to assign object a new value in SQL.
        /// </summary>
        public const char AssignmentOperator = '=';
        /// <summary>
        /// The command used to set something to a new value.
        /// </summary>
        public const string Set = "SET";
        /// <summary>
        /// The separator used to separate sql statements.
        /// </summary>
        public const char Separator = ';';
        #endregion

        #region Helpers
        /// <summary>
        /// Joins the query builder results from any <see cref="IQueryBuilder"/> in <paramref name="builders"/> into a single string.
        /// </summary>
        /// <param name="options">The settings for generating the queries</param>
        /// <param name="builders">Object containing either <see cref="IQueryBuilder"/> that is used to generate queries or object who's <see cref="object.ToString()"/> value will be added to the string</param>
        /// <returns>String created from <paramref name="builders"/></returns>
        public static string Join(ExpressionCompileOptions options, params object[] builders)
        {
            builders.ValidateArgument(nameof(builders));

            var stringBuilder = new StringBuilder();
            var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

            builders.Execute((i, x) =>
            {
                if (x is IQueryBuilder builder) builder.Build(stringBuilder, options); else stringBuilder.Append(x);

                if (i < builders.Length - 1) if (isFormatted) stringBuilder.AppendLine(); else stringBuilder.AppendSpace();
            });

            return stringBuilder.ToString();
        }
        #endregion

        #region Expression
        /// <summary>
        /// Contains helper methods for creating common sql expression.
        /// </summary>
        public static class Expressions
        {
            /// <inheritdoc cref="RawExpression"/>
            /// <param name="sql">Object that will be converted to sql using <see cref="object.ToString"/></param>
            /// <returns>A new instance of <see cref="RawExpression"/></returns>
            public static RawExpression Raw(object sql) => new RawExpression(sql.ValidateArgument(nameof(sql)));
            /// <inheritdoc cref="ConstantExpression"/>
            /// <param name="value"><inheritdoc cref="ConstantExpression.Value"/></param>
            /// <returns>A new instance of <see cref="ConstantExpression"/></returns>
            public static ConstantExpression Value(object value) => new ConstantExpression(value);
            /// <inheritdoc cref="ParameterExpression"/>
            /// <param name="name">The name of the sql parameter</param>
            /// <param name="index">Optional suffix to appends after the name of the parameter. Useful when using multiple entities in the same query or when columns share the same name.</param>
            /// <returns>A new instance of <see cref="ParameterExpression"/></returns>
            public static ParameterExpression Parameter(string name, string index = null) => new ParameterExpression(name.ValidateArgument(nameof(name)), index);
            /// <summary>
            /// Creates a new expression created using <paramref name="builder"/>.
            /// </summary>
            /// <typeparam name="T">The main entity to create the expression for</typeparam>
            /// <param name="builder">The delegate that creates the expression to return</param>
            /// <returns>The expression created by <paramref name="builder"/></returns>
            public static IExpression FromBuilder<T>(Action<ISharedExpressionBuilder<T, Null>> builder) => new ExpressionBuilder<T>(builder.ValidateArgument(nameof(builder))).Expression;
            /// <summary>
            /// Creates a new expression created using <paramref name="builder"/>.
            /// </summary>
            /// <param name="builder">The delegate that creates the expression to return</param>
            /// <returns>The expression created by <paramref name="builder"/></returns>
            public static IExpression FromBuilder(Action<ISharedExpressionBuilder<Null, Null>> builder) => FromBuilder<Null>(builder);
        }
        #endregion
    }
}
