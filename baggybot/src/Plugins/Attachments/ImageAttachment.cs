using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Plugins
{
	public class ImageAttachment : Attachment
	{
		public string Url { get; }

		public ImageAttachment(string url)
		{
			Url = url;
		}

		public override string GetPlaintext()
		{
			return Url;
		}
	}
}
