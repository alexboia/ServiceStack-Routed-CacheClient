using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack;

namespace LVD.ServiceStackRoutedCacheClient.Example.ServiceModel
{
   [Route("/list-cache-providers-data", "GET")]
   public class ListAllCacheClientRules : IReturn<ListAllCacheClientRulesResponse>
   {
      //
   }

   public class ListAllCacheClientRulesResponse
   {
      public ListAllCacheClientRulesResponse()
      {
         CacheProvidersData = new Dictionary<string, Dictionary<string, string>>();
      }

      public Dictionary<string, Dictionary<string, string>> CacheProvidersData { get; set; }
   }
}
