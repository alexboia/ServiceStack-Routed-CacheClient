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

## How



## What's next

As a rough timeline, I would like to see the following happening:

	- Refactor things a bit and add some comments, at least for critical areas;
	- Add some more routing rules;
	- Additional automated tests.