using Dapper;

namespace DapperDan.EntityStores.QueryExecution
{
	internal class DeleteQueryBuilder : QueryBuilderBase
	{
		public DeleteQueryBuilder(ConnectionInfo connectionInfo)
			: base(connectionInfo)
		{
		}

		public string BuildQuery(object entityKey, DynamicParameters parameters)
		{
			parameters.Add(ConnectionInfo.KeyColumnName.ToUpper(), entityKey);

			return $"delete from {ConnectionInfo.TableName} where {ConnectionInfo.KeyColumnName} = @{ConnectionInfo.KeyColumnName.ToUpper()}";
		}
	}
}
