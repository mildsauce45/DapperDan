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
			
			var keyColumnName = GetKeyColumnName();

			var properties = toUpdate.GetType().GetProperties();

			parameters.Add("UPDATE_RECORD_KEY", properties.FirstOrDefault(pi => pi.Name == keyColumnName).GetValue(toUpdate));

			var temp = new List<string>();

			foreach (var cfu in columnsForUpdate)
			{
				var pi = properties.FirstOrDefault(p => p.Name == cfu.DbColumnName || p.Name == cfu.EntityName);
				if (pi == null)
					continue;

				temp.Add($"{cfu.ResolvedName} = @{cfu.ResolvedName.ToUpper()}");
				parameters.Add(cfu.ResolvedName.ToUpper(), pi.GetValue(toUpdate));
			}

			var columnsClause = string.Join(",", temp);

			return $"update {ConnectionInfo.TableName} set {columnsClause} where {keyColumnName} = @UPDATE_RECORD_KEY";
		}
	}
}
