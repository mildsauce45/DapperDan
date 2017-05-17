using System;

namespace DapperDan.Utilities
{
	internal class ArgumentHelpers
	{
		internal static void ThrowIfNull(Func<object> func, string message = "Argument cannot be null")
		{
			if (func() == null)
				throw new ArgumentNullException(message);
		}

		internal static void ThrowIfNullOrWhitespace(Func<string> func, string message = "Argument cannot be null or whitespace")
		{
			if (string.IsNullOrWhiteSpace(func()))
				throw new ArgumentNullException(message);
		}
	}
}
