using BaggyBot.Database.Model;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;

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
				["1.2"] = UpgradeFrom1_2,
				["1.2.2"] = UpgradeFrom1_2_2,
				["2.0"] = UpgradeFrom2_0
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

		private string UpgradeFrom2_0()
		{
			connection.CreateTable<PermissionEntry>();
			connection.CreateTable<PermissionGroup>();
			connection.CreateTable<PermissionGroupMembership>();
			connection.CreateTable<UserGroup>();
			connection.CreateTable<UserGroupMembership>();
			return "2.1";
		}

		private string UpgradeFrom1_2_2()
		{
			var cmd = connection.CreateCommand();
			cmd.CommandText = "ALTER TABLE used_word ADD COLUMN is_ignored boolean NOT NULL DEFAULT false; ALTER TABLE used_word ALTER COLUMN is_ignored DROP DEFAULT;";
			cmd.ExecuteNonQuery();
			return "1.3";
		}

		private string UpgradeFrom1_2()
		{
			var miscData = connection.GetTable<MiscData>();
			miscData
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
