using Funq;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LVD.ServiceStackRoutedCacheClient;
using ServiceStack.Caching;

namespace LVD.ServiceStackRoutedCacheClient.Example
{
   public class AppHost : AppHostBase
   {
      public AppHost() :
         base("Sample Routed Cache Client Integration", typeof(AppHost).Assembly)
      {
         return;
      }

      public override void Configure(Container container)
      {
         //Configure cache client - we're just going to use 
         // another memory cache client instance for session storage
         ICacheClient fallbackCacheClient = new MemoryCacheClient();
         ICacheClient sessionCacheClient = new MemoryCacheClient();

         IRoutedCacheClient routedCacheClient = new DefaultRoutedCacheClient(fallbackCacheClient);

         //TODO: allow listing of registered cache clients 
         //    for an IRoutedCacheClient
         //TODO: use method chaining when registering cache client rules 
         //    to an IRoutedCacheClient
         routedCacheClient.PushServiceStackSessionCacheClient(sessionCacheClient);

         container.Register<ICacheClient>(routedCacheClient);
         container.Register<ICacheClientExtended>(routedCacheClient);

         container.Register<IDictionary<string, ICacheClient>>(new Dictionary<string, ICacheClient>()
         {
            { "fallbackCacheClient", fallbackCacheClient },
            { "sessionCacheClient", sessionCacheClient }
         });

         //Register session feature
         Plugins.Add(new SessionFeature());
      }
   }
}
