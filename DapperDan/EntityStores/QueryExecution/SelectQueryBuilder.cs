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
		private PagingInfo pagingInfo;

		internal SelectQueryBuilder(ConnectionInfo connectionInfo, IEnumerable<ColumnFilter> filters, PagingInfo pagingInfo)
			: base(connectionInfo)
		{
			this.filters = filters;
			this.pagingInfo = pagingInfo;
		}

		public string BuildQuery(DynamicParameters parameters)
		{
			string orderByClause = string.Empty;

			if ((pagingInfo?.Sorts.Any()).GetValueOrDefault())
				orderByClause = BuildOrderByClause();

			if (pagingInfo != null && pagingInfo.Skip.GetValueOrDefault() > 0 && pagingInfo.Take.HasValue)
			{
				var skipTakePredicate = BuildSkipTakePredicate();
				var orderedSetName = $"Ordered{ConnectionInfo.TableName}";
				var originalSetName = $"Orig{ConnectionInfo.TableName}";

				return string.Format("select {0} from (select {1}, row_number() over ({2}) as RowNumber from (select {1} from {3} {4}) as {5}) as {6} where {7}",
					GetSelectableColumns(),
					GetSelectableColumns(false),
					orderByClause,
					ConnectionInfo.TableName,
					BuildPredicate(filters, ConnectionInfo.Columns, parameters),
					orderedSetName,
					orderedSetName,
					skipTakePredicate);
			}
			else
			{
				return string.Format("select {0} {1} from {2} {3} {4}",
					pagingInfo != null && pagingInfo.Take.HasValue ? $"top {pagingInfo.Take}" : string.Empty,
					GetSelectableColumns(),
					ConnectionInfo.TableName,
					BuildPredicate(filters, ConnectionInfo.Columns, parameters),
					orderByClause);
			}
		}

		private string GetSelectableColumns(bool useAlias = true)
		{
			if (ConnectionInfo.Columns == null || !ConnectionInfo.Columns.Any())
				return "*";

			var columns = new List<string>();

			foreach (var ci in GetColumnsForOperation(QueryTypes.Read))
			{
				if (ci.DbColumnName != null)
					columns.Add($"{ci.DbColumnName} {(useAlias ? "as " + ci.EntityName : ci.DbColumnName)}".Trim());
				else
					columns.Add(ci.EntityName);
			}

			return string.Join(",", columns);
		}

		private string BuildPredicate(IEnumerable<ColumnFilter> filters, IEnumerable<ColumnInfo> columns, DynamicParameters parameters)
		{
			if (filters == null || !filters.Any())
				return string.Empty;

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

		private string BuildOrderByClause()
		{
			return $" order by {string.Join(",", pagingInfo.Sorts.Select(s => string.Format("{0} {1}", s.Item1, s.Item2 == SortDirection.Descending ? "desc" : string.Empty).Trim()))}";
		}

		private string BuildSkipTakePredicate()
		{
			var skip = pagingInfo.Skip.Value;
			var take = pagingInfo.Take.Value;

			var maxRow = take + skip;

			return $" RowNumber > {skip} and RowNumber <= {maxRow}";
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
