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
   public class DefaultRoutedCacheClientTests
   {
      [Test]
      [TestCase(0, null)]
      [TestCase(1, "test")]
      public void Test_CanRoute_SingleAddCalls(int sessionClientVal, string fallbackClientVal)
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         DateTime dateTimeRef = DateTime.Now;
         TimeSpan timeSpanRef = TimeSpan.FromHours(1);

         sessionClientMocker.Setup(c => c.Add<int>(sessionKey, sessionClientVal))
            .Returns(true);
         sessionClientMocker.Setup(c => c.Add<int>(sessionKey, sessionClientVal, dateTimeRef))
            .Returns(true);
         sessionClientMocker.Setup(c => c.Add<int>(sessionKey, sessionClientVal, timeSpanRef))
            .Returns(true);

         fallbackClientMocker.Setup(c => c.Add<string>(randomKey, fallbackClientVal))
            .Returns(true);
         fallbackClientMocker.Setup(c => c.Add<string>(randomKey, fallbackClientVal, dateTimeRef))
            .Returns(true);
         fallbackClientMocker.Setup(c => c.Add<string>(randomKey, fallbackClientVal, timeSpanRef))
            .Returns(true);

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClient, sessionClient);

         routedCacheClient.Add<int>(sessionKey, sessionClientVal);
         routedCacheClient.Add<int>(sessionKey, sessionClientVal, dateTimeRef);
         routedCacheClient.Add<int>(sessionKey, sessionClientVal, timeSpanRef);

         routedCacheClient.Add<string>(randomKey, fallbackClientVal);
         routedCacheClient.Add<string>(randomKey, fallbackClientVal, timeSpanRef);
         routedCacheClient.Add<string>(randomKey, fallbackClientVal, dateTimeRef);

         sessionClientMocker.Verify(c => c.Add<int>(sessionKey, sessionClientVal),
            Times.Once);
         sessionClientMocker.Verify(c => c.Add<int>(sessionKey, sessionClientVal, dateTimeRef),
            Times.Once);
         sessionClientMocker.Verify(c => c.Add<int>(sessionKey, sessionClientVal, timeSpanRef),
            Times.Once);

         fallbackClientMocker.Verify(c => c.Add<string>(randomKey, fallbackClientVal),
            Times.Once);
         fallbackClientMocker.Verify(c => c.Add<string>(randomKey, fallbackClientVal, dateTimeRef),
            Times.Once);
         fallbackClientMocker.Verify(c => c.Add<string>(randomKey, fallbackClientVal, timeSpanRef),
            Times.Once);

         sessionClientMocker.VerifyNoOtherCalls();
         fallbackClientMocker.VerifyNoOtherCalls();
      }

      [Test]
      [TestCase(0, null)]
      [TestCase(10, "bogus")]
      public void Test_CanRoute_SingleGetCalls(int sessionClientVal, string fallbackClientVal)
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         sessionClientMocker.Setup(c => c.Get<int>(sessionKey))
            .Returns(sessionClientVal);

         fallbackClientMocker.Setup(c => c.Get<string>(randomKey))
            .Returns(fallbackClientVal);

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClient, sessionClient);

         Assert.AreEqual(sessionClientVal, routedCacheClient
            .Get<int>(sessionKey));
         Assert.AreEqual(fallbackClientVal, routedCacheClient
            .Get<string>(randomKey));

         sessionClientMocker.Verify(c => c.Get<int>(sessionKey),
            Times.Once);
         fallbackClientMocker.Verify(c => c.Get<string>(randomKey),
            Times.Once);

         sessionClientMocker.VerifyNoOtherCalls();
         fallbackClientMocker.VerifyNoOtherCalls();
      }

      [Test]
      public void Test_CanRoute_SingleRemoveCalls()
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         sessionClientMocker.Setup(c => c.Remove(sessionKey))
            .Returns(true);

         fallbackClientMocker.Setup(c => c.Remove(randomKey))
            .Returns(true);

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClient, sessionClient);

         Assert.IsTrue(routedCacheClient.Remove(sessionKey));
         Assert.IsTrue(fallbackClient.Remove(randomKey));

         sessionClientMocker.Verify(c => c.Remove(sessionKey),
            Times.Once);
         fallbackClientMocker.Verify(c => c.Remove(randomKey),
            Times.Once);

         sessionClientMocker.VerifyNoOtherCalls();
         fallbackClientMocker.VerifyNoOtherCalls();
      }

      [Test]
      public void Test_CanRoute_FlushAllCall_UniqueCacheClients()
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         sessionClientMocker.Setup(c => c.FlushAll());
         fallbackClientMocker.Setup(c => c.FlushAll());

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClient, sessionClient);

         routedCacheClient.FlushAll();

         sessionClientMocker.Verify(c => c.FlushAll(),
            Times.Once);
         fallbackClientMocker.Verify(c => c.FlushAll(),
            Times.Once);

         sessionClientMocker.VerifyNoOtherCalls();
         fallbackClientMocker.VerifyNoOtherCalls();
      }

      [Test]
      public void Test_CanRoute_FlushAllCall_SameCacheClientForMultipleRules()
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         sessionClientMocker.Setup(c => c.FlushAll());
         fallbackClientMocker.Setup(c => c.FlushAll());

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClient,
               new KeyStartsWithStringCacheClientRule(sessionClient, StringComparison.InvariantCultureIgnoreCase, "xyz:"),
               new KeyStartsWithStringCacheClientRule(sessionClient, StringComparison.InvariantCultureIgnoreCase, "session:"));

         routedCacheClient.FlushAll();

         sessionClientMocker.Verify(c => c.FlushAll(),
            Times.Once);
         fallbackClientMocker.Verify(c => c.FlushAll(),
            Times.Once);

         sessionClientMocker.VerifyNoOtherCalls();
         fallbackClientMocker.VerifyNoOtherCalls();
      }

      [Test]
      [TestCase(0, null)]
      [TestCase(1.5, "test")]
      [TestCase(100.0, "anotherTest")]
      public void Test_CanRoute_SingleSetCall(decimal sessionClientVal, string fallbackClientVal)
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         DateTime dateTimeRef = DateTime.Now;
         TimeSpan timeSpanRef = TimeSpan.FromHours(1);

         sessionClientMocker.Setup(c => c.Set<decimal>(sessionKey, sessionClientVal))
            .Returns(true);
         sessionClientMocker.Setup(c => c.Set<decimal>(sessionKey, sessionClientVal, dateTimeRef))
            .Returns(true);
         sessionClientMocker.Setup(c => c.Set<decimal>(sessionKey, sessionClientVal, timeSpanRef))
            .Returns(true);

         fallbackClientMocker.Setup(c => c.Set<string>(randomKey, fallbackClientVal))
            .Returns(true);
         fallbackClientMocker.Setup(c => c.Set<string>(randomKey, fallbackClientVal, dateTimeRef))
            .Returns(true);
         fallbackClientMocker.Setup(c => c.Set<string>(randomKey, fallbackClientVal, timeSpanRef))
            .Returns(true);

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClient, sessionClient);

         Assert.IsTrue(routedCacheClient.Set(sessionKey, sessionClientVal));
         Assert.IsTrue(routedCacheClient.Set(sessionKey, sessionClientVal, dateTimeRef));
         Assert.IsTrue(routedCacheClient.Set(sessionKey, sessionClientVal, timeSpanRef));

         Assert.IsTrue(routedCacheClient.Set(randomKey, fallbackClientVal));
         Assert.IsTrue(routedCacheClient.Set(randomKey, fallbackClientVal, dateTimeRef));
         Assert.IsTrue(routedCacheClient.Set(randomKey, fallbackClientVal, timeSpanRef));

         sessionClientMocker.Verify(c => c.Set<decimal>(sessionKey, sessionClientVal),
            Times.Once);
         sessionClientMocker.Verify(c => c.Set<decimal>(sessionKey, sessionClientVal, dateTimeRef),
            Times.Once);
         sessionClientMocker.Verify(c => c.Set<decimal>(sessionKey, sessionClientVal, timeSpanRef),
            Times.Once);

         fallbackClientMocker.Verify(c => c.Set<string>(randomKey, fallbackClientVal),
            Times.Once);
         fallbackClientMocker.Verify(c => c.Set<string>(randomKey, fallbackClientVal, dateTimeRef),
            Times.Once);
         fallbackClientMocker.Verify(c => c.Set<string>(randomKey, fallbackClientVal, timeSpanRef),
            Times.Once);

         sessionClientMocker.VerifyNoOtherCalls();
         fallbackClientMocker.VerifyNoOtherCalls();
      }

      [Test]
      [TestCase(1, 100)]
      [TestCase(25, 45)]
      public void Test_CanRoute_IncrementCall(int sessionClientVal, int fallbackClientVal)
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         sessionClientMocker.Setup(c => c.Increment(sessionKey, 1))
            .Returns(sessionClientVal + 1);

         fallbackClientMocker.Setup(c => c.Increment(randomKey, 10))
            .Returns(fallbackClientVal + 10);

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClient, sessionClient);

         Assert.AreEqual(sessionClientVal + 1,
            routedCacheClient.Increment(sessionKey, 1));

         Assert.AreEqual(fallbackClientVal + 10,
            routedCacheClient.Increment(randomKey, 10));

         sessionClientMocker.Verify(c => c.Increment(sessionKey, 1),
            Times.Once);
         fallbackClientMocker.Verify(c => c.Increment(randomKey, 10),
            Times.Once);

         sessionClientMocker.VerifyNoOtherCalls();
         fallbackClientMocker.VerifyNoOtherCalls();
      }

      [Test]
      [TestCase(1, 100)]
      [TestCase(25, 45)]
      public void Test_CanRoute_DecrementCall(int sessionClientVal, int fallbackClientVal)
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         string randomKey = RandomKey;
         string sessionKey = SessionKey;

         sessionClientMocker.Setup(c => c.Decrement(sessionKey, 1))
           .Returns(sessionClientVal - 1);

         fallbackClientMocker.Setup(c => c.Decrement(randomKey, 10))
            .Returns(fallbackClientVal - 10);

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClient, sessionClient);

         Assert.AreEqual(sessionClientVal - 1,
            routedCacheClient.Decrement(sessionKey, 1));

         Assert.AreEqual(fallbackClientVal - 10,
            routedCacheClient.Decrement(randomKey, 10));

         sessionClientMocker.Verify(c => c.Decrement(sessionKey, 1),
            Times.Once);
         fallbackClientMocker.Verify(c => c.Decrement(randomKey, 10),
            Times.Once);

         sessionClientMocker.VerifyNoOtherCalls();
         fallbackClientMocker.VerifyNoOtherCalls();
      }

      [Test]
      public void Test_CanClearRules_HasOnlyFallbackClientRule()
      {
         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClientMocker.Object);

         Assert.AreEqual(1, routedCacheClient.RulesCount);

         routedCacheClient.ClearRules();

         Assert.AreEqual(1, routedCacheClient.RulesCount);
      }

      [Test]
      public void Test_CanClearRules_HasManyClientRules()
      {
         Mock<ICacheClient> sessionClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         Mock<ICacheClient> fallbackClientMocker =
            new Mock<ICacheClient>(MockBehavior.Strict);

         ICacheClient sessionClient = sessionClientMocker.Object;
         ICacheClient fallbackClient = fallbackClientMocker.Object;

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClient,
               new KeyStartsWithStringCacheClientRule(sessionClient, StringComparison.InvariantCultureIgnoreCase, "xyz:"),
               new KeyStartsWithStringCacheClientRule(sessionClient, StringComparison.InvariantCultureIgnoreCase, "session:"));

         Assert.AreEqual(3, routedCacheClient.RulesCount);

         routedCacheClient.ClearRules();

         Assert.AreEqual(1, routedCacheClient.RulesCount);
      }

      [Test]
      [TestCase("xyz:")]
      [TestCase("session:")]
      public void Test_CanGetKeysByPattern_HasOnlyFallbackClientRule(string pattern)
      {
         List<string> expectedResult =
            new List<string> { "x", "y", "z" };

         Mock<ICacheClientExtended> fallbackClientMocker =
            new Mock<ICacheClientExtended>(MockBehavior.Strict);

         fallbackClientMocker.Setup(c => c.GetKeysByPattern(pattern))
            .Returns(expectedResult);

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClientMocker.Object);

         IEnumerable<string> actualResult = routedCacheClient.GetKeysByPattern(pattern);

         Assert.NotNull(actualResult);
         CollectionAssert.AreEqual(expectedResult, actualResult);

         fallbackClientMocker.Verify(c => c.GetKeysByPattern(pattern),
            Times.Once);

         fallbackClientMocker.VerifyNoOtherCalls();
      }

      [Test]
      [TestCase("xyz:")]
      [TestCase("session:")]
      public void Test_CanGetKeysByPattern_HasManyClientRules_AllDifferent(string pattern)
      {
         List<string> expectedResult =
            new List<string> { "x", "y", "z", "a", "b", "c", "1", "2", "3" };

         Mock<ICacheClientExtended> fallbackClientMocker =
            new Mock<ICacheClientExtended>(MockBehavior.Strict);

         Mock<ICacheClientExtended> client1Mocker =
            new Mock<ICacheClientExtended>(MockBehavior.Strict);

         Mock<ICacheClientExtended> client2Mocker =
            new Mock<ICacheClientExtended>(MockBehavior.Strict);

         fallbackClientMocker.Setup(c => c.GetKeysByPattern(pattern))
            .Returns(new List<string>() { "x", "y", "z" });

         client1Mocker.Setup(c => c.GetKeysByPattern(pattern))
            .Returns(new List<string>() { "a", "b", "c" });

         client2Mocker.Setup(c => c.GetKeysByPattern(pattern))
            .Returns(new List<string>() { "1", "2", "3" });

         IRoutedCacheClient routedCacheClient =
            CreateRoutedCacheClient(fallbackClientMocker.Object,
               new KeyStartsWithStringCacheClientRule(client1Mocker.Object,
                  StringComparison.InvariantCultureIgnoreCase,
                  Guid.NewGuid().ToString()),
               new KeyStartsWithStringCacheClientRule(client2Mocker.Object,
                  StringComparison.InvariantCultureIgnoreCase,
                  Guid.NewGuid().ToString()));

         IEnumerable<string> actualResult = routedCacheClient.GetKeysByPattern(pattern);

         Assert.NotNull(actualResult);
         CollectionAssert.AreEquivalent(expectedResult, actualResult);

         fallbackClientMocker.Verify(c => c.GetKeysByPattern(pattern),
            Times.Once);
         client1Mocker.Verify(c => c.GetKeysByPattern(pattern),
            Times.Once);
         client2Mocker.Verify(c => c.GetKeysByPattern(pattern),
            Times.Once);

         fallbackClientMocker.VerifyNoOtherCalls();
         client1Mocker.VerifyNoOtherCalls();
         client2Mocker.VerifyNoOtherCalls();
      }

      [Test]
      [TestCase(0)]
      [TestCase(1)]
      [TestCase(5)]
      [TestCase(10)]
      public void Test_CanGetRegisteredCacheClients_SameType(int additionalRuleCount)
      {
         Mock<ICacheClientExtended> fallbackClientMocker =
            new Mock<ICacheClientExtended>(MockBehavior.Loose);

         DefaultRoutedCacheClient cacheClient =
            new DefaultRoutedCacheClient(fallbackClientMocker.Object);

         for (int i = 0; i < additionalRuleCount; i++)
            cacheClient.PushClientWithRule(new KeyStartsWithStringCacheClientRule(new MockCacheClient1(),
               StringComparison.InvariantCultureIgnoreCase,
               $":test{i}"));

         IDictionary<string, ICacheClient> cacheClients = cacheClient
            .GetRegisteredClients();

         IDictionary<string, ICacheClient> cacheClientsFromExt = cacheClient
            .GetRegisteredCacheClients();

         CollectionAssert.AreEquivalent(cacheClients,
            cacheClientsFromExt);

         Assert.AreEqual(additionalRuleCount + 1,
            cacheClients.Count);

         Assert.AreEqual(additionalRuleCount,
            cacheClients.Count(r => r.Key.StartsWith("mockCacheClient1")));
      }

      [Test]
      [TestCase(0, 0)]
      [TestCase(0, 1)]
      [TestCase(1, 0)]
      [TestCase(0, 5)]
      [TestCase(5, 0)]
      [TestCase(1, 5)]
      [TestCase(5, 1)]
      [TestCase(5, 10)]
      [TestCase(10, 5)]
      public void Test_CanGetRegisteredCacheClients_DifferentTypes(int additionalRuleCount1, int additionalRuleCount2)
      {
         Mock<ICacheClientExtended> fallbackClientMocker =
            new Mock<ICacheClientExtended>(MockBehavior.Loose);

         DefaultRoutedCacheClient cacheClient =
            new DefaultRoutedCacheClient(fallbackClientMocker.Object);

         for (int i = 0; i < additionalRuleCount1; i++)
            cacheClient.PushClientWithRule(new KeyStartsWithStringCacheClientRule(new MockCacheClient1(),
               StringComparison.InvariantCultureIgnoreCase,
               $":test1{i}"));

         for (int i = 0; i < additionalRuleCount2; i++)
            cacheClient.PushClientWithRule(new KeyStartsWithStringCacheClientRule(new MockCacheClient2(),
               StringComparison.InvariantCultureIgnoreCase,
               $":test2{i}"));

         IDictionary<string, ICacheClient> cacheClients = cacheClient
            .GetRegisteredClients();

         IDictionary<string, ICacheClient> cacheClientsFromExt = cacheClient
            .GetRegisteredCacheClients();

         CollectionAssert.AreEquivalent(cacheClients,
            cacheClientsFromExt);

         Assert.AreEqual(additionalRuleCount1 + additionalRuleCount2 + 1,
            cacheClients.Count);

         Assert.AreEqual(additionalRuleCount1,
            cacheClients.Count(r => r.Key.StartsWith("mockCacheClient1")));

         Assert.AreEqual(additionalRuleCount2,
            cacheClients.Count(r => r.Key.StartsWith("mockCacheClient2")));
      }

      private IRoutedCacheClient CreateRoutedCacheClient(ICacheClient fallbackClient, ICacheClient sessionClient, params IRoutedCacheClientRule[] rules)
      {
         IRoutedCacheClientRule sessionCacheClientRule = new KeyStartsWithStringCacheClientRule(sessionClient,
            StringComparison.InvariantCultureIgnoreCase,
            SessionKeyPrefix);

         IRoutedCacheClientRule[] newRules = new IRoutedCacheClientRule[rules.Length + 1];

         newRules[0] = sessionCacheClientRule;
         for (int i = 0; i < rules.Length; i++)
            newRules[i + 1] = rules[i];

         return CreateRoutedCacheClient(fallbackClient,
            newRules);
      }

      private IRoutedCacheClient CreateRoutedCacheClient(ICacheClient fallbackClient, params IRoutedCacheClientRule[] rules)
      {
         IRoutedCacheClient routedCacheClient =
            new DefaultRoutedCacheClient(fallbackClient);

         foreach (IRoutedCacheClientRule rule in rules)
            routedCacheClient.PushClientWithRule(rule);

         return routedCacheClient;
      }

      private string RandomKey => $"urn:randomkey:{Guid.NewGuid().ToString()}";

      private string SessionKey => $"urn:iauthsession:{Guid.NewGuid().ToString()}";

      private string RandomKeyPrefix => "urn:randomkey:";

      private string SessionKeyPrefix => "urn:iauthsession:";
   }
}
