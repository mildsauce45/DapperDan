using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using DapperDan.Filtering;

namespace DapperDan.EntityStores
{
	public interface IEntityStore
	{
		Task<IEnumerable<TEntity>> GetAsync<TEntity>();
		Task<IEnumerable<TEntity>> GetAsync<TEntity>(string sql, DynamicParameters parameters);

		Task<TEntity> AddAsync<TEntity>(TEntity newRow) where TEntity : new();

		Task<TEntity> UpdateAsync<TEntity>(TEntity toUpdate);

		IEntityStore WithEntity<TEntity>(string alias = null);
		IEntityStore WithFilter(string propName, object value, FilterOperation operation = FilterOperation.Equals);
		IEntityStore WithConnection(string connectionString);		
	}
}
