using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DotOrmLib.GrpcServices
{

    [DataContract]
    public class IntValue
    {
        [DataMember(Order = 1)]
        public int Value { get; set; }

        public static implicit operator IntValue(int value)
        {
            return new IntValue { Value = value };
        }

        public static implicit operator int(IntValue value)
        {
            return value.Value;
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

    [ServiceContract(Namespace = "http://DotOrmLib.Proxy")]
    public interface IServiceController<T>
    {
        ValueTask<T> Add(T entity);
        ValueTask<IntValue> Update(T entity);
        ValueTask<IntValue> Delete(IntValue id);
        ValueTask<T> GetById(IntValue idRequest);
        ValueTask<PaginatedResult<T>> GetList(int skip, int limit);
        ValueTask<HealthCheckResponse> HealthCheck();
    }

    public class ControllerBase<T>
        : IServiceController<T>
        where T : class
    {
        protected DotOrmRepo<T> repo;
        public ControllerBase()
            : this(ConnectionStringProvider.Create().ConnectionString)
        {

        }
        public ControllerBase(string connectionString)
        {
            this.repo = new DotOrmRepo<T>(connectionString);
        }

        public async ValueTask<T> Add(T entity)
        {
            var result = await repo.Add(entity);
            return result;
        }

        public async ValueTask<IntValue> Delete(IntValue request)
        {
            var result = await repo.DeleteById(request.Value);
            return new IntValue { Value = result };
        }

        public async ValueTask<T> GetById(IntValue request)
        {
            var result= await repo.GetById(request.Value);
            return result;
        }

        public async ValueTask<PaginatedResult<T>> GetList(int skip, int take)
        {
            var count = await repo.Count();
            var hasMore = skip + take < count;
            var items = await repo.GetList(skip, take);
            var result = new PaginatedResult<T>
            {
                Count = count,
                HasMore = hasMore,
                Items = items
            };
            return result;
        }

        public async ValueTask<PaginatedResult<T>> GetList(int skip, int take,
            string whereClause,
            string parameterJson)
        {
            var count = await repo.Count(whereClause, parameterJson);
            var hasMore = skip + take < count;
            var items = await repo.GetList(skip, take, whereClause, parameterJson);
            var result = new PaginatedResult<T>
            {
                Count = count,
                HasMore = hasMore,
                Items = items
            };
            return result;
        }

        public ValueTask<HealthCheckResponse> HealthCheck()
        {
            var result = $"[{DateTime.Now}] - Hello World";
            var response = new HealthCheckResponse { Result = result };
            return new ValueTask<HealthCheckResponse>(response);
        }

        public async ValueTask<IntValue> Update(T entity)
        {
            var result = await repo.Update(entity);
            return new IntValue { Value = result };
        }
    }

    [ServiceContract(Namespace = "http://DotOrmLib.Proxy.Scc1")]
    public interface IHealthCheckService
    {
        ValueTask<HealthCheckResponse> HealthCheck();
    }

    [DataContract]
    public class HealthCheckResponse
    {
        [DataMember(Order = 1)]
        public string Result { get; set; }
    }

    [DataContract]
    public class PagingParameters
    {
        [DataMember(Order = 1)]
        public int Skip { get; set; }
        [DataMember(Order = 2)]
        public int Take { get; set; }
    }


    public class HealthCheckService : IHealthCheckService
    {
        public ValueTask<HealthCheckResponse> HealthCheck()
        {
            var result = $"[{DateTime.Now}] - Hello world from DotOrm";
            var res = new HealthCheckResponse { Result = result };
            return new ValueTask<HealthCheckResponse>(res);
        }
    }

}
