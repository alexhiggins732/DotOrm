using DotOrmLib.Proxy.Scc1.Services;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace DotOrmLib.Proxy;


public interface IServiceController<T>
{
    //ValueTask<T> Add(T entity);
    //ValueTask<T> Update(T entity);
    //ValueTask<int> Delete(int id);
    //ValueTask<T> GetById(IntValue idRequest);
    //ValueTask<PaginatedResult<T>> GetList(int skip, int limit);
    ValueTask<HealthCheckResponse> HealthCheck();
}

[DataContract]
public class IntValue
{
    [DataMember(Order = 1)]
    public int Value { get; set; }

    public static implicit operator IntValue(int value)
    {
        return new IntValue { Value = value };
    }
}

[DataContract]
public class PaginatedResult<T>
{
    [DataMember(Order = 1)]
    public int Count { get; set; }
    [DataMember(Order = 21)]
    public bool HasMore { get; set; }
    [DataMember(Order = 3)]
    public List<T> Items { get; set; } = null!;
}
