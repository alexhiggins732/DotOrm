using DotOrmLib.GrpcModels.Services;
using DotOrmLib.GrpcServices;
using DotOrmLib.Proxy.Scc1.Interfaces;
using DotOrmLib.Proxy.Scc1.Services;
using ProtoBuf.Grpc.Server;
using System.Reflection.Emit;
using System.Reflection;
using System.ServiceModel;
using DotOrmLib;

namespace DotOrmGrpcServer
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var defaultconn = ConnectionStringProvider.Create("Scc1");
            var ctl = new TblScoringController();
            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddCodeFirstGrpc();
            var services = builder.Services;
            services.AddHttpContextAccessor();
            services.AddEndpointsApiExplorer();
          
            var app = builder.Build();
            app.AddDynamicGrpcServiceFromTypeAssembly(typeof(IActionsController));

            //AddServices(typeof(IActionsController));


            // Configure the HTTP request pipeline.
            app.MapGrpcService<HealthCheckService>();
            app.MapGrpcService<TblScoringController>();
            app.MapGrpcService<GrpcScalarTestService>();
            app.MapGet("/Scc1/TblScoringController", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }

    }
}