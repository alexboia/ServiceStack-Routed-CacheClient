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
   public class RoutedCacheClientTests
   {
      [Test]
      [TestCase( 1, "test" )]
      public void Test_CanRoute_SingleAddCalls ( int sessionClientVal, string fallbackClientVal )
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         DateTime dateTimeRef = DateTime.Now;
         TimeSpan timeSpanRef = TimeSpan.FromHours( 1 );

         sessionClientMocker.Setup( c => c.Add<int>( sessionKey, sessionClientVal ) )
            .Returns( true );

         sessionClientMocker.Setup( c => c.Add<int>( sessionKey, sessionClientVal, dateTimeRef ) )
            .Returns( true );

         sessionClientMocker.Setup( c => c.Add<int>( sessionKey, sessionClientVal, timeSpanRef ) )
            .Returns( true );

         fallbackClientMocker.Setup( c => c.Add<string>( randomKey, fallbackClientVal ) )
            .Returns( true );

         fallbackClientMocker.Setup( c => c.Add<string>( randomKey, fallbackClientVal, dateTimeRef ) )
            .Returns( true );

         fallbackClientMocker.Setup( c => c.Add<string>( randomKey, fallbackClientVal, timeSpanRef ) )
            .Returns( true );

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient( fallbackClient, sessionClient );

         routedCacheClient.Add<int>( sessionKey, sessionClientVal );
         routedCacheClient.Add<int>( sessionKey, sessionClientVal, dateTimeRef );
         routedCacheClient.Add<int>( sessionKey, sessionClientVal, timeSpanRef );

         routedCacheClient.Add<string>( randomKey, fallbackClientVal );
         routedCacheClient.Add<string>( randomKey, fallbackClientVal, timeSpanRef );
         routedCacheClient.Add<string>( randomKey, fallbackClientVal, dateTimeRef );

         sessionClientMocker.VerifyAll();
         fallbackClientMocker.VerifyAll();
      }

      [Test]
      [TestCase( 10, "bogus" )]
      public void Test_CanRoute_SingleGetCalls ( int sessionClientVal, string fallbackClientVal )
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         sessionClientMocker.Setup( c => c.Get<int>( sessionKey ) )
            .Returns( sessionClientVal );

         fallbackClientMocker.Setup( c => c.Get<string>( randomKey ) )
            .Returns( fallbackClientVal );

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient( fallbackClient, sessionClient );

         Assert.AreEqual( sessionClientVal, routedCacheClient.Get<int>( sessionKey ) );
         Assert.AreEqual( fallbackClientVal, routedCacheClient.Get<string>( randomKey ) );

         sessionClientMocker.VerifyAll();
         fallbackClientMocker.VerifyAll();
      }

      [Test]
      public void Test_CanRoute_SingleRemoveCalls ()
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         sessionClientMocker.Setup( c => c.Remove( sessionKey ) )
            .Returns( true );

         fallbackClientMocker.Setup( c => c.Remove( randomKey ) )
            .Returns( true );

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient( fallbackClient, sessionClient );

         Assert.IsTrue( routedCacheClient.Remove( sessionKey ) );
         Assert.IsTrue( fallbackClient.Remove( randomKey ) );

         sessionClientMocker.VerifyAll();
         fallbackClientMocker.VerifyAll();
      }

      [Test]
      public void Test_CanRoute_FlushAllCall ()
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         sessionClientMocker.Setup( c => c.FlushAll() );
         fallbackClientMocker.Setup( c => c.FlushAll() );

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient( fallbackClient, sessionClient );

         routedCacheClient.FlushAll();

         sessionClientMocker.VerifyAll();
         fallbackClientMocker.VerifyAll();
      }

      [Test]
      [TestCase( 1.5, "test" )]
      [TestCase( 100.0, "anotherTest" )]
      public void Test_CanRoute_SingleSetCall ( decimal sessionClientVal, string fallbackClientVal )
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         DateTime dateTimeRef = DateTime.Now;
         TimeSpan timeSpanRef = TimeSpan.FromHours( 1 );

         sessionClientMocker.Setup( c => c.Set<decimal>( sessionKey, sessionClientVal ) )
            .Returns( true );

         sessionClientMocker.Setup( c => c.Set<decimal>( sessionKey, sessionClientVal, dateTimeRef ) )
            .Returns( true );

         sessionClientMocker.Setup( c => c.Set<decimal>( sessionKey, sessionClientVal, timeSpanRef ) )
            .Returns( true );

         fallbackClientMocker.Setup( c => c.Set<string>( randomKey, fallbackClientVal ) )
            .Returns( true );

         fallbackClientMocker.Setup( c => c.Set<string>( randomKey, fallbackClientVal, dateTimeRef ) )
            .Returns( true );

         fallbackClientMocker.Setup( c => c.Set<string>( randomKey, fallbackClientVal, timeSpanRef ) )
            .Returns( true );

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient( fallbackClient, sessionClient );

         Assert.IsTrue( routedCacheClient.Set( sessionKey, sessionClientVal ) );
         Assert.IsTrue( routedCacheClient.Set( sessionKey, sessionClientVal, dateTimeRef ) );
         Assert.IsTrue( routedCacheClient.Set( sessionKey, sessionClientVal, timeSpanRef ) );

         Assert.IsTrue( routedCacheClient.Set( randomKey, fallbackClientVal ) );
         Assert.IsTrue( routedCacheClient.Set( randomKey, fallbackClientVal, dateTimeRef ) );
         Assert.IsTrue( routedCacheClient.Set( randomKey, fallbackClientVal, timeSpanRef ) );

         sessionClientMocker.VerifyAll();
         fallbackClientMocker.VerifyAll();
      }

      [Test]
      [TestCase( 1, 100 )]
      [TestCase( 25, 45 )]
      public void Test_CanRoute_IncrementCall ( int sessionClientVal, int fallbackClientVal )
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         sessionClientMocker.Setup( c => c.Increment( sessionKey, 1 ) )
            .Returns( sessionClientVal + 1 );

         fallbackClientMocker.Setup( c => c.Increment( randomKey, 10 ) )
            .Returns( fallbackClientVal + 10 );

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient( fallbackClient, sessionClient );

         Assert.AreEqual( sessionClientVal + 1,
            routedCacheClient.Increment( sessionKey, 1 ) );

         Assert.AreEqual( fallbackClientVal + 10,
            routedCacheClient.Increment( randomKey, 10 ) );

         sessionClientMocker.VerifyAll();
         fallbackClientMocker.VerifyAll();
      }

      [Test]
      [TestCase( 1, 100 )]
      [TestCase( 25, 45 )]
      public void Test_CanRoute_DecrementCall ( int sessionClientVal, int fallbackClientVal )
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>( MockBehavior.Strict );

         string randomKey = RandomKey;
         string sessionKey = SessionKey;
      }

      private IRoutedCacheClient CreateRoutedCacheClient ( ICacheClient fallbackClient, ICacheClient sessionClient, params IRoutedCacheClientRule[] rules )
      {
         IRoutedCacheClient routedCacheClient =
            new DefaultRoutedCacheClient( fallbackClient );

         routedCacheClient.RegisterRoutedClient( new KeyStartsWithStringCacheClientRule( sessionClient,
            StringComparison.InvariantCultureIgnoreCase,
            SessionKeyPrefix ) );

         foreach ( IRoutedCacheClientRule rule in rules )
            routedCacheClient.RegisterRoutedClient( rule );

         return routedCacheClient;
      }

      private string RandomKey => $"urn:randomkey:{Guid.NewGuid().ToString()}";

      private string SessionKey => $"urn:iauthsession:{Guid.NewGuid().ToString()}";

      private string RandomKeyPrefix => "urn:randomkey:";

      private string SessionKeyPrefix => "urn:iauthsession:";
   }
}
