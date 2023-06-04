﻿using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sels.Core.Extensions.Linq;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions
{
    /// <summary>
    /// Expression that represents the MySql CONCAT function for joining strings.
    /// </summary>
    public class ConcatExpression : BaseExpressionContainer
    {
        // Constants
        /// <summary>
        /// The name of the MySql Concat function.
        /// </summary>
        public const string Function = "CONCAT";

        // Properties
        /// <summary>
        /// The expressions to supply to the concat function.
        /// </summary>
        public IExpression[] Expressions { get; }

        /// <inheritdoc cref=" ConcatExpression"/>
        /// <param name="expressions"><inheritdoc cref="Expressions"/></param>
        public ConcatExpression(IEnumerable<IExpression> expressions)
        {
            expressions.ValidateArgumentNotNullOrEmpty(nameof(expressions));
            expressions.GetCount().ValidateArgumentLarger($"{nameof(expressions)}.Count()", 1);

            Expressions = expressions.ToArray();
        }
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            builder.Append(Function).Append('(');
            Expressions.Execute((i, e) =>
            {
                subBuilder(builder, e);
                if(i < Expressions.Length-1) builder.Append(',');
            });
            builder.Append(')');
        }
    }
}
