using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DapperDan.Filtering;

namespace DapperDan.EntityStores
{
	public interface IEntityStore
	{
		Task<IEnumerable<TEntity>> GetAsync<TEntity>();
		Task<IEnumerable<TEntity>> GetAsync<TEntity>(string sql, DynamicParameters parameters);

		IEntityStore WithEntity<TEntity>(string alias = null);
		IEntityStore WithFilter(string propName, object value, FilterOperation operation = FilterOperation.Equals);
		IEntityStore WithConnection(string connectionString);		
	}
}
