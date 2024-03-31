using Sels.Core.Data.Contracts.Repository;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using Sels.SQL.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sels.Core.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Sels.SQL.QueryBuilder.Builder;
using Dapper;
using Sels.Core.Extensions.Linq;
using System.Linq;
using Sels.SQL.QueryBuilder.Extensions;
using Sels.Core.Extensions.Fluent;
using Sels.SQL.QueryBuilder.Expressions;
using System.Data.Common;
using Sels.Core.Conversion.Attributes.Serialization;
using Sels.SQL.QueryBuilder.Statements;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Castle.Core.Resource;
using Sels.Core.Extensions.Text;
using Sels.Core.Data.SQL.Extensions.Dapper;

namespace Sels.DistributedLocking.SQL.Templates
{
    /// <summary>
    /// Base class for creating a <see cref="ISqlLockRepository"/> using a <see cref="ICachedSqlQueryProvider"/> and Dapper.
    /// </summary>
    public abstract class BaseSqlLockRepository : ISqlLockRepository
    {
        // Constants
        /// <summary>
        /// The table alias for <see cref="SqlLock"/>.
        /// </summary>
        protected const string SqlLockAlias = "L";
        /// <summary>
        /// The table alias for <see cref="SqlLockRequest"/>.
        /// </summary>
        protected const string SqlLockRequestAlias = "LR";

        // Fields
        /// <summary>
        /// The compile options used to build queries.
        /// </summary>
        protected ExpressionCompileOptions _queryOptions = ExpressionCompileOptions.DateAsUtc;
        /// <summary>
        /// The default format used to created named queries.
        /// </summary>
        protected readonly string _queryNameFormat;
        /// <summary>
        /// Service that builds database independant sql queries.
        /// </summary>
        private readonly Lazy<ICachedSqlQueryProvider> _queryProvider;
        /// <summary>
        /// Optional logger for tracing.
        /// </summary>
        protected readonly ILogger _logger;

        // Properties
        /// <summary>
        /// Service that builds database independant sql queries.
        /// </summary>
        protected ICachedSqlQueryProvider QueryProvider => _queryProvider.Value;
        /// <summary>
        /// The table name for <see cref="SqlLock"/>
        /// </summary>
        protected string SqlLockTableName { get; set; } = nameof(SqlLock);
        /// <summary>
        /// The table name for <see cref="SqlLockRequest"/>
        /// </summary>
        protected string SqlLockRequestTableName { get; set; } = nameof(SqlLockRequest);
        /// <summary>
        /// The schema the lock tables are located in. Null by default.
        /// </summary>
        protected string Schema { get; set; }

        /// <inheritdoc cref="BaseSqlLockRepository"/>
        /// <param name="queryProvider"><inheritdoc cref="_queryProvider"/></param>
        /// <param name="logger">O<inheritdoc cref="_logger"/></param>
        public BaseSqlLockRepository(ICachedSqlQueryProvider queryProvider, ILogger logger = null)
        {
            queryProvider.ValidateArgument(nameof(queryProvider));
            _queryProvider = new Lazy<ICachedSqlQueryProvider>(() => queryProvider.CreateSubCachedProvider(x => x.OnBuilderCreated(OnBuilderCreated).WithExpressionCompileOptions(_queryOptions)), true);
            _queryNameFormat = $"{GetType().GetDisplayName()}.{{0}}";

#if DEBUG
            _queryOptions |= ExpressionCompileOptions.Format;
#endif

            _logger = logger;
        }
        /// <inheritdoc/>
        public async Task<SqlLockRequest[]> GetAllLockRequestsByIdsAsync(IRepositoryTransaction transaction, long[] ids, CancellationToken token)
        {
            ids.ValidateArgumentNotNullOrEmpty(nameof(ids));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Fetching the state of <{ids.Length}> lock requests");
            // Generate query
            var query = QueryProvider.Select<SqlLockRequest>()
                                        .AllOf<SqlLock>()
                                        .AllOf()
                                        .InnerJoin().Table<SqlLock>().On(o => o.Column(c => c.Resource).EqualTo.Column<SqlLock>(c => c.Resource))
                                        .Where(w => w.Column(c => c.Id).In.Values(ids))
                                        .OrderBy(x => x.IsAssigned, SortOrders.Descending)
                                        .Build(_queryOptions);
            _logger.Trace($"Fetching the state of <{ids.Length}> lock requests using query <{query}>");

            // Execute query
            var locks = (await dbConnection.QueryAsync< SqlLock, SqlLockRequest, SqlLockRequest>(new CommandDefinition(query, null, transaction: dbTransaction, cancellationToken: token), (l, r) =>
            {
                r.Lock = l;
                return r;
            }, nameof(SqlLockRequest.Id)).ConfigureAwait(false)).ToArray();
            _logger.Log($"Fetched <{locks.Length}> lock requests");
            return locks;
        }
        /// <inheritdoc/>
        public virtual async Task<SqlLock> GetLockByResourceAsync(IRepositoryTransaction transaction, string resource, bool countRequests, CancellationToken token)
        {
            resource.ValidateArgument(nameof(resource));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Fetching lock on resource <{resource}>");
            // Generate query
            string query = null;
            if (countRequests)
            {
                query = _queryProvider.Value.GetQuery(_queryNameFormat.FormatString($"{nameof(GetLockByResourceAsync)}.WithCount"), x =>
                {
                    return x.Select<SqlLock>()
                        .AllOf()
                        .ColumnExpression(b => b.Query(QueryProvider.Select<SqlLockRequest>().CountAll().Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))))).As(nameof(SqlLock.PendingRequests))
                        .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)));
                });
            }
            else
            {
                query = _queryProvider.Value.GetQuery(_queryNameFormat.FormatString($"{nameof(GetLockByResourceAsync)}.NoCount"), x =>
                {
                    return x.Select<SqlLock>()
                        .AllOf()
                        .Value(0).As(nameof(SqlLock.PendingRequests))
                        .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)));
                });
            }

            var parameters = new DynamicParameters()
                                 .AddParameter($"@{nameof(resource)}", resource);
            _logger.Trace($"Fetching lock on resource <{resource}> using <{query}>");

            // Execute query
            var sqlLock = await dbConnection.QuerySingleOrDefaultAsync<SqlLock>(new CommandDefinition(query, parameters, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);

            // Query always returns even though there's no row so check resource too
            if (sqlLock == null || sqlLock.Resource == null)
            {
                _logger.Warning($"Lock on resource <{resource}> does not exist");
                return null;
            }
            else
            {
                _logger.Log($"Fetched lock on resource <{resource}>");
            }

            return sqlLock?.SetToLocal();
        }
        /// <inheritdoc/>
        public virtual async Task DeleteAllRequestsById(IRepositoryTransaction transaction, long[] ids, CancellationToken token)
        {
            ids.ValidateArgumentNotNullOrEmpty(nameof(ids));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Deleting <{ids.Length}> lock requests");
            var query = QueryProvider.Delete<SqlLockRequest>()
                                            .Where(w => w.Column(c => c.Id).In.Values(ids))
                                            .Build(_queryOptions);
            _logger.Trace($"Deleting <{ids.Length}> lock requests using query <{query}>");

            var deleted = await dbConnection.ExecuteAsync(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);
            _logger.Log($"Deleted <{deleted}> lock requests");
        }
        /// <inheritdoc/>
        public virtual async Task<int> DeleteInActiveLocksAsync(IRepositoryTransaction transaction, int? inactiveTime = null, CancellationToken token = default)
        {
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);
            _logger.Log($"Deleting all {(inactiveTime.HasValue ? "inactive" : "free")} locks");
            // Delete all locks with no pending requests that aren't locked
            var query = QueryProvider.Delete<SqlLock>()
                                        .Where(w => w.Column(c => c.LockedBy).IsNull
                                                    // Add extra filter on last lock date when inactive is set
                                                    .When(inactiveTime.HasValue, b => b.And.WhereGroup(g => g.Column(c => c.LastLockDate).IsNull.Or.Column(c => c.LastLockDate).LesserThan.ModifyDate(b => b.CurrentDate(), -inactiveTime.Value, DateInterval.Millisecond)))
                                                    .And.Not().ExistsIn(
                                                                        QueryProvider.Select<SqlLockRequest>().Where(w => w.Column(c => c.Resource).EqualTo.Column<SqlLock>(c => c.Resource))
                                                                        )
                                                )
                                        .Build(_queryOptions);

            _logger.Trace($"Deleting all {(inactiveTime.HasValue ? "inactive" : "free")} locks using query {query}");
            var deleted = await dbConnection.ExecuteAsync(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);
            _logger.Log($"Deleted <{deleted}> {(inactiveTime.HasValue ? "inactive" : "free")} locks");
            return deleted;
        }
        /// <inheritdoc/>
        public virtual async Task<SqlLockRequest[]> GetAllLockRequestsByResourceAsync(IRepositoryTransaction transaction, string resource, CancellationToken token)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Fetching all lock requests for resource <{resource}>");
            var query = QueryProvider.GetQuery(_queryNameFormat.FormatString(nameof(GetAllLockRequestsByResourceAsync)), p => p.Select<SqlLockRequest>().All()
                                                                                                                                .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)))
                                                                                                                                .OrderBy(c => c.CreatedAt, SortOrders.Ascending));
            _logger.Trace($"Fetching all lock requests for resource <{resource}> using <{query}>");
            var parameters = new DynamicParameters();
            parameters.Add($"@{nameof(resource)}", resource);

            var results = (await dbConnection.QueryAsync<SqlLockRequest>(new CommandDefinition(query, parameters, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false)).Select(x => x.SetToLocal()).ToArray();
            _logger.Log($"Fetched <{results.Length}> lock requests for resource <{resource}>");
            return results;
        }
        /// <inheritdoc/>
        public virtual async Task<int> GetLockAmountAsync(IRepositoryTransaction transaction, CancellationToken token = default)
        {
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);
            _logger.Log($"Getting the total amount of locks");
            var query = QueryProvider.GetQuery(_queryNameFormat.FormatString(nameof(GetLockAmountAsync)), p => p.Select<SqlLock>().CountAll());

            _logger.Trace($"Getting the total amount of locks using query <{query}>");

            var count = await dbConnection.ExecuteScalarAsync<int>(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);
            _logger.Log($"Total amount of locks is <{count}>");
            return count;
        }
        /// <inheritdoc/>
        public virtual async Task<(bool WasUnlocked, SqlLock CurrentState)> TryUnlockAsync(IRepositoryTransaction transaction, string resource, string requester, CancellationToken token)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Trying to unlock resource <{resource}> for <{requester}>");
            var query = QueryProvider.GetQuery(_queryNameFormat.FormatString(nameof(TryUnlockAsync)), p =>
            {
                var multiBuilder = p.New();

                // Try update first
                multiBuilder.Append(p.Update<SqlLock>()
                                        .Set.Column(c => c.LockedBy).To.Null()
                                        .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))
                                                    .And.Column(c => c.LockedBy).EqualTo.Parameter(nameof(requester))));
                // Select latest state
                multiBuilder.Append(p.Select<SqlLock>()
                                     .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))));

                return multiBuilder;
            });

            _logger.Trace($"Trying to unlock resource <{resource}> for <{requester}> using query <{query}>");
            var parameters = new DynamicParameters();
            parameters.Add($"@{nameof(resource)}", resource);
            parameters.Add($"@{nameof(requester)}", requester);

            var currentLock = await dbConnection.QuerySingleOrDefaultAsync<SqlLock>(new CommandDefinition(query, parameters, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);
            bool wasUnlocked = currentLock?.LockedBy == null;

            _logger.Log($"Resource <{resource}> was {(wasUnlocked ? "unlocked" : "not unlocked")} by <{requester}>");
            return (wasUnlocked, currentLock?.SetToLocal());
        }
        /// <inheritdoc/>
        public virtual async Task<SqlLock> TryUpdateExpiryDateAsync(IRepositoryTransaction transaction, string resource, string requester, TimeSpan extendTime, CancellationToken token)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            requester.ValidateArgumentNotNullOrWhitespace(nameof(requester));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Trying to update expiry time for lock on resource <{resource}> for <{requester}>");
            var query = QueryProvider.GetQuery(_queryNameFormat.FormatString(nameof(TryUpdateExpiryDateAsync)), p =>
            {
                var multiBuilder = p.New();

                // Try update
                multiBuilder.Append(p.Update<SqlLock>()
                                        .Set.Column(c => c.ExpiryDate).To.Case(ca =>
                                                                            ca.When(w => w.Column(c => c.ExpiryDate).IsNull)
                                                                                .Then.ModifyDate(b => b.CurrentDate(DateType.Utc), b => b.Parameter(nameof(extendTime)), DateInterval.Millisecond)
                                                                              .Else.ModifyDate(b => b.Column(c => c.ExpiryDate), b => b.Parameter(nameof(extendTime)), DateInterval.Millisecond))
                                        .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))
                                                    .And.Column(c => c.LockedBy).EqualTo.Parameter(nameof(requester))));

                // Select latest state
                multiBuilder.Append(p.Select<SqlLock>()
                                     .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))));

                return multiBuilder;
            });

            _logger.Trace($"Trying to update expiry time for lock on resource <{resource}> for <{requester}> using query <{query}>");
            var parameters = new DynamicParameters();
            parameters.Add($"@{nameof(resource)}", resource);
            parameters.Add($"@{nameof(requester)}", requester);
            parameters.Add($"@{nameof(extendTime)}", extendTime.TotalMilliseconds);

            var currentLock = await dbConnection.QuerySingleOrDefaultAsync<SqlLock>(new CommandDefinition(query, parameters, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);

            bool wasUpdated = requester.Equals(currentLock?.LockedBy, StringComparison.OrdinalIgnoreCase);

            if (wasUpdated) _logger.Log($"Expiry date on resource <{resource}> held by <{requester}> has been extended to <{currentLock?.ExpiryDate}>");
            else _logger.Warning($"Resource <{resource}> is no longer held by <{requester}> so can't extend expiry date. Resource is currently held by <{currentLock?.LockedBy}>");

            return currentLock?.SetToLocal();
        }
        /// <inheritdoc/>
        public async virtual Task<long[]> GetDeletedRequestIds(IRepositoryTransaction transaction, long[] ids, CancellationToken token)
        {
            ids.ValidateArgument(nameof(ids));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Log($"Checking which of the <{ids.Length}> lock requests have been removed");

            var query = QueryProvider.Select<SqlLockRequest>()
                                        .Column(c => c.Id)
                                        .Where(w => w.Column(c => c.Id).In.Values(ids))
                                        .Build(_queryOptions);
            _logger.Trace($"Checking which of the <{ids.Length}> lock requests have been removed using query <{query}>");

            var existingIds = (await dbConnection.QueryAsync<long>(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false)).ToArray();
            var removedIds = ids.Where(x => !existingIds.Contains(x)).ToArray();

            _logger.Log($"<{removedIds.Length}> lock requests out of the <{ids.Length}> have been removed");
            return removedIds;
        }
        /// <inheritdoc/>
        public async virtual Task ForceUnlockAsync(IRepositoryTransaction transaction, string resource, bool removePendingRequests, CancellationToken token = default)
        {
            resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            // Generate query
            _logger.Warning($"Forcefully removing lock owner on resource <{resource}>{(removePendingRequests ? " and any pending requests" : string.Empty)}");

            var queryBuilder = QueryProvider.New()
                                     .Append(QueryProvider.Update<SqlLock>()
                                                          .Set.Column(c => c.LockedBy).To.Null()
                                                          .Set.Column(c => c.LockedAt).To.Null()
                                                          .Set.Column(c => c.ExpiryDate).To.Null()
                                                          .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource)))
                                            );

            if (removePendingRequests)
            {
                queryBuilder.Append(QueryProvider.Delete<SqlLockRequest>()
                                                 .Where(w => w.Column(c => c.Resource).EqualTo.Parameter(nameof(resource))));
            }

            var query = queryBuilder.Build(_queryOptions);
            _logger.Trace($"Forcefully removing lock owner on resource <{resource}>{(removePendingRequests ? " and any pending requests" : string.Empty)} using query <{query}>");
            var parameters = new DynamicParameters();
            parameters.Add($"@{nameof(resource)}", resource);

            // Execute query
            await dbConnection.ExecuteAsync(new CommandDefinition(query, parameters, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);
            _logger.Warning($"Forcefully removed lock owner on resource <{resource}>{(removePendingRequests ? " and any pending requests" : string.Empty)}");
        }

        /// <inheritdoc/>
        public async virtual Task ClearAllAsync(IRepositoryTransaction transaction, CancellationToken token = default)
        {
            var (dbConnection, dbTransaction) = GetTransactionInfo(transaction);

            _logger.Warning($"Clearing table <{SqlLockTableName}> and <{SqlLockRequestTableName}>");
            var query = QueryProvider.GetQuery(_queryNameFormat.FormatString(nameof(ClearAllAsync)), p =>
            {
                return p.New()
                        .Append(p.Delete<SqlLockRequest>())
                        .Append(p.Delete<SqlLock>());
            });

            _logger.Trace($"Clearing table <{SqlLockTableName}> and <{SqlLockRequestTableName}> using query <{query}>");

            var deleted = await dbConnection.ExecuteAsync(new CommandDefinition(query, transaction: dbTransaction, cancellationToken: token)).ConfigureAwait(false);
            _logger.Log($"Cleared <{deleted}> locks and requests");
        }
        /// <inheritdoc/>
        public abstract Task<(SqlLock[] Results, int TotalMatching)> SearchAsync(IRepositoryTransaction transaction, SqlQuerySearchCriteria searchCriteria, CancellationToken token = default);
        /// <inheritdoc/>
        public abstract Task<SqlLock> TryLockAsync(IRepositoryTransaction transaction, string resource, string requester, DateTime? expiryDate, CancellationToken token);

        /// <summary>
        /// Returns the connection and transaction contained in <paramref name="transaction"/>.
        /// </summary>
        /// <param name="transaction">The transaction to get the info from</param>
        /// <returns>The connection and transaction contained in <paramref name="transaction"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected (IDbConnection Connection, IDbTransaction Transaction) GetTransactionInfo(IRepositoryTransaction transaction)
        {
            transaction.ValidateArgument(nameof(transaction));

            var (connection, dbTransaction) = GetRepositoryTransactionInfo(transaction);
            if (connection == null) throw new InvalidOperationException($"{nameof(GetRepositoryTransactionInfo)} did not return a connection");
            if (dbTransaction == null) throw new InvalidOperationException($"{nameof(GetRepositoryTransactionInfo)} did not return a transaction");
            return (connection, dbTransaction);
        }

        // Virtuals
        /// <summary>
        /// Raised when a query builder is created by the current repository.
        /// </summary>
        /// <param name="queryBuilder">The builder that was created</param>
        protected void OnBuilderCreated(IQueryBuilder queryBuilder)
        {
            queryBuilder.ValidateArgument(nameof(queryBuilder));

            // Globally set aliases
            if (queryBuilder is IAliasQueryBuilder aliasBuilder)
            {
                aliasBuilder.SetAlias<SqlLock>(SqlLockAlias);
                aliasBuilder.SetAlias<SqlLockRequest>(SqlLockRequestAlias);
            }

            queryBuilder.OnCompiling(x =>
            {
                if (x is TableExpression tableExpression)
                {
                    // Set schema
                    tableExpression.SetSchema(Schema);

                    // Overwrite table names
                    if (tableExpression?.Alias?.Set is Type sqlLockType && sqlLockType.Is<SqlLock>())
                    {
                        tableExpression.SetTableName(SqlLockTableName);
                    }
                    else if (tableExpression?.Alias?.Set is Type sqlLockRequestType && sqlLockRequestType.Is<SqlLockRequest>())
                    {
                        tableExpression.SetTableName(SqlLockRequestTableName);
                    }
                }
            });
        }

        // Abstractions
        /// <summary>
        /// Returns the connection and transaction from <paramref name="transaction"/>.
        /// </summary>
        /// <param name="transaction">The transaction to get the connection and transaction for</param>
        /// <returns>The connection and transaction included in <paramref name="transaction"/></returns>
        protected abstract (IDbConnection Connection, IDbTransaction Transaction) GetRepositoryTransactionInfo(IRepositoryTransaction transaction);

        /// <inheritdoc/>
        public abstract Task TryAssignLockRequestsAsync(IRepositoryTransaction transaction, CancellationToken token);
        /// <inheritdoc/>
        public abstract Task<SqlLockRequest> CreateRequestAsync(IRepositoryTransaction transaction, SqlLockRequest request, CancellationToken token);
        /// <inheritdoc/>
        public abstract Task<IRepositoryTransaction> CreateTransactionAsync(CancellationToken token);
    }
}

