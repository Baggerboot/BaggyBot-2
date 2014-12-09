using System;
using BaggyBot.Database.MS_SQL;
using BaggyBot.Database.PostgreSQL;

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
					return new MsEntityProvider();
				case SupportedDatabases.PostgreSQL:
					return new PgEntityProvider();
				default:
					throw new ArgumentException("Invalid database type supplied. This database type is not supported.");
			}
		}
	}
}
