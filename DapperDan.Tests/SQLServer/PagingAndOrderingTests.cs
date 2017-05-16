using System.Linq;
using System.Threading.Tasks;
using DapperDan.EntityStores;
using DapperDan.Tests.Models;
using FluentAssertions;
using Xunit;

namespace DapperDan.Tests.SQLServer
{
	public class PagingAndOrderingTests
	{
		[Fact]
		public async Task TestOrdering()
		{
			var users = await CreateTestStore().WithSort(nameof(User.UserName)).GetAsync<User>();

			users.Should().HaveCount(2);
			users.First().UserName.ShouldBeEquivalentTo("jdarkmagic", "because j comes before m");
			users.ElementAt(1).UserName.ShouldBeEquivalentTo("mildsauce45", "because m comes after j");

			users = await CreateTestStore().WithSort(nameof(User.UserName), SortDirection.Descending).GetAsync<User>();

			users.Should().HaveCount(2);
			users.First().UserName.ShouldBeEquivalentTo("mildsauce45", "because m comes before j when sorting descending");
			users.ElementAt(1).UserName.ShouldBeEquivalentTo("jdarkmagic", "because j comes after m when sorting descending");
		}

		[Fact]
		public async Task TestSortingAliasedColumn()
		{
			var users = await CreateTestStore().WithSort(nameof(User.FamilyName)).GetAsync<User>();

			users.Should().HaveCount(2);
			users.First().FamilyName.ShouldBeEquivalentTo("Darkmagic", "because d comes before p");
		}

		[Fact]
		public async Task TestPaging()
		{
			var users = await CreateTestStore().WithSort(nameof(User.UserName)).WithPaging(1, 1).GetAsync<User>();

			users.Should().HaveCount(1);
			users.First().UserName.ShouldBeEquivalentTo("mildsauce45", "because our page size of 1 skips over jdarkmagic");
			users.First().FamilyName.ShouldBeEquivalentTo("Pinchot", "because the aliasing should still work");
		}

		private IEntityStore CreateTestStore() =>
			new EntityStore().WithConfigConnection("Core").WithEntity<User>();
	}
}
