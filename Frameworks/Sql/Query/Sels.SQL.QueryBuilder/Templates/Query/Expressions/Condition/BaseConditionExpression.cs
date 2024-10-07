using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Template for creating a new <see cref="IStatementConditionExpressionBuilder{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public abstract class BaseConditionExpression<TEntity> : BaseExpressionContainer, IStatementConditionExpressionBuilder<TEntity>, IStatementConditionOperatorExpressionBuilder<TEntity>, IStatementConditionRightExpressionBuilder<TEntity>, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>
    {
        // Fields
        private readonly List<IConditionExpression> _expressions = new List<IConditionExpression>();

        // State
        private bool _nextConditionIsNot = false;
        private ComparisonConditionExpression _lastExpression;

        // Properties
        /// <summary>
        /// The currently configured condition expressions.
        /// </summary>
        public IConditionExpression[] Expressions => _expressions.ToArray();
        /// <inheritdoc/>
        public IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> LastBuilder => this;

        /// <inheritdoc cref=" BaseConditionExpression{TEntity}"/>
        /// <param name="builder">Delegate for configuring the current builder</param>
        /// <param name="throwOnEmpty">If a <see cref="InvalidOperationException"/> should be thrown when <paramref name="builder"/> created no expressions</param>
        /// <exception cref="InvalidOperationException"></exception>
        public BaseConditionExpression(Func<IStatementConditionExpressionBuilder<TEntity>, object> builder, bool throwOnEmpty = true)
        {
            builder.ValidateArgument(nameof(builder));
            builder(this);
            if (throwOnEmpty && _expressions.Count == 0) throw new InvalidOperationException($"Expected {nameof(builder)} to create at least 1 expression");
        }

        /// <inheritdoc/>
        public IStatementConditionExpressionBuilder<TEntity> Not()
        {
            _nextConditionIsNot = true;
            return this;
        }
        /// <inheritdoc/>
        public IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> WhereGroup(Func<IStatementConditionExpressionBuilder<TEntity>, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>> builder)
        {
            builder.ValidateArgument(nameof(builder));

            return FullExpression(new ConditionGroupExpression<TEntity>(builder, true));
        }
        /// <inheritdoc/>
        public IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> FullExpression(IConditionExpression expression)
        {
            expression.ValidateArgument(nameof(expression));

            var isNot = NextConditionIsInverted();
            expression.IsNot = isNot;
            _expressions.Add(expression);
            return this;
        }
        /// <inheritdoc/>
        public IStatementConditionOperatorExpressionBuilder<TEntity> Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (_lastExpression != null) throw new InvalidOperationException($"Condition expression already created");

            _lastExpression = new ComparisonConditionExpression(expression);
            return this;
        }
        /// <inheritdoc/>
        public IStatementConditionRightExpressionBuilder<TEntity> CompareTo(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            if (_lastExpression == null) throw new InvalidOperationException("Expected expression to update but was null");

            _lastExpression.OperatorExpression = sqlExpression;
            return this;
        }
        IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> ISharedExpressionBuilder<TEntity, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>>.Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (_lastExpression == null) throw new InvalidOperationException("Expected expression to update but was null");
            _lastExpression.RightExpression = expression;

            var builder = FullExpression(_lastExpression);
            _lastExpression = null;
            return builder;
        }
        /// <inheritdoc/>
        public IStatementConditionExpressionBuilder<TEntity> AndOr(LogicOperators logicOperator = LogicOperators.And)
        {
            var last = _expressions.LastOrDefault();
            if (last == null) throw new InvalidOperationException("Expected expression to update but was null");

            last.LogicOperator = logicOperator;
            return this;
        }

        private bool NextConditionIsInverted()
        {
            // Get value from is not and reset
            var isNot = _nextConditionIsNot;
            _nextConditionIsNot = false;
            return isNot;
        }
    }
}
