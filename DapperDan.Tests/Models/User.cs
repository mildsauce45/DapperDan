using System;
using System.ComponentModel.DataAnnotations;
using DapperDan.Attributes;

namespace DapperDan.Tests.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }

		public string UserName { get; set; }

		[QueryType(QueryTypes.Read | QueryTypes.Insert)]
		public DateTime CreatedDate { get; set; }

		public string FirstName { get; set; }

		[Alias("LastName")]
		public string FamilyName { get; set; }
	}
}
