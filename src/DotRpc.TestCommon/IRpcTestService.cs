namespace DotRpc.TestCommon
{

    [RpcService(CallbackContract = typeof(IRpcTestService))]
    public interface IRpcTestService
    {
        bool Add(string key, string value);
        bool AddOrUpdate(string key, string value);
        bool Remove(string key);
        bool Set(string key, string value);
        string? Get(string key);
        string GetOrAdd(string key, string value);

    }

    [RpcService(CallbackContract = typeof(IRpcTestService))]
    public interface IRpcGenericTestService
    {
        bool AddGeneric<T>(string key, T value);
        bool AddOrUpdateGeneric<T>(string key, T value);
        bool RemoveGeneric(string key);
        bool SetGeneric<T>(string key, T value);
        T? GetGeneric<T>(string key);
        T? GetOrAddGeneric<T>(string key, T value);

    }

    [RpcService(CallbackContract = typeof(IRpcTestServiceWithRequests))]
    public interface IRpcTestServiceWithRequests
    {
        bool Add(AddValueRequest request);
        string? Get(GetValueRequest request);
        string GetOrAdd(GetOrAddValueRequest request);
        bool Remove(RemoveValueRequest request);
        bool Set(SetValueRequest request);
    }
    public class AddValueRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class RemoveValueRequest
    {
        public string Key { get; set; }
    }
    public class GetValueRequest
    {
        public string Key { get; set; }
    }
    public class GetOrAddValueRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class SetValueRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

}