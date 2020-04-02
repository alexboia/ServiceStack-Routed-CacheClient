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
      public void Test_CanCollect ( int numKeys, int numClients )
      {
         PerKeyCacheClientRuleAggregator aggregator =
            new PerKeyCacheClientRuleAggregator();

         Dictionary<Guid, IList<string>> expectedkeysForCacheClients =
            new Dictionary<Guid, IList<string>>();

         List<IRoutedCacheClientRule> rules =
            new List<IRoutedCacheClientRule>();

         List<string> keys =
            new List<string>();

         for ( int i = 0; i < numClients; i++ )
         {
            Mock<ICacheClient> cacheClientMocker =
               new Mock<ICacheClient>( MockBehavior.Loose );

            IRoutedCacheClientRule rule =
               new AllwaysTrueCacheClientRule( cacheClientMocker.Object );

            expectedkeysForCacheClients[ rule.Id ] = new List<string>();
            rules.Add( rule );
         }

         for ( int i = 0; i < numKeys; i++ )
            keys.Add( Guid.NewGuid().ToString() );

         for ( int i = 0; i < numKeys; i++ )
         {
            string key = keys[ i ];
            IRoutedCacheClientRule rule = rules[ i % numClients ];

            expectedkeysForCacheClients[ rule.Id ].Add( key );
            aggregator.Collect( key, rule );
         }

         Assert.NotNull( aggregator.CacheClients );
         Assert.NotNull( aggregator.KeysForCacheClient );

         Assert.AreEqual( Math.Min( numKeys, expectedkeysForCacheClients.Count ),
            aggregator.KeysForCacheClient.Count );

         foreach ( KeyValuePair<Guid, IList<string>> actualKeysForCacheClientPair in aggregator.KeysForCacheClient )
         {
            Assert.IsTrue( expectedkeysForCacheClients
               .ContainsKey( actualKeysForCacheClientPair.Key ) );

            CollectionAssert.AreEqual( expectedkeysForCacheClients[ actualKeysForCacheClientPair.Key ],
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
   }
}
