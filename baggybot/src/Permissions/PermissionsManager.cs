using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaggyBot.Commands;
using BaggyBot.Configuration;
using BaggyBot.Database;
using BaggyBot.MessagingInterface;
using BaggyBot.Monitoring;

namespace BaggyBot.Permissions
{
	internal class PermissionsManager
	{
		private readonly StatsDatabaseManager database;
		private readonly IEnumerable<Operator> operators;

		public PermissionsManager(StatsDatabaseManager database, IEnumerable<Operator> operators)
		{
			this.database = database;
			this.operators = operators;
		}

		/// <summary>
		/// Checks whether the given user is defined as an operator in the config file.
		/// </summary>
		public bool IsOperator(ChatUser user)
		{
			Logger.Log(null, "Performing operator check on " + user);
			return operators.Any(op => Validate(user, op));
		}

		/// <summary>
		/// Validates a user against an operator configuration.
		/// <returns>
		/// True if the Operator configuration matches the given user,
		/// false otherwise.
		/// </returns>
		/// </summary>
		private bool Validate(ChatUser user, Operator op)
		{
			Func<string, string, bool> match = (input, reference) => reference.Equals("*") || input.Equals(reference);

			var nickM = match(user.Nickname, op.Nick);
			var uniqueIdM = match(user.UniqueId, op.UniqueId);
			var uidM = true;
			if (op.Uid != "*")
			{
				uidM = database.MapUser(user).Id == int.Parse(op.Uid);
			}

			return nickM && uniqueIdM && uidM;
		}


		/// <summary>
		/// Checks whether the sender of a command has access to the given permission node.
		/// Tries to use the permission system first, but falls back to an operator
		/// check otherwise.
		/// </summary>
		public bool Test(CommandArgs command, PermNode permissionName)
		{
			return Test(command.Sender, command.Channel, permissionName);
		}

		/// <summary>
		/// Checks whether the sender of a message has access to the given permission node.
		/// Tries to use the permission system first, but falls back to an operator
		/// check otherwise.
		/// </summary>
		public bool Test(ChatMessage message, PermNode permissionName)
		{
			return Test(message.Sender, message.Channel, permissionName);
		}

		/// <summary>
		/// Checks whether a user has access to the given permission node.
		/// Tries to use the permission system first, but falls back to an operator
		/// check otherwise.
		/// </summary>
		public bool Test(ChatUser user, ChatChannel channel, PermNode permissionName)
		{
			var permEntry = CheckPermission(user, channel, permissionName);

			return permEntry ?? IsOperator(user);
		}

		/// <summary>
		/// Checks whether a user has access to the given permission node.
		/// <returns>True or false if permission is explicitly granted or denied,
		/// null if no permission entries are applicable for this user.</returns>
		/// </summary>
		public bool? CheckPermission(ChatUser user, PermNode permissionName)
		{
			return CheckPermission(user, null, permissionName);
		}

		/// <summary>
		/// Checks whether a user has access to the given permission node.
		/// <returns>True or false if permission is explicitly granted or denied,
		/// null if no permission entries are applicable for this user.</returns>
		/// </summary>
		public bool? CheckPermission(ChatUser user, ChatChannel channel, PermNode permissionName)
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

		private IEnumerable<string> GetNodes(PermNode permissionName)
		{
			yield return permissionName.Path;

			var cur = permissionName.Path.Length;
			while ((cur = permissionName.Path.LastIndexOf(".", cur - 1, StringComparison.Ordinal)) >= 0)
			{
				yield return permissionName.Path.Substring(0, cur) + ".*";
			}
		}
	}
}