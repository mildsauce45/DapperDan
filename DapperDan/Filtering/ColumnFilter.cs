namespace DapperDan.Filtering
{
	internal class ColumnFilter
	{
		public string EntityName { get; }
		public object Value { get; }
		public FilterOperation Operation { get; }

		internal ColumnFilter(string entityName, object value, FilterOperation operation = FilterOperation.Equals)
		{
			EntityName = entityName;
			Value = value;
			Operation = operation;
		}
	}
}
