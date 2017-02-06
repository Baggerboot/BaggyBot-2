using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using BaggyBot.CommandParsing;
using BaggyBot.Formatting;
using BaggyBot.Tools;

namespace BaggyBot.Commands
{
	internal class Sql : Command
	{
		public override PermissionLevel Permissions => PermissionLevel.BotOperator;
		public override string Usage => "[-t|--tables]|[<-r|--rows> <rows>] <SQL code>";
		public override string Description => "Execute arbitrary SQL code and return its result.";

		public override void Use(CommandArgs command)
		{
			var parser = new CommandParser(new Operation()
				.AddKey("rows", 3, 'r')
				.AddFlag("tables", 't')
				.AddRestArgument(string.Empty));
			var parsed = parser.Parse(command.FullArgument);

			if (parsed.Flags["tables"])
			{
				command.Reply($"the following tables are available: {string.Join(", ", command.Client.StatsDatabase.GetTableNames())}");
				return;
			}
			if (string.IsNullOrWhiteSpace(parsed.RestArgument))
			{
				InformUsage(command);
				return;
			}

			var table = command.Client.StatsDatabase.ExecuteQuery(parsed.RestArgument);
			if (table.Rows.Count == 0)
			{
				command.Reply("done.");
				return;
			}

			var maxRows = parsed.GetKey<int>("rows");

			var columnLengths = GetColumnLengths(table, maxRows);
			if (command.Client.AllowsMultilineMessages)
			{
				var dividerLength = columnLengths.Sum() + (columnLengths.Length - 1) * " | ".Length;
				var divider = string.Concat(Enumerable.Repeat('=', dividerLength));
				command.ReturnMessage($"{Frm.CodeBlockStart}{PrintHeader(table, columnLengths)}\n{divider}\n{PrintBody(table, columnLengths, maxRows)}{Frm.CodeBlockEnd}");
			}
			else
			{
				var header = $"{Frm.U}{PrintHeader(table, columnLengths)}{Frm.U}";
				var lines = PrintBody(table, columnLengths, maxRows).Split('\n');
				command.ReturnMessage(header);
				foreach (var line in lines)
				{
					command.ReturnMessage(line);
				}
			}
		}

		private static int GetMaxLength(DataColumn column, int maxRows)
		{
			var headerLength = column.ColumnName.Length;
			var maxColumnLength = GetColumnValues(column, maxRows).Select(v => v.ToString().Length).Max();
			return Math.Max(headerLength, maxColumnLength);
		}

		private static IEnumerable<object> GetColumnValues(DataColumn column, int maxRows)
		{
			for (var i = 0; i < Math.Min(column.Table.Rows.Count, maxRows); i++)
			{
				yield return column.Table.Rows[i][column.Ordinal];
			}
		}

		private static int[] GetColumnLengths(DataTable table, int rowLimit = -1)
		{
			if (rowLimit < 0)
			{
				rowLimit = table.Rows.Count;
			}
			return table.GetColumns().Select(c => GetMaxLength(c, rowLimit)).ToArray();
		}

		private static string PrintHeader(DataTable table, int[] columnLengths)
		{
			return string.Join(" | ", table.GetColumnNames().Select((n, i) => n.PadRight(columnLengths[i])));
		}

		private static string PrintBody(DataTable table, int[] columnLengths, int maxRows)
		{
			var sb = new StringBuilder();

			var index = 0;
			foreach (DataRow row in table.Rows)
			{
				index++;
				sb.AppendLine(string.Join(" | ", row.ItemArray.Select((c, i) => c.ToString().PadRight(columnLengths[i]))));
				if (index >= maxRows) break;
			}
			if (maxRows < table.Rows.Count)
			{
				var remaining = table.Rows.Count - maxRows;
				sb.Append($"... and {remaining} more row{(remaining == 1 ? "" : "s")}.");
			}
			return sb.ToString();
		}
	}
}
