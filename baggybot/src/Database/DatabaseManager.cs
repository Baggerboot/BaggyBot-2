using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using BaggyBot.Database.Model;
using BaggyBot.Tools;
using LinqToDB.Mapping;

namespace BaggyBot.Database
{
	public class DatabaseManager : IDisposable
	{
		protected SqlConnector SqlConnector;
		protected LockObject LockObj;
		public ConnectionState ConnectionState => SqlConnector.ConnectionState;


		public DatabaseManager(SqlConnector sqlConnector)
		{
			this.SqlConnector = sqlConnector;
			LockObj = new LockObject();
		}

		public IEnumerable<string> GetTableNames()
		{
			return typeof (SqlConnector).GetProperties()
				.Where(p => p.PropertyType.Name == "ITable`1")
				.Select(p => p.PropertyType.GetCustomAttribute<TableAttribute>().Name);
		}

		protected void Update<T>(T match) where T : Poco
		{
			if (Monitor.TryEnter(LockObj))
			{
				// Ensure the caller called lock(LockObj)
				throw new InvalidOperationException("Update() cannot be called from a method that does not lock the database.");
			}
			SqlConnector.Update(match);
		}

		public int ExecuteStatement(string statement)
		{
			int result;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				result = SqlConnector.ExecuteStatement(statement);
			}
			LockObj.LockMessage = "None";
			return result;
		}

		public DataTable ExecuteQuery(string query)
		{
			DataTable results;
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				results = SqlConnector.ExecuteQuery(query);
			}
			LockObj.LockMessage = "None";
			return results;
		}

		public void Dispose()
		{
			SqlConnector.Dispose();
		}

		public void Reset()
		{
			lock (LockObj)
			{
				LockObj.LockMessage = MiscTools.GetCurrentMethod();
				SqlConnector.Reset();
			}
		}
	}
}