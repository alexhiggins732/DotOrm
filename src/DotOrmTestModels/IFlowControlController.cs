
using System.ServiceModel;

namespace DotOrmLib.Proxy
{
    namespace Scc1
    {
        using Azure;
        using Azure.Core;
        using DotOrmLib.Proxy.Scc1.Models;
        using Interfaces;
        using Microsoft.IdentityModel.Tokens;
        using Models;
        using Newtonsoft.Json;
        using System.Collections.Generic;
        using System.Data.Common;
        using System.Runtime.CompilerServices;
        using System.Runtime.Serialization;
        using System.Threading.Tasks;

        namespace Services
        {
            public class ControllerBase<T>
                : IServiceController<T>
                where T : class
            {
                protected private DapperRepoBase<T> repo;
                public ControllerBase()
                    : this(ConnectionStringProvider.Create("Scc1").ConnectionString)
                {

                }
                public ControllerBase(string connectionString)
                {
                    this.repo = new DapperRepoBase<T>(connectionString);
                }

                public async ValueTask<T> Add(T entity)
                {
                    var result = await repo.Add(entity);
                    return result;
                }

                public async ValueTask<IntValue> Delete(IntValue request)
                {
                    var result= await repo.DeleteById(request.Value);
                    return new IntValue { Value = result };
                }

                public async ValueTask<T> GetById(IntValue request)
                {
                    return await repo.GetById(request.Value);
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
                    var response= new HealthCheckResponse { Result=result};
                    return new ValueTask<HealthCheckResponse>(response);
                }

                public async ValueTask<IntValue> Update(T entity)
                {
                    var result = await repo.Update(entity);
                    return new IntValue { Value = result };
                }
            }

            [ServiceContract(Namespace = "http://DotOrmLib.Proxy.Scc1")]
            public interface ITblScoringController1
                 :IServiceController<TblScoring>
            {
                ValueTask<HealthCheckResponse> HealthCheck();
                new ValueTask<TblScoring> GetById(IntValue request);
                new ValueTask<IntValue> DeleteById(IntValue request);
                new ValueTask<IntValue> Delete(TblScoring request);
                new ValueTask<IntValue> Update(TblScoring request);
                new ValueTask<PaginatedResult<TblScoring>> GetList(FilterRequest pageParams);
            }

            public class TblScoringController :
                ControllerBase<TblScoring>,
                ITblScoringController1
                //IServiceController<TblScoring>
            {
     
                public TblScoringController() : base(ConnectionStringProvider.Create("scc1").ConnectionString) { }

                public async ValueTask<IntValue> Delete(TblScoring request)
                {
                    var result = await repo.Delete(request);
                    return result;
                }

                public async ValueTask<IntValue> DeleteById(IntValue request)
                {
                    var result = await repo.DeleteById(request.Value);
                    return result;
                }

                public new async ValueTask<TblScoring> GetById(IntValue request)
                {
                    var result = await base.GetById(request);
                    return result;
                }

       
                public async ValueTask<PaginatedResult<TblScoring>> GetList(FilterRequest request)
                {
                    
                    var param = JsonConvert.DeserializeObject(request.ParameterJson);
                    var result = await GetList(request.Skip, request.Take, request.WhereClause, request.ParameterJson);
                    return result;
                }

                public new async ValueTask<IntValue> Update(TblScoring request)
                {
                    var result= await repo.Update(request);
                    return result;
                }
                public ValueTask<HealthCheckResponse> HealthCheck()
                {
                    var result = $"[{DateTime.Now}] - Hello World";
                    var response = new HealthCheckResponse { Result = result };
                    return new ValueTask<HealthCheckResponse>(response);
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

    }


}

