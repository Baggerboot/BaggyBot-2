using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Commands.Interpreters
{
	internal static class CodeFormatter
	{
		private static bool WorksAsEnumerable(object obj)
		{
			var enumerable = obj as IEnumerable;
			if (enumerable != null)
			{
				try
				{
					enumerable.GetEnumerator();
					return true;
				}
				catch
				{
					// nothing, we return false below
				}
			}
			return false;
		}

		public static string EscapeString(string s)
		{
			return s.Replace("\"", "\\\"");
		}

		public static string PrettyPrint(object result)
		{
			var sb = new StringBuilder();
			if (result == null)
			{
				sb.Append("null");
				return sb.ToString();
			}

			if (result is Array)
			{
				var a = (Array)result;

				sb.Append("{ ");
				var top = a.GetUpperBound(0);
				for (var i = a.GetLowerBound(0); i <= top; i++)
				{
					sb.Append(PrettyPrint(a.GetValue(i)));
					if (i != top) sb.Append(", ");
				}
				sb.Append(" }");
			}
			else if (result is bool)
			{
				if ((bool)result)
				{
					sb.Append("true");
				}
				else
				{
					sb.Append("false");
				}
			}
			else if (result is string)
			{
				sb.Append($"\"{EscapeString((string)result)}\"");
			}
			else if (result is IDictionary)
			{
				var dict = (IDictionary)result;
				int top = dict.Count, count = 0;

				sb.Append("{");
				foreach (DictionaryEntry entry in dict)
				{
					count++;
					sb.Append("{ ");
					sb.Append(PrettyPrint(entry.Key));
					sb.Append(", ");
					sb.Append(PrettyPrint(entry.Value));
					if (count != top)
						sb.Append(" }, ");
					else
						sb.Append(" }");
				}
				sb.Append("}");
			}
			else if (WorksAsEnumerable(result))
			{
				var i = 0;
				sb.Append("{ ");
				foreach (var item in (IEnumerable)result)
				{
					if (i++ != 0)
						sb.Append(", ");

					sb.Append(PrettyPrint(item));
				}
				sb.Append(" }");
			}
			else if (result is char)
			{
				sb.Append(EscapeChar((char)result));
			}
			else {
				sb.Append(result);
			}
			return sb.ToString();
		}

		public static string EscapeChar(char c)
		{
			if (c == '\'')
			{
				return "'\\''";
			}
			if (c > 32)
			{
				return $"'{c}'";
			}
			switch (c)
			{
				case '\a':
					return "'\\a'";

				case '\b':
					return "'\\b'";

				case '\n':
					return "'\\n'";

				case '\v':
					return "'\\v'";

				case '\r':
					return "'\\r'";

				case '\f':
					return "'\\f'";

				case '\t':
					return "'\\t";

				default:
					return $"'\\x{(int)c:x}";
			}
		}
	}
}
