using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Reflection;
using DapperDan.Attributes;

namespace DapperDan.EntityStores
{
	internal class ConnectionInfo
	{
		public string ConnectionString { get; private set; }
		public string TableName { get; private set; }

		public IEnumerable<ColumnInfo> Columns { get; private set; }

		internal string KeyColumnName { get; private set; }

		internal ConnectionInfo()
		{
		}

		internal ConnectionInfo WithConnection(string connectionString)
		{
			ConnectionString = connectionString;
			return this;
		}

		internal ConnectionInfo WithTableName(string tableName)
		{
			TableName = tableName;
			return this;
		}

		internal ConnectionInfo WithEntity<TEntity>()
		{
			var et = typeof(TEntity);

			TableName = PluralizationService.CreateService(CultureInfo.CurrentCulture).Pluralize(et.Name);

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
					KeyColumnName = entityName;
				}

				columns.Add(new ColumnInfo(entityName, alias, queryTypes));
			}

			Columns = columns;

			return this;
		}
	}
}
