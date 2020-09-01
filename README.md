# ServiceStack Routed Cache Client

## What

The ServiceStack Routed Cache Client is an `ICacheClient` implementation 
(`ICacheClientExtended` to be more exact) that acts as a facade 
to a collection of multiple other cache clients. 
It has minimum overhead and does not store cache values itself, but only routes 
various cache operations (read, write, flush, remove etc.) to a specific registered client, 
based on an associated rule  (for instance, whether the key starts with a given string).  

## Why 

The main issue for me was (and possibly many others) that I could not register 
a different cache client for session storage (say, `OrmLiteCacheClient`), 
whilst continuing to make use of the standard `MemoryCacheClient `.  

## Installation

Available as a NuGet package, [here](https://www.nuget.org/packages/LVD.ServiceStack.RoutedCacheClient/).

### Via Package Manager

`Install-Package LVD.ServiceStack.RoutedCacheClient -Version 1.0.0`

### Via .NET CLI
`dotnet add package LVD.ServiceStack.RoutedCacheClient --version 1.0.0`

## How to use

### 1. Add namespace reference

`using LVD.ServiceStackRoutedCacheClient`.

### 2. Create your routed cache client instance

In this example, the default `MemoryCacheClient` provided by ServiceStack is used as a fallback client.

```csharp
IRoutedCacheClient routedCacheClient = new DefaultRoutedCacheClient(new MemoryCacheClient());
```

### 3. Register your other cache clients

For example, registering a ServiceStack OrmLite cache client for ServiceStack sessions:

```csharp
OrmLiteCacheClient<CacheEntry> ormLiteCacheClient =
    new OrmLiteCacheClient<CacheEntry>();

routedCacheClient.PushClientWithRule(new KeyStartsWithStringCacheClientRule(ormLiteCacheClient,
    StringComparison.InvariantCultureIgnoreCase,
    "urn:iauthsession:",
    "sess:"));
```

Or simpler, using `PushServiceStackSessionCacheClient()`, which automatically registers your provided cache client
with a `KeyStartsWithStringCacheClientRule` and the above prefixes: `urn:iauthsession:` and `sess:`.

```csharp
OrmLiteCacheClient<CacheEntry> ormLiteCacheClient =
    new OrmLiteCacheClient<CacheEntry>();

routedCacheClient.PushServiceStackSessionCacheClient(ormLiteCacheClient);
```

### 4. Built-in rules

The library provides the following built-in rules:

- `GenericConditionBasedCacheClientRule` - matches keys that satisfy a given condition; 
- `KeyStartsWithStringCacheClientRule` - matches keys that start with a given sub-string (uses `KeyStartsWithStringCacheClientRuleCondition`);
- `KeyEndsWithStringCacheClientRule` - matches keys that start with a given sub-string (uses `KeyEndsWithStringCacheClientRuleCondition`);
- `AlwaysTrueCacheClientRule` - matches keys that start with a given sub-string (uses `AlwaysTrueCacheClientRuleCondition`);
- `AlwaysFalseCacheClientRule` - matches keys that start with a given sub-string (uses `AlwaysFalseCacheClientRuleCondition`).

### 5. Extending the Routing Behaviour

#### 5.2. Creating Custom Conditions

Creating your own conditions is as easy as implementing `IRoutedCacheClientRuleCondition` 
with the single method `bool Matches ( string key )`.
This is also the recommended way of exteding the library's routing behaviour.

As an example, here is the implementation of the built-in `KeyStartsWithStringCacheClientRuleCondition` condition:

```csharp
public class KeyStartsWithStringCacheClientRuleCondition : IRoutedCacheClientRuleCondition
{
	private List<string> mTokens = new List<string>();

	private StringComparison mStringComparisonMode;

	public KeyStartsWithStringCacheClientRuleCondition ( StringComparison stringComparisonMode,
		params string[] tokens )
	{
		if ( tokens == null || tokens.Length == 0 )
			throw new ArgumentNullException( nameof( tokens ) );

		mTokens.AddRange( tokens );
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

	public StringComparison StringComparisonMode 
		=> mStringComparisonMode;
}
```

#### 5.1. Creating Custom Rules

Creating your own rules is as easy as 
	- extending the `GenericConditionBasedCacheClientRule` class and providing a condition to this base class (this is a useful shortcut to always instantiating `GenericConditionBasedCacheClientRule` with the same condition). 
	- or extending the `BaseCacheClientRule` class and providing an implementation for the `bool Matches ( string key )` method (should you wish to bypass using a condition altogether).

Example #1 - the implementation of the built-in `KeyStartsWithStringCacheClientRule` rule:

```csharp
public class KeyStartsWithStringCacheClientRule : GenericConditionBasedCacheClientRule
{
	public KeyStartsWithStringCacheClientRule ( ICacheClient cacheClient,
		KeyStartsWithStringCacheClientRuleCondition condition )
		: base( cacheClient, condition )
	{
		return;
	}

	public KeyStartsWithStringCacheClientRule ( ICacheClient cacheClient,
		StringComparison stringComparisonMode,
		params string[] tokens )
		: this( cacheClient, new KeyStartsWithStringCacheClientRuleCondition( stringComparisonMode, tokens ) )
	{
		return;
	}
}
```

Example #2 - the implementation of the built-in `GenericConditionBasedCacheClientRule` rule:
```csharp
public class GenericConditionBasedCacheClientRule : BaseCacheClientRule
{
	private IRoutedCacheClientRuleCondition mCondition;

	public GenericConditionBasedCacheClientRule ( ICacheClient cacheClient, IRoutedCacheClientRuleCondition condition )
		: base( cacheClient )
	{
		mCondition = condition
			?? throw new ArgumentNullException( nameof( condition ) );
	}

	public override bool Matches ( string key )
	{
		if ( string.IsNullOrWhiteSpace( key ) )
			throw new ArgumentNullException( nameof( key ) );

		return mCondition.Matches( key );
	}

	private IRoutedCacheClientRuleCondition Condition 
		=> mCondition;
}
```

### 6. Sample project

A sample project has been provided, to see just how this would behave in the real world.  
Head over to `LVD.ServiceStack.RoutedCacheClient.Example` for the full details, but, in short, it exposes three service endpoints:

- `/set-cache-randomly` - set some cache values using the `Service.Cache` instance;
- `/set-session-randomly` - set some session values using the `Service.SessionBag` instance;
- `/list-cache-providers-data` - list all values for each of the cache providers.

The output of `/list-cache-providers-data` looks something like:

```json
{
    "cacheProvidersData": {
        "fallbackCacheClient": {
            "SampleCacheClientService.randomText": "ef42894b-fbb6-4971-b983-0aea9e88f10a",
            "SampleCacheClientService2020-04-06 10:51.hitCountsPerMinute": "12",
            "SampleCacheClientService2020-04-06 10:52.hitCountsPerMinute": "4"
        },
        "sessionCacheClient": {
            "sess:vKQ8aph3Xdp1ro9YqBzK:SampleCacheClientService.sessionRandomText": "e2f581da-ad0c-4896-8b4f-b3646f79bc7e",
            "sess:vKQ8aph3Xdp1ro9YqBzK:SampleCacheClientService.hitCountsPerSession": "12"
        }
    }
}
```

## Breaking changes

### In Version 1.1.0

- `KeyStartsWithStringCacheClientRule` no longer extends `BaseCacheClientRule`, but `GenericConditionBasedCacheClientRule`;
- `ServiceStackSessionKeyCacheClientRule` no longer extends `BaseCacheClientRule`, but `GenericConditionBasedCacheClientRule`;
- `AlwaysTrueCacheClientRule` no longer extends `BaseCacheClientRule`, but `GenericConditionBasedCacheClientRule`;
- `AlwaysTrueCacheClientRule` now throws an exception when checking for an empty key (calling `.Matches(null|string.Empty)`);
- Calling any method on a disposed `DefaultRoutedCacheClient` instance throws `ObjectDisposedException`.

## Changelog

### Version 1.1.0

- Added utility methods to inspect cache client configuration - [Issue #4](https://github.com/alexboia/ServiceStack-Routed-CacheClient/issues/4);
- Refactoring - extracted the explicit concept of condition from that of a cache client rule - [Issue #2](https://github.com/alexboia/ServiceStack-Routed-CacheClient/issues/2);
- Additional conditions provided:
    - cache key ends with substring - [Issue #5](https://github.com/alexboia/ServiceStack-Routed-CacheClient/issues/5);
    - cache key matches a given regexp - [Issue #6](https://github.com/alexboia/ServiceStack-Routed-CacheClient/issues/6);
    - compose conditions using OR operator;
    - compose conditions using AND operator.
- Improved test coverage and code comments.

### Version 1.0.0

Initial release.

## What's next

As a rough timeline, I would like to see the following happening:

- Refactor things a bit and add some comments, at least for critical areas;
- Add some more routing rules. Here are some of the rules I'm thinking about:
	- ~~match a regex pattern~~;
	- ~~match rules that ends with a given string~~;
	- ~~composed rules using basic `AND`/`OR` logical operators~~.
- Enhance the API:
	- ~~would be useful to have a way of listing all the registered cache clients~~;
	- ~~allow method chaining when registering cache client rules~~.
- Additional automated tests (in-progress).

## Donate

I put some of my free time into developing and maintaining this plugin.
If helped you in your projects and you are happy with it, you can...

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Q5Q01KGLM)