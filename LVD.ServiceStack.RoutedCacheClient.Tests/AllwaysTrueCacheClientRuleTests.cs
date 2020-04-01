using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveLMS.RoutedCacheClient;
using NUnit.Framework;
using Moq;
using ServiceStack.Caching;

namespace LiveLMS.RoutedCacheClient.Tests
{
   [TestFixture]
   public class AllwaysTrueCacheClientRuleTests
   {
      [Test]
      [TestCase( "urn:iauthsession:sessionId" )]
      [TestCase( "sess:sessionId:key" )]
      [TestCase( "randomKey" )]
      [TestCase( "53849296-dfda-49bb-94c1-742e90703d4b" )]
      [TestCase( "0e925156-34d0-487a-8ec7-9f229fa6e269" )]
      [TestCase( "f6007030-97a4-482d-bdcc-ec80fa163f06" )]
      [TestCase( "5a78a826-7f87-4f43-8987-6185fff83f5f" )]
      [TestCase( "92c3e650-76eb-4957-a95f-f114779abb3d" )]
      [TestCase( "" )]
      [TestCase( null )]
      public void Test_AllwaysReturnsTrue ( string testKey )
      {
         Mock<ICacheClient> cacheClientMocker =
            new Mock<ICacheClient>( MockBehavior.Loose );

         ICacheClient cacheClient = cacheClientMocker.Object;

         AllwaysTrueCacheClientRule rule = 
            new AllwaysTrueCacheClientRule( cacheClient );

         Assert.IsTrue( rule.Matches( testKey ) );
         Assert.AreSame( cacheClient, rule.Client );
      }
   }
}
