using System;
using System.Collections.Generic;
using System.Linq;
using DapperDan.Attributes;

namespace DapperDan.EntityStores.QueryExecution
{
	internal abstract class QueryBuilderBase
	{
		protected ConnectionInfo ConnectionInfo { get; private set; }

		public QueryBuilderBase(ConnectionInfo connectionInfo)
		{
			ConnectionInfo = connectionInfo;
		}

		protected IEnumerable<ColumnInfo> GetColumnsForOperation(QueryTypes queryType) =>
			ConnectionInfo.Columns.Where(ci => (ci.QueryTypes & queryType) > 0);

		protected string GetKeyColumnName()
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
