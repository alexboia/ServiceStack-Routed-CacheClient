//
// Copyright (c) 2016-2017 Live Design SRL
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Caching;

namespace LiveLMS.RoutedCacheClient
{
   public interface IRoutedCacheClient : ICacheClientExtended
   {
      void RegisterRoutedClient ( IRoutedCacheClientRule rule );
   }
}
