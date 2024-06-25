using Microsoft.Extensions.Logging;
using Sels.Core;
using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.Expressions.Condition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Text;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Sels.SQL.QueryBuilder.Expressions;
using Sels.SQL.QueryBuilder.MySQL.Expressions;

namespace Sels.SQL.QueryBuilder.MySQL
{
    /// <summary>
    /// Query compiler that converts sql expressions to the mysql syntax.
    /// </summary>
    public class MySqlCompiler : ISqlCompiler
    {
        // Fields
        private readonly ILogger _logger;
        private readonly string _name = nameof(MySqlCompiler);

        // Properties
        /// <summary>
        /// Global logger that can be set that will be used as the default logger when none is provided.
        /// </summary>
        public static ILogger DefaultLogger { get; set; }

        #region Configs
        private static readonly IReadOnlyDictionary<InsertExpressionPositions, (bool IsNewLine, string Prefix, string[] ExpressionJoinValues, string Suffix, bool IsSingleExpression)> _insertPositionConfigs = new Dictionary<InsertExpressionPositions, (bool IsNewLine, string Prefix, string[] ExpressionJoinValues, string Suffix, bool IsSingleExpression)>()
        {
            { InsertExpressionPositions.Into, (false, Sql.Clauses.Into, null, null, true) },
            { InsertExpressionPositions.Columns, (false, "(", new string[] {  Constants.Strings.Comma}, ")", false) },
            { InsertExpressionPositions.Values, (true, Sql.Clauses.Values + Constants.Strings.Space, new string[]{ Constants.Strings.Comma , Environment.NewLine}, null, false) }
        };
        private static readonly IReadOnlyDictionary<SelectExpressionPositions, (bool IsNewLine, string Clause, string ExpressionJoinValue, bool IsSingleExpression)> _selectPositionConfigs = new Dictionary<SelectExpressionPositions, (bool IsNewLine, string Clause, string ExpressionJoinValue, bool IsSingleExpression)>()
        {
            { SelectExpressionPositions.Column, (false, null, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.From, (true, Sql.Clauses.From, null, true) },
            { SelectExpressionPositions.Join, (true, null, Environment.NewLine, false) },
            { SelectExpressionPositions.Where, (true, Sql.Clauses.Where, Constants.Strings.Comma + Sql.LogicOperators.And, false) },
            { SelectExpressionPositions.OrderBy, (true, Sql.Clauses.OrderBy, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.GroupBy, (true, Sql.Clauses.GroupBy, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.Having, (true, Sql.Clauses.Having, Constants.Strings.Comma + Sql.LogicOperators.And, false) },
            { SelectExpressionPositions.After, (true, null, Environment.NewLine, false) },
        };
        private static readonly IReadOnlyDictionary<UpdateExpressionPositions, (bool IsNewLine, string Prefix, string[] ExpressionJoinValues, string Suffix, bool IsSingleExpression)> _updatePositionConfigs = new Dictionary<UpdateExpressionPositions, (bool IsNewLine, string Prefix, string[] ExpressionJoinValues, string Suffix, bool IsSingleExpression)>()
        {
            { UpdateExpressionPositions.Table, (false, null, new string[] {  Constants.Strings.Comma}, null, false) },
            { UpdateExpressionPositions.Join, (true, null, new string[] {  Environment.NewLine}, null, false) },
            { UpdateExpressionPositions.OrderBy, (true, Sql.Clauses.OrderBy, new string[]{ Constants.Strings.Comma }, null, false) },
            { UpdateExpressionPositions.Set, (true, Sql.Clauses.Set + Constants.Strings.Space, new string[] { Constants.Strings.Comma ,Environment.NewLine }, null, false) },
            { UpdateExpressionPositions.Where, (true, Sql.Clauses.Where + Constants.Strings.Space, new string[] { Constants.Strings.Comma, Sql.LogicOperators.And }, null, false) }
        };
        private static readonly IReadOnlyDictionary<DeleteExpressionPositions, (bool IsNewLine, string Clause, string ExpressionJoinValue, bool IsSingleExpression)> _deletePositionConfigs = new Dictionary<DeleteExpressionPositions, (bool IsNewLine, string Clause, string ExpressionJoinValue, bool IsSingleExpression)>()
        {
            { DeleteExpressionPositions.From, (true, Sql.Clauses.From, Constants.Strings.Comma, false) },
            { DeleteExpressionPositions.Join, (true, null, Environment.NewLine, false) },
            { DeleteExpressionPositions.Where, (true, Sql.Clauses.Where, Constants.Strings.Comma + Sql.LogicOperators.And, false) },
            { DeleteExpressionPositions.OrderBy, (true, Sql.Clauses.OrderBy, Constants.Strings.Comma, false) },
            { DeleteExpressionPositions.After, (true, null, Environment.NewLine, false) },
        };

        private static readonly IReadOnlyDictionary<Type, Func<IExpression, IExpression>> _mappedExpressions = new Dictionary<Type, Func<IExpression, IExpression>>()
        {
            // Override assignment operator
            { typeof(VariableInlineAssignmentExpression), x => {
                var casted = x.CastTo<VariableInlineAssignmentExpression>();
                casted.AssignmentOperator = MySql.Keywords.VariableAssignmentOperator;
                return casted;
            }},
            // Remove schema as MySql doesn't support it
            { typeof(TableExpression), x => {
                var casted = x.CastTo<TableExpression>();
                casted.SetSchema(null);
                return casted;
            }},
            // Needs custom compiler support
            { typeof(CurrentDateExpression), x => new NowExpression(x.CastTo<CurrentDateExpression>().Type) },
            { typeof(ModifyDateExpression), x => {
                var casted = x.CastTo<ModifyDateExpression>();
                return new DateAddExpression(casted.Date, casted.Amount, casted.Interval);
            }},
        };
        #endregion

        /// <inheritdoc cref="MySqlCompiler"/>
        /// <param name="logger">Optional logger for tracing</param>
        public MySqlCompiler(ILogger logger = null)
        {
            _logger = logger ?? DefaultLogger;
        }

        #region Insert
        /// <inheritdoc/>
        void IQueryCompiler<InsertExpressionPositions>.CompileTo(StringBuilder builder, IQueryBuilder<InsertExpressionPositions> queryBuilder, Action<ICompilerOptions> configurator, ExpressionCompileOptions options)
        {
            using (_logger.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                var compilerOptions = new MySqlCompilerOptions(configurator);
                var builderExpressions = queryBuilder.Expressions;

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

                _logger.Log($"{_name} compiling <{expressionCount}> expressions into a insert query");
                if (expressionCount == 0) throw new NotSupportedException($"No expressions to compile into a insert query");

                builder.Append(Sql.Statements.Insert);

                foreach (var builderExpression in builderExpressions.OrderBy(x => x.Key).Where(x => x.Value.HasValue()))
                {
                    var position = builderExpression.Key;
                    var expressions = builderExpression.Value.OrderBy(x => x.Order).Select(x => x.Expression).ToArray();

                    // Position settings
                    var isConfigSet = _insertPositionConfigs.ContainsKey(position);
                    var isNewLine = isConfigSet ? _insertPositionConfigs[position].IsNewLine : false;
                    var prefix = isConfigSet ? _insertPositionConfigs[position].Prefix : null;
                    var joinValues = (isConfigSet ? _insertPositionConfigs[position].ExpressionJoinValues : null) ?? Constants.Strings.Space.AsArray();
                    var suffix = isConfigSet ? _insertPositionConfigs[position].Suffix : null;
                    var isSingleExpression = isConfigSet ? _insertPositionConfigs[position].IsSingleExpression : false;

                    _logger.Debug($"{_name} compiling <{expressions.Length}> expressions for position <{position}>");

                    // Formatting
                    if (isFormatted && isNewLine)
                    {
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.AppendSpace();
                    }

                    // Prefix
                    if (prefix.HasValue())
                    {
                        builder.Append(prefix).AppendSpace();
                    }

                    // Single value
                    if (isSingleExpression)
                    {
                        if (expressions.Length > 1) _logger.Warning($"Expected only 1 expression for position <{position}> but got <{expressions.Length}>. Taking the last expression");
                        CompileExpressionTo(builder, compilerOptions, options, expressions.Last(), true);
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {
                            CompileExpressionTo(builder, compilerOptions, options, e, true);
                            if (i < expressions.Length - 1) {
                                var joinChars = joinValues.Select(x => x == Environment.NewLine && !isFormatted ? Constants.Strings.Space : x);
                                joinChars.Execute(x => builder.Append(x));
                            }
                        });
                    }

                    // Suffix
                    if (suffix.HasValue())
                    {
                        builder.Append(suffix);
                    }
                }
                if (options.HasFlag(ExpressionCompileOptions.AppendSeparator)) builder.Append(';');
            }
        }
        #endregion

        #region Select
        /// <inheritdoc/>
        void IQueryCompiler<SelectExpressionPositions>.CompileTo(StringBuilder builder, IQueryBuilder<SelectExpressionPositions> queryBuilder, Action<ICompilerOptions> configurator, ExpressionCompileOptions options)
        {
            using (_logger.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                var compilerOptions = new MySqlCompilerOptions(configurator);
                var builderExpressions = queryBuilder.Expressions;

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

                _logger.Log($"{_name} compiling <{expressionCount}> expressions into a select query");
                if (expressionCount == 0) throw new NotSupportedException($"No expressions to compile into a select query");
 
                builder.Append(Sql.Statements.Select);

                foreach (var builderExpression in builderExpressions.OrderBy(x => x.Key).Where(x => x.Value.HasValue()))
                {
                    var position = builderExpression.Key;
                    var expressions = builderExpression.Value.OrderBy(x => x.Order).Select(x => x.Expression).ToArray();

                    // Position settings
                    var isConfigSet = _selectPositionConfigs.ContainsKey(position);
                    var isNewLine = isConfigSet ? _selectPositionConfigs[position].IsNewLine : false;
                    var clause = isConfigSet ? _selectPositionConfigs[position].Clause : null;
                    var joinValue = (isConfigSet ? _selectPositionConfigs[position].ExpressionJoinValue : null) ?? Constants.Strings.Space;
                    var isSingleExpression = isConfigSet ? _selectPositionConfigs[position].IsSingleExpression : false;

                    _logger.Debug($"{_name} compiling <{expressions.Length}> expressions for position <{position}>");

                    // Formatting
                    if (isFormatted && isNewLine)
                    {
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.AppendSpace();
                    }

                    // Clause
                    if (clause.HasValue())
                    {
                        builder.Append(clause).AppendSpace();
                    }

                    // Single value
                    if (isSingleExpression)
                    {
                        if (expressions.Length > 1) _logger.Warning($"Expected only 1 expression for position <{position}> but got <{expressions.Length}>. Taking the last expression");
                        CompileExpressionTo(builder, compilerOptions, options, expressions.Last());
                        continue;
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {
                            var expressionJoinValue = joinValue == Environment.NewLine && !isFormatted ? Constants.Strings.Space : joinValue;

                            CompileExpressionTo(builder, compilerOptions, options, e);
                            if (i < expressions.Length - 1) builder.Append(expressionJoinValue);
                        });
                    }
                }

                if (options.HasFlag(ExpressionCompileOptions.AppendSeparator)) builder.Append(';');
            }
        }
        #endregion

        #region Update
        /// <inheritdoc/>
        public void CompileTo(StringBuilder builder, IQueryBuilder<UpdateExpressionPositions> queryBuilder, Action<ICompilerOptions> configurator, ExpressionCompileOptions options)
        {
            using (_logger.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                var compilerOptions = new MySqlCompilerOptions(configurator);
                var builderExpressions = queryBuilder.Expressions;

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

                _logger.Log($"{_name} compiling <{expressionCount}> expressions into an update query");
                if (expressionCount == 0) throw new NotSupportedException($"No expressions to compile into an update query");

                builder.Append(Sql.Statements.Update);

                foreach (var builderExpression in builderExpressions.OrderBy(x => x.Key).Where(x => x.Value.HasValue()))
                {
                    var position = builderExpression.Key;
                    var expressions = builderExpression.Value.OrderBy(x => x.Order).Select(x => x.Expression).ToArray();

                    // Position settings
                    var isConfigSet = _updatePositionConfigs.ContainsKey(position);
                    var isNewLine = isConfigSet ? _updatePositionConfigs[position].IsNewLine : false;
                    var prefix = isConfigSet ? _updatePositionConfigs[position].Prefix : null;
                    var joinValues = (isConfigSet ? _updatePositionConfigs[position].ExpressionJoinValues : null) ?? Constants.Strings.Space.AsArray();
                    var suffix = isConfigSet ? _updatePositionConfigs[position].Suffix : null;
                    var isSingleExpression = isConfigSet ? _updatePositionConfigs[position].IsSingleExpression : false;

                    _logger.Debug($"{_name} compiling <{expressions.Length}> expressions for position <{position}>");

                    // Formatting
                    if (isFormatted && isNewLine)
                    {
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.AppendSpace();
                    }

                    // Prefix
                    if (prefix.HasValue())
                    {
                        builder.Append(prefix).AppendSpace();
                    }

                    // Single value
                    if (isSingleExpression)
                    {
                        if (expressions.Length > 1) _logger.Warning($"Expected only 1 expression for position <{position}> but got <{expressions.Length}>. Taking the last expression");
                        CompileExpressionTo(builder, compilerOptions, options, expressions.Last());
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {

                            CompileExpressionTo(builder, compilerOptions, options, e);
                            if (i < expressions.Length - 1)
                            {
                                var joinChars = joinValues.Select(x => x == Environment.NewLine && !isFormatted ? Constants.Strings.Space : x);
                                joinChars.Execute(x => builder.Append(x));
                            }
                        });
                    }

                    // Suffix
                    if (suffix.HasValue())
                    {
                        builder.Append(suffix);
                    }
                }

                if (options.HasFlag(ExpressionCompileOptions.AppendSeparator)) builder.Append(';');
            }
        }
        #endregion

        #region Delete
        /// <inheritdoc/>
        void IQueryCompiler<DeleteExpressionPositions>.CompileTo(StringBuilder builder, IQueryBuilder<DeleteExpressionPositions> queryBuilder, Action<ICompilerOptions> configurator, ExpressionCompileOptions options)
        {
            using (_logger.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                var compilerOptions = new MySqlCompilerOptions(configurator);
                var builderExpressions = queryBuilder.Expressions;

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);
                
                _logger.Log($"{_name} compiling <{expressionCount}> expressions into a delete query");
                if (expressionCount == 0) throw new NotSupportedException($"No expressions to compile into a delete query");

                builder.Append(Sql.Statements.Delete);

                foreach (var builderExpression in builderExpressions.OrderBy(x => x.Key).Where(x => x.Value.HasValue()))
                {
                    var position = builderExpression.Key;
                    var expressions = builderExpression.Value.OrderBy(x => x.Order).Select(x => x.Expression).ToArray();

                    // Position settings
                    var isConfigSet = _deletePositionConfigs.ContainsKey(position);
                    var isNewLine = isConfigSet ? _deletePositionConfigs[position].IsNewLine : false;
                    var clause = isConfigSet ? _deletePositionConfigs[position].Clause : null;
                    var joinValue = (isConfigSet ? _deletePositionConfigs[position].ExpressionJoinValue : null) ?? Constants.Strings.Space;
                    var isSingleExpression = isConfigSet ? _deletePositionConfigs[position].IsSingleExpression : false;

                    _logger.Debug($"{_name} compiling <{expressions.Length}> expressions for position <{position}>");

                    // Formatting
                    if (isFormatted && isNewLine)
                    {
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.AppendSpace();
                    }

                    // Special cases
                    if (position == DeleteExpressionPositions.From)
                    {
                        if(expressions.Any(x => x is IObjectExpression || x is TableExpression))
                        {
                            var aliases = expressions.Where(x => x is IObjectExpression || x is TableExpression)
                                                        .Select(x => ConvertDataSet(x is TableExpression tableExpression ? tableExpression.Alias?.Set : x.CastTo<IObjectExpression>().Set, compilerOptions))
                                                        .Where(x => x != null).ToArray();

                            if (aliases.HasValue())
                            {
                                aliases.Execute((i, a) =>
                                {
                                    builder.Append(a);
                                    if (i != aliases.Length - 1) builder.Append(',');
                                });

                                // Formatting
                                if (isFormatted && isNewLine)
                                {
                                    builder.AppendLine();
                                }
                                else
                                {
                                    builder.AppendSpace();
                                }
                            }
                            else
                            {
                                _logger.Debug($"{_name}: No table aliases defined. Using default format");
                            }
                        }
                        else
                        {
                            _logger.Debug($"{_name}: No expression is <{nameof(IObjectExpression)}>. Won't be able to determine table to delete from. Using default format");
                        }
                    }

                    //Clause
                    if (clause.HasValue())
                    {
                        builder.Append(clause).AppendSpace();
                    }

                    // Single value
                    if (isSingleExpression)
                    {
                        if (expressions.Length > 1) _logger.Warning($"Expected only 1 expression for position <{position}> but got <{expressions.Length}>. Taking the last expression");
                        CompileExpressionTo(builder, compilerOptions, options, expressions.Last());
                        continue;
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {
                            var expressionJoinValue = joinValue == Environment.NewLine && !isFormatted ? Constants.Strings.Space : joinValue;

                            CompileExpressionTo(builder, compilerOptions, options, e);
                            if (i < expressions.Length - 1) builder.Append(expressionJoinValue);
                        });
                    }
                }

                if (options.HasFlag(ExpressionCompileOptions.AppendSeparator)) builder.Append(';');
            }
        }
        #endregion

        #region If 
        /// <inheritdoc/>
        public void CompileTo(StringBuilder builder, IIfStatementBuilder statementBuilder, Action<ICompilerOptions> configurator = null, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                statementBuilder.ValidateArgument(nameof(statementBuilder));
                var compilerOptions = new MySqlCompilerOptions(configurator);

                var expressionCount = statementBuilder.InnerExpressions.Count();
                var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

                _logger.Log($"{_name} compiling <{expressionCount}> expressions into a condition query");
                if (expressionCount == 0) throw new NotSupportedException($"No expressions to compile into a condition query");

                builder.Append(Sql.Statements.If);
                builder.AppendSpace();
                // Conditions for IF statement
                var conditions = statementBuilder.ConditionExpressions;
                conditions.Execute((i, e) =>
                {
                    CompileExpressionTo(builder, compilerOptions, options, e);
                    if (i < conditions.Length - 1) builder.AppendSpace().Append(Sql.LogicOperators.And).AppendSpace();
                });
                builder.AppendSpace().Append(MySql.Keywords.Then);

                if (isFormatted) builder.AppendLine();
                else builder.AppendSpace();

                // Body for IF statement
                statementBuilder.BodyBuilder.Build(builder, options);
                if (isFormatted) builder.AppendLine().AppendLine();
                else builder.AppendSpace();

                // ELSE IF statements
                if (statementBuilder.ElseIfStatements.HasValue())
                {
                    foreach(var (elseIfConditions, elseIfBodyBuilder) in statementBuilder.ElseIfStatements)
                    {
                        builder.Append(Sql.Statements.ElseIf);
                        builder.AppendSpace();

                        // Conditions for ELSE IF statement
                        elseIfConditions.Execute((i, e) =>
                        {
                            CompileExpressionTo(builder, compilerOptions, options, e);
                            if (i < elseIfConditions.Length - 1) builder.AppendSpace().Append(Sql.LogicOperators.And).AppendSpace();
                        });
                        builder.AppendSpace().Append(MySql.Keywords.Then);

                        if (isFormatted) builder.AppendLine();
                        else builder.AppendSpace();

                        // Body for ELSE IF statement
                        elseIfBodyBuilder.Build(builder, options);
                        if (isFormatted) builder.AppendLine().AppendLine();
                        else builder.AppendSpace();
                    }
                }

                // Body for ELSE statement
                if (statementBuilder.ElseBodyBuilder.InnerExpressions.HasValue())
                {
                    builder.Append(Sql.Statements.Else);
                    if (isFormatted) builder.AppendLine();
                    else builder.AppendSpace();

                    statementBuilder.ElseBodyBuilder.Build(builder, options);
                    if (isFormatted) builder.AppendLine().AppendLine();
                    else builder.AppendSpace();
                }

                builder.Append(MySql.Keywords.EndIf);
                if (options.HasFlag(ExpressionCompileOptions.AppendSeparator)) builder.Append(';');
            }
        }
        #endregion

        #region Variable
        /// <inheritdoc/>
        public void CompileTo(StringBuilder builder, IVariableDeclarationStatementBuilder statementBuilder, Action<ICompilerOptions> configurator = null, ExpressionCompileOptions options = ExpressionCompileOptions.None) => throw new NotSupportedException($"MySql does not support variable declarations");
        /// <inheritdoc/>
        public void CompileTo(StringBuilder builder, IVariableSetterStatementBuilder statementBuilder, Action<ICompilerOptions> configurator = null, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                statementBuilder.ValidateArgument(nameof(statementBuilder));
                var compilerOptions = new MySqlCompilerOptions(configurator);

                var expressionCount = statementBuilder.InnerExpressions.Count();
                var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

                _logger.Log($"{_name} compiling <{expressionCount}> expressions into a variable set statement");
                if (expressionCount == 0) throw new NotSupportedException($"No expressions to compile into a variable set statement");

                builder.Append(Sql.Set);
                builder.AppendSpace();

                // Variable to set
                CompileExpressionTo(builder, compilerOptions, options, statementBuilder.Variable);
                builder.AppendSpace();

                // Value to assign
                builder.Append(Sql.AssignmentOperator).AppendSpace();
                CompileExpressionTo(builder, compilerOptions, options, statementBuilder.Value);

                if (options.HasFlag(ExpressionCompileOptions.AppendSeparator)) builder.Append(';');
            }
        }
        #endregion

        /// <inheritdoc/>
        public string Compile(IExpression expression, Action<ICompilerOptions> configurator, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                expression.ValidateArgument(nameof(expression));

                return Compile(new StringBuilder(), expression, configurator, options).ToString();
            }                 
        }
        /// <inheritdoc/>
        public StringBuilder Compile(StringBuilder builder, IExpression expression, Action<ICompilerOptions> configurator, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                expression.ValidateArgument(nameof(expression));

                var compilerOptions = new MySqlCompilerOptions(configurator);
                CompileExpressionTo(builder, compilerOptions, options, expression);

                return builder;
            }
        }

        private void CompileExpressionTo(StringBuilder builder, MySqlCompilerOptions compilerOptions, ExpressionCompileOptions options, IExpression expression, bool isInsert = false, bool allowMap = true)
        {
            builder.ValidateArgument(nameof(builder));
            expression.ValidateArgument(nameof(expression));
            compilerOptions.ValidateArgument(nameof(compilerOptions));

            _logger.Trace($"{_name} compiling expression of type <{expression.GetType().GetDisplayName()}>");

            if(allowMap && _mappedExpressions.TryGetValue(expression.GetType(), out var mapper))
            {
                var mappedExpression = mapper(expression);
                CompileExpressionTo(builder, compilerOptions, options, mappedExpression, isInsert, false);
            }
            else
            {
                compilerOptions.RaiseOnCompiling(expression);

                if (expression is IColumnExpression columnExpression && !columnExpression.Object.Equals(Sql.All.ToString()))
                {
                    // Add back ticks around column names 
                    columnExpression.ToSql(builder, x => ConvertDataSet(x, compilerOptions, isInsert), x => $"`{x}`", options);
                }
                else if (expression is IObjectExpression objectExpression && !objectExpression.Object.Equals(Sql.All.ToString()))
                {
                    // Add back ticks around sql object names 
                    objectExpression.ToSql(builder, x => ConvertDataSet(x, compilerOptions, isInsert), x => $"`{x}`", options);
                }
                else if (expression is IDataSetExpression dataSetExpression)
                {
                    dataSetExpression.ToSql(builder, x => ConvertDataSet(x, compilerOptions, isInsert), options);
                }
                else if (expression is IExpressionContainer expressionContainer)
                {
                    expressionContainer.ToSql(builder, (b, e) => CompileExpressionTo(b, compilerOptions, options, e, isInsert), options);
                }
                else if (expression is ITypeExpression typeExpression)
                {
                    typeExpression.ToSql(builder, MapTypeToMySqlType, options);
                }
                else
                {
                    expression.ToSql(builder, options);
                }
            }
        }

        private string ConvertDataSet(object dataSet, MySqlCompilerOptions compilerOptions, bool isInsert = false)
        {
            if (dataSet == null) return null;
            // Disable datasets in insert statements
            if (isInsert) return null;

            return compilerOptions.DataSetConverter(dataSet);
        }

        private void MapTypeToMySqlType(StringBuilder builder, Type type, int? length)
        {
            builder.ValidateArgument(nameof(builder));
            type.ValidateArgument(nameof(type));
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type.Is<string>())
            {
                if (length.HasValue) builder.Append("VARCHAR(").Append(length.Value).Append(')');
                else builder.Append("LONGTEXT");
            }
            else if (type.Is<char>())
            {
                builder.Append("VARCHAR(").Append(1).Append(')');
            }
            else if (type.Is<bool>())
            {
                builder.Append("BIT(").Append(1).Append(')');
            }
            else if (type.Is<DateTime>() || type.Is<DateTimeOffset>())
            {
                builder.Append("DATETIME");
            }
            else if (type.Is<decimal>())
            {
                builder.Append("DECIMAL");
            }
            else if (type.Is<double>())
            {
                builder.Append("DOUBLE");
            }
            else if (type.Is<short>())
            {
                builder.Append("SMALLINT");
            }
            else if (type.Is<int>())
            {
                builder.Append("INT");
            }
            else if (type.Is<long>())
            {
                builder.Append("BIGINT");
            }
            else if (type.Is<Guid>())
            {
                builder.Append("CHAR(36)");
            }
            else if (type.Is<TimeSpan>())
            {
                builder.Append("TIME");
            }
            else if (type.Is<sbyte>())
            {
                builder.Append("TINYINT");
            }
            else if (type.Is<float>())
            {
                builder.Append("FLOAT");
            }
            else if (type.Is<byte>())
            {
                builder.Append("TINYINT");
            }
            else if (type.Is<byte[]>())
            {
                if (length.HasValue) builder.Append("VARBINARY(").Append(length.Value).Append(')');
                else builder.Append("LONGBLOB");
            }
            else
            {
                throw new NotSupportedException($"No mapping exists for mapping .net type <{type}> to an MySql type");
            }
        }

        /// <inheritdoc cref="ICompilerOptions"/>
        private class MySqlCompilerOptions : ICompilerOptions
        {
            // Fields
            private readonly List<Action<IExpression>> _compilationHandlers = new List<Action<IExpression>>();

            // Properties
            /// <summary>
            /// Delegate that converts an object that represents a dataset into it's sql equivalent.
            /// </summary>
            public Func<object, string> DataSetConverter { get; private set; }

            /// <inheritdoc cref="MySqlCompilerOptions"/>
            /// <param name="configurator">Optional delegate for configuring the current instance</param>
            public MySqlCompilerOptions(Action<ICompilerOptions> configurator)
            {
                // Set defaults
                SetDataSetConverter(x => x.ToString());

                // Configure
                configurator?.Invoke(this);
            }

            internal void RaiseOnCompiling(IExpression expression)
            {
                expression.ValidateArgument(nameof(expression));

                foreach(var handler in _compilationHandlers)
                {
                    handler(expression);
                }
            }

            /// <inheritdoc/>
            public ICompilerOptions SetDataSetConverter(Func<object, string> converterDelegate)
            {
                DataSetConverter = converterDelegate.ValidateArgument(nameof(converterDelegate));
                return this;
            }

            /// <inheritdoc/>
            public ICompilerOptions OnCompiling(Action<IExpression> action)
            {
                _compilationHandlers.Add(action.ValidateArgument(nameof(action)));
                return this;
            }
        }
    }
}
