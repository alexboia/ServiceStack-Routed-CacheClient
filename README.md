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

### 1. Add namespace referance

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

### 4. Creating Custom Rules

The library only comes with a handful of rules, but creating your own 
is as easy as extending the `BaseCacheClientRule` class and providing 
an implementation for the `bool Match(string key)` method. 

As an example, here is the implementation of the built-in `KeyStartsWithStringCacheClientRule` rule:

```csharp
public class KeyStartsWithStringCacheClientRule : BaseCacheClientRule
{
    private List<string> mTokens = new List<string>();

    private StringComparison mStringComparisonMode;

    public KeyStartsWithStringCacheClientRule(ICacheClient cacheClient,
        StringComparison stringComparisonMode,
        params string[] tokens)
        : base(cacheClient)
    {
        if (tokens == null || tokens.Length == 0)
        throw new ArgumentNullException(nameof(tokens));

        mTokens.AddRange(tokens);
        mStringComparisonMode = stringComparisonMode;
    }

    public override bool Matches(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        throw new ArgumentNullException(nameof(key));

        foreach (string token in mTokens)
        if (key.StartsWith(token, mStringComparisonMode))
            return true;

        return false;
    }

    public StringComparison StringComparisonMode => mStringComparisonMode;
}
```

### 5. Sample project

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

## What's next

As a rough timeline, I would like to see the following happening:

- Refactor things a bit and add some comments, at least for critical areas;
- Add some more routing rules. Here are some of the rules I'm thinking about:
	- match a regex pattern;
	- match rules that ends with a given string;
	- composed rules using basic `AND`/`OR` logical operators.
- Enhance the API:
	- would be useful to have a way of listing all the registered cache clients;
	- allow method chaining when registering cache client rules.
- Additional automated tests.

## Donate

I put some of my free time into developing and maintaining this plugin.
If helped you in your projects and you are happy with it, you can...

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Q5Q01KGLM)