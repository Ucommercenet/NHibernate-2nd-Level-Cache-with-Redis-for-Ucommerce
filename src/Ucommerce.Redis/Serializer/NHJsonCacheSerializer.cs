using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate.Cache;
using NHibernate.Cache.Entry;
using NHibernate.Caches.Common;
using StackExchange.Redis;

namespace Ucommerce.Redis.Serializer
{
    // IMPORTANT: This might not be a complete implementation.For example, if you use
    // custom NHibernate types, you will have to modify this (e.g. inside of
    // CustomContractResolver.CreateObjectContract and maybe writing a custom
    // JsonConverter) to support your custom types. You'll want to test this
    // implementation with your data and use cases.
    public class NhJsonCacheSerializer : CacheSerializerBase
    {
        private readonly JsonSerializerSettings _settings;

        public NhJsonCacheSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            _settings.Converters.Add(new ExplicitTypesConverter());
            _settings.ContractResolver = new CustomContractResolver();
        }

        public override object Deserialize(byte[] data)
        {
            if (data is null)
            {
                return null;
            }

            var str = Encoding.Default.GetString(data);
            var result = JsonConvert.DeserializeObject(str, _settings);
            return result;
        }

        public override byte[] Serialize(object value)
        {
            if (value is null) return RedisValue.Null;

            var result = JsonConvert.SerializeObject(value, Formatting.None, _settings);
            return Encoding.Default.GetBytes(result);
        }

        private class CustomContractResolver : DefaultContractResolver
        {
            private static readonly ISet<Type> _nhibernateCacheObjectTypes = new HashSet<Type>(new[]
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
                if (_nhibernateCacheObjectTypes.Contains(objectType))
                {
                    result.DefaultCreator = () => FormatterServices.GetUninitializedObject(objectType);
                }

                return result;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                if (!_nhibernateCacheObjectTypes.Contains(type))
                    return base.CreateProperties(type, memberSerialization);
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
                                           })
                                   .ToList();
                return result;
            }
        }
    }
}
