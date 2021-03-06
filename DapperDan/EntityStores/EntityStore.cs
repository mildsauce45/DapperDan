﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DapperDan.Caching;
using DapperDan.EntityStores.QueryExecution;
using DapperDan.Filtering;
using DapperDan.Utilities;

namespace DapperDan.EntityStores
{
	public class EntityStore<TEntity>
	{
		internal ConnectionInfo ConnectionInfo { get; private set; }
		internal PagingInfo PagingInfo { get; private set; }

		internal IList<ColumnFilter> Filters { get; private set; }
		internal ICache Cache { get; set; }

		private Type entityType;

		public EntityStore()
		{
			entityType = typeof(TEntity);

			ConnectionInfo = new ConnectionInfo().WithEntity<TEntity>();
			PagingInfo = new PagingInfo();

			Filters = new List<ColumnFilter>();
		}

		#region CRUD Methods

		public async Task<IEnumerable<TEntity>> GetAsync()
		{
			var parameters = new DynamicParameters();
			var sql = new SelectQueryBuilder(ConnectionInfo, Filters, PagingInfo).BuildQuery(parameters);

			return await GetAsync<TEntity>(sql, parameters);
		}

		public async Task<IEnumerable<T>> GetAsync<T>(string sql, DynamicParameters parameters)
		{
			ArgumentHelpers.ThrowIfNullOrWhitespace(() => sql);

			EnsureConnectionString();

			using (var db = new SqlConnection(ConnectionInfo.ConnectionString))
			{
				parameters = parameters ?? new DynamicParameters();

				try
				{
					var result = await db.QueryAsync<T>(sql, parameters);

					return result;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}

			return Enumerable.Empty<T>();
		}

		public async Task<TEntity> AddAsync(TEntity newRow)
		{
			ArgumentHelpers.ThrowIfNull(() => newRow);

			var parameters = new DynamicParameters();
			var sql = new InsertQueryBuilder(ConnectionInfo).BuildQuery(newRow, parameters);

			var result = (await GetAsync<TEntity>(sql, parameters)).FirstOrDefault();

			if (result == null)
				return default(TEntity);

			object newKey = GetKey(result);

			this.WithFilter(ConnectionInfo.KeyColumnName, newKey);

			var newEntity = (await GetAsync()).FirstOrDefault();

			if (Cache != null)
				await Cache.Add(newKey, newEntity);

			return newEntity;
		}

		public async Task<TEntity> UpdateAsync(TEntity toUpdate)
		{
			ArgumentHelpers.ThrowIfNull(() => toUpdate);

			EnsureConnectionString();

			var parameters = new DynamicParameters();
			var sql = new UpdateQueryBuilder(ConnectionInfo).BuildQuery(toUpdate, parameters);

			using (var db = new SqlConnection(ConnectionInfo.ConnectionString))
			{
				await db.ExecuteAsync(sql, parameters);

				if (Cache != null)
					await Cache.Replace(GetKey(toUpdate), toUpdate);
			}

			return toUpdate;
		}

		public async Task DeleteAsync(TEntity entity)
		{
			ArgumentHelpers.ThrowIfNull(() => entity);

			var entityKey = typeof(TEntity).GetProperty(ConnectionInfo.KeyColumnName).GetValue(entity);

			await DeleteAsync(entityKey);
		}

		public async Task DeleteAsync(object entityKey)
		{
			ArgumentHelpers.ThrowIfNull(() => entityKey);

			var parameters = new DynamicParameters();
			var sql = new DeleteQueryBuilder(ConnectionInfo).BuildQuery(entityKey, parameters);

			using (var db = new SqlConnection(ConnectionInfo.ConnectionString))
			{
				await db.ExecuteAsync(sql, parameters);

				if (Cache != null)
					await Cache.Remove(entityKey);
			}
		}

		#endregion

		public async Task<IEnumerable<TEntity>> Search(string searchTerm)
		{
			if (Cache == null)
				return Enumerable.Empty<TEntity>();

			return (await Cache.Search(searchTerm)).OfType<TEntity>();
		}

		#region Private Helpers

		private void EnsureConnectionString()
		{
			if (ConnectionInfo == null || string.IsNullOrWhiteSpace(ConnectionInfo.ConnectionString))
				throw new InvalidOperationException("Cannot connect to a database. No connection string provided");
		}

		private object GetKey(TEntity entity) =>
			entityType.GetProperty(ConnectionInfo.KeyColumnName).GetValue(entity);


		#endregion
	}
}
