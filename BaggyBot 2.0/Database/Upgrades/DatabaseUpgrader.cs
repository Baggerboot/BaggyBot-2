using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaggyBot.Database.Model;
using LinqToDB;
using LinqToDB.Data;

namespace BaggyBot.Database.Upgrades
{
	class DatabaseUpgrader
	{
		private readonly Dictionary<string, Func<string>> upgrades;
		private readonly DataConnection connection;

		public DatabaseUpgrader(DataConnection conn)
		{
			connection = conn;

			upgrades = new Dictionary<string, Func<string>>
			{
				{"1.1", UpgradeFrom1_1}
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

		private string UpgradeFrom1_1()
		{
			connection.CreateTable<MiscData>();
			return "1.2";
		}
	}
}
