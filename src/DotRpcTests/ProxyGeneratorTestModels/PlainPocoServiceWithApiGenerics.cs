namespace DotRpc.Tests.ProxyGeneratorTestModels
{
    public class PlainPocoServiceWithApiGenerics : IPlainPocoServiceWithApiGenerics
    {
        public ApiResponse<PlainPoco> Add(ApiRequest<string> request)
        {
            return new ApiResponse<PlainPoco> { Value = new() { Id = 1, Name = request.Value } };
        }
        public ApiResponse<PlainPoco> Add(ApiRequest<PlainPoco> request)
        {
            return new ApiResponse<PlainPoco> { Value = request.Value };
        }
        public ApiResponse<bool> Delete(ApiRequest<int> id)
        {
            return new ApiResponse<bool>() { Value = true };
        }
        public ApiResponse<bool> Delete(ApiRequest<PlainPoco> poco)
        {
            return new ApiResponse<bool>() { Value = true };
        }
        public ApiResponse<PlainPoco> Get(ApiRequest<int> id)
        {
            return new ApiResponse<PlainPoco> { Value = new() { Id = id.Value } };
        }
        public ApiResponse<IEnumerable<PlainPoco>> GetByIds(IEnumerable<ApiRequest<int>> ids)
        {
            return new ApiResponse<IEnumerable<PlainPoco>>() { Value = ids.Select(x => new PlainPoco() { Id = x.Value }) };
        }
    }

}
