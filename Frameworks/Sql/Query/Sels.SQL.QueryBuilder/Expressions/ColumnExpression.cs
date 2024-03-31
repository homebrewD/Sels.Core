using Sels.Core.Extensions;
using System;
using System.Text;
using Sels.Core.Extensions.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents an sql column.
    /// </summary>
    public class ColumnExpression : ObjectExpression, IColumnExpression
    {
        /// <inheritdoc cref="ColumnExpression"/>
        /// <param name="dataSet"><inheritdoc cref="IDataSetExpression.Set"/></param>
        /// <param name="column"><inheritdoc cref="IObjectExpression.Object"/></param>
        public ColumnExpression(object dataSet, string column) : base(dataSet, column)
        {
        }
    }
}
