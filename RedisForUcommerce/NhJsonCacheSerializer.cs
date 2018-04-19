using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate.Cache;
using NHibernate.Cache.Entry;
using NHibernate.Caches.Redis;
using StackExchange.Redis;

namespace RedisForUcommerce
{
    // IMPORTANT: This might not be a complete implementation. For example, if you use
    // custom NHibernate types, you will have to modify this (e.g. inside of
    // CustomContractResolver.CreateObjectContract and maybe writing a custom
    // JsonConverter) to support your custom types. You'll want to test this
    // implementation with your data and use cases.
    public class NhJsonCacheSerializer : ICacheSerializer
    {
        // By default, JSON.NET will always use Int64/Double when deserializing numbers
        // since there isn't an easy way to detect the proper number size. However,
        // because NHibernate does casting to the correct number type, it will fail.
        // Adding the type to the serialize object is what the "TypeNameHandling.All"
        // option does except that it doesn't include certain types.
        private class ExplicitTypesConverter : JsonConverter
        {
            // We shouldn't have to account for Nullable<T> because the serializer
            // should see them as null.
            private static readonly ISet<Type> explicitTypes = new HashSet<Type>(new[]
            {
            // Int64 and Double are correctly serialzied/deserialized by JSON.NET.
            typeof(Byte), typeof(SByte),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
            typeof(Int16), typeof(Int32),
            typeof(Single), typeof(Decimal),
            typeof(Guid)
        });

            public override bool CanConvert(Type objectType)
            {
                return explicitTypes.Contains(objectType);
            }

            // JSON.NET will deserialize a value with the explicit type when 
            // the JSON object exists with $type/$value properties. So, we 
            // don't need to implement reading.
            public override bool CanRead { get { return false; } }

            public override bool CanWrite { get { return true; } }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                // CanRead is false.
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("$type");
                var typeName = value.GetType().FullName;
                writer.WriteValue(typeName);
                writer.WritePropertyName("$value");
                writer.WriteValue(value);
                writer.WriteEndObject();
            }
        }

        private class CustomContractResolver : DefaultContractResolver
        {
            private static readonly ISet<Type> nhibernateCacheObjectTypes = new HashSet<Type>(new[]
            {
            typeof(CachedItem),
            typeof(CacheLock),
            typeof(CacheEntry),
            typeof(CollectionCacheEntry)
        });

            protected override JsonObjectContract CreateObjectContract(Type objectType)
            {
                var result = base.CreateObjectContract(objectType);

                // By default JSON.NET will only use the public constructors that 
                // require parameters such as ISessionImplementor. Because the 
                // NHibernate cache objects use internal constructors that don't 
                // do anything except initialize the fields, it's much easier 
                // (no constructor lookup) to just get an uninitialized object and 
                // fill in the fields.
                if (nhibernateCacheObjectTypes.Contains(objectType))
                {
                    result.DefaultCreator = () => FormatterServices.GetUninitializedObject(objectType);
                }

                return result;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                if (nhibernateCacheObjectTypes.Contains(type))
                {
                    // By default JSON.NET will serialize the NHibernate objects with
                    // their public properties. However, the backing fields/property
                    // names don't always match up. Therefore, we *only* use the fields
                    // so that we can get/set the correct value.
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Select(f => base.CreateProperty(f, memberSerialization));

                    var result = fields.Select(p =>
                    {
                        p.Writable = true;
                        p.Readable = true;
                        return p;
                    }).ToList();
                    return result;
                }
                else
                {
                    return base.CreateProperties(type, memberSerialization);
                }
            }
        }

        private readonly JsonSerializerSettings settings;

        public NhJsonCacheSerializer()
        {
            this.settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            settings.Converters.Add(new ExplicitTypesConverter());
            settings.ContractResolver = new CustomContractResolver();
        }

        public RedisValue Serialize(object value)
        {
            if (value == null) return RedisValue.Null;

            var result = JsonConvert.SerializeObject(value, Formatting.None, settings);
            return result;
        }

        public object Deserialize(RedisValue value)
        {
            if (value.IsNull) return null;

            var result = JsonConvert.DeserializeObject(value, settings);
            return result;
        }
    }
}