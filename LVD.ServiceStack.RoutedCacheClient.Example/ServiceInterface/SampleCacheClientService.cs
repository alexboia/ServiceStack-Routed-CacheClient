using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack;
using LVD.ServiceStackRoutedCacheClient.Example.ServiceModel;
using ServiceStack.Caching;

namespace LVD.ServiceStackRoutedCacheClient.Example.ServiceInterface
{
   public class SampleCacheClientService : Service
   {
      public object Post(CacheSetRandomValue request)
      {
         string baseKey = nameof(SampleCacheClientService);

         string stringKey = baseKey + ".randomText";
         Cache.Set<string>(stringKey, Guid.NewGuid().ToString());

         int minuteCounter;
         string counterKey = baseKey + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ".hitCountsPerMinute";

         int.TryParse(Cache.Get<string>(counterKey) ?? "0", out minuteCounter);
         Cache.Set<string>(counterKey, (minuteCounter + 1).ToString());

         return true;
      }

      public object Post(SessionSetRandomValue request)
      {
         string baseKey = nameof(SampleCacheClientService);

         string stringKey = baseKey + ".sessionRandomText";
         SessionBag[stringKey] = Guid.NewGuid().ToString();

         int sessionCounter;
         string counterKey = baseKey + ".hitCountsPerSession";

         int.TryParse(SessionBag[counterKey]?.ToString() ?? "0", out sessionCounter);
         SessionBag[counterKey] = (sessionCounter + 1).ToString();

         return true;
      }

      public object Get(ListAllCacheClientRules request)
      {
         ListAllCacheClientRulesResponse response = new ListAllCacheClientRulesResponse();

         foreach (KeyValuePair<string, ICacheClient> clientInfo in CacheClientRegistry)
         {
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (string key in clientInfo.Value.GetAllKeys())
               values.Add(key, clientInfo.Value.Get<string>(key));

            response.CacheProvidersData.Add(clientInfo.Key, values);
         }

         return response;
      }

      public IDictionary<string, ICacheClient> CacheClientRegistry { get; set; }
   }
}
