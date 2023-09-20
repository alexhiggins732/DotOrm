namespace DotRpc.Tests.ProxyGeneratorTestModels
{
    public interface IGenericPocoServiceWithNestedGenerics<T1, T2>
    {
        ApiResponse<GenericPoco<T1, T2>> Add(ApiRequest<GenericPoco<T1, T2>> request);
        ApiResponse<GenericPoco<T1, T2>> Add(ApiRequest<T2> request);
        ApiResponse<bool> Delete(ApiRequest<GenericPoco<T1, T2>> poco);
        ApiResponse<bool> Delete(ApiRequest<T1> id);
        ApiResponse<GenericPoco<T1, T2>> Get(ApiRequest<T1> id);
        ApiResponse<IEnumerable<GenericPoco<T1, T2>>> GetByIds(IEnumerable<ApiRequest<T1>> ids);
    }
}