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
using System.Text;
using NUnit.Framework;
using LVD.ServiceStackRoutedCacheClient.Conditions;
using Bogus;

namespace LVD.ServiceStackRoutedCacheClient.Tests
{
	[TestFixture]
	public class MatchAnyRuleConditionTests
	{
		[Test]
		[TestCase( 1 )]
		[TestCase( 5 )]
		[TestCase( 10 )]
		[Repeat( 10 )]
		public void Test_CanMatchAnyRuleConditions_AllTrue ( int nRules )
		{
			Faker faker = new Faker();
			IRoutedCacheClientRuleCondition[] conditions =
				new IRoutedCacheClientRuleCondition[ nRules ];

			for ( int i = 0; i < nRules; i++ )
				conditions[ i ] = new AlwaysTrueCacheClientRuleCondition();

			MatchAnyRuleCondition cond = new MatchAnyRuleCondition( conditions );

			Assert.IsTrue( cond.Matches( faker.Random.AlphaNumeric( 10 ) ) );
		}

		[Test]
		[TestCase( 1, 1 )]
		[TestCase( 1, 5 )]
		[TestCase( 5, 1 )]
		[TestCase( 5, 10 )]
		[Repeat( 10 )]
		public void Test_TryMatchAnyRuleConditions_SomeFalse ( int nTruthyRules, int nFalsyRules )
		{
			Faker faker = new Faker();
			IRoutedCacheClientRuleCondition[] conditions =
				new IRoutedCacheClientRuleCondition[ nTruthyRules + nFalsyRules ];

			for ( int i = 0; i < nTruthyRules; i++ )
				conditions[ i ] = new AlwaysTrueCacheClientRuleCondition();

			for ( int i = 0; i < nFalsyRules; i++ )
				conditions[ i + nTruthyRules ] = new AlwaysFalseCacheClientRuleCondition();

			MatchAnyRuleCondition cond = new MatchAnyRuleCondition( conditions );

			Assert.IsTrue( cond.Matches( faker.Random.AlphaNumeric( 10 ) ) );
		}

		[Test]
		[TestCase( 1 )]
		[TestCase( 5 )]
		[TestCase( 10 )]
		[Repeat( 10 )]
		public void Test_TryMatchAnyRuleConditions_AllFalse ( int nRules )
		{
			Faker faker = new Faker();
			IRoutedCacheClientRuleCondition[] conditions =
				new IRoutedCacheClientRuleCondition[ nRules ];

			for ( int i = 0; i < nRules; i++ )
				conditions[ i ] = new AlwaysFalseCacheClientRuleCondition();

			MatchAnyRuleCondition cond = new MatchAnyRuleCondition( conditions );

			Assert.IsFalse( cond.Matches( faker.Random.AlphaNumeric( 10 ) ) );
		}
	}
}
