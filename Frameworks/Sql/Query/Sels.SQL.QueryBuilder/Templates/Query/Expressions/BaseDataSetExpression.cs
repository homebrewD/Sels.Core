using System;
using System.Text;
using Sels.Core.Extensions;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Template for creating a <see cref="IDataSetExpression"/>.
    /// </summary>
    public abstract class BaseDataSetExpression : BaseExpression, IDataSetExpression
    {
        /// <inheritdoc/>
        public object Set { get; }

        ///<inheritdoc cref="BaseDataSetExpression"/>
        /// <param name="dataset"><inheritdoc cref="Set"/></param>
        public BaseDataSetExpression(object dataset)
        {
            Set = dataset;
        }
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            ToSql(builder, x => x.ToString(), options);
        }

        /// <inheritdoc/>
        public abstract void ToSql(StringBuilder builder, Func<object, string> datasetConverterer, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
