using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace BaggyBot
{
	class Settings
	{
		private Dictionary<string, string> settings = new Dictionary<string, string>();
		private bool settingsChanged;

		private const string filename = "baggybot.settings";

		public string this[string key]
		{
			get
			{
				return settings[key];
			}
			set
			{
				settings[key] = value;
				SaveSettings();
			}
		}

		public Settings()
		{
			using (var sr = new StreamReader(filename))
			{
				while (!sr.EndOfStream) {
					var line = sr.ReadLine();
					if (line == string.Empty || line.StartsWith("#")) continue;
					var equalsIndex = line.IndexOf('=');
					var property = line.Substring(0, equalsIndex);
					var value = line.Substring(equalsIndex+1);
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
	}
}
