using Sels.Core.Extensions;
using System;
using System.Data;
using System.Text;
using Sels.Core.Extensions.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a table to perform a query on.
    /// </summary>
    public class TableExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// Optional alias that can be added to reference the table.
        /// </summary>
        public DataSetExpression Alias { get; private set; }
        /// <summary>
        /// Contains the database where the table is located in.
        /// </summary>
        public ObjectExpression Database { get; private set; }
        /// <summary>
        /// Contains the schema the table is located in.
        /// </summary>
        public ObjectExpression Schema { get; private set; }
        /// <summary>
        /// Contains the table name.
        /// </summary>
        public ObjectExpression Table { get; private set; }

        /// <inheritdoc cref="TableExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.Set"/></param>
        /// <param name="database"><inheritdoc cref="Database"/></param>
        /// <param name="schema"><inheritdoc cref="Schema"/></param>
        /// <param name="table"><inheritdoc cref="Table"/></param>
        public TableExpression(string database, string schema, string table, object dataset)
        {
            SetTableName(table.ValidateArgumentNotNullOrWhitespace(nameof(table)));
            SetAlias(dataset);
            SetSchema(schema);
            SetDatabase(database);
        }

        /// <summary>
        /// Sets the table name to <paramref name="tablename"/>.
        /// </summary>
        /// <param name="tablename">The table name to set</param>
        /// <returns>Current expression for method chaining</returns>
        public TableExpression SetTableName(string tablename)
        {
            Table = new ObjectExpression(null, tablename.ValidateArgumentNotNullOrWhitespace(nameof(tablename)));
            return this;
        }

        /// <summary>
        /// Sets the alias to <paramref name="alias"/>.
        /// </summary>
        /// <param name="alias">The alias to set, can be null</param>
        /// <returns>Current expression for method chaining</returns>
        public TableExpression SetAlias(object alias)
        {
            Alias = alias != null ? new DataSetExpression(alias) : null;
            return this;
        }

        /// <summary>
        /// Sets the schema to <paramref name="schema"/>.
        /// </summary>
        /// <param name="schema">The schema to set, can be null</param>
        /// <returns>Current expression for method chaining</returns>
        public TableExpression SetSchema(string schema)
        {
            Schema = schema != null ? new ObjectExpression(null, schema) : null;
            return this;
        }

        /// <summary>
        /// Sets the database to <paramref name="database"/>.
        /// </summary>
        /// <param name="database">The database to set, can be null</param>
        /// <returns>Current expression for method chaining</returns>
        public TableExpression SetDatabase(string database)
        {
            Database = database != null ? new ObjectExpression(null, database) : null;
            return this;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if(Database != null)
            {
                subBuilder(builder, Database);
                builder.Append('.');
            }

            if (Schema != null)
            {
                subBuilder(builder, Schema);
                builder.Append('.');
            }

            subBuilder(builder, Table);

            if(Alias != null)
            {
                builder.AppendSpace();
                subBuilder(builder, Alias);
            }
        }
    }
}
