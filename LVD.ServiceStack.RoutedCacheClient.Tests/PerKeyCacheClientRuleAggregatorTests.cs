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
using Moq;
using NUnit.Framework;
using ServiceStack.Caching;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LVD.ServiceStackRoutedCacheClient.Tests
{
	[TestFixture]
	public class PerKeyCacheClientRuleAggregatorTests
	{
		[Test]
		[TestCase( 10, 1 )]
		[TestCase( 13, 2 )]
		[TestCase( 23, 5 )]
		[TestCase( 3, 5 )]
		public void Test_CanCollect_Single ( int numKeys, int numClients )
		{
			PerKeyCacheClientRuleAggregator aggregator =
			   new PerKeyCacheClientRuleAggregator();

			Dictionary<Guid, IList<string>> keysForCacheClients =
			   new Dictionary<Guid, IList<string>>();

			//Init rules
			List<IRoutedCacheClientRule> cacheClientRules =
				CreateTestRules( numClients );

			foreach ( IRoutedCacheClientRule r in cacheClientRules )
				keysForCacheClients[ r.Id ] = new List<string>();

			//Generate some keys and distribute 
			// them per cache client rules
			List<string> testCacheKeys = GenerateTestKeys( numKeys,
				cacheClientRules,
				keysForCacheClients );

			foreach ( KeyValuePair<Guid, IList<string>> keysForRules in keysForCacheClients )
			{
				IRoutedCacheClientRule rule = cacheClientRules.FirstOrDefault( r => r.Id == keysForRules.Key );
				foreach ( string key in keysForRules.Value )
					aggregator.Collect( key, rule );
			}

			Assert_KeysCollected( aggregator, 
				keysForCacheClients, 
				cacheClientRules, 
				numKeys );
		}	

		[Test]
		[TestCase( 10, 1 )]
		[TestCase( 13, 2 )]
		[TestCase( 23, 5 )]
		[TestCase( 3, 5 )]
		public void Test_CanCollect_All ( int numKeys, int numClients )
		{
			PerKeyCacheClientRuleAggregator aggregator =
			   new PerKeyCacheClientRuleAggregator();

			Dictionary<Guid, IList<string>> keysForCacheClients =
			   new Dictionary<Guid, IList<string>>();

			//Init rules
			List<IRoutedCacheClientRule> cacheClientRules =
				CreateTestRules( numClients );

			foreach ( IRoutedCacheClientRule r in cacheClientRules )
				keysForCacheClients[ r.Id ] = new List<string>();

			//Generate some keys and distribute 
			// them per cache client rules
			List<string> testCacheKeys = GenerateTestKeys( numKeys,
				cacheClientRules,
				keysForCacheClients );

			aggregator.CollectAll( testCacheKeys, key => 
			{
				KeyValuePair<Guid, IList<string>> ruleWithKey = keysForCacheClients
					.FirstOrDefault( p => p.Value.Contains( key ) );

				return cacheClientRules
					.FirstOrDefault( r => r.Id == ruleWithKey.Key );
			} );

			Assert_KeysCollected( aggregator,
				keysForCacheClients,
				cacheClientRules,
				numKeys );
		}

		private static void Assert_KeysCollected ( PerKeyCacheClientRuleAggregator aggregator,
			Dictionary<Guid, IList<string>> expectedkeysForCacheClients,
			List<IRoutedCacheClientRule> rules,
			int numKeys )
		{
			Assert.NotNull( aggregator.CacheClients );
			Assert.NotNull( aggregator.KeysForCacheClient );

			Assert.AreEqual( Math.Min( numKeys, expectedkeysForCacheClients.Count ),
			   aggregator.KeysForCacheClient.Count );

			foreach ( KeyValuePair<Guid, IList<string>> actualKeysForCacheClientPair in aggregator.KeysForCacheClient )
			{
				Assert.IsTrue( expectedkeysForCacheClients
				   .ContainsKey( actualKeysForCacheClientPair.Key ) );

				CollectionAssert.AreEquivalent( expectedkeysForCacheClients[ actualKeysForCacheClientPair.Key ],
				   actualKeysForCacheClientPair.Value );
			}

			Assert.AreEqual( Math.Min( rules.Count, numKeys ),
			   aggregator.CacheClients.Count );

			foreach ( KeyValuePair<Guid, ICacheClient> actualCacheClientPair in aggregator.CacheClients )
			{
				Assert.IsTrue( rules.Any( r => r.Id == actualCacheClientPair.Key ) );

				Assert.AreSame( rules.FirstOrDefault( r => r.Id == actualCacheClientPair.Key ).Client,
				   actualCacheClientPair.Value );
			}
		}

		private List<IRoutedCacheClientRule> CreateTestRules ( int numClients )
		{
			List<IRoutedCacheClientRule> rules =
			   new List<IRoutedCacheClientRule>();

			for ( int i = 0; i < numClients; i++ )
			{
				Mock<ICacheClient> cacheClientMocker =
				   new Mock<ICacheClient>( MockBehavior.Loose );

				IRoutedCacheClientRule rule =
				   new AlwaysTrueCacheClientRule( cacheClientMocker.Object );
				rules.Add( rule );
			}

			return rules;
		}

		private List<string> GenerateTestKeys ( int numKeys,
			List<IRoutedCacheClientRule> forRules,
			Dictionary<Guid, IList<string>> keysForClients )
		{
			List<string> testCacheKeys =
				new List<string>();

			for ( int i = 0; i < numKeys; i++ )
			{
				string key = Guid.NewGuid().ToString();
				testCacheKeys.Add( key );

				IRoutedCacheClientRule rule = forRules[ i % forRules.Count ];
				keysForClients[ rule.Id ].Add( key );
			}

			return testCacheKeys;
		}
	}
}
