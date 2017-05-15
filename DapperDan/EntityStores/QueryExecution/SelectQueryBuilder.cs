using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperDan.Attributes;
using DapperDan.Filtering;

namespace DapperDan.EntityStores.QueryExecution
{
	internal class SelectQueryBuilder : QueryBuilderBase
	{
		private IEnumerable<ColumnFilter> filters;

		internal SelectQueryBuilder(ConnectionInfo connectionInfo, IEnumerable<ColumnFilter> filters)
			: base(connectionInfo)
		{
			this.filters = filters;
		}

		public string BuildQuery(DynamicParameters parameters)
		{
			var baseClause = $"select {GetSelectableColumns(ConnectionInfo.Columns)} from {ConnectionInfo.TableName}";
			string filterClause = string.Empty;

			if (filters != null && filters.Any())
				filterClause = BuildFilterClause(filters, ConnectionInfo.Columns, parameters);

			return baseClause + filterClause;
		}

		private string GetSelectableColumns(IEnumerable<ColumnInfo> columns)
		{
			if (columns == null || !columns.Any())
				return "*";

			return string.Join(",", GetColumnsForOperation(QueryTypes.Read).Select(ci => ci.DbColumnName != null ? $"{ci.DbColumnName} as {ci.EntityName}" : ci.EntityName));
		}

		private string BuildFilterClause(IEnumerable<ColumnFilter> filters, IEnumerable<ColumnInfo> columns, DynamicParameters parameters)
		{
			var filterStrs = new List<string>();

			foreach (var filter in filters)
			{
				var column = columns.FirstOrDefault(ci => ci.EntityName == filter.EntityName);
				var filterColumn = column.DbColumnName ?? column.EntityName;

				filterStrs.Add($"{filterColumn} {GetStringOperation(filter.Operation)} @{filterColumn.ToUpper()}");

				parameters.Add(filterColumn.ToUpper(), filter.Value);
			}

			return $" where {string.Join(" and ", filterStrs)}";
		}

		private string GetStringOperation(FilterOperation operation)
		{
			switch (operation)
			{
				case FilterOperation.Equals:
					return "=";
				case FilterOperation.GreaterThan:
					return ">";
				case FilterOperation.LessThan:
					return "<";
				case FilterOperation.NotEquals:
					return "<>";
				case FilterOperation.GreatherThanOrEquals:
					return ">=";
				case FilterOperation.LessThanOrEquals:
					return "<=";
				case FilterOperation.In:
					return "in";
			}

			throw new ArgumentException($"Unknown filter operation {operation}");
		}
	}
}
