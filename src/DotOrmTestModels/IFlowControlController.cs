
using System.ServiceModel;

namespace DotOrmLib.Proxy
{
    namespace Scc1
    {
        using Azure;
        using Azure.Core;
        using DotOrmLib.GrpcModels.Scalars;
        using DotOrmLib.GrpcServices;
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


            public class DynamicServiceController : ControllerBase<RefactorLog>, IRefactorLogController
            {
                // Implement the methods from the IRefactorLogController interface here

            }

            [ServiceContract(Namespace = "http://DotOrmLib.Proxy.Scc1")]
            public interface ITblScoringController1
                 : IServiceController<TblScoring>
            {
                ValueTask<HealthCheckResponse> HealthCheck();
                new ValueTask<Result<TblScoring>> GetById(IntValue request);
                new ValueTask<Result<IntValue>> DeleteById(IntValue request);
                new ValueTask<Result<IntValue>> Delete(TblScoring request);
                new ValueTask<Result<IntValue>> Update(TblScoring request);
                new ValueTask<Result<PaginatedResult<TblScoring>>> GetList(FilterRequest pageParams);
            }

            public class TblScoringController :
                ControllerBase<TblScoring>,
                ITblScoringController1
            //IServiceController<TblScoring>
            {

                public TblScoringController() : base(ConnectionStringProvider.Create("scc1").ConnectionString) { }

                public async ValueTask<Result<IntValue>> Delete(TblScoring request)
                {
                    var result = await repo.Delete(request);
                    return (IntValue)result;
                }

                public async ValueTask<Result<IntValue>> DeleteById(IntValue request)
                {
                    var result = await repo.DeleteById(request.Value);
                    return (IntValue)result;
                }

                public new async ValueTask<Result<TblScoring>> GetById(IntValue request)
                {
                    var result = await base.GetById(request);
                    return result;
                }


                public async ValueTask<Result<PaginatedResult<TblScoring>>> GetList(FilterRequest request)
                {

                    var param = JsonConvert.DeserializeObject(request.ParameterJson);
                    var result = await GetList(request.Skip, request.Take, request.WhereClause, request.ParameterJson);
                    return result;
                }

                public new async ValueTask<Result<IntValue>> Update(TblScoring request)
                {
                    var result = await repo.Update(request);
                    return (IntValue)result;
                }
                public ValueTask<HealthCheckResponse> HealthCheck()
                {
                    var result = $"[{DateTime.Now}] - Hello World";
                    var response = new HealthCheckResponse { Result = result };
                    return new ValueTask<HealthCheckResponse>(response);
                }

            }





        }

    }


}

