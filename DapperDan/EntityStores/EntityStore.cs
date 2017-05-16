using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DapperDan.EntityStores.QueryExecution;
using DapperDan.Filtering;

namespace DapperDan.EntityStores
{
	public class EntityStore : IEntityStore
	{
		private ConnectionInfo connectionInfo;
		private IList<ColumnFilter> filters;
		private PagingInfo pagingInfo;

		public async Task<IEnumerable<TEntity>> GetAsync<TEntity>()
		{
			var parameters = new DynamicParameters();
			var sql = new SelectQueryBuilder(connectionInfo, filters, pagingInfo).BuildQuery(parameters);

			return await GetAsync<TEntity>(sql, parameters);
		}

		public async Task<IEnumerable<TEntity>> GetAsync<TEntity>(string sql, DynamicParameters parameters)
		{
			EnsureConnectionString();

			using (var db = new SqlConnection(connectionInfo.ConnectionString))
			{
				parameters = parameters ?? new DynamicParameters();

				try
				{
					var result = await db.QueryAsync<TEntity>(sql, parameters);

					return result;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}

			return Enumerable.Empty<TEntity>();
		}

		public async Task<TEntity> AddAsync<TEntity>(TEntity newRow) where TEntity : new()
		{
			if (newRow == null)
				return default(TEntity);

			var parameters = new DynamicParameters();
			var sql = new InsertQueryBuilder(connectionInfo).BuildQuery(newRow, parameters);

			var result = (await GetAsync<TEntity>(sql, parameters)).FirstOrDefault();

			if (result == null)
				return default(TEntity);

			WithFilter(connectionInfo.KeyColumnName, result.GetType().GetProperty(connectionInfo.KeyColumnName).GetValue(result));

			return (await GetAsync<TEntity>()).FirstOrDefault();
		}

		public async Task<TEntity> UpdateAsync<TEntity>(TEntity toUpdate)
		{
			if (toUpdate == null)
				return default(TEntity);

			EnsureConnectionString();

			var parameters = new DynamicParameters();
			var sql = new UpdateQueryBuilder(connectionInfo).BuildQuery(toUpdate, parameters);

			using (var db = new SqlConnection(connectionInfo.ConnectionString))
			{
				await db.ExecuteAsync(sql, parameters);
			}

			return toUpdate;
		}

		public async Task DeleteAsync<TEntity>(TEntity entity)
		{
			if (entity == null)
				return;

			var entityKey = typeof(TEntity).GetProperty(connectionInfo.KeyColumnName).GetValue(entity);

			await DeleteAsync(entityKey);
		}

		public async Task DeleteAsync(object entityKey)
		{
			var parameters = new DynamicParameters();
			var sql = new DeleteQueryBuilder(connectionInfo).BuildQuery(entityKey, parameters);

			using (var db = new SqlConnection(connectionInfo.ConnectionString))
			{
				await db.ExecuteAsync(sql, parameters);
			}
		}

		private void EnsureConnectionString()
		{
			if (connectionInfo == null || string.IsNullOrWhiteSpace(connectionInfo.ConnectionString))
				throw new InvalidOperationException("Cannot connect to a database. No connection string provided");
		}

		#region Composition Methods

		public IEntityStore WithConnection(string connectionString)
		{
			connectionInfo = (connectionInfo ?? new ConnectionInfo()).WithConnection(connectionString);

			return this;
		}

		public IEntityStore WithEntity<TEntity>(string alias = null)
		{
			connectionInfo = (connectionInfo ?? new ConnectionInfo())
				.WithTableName(alias ?? PluralizationService.CreateService(CultureInfo.CurrentCulture).Pluralize(typeof(TEntity).Name))
				.WithEntity<TEntity>();

			return this;
		}

		public IEntityStore WithFilter(string propName, object value, FilterOperation operation = FilterOperation.Equals)
		{
			if (filters == null)
				filters = new List<ColumnFilter>();

			filters.Add(new ColumnFilter(propName, value, operation));

			return this;
		}

		public IEntityStore WithSort(string propName, SortDirection direction = SortDirection.Ascending)
		{
			pagingInfo = (pagingInfo ?? new PagingInfo()).WithSort(propName, direction);
			return this;
		}

		public IEntityStore WithPaging(int? skip, int? take)
		{
			pagingInfo = (pagingInfo ?? new PagingInfo()).WithSkip(skip).WithTake(take);
			return this;
		}

		#endregion
	}
}
