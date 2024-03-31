using Sels.Core.Extensions;
using System;
using System.Text;
using Sels.Core.Extensions.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a sub query.
    /// </summary>
    public class SubQueryExpression : BaseDataSetExpression
    {
        // Fields
        private readonly Action<StringBuilder, ExpressionCompileOptions> _subQueryBuilder;
        private readonly bool _wrap;
        private readonly bool _canSeparatorBeAppended;

        /// <inheritdoc cref="SubQueryExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.Set"/></param>
        /// <param name="subQueryBuilder">Delegate that adds the query to the supplied builder</param>
        /// <param name="wrap">If the query needs to be wrapped with ()</param>
        /// <param name="canSeparatorBeAppended">If separator can be appended to the sub query. In most cases it is not allowed</param>
        public SubQueryExpression(object dataset, Action<StringBuilder, ExpressionCompileOptions> subQueryBuilder, bool wrap = true, bool canSeparatorBeAppended = false) : base(dataset)
        {
            _subQueryBuilder = subQueryBuilder.ValidateArgument(nameof(subQueryBuilder));
            _wrap = wrap;
            _canSeparatorBeAppended = canSeparatorBeAppended;
        }
        /// <inheritdoc cref="SubQueryExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.Set"/></param>
        /// <param name="queryBuilder">Builder that creates the query string</param>
        /// <param name="wrap">If the query needs to be wrapped with ()</param>
        /// <param name="canSeparatorBeAppended">If separator can be appended to the sub query. In most cases it is not allowed</param>
        public SubQueryExpression(object dataset, IQueryBuilder queryBuilder, bool wrap = true, bool canSeparatorBeAppended = false) : this(dataset, (b, o) => queryBuilder.Build(b, o), wrap, canSeparatorBeAppended)
        {
            queryBuilder.ValidateArgument(nameof(queryBuilder));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string> datasetConverterer, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var dataset = Set != null ? datasetConverterer(Set) : null;

            if (_wrap) builder.Append('(');
            if (!_canSeparatorBeAppended) options &= ~ExpressionCompileOptions.AppendSeparator;
            _subQueryBuilder(builder, options);
            if (_wrap) builder.Append(')');
            if (dataset.HasValue()) builder.AppendSpace().Append(dataset);
        }
    }
}
