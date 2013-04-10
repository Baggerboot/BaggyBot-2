using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot
{
	class RecordNullException : Exception
	{
		public RecordNullException()
			:base("Database returned NULL")
		{
			
		}
	}
}
