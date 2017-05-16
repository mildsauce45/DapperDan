using System;
using System.Collections.Generic;

namespace DapperDan.EntityStores
{
	internal class PagingInfo
	{
		public IList<Tuple<string, SortDirection>> Sorts { get; private set; }

		public int? Skip { get; private set; }
		public int? Take { get; private set; }

		internal PagingInfo()
		{
			Sorts = new List<Tuple<string, SortDirection>>();
		}

		internal PagingInfo WithSort(string name, SortDirection direction)
		{
			Sorts.Add(Tuple.Create(name, direction));
			return this;
		}

		internal PagingInfo WithSkip(int? skip)
		{
			Skip = skip;
			return this;
		}

		internal PagingInfo WithTake(int? take)
		{
			Take = take;
			return this;
		}
	}
}
