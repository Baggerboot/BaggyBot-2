// To switch between compiling for Transact-SQL and PostgreSQL, comment and uncomment the correct options
//#define mssql

//BaggyBot.Database.PostgreSQL

using System;
using System.Data;
using System.Collections.Generic;
using BaggyBot.Database.EntityProvider;
using BaggyBot.Database.PostgreSQL;
#if postgresql
using DbLinq.Data.Linq;
#endif
#if mssql
using System.Data.Linq;
#endif

namespace BaggyBot.Database
{
	public class SqlConnector : IDisposable
	{
		private AbstractEntityProvider provider;
		public ConnectionState ConnectionState
		{
			get
			{
				if (provider == null) {
					return ConnectionState.Closed;
				}
				return provider.ConnectionState;
			}
		}

#if postgresql
		public Table<UserCredentials> UserCreds;
		public Table<Quote> Quotes;
		public Table<UserStatistics> UserStats;
		public Table<Emoticon> Emoticons;
		public Table<KeyValuePair> KeyValuePairs;
		public Table<Url> Urls;
		public Table<Name> UserNames;
		public Table<Word> Words;
		public Table<IrcLog> IrcLog;
#endif
#if mssql
		public Table<MS_SQL.UserCredentials> UserCreds;
		public Table<MS_SQL.Quote> Quotes;
		public Table<MS_SQL.UserStatistics> UserStats;
		public Table<MS_SQL.Emoticon> Emoticons;
		public Table<MS_SQL.KeyValuePair> KeyValuePairs;
		public Table<MS_SQL.Url> Urls;
		public Table<MS_SQL.Name> UserNames;
		public Table<MS_SQL.Word> Words;
#endif

		public void SubmitChanges()
		{
			provider.SubmitChanges();
		}
		public bool OpenConnection()
		{
			var useDbLinq = false;
			if (bool.TryParse(Settings.Instance["sql_use_dblinq"], out useDbLinq)) {
				provider = new EntityProviderFactory().CreateEntityProvider(useDbLinq ? SupportedDatabases.PostgreSQL : SupportedDatabases.MsSql);
			} else {
				Logger.Log(this, "Unable to connect to the SQL database: settings value for sql_use_dblinq not set.", LogLevel.Error);
				return false;
			}
			var result = provider.OpenConnection();

#if postgresql
			UserCreds = (Table<UserCredentials>)provider.UserCreds;
			Quotes = (Table<Quote>)provider.Quotes;
			UserStats = (Table<UserStatistics>)provider.UserStats;
			Emoticons = (Table<Emoticon>)provider.Emoticons;
			KeyValuePairs = (Table<KeyValuePair>)provider.KeyValuePairs;
			Urls = (Table<Url>)provider.Urls;
			UserNames = (Table<Name>)provider.UserNames;
			Words = (Table<Word>)provider.Words;
			IrcLog = (Table<IrcLog>)provider.IrcLog;
#endif
#if mssql
			UserCreds = (Table<MS_SQL.UserCredentials>) provider.UserCreds;
			Quotes = (Table<MS_SQL.Quote>) provider.Quotes;
			UserStats = (Table<MS_SQL.UserStatistics>) provider.UserStats;
			Emoticons = (Table<MS_SQL.Emoticon>) provider.Emoticons;
			KeyValuePairs = (Table<MS_SQL.KeyValuePair>) provider.KeyValuePairs;
			Urls = (Table<MS_SQL.Url>) provider.Urls;
			UserNames = (Table<MS_SQL.Name>) provider.UserNames;
			Words = (Table<MS_SQL.Word>) provider.Words;
#endif
			return result;
		}

		public bool CloseConnection()
		{
			return provider.CloseConnection();
		}

		public void Reconnect()
		{
			provider.Reconnect();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool cleanAll)
		{
			if (provider != null) {
				provider.Dispose();
			}
		}

		~SqlConnector()
		{
			Dispose(false);
		}

		internal int ExecuteStatement(string statement)
		{
			return provider.ExecuteStatement(statement);
		}

		internal List<object> ExecuteQuery(string query)
		{
			return provider.ExecuteQuery(query);
		}
	}
}
