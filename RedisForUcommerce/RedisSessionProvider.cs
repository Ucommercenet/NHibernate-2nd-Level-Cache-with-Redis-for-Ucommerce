using NHibernate;
using NHibernate.Caches.Redis;
using StackExchange.Redis;
using UCommerce.EntitiesV2;

namespace RedisForUcommerce
{
    public class RedisSessionProvider : SessionProvider
    {
        protected override ISessionFactory CreateSessionFactory(bool enableCache, string cacheProvider)
        {
            if (enableCache)
            {
                // Or use your IoC container to wire this up.
                var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379");
                RedisCacheProvider.SetConnectionMultiplexer(connectionMultiplexer);

                var options = new RedisCacheProviderOptions()
                {
                    Serializer = new NhJsonCacheSerializer(),
                };
                RedisCacheProvider.SetOptions(options);

                
            }
            
            var sessionFactory = base.CreateSessionFactory(enableCache, cacheProvider);

            return sessionFactory;
        }
    }
}