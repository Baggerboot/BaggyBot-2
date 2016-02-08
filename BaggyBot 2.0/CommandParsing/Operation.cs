using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Community.CsharpSqlite;
using LinqToDB.DataProvider.SapHana;
using Microsoft.Scripting.Hosting.Shell;

namespace BaggyBot.CommandParsing
{
	class Operation
	{
		private readonly List<Option> options;
		private readonly List<Argument> arguments;

		public Operation()
		{
			options = new List<Option>();
			arguments = new List<Argument>();
		}
		public Operation(IEnumerable<Option> options, IEnumerable<Argument> arguments)
		{
			this.arguments = arguments.ToList();
			this.options = options.ToList();
		}

		public Operation AddArgument(string name, string defaultValue)
		{
			arguments.Add(new Argument(name, defaultValue));
			return this;
		}

		public Operation AddKey(string longForm, string defaultValue, char? shortForm = null)
		{
			if (options.Count(opt => opt.Short != null && opt.Short == shortForm) > 0)
			{
				throw new InvalidOperationException($"An option with the short form \"-{shortForm}\" has already been defined.");
			}
			if (options.Count(opt => opt.Long != null && opt.Long == longForm) > 0)
			{
				throw new InvalidOperationException($"An option with the long form \"--{longForm}\" has already been defined.");
			}
			options.Add(new Key(longForm, defaultValue, shortForm));
			return this;
		}

		/*public bool GetFlag(string longForm)
		{
			var matches = options.Where(opt => opt is Flag).Select(opt => ((Flag)opt).IsSet).ToArray();
			var count = matches.Length;
			if (count > 1)
			{
				throw new InvalidOperationException("Multiple options with the same long form have been defined.");
			}
			else if (count == 0)
			{
				throw new NonexistentOptionException("--" + longForm);
			}
			else
			{
				return matches[0];
			}
		}

		public string GetValue(string longForm)
		{
			var matches = options.Where(opt => opt is Key).Select(opt => ((Key)opt).Value).ToArray();
			var count = matches.Length;
			if (count > 1)
			{
				throw new InvalidOperationException("Multiple options with the same long form have been defined.");
			}
			else if (count == 0)
			{
				throw new NonexistentOptionException("--" + longForm);
			}
			else
			{
				return matches[0];
			}
		}*/
		
		private Option GetLongOption(string option)
		{
			var matches = options.Where(opt => opt.Long == option).ToArray();
			var count = matches.Length;
			if (count > 1)
			{
				throw new InvalidOperationException("Multiple options with the same long form have been defined.");
			}
			else if (count == 0)
			{
				//throw new NonexistentOptionException("--" + option);
				return null;
			}
			else
			{
				return matches[0];
			}
		}

		private Option GetShortOption(char option)
		{
			var matches = options.Where(opt => opt.Short == option).ToArray();
			var count = matches.Length;
			if (count > 1)
			{
				throw new InvalidOperationException("Multiple options with the same long form have been defined.");
			}
			else if (count == 0)
			{
				//throw new NonexistentOptionException("-" + option);
				return null;
			}
			else
			{
				return matches[0];
			}
		}

		private OperationResult BuildDefaultResult(string operationName)
		{
			var result = new OperationResult(operationName);

			foreach (var option in options)
			{
				var keyOpt = option as Key;
				var flagOpt = option as Flag;
				if (keyOpt != null)
				{
					result.Keys[keyOpt.Long] = keyOpt.DefaultValue;
				}
				else if (flagOpt != null)
				{
					result.Flags[flagOpt.Long] = false;
				}
				else
				{
					throw new InvalidOperationException($"The option \"{option.ToString()}\" is of an unknown type (\"{option.GetType().Name}\")");
				}
			}
			foreach (var argument in arguments)
			{
				result.Arguments[argument.Name] = argument.DefaultValue;
			}
			return result;
		}

		internal OperationResult Parse(IEnumerable<string> components, string operationName)
		{
			var result = BuildDefaultResult(operationName);

			if (!components.Any())
			{
				return result;
			}
			
			Key currentKey = null;
			foreach (var component in components)
			{
				if (currentKey != null)
				{
					result.Keys[currentKey.Long] = component;
					currentKey = null;
				}
				if (component.StartsWith("--"))
				{
					// It looks like a long-form option, let's see if it is one.
					var option = GetLongOption(component.Substring(2));

					if (option == null)
					{
						// Guess not. It might be an argument though, so let's return it.
						result.Arguments.AddArgument(component);
					}
					else
					{
						currentKey = SetOption(option, result);
					}
				}
				else if (component.StartsWith("-"))
				{
					// It looks like a short-form option.
					var shortOptions = component.Substring(1).ToCharArray();
					foreach (var shortOption in shortOptions)
					{
						var option = GetShortOption(shortOption);
						if (option == null)
						{
							result.Arguments.AddArgument(component);
						}
						else
						{
							currentKey = SetOption(option, result);
						}
					}
				}
				else
				{
					// This doesn't look like an option at all, so it's probably an argument.
					result.Arguments.AddArgument(component);
				}
			}
			return result;
		}

		private Key SetOption(Option option, OperationResult result)
		{
			Key currentKey = null;
			var keyOpt = option as Key;
			var flagOpt = option as Flag;

			if (keyOpt != null)
			{
				currentKey = keyOpt;
			}
			else if (flagOpt != null)
			{
				result.Flags[flagOpt.Long] = true;
			}
			else
			{
				throw new InvalidOperationException($"The option \"{option.ToString()}\" is of an unknown type (\"{option.GetType().Name}\")");
			}
			return currentKey;
		}
	}
}
