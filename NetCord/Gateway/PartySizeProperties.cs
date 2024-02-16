﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetCord.Gateway;

[JsonConverter(typeof(PartySizePropertiesConverter))]
public partial class PartySizeProperties(int currentSize, int maxSize)
{
    public int CurrentSize { get; set; } = currentSize;

    public int MaxSize { get; set; } = maxSize;

    public class PartySizePropertiesConverter : JsonConverter<PartySizeProperties>
    {
        public override PartySizeProperties? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, PartySizeProperties value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.CurrentSize);
            writer.WriteNumberValue(value.MaxSize);
            writer.WriteEndArray();
        }
    }
}
