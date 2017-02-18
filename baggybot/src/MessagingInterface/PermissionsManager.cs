using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaggyBot.Database;
using BaggyBot.Database.Model;
using BaggyBot.Monitoring;

namespace BaggyBot.MessagingInterface
{
	internal class PermissionsManager
	{
		private StatsDatabaseManager database;

		public PermissionsManager(StatsDatabaseManager statsDatabase)
		{
			database = statsDatabase;
		}

		/// <summary>
		/// Checks whether a user has access to the given permission node.
		/// <returns>True or false if permission is explicitly granted or denied,
		/// null if no permission entries are applicable for this user.</returns>
		/// </summary>
		public bool? CheckPermission(ChatUser user, string permissionName)
		{
			return CheckPermission(user, null, permissionName);
		}

		/// <summary>
		/// Checks whether a user has access to the given permission node.
		/// <returns>True or false if permission is explicitly granted or denied,
		/// null if no permission entries are applicable for this user.</returns>
		/// </summary>
		public bool? CheckPermission(ChatUser user, ChatChannel channel, string permissionName)
		{
			// If we don't have a DB connection, we should fall back to built-in permissions.
			if (database.ConnectionState != ConnectionState.Open) return null;

			var entries = database.GetPermissionEntries(user.DbUser, channel, GetNodes(permissionName).ToArray());
			if (entries.Any())
			{
				Logger.Log(this, $"Applicable permissions: \"{string.Join(", ", entries.Select(e => e.ToString()))}\"");
			}
			else
			{
				Logger.Log(this, $"No permission entries apply to {user} in {channel} for {permissionName}");
			}
			return entries.FirstOrDefault()?.Value;
		}

		private IEnumerable<string> GetNodes(string permissionName)
		{
			yield return permissionName;

			var cur = permissionName.Length;
			while ((cur = permissionName.LastIndexOf(".", cur-1, StringComparison.Ordinal)) >= 0)
			{
				yield return permissionName.Substring(0, cur) + ".*";
			}
		}
	}
}