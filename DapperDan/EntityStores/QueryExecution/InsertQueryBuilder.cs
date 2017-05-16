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
	}
}
