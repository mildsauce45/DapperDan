using System;

namespace DapperDan.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class QueryTypeAttribute : Attribute
	{
		public QueryTypes QueryTypes { get; }

		public QueryTypeAttribute(QueryTypes queryTypes)
		{
			QueryTypes = queryTypes;
		}
	}

	[Flags]
	public enum QueryTypes
	{
		None = 0,

		Read = 1,
		Update = 2,
		Insert = 4,
		Delete = 8,

		All = Read | Update | Insert | Delete
	}
}
