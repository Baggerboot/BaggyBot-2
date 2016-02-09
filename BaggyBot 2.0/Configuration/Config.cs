using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using BaggyBot.Commands;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace BaggyBot.Configuration
{
	public static class ConfigManager
	{
		public enum LoadResult
		{
			Success,
			Failure,
			NewFileCreated
		}

		private static string fileName;

		public static Configuration Config { get; private set; }
		//private static Configuration ConfigOnDisk;

		public static LoadResult Load(string fileName)
		{
			if (!File.Exists(fileName))
			{
				Logger.Log(null, "Config file not found. Creating a new one...", LogLevel.Info);
				try
				{
					var exampleConfigStream =
						Assembly.GetExecutingAssembly().GetManifestResourceStream("BaggyBot.Embedded.example-config.yaml");
					exampleConfigStream.CopyTo(File.Create(fileName));
				}
				catch (Exception e) when (e is FileNotFoundException || e is FileLoadException)
				{
					Logger.Log(null, "Unable to load the default config file.", LogLevel.Error);
					Logger.Log(null, "Default config file not created. You might have to create one yourself.", LogLevel.Warning);
					return LoadResult.Failure;
				}

				return LoadResult.NewFileCreated;
			}
			ConfigManager.fileName = fileName;

			var deserialiser = new Deserializer(namingConvention: new HyphenatedNamingConvention(), ignoreUnmatched: false);
			using (var reader = File.OpenText(fileName))
			{
				Config = deserialiser.Deserialize<Configuration>(reader);
			}
			/*using (var reader = File.OpenText(fileName))
			{
				ConfigOnDisk = deserialiser.Deserialize<Configuration>(reader);
			}*/
			return LoadResult.Success;
		}

		/*public static void Save()
		{
			var diff = CompareMembers(new Dictionary<string, object>(), "", Config, ConfigOnDisk);

			var hyphenated = diff.Select(prop =>
				new KeyValuePair<string[], object>(
						prop.Key.Substring(1)
						.Split('.')
						.Select(member => member.FromCamelCase("-")).ToArray(),
						prop.Value
					)).ToDictionary(pair => pair.Key, pair => pair.Value);


			var stream = new YamlStream();
			using (var reader = new StreamReader(File.Open(fileName, FileMode.Open)))
			{
				stream.Load(reader);
			}

			var doc = stream.Documents.First();
			var rootNode = (YamlMappingNode)doc.RootNode;

			foreach (var changedProperty in hyphenated)
			{
				YamlNode currentNode = rootNode;
				foreach (var member in changedProperty.Key)
				{
					currentNode = ((YamlMappingNode)currentNode).Children[new YamlScalarNode(member)];
				}
				var newValue = changedProperty.Value.ToString();
				((YamlScalarNode)currentNode).Value = newValue;
			}

			using (var writer = new StreamWriter(File.Open(fileName, FileMode.Open)))
			{
				stream.Save(writer);
			}
			Debugger.Break();
		}*/

		/// <summary>
		/// Recurses through the properties of an object, comparing them against the properties of another object,
		/// adding all unmatched properties to a list.
		/// </summary>
		private static Dictionary<string, object> CompareMembers(Dictionary<string, object> properties, string fullName, object a, object b)
		{
			var aType = a.GetType();
			var bType = b.GetType();
			if (aType != bType)
			{
				throw new ArgumentException("A and B are not of the same type.");
			}

			foreach (var aProp in aType.GetProperties())
			{
				var aPropVal = aProp.GetValue(a);
				var bPropVal = aProp.GetValue(b);

				if (System.Convert.GetTypeCode(aPropVal) == TypeCode.Object)
				{
					if (aPropVal is IList)
					{
						// TODO: Implement IList comparison.
						// For now, we'll simply ignore changed lists.
						//CompareMembers(properties, fullName + "." + aProp.Name, aPropVal, bPropVal);
						/* bEnumerable = ((IEnumerable) bPropVal).GetEnumerator();

						foreach (var member in (IEnumerable) aPropVal)
						{
							var bMember = bEnumerable.Current;
							bEnumerable.MoveNext();
						}*/
					}
					else
					{
						CompareMembers(properties, fullName + "." + aProp.Name, aPropVal, bPropVal);
					}
				}
				else
				{
					if (!aPropVal.Equals(bPropVal))
					{
						properties.Add(fullName + "." + aProp.Name, aPropVal);
					}
				}
			}
			return properties;
		}
	}

	public class Configuration
	{
		public bool DebugMode { get; set; } = false;
		public bool LogPerformance { get; set; } = false;
		public int FloodLimit { get; set; } = 4;

		public Interpreters Interpreters { get; private set; } = new Interpreters();
		public Backend Backend { get; private set; } = new Backend();
		public Integrations Integrations { get; private set; } = new Integrations();
		public Quotes Quotes { get; private set; } = new Quotes();
		public Logging Logging { get; private set; } = new Logging();
		public Metadata Metadata { get; private set; } = new Metadata();

		public Identity[] Identities { get; set; } = new Identity[0];
		public Operator[] Operators { get; set; } = new Operator[0];
		public Server[] Servers { get; set; } = new Server[0];
	}

	public class Interpreters
	{
		public bool Enabled { get; set; } = true;
		public ReadEvaluatePrintCommand.InterpreterSecurity StartupSecurityLevel { get; set; } = ReadEvaluatePrintCommand.InterpreterSecurity.Block;
	}

	public class Quotes
	{
		public double SilentQuoteChance { get; set; } = 0.6;
		public int MinDelayHours { get; set; } = 4;
		public double Chance { get; set; } = 0.015;
		public bool AllowQuoteNotifications { get; set; } = false;
	}

	public class Server
	{
		public string Host { get; set; }
		public int Port { get; set; } = 6667;
		public string Password { get; set; } = null;
		public Identity Identity { get; set; } = new Identity();
		public Operator[] Operators { get; set; } = new Operator[0];
		public string[] AutoJoinChannels { get; set; } = new string[0];
		public bool UseTls { get; set; } = false;
		public bool VerifyCertificate { get; set; } = true;
		public string[] CompatModes { get; set; } = new string[0];
	}

	public class Operator
	{
		public string Nick { get; set; } = "*";
		public string Ident { get; set; } = "*";
		public string Host { get; set; } = "*";
		public string Uid { get; set; } = "*";
	}

	public class Identity
	{
		public string Nick { get; set; } = "BaggyBot";
		public string Ident { get; set; } = "Dredger";
		public string RealName { get; set; } = "BaggyBot Statistics Collector";
		public bool Hidden { get; set; } = true;
	}

	public class Integrations
	{
		public WolframAlpha WolframAlpha { get; set; } = new WolframAlpha();
	}

	public class Backend
	{
		public string ConnectionString { get; set; }
	}

	public class Metadata
	{
		public string BotVersion { get; set; } = Bot.Version;
		public string ConfigVersion { get; set; } = Bot.ConfigVersion;
	}

	public class Logging
	{
		public string LogFile { get; set; } = "baggybot.log";
		public bool ShowDebug { get; set; } = false;
	}

	public class WolframAlpha
	{
		public string AppId { get; set; }
	}
	
	public static class StringExtensions
	{
		private static string ToCamelOrPascalCase(string str, Func<char, char> firstLetterTransform)
		{
			string input = str;
			string pattern = "([_\\-])(?<char>[a-z])";
			int num = 1;
			string str1 = Regex.Replace(input, pattern, (MatchEvaluator)(match => match.Groups["char"].Value.ToUpperInvariant()), (RegexOptions)num);
			return firstLetterTransform(str1[0]).ToString() + str1.Substring(1);
		}

		/// <summary>
		/// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to
		/// camel case (thisIsATest). Camel case is the same as Pascal case, except the first letter
		/// is lowercase.
		/// </summary>
		/// <param name="str">String to convert</param>
		/// <returns>
		/// Converted string
		/// </returns>
		public static string ToCamelCase(this string str)
		{
			return StringExtensions.ToCamelOrPascalCase(str, new Func<char, char>(char.ToLowerInvariant));
		}

		/// <summary>
		/// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to
		/// pascal case (ThisIsATest). Pascal case is the same as camel case, except the first letter
		/// is uppercase.
		/// </summary>
		/// <param name="str">String to convert</param>
		/// <returns>
		/// Converted string
		/// </returns>
		public static string ToPascalCase(this string str)
		{
			return StringExtensions.ToCamelOrPascalCase(str, new Func<char, char>(char.ToUpperInvariant));
		}

		/// <summary>
		/// Convert the string from camelcase (thisIsATest) to a hyphenated (this-is-a-test) or
		/// underscored (this_is_a_test) string
		/// </summary>
		/// <param name="str">String to convert</param><param name="separator">Separator to use between segments</param>
		/// <returns>
		/// Converted string
		/// </returns>
		public static string FromCamelCase(this string str, string separator)
		{
			str = char.ToLower(str[0]).ToString() + str.Substring(1);
			str = Regex.Replace(StringExtensions.ToCamelCase(str), "(?<char>[A-Z])", (MatchEvaluator)(match => separator + match.Groups["char"].Value.ToLowerInvariant()));
			return str;
		}
	}
}
