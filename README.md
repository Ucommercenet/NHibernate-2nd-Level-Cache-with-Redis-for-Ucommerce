# NHibernate 2nd Level Cache with Redis for Ucommerce

How to use Redis as a NHibernate 2nd Level Cache for Ucommerce.

Reading the documentation for NHibernate.Caches.Redis https://github.com/TheCloudlessSky/NHibernate.Caches.Redis you need to do initialize redis connections. Ucommerce has a ISessionProvider that can be extended for that purpose.
Look at the class RedisCacheSessionProvider.cs file.

## Install

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

Navigate the back office of Ucommerce.

Open a cmd and run redis-cli.exe after run the command -> keys *
NHibernate-Cache:UCommerce.xxx entries should be shown.


