using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaggyBot.Plugins
{
	public class ServerCapabilities
	{
		/// <summary>
		/// Users can be mentioned by prefixing their name with an '@' sign.
		/// </summary>
		public bool AtMention { get; set; }
		/// <summary>
		/// Newlines (<code>\n</code>) in messages are supported.
		/// </summary>
		public bool AllowsMultilineMessages { get; set; }

		/// <summary>
		/// Messages can be edited by their owner or someone with the appropriate permissions.
		/// </summary>
		public bool CanEditMessages { get; set; }
		/// <summary>
		/// Messages can be deleted by their owner or someone with the appropriate permissions.
		/// </summary>
		public bool CanDeleteMessages { get; set; }

		/// <summary>
		/// Other chat clients are able to correctly handle special unicode
		/// characters (such as mathematical symbols).
		/// </summary>
		public bool SupportsSpecialCharacters { get; set; } = true;

		/// <summary>
		/// Attaching photos is supported.
		/// </summary>
		public bool CanAttachPictures { get; set; } = false;
		/// <summary>
		/// Photos should be re-uploaded to imgur before being attached.
		/// </summary>
		public bool RequireReupload { get; set; } = false;
	}
}
