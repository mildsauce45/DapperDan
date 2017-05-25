using System;

namespace DapperDan.Caching
{
	[AttributeUsage(AttributeTargets.Property)]
	public class FullTextKeyAttribute : Attribute
	{
		public bool Tokenize { get; }

		public FullTextKeyAttribute()
			: this(true)
		{
		}

		public FullTextKeyAttribute(bool tokenize)
		{
			Tokenize = tokenize;
		}
	}
}
