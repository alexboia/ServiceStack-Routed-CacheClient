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

      protected string mName;

      public BaseCacheClientRule(ICacheClient client)
         : this(null, client)
      {
         return;
      }

      public BaseCacheClientRule(string name, ICacheClient client)
      {
         if (client == null)
            throw new ArgumentNullException(nameof(client));

         mClient = client;
         mName = name ?? InferName(client.GetType());
      }

      private string InferName(Type type)
      {
         return string.Format("{0}{1}_{2}",
            char.ToLowerInvariant(type.Name[0]),
            type.Name.Substring(1),
            mId.ToString().Replace("-", "_"));
      }

      public abstract bool Matches(string key);

      public virtual string Name => mName;

      public virtual ICacheClient Client => mClient;

      public virtual Guid Id => mId;
   }
}
