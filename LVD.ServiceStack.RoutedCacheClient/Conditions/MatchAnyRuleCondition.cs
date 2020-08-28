﻿// 
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

namespace LVD.ServiceStackRoutedCacheClient.Conditions
{
	public class MatchAnyRuleCondition : IRoutedCacheClientRuleCondition
	{
		private IRoutedCacheClientRuleCondition[] mInnerConditions;

		public MatchAnyRuleCondition ( params IRoutedCacheClientRuleCondition[] conditions )
		{
			if ( conditions == null || conditions.Length == 0 )
				throw new ArgumentNullException( nameof( conditions ) );

			mInnerConditions = conditions;
		}

		public bool Matches ( string key )
		{
			if ( string.IsNullOrEmpty( key ) )
				throw new ArgumentNullException( nameof( key ) );

			bool matches = false;

			foreach ( IRoutedCacheClientRuleCondition cond in mInnerConditions )
			{
				if ( cond.Matches( key ) )
				{
					matches = true;
					break;
				}
			}

			return matches;
		}
	}
}
