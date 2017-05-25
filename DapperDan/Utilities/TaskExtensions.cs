using System.Threading.Tasks;

namespace DapperDan.Utilities
{
	internal static class TaskExtensions
	{
		internal static Task CompletedTask => Task.FromResult<object>(null);
	}
}
