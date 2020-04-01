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
   public class AllwaysTrueCacheClientRule : IRoutedCacheClientRule
   {
      private ICacheClient mClient;

      private Guid mId = Guid.NewGuid();

      public AllwaysTrueCacheClientRule ( ICacheClient cacheClient )
      {
         if ( cacheClient == null )
            throw new ArgumentNullException( nameof( cacheClient ) );
         mClient = cacheClient;
      }

      public bool Matches ( string key )
      {
         return true;
      }

      public ICacheClient Client => mClient;

      public Guid Id => mId;
   }
}
