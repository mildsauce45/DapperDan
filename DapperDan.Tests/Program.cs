using System;
using System.Configuration;
using System.Threading.Tasks;
using DapperDan.EntityStores;
using DapperDan.Tests.Models;

namespace DapperDan.Tests
{
	class Program
	{
		static void Main(string[] args)
		{
			IEntityStore userStore = new EntityStore()
				.WithConnection(ConfigurationManager.ConnectionStrings["Core"].ConnectionString)
				.WithEntity<User>()
				.WithFilter(nameof(User.FirstName), "Jim");

			var uTask = userStore.GetAsync<User>();

			Task.WaitAll(uTask);

			var users = uTask.Result;

			Console.WriteLine("");
		}
	}
}
