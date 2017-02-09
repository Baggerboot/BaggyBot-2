namespace BaggyBot.Formatting
{
	/// <summary>
	/// Defines a simple, consistent formatting syntax.
	/// Formatting control codes are represented as Unicode Private Use Area characters.
	/// Plugins define or implement a formatter class which will then process these control codes
	/// and turn them into the formatting codes expected by the plugin.
	/// </summary>
	static class Frm
	{
		/// <summary>
		/// Italic
		/// </summary>
		public const string I = "\ufb00";
		/// <summary>
		/// Bold
		/// </summary>
		public const string B = "\ufb10";
		/// <summary>
		/// Underline
		/// </summary>
		public const string U = "\ufb20";
		/// <summary>
		/// Strikethrough
		/// </summary>
		public const string S = "\ufb30";
		/// <summary>
		/// Quote
		/// </summary>
		public const string Q = "\ufb40";
		/// <summary>
		/// Code
		/// </summary>
		public const string M = "\ufb50";
		/// <summary>
		/// Mulitiline code block
		/// </summary>
		public const string MMultiline = "\ufb60";
	}
}
