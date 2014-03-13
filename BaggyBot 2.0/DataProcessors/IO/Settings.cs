﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace BaggyBot
{
	public class Settings
	{
		private Dictionary<string, string> settings = new Dictionary<string, string>();

		private const string filename = "baggybot.settings";

		private static Settings instance;
		public static Settings Instance
		{
			get { 
				if (instance == null) 
					instance = new Settings(); 
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
					// TODO: This should be null, not string.Empty. Figure out if changing this to null breaks anything.
					return string.Empty;
				}
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

		private Settings()
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
