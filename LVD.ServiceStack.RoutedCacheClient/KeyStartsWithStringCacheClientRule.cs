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
   public class KeyStartsWithStringCacheClientRule : IRoutedCacheClientRule
   {
      private List<string> mTokens = new List<string>();

      private ICacheClient mCacheClient;

      private Guid mId = Guid.NewGuid();

      private StringComparison mStringComparisonMode;

      public KeyStartsWithStringCacheClientRule ( ICacheClient cacheClient,
         StringComparison stringComparisonMode,
         params string[] tokens )
      {
         if ( tokens == null || tokens.Length == 0 )
            throw new ArgumentNullException( nameof( tokens ) );

         if ( cacheClient == null )
            throw new ArgumentNullException( nameof( cacheClient ) );

         mTokens.AddRange( tokens );
         mCacheClient = cacheClient;
         mStringComparisonMode = stringComparisonMode;
      }

      public bool Matches ( string key )
      {
         if ( string.IsNullOrWhiteSpace( key ) )
            throw new ArgumentNullException( nameof( key ) );

         foreach ( string token in mTokens )
            if ( key.StartsWith( token, mStringComparisonMode ) )
               return true;

         return false;
      }

      public ICacheClient Client => mCacheClient;

      public StringComparison StringComparisonMode => mStringComparisonMode;

      public Guid Id => mId;
   }
}
