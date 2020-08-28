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

namespace LVD.ServiceStackRoutedCacheClient
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

			try
			{
				//A cache client may be registered multiple 
				// times with different rules;
				// so we need to keep a book on what we've already flushed
				foreach ( IRoutedCacheClientRule rule in mRules )
				{
					if ( !visited.Contains( rule.Client ) )
					{
						rule.Client.FlushAll();
						visited.Add( rule.Client );
					}
				}
			}
			finally
			{
				visited.Clear();
			}
		}

		public T Get<T> ( string key )
		{
			return FindClient( key )
			   .Get<T>( key );
		}

		public IDictionary<string, T> GetAll<T> ( IEnumerable<string> keys )
		{
			if ( keys == null )
				throw new ArgumentNullException( nameof( keys ) );

			IDictionary<string, T> values =
			   new Dictionary<string, T>();

			PerKeyCacheClientRuleAggregator aggregator
			   = new PerKeyCacheClientRuleAggregator();

			try
			{
				//The naive approach would be to find the cache client for each key
				// then issue a Get() call for that key and value,
				// but that won't use any optimisation a cache client might have for bulk operations.
				//Issuing a GetAll() for each client with the given set of values, doesn't work either,
				// since that would bypass any routing.
				//So, we need to see first which key by which client can be resolved,
				// and then issue a GetAll() on each client using only the subset of keys
				// it can resolve

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
			}
			finally
			{
				aggregator.Clear();
			}

			return values;
		}

		public IEnumerable<string> GetKeysByPattern ( string pattern )
		{
			List<string> keys = new List<string>();
			List<ICacheClient> visited = new List<ICacheClient>();

			//A cache client may be registered multiple 
			// times with different rules;
			// so we need to keep a book on what we've already checked
			//Also, since we are allowing a user to register a regular ICacheClient,
			// we also need to filter out cache clients that are not ICacheClientExtended
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

		public bool Remove ( string key )
		{
			return FindClient( key )
			   .Remove( key );
		}

		public void RemoveAll ( IEnumerable<string> keys )
		{
			if ( keys == null )
				throw new ArgumentNullException( nameof( keys ) );

			PerKeyCacheClientRuleAggregator aggregator
			   = new PerKeyCacheClientRuleAggregator();

			try
			{
				//The naive approach would be to find the cache client for each key
				// then issue a Remove() call for that key and value,
				// but that won't use any optimisation a cache client might have for bulk operations.
				//Issuing a RemoveAll() for each client with the given set of values, doesn't work either,
				// since that would bypass any routing.
				//So, we need to see first which key by which client can be resolved,
				// and then issue a RemoveAll() on each client using only the subset of keys
				// it can resolve

				aggregator.CollectAll( keys, FindRule );

				foreach ( KeyValuePair<Guid, IList<string>> keysForClient in aggregator.KeysForCacheClient )
				{
					ICacheClient client = aggregator
					   .CacheClients[ keysForClient.Key ];

					client.RemoveAll( keysForClient.Value );
				}
			}
			finally
			{
				aggregator.Clear();
			}
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
			if ( values == null )
				throw new ArgumentNullException( nameof( values ) );

			PerKeyCacheClientRuleAggregator aggregator
			   = new PerKeyCacheClientRuleAggregator();

			try
			{
				//The naive approach would be to find the cache client for each key
				// then issue a Set() call for that key and value,
				// but that won't use any optimisation a cache client might have for bulk operations.
				//Issuing a SetAll() for each client with the given set of values, doesn't work either,
				// since that would bypass any routing.
				//So, we need to see first which key by which client can be resolved,
				// and then issue a SetAll() on each client using only the subset of keys
				// it can resolve

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
			}
			finally
			{
				aggregator.Clear();
			}
		}

		public IRoutedCacheClient PushClientWithRule ( IRoutedCacheClientRule rule )
		{
			if ( rule == null )
				throw new ArgumentNullException( nameof( rule ) );

			mRules.Push( rule );
			return this;
		}

		public IRoutedCacheClient ClearRules ()
		{
			while ( mRules.Count > 1 )
				mRules.Pop();

			return this;
		}

		public IDictionary<string, ICacheClient> GetRegisteredClients ()
		{
			Dictionary<string, int> discriminatorMap =
			   new Dictionary<string, int>();

			Dictionary<string, ICacheClient> clientsSnapshot =
			   new Dictionary<string, ICacheClient>();

			foreach ( IRoutedCacheClientRule rule in mRules )
			{
				string snapshotKey = rule.Name;

				if ( clientsSnapshot.ContainsKey( snapshotKey ) )
				{
					int discriminator;

					if ( !discriminatorMap.TryGetValue( snapshotKey, out discriminator ) )
						discriminator = 1;
					else
						discriminator++;

					discriminatorMap[ snapshotKey ] = discriminator;

					snapshotKey = string.Format( "{0}_{1}",
					   snapshotKey,
					   discriminator );
				}

				clientsSnapshot[ snapshotKey ] = rule.Client;
			}

			return clientsSnapshot;
		}

		public IEnumerable<IRoutedCacheClientRule> GetRegisteredClientRules ()
		{
			List<IRoutedCacheClientRule> rulesSnapshot = new List<IRoutedCacheClientRule>();

			foreach ( IRoutedCacheClientRule rule in mRules )
				rulesSnapshot.Add( rule );

			return rulesSnapshot;
		}

		private IRoutedCacheClientRule FindRule ( string key )
		{
			return mRules.FirstOrDefault( r => r.Matches( key ) );
		}

		private ICacheClient FindClient ( string key )
		{
			return FindRule( key )?.Client;
		}

		protected void Dispose ( bool disposing )
		{
			if ( disposing )
			{
				while ( mRules.Count > 0 )
				{
					IRoutedCacheClientRule rule = mRules.Pop();
					rule.Client.Dispose();
				}
			}
		}

		public void Dispose ()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		public int RulesCount => mRules.Count;
	}
}
