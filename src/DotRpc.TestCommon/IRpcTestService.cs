namespace DotRpc.TestCommon
{

    [RpcService(CallbackContract = typeof(IRpcTestService))]
    public interface IRpcTestService
    {
        bool Add(string key, string value);
        bool Remove(string key);
        bool Set(string key, string value);

        bool Add(AddValueRequest request);
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
    public class SetValueRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

}