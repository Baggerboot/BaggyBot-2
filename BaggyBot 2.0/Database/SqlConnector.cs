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
	class SqlConnector : IDisposable
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

		internal void SubmitChanges()
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

		public void CloseConnection()
		{
			provider.CloseConnection();
		}

		public void Reconnect()
		{
			provider.Reconnect();
		}

		/*private DataView Select(string query)
		{
			lock (connection) {
				DataView ret;
				using (SqlCommand cmd = new SqlCommand(query, connection)) {
					using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
						using (DataSet ds = new DataSet()) {
							da.Fill(ds);
							ret = ds.Tables[0].DefaultView;
						}
					}
				}
				return ret;
			}
		}*/
		/// Selects a vector and returns it in the form of an array.
		/// The data returned may only contain one column, or else an InvalidOperationException will be thrown.
		/// </summary>
		/*internal T[] SelectVector<T>(string query)
		{
			int ID = new Random().Next(100, 999);
			T[] data;
			using (DataView dv = Select(query)) {
				if (dv.Table.Columns.Count > 1) {
					//busy = false;
					throw new InvalidOperationException("The passed query returned more than one column.");
				} else {
					data = new T[dv.Count];
					for (int i = 0; i < data.Length; i++) {
						Object value = dv[0][i];
						data[i] = (T)value;
					}
				}
			}
			return data;
		}*/


		/*internal T SelectOne<T>(string query)
		{

			Object data;
			using (DataView dv = Select(query)) {
				if (dv.Count == 1) {
					data = dv[0][0];
				} else {
					throw new InvalidOperationException("The passed query returned more or less than one record.");
				}
			}
			if (data == DBNull.Value && Nullable.GetUnderlyingType(typeof(T)) == null) {
				if (typeof(T) == typeof(String)) {
					return default(T);
				}
				throw new RecordNullException();
			} else {
				return (T)data;
			}
		}*/

		public void Dispose()
		{
			provider.Dispose();
		}
	}
}
