// To switch between compiling for Transact-SQL and PostgreSQL, comment and uncomment the correct options
//#define mssql

//BaggyBot.Database.PostgreSQL

using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using BaggyBot.Database.Model;
using LinqToDB;


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
					return provider.Connection.State;
				}
			}
		}

		public void Insert(Poco row)
		{
			provider.Insert(row);
		}

		public void SubmitChanges()
		{
			provider.Insert(new IrcLog
			{
				Channel = "test"
			});
			
			// TODO: implement this

			throw new NotImplementedException();
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

			UserCredentials = provider.GetTable<UserCredential>();
			//Quotes = provider.GetTable<Quote>();
			UserStatistics = provider.GetTable<UserStatistic>();
			//Emoticons = provider.GetTable<Emoticon>();
			//KeyValuePairs = provider.GetTable<KeyValuePair>();
			LinkedUrls = provider.GetTable<LinkedUrl>();
			Users = provider.GetTable<User>();
			//Words = provider.GetTable<Word>();
			//IrcLog = provider.GetTable<IrcLog>();

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
			return provider.Execute(statement);
		}

		internal List<object> ExecuteQuery(string query)
		{
			//todo: implement query execution
			throw new NotImplementedException();
			//return provider.ExecuteReader(query);
		}
	}
}
