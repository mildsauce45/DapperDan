using System.Linq;
using System.Threading.Tasks;
using DapperDan.Caching;
using DapperDan.EntityStores;
using DapperDan.Tests.Models;
using FluentAssertions;
using Xunit;

namespace DapperDan.Tests.Caching
{
	public class CachingTests
	{
		[Fact]
		public async Task TestFullTextSearch()
		{
			var testCache = new FullTextCache<Card>();

			await PrimeCache(testCache);

			var cards = await testCache.Search("white");

			cards.Should().NotBeNullOrEmpty();

			cards = await testCache.Search("creature");

			cards.Should().NotBeNullOrEmpty();
			cards.Should().HaveCount(3);
		}

		[Fact]
		public async Task TestFullTextSearchFromStore()
		{
			var testCache = new FullTextCache<Card>();

			await PrimeCache(testCache);

			var store = CreateTestStore().WithCache(testCache);

			var cards = await store.Search("CREATURE");

			cards.Should().NotBeNullOrEmpty();
		}

		[Fact]
		public async Task TestRemoveItem()
		{
			var testCache = new FullTextCache<Card>();

			await PrimeCache(testCache);

			var cards = await testCache.Get(new object[] { 9 });

			cards.Should().NotBeNull("because the cache should always be returning, at least, an empty dictionary.");
			cards.Should().NotBeEmpty("because their are at least nine items in the DB.");

			var card = cards.First().Value as Card;

			card.Name.ShouldBeEquivalentTo("Swamp", "because thats the 9th item being added");

			await testCache.Remove(9);

			cards = await testCache.Get(new object[] { 9 });

			cards.Should().NotBeNull("because the cache should always be returning, at least, an empty dictionary.");
			cards.Should().BeEmpty("because we removed it");			
		}

		private async Task PrimeCache(FullTextCache<Card> cache)
		{
			var cards = await CreateTestStore().GetAsync();

			await cache.Initialize(cards.ToDictionary(c => (object)c.Id, c => (object)c));
		}

		private EntityStore<Card> CreateTestStore() => new EntityStore<Card>().WithConfigConnection("Core");
	}
}
