using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot
{
	class CodeFormatter
	{

		private bool WorksAsEnumerable(object obj)
		{
			System.Collections.IEnumerable enumerable = obj as System.Collections.IEnumerable;
			if (enumerable != null) {
				try {
					enumerable.GetEnumerator();
					return true;
				} catch {
					// nothing, we return false below
				}
			}
			return false;
		}

		public string EscapeString(string s)
		{
			return s.Replace("\"", "\\\"");
		}

		public string PrettyPrint(object result)
		{
			StringBuilder sb = new StringBuilder();
			if (result == null) {
				sb.Append("null");
				return sb.ToString();
			}

			if (result is Array) {
				Array a = (Array)result;

				sb.Append("{ ");
				int top = a.GetUpperBound(0);
				for (int i = a.GetLowerBound(0); i <= top; i++) {
					sb.Append(PrettyPrint(a.GetValue(i)));
					if (i != top)
						sb.Append(", ");
				}
				sb.Append(" }");
			} else if (result is bool) {
				if ((bool)result)
					sb.Append("true");
				else
					sb.Append("false");
			} else if (result is string) {
				sb.Append(String.Format("\"{0}\"", EscapeString((string)result)));
			} else if (result is System.Collections.IDictionary) {
				System.Collections.IDictionary dict = (System.Collections.IDictionary)result;
				int top = dict.Count, count = 0;

				sb.Append("{");
				foreach (System.Collections.DictionaryEntry entry in dict) {
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
			} else if (WorksAsEnumerable(result)) {
				int i = 0;
				sb.Append("{ ");
				foreach (object item in (System.Collections.IEnumerable)result) {
					if (i++ != 0)
						sb.Append(", ");

					sb.Append(PrettyPrint(item));
				}
				sb.Append(" }");
			} else if (result is char) {
				sb.Append(EscapeChar((char)result));
			} else {
				sb.Append(result.ToString());
			}
			return sb.ToString();
		}

		public string EscapeChar(char c)
		{
			if (c == '\'') {
				return "'\\''";
			}
			if (c > 32) {
				return string.Format("'{0}'", c);
			}
			switch (c) {
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
					return string.Format("'\\x{0:x}", (int)c);
			}
		}

	}
}
