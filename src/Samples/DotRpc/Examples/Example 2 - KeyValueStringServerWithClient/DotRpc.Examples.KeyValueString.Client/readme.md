Getting Started


To get started install the DotRpc Nuget package

``` csharp

Install-Package DotRpc

```

Define your service contract:

``` csharp
using DotRpc;

[RpcService]
public interface IKeyValueStore
{
    bool Add(string key, string value);
    bool AddOrUpdate(string key, string value);
    string? Get(string key);
    string GetOrAdd(string key, string value);
    bool Remove(string key);
    bool Set(string key, string value);
}

	
Implement Your Service:

``` csharp
public class KeyValueStore : IKeyValueStore
{
    static ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);
    public KeyValueStore() { }

    public bool Add(string key, string value)
    {
        var result = _cache.TryAdd(key, value);
        return result;
    }

    public bool AddOrUpdate(string key, string value)
    {
        var result = _cache.AddOrUpdate(key, value, (key, existingValue) => value);
        return result == value;
    }

    public string? Get(string key)
    {
        _cache.TryGetValue(key, out var value);
        return value;
    }

    public string GetOrAdd(string key, string value)
    {
        var result = _cache.GetOrAdd(key, value);
        return result;
    }

    public bool Remove(string key)
    {
        var result = _cache.TryRemove(key, out var value);
        return result;
    }

    public bool Set(string key, string value)
    {
        var result = _cache.TryUpdate(key, value, key);
        return result;
    }
}

```

Register your services in depenency injection:

``` csharp

services.AddSingleton<IKeyValueStore, KeyValueStore>();

```

Wire up DotRpc to run your services.

``` csharp

app.AddDotRpcFromAssembly(typeof(Program).Assembly);

```

A complete minimal program.cs along with configuration of swagger for testing:

``` csharp

using DotRpc;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDotRpc();
var services = builder.Services;

// register your services
services.AddSingleton<IKeyValueStore, KeyValueStore>();



services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SchemaGeneratorOptions.SchemaIdSelector = x => x.DotRpcSwaggerSchemaIdGenerator();
});

var app = builder.Build();


app.AddDotRpcFromAssembly(typeof(Program).Assembly);
app.UseSwagger();
app.UseSwaggerUI();

app.Run();

```

Launch you application and browse to /swagger.

[DotRpc.Examples.KeyValueStringServer.Swagger.png]

Test out your new api.

Add a key/value pair.
[DotRpc.Examples.KeyValueStringServer.Swagger.AddKey.png]

Get a value.
[DotRpc.Examples.KeyValueStringServer.Swagger.GetKey.png]

Remove a value.
[DotRpc.Examples.KeyValueStringServer.Swagger.RemoveKey.png]


