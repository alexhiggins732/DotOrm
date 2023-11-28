using DotRpc;
using DotRpc.TestCommon;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using DotRpc.TestCommon.IamCrudService;
using ProtoBuf.Grpc.Server;
using System.Net;
using Newtonsoft.Json;

namespace DotRpc.TestServer
{
    public class Program
    {


        static void Main(string[] args)
        {
            Log.Info("Starting server");
            try
            {
                bool IsTest = args.Any(x => x.Equals("test", StringComparison.OrdinalIgnoreCase));

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                Server.RunServer(args, IsTest);
            }
            catch (Exception ex)
            {
                Log.Error($"Unhandled Exception: {ex}");
            }

        }
    }
    public class Log
    {
        public static void Info(string message, bool appendDate = true)
        {
            if (appendDate)
                message = $"{DateTime.Now} {message}";
            Console.Write(message);
            File.AppendAllLines("log.txt", new[] { message });
        }
        public static void Error(string message, bool appendDate = true)
        {
            if (appendDate)
                message = $"{DateTime.Now} [Error] {message}";
            else message = $"[Error] {message}";
            Console.Write(message);
            File.AppendAllLines("log.txt", new[] { message });
        }
    }
    public partial class Server
    {
        public static WebApplication WebApplication { get; private set; }
        public static void RunServer(string[] args, bool IsTest)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddDotRpc();
            var services = builder.Services;

            services.AddSingleton<IRpcTestService, RpcTestService>();
            services.AddSingleton<IRpcTestServiceWithRequests, RpcTestServiceWithRequests>();
            services.AddSingleton<IRpcGenericTestService, RpcGenericTestService>();
            services.AddSingleton<IIntEntityCrudService<App>, IntEntityCrudService<App>>();
            services.AddSingleton<IAppCrudService, AppCrudService>();
            services.AddHttpContextAccessor();
            services.AddEndpointsApiExplorer();

            services.AddDotRpcSwagger();
            services.AddCodeFirstGrpc();

            var app = builder.Build();



            //AddServices(typeof(IActionsController));
            app.UseDotRpcFromAssembly(typeof(IRpcTestService).Assembly);
            app.UseDotRpcSwagger();
            app.UseSwagger();
            app.UseSwaggerUI();
            // Configure the HTTP request pipeline.
            app.MapGrpcService<RpcTestServiceWithRequests>();
            app.MapGet("/Scc1/TblScoringController", () => "Communication with DotRpc endpoints must be made through a DotRpc client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
            WebApplication = app;
            if (!IsTest)
            {
                app.Run();
            }
            else
            {
                app.RunAsync();
            }

        }

    }

    public class RpcGenericTestService : IRpcGenericTestService
    {
        static ConcurrentDictionary<string, object> _cache = new(StringComparer.OrdinalIgnoreCase);
        public RpcGenericTestService() { }

        public bool AddGeneric<T>(string key, T value)
        {
            var result = _cache.TryAdd(key, value);
            return result;
        }

        public bool AddOrUpdateGeneric<T>(string key, T value)
        {
            bool result = AddGeneric(key, value);
            if (!result)
                result = SetGeneric(key, value);
            return result;
            //var result = _cache.AddOrUpdate(key, value, (key, existingValue) => value);
            //return result == value;
        }

        public T? GetGeneric<T>(string key)
        {
            _cache.TryGetValue(key, out var value);
            return (T)value;
        }

        public T GetOrAddGeneric<T>(string key, T value)
        {
            var result = _cache.GetOrAdd(key, value);
            return (T)result;
        }

        public bool RemoveGeneric(string key)
        {
            var result = _cache.TryRemove(key, out var value);
            return result;
        }

        public bool SetGeneric<T>(string key, T value)
        {
            var result = _cache.TryUpdate(key, value, value);
            if (!result)
            {
                _cache[key] = value;
                result = true;
            }
            return result;
        }


    }

    public class RpcTestService : IRpcTestService
    {
        static ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);
        public RpcTestService() { }

        public bool Add(string key, string value)
        {
            var result = _cache.TryAdd(key, value);
            return result;
        }

        public bool AddOrUpdate(string key, string value)
        {
            var result = _cache.AddOrUpdate(key, value, (key, existingValue) => value);
            return result == value;
        }

        public string? Get(string key)
        {
            _cache.TryGetValue(key, out var value);
            return value;
        }

        public string GetOrAdd(string key, string value)
        {
            var result = _cache.GetOrAdd(key, value);
            return result;
        }

        public bool Remove(string key)
        {
            var result = _cache.TryRemove(key, out var value);
            return result;
        }

        public bool Set(string key, string value)
        {
            var result = _cache.TryUpdate(key, value, key);
            return result;
        }


    }

    public class RpcTestServiceWithRequests : IRpcTestServiceWithRequests
    {
        static ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);
        public RpcTestServiceWithRequests() { }



        public ApiBoolResponse Add(AddValueRequest request)
        {
            Log.Info($"Processing request: {JsonConvert.SerializeObject(request)}");
            try
            {
                var result = _cache.TryAdd(request.Key, request.Value);
                var response = new ApiBoolResponse { Value = result };
                Log.Info($"Returning response request: {JsonConvert.SerializeObject(response)}");
                return response;
            }
            catch (Exception ex)
            {
                Log.Error($"Unhanled exception: {ex}");

                var response = new ApiBoolResponse { Value = false };
                return response;
            }
         
        }

        public ApiStringResponse Get(GetValueRequest request)
        {
            _cache.TryGetValue(request.Key, out var value);
            return new ApiStringResponse { Value = value };
        }

        public ApiStringResponse GetOrAdd(GetOrAddValueRequest request)
        {
            var value = _cache.GetOrAdd(request.Key, request.Value);
            return new ApiStringResponse { Value = value };
        }

        public ApiBoolResponse Remove(RemoveValueRequest request)
        {
            var result = _cache.TryRemove(request.Key, out var value);
            return new ApiBoolResponse { Value = result };

        }

        public ApiBoolResponse Set(SetValueRequest request)
        {
            var result = _cache.TryUpdate(request.Key, request.Value, request.Key);
            return new ApiBoolResponse { Value = result };
        }
    }
}