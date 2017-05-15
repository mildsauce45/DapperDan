using System;
using System.Linq;
using Dapper;
using DapperDan.Attributes;

namespace DapperDan.EntityStores.QueryExecution
{
	internal class InsertQueryBuilder : QueryBuilderBase
	{
		public InsertQueryBuilder(ConnectionInfo connectionInfo)
			: base(connectionInfo)
		{
		}

		public string BuildQuery(object newRow, DynamicParameters parameters)
		{
			var columnsForInsert = GetColumnsForOperation(QueryTypes.Insert);

			var insertableColumns = string.Join(",", columnsForInsert.Select(ci => ci.ResolvedName));
			var keyColumnName = GetKeyColumnName();
			var columnValues = string.Join(",", columnsForInsert.Select(ci => $"@{(ci.ResolvedName).ToUpper()}"));

			var properties = newRow.GetType().GetProperties();
			
			foreach (var cfi in columnsForInsert)
			{
				var pi = properties.FirstOrDefault(p => p.Name == cfi.DbColumnName || p.Name == cfi.EntityName);
				if (pi == null)
					continue;

				parameters.Add(cfi.ResolvedName.ToUpper(), pi.GetValue(newRow));
			}

			return $"insert into {ConnectionInfo.TableName} ({insertableColumns}) output inserted.{keyColumnName} values ({columnValues})";
		}

		private string GetKeyColumnName()
		{
			if (string.IsNullOrWhiteSpace(ConnectionInfo.KeyColumnName))
				throw new InvalidOperationException("Cannot insert into a table without a primary key");

			var keyColumn = ConnectionInfo.Columns.FirstOrDefault(ci => ci.EntityName == ConnectionInfo.KeyColumnName);

			// For the moment I'm not going to handle alias key columns, as I think it's a very rare corner case, but I've left this
			// code in a semi-unrefactored state so that if the day comes that I must, it'll go here.
			return keyColumn.EntityName;
		}
	}
}
