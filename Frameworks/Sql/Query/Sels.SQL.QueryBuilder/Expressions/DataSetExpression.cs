using Sels.Core.Extensions;
using System;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents an alias given to a dataset (e.g. Tables, columns, sub queries, common table expressions, ...)
    /// </summary>
    public class DataSetExpression : BaseDataSetExpression
    {
        /// <inheritdoc cref="DataSetExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.Set"/></param>
        public DataSetExpression(object dataset) : base(dataset)
        {
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string> datasetConverterer, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var dataSet = Set != null ? datasetConverterer(Set) : null;

            if (dataSet != null) builder.Append(dataSet);
        }
    }
}
