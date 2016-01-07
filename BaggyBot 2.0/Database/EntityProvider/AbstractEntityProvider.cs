using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace BaggyBot.Database.EntityProvider
{
	abstract class AbstractEntityProvider : IDisposable
	{
		protected DbConnection connection;

		public abstract IQueryable UserCreds { get; }
		public abstract IQueryable Quotes { get; }
		public abstract IQueryable UserStats { get; }
		public abstract IQueryable Emoticons { get; }
		public abstract IQueryable KeyValuePairs { get; }
		public abstract IQueryable Urls { get; }
		public abstract IQueryable UserNames { get; }
		public abstract IQueryable Words { get; }
		public abstract IQueryable IrcLog { get; }

		public abstract void SubmitChanges();
		public abstract bool OpenConnection();
		public abstract void Dispose();
		public ConnectionState ConnectionState { get { return connection.State; } }

		public void Reconnect()
		{
			Logger.Log(this, "Attempting to reconnect to the SQL database.");
			connection.Close();
			connection.Dispose();
			connection = null;
			OpenConnection();
		}
		public bool CloseConnection()
		{
			try {
				Logger.Log(this, "Closing SQL server connection", LogLevel.Info);
				connection.Close();
				return true;
			} catch (SqlException ex) {
				Logger.Log(this, "Failed to close the connection to the SQL Database.MS_SQL. Error mesage: " + ex.Message, LogLevel.Error);
				return false;
			}
		}
		public int ExecuteStatement(string statement)
		{
			int result;
			using (var cmd = connection.CreateCommand()) {
				cmd.CommandText = statement;
				result = cmd.ExecuteNonQuery();
			}
			return result;
		}

		public List<object> ExecuteQuery(string query)
		{
			var data = new List<object>();
			Logger.Log(this, "Manually executing an SQL query.");
			using (var cmd = connection.CreateCommand()) {
				cmd.CommandText = query;
				var reader = cmd.ExecuteReader();
				while (reader.Read()) {
					data.Add(reader[0]);
				}
			}
			return data;
		}
	}
}
