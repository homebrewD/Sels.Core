using Sels.Core.Extensions;
using System;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents the syntax for selecting everything (e.g. *) from a dataset.
    /// </summary>
    public class AllColumnsExpression : BaseExpression, IDataSetExpression
    {
        /// <summary>
        /// Object containing the dataset to select everything from.
        /// </summary>
        public object Set { get; }

        /// <inheritdoc cref="AllColumnsExpression"/>
        /// <param name="dataset"><inheritdoc cref="Set"/></param>
        public AllColumnsExpression(object dataset = null)
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
        public void ToSql(StringBuilder builder, Func<object, string> datasetConverterer, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var dataset = Set != null ? datasetConverterer(Set) : null;

            if (dataset.HasValue()) builder.Append(dataset).Append('.');
            builder.Append(Sql.All);
        }
    }
}
