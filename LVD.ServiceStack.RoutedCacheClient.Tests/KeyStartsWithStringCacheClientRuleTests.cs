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

namespace LVD.ServiceStackRoutedCacheClient.Tests
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
