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
using ServiceStack.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVD.ServiceStackRoutedCacheClient
{
   public class PerKeyCacheClientRuleAggregator
   {
      private IDictionary<Guid, IList<string>> mKeysForCacheClient =
         new Dictionary<Guid, IList<string>>();

      private IDictionary<Guid, ICacheClient> mCacheClients =
         new Dictionary<Guid, ICacheClient>();

      public void CollectAll(IEnumerable<string> keys, Func<string, IRoutedCacheClientRule> ruleSelector)
      {
         foreach (string key in keys)
         {
            IRoutedCacheClientRule rule = ruleSelector.Invoke(key);
            Collect(key, rule);
         }
      }

      public void Collect(string key, IRoutedCacheClientRule rule)
      {
         IList<string> keysForClient;

         if (!mKeysForCacheClient.TryGetValue(rule.Id, out keysForClient))
         {
            keysForClient = new List<string>();
            mKeysForCacheClient[rule.Id] = keysForClient;
            mCacheClients[rule.Id] = rule.Client;
         }

         keysForClient.Add(key);
      }

      public void Clear()
      {
         mKeysForCacheClient.Clear();
         mCacheClients.Clear();
      }

      public IDictionary<Guid, IList<string>> KeysForCacheClient
         => mKeysForCacheClient;

      public IDictionary<Guid, ICacheClient> CacheClients
         => mCacheClients;
   }
}
