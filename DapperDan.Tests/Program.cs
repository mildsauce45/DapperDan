using System;
using System.Linq;
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
				.WithEntity<User>();				

			var uTask = userStore.GetAsync<User>();

			Task.WaitAll(uTask);

			var users = uTask.Result;

			//TestInsertUser();
			TestUpdateUser(users.FirstOrDefault(u => u.Id == 5));
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

		private static void TestUpdateUser(User u)
		{
			u.FamilyName = "Fisher";

			IEntityStore userStore = new EntityStore()
				.WithConfigConnection("Core")
				.WithEntity<User>();

			var uTask = userStore.UpdateAsync(u);

			Task.WaitAll(uTask);

			var newUser = uTask.Result;

			Console.WriteLine("");
		}
	}
}
