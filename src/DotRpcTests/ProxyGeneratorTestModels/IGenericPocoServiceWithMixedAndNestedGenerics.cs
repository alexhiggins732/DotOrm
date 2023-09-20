namespace DotRpc.Tests.ProxyGeneratorTestModels
{
    public interface IGenericPocoServiceWithMixedAndNestedGenerics<T1>
    {
        ApiResponse<GenericPoco<T1, T2>> Add<T2>(ApiRequest<GenericPoco<T1, T2>> request);
        ApiResponse<GenericPoco<T1, T2>> Add<T2>(ApiRequest<T2> request);
        ApiResponse<bool> Delete(ApiRequest<T1> id);
        ApiResponse<bool> DeleteAdd<T2>(ApiRequest<GenericPoco<T1, T2>> poco);
        ApiResponse<GenericPoco<T1, T2>> Get<T2>(ApiRequest<T1> id);
        ApiResponse<IEnumerable<GenericPoco<T1, T2>>> GetByIds<T2>(IEnumerable<ApiRequest<T1>> ids);
    }
}