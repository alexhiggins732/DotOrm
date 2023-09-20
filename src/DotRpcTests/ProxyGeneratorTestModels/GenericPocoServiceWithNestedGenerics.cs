namespace DotRpc.Tests.ProxyGeneratorTestModels
{
    public class GenericPocoServiceWithNestedGenerics<T1, T2> : IGenericPocoServiceWithNestedGenerics<T1, T2>
    {
        public ApiResponse<GenericPoco<T1, T2>> Add(ApiRequest<T2> request)
        {
            return new ApiResponse<GenericPoco<T1, T2>> { Value = new() { Name = request.Value } };
        }
        public ApiResponse<GenericPoco<T1, T2>> Add(ApiRequest<GenericPoco<T1, T2>> request)
        {
            return new ApiResponse<GenericPoco<T1, T2>> { Value = request.Value };
        }
        public ApiResponse<bool> Delete(ApiRequest<T1> id)
        {
            return new ApiResponse<bool>() { Value = true };
        }
        public ApiResponse<bool> Delete(ApiRequest<GenericPoco<T1, T2>> poco)
        {
            return new ApiResponse<bool>() { Value = true };
        }
        public ApiResponse<GenericPoco<T1, T2>> Get(ApiRequest<T1> id)
        {
            return new ApiResponse<GenericPoco<T1, T2>> { Value = new() { Id = id.Value } };
        }
        public ApiResponse<IEnumerable<GenericPoco<T1, T2>>> GetByIds(IEnumerable<ApiRequest<T1>> ids)
        {
            return new ApiResponse<IEnumerable<GenericPoco<T1, T2>>>() { Value = ids.Select(x => new GenericPoco<T1, T2>() { Id = x.Value }) };
        }
    }

}
