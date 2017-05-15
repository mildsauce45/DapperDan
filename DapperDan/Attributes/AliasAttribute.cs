using System;

namespace DapperDan.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class AliasAttribute : Attribute
	{
		public string Alias { get; }

		public AliasAttribute(string alias)
		{
			Alias = alias;
		}
	}
}
