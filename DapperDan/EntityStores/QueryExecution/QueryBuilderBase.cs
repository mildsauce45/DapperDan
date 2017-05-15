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
	}
}
