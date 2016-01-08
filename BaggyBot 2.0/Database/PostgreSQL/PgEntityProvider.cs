using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BaggyBot.Database.EntityProvider;

namespace BaggyBot.Database.PostgreSQL
{
	class PgEntityProvider : AbstractEntityProvider
	{
		private dynamic context;

		public override IQueryable UserCreds { get { return context.UserCReds; } }
		public override IQueryable Quotes { get { return context.Quotes; } }
		public override IQueryable UserStats { get { return context.UserStatistics; } }
		public override IQueryable Emoticons { get { return context.EmOtIcons; } }
		public override IQueryable KeyValuePairs { get { return context.KeyValuePairs; } }
		public override IQueryable Urls { get { return context.URLS; } }
		public override IQueryable UserNames { get { return context.Names; } }
		public override IQueryable Words { get { return context.Words; } }
		public override IQueryable IrcLog { get { return context.IrcLog; } }

		public override bool OpenConnection()
		{
			var connectionString = Settings.Instance["sql_connection_string"];
			if (string.IsNullOrWhiteSpace(connectionString)) {
				Logger.Log(this, "Unable to connect to the SQL database: No connection specified.", LogLevel.Error);
				return false;
			}
			//context = new BaggyBoT(connectionString);
			base.connection = context.Connection;
            context.Connection.Open();

		    return true;
		}
		public override void SubmitChanges()
		{
			context.SubmitChanges();
			return;
		}

		public override void Dispose()
		{
            if (context != null)
            {
                context.Dispose();
            }
			if(connection != null) connection.Dispose();
		}
	}
}
