using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Data.Linq;

namespace BaggyBot.Database.EntityProvider
{
	abstract class AbstractEntityProvider : IDisposable
	{
		protected SqlConnection connection;

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

		public void Reconnect()
		{
			Logger.Log("Attempting to reconnect to the SQL database.");
			connection.Close();
			connection.Dispose();
			connection = null;
			OpenConnection();
		}
		public bool CloseConnection()
		{
			try {
				Logger.Log("Closing SQL server connection", LogLevel.Info);
				connection.Close();
				return true;
			} catch (SqlException ex) {
				Logger.Log("Failed to close the connection to the SQL Database.MS_SQL. Error mesage: " + ex.Message, LogLevel.Error);
				return false;
			}
		}
		public int ExecuteStatement(string statement)
		{
			throw new NotImplementedException();
			/*Logger.Log("Manually executing an SQL statement.");
			int result;
			using (SqlCommand cmd = new SqlCommand(statement, connection)) {
				result = cmd.ExecuteNonQuery();
			}
			return result;*/
		}
	}
}
