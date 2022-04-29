# NHibernate 2nd Level Cache with Redis for Ucommerce

How to use Redis as a NHibernate 2nd Level Cache for Ucommerce.

Reading the documentation for [NHibernate.Caches.StackExchangeRedis](https://github.com/nhibernate/NHibernate-Caches) you need to register a ConnectionMultiplexer. Depending on the CMS, you are running on and the package installation you are using, there is two ways to set this up.

In the following sections we will walk you through how to setup Redis purly with nuget and next section will show you have to do it as a Ucommerce Application.

### Install via Nuget
In order to use the official NHibernate.Caches.StackExchangeRedis cache provider, there’s a little configuration.
#### Add the package reference
Install [NHibernate.Caches.StackExchangeRedis](https://www.nuget.org/packages/NHibernate.Caches.StackExchangeRedis/5.7.0)

```
Install-Package NHibernate.Caches.StackExchangeRedis -Version 5.7.0
```

#### Update the web.config
Replace the default 'syscache' config section registration:

```
<section name="syscache" type="NHibernate.Caches.SysCache.SysCacheSectionHandler, NHibernate.Caches.SysCache, Version=5.5.0.0, Culture=neutral, PublicKeyToken=6876f2ea66c9f443" requirePermission="false" />
```

With:
```
<section name="redis" type="NHibernate.Caches.StackExchangeRedis.RedisSectionHandler, NHibernate.Caches.StackExchangeRedis" />
```

Now replace the syscache section:
```
<syscache>
  <!-- Cache catalog objects for 60 mins before refreshing -->
  <cache region="CatalogFoundation" expiration="3600" priority="5" />
  <cache region="MarketingFoundation" expiration="3600" priority="5" />
  <cache region="SecurityFoundation" expiration="3600" priority="5" />
  <cache region="Backend" expiration="3600" priority="5" />
</syscache>
```

With:
```
<redis configuration="localhost:6379">
  <cache region="CatalogFoundation" expiration="3600" />
  <cache region="MarketingFoundation" expiration="3600" />
  <cache region="SecurityFoundation" expiration="3600" />
  <cache region="Backend" expiration="3600" />
</redis>
```

#### Register the cache provider
Finally, to register the redis cache provider add a new property to a custom .config (or replace the default Configuration/Settings/Settings.config)
```
<properties
  <cacheProvider>NHibernate.Caches.StackExchangeRedis.RedisCacheProvider, NHibernate.Caches.StackExchangeRedis</cacheProvider>
</properties>
```

### Install via Application

Create a folder in Ucommerce\Apps\RedisForUcommerce on the website.
Copy NHibernate.Caches.Redis.dll, RedisForUcommerce.dll, StackExchange.Redis.dll and RedisForUcommerce.config to the folder.
Include pdb files if needed.

Remove the default syscache from web.config:

	<section name="syscache" type="NHibernate.Caches.SysCache.SysCacheSectionHandler, NHibernate.Caches.SysCache2, Version=3.3.1.4000, Culture=neutral, PublicKeyToken=6876f2ea66c9f443" requirePermission="false"/>
	
and
	
	<syscache>
    	<!-- Cache catalog objects for 60 mins before refreshing -->
    	<cache region="CatalogFoundation" expiration="3600" priority="5"/>
    	<cache region="MarketingFoundation" expiration="3600" priority="5"/>
    	<cache region="SecurityFoundation" expiration="3600" priority="5"/>
  	</syscache>
	
Change the web.config commerce section:

	<commerce>
    <runtimeConfiguration enableCache="true" cacheProvider="NHibernate.Caches.SysCache2.SysCacheProvider, NHibernate.Caches.SysCache2, Version=3.3.1.4000, Culture=neutral, PublicKeyToken=6876f2ea66c9f443" connectionString="???"/>

to

	<commerce>
    <runtimeConfiguration enableCache="true" cacheProvider="NHibernate.Caches.Redis.RedisCacheProvider, 
    NHibernate.Caches.Redis" connectionString="???"/>
	
In web.config add assembly redirects:

	<dependentAssembly>
		<assemblyIdentity name="Iesi.Collections" publicKeyToken="aa95f207798dfdb4" culture="neutral"/>
		<bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0"/>
	</dependentAssembly>
	
	<dependentAssembly>
		<assemblyIdentity name="NHibernate" publicKeyToken="aa95f207798dfdb4"  culture="neutral"/>
		<bindingRedirect oldVersion="0.0.0.0-4.0.0.4000" newVersion="4.0.0.4000"/>
	</dependentAssembly>
	
## Test it in a Windows Environment

Download redis for windows https://github.com/MicrosoftArchive/redis/releases
Run the service as windows service (default with MSI installer), or run the redis-server.exe in a cmd.

Navigate the back-office of Ucommerce.

Open a cmd and run redis-cli.exe after run the command -> keys *
NHibernate-Cache:UCommerce.xxx entries should be shown.


