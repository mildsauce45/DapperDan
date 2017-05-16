using System;
using System.Linq;
using System.Threading.Tasks;
using DapperDan.EntityStores;
using DapperDan.Filtering;
using DapperDan.Tests.Models;
using FluentAssertions;
using Xunit;

namespace DapperDan.Tests.SQLServer
{
	public class FilteringTests
	{
		[Fact]
		public async Task TestEqualsFilter()
		{
			var users = await CreateTestStore().WithFilter(nameof(User.FirstName), "Jared").GetAsync<User>();

			users.Should().HaveCount(1, "theres only one record with the first name of Jared");

			users = await CreateTestStore().WithFilter(nameof(User.Id), 0).GetAsync<User>();

			users.Should().HaveCount(0, "the users table should never have a record with 0 as the id");
		}

		[Fact]
		public async Task TestLessThanFilter()
		{
			var users = await CreateTestStore().WithFilter(nameof(User.Id), 7, FilterOperation.LessThan).GetAsync<User>();

			users.Should().HaveCount(2, "there are two records with an Id less than 7");

			users = await CreateTestStore().WithFilter(nameof(User.CreatedDate), new DateTime(2017, 5, 16), FilterOperation.LessThan).GetAsync<User>();

			users.Should().HaveCount(1, "only the mildsauce45 record was created before this date");
		}

		[Fact]
		public async Task TestGreaterThanFilter()
		{
			var store = CreateTestStore();

			var allUsers = await store.GetAsync<User>();

			var users = await store.WithFilter(nameof(User.CreatedDate), new DateTime(2017, 5, 16), FilterOperation.GreaterThan).GetAsync<User>();

			users.Should().HaveCount(allUsers.Count() - 1, "only one record was created before this time");
		}

		private IEntityStore CreateTestStore() =>
			new EntityStore().WithConfigConnection("Core").WithEntity<User>();
	}
}
