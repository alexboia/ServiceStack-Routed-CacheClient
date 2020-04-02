using ServiceStack.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace LVD.ServiceStackRoutedCacheClient
{
   public abstract class BaseCacheClientRule : IRoutedCacheClientRule
   {
      protected Guid mId = Guid.NewGuid();

      protected ICacheClient mClient;

      public BaseCacheClientRule(ICacheClient client)
      {
         if (client == null)
            throw new ArgumentNullException(nameof(client));
         mClient = client;
      }

      public abstract bool Matches(string key);

      public virtual ICacheClient Client => mClient;

      public virtual Guid Id => mId;
   }
}
