﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RandomizerCore.Logic.StateLogic
{
    public readonly record struct RawStateField(string Name, StateFieldType Type, object? DefaultValue = null);

    [JsonConverter(typeof(CompactStateFieldConverter))]
    public readonly record struct CompactStateField(string Name, object? DefaultValue = null)
    {
        public CompactStateField(StateBool sb) : this(sb.Name, sb.DefaultValue == true ? true : null) { }
        public CompactStateField(StateInt si) : this(si.Name, si.DefaultValue != 0 ? si.DefaultValue : null) { }

        public class CompactStateFieldConverter : JsonConverter<CompactStateField>
        {
            public override CompactStateField ReadJson(JsonReader reader, Type objectType, CompactStateField existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                JToken t = JToken.Load(reader);
                if (t.Type == JTokenType.String) return new(t.ToObject<string>());
                else
                {
                    JObject jo = (JObject)t;
                    string name = jo.GetValue("Name", StringComparison.OrdinalIgnoreCase).Value<string>();
                    object defaultValue = jo.GetValue("DefaultValue", StringComparison.OrdinalIgnoreCase).ToObject<object>(serializer);
                    return new(name, defaultValue);
                }
            }

            public override void WriteJson(JsonWriter writer, CompactStateField value, JsonSerializer serializer)
            {
                if (value.DefaultValue is null) writer.WriteValue(value.Name);
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName(nameof(Name));
                    serializer.Serialize(writer, value.Name);
                    writer.WritePropertyName(nameof(DefaultValue));
                    serializer.Serialize(writer, value.DefaultValue);
                    writer.WriteEndObject();
                }
            }
        }
    }
    public class RawStateData
    {
        public RawStateData() { }

        public RawStateData(StateManager sm)
        {
            Fields = new()
            {
                { StateFieldType.Bool.ToString(), sm.Bools.Select(sb => new CompactStateField(sb)).ToList()  },
                { StateFieldType.Int.ToString(), sm.Ints.Select(si => new CompactStateField(si)).ToList()  },
            };
            Tags = sm.TagLookup.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(sf => sf.Name).ToList());
        }

        public Dictionary<string, List<CompactStateField>> Fields;
        public Dictionary<string, List<string>> Tags;
    }
}
