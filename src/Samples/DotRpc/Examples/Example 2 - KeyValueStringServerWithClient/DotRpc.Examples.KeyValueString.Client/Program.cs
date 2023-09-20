using DotRpc;
using DotRpc.RpcClient;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddDotRpc();
services.AddDotRpcClientsFromAssembly(typeof(IKeyValueStore).Assembly);
var provider = services.BuildServiceProvider();

var client = provider.GetRequiredService<IKeyValueStore>();

var key = "My Custom Key";
client.Add(key, "My Custom Value");

var value = client.Get("My Custom Key");

Console.WriteLine($"Client Result: {key}: {value}");

client.Remove(key);





