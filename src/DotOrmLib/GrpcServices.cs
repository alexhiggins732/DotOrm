using DotOrmLib.GrpcModels.Scalars;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
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
        ValueTask<Result<T>> Add(T entity);
        ValueTask<Result<IntValue>> Update(T entity);
        ValueTask<Result<IntValue>> Delete(IntValue id);
        ValueTask<Result<T>> GetById(IntValue idRequest);
        ValueTask<Result<PaginatedResult<T>>> GetList(FilterRequest filter);
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

        public Result<T> Ok(T entity) => new Result<T>(entity);
        public Result<TResult> Ok<TResult>(TResult entity) => new Result<TResult>(entity);
        public Result<T> Error(string errorMessage, IEnumerable<string> errorMessages)
            => new Result<T>(errorMessage, errorMessages.ToList());
        public Result<TResult> Error<TResult>(string errorMessage, IEnumerable<string> errorMessages)
            => new Result<TResult>(errorMessage, errorMessages.ToList());

        public async ValueTask<Result<T>> Add(T entity)
        {
            try
            {
                var result = await repo.Add(entity);
                return result;
            }
            catch (Exception ex)
            {
                return Error(ex.Message, new[] { ex.ToString() });
            }
        }

        public async ValueTask<Result<IntValue>> Delete(IntValue request)
        {
            try
            {
                var result = await repo.DeleteById(request.Value);
                return new IntValue { Value = result };
            }
            catch (Exception ex)
            {
                return Error<IntValue>(ex.Message, new[] { ex.ToString() });
            }
        }

        public async ValueTask<Result<T>> GetById(IntValue request)
        {
            try
            {
                var result = await repo.GetById(request.Value);
                return result;
            }
            catch (Exception ex)
            {
                return Error(ex.Message, new[] { ex.ToString() });
            }
        }

        public async ValueTask<Result<PaginatedResult<T>>> GetList(FilterRequest filter)
        {
            try
            {
                var count = await repo.Count(filter.WhereClause, filter.ParameterJson);
                var hasMore = filter.Skip + filter.Take < count;
                var items = await repo.GetList(filter.Skip, filter.Take, filter.WhereClause, filter.ParameterJson);
                var result = new PaginatedResult<T>
                {
                    Count = count,
                    HasMore = hasMore,
                    Items = items
                };
                return result;
            }
            catch (Exception ex)
            {
                return Error<PaginatedResult<T>>(ex.Message, new[] { ex.ToString() });
            }

        }

        public async ValueTask<Result<PaginatedResult<T>>> GetList(int skip, int take,
            string whereClause,
            string parameterJson)
        {
            try
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
            catch (Exception ex)
            {
                return Error<PaginatedResult<T>>(ex.Message, new[] { ex.ToString() });
            }
        }

        public ValueTask<HealthCheckResponse> HealthCheck()
        {
            var result = $"[{DateTime.Now}] - Hello World";
            var response = new HealthCheckResponse { Result = result };
            return new ValueTask<HealthCheckResponse>(response);
        }

        public async ValueTask<Result<IntValue>> Update(T entity)
        {
            try
            {
                var result = await repo.Update(entity);
                return new IntValue { Value = result };
            }
            catch (Exception ex)
            {
                return Error<IntValue>(ex.Message, new[] { ex.ToString() });
            }
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
