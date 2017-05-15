using System;
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
				.WithConfigConnection("Core")
				.WithEntity<User>()
				.WithFilter(nameof(User.FirstName), "Jared");

			var uTask = userStore.GetAsync<User>();

			Task.WaitAll(uTask);

			var users = uTask.Result;

			//TestInsertUser();
			Console.WriteLine("");
		}

		private static void TestInsertUser()
		{
			var u = new User();
			u.FirstName = "Jim";
			u.FamilyName = "Darkmagic";
			u.UserName = "jdarkmagic";
			u.CreatedDate = DateTime.Now;

			IEntityStore userStore = new EntityStore()
				.WithConfigConnection("Core")
				.WithEntity<User>();

			var uTask = userStore.AddAsync(u);

			Task.WaitAll(uTask);

			var newUser = uTask.Result;

			Console.WriteLine("");
		}
	}
}
