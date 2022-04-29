using NHibernate;
using NHibernate.Caches.StackExchangeRedis;
using StackExchange.Redis;
using Ucommerce.EntitiesV2;
using Ucommerce.Infrastructure.Components.Windsor;
using Ucommerce.Redis.Serializer;

namespace Ucommerce.Redis
{
    public class RedisSessionProvider : SessionProvider
    {
        [Mandatory]
        public string RedisConnectionString { get; set; }

        protected override ISessionFactory CreateSessionFactory(bool enableCache, string cacheProvider)
        {
            if (enableCache)
            {
                RedisCacheProvider.DefaultCacheConfiguration = new RedisCacheConfiguration
                {
                    DefaultUseSlidingExpiration = true, 
                    Serializer = new NhJsonCacheSerializer(),
                    ConnectionMultiplexerProvider = new ConnectionMultiplexerProvider()
                };
            }

            var sessionFactory = base.CreateSessionFactory(enableCache, cacheProvider);

            return sessionFactory;
        }
    }
}
