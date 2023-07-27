using DotOrmLib.Proxy.Scc1.Services;
using ProtoBuf.Grpc.Server;

namespace DotOrmGrpcServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
      

            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddCodeFirstGrpc();

            var app = builder.Build();
            var ctl = new TblScoringController();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<HealthCheckService>();
            app.MapGrpcService<TblScoringController>();
            app.MapGet("/Scc1/TblScoringController", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}