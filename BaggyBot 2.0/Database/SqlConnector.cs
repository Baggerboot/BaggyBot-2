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
using LinqToDB;
using Mono.CSharp.Linq;
using Npgsql;


namespace BaggyBot.Database
{
	class SqlConnector : IDisposable
	{
		private DataConnection provider;

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
		private ITable<Metadata> Metadata;

		public ConnectionState ConnectionState
		{
			get
			{
				if (provider == null)
				{
					return ConnectionState.Closed;
				}
				else
				{
					return ConnectionState.Open;
					return provider.Connection.State;
				}
			}
		}

		public void Insert<T>(T row)
		{
			provider.Insert(row);
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
			provider = PostgreSQLTools.CreateDataConnection(connectionString);
			var schema = provider.DataProvider.GetSchemaProvider().GetSchema(provider);

			Metadata = provider.GetTable<Metadata>();
			try
			{
				var version = from entry in Metadata
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
					Logger.Log(this, "Bot and database version do not match (bot is at {0}, database is at {1}). The database connection will be dropped.", LogLevel.Warning, true, Bot.Version, version.First());
					return false;
				}
			}
			catch (NpgsqlException e)
			{
				if (e.Message.ToLower().Contains("metadata") && e.Message.ToLower().Contains("does not exist"))
				{
					Logger.Log(this, "Metadata table not found. A new tableset will be created.", LogLevel.Warning);
					try
					{
						provider.CreateTable<UserCredential>();
						provider.CreateTable<Quote>();
						provider.CreateTable<UserStatistic>();
						provider.CreateTable<UsedEmoticon>();
						provider.CreateTable<KeyValuePair>();
						provider.CreateTable<LinkedUrl>();
						provider.CreateTable<User>();
						provider.CreateTable<UsedWord>();
						provider.CreateTable<IrcLog>();
						provider.CreateTable<Metadata>();
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

			UserCredentials = provider.GetTable<UserCredential>();
			Quotes = provider.GetTable<Quote>();
			UserStatistics = provider.GetTable<UserStatistic>();
			Emoticons = provider.GetTable<UsedEmoticon>();
			KeyValuePairs = provider.GetTable<KeyValuePair>();
			LinkedUrls = provider.GetTable<LinkedUrl>();
			Users = provider.GetTable<User>();
			Words = provider.GetTable<UsedWord>();
			IrcLog = provider.GetTable<IrcLog>();

			return true;
		}

		public bool CloseConnection()
		{
			provider.Close();
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
			if (provider != null)
			{
				provider.Dispose();
			}
		}

		~SqlConnector()
		{
			Dispose(false);
		}

		internal int ExecuteStatement(string statement)
		{
			var cmd = provider.CreateCommand();
			cmd.CommandText = statement;
			return cmd.ExecuteNonQuery();
		}

		internal List<object> ExecuteQuery(string query)
		{
			var cmd = provider.CreateCommand();
			cmd.CommandText = query;
			throw new NotImplementedException();

		}

		public void Update<T>(T match)
		{
			provider.Update(match);
		}
	}
}
