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

## What's next

As a rough timeline, I would like to see the following happening:

- Refactor things a bit and add some comments, at least for critical areas;
- Add some more routing rules;
- Additional automated tests.