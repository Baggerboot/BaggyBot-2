using System;
using System.Linq;
using System.Collections.Generic;
using BaggyBot.Database.Model;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;

namespace BaggyBot.Database.Upgrades
{
	internal class DatabaseUpgrader
	{
		private readonly Dictionary<string, Func<string>> upgrades;
		private readonly DataConnection connection;

		public DatabaseUpgrader(DataConnection conn)
		{
			connection = conn;

			upgrades = new Dictionary<string, Func<string>>
			{
				["1.1"] = UpgradeFrom1_1,
				["1.2"] = UpgradeFrom1_2
			};
		}

		public bool HasUpgrade(string startVersion)
		{
			return upgrades.ContainsKey(startVersion);
		}

		public string UpgradeFrom(string version)
		{
			return upgrades[version]();
		}

		private string UpgradeFrom1_2()
		{
			var miscData = connection.GetTable<MiscData>();
			var res = miscData
				.Where(s => s.Type == "rem")
				.Set(s => s.Value, s => "say " + s.Value)
				.Set(s => s.Type, "alias")
				.Update();
			return "1.2.1";
		}

		private string UpgradeFrom1_1()
		{
			connection.CreateTable<MiscData>();
			return "1.2";
		}
	}
}
