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

An Application in Ucommerce is a set of dynamic loaded dll's and a configuration file, Applications are stored in the path `~\Ucommerce\Apps\*`. If you are running a completely new Ucommerce there will be installed some default Applications, Redis is not one of them.

To use Ucommerce.Redis as an Application you will have to clone this repository and build/pack it yourself. We have tried to make it easy, so everything should be setup and ready to build. What you need for that tasks:

1. Clone this repository to your PC
2. Install [.NET framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) 
3. Install [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
4. (Optional) Install [Nuke](https://nuke.build/) as en extension to your IDE

After all the installation there is three ways to build and pack this extension to Ucommerce.

1. Use the build scripts in the root of this repository. (build.cmd, build.ps1 and build.sh)
2. Use the Nuke extension in your IDE, open the solution and browse to `build.cs` in the _build.csproj. Here you could run the package command
3. This way is a little harder, you can just build the project and gather the dll's an create the config file yourself.

When you have your Redis Application ready (you can find it in `~\.nuke\temp\dist`) copy it to the Application folder in Ucommerce `~\Ucommerce\Apps\*`. If you postfix it with `.disabled` the application will be disabled. Hint this can be used to fallback to the syscache in development and then have a deploy script to remove the `.disabled` in production.

	
## Test it in a Windows Environment

Download redis for windows https://github.com/MicrosoftArchive/redis/releases
Run the service as windows service (default with MSI installer), or run the redis-server.exe in a cmd.

Navigate the back-office of Ucommerce.

Open a cmd and run redis-cli.exe after run the command -> keys *
NHibernate-Cache:UCommerce.xxx entries should be shown.


