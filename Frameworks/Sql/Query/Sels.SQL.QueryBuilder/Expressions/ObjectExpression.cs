using Sels.Core.Extensions;
using System;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents an sql object (e.g. Tables, columns, schema's, ...)
    /// </summary>
    public class ObjectExpression : BaseObjectExpression
    {
        /// <inheritdoc cref="ObjectExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.Set"/></param>
        /// <param name="objectName"><inheritdoc cref="IObjectExpression.Object"/></param>
        public ObjectExpression(object dataset, string objectName) : base(dataset, objectName)
        {
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string> datasetConverterer, Func<string, string> objectConverter, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            builder.ValidateArgument(nameof(datasetConverterer));

            var dataSet = Set != null ? datasetConverterer(Set) : null;
            var objectName = (objectConverter != null ? objectConverter(Object) : Object) ?? throw new InvalidOperationException($"{nameof(datasetConverterer)} returned null as the object name");

            if (dataSet.HasValue()) builder.Append(dataSet).Append('.');
            builder.Append(objectName);
        }
    }
}
