using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DapperDan.Utilities;

namespace DapperDan.Caching
{
	public class FullTextCache<TItem> : ICache
	{
		private bool isPrimed;

		private Type itemType;
		private IEnumerable<Tuple<PropertyInfo, FullTextKeyAttribute>> propertiesToAdd;
		private static readonly char[] splitChars = new[] { ' ' };

		private SortedList<string, LinkedList<TItem>> fullTextCache;
		private Dictionary<TItem, List<string>> reverseKeyedIndex;
		private Dictionary<object, TItem> inMemoryCache;

		private SemaphoreSlim writeLock = new SemaphoreSlim(1);
		private SemaphoreSlim readLock = new SemaphoreSlim(1);

		public FullTextCache()
		{
			itemType = typeof(TItem);
			propertiesToAdd = itemType.GetProperties()
								.Where(pi => pi.GetCustomAttribute<FullTextKeyAttribute>(false) != null)
								.Select(pi => Tuple.Create(pi, pi.GetCustomAttribute<FullTextKeyAttribute>()));

			fullTextCache = new SortedList<string, LinkedList<TItem>>();
			reverseKeyedIndex = new Dictionary<TItem, List<string>>();
			inMemoryCache = new Dictionary<object, TItem>();
		}

		#region ICache Implementation

		public Task Add(object key, object item)
		{
			ArgumentHelpers.ThrowIfNull(() => key);
			ArgumentHelpers.ThrowIfNull(() => item);

			var type = item.GetType();

			if (type != itemType)
				throw new ArgumentException($"item is not of type {itemType}");

			// There are two possible actions here.
			// 1) The key being added doesnt exist in the case, in which case, we'll add it
			// 2) The key does exist, in which case, we'll clear out the existing keys and then re-add the new ones

			var toAdd = (TItem)item;

			// Handle the second case first
			if (reverseKeyedIndex.ContainsKey(toAdd))
			{
				foreach (var searchTerm in reverseKeyedIndex[toAdd])
					RemoveSearchTerm(toAdd, searchTerm);
			}

			// Now that we've cleared out any potential old datya, let's add stuff to the cache
			var searchTerms = GetSearchTerms(toAdd);

			foreach (var searchTerm in searchTerms)
				AddSearchTerm(toAdd, searchTerm);

			// Now add this stuff to the straight up cache
			if (inMemoryCache.ContainsKey(key))
				inMemoryCache[key] = toAdd;
			else
				inMemoryCache.Add(key, toAdd);

			return Utilities.TaskExtensions.CompletedTask;
		}

		public Task<bool> Exists(object key) => Task.FromResult(inMemoryCache.ContainsKey(key));

		public Task<IDictionary<object, object>> Get(IEnumerable<object> keys)
		{
			ArgumentHelpers.ThrowIfNull(() => keys);

			IDictionary<object, object> results = new Dictionary<object, object>();

			foreach (var key in keys)
			{
				if (!inMemoryCache.ContainsKey(key))
					continue;

				var obj = (object)inMemoryCache[key];

				results.Add(key, obj);
			}

			return Task.FromResult(results);
		}

		public Task Initialize(IDictionary<object, object> initialItems)
		{
			if (initialItems == null || !initialItems.Any() || isPrimed)
				return Utilities.TaskExtensions.CompletedTask;

			var addTasks = new List<Task>();

			foreach (var kvp in initialItems)
				addTasks.Add(Add(kvp.Key, kvp.Value));

			Task.WhenAll(addTasks).Wait();

			isPrimed = true;

			return Utilities.TaskExtensions.CompletedTask;
		}

		public Task<bool> IsInitialized() => Task.FromResult(isPrimed);

		public Task Remove(object key)
		{
			ArgumentHelpers.ThrowIfNull(() => key);

			TItem toRemove = default(TItem);

			if (inMemoryCache.ContainsKey(key))
			{
				toRemove = inMemoryCache[key];
				inMemoryCache.Remove(key);
			}

			if (reverseKeyedIndex.ContainsKey(toRemove))
			{
				foreach (var searchTerm in reverseKeyedIndex[toRemove])
					RemoveSearchTerm(toRemove, searchTerm);

				reverseKeyedIndex.Remove(toRemove);
			}

			return Utilities.TaskExtensions.CompletedTask;
		}

		public Task Replace(object key, object item)
		{
			// In this case Add handles the replace case for us
			return Add(key, item);
		}

		public Task<IEnumerable<object>> Search(string term)
		{
			/// TODO: Strip non-alpha, collapse whitespace from result of split
			var searchTerms = (term ?? string.Empty).Split(splitChars).Select(s => s.ToLower().Trim()).ToArray();

			readLock.Wait();

			IEnumerable<TItem> allResults = null;

			foreach (var st in searchTerms)
			{
				var singleResults = FindItemsForSingleTerm(st).OfType<TItem>();

				allResults = allResults == null ? singleResults : Enumerable.Intersect(allResults, singleResults);
			}

			readLock.Release();

			return Task.FromResult(allResults.Distinct().OfType<object>());
		}

		#endregion

		#region Private Helpers

		private void AddSearchTerm(TItem item, string searchTerm)
		{
			if (string.IsNullOrWhiteSpace(searchTerm))
				return;

			// For searching, we lowercase all terms
			searchTerm = searchTerm.ToLower();

			writeLock.Wait();

			if (!fullTextCache.ContainsKey(searchTerm))
				fullTextCache.Add(searchTerm, new LinkedList<TItem>());

			if (fullTextCache.ContainsKey(searchTerm))
				fullTextCache[searchTerm].AddLast(item);

			if (!reverseKeyedIndex.ContainsKey(item))
				reverseKeyedIndex.Add(item, new List<string>());

			if (!reverseKeyedIndex[item].Contains(searchTerm))
				reverseKeyedIndex[item].Add(searchTerm);

			writeLock.Release();
		}

		private void RemoveSearchTerm(TItem item, string searchTerm)
		{
			writeLock.Wait();

			if (fullTextCache.ContainsKey(searchTerm) && fullTextCache[searchTerm].Contains(item))
			{
				fullTextCache[searchTerm].Remove(item);

				// If the list of keys is now empty, remove this key from the index
				if (fullTextCache[searchTerm].Count == 0)
					fullTextCache.Remove(searchTerm);
			}

			writeLock.Release();
		}

		private IEnumerable<string> GetSearchTerms(TItem item)
		{
			var results = new List<string>();

			foreach (var props in propertiesToAdd)
			{
				var propValue = props.Item1.GetValue(item);
				if (propValue == null)
					continue;

				var strVal = propValue.ToString();

				if (string.IsNullOrWhiteSpace(strVal))
					continue;

				if (!props.Item2.Tokenize)
					results.Add(strVal);
				else
					results.AddRange(strVal.Split(' ').Select(s => s.Trim()));
			}

			return results;
		}

		private IEnumerable<TItem> FindItemsForSingleTerm(string searchTerm)
		{
			var searchResultsIndex = BinarySearch(searchTerm, fullTextCache);

			if (searchResultsIndex == -1 || searchResultsIndex >= fullTextCache.Count)
				return Enumerable.Empty<TItem>();

			var searchResults = new List<TItem>();

			while (searchResultsIndex < fullTextCache.Count && fullTextCache.Keys[searchResultsIndex].StartsWith(searchTerm))
			{
				var fullTextKey = fullTextCache.Keys[searchResultsIndex];

				if (!string.IsNullOrWhiteSpace(fullTextKey))
					searchResults.AddRange(fullTextCache[fullTextKey]);

				searchResultsIndex += 1;
			}

			return searchResults;
		}

		private int BinarySearch<U, V>(U key, SortedList<U, V> list) where U : IComparable<U>
		{
			if (list.Count == 0)
				return -1;

			int mid;
			int low = 0;
			int high = (list.Count - 1);

			while (low < high)
			{
				mid = (low + high) / 2;

				U currentKey = list.Keys[mid];

				if (currentKey.CompareTo(key) < 0)
					low = mid + 1;
				else
					high = mid;
			}

			if (list.Keys[low].CompareTo(key) < 0)
				return -1;

			while (low >= 0 && list.Keys[low].CompareTo(key) >= 0)
				low--;

			low++;

			return low;
		}

		#endregion
	}
}
