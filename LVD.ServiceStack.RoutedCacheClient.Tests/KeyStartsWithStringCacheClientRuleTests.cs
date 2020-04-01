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
   public class KeyStartsWithStringCacheClientRuleTests
   {
      [Test]
      [TestCase( "urn:iauthsession:", "urn:iauthsession:04562f63-ea5a-4859-bc6d-28771bda2f31",
         StringComparison.InvariantCultureIgnoreCase,
         true )]

      [TestCase( "URN:IAUTHSESSION:", "urn:iauthsession:04562f63-ea5a-4859-bc6d-28771bda2f31",
         StringComparison.InvariantCultureIgnoreCase,
         true )]

      [TestCase( "URN:IAUTHSESSION:", "urn:iauthsession:04562f63-ea5a-4859-bc6d-28771bda2f31",
         StringComparison.InvariantCulture,
         false )]

      [TestCase( "urn:iauthsessionz:", "urn:iauthsession:04562f63-ea5a-4859-bc6d-28771bda2f31",
         StringComparison.InvariantCultureIgnoreCase,
         false )]

      [TestCase( "urn:iauthsessionz:", "urn:iauthsession:04562f63-ea5a-4859-bc6d-28771bda2f31",
         StringComparison.InvariantCulture,
         false )]

      [TestCase( "sess:", "sess:f405d591-2aa7-4b33-a6cb-6f5a03fc91ec:testKey",
         StringComparison.InvariantCultureIgnoreCase,
         true )]

      [TestCase( "SESS:", "sess:f405d591-2aa7-4b33-a6cb-6f5a03fc91ec:testKey",
         StringComparison.InvariantCultureIgnoreCase,
         true )]

      [TestCase( "SESS:", "sess:f405d591-2aa7-4b33-a6cb-6f5a03fc91ec:testKey",
         StringComparison.InvariantCulture,
         false )]

      [TestCase( "sessz:", "sess:f405d591-2aa7-4b33-a6cb-6f5a03fc91ec:testKey",
         StringComparison.InvariantCultureIgnoreCase,
         false )]

      [TestCase( "sessz:", "sess:f405d591-2aa7-4b33-a6cb-6f5a03fc91ec:testKey",
         StringComparison.InvariantCulture,
         false )]
      public void Test_CanMatch ( string token, string key, StringComparison comparisonMode, bool expectedResult )
      {
         Mock<ICacheClient> cacheClientMocker =
            new Mock<ICacheClient>( MockBehavior.Loose );

         ICacheClient cacheClient = cacheClientMocker.Object;

         KeyStartsWithStringCacheClientRule rule =
            new KeyStartsWithStringCacheClientRule( cacheClient, comparisonMode, token );

         Assert.AreEqual( expectedResult, rule.Matches( key ) );
         Assert.AreSame( cacheClient, rule.Client );
      }

      [Test]
      [TestCase( "sess:", StringComparison.InvariantCulture, true )]
      [TestCase( "urn:iauthsession:", StringComparison.InvariantCulture, true )]
      public void Test_CanMatch_MultipleTokes ( string testKey, StringComparison comparisonMode, bool expectedMatch )
      {
         Mock<ICacheClient> cacheClientMocker =
            new Mock<ICacheClient>( MockBehavior.Loose );

         ICacheClient cacheClient = cacheClientMocker.Object;

         KeyStartsWithStringCacheClientRule rule = new KeyStartsWithStringCacheClientRule( cacheClient,
            comparisonMode,
            "urn:iauthsession:",
            "sess:" );

         Assert.AreEqual( expectedMatch, rule.Matches( testKey ) );
      }
   }
}
