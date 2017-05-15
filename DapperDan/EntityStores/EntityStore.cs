using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Data.SqlClient;
using System.Globalization;
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

		public async Task<IEnumerable<TEntity>> GetAsync<TEntity>()
		{
			var parameters = new DynamicParameters();
			var sql = new SelectQueryBuilder(connectionInfo, filters).BuildQuery(parameters);

			return await GetAsync<TEntity>(sql, parameters);
		}

		public async Task<IEnumerable<TEntity>> GetAsync<TEntity>(string sql, DynamicParameters parameters)
		{
			EnsureConnectionString();

			using (var db = new SqlConnection(connectionInfo.ConnectionString))
			{
				parameters = parameters ?? new DynamicParameters();

				var result = await db.QueryAsync<TEntity>(sql, parameters);

				return result;
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

		#endregion
	}
}
