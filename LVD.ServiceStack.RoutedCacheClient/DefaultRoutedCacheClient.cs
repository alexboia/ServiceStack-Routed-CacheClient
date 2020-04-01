//
// Copyright (c) 2016-2017 Live Design SRL
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Caching;

namespace LiveLMS.RoutedCacheClient
{
   public class DefaultRoutedCacheClient : IRoutedCacheClient
   {
      private Stack<IRoutedCacheClientRule> mRules =
         new Stack<IRoutedCacheClientRule>();

      public DefaultRoutedCacheClient ( ICacheClient fallbackClient )
      {
         if ( fallbackClient == null )
            throw new ArgumentNullException( nameof( fallbackClient ) );

         mRules.Push( new AllwaysTrueCacheClientRule( fallbackClient ) );
      }

      private IRoutedCacheClientRule FindRule ( string key )
      {
         return mRules.FirstOrDefault( r => r.Matches( key ) );
      }

      private ICacheClient FindClient ( string key )
      {
         return FindRule( key ).Client;
      }

      public bool Add<T> ( string key, T value )
      {
         return FindClient( key )
            .Add<T>( key, value );
      }

      public bool Add<T> ( string key, T value, TimeSpan expiresIn )
      {
         return FindClient( key )
            .Add<T>( key, value, expiresIn );
      }

      public bool Add<T> ( string key, T value, DateTime expiresAt )
      {
         return FindClient( key )
            .Add<T>( key, value, expiresAt );
      }

      public long Decrement ( string key, uint amount )
      {
         return FindClient( key )
            .Decrement( key, amount );
      }

      public void FlushAll ()
      {
         List<ICacheClient> visited = new List<ICacheClient>();

         foreach ( IRoutedCacheClientRule rule in mRules )
         {
            if ( !visited.Contains( rule.Client ) )
            {
               rule.Client.FlushAll();
               visited.Add( rule.Client );
            }
         }

         visited.Clear();
      }

      public T Get<T> ( string key )
      {
         return FindClient( key )
            .Get<T>( key );
      }

      public IDictionary<string, T> GetAll<T> ( IEnumerable<string> keys )
      {
         IDictionary<string, T> values =
            new Dictionary<string, T>();

         PerKeyCacheClientRuleAggregator aggregator
            = new PerKeyCacheClientRuleAggregator();

         aggregator.CollectAll( keys, FindRule );

         foreach ( KeyValuePair<Guid, IList<string>> keysForClient in aggregator.KeysForCacheClient )
         {
            ICacheClient client = aggregator
               .CacheClients[ keysForClient.Key ];

            IDictionary<string, T> clientValues =
               client.GetAll<T>( keysForClient.Value );

            if ( clientValues != null )
               foreach ( KeyValuePair<string, T> valuePair in clientValues )
                  values.Add( valuePair );
         }

         aggregator.Clear();
         return values;
      }

      public IEnumerable<string> GetKeysByPattern ( string pattern )
      {
         List<string> keys = new List<string>();
         List<ICacheClient> visited = new List<ICacheClient>();

         foreach ( IRoutedCacheClientRule rule in mRules )
         {
            if ( rule.Client is ICacheClientExtended && !visited.Contains( rule.Client ) )
            {
               IEnumerable<string> clientKeys = ( ( ICacheClientExtended )rule.Client ).GetKeysByPattern( pattern )
                  ?? new List<string>();

               keys.AddRange( clientKeys );
               visited.Add( rule.Client );
            }
         }

         visited.Clear();
         return keys;
      }

      public TimeSpan? GetTimeToLive ( string key )
      {
         ICacheClient client = FindClient( key );
         if ( client is ICacheClientExtended )
            return ( ( ICacheClientExtended )client ).GetTimeToLive( key );
         else
            return null;
      }

      public long Increment ( string key, uint amount )
      {
         return FindClient( key )
            .Increment( key, amount );
      }

      public void RegisterRoutedClient ( IRoutedCacheClientRule rule )
      {
         if ( rule == null )
            throw new ArgumentNullException( nameof( rule ) );
         mRules.Push( rule );
      }

      public bool Remove ( string key )
      {
         return FindClient( key )
            .Remove( key );
      }

      public void RemoveAll ( IEnumerable<string> keys )
      {
         PerKeyCacheClientRuleAggregator aggregator
            = new PerKeyCacheClientRuleAggregator();

         aggregator.CollectAll( keys, FindRule );

         foreach ( KeyValuePair<Guid, IList<string>> keysForClient in aggregator.KeysForCacheClient )
         {
            ICacheClient client = aggregator
               .CacheClients[ keysForClient.Key ];

            client.RemoveAll( keysForClient.Value );
         }

         aggregator.Clear();
      }

      public bool Replace<T> ( string key, T value )
      {
         return FindClient( key )
            .Replace<T>( key, value );
      }

      public bool Replace<T> ( string key, T value, TimeSpan expiresIn )
      {
         return FindClient( key )
            .Replace<T>( key, value, expiresIn );
      }

      public bool Replace<T> ( string key, T value, DateTime expiresAt )
      {
         return FindClient( key )
            .Replace<T>( key, value, expiresAt );
      }

      public bool Set<T> ( string key, T value )
      {
         return FindClient( key )
            .Set<T>( key, value );
      }

      public bool Set<T> ( string key, T value, TimeSpan expiresIn )
      {
         return FindClient( key )
            .Set<T>( key, value, expiresIn );
      }

      public bool Set<T> ( string key, T value, DateTime expiresAt )
      {
         return FindClient( key )
            .Set<T>( key, value, expiresAt );
      }

      public void SetAll<T> ( IDictionary<string, T> values )
      {
         PerKeyCacheClientRuleAggregator aggregator
            = new PerKeyCacheClientRuleAggregator();

         aggregator.CollectAll( values.Keys, FindRule );

         foreach ( KeyValuePair<Guid, IList<string>> keysForClient in aggregator.KeysForCacheClient )
         {
            ICacheClient client =
               aggregator.CacheClients[ keysForClient.Key ];

            IDictionary<string, T> clientValues = values
               .Where( v => keysForClient.Value.Contains( v.Key ) )
               .ToDictionary( v => v.Key, v => v.Value );

            client.SetAll( clientValues );
         }

         aggregator.Clear();
      }

      private void DeRegisterAllClients ()
      {
         mRules.Clear();
      }

      public void Dispose ()
      {
         DeRegisterAllClients();
      }
   }
}
