//
// Copyright (c) 2016-2017 Live Design SRL
// All rights reserved.
//
using ServiceStack.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveLMS.RoutedCacheClient
{
   public class PerKeyCacheClientRuleAggregator
   {
      private IDictionary<Guid, IList<string>> mKeysForCacheClient =
         new Dictionary<Guid, IList<string>>();

      private IDictionary<Guid, ICacheClient> mCacheClients =
         new Dictionary<Guid, ICacheClient>();

      public void CollectAll ( IEnumerable<string> keys, Func<string, IRoutedCacheClientRule> ruleSelector )
      {
         foreach ( string key in keys )
         {
            IRoutedCacheClientRule rule = ruleSelector.Invoke( key );
            Collect( key, rule );
         }
      }

      public void Collect ( string key, IRoutedCacheClientRule rule )
      {
         IList<string> keysForClient;

         if ( !mKeysForCacheClient.TryGetValue( rule.Id, out keysForClient ) )
         {
            keysForClient = new List<string>();
            mKeysForCacheClient[ rule.Id ] = keysForClient;
            mCacheClients[ rule.Id ] = rule.Client;
         }

         keysForClient.Add( key );
      }

      public void Clear ()
      {
         mKeysForCacheClient.Clear();
         mCacheClients.Clear();
      }

      public IDictionary<Guid, IList<string>> KeysForCacheClient
         => mKeysForCacheClient;

      public IDictionary<Guid, ICacheClient> CacheClients
         => mCacheClients;
   }
}
