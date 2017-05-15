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

		[QueryType(QueryTypes.Read)]
		public DateTime CreatedDate { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }
	}
}
