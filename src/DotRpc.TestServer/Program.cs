using DotRpc;
using DotRpc.TestCommon;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DotRpc.TestServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddDotRpc();
            var services = builder.Services;
            services.AddSingleton<IRpcTestService, RpcTestService>();
            services.AddHttpContextAccessor();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            var app = builder.Build();



            //AddServices(typeof(IActionsController));
            app.AddDotRpcFromAssembly(typeof(IRpcTestService).Assembly);
            app.UseSwagger();
            app.UseSwaggerUI();
            // Configure the HTTP request pipeline.

            app.MapGet("/Scc1/TblScoringController", () => "Communication with DotRpc endpoints must be made through a DotRpc client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }

    public class RpcTestService : IRpcTestService
    {
        static Dictionary<string, string> _cache = new();
        public RpcTestService() { }

        public bool Add(string key, string value)
        {
           return _cache.TryAdd(key, value);
        }

        public bool Add(AddValueRequest request)
        {
            return _cache.TryAdd(request.Key, request.Value);
        }

        public bool Remove(string key)
        {
            return _cache.Remove(key);
        }

        public bool Remove(RemoveValueRequest request)
        {
            return _cache.Remove(request.Key);
        }

        public bool Set(string key, string value)
        {
            return (_cache[key] = value) == value;
        }

        public bool Set(SetValueRequest request)
        {
            return (_cache[request.Key] = request.Value) == request.Value;
        }
    }

}