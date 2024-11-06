using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sels.Core.Extensions.Collections;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Template for creating a <see cref="IQueryBuilder{TPosition}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TPosition">Type that defines where in a query expressions should be located</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public abstract class BaseStatementBuilder<TEntity, TPosition, TDerived> : BaseQueryBuilder, 
        IStatementQueryBuilder<TEntity, TPosition, TDerived>, 
        IStatementJoinBuilder<TEntity, TDerived>,
        IStatementConditionBuilder<TEntity, TDerived>
        where TPosition : notnull
    {
        // Fields
        internal readonly Dictionary<Type, string> _aliases = new Dictionary<Type, string>();
        internal readonly Dictionary<TPosition, List<OrderedExpression>> _expressions = new Dictionary<TPosition, List<OrderedExpression>>();
        internal readonly IQueryCompiler<TPosition> _compiler;
        internal readonly List<Action<IExpression>> _expressionHandlers = new List<Action<IExpression>>();
        internal readonly List<Action<OrderedExpression, TPosition>> _positionExpressionHandlers = new List<Action<OrderedExpression, TPosition>>();

        // Properties
        /// <inheritdoc/>
        public IReadOnlyDictionary<TPosition, OrderedExpression[]> Expressions => _expressions.ToDictionary(x => x.Key, x => x.Value.ToArray());
        /// <inheritdoc/>
        public override IExpression[] InnerExpressions => _expressions.OrderBy(x => x.Key).SelectMany(x => x.Value).OrderBy(x => x.Order).Select(x => x.Expression).ToArray();
        /// <inheritdoc/>
        public IReadOnlyDictionary<Type, string> Aliases => _aliases;

        /// <inheritdoc cref="BaseStatementBuilder{TEntity, TPosition, TDerived}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        public BaseStatementBuilder(IQueryCompiler<TPosition> compiler)
        {
            _compiler = compiler.ValidateArgument(nameof(compiler));
        }
        /// <summary>
        /// <inheritdoc cref="BaseStatementBuilder{TEntity, TPosition, TDerived}"/>
        /// Copies settings from <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The builder to copy settings from</param>
        protected BaseStatementBuilder(BaseStatementBuilder<TEntity, TPosition, TDerived> other)
        {
            other.ValidateArgument(nameof(other));
            _compiler = other._compiler.ValidateArgument(nameof(_compiler));
            // Create new reference
            _expressions = other._expressions.ValidateArgument(nameof(_expressions)).ToDictionary(x => x.Key, x => x.Value.ToList());
            _aliases = other._aliases.ValidateArgument(nameof(_aliases)).ToDictionary(x => x.Key, x => x.Value);
            _expressionHandlers = other._expressionHandlers.ValidateArgument(nameof(_expressionHandlers)).ToList();
            _positionExpressionHandlers = other._positionExpressionHandlers.ValidateArgument(nameof(_positionExpressionHandlers)).ToList();
        }

        #region Alias
        /// <inheritdoc/>
        public void SetAlias(Type type, string alias)
        {
            type.ValidateArgument(nameof(type));

            _aliases.AddOrUpdate(type, alias);
        }
        /// <inheritdoc/>
        public TDerived AliasFor<T>(string tableAlias)
        {
            _aliases.AddOrUpdate(typeof(T), tableAlias);
            return Instance;
        }
        /// <inheritdoc/>
        public string GetAlias<T>()
        {
            return GetAlias(typeof(T));
        }
        /// <inheritdoc/>
        public string GetAlias(Type type)
        {
            type.ValidateArgument(nameof(type));

            return GetOrSetAlias(type);
        }
        /// <inheritdoc/>
        public TDerived OutAlias<T>(out string tableAlias)
        {
            tableAlias = GetAlias<T>();
            return Instance;
        }

        private string GetOrSetAlias(Type type)
        {
            type.ValidateArgument(nameof(type));

            if (_aliases.ContainsKey(type))
            {
                return _aliases[type];
            }
            else
            {
                var alias = type.Name[0].ToString();

                var counter = 1;
                while(_aliases.Any(x => x.Value != null && x.Value.Equals(alias, StringComparison.OrdinalIgnoreCase)))
                {
                    alias = alias + counter;
                    counter++;
                }

                _aliases.Add(type, alias);
                return alias;
            }
        }

        /// <inheritdoc/>
        public string TranslateToAlias(object alias)
        {
            return alias is Type type ? GetAlias(type) : alias.ToString();
        }
        #endregion

        #region Join
        /// <inheritdoc/>
        public IStatementJoinResultSetBuilder<TEntity, TDerived> Join(Joins joinType)
        {
            var expression = new JoinExpression<TEntity, TDerived>(joinType, Instance);
            Expression(expression, GetPositionForJoinExpression(expression));

            return expression;
        }
        #endregion

        #region Condition
        /// <inheritdoc/>
        public TDerived Where(Func<IStatementConditionExpressionBuilder<TEntity>, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>> builder)
        {
            builder.ValidateArgument(nameof(builder));

            var expression = new ConditionGroupExpression<TEntity>(builder, false);
            if(expression.Expressions.Length != 0) Expression(expression, GetPositionForConditionExpression(expression));

            return Instance;
        }
        #endregion

        #region Expression
        /// <inheritdoc/>
        public TDerived Expression(IExpression sqlExpression, TPosition position, int order = 0)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            position.ValidateArgument(nameof(position));

            var orderedExpression = new OrderedExpression(sqlExpression, order);
            _expressions.AddValueToList(position, orderedExpression);
            RaiseExpressionAdded(position, orderedExpression);
            return Instance;
        }

        private void RaiseExpressionAdded(TPosition position, OrderedExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            position.ValidateArgument(nameof(position));
            foreach (var handler in _positionExpressionHandlers)
            {
                handler(expression, position);
            }
            foreach (var handler in _expressionHandlers)
            {
                handler(expression.Expression);
            }
        }
        #endregion

        #region Build
        /// <inheritdoc/>
        public override StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            _compiler.CompileTo(builder, this, x => x.SetDataSetConverter(TranslateToAlias).OnCompiling(OnCompiling), options);
            return builder;
        }
        #endregion

        /// <summary>
        /// Returns all properties on type <typeparamref name="T"/> that can be used as object names/values for a query.
        /// </summary>
        /// <typeparam name="T">The type to get the properties from</typeparam>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>All usable properties on <typeparamref name="T"/></returns>
        protected IEnumerable<PropertyInfo> GetColumnPropertiesFrom<T>(string[] excludedProperties)
        {
            return typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetIndexParameters()?.Length == 0 && (!excludedProperties.HasValue() || !excludedProperties.Contains(x.Name, StringComparer.OrdinalIgnoreCase)));
        }

        /// <inheritdoc/>
        public IQueryBuilder<TPosition> OnExpressionAdded(Action<OrderedExpression, TPosition> action)
        {
            _positionExpressionHandlers.Add(action.ValidateArgument(nameof(action)));
            return this;
        }

        // Abstractions
        /// <inheritdoc/>
        public abstract TDerived Instance { get; }

        /// <summary>
        /// Gets the position for <paramref name="joinExpression"/>.
        /// </summary>
        /// <param name="joinExpression">The expression to get the position for</param>
        protected abstract TPosition GetPositionForJoinExpression(JoinExpression<TEntity, TDerived> joinExpression);
        /// <summary>
        /// Gets the position for <paramref name="conditionExpression"/>.
        /// </summary>
        /// <param name="conditionExpression">The expression to get the position for</param>
        protected abstract TPosition GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression);

        ///<inheritdoc/>
        public abstract TDerived Clone();

    }
}
