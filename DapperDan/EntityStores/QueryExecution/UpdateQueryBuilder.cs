using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DapperDan.Attributes;

namespace DapperDan.EntityStores.QueryExecution
{
	internal class UpdateQueryBuilder : QueryBuilderBase
	{
		internal UpdateQueryBuilder(ConnectionInfo connectionInfo)
			: base(connectionInfo)
		{
		}

		public string BuildQuery(object toUpdate, DynamicParameters parameters)
		{
			var columnsForUpdate = GetColumnsForOperation(QueryTypes.Update);

			//var 
			return string.Empty;
		}
	}
}
