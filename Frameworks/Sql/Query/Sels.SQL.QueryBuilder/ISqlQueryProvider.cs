﻿using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder
{
    /// <summary>
    /// Provides fluent builders that can be used to create SQL queries.
    /// </summary>
    public interface ISqlQueryProvider
    {
        /// <summary>
        /// Returns a builder for creating an insert sql statement.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <returns>A fluent builder for creating an insert sql statement/returns>
        public IInsertStatementBuilder<T> Insert<T>();
        /// <summary>
        /// Returns a builder for creating an insert sql statement.
        /// </summary>
        /// <returns>A fluent builder for creating an insert sql statement/returns>
        public IInsertStatementBuilder<object> Insert() => Insert<object>();
        /// <summary>
        /// Returns a builder for creating a select sql statement.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <returns>A fluent builder for creating a select sql statement/returns>
        public ISelectStatementBuilder<T> Select<T>();
        /// <summary>
        /// Returns a builder for creating a select sql statement.
        /// </summary>
        /// <returns>A fluent builder for creating a select sql statement/returns>
        public ISelectStatementBuilder<object> Select() => Select<object>();
        /// <summary>
        /// Returns a builder for creating a sql statement using common table expressions.
        /// </summary>
        /// <returns>A fluent builder for creating a sql statement using common table expressions</returns>
        public ICteStatementBuilder With();
        /// <summary>
        /// Returns a builder for creating an update sql statement.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <returns>A fluent builder for creating an update sql statement/returns>
        public IUpdateStatementBuilder<T> Update<T>();
        /// <summary>
        /// Returns a builder for creating an update sql statement.
        /// </summary>
        /// <returns>A fluent builder for creating an update sql statement/returns>
        public IUpdateStatementBuilder<object> Update() => Update<object>();
        /// <summary>
        /// Returns a builder for creating a delete sql statement.
        /// </summary>
        /// <typeparam name="T">The main entity to query</typeparam>
        /// <returns>A fluent builder for creating a delete sql statement/returns>
        public IDeleteStatementBuilder<T> Delete<T>();
        /// <summary>
        /// Returns a builder for creating a delete sql statement.
        /// </summary>
        /// <returns>A fluent builder for creating a delete sql statement/returns>
        public IDeleteStatementBuilder<object> Delete() => Delete<object>();
        /// <summary>
        ///  Returns a builder for creating a query consisting of multiple statements and/or expressions.
        /// </summary>
        /// <returns>A fluent builder for creating a query consisting of multiple statements and/or expressions</returns>
        public IMultiStatementBuilder Build();
        /// <summary>
        /// Returns a builder for creating an IF ELSE sql statement.
        /// </summary>
        /// <returns>A fluent builder for creating an IF ELSE sql statement</returns>
        public IIfConditionStatementBuilder If();
        /// <summary>
        /// Returns a builder for declaring an sql variable.
        /// </summary>
        /// <returns>A fluent builder for dcelaring an sql variable</returns>
        public IVariableDeclarationRootStatementBuilder Declare();
        /// <summary>
        /// Returns a builder for creating an sql statement to assign a value to a sql variable.
        /// </summary>
        /// <returns>A fluent builder for creating an sql statement to assign a value to a sql variable</returns>
        public IVariableSetterRootStatementBuilder Set();
    }

    /// <summary>
    /// Extends <see cref="ISqlQueryProvider"/> with an option to cache named queries to improve query performance.
    /// </summary>
    public interface ICachedSqlQueryProvider : ISqlQueryProvider
    {
        /// <summary>
        /// Fetches query with name <paramref name="queryName"/>. If it does not exist yet it will be initialized with <paramref name="queryBuilder"/>.
        /// </summary>
        /// <param name="queryName">The name of the query to get. Must be globally unique</param>
        /// <param name="queryBuilder">Delegate that creates and returns the builder to generate the query</param>
        /// <returns></returns>
        public string GetQuery(string queryName, Func<ISqlQueryProvider, IQueryBuilder> queryBuilder);
        /// <summary>
        /// Fetches query with name <paramref name="queryName"/>. If it does not exist yet it will be initialized with <paramref name="queryBuilder"/>.
        /// </summary>
        /// <param name="queryName">The name of the query to get. Must be globally unique</param>
        /// <param name="queryBuilder">Delegate that returns the query string</param>
        /// <returns></returns>
        public string GetQuery(string queryName, Func<ISqlQueryProvider, string> queryBuilder);
    }
}
