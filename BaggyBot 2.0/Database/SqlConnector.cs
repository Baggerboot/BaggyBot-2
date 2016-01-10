// To switch between compiling for Transact-SQL and PostgreSQL, comment and uncomment the correct options
//#define mssql

//BaggyBot.Database.PostgreSQL

using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using BaggyBot.Database.Model;
using BaggyBot.Database.Upgrades;
using LinqToDB;
using Mono.CSharp.Linq;
using Npgsql;


namespace BaggyBot.Database
{
	class SqlConnector : IDisposable
	{
		private DataConnection connection;

		public ITable<LinkedUrl> LinkedUrls;
		public ITable<User> Users;
		public ITable<UserCredential> UserCredentials;
		public ITable<UserStatistic> UserStatistics;
		public ITable<UsedEmoticon> Emoticons;
		public ITable<IrcLog> IrcLog;
		public ITable<KeyValuePair> KeyValuePairs;
		public ITable<Quote> Quotes;
		public ITable<LinkedUrl> Urls;
		public ITable<UsedWord> Words;
		public ITable<MiscData> MiscData;

		private ITable<Metadata> metadata;


		public ConnectionState ConnectionState
		{
			get
			{
				if (connection == null)
				{
					return ConnectionState.Closed;
				}
				else
				{
					return ConnectionState.Open;
					return connection.Connection.State;
				}
			}
		}

		public void Insert<T>(T row)
		{
			connection.Insert(row);
		}

		public void SubmitChanges()
		{

		}
		public bool OpenConnection()
		{
			var connectionString = Settings.Instance["sql_connection_string"];
			if (string.IsNullOrWhiteSpace(connectionString))
			{
				Logger.Log(this, "Unable to connect to the SQL database: No connection specified.", LogLevel.Error);
				return false;
			}
			connection = PostgreSQLTools.CreateDataConnection(connectionString);
			var schema = connection.DataProvider.GetSchemaProvider().GetSchema(connection);

			metadata = connection.GetTable<Metadata>();
			try
			{
				var version = from entry in metadata
							  where entry.Key == "version"
							  select entry.Value;

				var count = version.Count();
				if (count != 1)
				{
					Logger.Log(this, "Zero or multiple 'version' entries found in the Metadata table. The database connection will be dropped.", LogLevel.Error);
					return false;
				}

				if (version.First() != Bot.DatabaseVersion)
				{
					Logger.Log(this, "Bot and database version do not match (bot is at {0}, database is at {1}). The database will now be upgraded.", LogLevel.Info, true, Bot.DatabaseVersion, version.First());
					if (!UpgradeDatabase(version.First()))
					{
						Logger.Log(this, "Upgrade failed. The database connection will be dropped.", LogLevel.Warning);
						return false;
					}
				}
			}
			catch (NpgsqlException e)
			{
				if (e.Message.ToLower().Contains("metadata") && e.Message.ToLower().Contains("does not exist"))
				{
					Logger.Log(this, "Metadata table not found. A new tableset will be created.", LogLevel.Warning);
					try
					{
						connection.CreateTable<UserCredential>();
						connection.CreateTable<Quote>();
						connection.CreateTable<UserStatistic>();
						connection.CreateTable<UsedEmoticon>();
						connection.CreateTable<KeyValuePair>();
						connection.CreateTable<LinkedUrl>();
						connection.CreateTable<User>();
						connection.CreateTable<UsedWord>();
						connection.CreateTable<IrcLog>();
						connection.CreateTable<Metadata>();
						connection.CreateTable<MiscData>();
					}
					catch (NpgsqlException f)
					{
						Logger.Log(this, "Unable to create a new tableset: An exception occurred({0}: {1}). The database connection will be dropped.", LogLevel.Error, true, e.GetType().Name, e.Message);
						return false;
					}
					Logger.Log(this, "The database has been populated. Writing metadata...", LogLevel.Info);
					Insert(new Metadata
					{
						Key = "version",
						Value = Bot.DatabaseVersion
					});
				}
				else
				{
					Logger.Log(this, "Unable to retrieve the database version: An exception occurred ({0}: {1}). The database connection will be dropped.", LogLevel.Error, true, e.GetType().Name, e.Message);
					return false;
				}
			}

			UserCredentials = connection.GetTable<UserCredential>();
			Quotes = connection.GetTable<Quote>();
			UserStatistics = connection.GetTable<UserStatistic>();
			Emoticons = connection.GetTable<UsedEmoticon>();
			KeyValuePairs = connection.GetTable<KeyValuePair>();
			LinkedUrls = connection.GetTable<LinkedUrl>();
			Users = connection.GetTable<User>();
			Words = connection.GetTable<UsedWord>();
			IrcLog = connection.GetTable<IrcLog>();

			return true;
		}

		private bool UpgradeDatabase(string fromVersion)
		{
			var upgrader = new DatabaseUpgrader(connection);

			var currentVersion = fromVersion;
			while (upgrader.HasUpgrade(currentVersion))
			{
				var newVersion = upgrader.UpgradeFrom(currentVersion);
				Logger.Log(this, "Successfully upgraded the database from version {0} to version {1}", LogLevel.Info, true, currentVersion, newVersion);
				currentVersion = newVersion;
			}

			metadata.Where(entry => entry.Key == "version").Set(entry => entry.Value, currentVersion).Update();

			if (currentVersion == Bot.DatabaseVersion)
			{
				Logger.Log(this, "Database upgrade completed.", LogLevel.Info);
				return true;
			}
			else
			{
				Logger.Log(this, "Unable to upgrade the database (current version: {0}) to match the bot (current version {1})", LogLevel.Error, true, currentVersion, Bot.DatabaseVersion);
				return false;
			}
		}

		public bool CloseConnection()
		{
			connection.Close();
			return true;
		}

		public void Reconnect()
		{
			// TODO: implement reconnect
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool cleanAll)
		{
			if (connection != null)
			{
				connection.Dispose();
			}
		}

		~SqlConnector()
		{
			Dispose(false);
		}

		internal int ExecuteStatement(string statement)
		{
			var cmd = connection.CreateCommand();
			cmd.CommandText = statement;
			return cmd.ExecuteNonQuery();
		}

		internal List<object> ExecuteQuery(string query)
		{
			var cmd = connection.CreateCommand();
			cmd.CommandText = query;
			throw new NotImplementedException();

		}

		public void Update<T>(T match)
		{
			connection.Update(match);
		}
	}
}
