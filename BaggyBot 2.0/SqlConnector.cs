using System;
using System.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Configuration;

namespace BaggyBot
{
	class SqlConnector
	{
		private SqlConnection connection;
		private Database.StatsBotDataContext context;

		public Table<Database.UserStatistics> UserStats { get; private set; }
		public Table<Database.Name> UserNames { get; private set; }
		public Table<Database.Url> Urls { get; private set; }
		public Table<Database.Quote> Quotes { get; private set; }
		public Table<Database.Emoticon> Emoticons { get; private set; }
		public Table<Database.UserCredentials> UserCreds { get; private set; }
		public Table<Database.KeyValuePair> KeyValuePairs { get; private set; }
		public Table<Database.Word> Words { get; private set; }
		

		public SqlConnector()
		{
			Initialize();
		}

		private void Initialize()
		{
			connection = new SqlConnection();

			Settings set = Settings.Instance;
			string uid = set["sql_user"];
			string password = set["sql_password"];
			string server = set["sql_host"];
			string database = set["sql_database"];

			connection.ConnectionString = "persist security info=False;integrated security=SSPI;database=stats_bot;server=localhost";
		}

		internal void SubmitChanges()
		{
			context.SubmitChanges();
		}

		public bool OpenConnection()
		{
			try {
				connection.Open();
				context = new Database.StatsBotDataContext(connection);
				UserStats = context.GetTable<Database.UserStatistics>();
				Emoticons = context.GetTable<Database.Emoticon>();
				UserNames = context.GetTable<Database.Name>();
				Quotes = context.GetTable<Database.Quote>();
				Urls = context.GetTable<Database.Url>();
				UserCreds = context.GetTable<Database.UserCredentials>();
				KeyValuePairs = context.GetTable<Database.KeyValuePair>();
				Words = context.GetTable<Database.Word>();

				return true;
			} catch (SqlException e) {
				Logger.Log("Failed to open a connection to the SQL database. Error mesage: " + e.Message, LogLevel.Error);
				return false;
			} catch (InvalidOperationException e) {
				Logger.Log("Failed to open a connection to the SQL database. Error message: " + e.Message, LogLevel.Error);
				return true;
			}
		}

		public void Reconnect()
		{
			lock (connection) {
				connection.Close();
				connection.Dispose();
				connection = null;
				Initialize();
				OpenConnection();
			}
		}

		public bool CloseConnection()
		{
			try {
				connection.Close();
				return true;
			} catch (SqlException ex) {
				Logger.Log("Failed to close the connection to the SQL database. Error mesage: " + ex.Message, LogLevel.Error);
				return false;
			}
		}

		internal int ExecuteStatement(string statement)
		{
			lock (connection) {

				int result;
				using (SqlCommand cmd = new SqlCommand(statement, connection)) {
					result = cmd.ExecuteNonQuery();
				}
				return result;
			}
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

		internal void Dispose()
		{
			connection.Dispose();
		}
	}
}
