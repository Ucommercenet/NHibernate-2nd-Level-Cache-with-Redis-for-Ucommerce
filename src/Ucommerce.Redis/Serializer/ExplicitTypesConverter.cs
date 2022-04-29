using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ucommerce.Redis.Serializer
{
// By default, JSON.NET will always use Int64/Double when deserializing numbers
    // since there isn't an easy way to detect the proper number size. However,
    // because NHibernate does casting to the correct number type, it will fail.
    // Adding the type to the serialize object is what the "TypeNameHandling.All"
    // option does except that it doesn't include certain types.
    internal class ExplicitTypesConverter : JsonConverter
    {
        // We shouldn't have to account for Nullable<T> because the serializer
        // should see them as null.
        private static readonly ISet<Type> _explicitTypes = new HashSet<Type>(new[]
        {
            // Int64 and Double are correctly serialzied/deserialized by JSON.NET.
            typeof(byte), typeof(sbyte),
            typeof(ushort), typeof(uint), typeof(ulong),
            typeof(short), typeof(int),
            typeof(float), typeof(decimal),
            typeof(Guid)
        });

        // JSON.NET will deserialize a value with the explicit type when 
        // the JSON object exists with $type/$value properties. So, we 
        // don't need to implement reading.
        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return _explicitTypes.Contains(objectType);
        }

        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                        object existingValue,
                                        JsonSerializer serializer)
        {
            // CanRead is false.
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            var typeName = value.GetType()
                                .FullName;
            writer.WriteValue(typeName);
            writer.WritePropertyName("$value");
            writer.WriteValue(value);
            writer.WriteEndObject();
        }
    }
}
