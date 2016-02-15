using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BaggyBot.Monitoring;
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

		public static Configuration Config { get; private set; } = new Configuration();
		//private static Configuration ConfigOnDisk;

		public static LoadResult Load(string fileName)
		{
			if (!File.Exists(fileName))
			{
				Logger.Log(null, "Config file not found. Creating a new one...", LogLevel.Info);
				try
				{
					var exampleConfigStream =
						Assembly.GetExecutingAssembly().GetManifestResourceStream("BaggyBot.src.EmbeddedData.Configuration.example-config.yaml");
					exampleConfigStream.CopyTo(File.Create(fileName));
				}
				catch (Exception e) when (e is FileNotFoundException || e is FileLoadException || e is IOException)
				{
					Logger.Log(null, "Unable to load the default config file.", LogLevel.Error);
					Logger.Log(null, "Default config file not created. You might have to create one yourself.", LogLevel.Warning);
					return LoadResult.Failure;
				}

				return LoadResult.NewFileCreated;
			}

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

				if (Convert.GetTypeCode(aPropVal) == TypeCode.Object)
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
}
