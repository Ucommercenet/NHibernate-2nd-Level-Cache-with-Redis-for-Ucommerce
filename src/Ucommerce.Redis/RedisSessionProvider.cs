using System;
using System.Diagnostics;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Caches.StackExchangeRedis;
using Ucommerce.EntitiesV2;
using Ucommerce.Extensions;
using Ucommerce.Infrastructure.Components.Windsor;
using Ucommerce.Redis.Serializer;
using Environment = NHibernate.Cfg.Environment;

namespace Ucommerce.Redis
{
    public class RedisSessionProvider : SessionProvider
    {
        [Mandatory] public string RedisConnectionString { get; set; }

        protected override ISessionFactory CreateSessionFactory(bool enableCache, string cacheProvider)
        {
            if (enableCache)
            {
                RedisCacheProvider.DefaultCacheConfiguration = new RedisCacheConfiguration
                {
                    DefaultUseSlidingExpiration = true,
                    DefaultExpiration = TimeSpan.FromMinutes(5),
                    Serializer = new NhJsonCacheSerializer(),
                    CacheKeyPrefix = "ucommerce:"
                };
            }

            var msSqlConfiguration = MsSqlConfiguration.MsSql2008;
            if (string.IsNullOrEmpty(ConnectionIsolation) == false)
            {
                msSqlConfiguration = msSqlConfiguration.Raw("connection.isolation", ConnectionIsolation);
            }

            msSqlConfiguration
                .ConnectionString(ConnectionStringLocator.LocateConnectionString())
                .UseReflectionOptimizer() // Will not work in medium trust environments
                .AdoNetBatchSize(200);

            var factory = Fluently.Configure()
                                  .Database(msSqlConfiguration)
                                  .Cache(c =>
                                         {
                                             if (enableCache)
                                                 c
                                                     .UseQueryCache()
                                                     .UseSecondLevelCache()
                                                     .ProviderClass("NHibernate.Caches.StackExchangeRedis.RedisCacheProvider, NHibernate.Caches.StackExchangeRedis");
                                         })
                                  .Mappings(m =>
                                            {
                                                m.FluentMappings
                                                 .AddFromTaggedAssemblies(MapAssemblyTags
                                                                              .Reverse()) // Reverse the list to add user defined maps first.
                                                 .Conventions.Add(Table.Is(x => "Ucommerce_" + x.EntityType.Name))
                                                 .Conventions.Add(PrimaryKey.Name.Is(FormatPrimaryKey))
                                                 .Conventions.Add(ForeignKey.Format(FormatForeignKey))
                                                 .Conventions.AddFromAssemblyOf<IdPropertyConvention>();
                                                m.HbmMappings.AddFromAssemblyOf<SessionProvider>();
                                            }
                                           )
                                  .ExposeConfiguration(BuildSchema)
                                  .ExposeConfiguration(c =>
                                                       {
                                                           // http://ronaldrosiernet.azurewebsites.net/Blog/2013/04/20/timeout_in_nhibernate_batched_sessions

                                                           // This will set the command_timeout property on factory-level
                                                           c.SetProperty(Environment.CommandTimeout, "6000");
                                                           // This will set the command_timeout property on system-level
                                                           Environment.Properties.Add(Environment.CommandTimeout,
                                                               "6000");
                                                       })
                                  .ExposeConfiguration(cfg =>
                                                       {
                                                           cfg.Properties.Add("cache.default_expiration", "900");
                                                           cfg.Properties.Add("cache.use_sliding_expiration", "true");
                                                           cfg.Properties.Add("cache.configuration",
                                                                              RedisConnectionString);
                                                       })
                                  .BuildSessionFactory();

            return factory;
        }
    }
}
