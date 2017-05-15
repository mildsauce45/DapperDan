using DapperDan.Attributes;

namespace DapperDan.EntityStores
{
	internal class ColumnInfo
	{
		public string EntityName { get; }
		public string DbColumnName { get; }
		public QueryTypes QueryTypes { get; }

		/// <summary>
		/// Prefers the unaliased column name to the entity name
		/// </summary>
		public string ResolvedName => DbColumnName ?? EntityName;


		internal ColumnInfo(string entityName, string dbColumnName = null, QueryTypes queryTypes = QueryTypes.All)
		{
			EntityName = entityName;
			DbColumnName = dbColumnName;
			QueryTypes = queryTypes;
		}
	}
}
