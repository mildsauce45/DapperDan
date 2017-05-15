using DapperDan.Attributes;

namespace DapperDan.EntityStores
{
	internal class ColumnInfo
	{
		public string EntityName { get; }
		public string DbColumnName { get; }
		public QueryTypes QueryTypes { get; }

		internal ColumnInfo(string entityName, string dbColumnName = null, QueryTypes queryTypes = QueryTypes.All)
		{
			EntityName = entityName;
			DbColumnName = dbColumnName;
			QueryTypes = queryTypes;
		}
	}
}
