using System.Collections.Generic;
using System.Threading.Tasks;

namespace DapperDan.Caching
{
	/// <summary>
	/// An interface for defining a cache that can be used by the entity store.
	/// All operations return tasks so that distributed caches like redis can be used as well.
	/// </summary>
	public interface ICache
	{
		Task<bool> IsInitialized();
		Task Initialize(IDictionary<object, object> initialItems);
		Task Add(object key, object item);
		Task Replace(object key, object item);
		Task Remove(object key);
		Task<bool> Exists(object key);
		Task<IDictionary<object, object>> Get(IEnumerable<object> keys);
		Task<IEnumerable<object>> Search(string term);
	}
}
