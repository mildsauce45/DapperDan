using System;
using System.Configuration;
using DapperDan.Caching;
using DapperDan.Filtering;
using DapperDan.Utilities;

namespace DapperDan.EntityStores
{
	public static class EntityStoreBuilderExtensions
	{
		public static EntityStore<TEntity> WithConnection<TEntity>(this EntityStore<TEntity> store, string connectionString)
		{
			ArgumentHelpers.ThrowIfNull(() => store);
			ArgumentHelpers.ThrowIfNullOrWhitespace(() => connectionString);

			store.ConnectionInfo.WithConnection(connectionString);

			return store;
		}

		public static EntityStore<TEntity> WithConfigConnection<TEntity>(this EntityStore<TEntity> store, string configConnectionStringName)
		{
			ArgumentHelpers.ThrowIfNull(() => store);
			ArgumentHelpers.ThrowIfNullOrWhitespace(() => configConnectionStringName);

			store.ConnectionInfo.WithConnection(ConfigurationManager.ConnectionStrings[configConnectionStringName].ConnectionString);

			return store;
		}

		public static EntityStore<TEntity> WithTableAlias<TEntity>(this EntityStore<TEntity> store, string tableAlias)
		{
			ArgumentHelpers.ThrowIfNull(() => store);
			ArgumentHelpers.ThrowIfNullOrWhitespace(() => tableAlias);

			store.ConnectionInfo.WithTableName(tableAlias);

			return store;
		}

		public static EntityStore<TEntity> WithFilter<TEntity>(this EntityStore<TEntity> store, string propName, object value, FilterOperation operation = FilterOperation.Equals)
		{
			ArgumentHelpers.ThrowIfNull(() => store);
			ArgumentHelpers.ThrowIfNullOrWhitespace(() => propName);

			store.Filters.Add(new ColumnFilter(propName, value, operation));

			return store;
		}

		public static EntityStore<TEntity> WithSort<TEntity>(this EntityStore<TEntity> store, string propName, SortDirection direction = SortDirection.Ascending)
		{
			ArgumentHelpers.ThrowIfNull(() => store);
			ArgumentHelpers.ThrowIfNullOrWhitespace(() => propName);

			store.PagingInfo.WithSort(propName, direction);

			return store;
		}

		public static EntityStore<TEntity> WithPaging<TEntity>(this EntityStore<TEntity> store, int? skip, int? take)
		{
			ArgumentHelpers.ThrowIfNull(() => store);

			store.PagingInfo.WithSkip(skip).WithTake(take);

			return store;
		}

		public static EntityStore<TEntity> WithCache<TEntity, TCache>(this EntityStore<TEntity> store) where TCache : ICache
		{
			ArgumentHelpers.ThrowIfNull(() => store);

			var cache = Activator.CreateInstance<TCache>();

			return WithCache(store, cache);			
		}

		/// <summary>
		/// Passing in null for cache will remove caching from this store
		/// </summary>
		public static EntityStore<TEntity> WithCache<TEntity>(this EntityStore<TEntity> store, ICache cache)
		{
			ArgumentHelpers.ThrowIfNull(() => store);

			store.Cache = cache;

			return store;
		}
	}
}
