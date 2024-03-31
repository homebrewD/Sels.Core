using System;
using System.Text;
using Sels.Core.Attributes.Enumeration.Value;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a sql function on a column.
    /// </summary>
    public class ColumnFunctionExpression : BaseObjectExpression, IColumnExpression
    {
        /// <summary>
        /// The sql function this expression represents.
        /// </summary>
        public Functions Function { get;}
        /// <summary>
        /// Expression that contains the column to perform the function on.
        /// </summary>
        public IColumnExpression Expression { get; set; }

        /// <inheritdoc cref="ColumnFunctionExpression"/>
        /// <param name="function"><inheritdoc cref="Function"/></param>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        public ColumnFunctionExpression(Functions function, IColumnExpression expression) : base(expression.Set, expression.Object)
        {
            Expression = expression.ValidateArgument(nameof(expression));
            Function = function;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string> datasetConverterer, Func<string, string> columnConverter = null, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var function = Function.GetStringValue();

            builder.Append(function).Append('(');

            Expression.ToSql(builder, datasetConverterer, columnConverter);

            builder.Append(')');
        }
    }
}
