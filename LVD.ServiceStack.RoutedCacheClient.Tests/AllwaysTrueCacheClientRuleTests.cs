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
		public void Test_AllwaysReturnsTrue_KeyNotEmpty ( string testKey )
		{
			Mock<ICacheClient> cacheClientMocker =
			   new Mock<ICacheClient>( MockBehavior.Loose );

			ICacheClient cacheClient = cacheClientMocker.Object;

			AlwaysTrueCacheClientRule rule =
			   new AlwaysTrueCacheClientRule( cacheClient );

			Assert.IsTrue( rule.Matches( testKey ) );
			Assert.AreSame( cacheClient, rule.Client );
		}

		[TestCase( "" )]
		[TestCase( null )]
		public void Test_AllwaysReturnsTrue_KeyNullOrEmpty ( string testKey )
		{
			Mock<ICacheClient> cacheClientMocker =
			   new Mock<ICacheClient>( MockBehavior.Loose );

			ICacheClient cacheClient = cacheClientMocker.Object;

			AlwaysTrueCacheClientRule rule =
			   new AlwaysTrueCacheClientRule( cacheClient );

			Assert.Throws<ArgumentNullException>( () => rule.Matches( testKey ) );
		}
	}
}
