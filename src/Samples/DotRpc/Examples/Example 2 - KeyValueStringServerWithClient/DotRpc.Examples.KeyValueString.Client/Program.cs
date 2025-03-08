using DotRpc;
using DotRpc.RpcClient;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddDotRpc();
services.AddSingleton<IRpcEndpointProvider>(new RpcEndpointProvider("https://localhost:50929"));
services.AddDotRpcClientsFromAssembly(typeof(IKeyValueStore).Assembly);
var provider = services.BuildServiceProvider();

var rpcClient = provider.GetRequiredService<IRpcClient<IKeyValueStore>>();
IKeyValueStore client = rpcClient.Service;
var key = "My Custom Key";
client.Add(key, "My Custom Value");

var value = client.Get("My Custom Key");

Console.WriteLine($"Client Result: {key}: {value}");

client.Remove(key);





