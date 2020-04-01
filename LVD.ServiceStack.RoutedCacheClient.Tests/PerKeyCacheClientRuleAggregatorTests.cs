//
// Copyright (c) 2016-2017 Live Design SRL
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using ServiceStack.Caching;

namespace LiveLMS.RoutedCacheClient.Tests
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
