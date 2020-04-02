using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Caching;

namespace LVD.ServiceStackRoutedCacheClient
{
   public static class RoutedCacheClientExtensions
   {
      public static IRoutedCacheClient PushServiceStackSessionCacheClient(this IRoutedCacheClient routedClient, ICacheClient cacheClient)
      {
         if (routedClient == null)
            throw new ArgumentNullException(nameof(routedClient));

         routedClient.PushClientWithRule(new ServiceStackSessionKeyCacheClientRule(cacheClient));
         return routedClient;
      }
   }
}
