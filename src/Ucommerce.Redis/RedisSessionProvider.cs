using System;
using System.Diagnostics;
using NHibernate;
using NHibernate.Caches.StackExchangeRedis;
using Ucommerce.EntitiesV2;
using Ucommerce.Infrastructure.Components.Windsor;
using Ucommerce.Redis.Serializer;

namespace Ucommerce.Redis
{
    public class RedisSessionProvider : SessionProvider
    {
        [Mandatory] public string RedisConnectionString { get; set; }

        protected override ISessionFactory CreateSessionFactory(bool enableCache, string cacheProvider)
        {
            Debugger.Launch();
            if (enableCache)
            {
                RedisCacheProvider.DefaultCacheConfiguration = new RedisCacheConfiguration
                {
                    DefaultUseSlidingExpiration = true,
                    DefaultExpiration = TimeSpan.FromMinutes(5),
                    Serializer = new NhJsonCacheSerializer(),
                    CacheKeyPrefix = "ucommerce:",
                    ConnectionMultiplexerProvider = new ConnectionMultiplexerProvider()
                };
            }

            var sessionFactory = base.CreateSessionFactory(enableCache,
                                                           "NHibernate.Caches.StackExchangeRedis.RedisCacheProvider, NHibernate.Caches.StackExchangeRedis");

            return sessionFactory;
        }
    }
}
