* Getting Started


To get started install the DotRpc Nuget package

``` csharp

Install-Package DotRpc

```

* Define Service Contracts and Implementations

To allow dynamic generation of clients, define your service contracts and implementation in a shared class library.

Common.Csproj.
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


* Add DotRpc Services to your web server.

Add a reference to Common.csproj in your server project and wire up dependency injection.

Register your services in depenency injection:

``` csharp

services.AddSingleton<IKeyValueStore, KeyValueStore>();

```

Wire up DotRpc to run your services.

``` csharp

app.AddDotRpcFromAssembly(typeof(Program).Assembly);

```

Here is a complete minimal program.cs along with configuration of swagger for testing:

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


* Creating and using a DotRpc Client

Add DotRpc Services to your service collection.
``` csharp

services.AddDotRpc();
services.AddDotRpcClientsFromAssembly(typeof(IKeyValueStore).Assembly);


```

After the service collection is build get a DotRpc client from the service provider and use it.


Add DotRpc Services to your service collection.

``` csharp

var client = provider.GetRequiredService<IKeyValueStore>();

var key = "My Custom Key";
client.Add(key, "My Custom Value");

var value = client.Get("My Custom Key");

Console.WriteLine($"Client Result: {key}: {value}");

client.Remove(key);
```


