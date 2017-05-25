using System.ComponentModel.DataAnnotations;
using DapperDan.Caching;

namespace DapperDan.Tests.Models
{
	public class Card
	{
		[Key]
		public int Id { get; set; }

		[FullTextKey]
		public string Name { get; set; }

		[FullTextKey]
		public string Text { get; set; }

		/// <summary>
		/// In the real world this would be an enum
		/// </summary>
		public int CardType { get; set; }

		public int Cost { get; set; }
	}
}
