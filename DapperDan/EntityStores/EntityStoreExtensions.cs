using System;
using System.Configuration;

namespace DapperDan.EntityStores
{
	public static class EntityStoreExtensions
	{
		public static IEntityStore WithConfigConnection(this IEntityStore store, string configConnectionStringName)
		{
			if (store == null || string.IsNullOrWhiteSpace(configConnectionStringName))
				throw new ArgumentNullException("Either the store or the configuration name is null.");

			store.WithConnection(ConfigurationManager.ConnectionStrings[configConnectionStringName].ConnectionString);

			return store;
		}
	}
}
