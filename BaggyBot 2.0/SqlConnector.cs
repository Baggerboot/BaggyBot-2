using System;
using System.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
namespace BaggyBot
{
	class SqlConnector
	{
		private MySqlConnection connection;

		public SqlConnector()
		{
			Initialize();
		}

		private void Initialize()
		{
			Settings set = Settings.Instance;
			string uid = set["sql_user"];
			string password = set["sql_password"];
			string server = set["sql_host"];
			string database = set["sql_database"];
			string connectionString = String.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};", server, database, uid, password);

			connection = new MySqlConnection(connectionString);
		}

		internal void InitializeDatabase()
		{
			Logger.Log("Attempting to initialize the database if this hasn't been done yet");

			List<string> createStmts = new List<string>();

			createStmts.Add( 
			@"CREATE  TABLE IF NOT EXISTS `usercreds` (
			  `id` INT NOT NULL AUTO_INCREMENT ,
			  `user_id` INT NOT NULL ,
			  `nick` VARCHAR(45) NOT NULL ,
			  `ident` VARCHAR(16) NOT NULL ,
			  `hostmask` VARCHAR(128) NOT NULL ,
			  `ns_login` VARCHAR(45) NULL ,
			  PRIMARY KEY (`id`) ,
			  UNIQUE INDEX `id_UNIQUE` (`id` ASC) )
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			createStmts.Add(
			@"CREATE  TABLE IF NOT EXISTS `userstats` (
			  `user_id` INT NOT NULL ,
			  `lines` INT NOT NULL ,
			  `words` INT NOT NULL ,
			  `actions` INT NOT NULL ,
			  `profanities` INT NOT NULL ,
			  PRIMARY KEY (`user_id`) ,
			  UNIQUE INDEX `user_id_UNIQUE` (`user_id` ASC) )
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			createStmts.Add(
			@"CREATE TABLE IF NOT EXISTS `quotes` (
			  `id` INT NOT NULL AUTO_INCREMENT,
			  `user_id` INT NOT NULL ,
			  `quote` TEXT NOT NULL ,
			  PRIMARY KEY (`id`) )
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			createStmts.Add(
			@"CREATE  TABLE IF NOT EXISTS `var` (
			  `id` INT NOT NULL AUTO_INCREMENT ,
			  `key` VARCHAR(45) NOT NULL ,
			  `value` INT NOT NULL ,
			  PRIMARY KEY (`id`) ,
			  UNIQUE INDEX `id_UNIQUE` (`id` ASC) ,
			  UNIQUE INDEX `key_UNIQUE` (`key` ASC) )
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			createStmts.Add(
			@"CREATE  TABLE IF NOT EXISTS `emoticons` (
			  `id` INT NOT NULL AUTO_INCREMENT ,
			  `emoticon` VARCHAR(45) NOT NULL ,
			  `uses` INT NOT NULL ,
			  `last_used_by` INT NOT NULL ,
			  PRIMARY KEY (`id`) ,
			  UNIQUE INDEX `id_UNIQUE` (`id` ASC) ,
			  UNIQUE INDEX `emoticon_UNIQUE` (`emoticon` ASC) )
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			createStmts.Add(
			@"CREATE  TABLE IF NOT EXISTS `urls` (
			  `id` INT NOT NULL AUTO_INCREMENT ,
			  `url` VARCHAR(220) NOT NULL ,
			  `uses` INT NOT NULL ,
			  `last_used_by` INT NOT NULL ,
			  `last_usage` TEXT NOT NULL ,
			  PRIMARY KEY (`id`) ,
			  UNIQUE INDEX `urlid_UNIQUE` (`id` ASC) ,
			  UNIQUE INDEX `url_UNIQUE` (`url` ASC) )
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			createStmts.Add(
			@"CREATE  TABLE IF NOT EXISTS `words` (
			  `id` INT NOT NULL AUTO_INCREMENT ,
			  `word` VARCHAR(220) NOT NULL ,
			  `uses` INT NOT NULL ,
			  PRIMARY KEY (`id`) ,
			  UNIQUE INDEX `id_UNIQUE` (`id` ASC) ,
			  UNIQUE INDEX `word_UNIQUE` (`word` ASC) )
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			createStmts.Add(
			@"CREATE  TABLE IF NOT EXISTS `names` (
			  `user_id` INT NOT NULL ,
			  `name` VARCHAR(90) NOT NULL ,
			  PRIMARY KEY (`user_id`) ,
			  UNIQUE INDEX `user_id_UNIQUE` (`user_id` ASC) );
			DEFAULT CHARACTER SET = utf8
			COLLATE = utf8_bin;");

			foreach (string str in createStmts) {
				ExecuteStatement(str);
			}

			Logger.Log("Done.");
		}

		public bool OpenConnection()
		{
			try {
				connection.Open();
				return true;
			} catch (MySqlException e) {
				Logger.Log("Failed to open a connection to the SQL database. Error mesage: " + e.Message, LogLevel.Error);
				return false;
			} catch (InvalidOperationException e) {
				Logger.Log("Failed to open a connection to the SQL database. Error message: " + e.Message, LogLevel.Error);
				return true;
			}
		}

		public bool CloseConnection()
		{
			try {
				connection.Close();
				return true;
			} catch (MySqlException ex) {
				Logger.Log("Failed to close the connection to the SQL database. Error mesage: " + ex.Message, LogLevel.Error);
				return false;
			}
		}

		internal int ExecuteStatement(string statement)
		{
			/*if (busy) {
				while (busy) {
					Thread.Sleep(10);
				}
				Logger.Log("Finished waiting for other thread to release resources.");
			}
			busy = true;*/

			lock (connection) {

				int result;
				using (MySqlCommand cmd = new MySqlCommand(statement, connection)) {
					result = cmd.ExecuteNonQuery();
				}
				return result;
			}
		}

		private DataView Select(string query)
		{
			lock (connection) {
				DataView ret;
				using (MySqlCommand cmd = new MySqlCommand(query, connection)) {
					using (MySqlDataAdapter da = new MySqlDataAdapter(cmd)) {
						using (DataSet ds = new DataSet()) {
							da.Fill(ds);
							ret = ds.Tables[0].DefaultView;
						}
					}
				}
				return ret;
			}
		}
		/// Selects a vector and returns it in the form of an array.
		/// The data returned may only contain one column, or else an InvalidOperationException will be thrown.
		/// </summary>
		internal T[] SelectVector<T>(string query)
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
		}

		internal T SelectOne<T>(string query)
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
		}
	}
}
