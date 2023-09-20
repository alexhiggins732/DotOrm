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
