using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Database.EntityProvider
{
	enum SupportedDatabases
	{
		MsSql,
		PostgreSQL
	}

	class EntityProviderFactory
	{
		public AbstractEntityProvider CreateEntityProvider(SupportedDatabases dbType)
		{
			switch (dbType) {
				case SupportedDatabases.MsSql:
					return new MS_SQL.MsEntityProvider();
				case SupportedDatabases.PostgreSQL:
					return new PostgreSQL.PgEntityProvider();
				default:
					throw new ArgumentException("Invalid database type supplied. This database type is not supported.");
			}
		}
	}
}
