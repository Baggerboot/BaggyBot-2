using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BaggyBot.Commands.Import.SlackHistory
{
	internal class MillisecondEpochConverter : DateTimeConverterBase
	{
		private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteRawValue(((int)(((DateTime)value - epoch).TotalMilliseconds)).ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.Value == null) { return null; }

			return epoch.AddMilliseconds(double.Parse(reader.Value.ToString(), CultureInfo.InvariantCulture));
		}
	}

	internal class SecondEpochConverter : DateTimeConverterBase
	{
		private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteRawValue(((DateTime)value - epoch).TotalSeconds.ToString(CultureInfo.InvariantCulture));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.Value == null) { return null; }

			return epoch.AddSeconds(double.Parse(reader.Value.ToString(), CultureInfo.InvariantCulture));
		}
	}
}
