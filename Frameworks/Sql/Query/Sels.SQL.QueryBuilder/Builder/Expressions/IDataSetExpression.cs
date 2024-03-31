using System;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that contains a dataset object that can be translated to a sql dataset alias.
    /// </summary>
    public interface IDataSetExpression : IExpression
    {
        // Properties
        /// <summary>
        /// Object containing a dataset name.
        /// </summary>
        public object Set { get; }

        /// <summary>
        /// Converts the current expression to sql.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="datasetConverterer">Delegate for converting dataset to sql</param>
        /// <param name="options">Optional settings for building the query</param>
        public void ToSql(StringBuilder builder, Func<object, string> datasetConverterer, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
