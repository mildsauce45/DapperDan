using System;
using System.Linq;
using System.Threading.Tasks;
using DapperDan.EntityStores;
using DapperDan.Tests.Models;
using FluentAssertions;
using Xunit;

namespace DapperDan.Tests.SQLServer
{
	public class CRUDMethodTests
	{
		[Fact]
		public async Task TestGetNoFilter()
		{
			var users = await CreateTestStore().GetAsync<User>();

			users.Should().HaveCount(2);
		}

		[Fact]
		public async Task TestGetUsersWithFilter()
		{
			var users = await CreateTestStore().WithFilter(nameof(User.FirstName), "Jared").GetAsync<User>();

			users.Should().HaveCount(1);
			users.First().FamilyName.ShouldBeEquivalentTo("Pinchot");
		}

		[Fact]
		public async Task TestInsertDelete()
		{
			var newUser = new User
			{
				UserName = "odran",
				CreatedDate = DateTime.Now,
				FirstName = "Omin",
				FamilyName = "Dran"
			};

			newUser = await CreateTestStore().AddAsync(newUser);

			newUser.Id.Should().BeGreaterThan(0);

			var deleteGetStore = CreateTestStore();

			await deleteGetStore.DeleteAsync(newUser);

			var users = await deleteGetStore.WithFilter(nameof(User.FirstName), "Omin").GetAsync<User>();

			users.Should().HaveCount(0);
		}

		[Fact]
		public async Task TestUpdate()
		{
			var store = CreateTestStore().WithFilter(nameof(User.FamilyName), "Darkmagic");

			var users = await store.GetAsync<User>();

			users.Should().HaveCount(1);

			var jd = users.First();
			jd.FirstName = "James";
			
			var previousTime = jd.CreatedDate;
			jd.CreatedDate = DateTime.Now;

			await store.UpdateAsync(jd);

			users = await store.GetAsync<User>();

			users.Should().HaveCount(1);
			users.First().FirstName.ShouldBeEquivalentTo("James");
			users.First().CreatedDate.ShouldBeEquivalentTo(previousTime, "this field is marked as not updatable on the model");

			// Let's undo what we've done
			jd.FirstName = "Jim";

			await store.UpdateAsync(jd);

			users = await store.GetAsync<User>();

			users.Should().HaveCount(1);
			users.First().FirstName.ShouldBeEquivalentTo("Jim");
		}

		private IEntityStore CreateTestStore() =>
			new EntityStore().WithConfigConnection("Core").WithEntity<User>();
	}
}
