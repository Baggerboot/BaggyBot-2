using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace BaggyBotHost
{
	/// <summary>
	/// Provides read-only access to the baggybot.settings file
	/// </summary>
	public class Settings
	{
		private Dictionary<string, string> settings = new Dictionary<string, string>();

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
				} else {
					return null;
				}
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
				return;
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
	}
}
