using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BaggyBot
{
	public class Settings
	{
		private readonly Dictionary<string, string> settings = new Dictionary<string, string>();

		private const string filename = "baggybot.settings";

		private static Settings instance;
		public static Settings Instance
		{
			get
			{
				if (instance == null) {
					instance = new Settings();

				}
				return instance;
			}
		}

		public string this[string key]
		{
			get
			{
				if (settings.ContainsKey(key)) {
					return settings[key];
				}
				return null;
			}
			set
			{
				settings[key] = value;
				SaveSettings();
			}
		}

		public bool SettingExists(string key)
		{
			return settings.ContainsKey(key);
		}

		public bool NewFileCreated
		{
			get;
			private set;
		}

		private Settings()
		{
			if (!File.Exists(filename)) {
				Logger.Log("No settings file found. Creating a new one.", LogLevel.Info);
				try {
					var stream = File.Create(filename);
					stream.Close();
					NewFileCreated = true;
				} catch (IOException e) {
					Logger.Log("Unable to create a new settings file, an exception ({0}) occurred: \"{1}\"", LogLevel.Error, true, e.GetType().Name, e.Message);
					return;
				}
			}
			using (var sr = new StreamReader(filename, Encoding.UTF8)) {
				while (!sr.EndOfStream) {
					var line = sr.ReadLine();
					if (line == string.Empty || line.StartsWith("#")) continue;
					var equalsIndex = line.IndexOf('=');
					var property = line.Substring(0, equalsIndex);
					var value = line.Substring(equalsIndex + 1);
					settings.Add(property, value);
				}
			}
		}
		private void SaveSettings()
		{
			using (var sw = new StreamWriter(filename)) {
				foreach (var property in settings) {
					sw.WriteLine(property.Key + "=" + property.Value);
				}
			}
		}

		internal void FillDefault()
		{
			this["irc_server"] = "irc.example.com";
			this["irc_port"] = "6667";
			this["irc_nick"] = "BaggyBot";
			this["irc_realname"] = "BaggyBot";
			this["irc_initial_channel"] = "#channel";
			this["irc_ident"] = "Dredger";

			this["sql_connection_string"] = "";
			this["sql_use_dblinq"] = "true";
			
			this["irc_flood_limit"] = "4";
			this["operator_nick"] = "";
			this["operator_ident"] = "";
			this["operator_host"] = "";
			this["operator_uid"] = "*";
			this["show_debug_log"] = "false";
			this["display_snag_message"] = "true";
			this["snag_silence_chance"] = "0.6";
			this["snag_chance"] = "1";
			this["snag_min_wait"] = "4";
			this["deployed"] = "true";
			this["wolfram_alpha_appid"] = "";
			this["enable_oidentd"] = "false";
		}
	}
}
