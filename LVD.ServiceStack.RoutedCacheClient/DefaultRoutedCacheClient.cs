// 
// BSD 3-Clause License
// 
// Copyright (c) 2020, Boia Alexandru
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
// 
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
