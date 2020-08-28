using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Caching;

namespace LVD.ServiceStackRoutedCacheClient
{
	/// <summary>
	/// Utility extension methods for working with IRoutedCacheClient instances
	/// </summary>
	public static class RoutedCacheClientExtensions
	{
		/// <summary>
		/// Registers the given cache client as being used for ServiceStack sessions 
		///		with the given target routed cache client.
		///	Essentially, what this does is register a ServiceStackSessionKeyCacheClientRule 
		///		with the given routed cache client, backed by the given cache client.
		/// </summary>
		/// <param name="routedClient">The target routed cache client</param>
		/// <param name="cacheClient">The cache client that will be used for session storage.</param>
		/// <returns></returns>
		public static IRoutedCacheClient PushServiceStackSessionCacheClient ( this IRoutedCacheClient routedClient, ICacheClient cacheClient )
		{
			if ( routedClient == null )
				throw new ArgumentNullException( nameof( routedClient ) );

			routedClient.PushClientWithRule( new ServiceStackSessionKeyCacheClientRule( cacheClient ) );
			return routedClient;
		}
	}
}
