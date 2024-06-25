using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlParameterExpression = Sels.SQL.QueryBuilder.Builder.Expressions.ParameterExpression;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builds an insert query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class InsertStatementBuilder<TEntity> : BaseStatementBuilder<TEntity, InsertExpressionPositions, IInsertStatementBuilder<TEntity>>, IInsertStatementBuilder<TEntity>
    {
        /// <inheritdoc cref="InsertStatementBuilder{TEntity}"/>
        /// <param name="compiler">>Compiler to create the query using the expressions defined in the current builder</param>
        public InsertStatementBuilder(IQueryCompiler<InsertExpressionPositions> compiler) : base(compiler)
        {
        }

        /// <inheritdoc cref="InsertStatementBuilder{TEntity}"/>
        /// <param name="other">The builder to copy settings from</param>
        public InsertStatementBuilder(InsertStatementBuilder<TEntity> other) : base(other)
        {
        }

        #region Base Builder
        /// <inheritdoc/>
        public override IInsertStatementBuilder<TEntity> Instance => this;
        /// <inheritdoc/>
        public override IInsertStatementBuilder<TEntity> Clone()
        {
            return new InsertStatementBuilder<TEntity>(this);
        }

        /// <inheritdoc/>
        protected override InsertExpressionPositions GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression)
        {
            throw new NotSupportedException("Where clause is not supported for an insert query");
        }
        /// <inheritdoc/>
        protected override InsertExpressionPositions GetPositionForJoinExpression(JoinExpression<TEntity, IInsertStatementBuilder<TEntity>> joinExpression)
        {
            throw new NotSupportedException("Join clause is not supported for an insert query");
        }
        #endregion

        #region Builder
        /// <inheritdoc/>
        public IInsertStatementBuilder<TEntity> Columns(IEnumerable<string> columns)
        {
            columns.ValidateArgumentNotNullOrEmpty(nameof(columns));

            var builder = this.CastTo<IInsertStatementBuilder<TEntity>>();

            foreach (var column in columns)
            {
                builder.Column(column);
            }

            return this;
        }
        /// <inheritdoc/>
        public IInsertStatementBuilder<TEntity> ColumnsOf<T>(params string[] excludedProperties)
        {
            return Columns(GetColumnPropertiesFrom<T>(excludedProperties).Select(x => x.Name));
        }
        /// <inheritdoc/>
        public IInsertStatementBuilder<TEntity> Values(IEnumerable<object> values)
        {
            values.ValidateArgumentNotNullOrEmpty(nameof(values));

            return Expression(new ListExpression(values.Select(x => x is IExpression e ? e : new ConstantExpression(x))), InsertExpressionPositions.Values);
        }
        /// <inheritdoc/>
        public IInsertStatementBuilder<TEntity> ValuesUsing<T>(T valueObject, params string[] excludedProperties)
        {
            valueObject.ValidateArgument(nameof(valueObject));

            return Values(GetColumnPropertiesFrom<T>(excludedProperties).Select(x => x.GetValue(valueObject)));
        }
        /// <inheritdoc/>
        public IInsertStatementBuilder<TEntity> ParametersFrom<T>(string suffix = null, params string[] excludedProperties)
        {
            var builder = this.CastTo<IInsertStatementBuilder<TEntity>>();

            return builder.Values(GetColumnPropertiesFrom<T>(excludedProperties).Select(x => x.Name).Select(x => new SqlParameterExpression(x, suffix)));
        }
        #endregion

        /// <inheritdoc/>
        public override StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            // Auto assume no implicit 
            if (typeof(TEntity) == typeof(object)) options |= ExpressionCompileOptions.NoImplitExpressions;

            // Add implicit expressions
            if (!options.HasFlag(ExpressionCompileOptions.NoImplitExpressions) && (!Expressions.ContainsKey(InsertExpressionPositions.Into) || !Expressions[InsertExpressionPositions.Into].HasValue())) this.CastTo<IInsertStatementBuilder<TEntity>>().Into<TEntity>();

            return base.Build(builder, options);
        }
    }
}
