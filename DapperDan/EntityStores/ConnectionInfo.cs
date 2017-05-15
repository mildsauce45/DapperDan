using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperDan.Attributes;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace DapperDan.EntityStores
{
	internal class ConnectionInfo
	{
		public string ConnectionString { get; private set; }
		public string TableName { get; private set; }

		public IEnumerable<ColumnInfo> Columns { get; private set; }

		private string keyColumnName;

		public ConnectionInfo()
		{
		}

		public ConnectionInfo WithConnection(string connectionString)
		{
			ConnectionString = connectionString;
			return this;
		}

		public ConnectionInfo WithTableName(string tableName)
		{
			TableName = tableName;
			return this;
		}

		public ConnectionInfo WithEntity<TEntity>()
		{
			var et = typeof(TEntity);

			var properties = et.GetProperties();

			var columns = new List<ColumnInfo>();

			foreach (var pi in properties)
			{
				var entityName = pi.Name;
				string alias = null;
				QueryTypes queryTypes = QueryTypes.All;
				
				var aliasAttr = pi.GetCustomAttribute<AliasAttribute>();
				if (aliasAttr != null)
					alias = aliasAttr.Alias;

				var queryTypesAttr = pi.GetCustomAttribute<QueryTypeAttribute>();
				if (queryTypesAttr != null)
					queryTypes = queryTypesAttr.QueryTypes;

				var keyAttr = pi.GetCustomAttribute<KeyAttribute>();
				if (keyAttr != null || string.Equals("Id", entityName, StringComparison.CurrentCultureIgnoreCase) || string.Equals("Guid", entityName, StringComparison.CurrentCultureIgnoreCase))
				{
					queryTypes = QueryTypes.Read;
					keyColumnName = entityName;
				}

				columns.Add(new ColumnInfo(entityName, alias, queryTypes));
			}

			Columns = columns;

			return this;
		}
	}
}
