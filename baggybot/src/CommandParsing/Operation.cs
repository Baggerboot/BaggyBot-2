using System;
using System.Collections.Generic;
using System.Linq;

namespace BaggyBot.CommandParsing
{
	class Operation
	{
		private readonly List<Option> options;
		private readonly List<Argument> arguments;
		private bool hasRestArgument;
		private string restArgumentDefaultValue;

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

		public Operation AddArgument(string name)
		{
			arguments.Add(new Argument(name));
			return this;
		}

		public Operation AddRestArgument(string defaultValue = null)
		{
			hasRestArgument = true;
			restArgumentDefaultValue = defaultValue;
			return this;
		}

		public Operation AddFlag(string longForm, char? shortForm = null)
		{
			if (options.Count(opt => opt.Short != null && opt.Short == shortForm) > 0)
			{
				throw new InvalidOperationException($"An option with the short form \"-{shortForm}\" has already been defined.");
			}
			if (options.Count(opt => opt.Long != null && opt.Long == longForm) > 0)
			{
				throw new InvalidOperationException($"An option with the long form \"--{longForm}\" has already been defined.");
			}
			options.Add(new Flag(longForm, shortForm));
			return this;
		}

		public Operation AddKey(string longForm, char? shortForm = null)
		{
			return AddKey(longForm, null, typeof(string), shortForm);
		}

		public Operation AddKey(string longForm, DateTime defaultValue, char? shortForm = null)
		{
			return AddKey(longForm, defaultValue, typeof(DateTime), shortForm);
		}

		public Operation AddKey(string longForm, int defaultValue, char? shortForm = null)
		{
			return AddKey(longForm, defaultValue, typeof(int), shortForm);
		}

		public Operation AddKey(string longForm, string defaultValue, char? shortForm = null)
		{
			return AddKey(longForm, defaultValue, typeof(string), shortForm);
		}

		public Operation AddKey(string longForm, object defaultValue, Type defaultValueType, char? shortForm = null)
		{
			if (options.Count(opt => opt.Short != null && opt.Short == shortForm) > 0)
			{
				throw new InvalidOperationException($"An option with the short form \"-{shortForm}\" has already been defined.");
			}
			if (options.Count(opt => opt.Long != null && opt.Long == longForm) > 0)
			{
				throw new InvalidOperationException($"An option with the long form \"--{longForm}\" has already been defined.");
			}
			options.Add(new Key(longForm, defaultValue, defaultValueType, shortForm));
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
			var result = new OperationResult(operationName, restArgumentDefaultValue);

			foreach (var option in options)
			{
				var keyOpt = option as Key;
				var flagOpt = option as Flag;
				if (keyOpt != null)
				{
					result.InternalKeys[keyOpt.Long] = keyOpt.DefaultValue;
				}
				else if (flagOpt != null)
				{
					result.Flags[flagOpt.Long] = false;
				}
				else
				{
					throw new InvalidOperationException($"The option \"{option}\" is of an unknown type (\"{option.GetType().Name}\")");
				}
			}
			foreach (var argument in arguments)
			{
				result.Arguments[argument.Name] = argument.DefaultValue;
			}
			return result;
		}

		internal OperationResult Parse(IEnumerable<CommandComponent> components, string operationName, string fullCommand)
		{
			var result = BuildDefaultResult(operationName);

			if (components == null || !components.Any())
			{
				EnsureArgumentsAssigned(result);
				return result;
			}
			
			Key currentKey = null;
			foreach (var component in components)
			{
				if (currentKey != null)
				{
					// We've just parsed a key, now we can assign its value
					result.InternalKeys[currentKey.Long] = ConvertString(component.Value, currentKey.ValueType);
					currentKey = null;
				}
				else if (component.Value.StartsWith("--"))
				{
					// It looks like a long-form option, let's see if it is one.
					var option = GetLongOption(component.Value.Substring(2));

					if (option == null)
					{
						// Guess not. It might be an argument though, so let's return it.
						if (result.Arguments.IsFull && hasRestArgument)
						{
							result.RestArgument = fullCommand.Substring(component.Position);
							break;
						}
						else
						{
							result.Arguments.AssignArgument(component.Value);
						}
					}
					else
					{
						currentKey = SetOption(option, result);
					}
				}
				else if (component.Value.StartsWith("-"))
				{
					// It looks like a short-form option.
					var shortOptions = component.Value.Substring(1).ToCharArray();
					foreach (var shortOption in shortOptions)
					{
						var option = GetShortOption(shortOption);
						if (option == null)
						{
							result.Arguments.AssignArgument(component.Value);
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
					if (result.Arguments.IsFull && hasRestArgument)
					{
						result.RestArgument = fullCommand.Substring(component.Position);
						break;
					}
					else
					{
						result.Arguments.AssignArgument(component.Value);
					}
				}
			}
			EnsureArgumentsAssigned(result);
			return result;
		}

		private void EnsureArgumentsAssigned(OperationResult result)
		{
			foreach (var arg in arguments.Where(a => a.Required))
			{
				if (result.Arguments[arg.Name] == arg.DefaultValue)
				{
					throw new InvalidCommandException("A required argument was not assigned.", arg.Name);
				}
			}
		}

		private static object ConvertString(string value, Type type)
		{
			if (type == typeof(string)) return value;
			if (type == typeof(int)) return Convert.ToInt32(value);
			if (type == typeof(double)) return Convert.ToDouble(value);
			if (type == typeof(DateTime)) return Convert.ToDateTime(value);
			throw new ArgumentException($"Unsupported argument type: {type}");
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
