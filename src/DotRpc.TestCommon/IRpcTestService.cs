using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Runtime.Serialization;
using System.ServiceModel;

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

    [DataContract]
    public class ApiResponse<T>
    {
        [DataMember(Order = 1)]
        public T Value { get; set; }

        public static implicit operator T(ApiResponse<T> response) => response.Value;
        public static implicit operator ApiResponse<T>(T value) => new ApiResponse<T> { Value = value };
    }

    [DataContract]
    public class ApiBoolResponse
    {
        [DataMember(Order = 1)]
        public bool Value { get; set; }

        //public static implicit operator bool(ApiBoolResponse response) => response.Value;
        //public static implicit operator ApiBoolResponse(bool value) => new ApiBoolResponse { Value = value };
    }

    [DataContract]
    public class ApiStringResponse
    {
        [DataMember(Order = 1)]
        public string Value { get; set; }

        //public static implicit operator string(ApiStringResponse response) => response.Value;
        //public static implicit operator ApiStringResponse(string value) => new ApiStringResponse { Value = value };
    }


    [RpcService(CallbackContract = typeof(IRpcTestServiceWithRequests))]
    [ServiceContract]
    public interface IRpcTestServiceWithRequests
    {
        ApiBoolResponse Add(AddValueRequest request);
        ApiStringResponse Get(GetValueRequest request);
        ApiStringResponse GetOrAdd(GetOrAddValueRequest request);
        ApiBoolResponse Remove(RemoveValueRequest request);
        ApiBoolResponse Set(SetValueRequest request);
    }

    [DataContract]
    public class AddValueRequest
    {
        [DataMember(Order = 1)]
        public string Key { get; set; }
        [DataMember(Order = 2)]
        public string Value { get; set; }
    }

    [DataContract]
    public class RemoveValueRequest
    {
        [DataMember(Order = 1)]
        public string Key { get; set; }
    }

    [DataContract]
    public class GetValueRequest
    {
        [DataMember(Order = 1)]
        public string Key { get; set; }
    }
    [DataContract]
    public class GetOrAddValueRequest
    {
        [DataMember(Order = 1)]
        public string Key { get; set; }
        [DataMember(Order = 2)]
        public string Value { get; set; }
    }
    [DataContract]
    public class SetValueRequest
    {
        [DataMember(Order = 1)]
        public string Key { get; set; }
        [DataMember(Order = 2)]
        public string Value { get; set; }
    }

}