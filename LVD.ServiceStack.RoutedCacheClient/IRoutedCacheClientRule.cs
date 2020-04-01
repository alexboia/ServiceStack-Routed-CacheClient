//
// Copyright (c) 2016-2017 Live Design SRL
// All rights reserved.
//
using ServiceStack.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveLMS.RoutedCacheClient
{
   public interface IRoutedCacheClientRule
   {
      bool Matches ( string key );

      ICacheClient Client { get; }

      Guid Id { get; }
   }
}
