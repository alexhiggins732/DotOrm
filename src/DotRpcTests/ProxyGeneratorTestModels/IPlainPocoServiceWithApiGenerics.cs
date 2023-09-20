namespace DotRpc.Tests.ProxyGeneratorTestModels
{
    public interface IPlainPocoServiceWithApiGenerics
    {
        ApiResponse<PlainPoco> Add(ApiRequest<PlainPoco> request);
        ApiResponse<PlainPoco> Add(ApiRequest<string> request);
        ApiResponse<bool> Delete(ApiRequest<int> id);
        ApiResponse<bool> Delete(ApiRequest<PlainPoco> poco);
        ApiResponse<PlainPoco> Get(ApiRequest<int> id);
        ApiResponse<IEnumerable<PlainPoco>> GetByIds(IEnumerable<ApiRequest<int>> ids);
    }

}
