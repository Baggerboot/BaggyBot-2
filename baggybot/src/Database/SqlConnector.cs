using BaggyBot.Database.Model;
using BaggyBot.Database.Upgrades;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaggyBot.Monitoring;
using Metadata = BaggyBot.Database.Model.Metadata;

namespace BaggyBot.Database
{
	public class SqlConnector : IDisposable
	{
		private DataConnection connection;

		public ITable<LinkedUrl> LinkedUrls { get; private set; }
		public ITable<User> Users { get; private set; }
		public ITable<UserCredential> UserCredentials { get; private set; }
		public ITable<UserStatistic> UserStatistics { get; private set; }
		public ITable<UsedEmoticon> Emoticons { get; private set; }
		public ITable<IrcLog> IrcLog { get; private set; }
		public ITable<KeyValuePair> KeyValuePairs { get; private set; }
		public ITable<Quote> Quotes { get; private set; }
		public ITable<UsedWord> Words { get; private set; }
		public ITable<MiscData> MiscData { get; private set; }

		private ITable<Metadata> metadata;

		private ConnectionState internalState;

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
					return internalState;
				}
			}
		}

		public int Insert<T>(T row)
		{
			return connection.Insert(row);
		}

		public int InsertOrReplace<T>(T row)
		{
			return connection.InsertOrReplace(row);
		}

		private void HandleConnectionFailure()
		{
			internalState = ConnectionState.Closed;
		}

		public void Reset()
		{
			Logger.Log(this, "Resetting tables...", LogLevel.Warning);
			DropTables();
			CreateTables();
			Logger.Log(this, "Writing metadata...", LogLevel.Info);
			AddMetadata();
		}

		private void DropTables()
		{
			connection.DropTable<UserCredential>();
			connection.DropTable<Quote>();
			connection.DropTable<UserStatistic>();
			connection.DropTable<UsedEmoticon>();
			connection.DropTable<KeyValuePair>();
			connection.DropTable<LinkedUrl>();
			connection.DropTable<User>();
			connection.DropTable<UsedWord>();
			connection.DropTable<IrcLog>();
			connection.DropTable<Metadata>();
			connection.DropTable<MiscData>();
		}

		private void CreateTables()
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

		private void AddMetadata()
		{
			Insert(new Metadata
			{
				Key = "version",
				Value = Bot.DatabaseVersion
			});
		}

		public bool OpenConnection(string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
			{
				Logger.Log(this, "Unable to connect to the SQL database: No connection specified.", LogLevel.Error);
				return false;
			}
			internalState = ConnectionState.Connecting;
			connection = PostgreSQLTools.CreateDataConnection(connectionString);
			internalState = ConnectionState.Open;
			connection.OnClosing += (sender, args) => internalState = ConnectionState.Closed;
			connection.OnClosed += (sender, args) => internalState = ConnectionState.Closed;
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
					HandleConnectionFailure();
					return false;
				}

				if (version.First() != Bot.DatabaseVersion)
				{
					Logger.Log(this, $"Bot and database version do not match (bot is at {Bot.DatabaseVersion}, database is at {version.First()}). The database will now be upgraded.", LogLevel.Info);
					if (!UpgradeDatabase(version.First()))
					{
						Logger.Log(this, "Upgrade failed. The database connection will be dropped.", LogLevel.Warning);
						HandleConnectionFailure();
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
						CreateTables();
					}
					catch (NpgsqlException f)
					{
						Logger.Log(this, $"Unable to create a new tableset: An exception occurred({f.GetType()}: {f.Message}). The database connection will be dropped.", LogLevel.Error);
						HandleConnectionFailure();
						return false;
					}
					AddMetadata();
					Logger.Log(this, "Writing metadata...", LogLevel.Info);
				}
				else
				{
					Logger.Log(this, $"Unable to retrieve the database version: An exception occurred ({e.GetType()}: {e.Message}). The database connection will be dropped.", LogLevel.Error);
					HandleConnectionFailure();
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
			MiscData = connection.GetTable<MiscData>();

			return true;
		}

		private bool UpgradeDatabase(string fromVersion)
		{
			var upgrader = new DatabaseUpgrader(connection);

			var currentVersion = fromVersion;
			while (upgrader.HasUpgrade(currentVersion))
			{
				var newVersion = upgrader.UpgradeFrom(currentVersion);
				Logger.Log(this, $"Successfully upgraded the database from version {currentVersion} to version {newVersion}", LogLevel.Info);
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
				Logger.Log(this, $"Unable to upgrade the database (current version: {currentVersion}) to match the bot (current version {Bot.DatabaseVersion})", LogLevel.Error);
				return false;
			}
		}

		public bool CloseConnection()
		{
			connection.Close();
			return true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool cleanAll)
		{
			connection?.Dispose();
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

		internal DataTable ExecuteQuery(string query)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = query;
				using (var reader = cmd.ExecuteReader())
				{
					var table = new DataTable();
					table.Load(reader);
					return table;
				}
			}
		}

		public void Update<T>(T match) where T:Poco
		{
			connection.Update(match);
		}
	}
}
