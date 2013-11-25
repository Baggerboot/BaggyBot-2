using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Linq;
using System.Data.SqlClient;
using BaggyBot.Database.EntityProvider;

namespace BaggyBot.Database.MS_SQL
{
	class MsEntityProvider : AbstractEntityProvider 
	{
		private StatsBotDataContext context;

		public override IQueryable UserCreds { get { return context.UserCredentials; }}
		public override IQueryable Quotes { get { return context.Quotes; }}
		public override IQueryable UserStats { get { return context.UserStatistics; }}
		public override IQueryable Emoticons { get { return context.Emoticons; }}
		public override IQueryable KeyValuePairs { get { return context.KeyValuePairs; }}
		public override IQueryable Urls { get { return context.Urls; }}
		public override IQueryable UserNames { get { return context.Names; }}
		public override IQueryable Words { get { return context.Words; } }
		public override IQueryable IrcLog
		{
			get { throw new NotImplementedException(); }
		}

		public override bool OpenConnection()
		{
			connection = new SqlConnection();
			connection.ConnectionString = Settings.Instance["sql_connection_string"];
			connection.Open();
			context = new StatsBotDataContext(connection);
			return true;
		}

		public override void SubmitChanges()
		{
			context.SubmitChanges();
		}

		public override void Dispose()
		{
			connection.Dispose();
		}
	}
}
