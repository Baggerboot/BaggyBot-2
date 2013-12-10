// To switch between compiling for Transact-SQL and PostgreSQL, comment and uncomment the correct options
//#define mssql

//BaggyBot.Database.PostgreSQL

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using BaggyBot.Database.EntityProvider;

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

#if postgresql
		public Table<PostgreSQL.UserCredentials> UserCreds;
		public Table<PostgreSQL.Quote> Quotes;
		public Table<PostgreSQL.UserStatistics> UserStats;
		public Table<PostgreSQL.Emoticon> Emoticons;
		public Table<PostgreSQL.KeyValuePair> KeyValuePairs;
		public Table<PostgreSQL.Url> Urls;
		public Table<PostgreSQL.Name> UserNames;
		public Table<PostgreSQL.Word> Words;
		public Table<PostgreSQL.IrcLog> IrcLog;
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

		public SqlConnector()
		{
			bool useDbLinq = false;
			bool.TryParse(Settings.Instance["sql_use_dblinq"], out useDbLinq);
			provider = new EntityProviderFactory().CreateEntityProvider(useDbLinq ? SupportedDatabases.PostgreSQL : SupportedDatabases.MsSql);
		}

		public bool OpenConnection()
		{
			bool result = provider.OpenConnection();

#if postgresql
			UserCreds = (Table<PostgreSQL.UserCredentials>)provider.UserCreds;
			Quotes = (Table<PostgreSQL.Quote>)provider.Quotes;
			UserStats = (Table<PostgreSQL.UserStatistics>)provider.UserStats;
			Emoticons = (Table<PostgreSQL.Emoticon>)provider.Emoticons;
			KeyValuePairs = (Table<PostgreSQL.KeyValuePair>)provider.KeyValuePairs;
			Urls = (Table<PostgreSQL.Url>)provider.Urls;
			UserNames = (Table<PostgreSQL.Name>)provider.UserNames;
			Words = (Table<PostgreSQL.Word>)provider.Words;
			IrcLog = (Table<PostgreSQL.IrcLog>)provider.IrcLog;
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
			if (cleanAll) {
			}
			provider.Dispose();
		}

		~SqlConnector()
		{
			Dispose(false);
		}
	}
}
