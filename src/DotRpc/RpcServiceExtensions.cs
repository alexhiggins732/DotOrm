using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

namespace DotRpc
{
    using static Utils;
    namespace RpcClient
    {
        public static class RpcClientExtensions
        {
            public static IServiceCollection AddDotRpcClientsFromAssembly(
                this IServiceCollection services,
                Type assembly,
                Action<IServiceCollection>? configure = null)
            {
                return services.AddDotRpcClientsFromAssembly(assembly.Assembly, configure);
            }

            public static IServiceCollection AddDotRpcClientsFromAssembly(
                this IServiceCollection services,
                Assembly assembly,
                Action<IServiceCollection>? configure = null)
            {
                var contracts = assembly.GetTypes().Where(x => IsRpcContact(x)).ToList();
                Dictionary<Type, Dictionary<MethodInfo, RpcEndpointConfiguration>> rpcServices = new();
                var clientServiceType = typeof(IRpcClient<>);
                var clientImplementationType = typeof(RpcClient<>);
                contracts.ForEach(contractType =>
                {

                    var att = contractType.GetCustomAttribute<RpcServiceAttribute>();
                    var area = $"/{att.Name}/";
                    Dictionary<MethodInfo, RpcEndpointConfiguration> serviceCalls = new();
                    foreach (var method in contractType.GetMethods())
                    {

                        var requestType = TypeFactory.GetMethodProxyType(method);
                        var path = NameService.GetApiPathName(method);
                        var routeEndpoint = $"{area}{path}";

                        //app.MapPost(routeEndpoint,

                        //    //(IRpcHandler handler, RpcPayload request) =>
                        //    //    handler.HandleRequest(method, request)
                        //    (IRpcHandler handler,
                        //    IDataContractRequestFormatter formatter,
                        //    [FromBody] JsonElement requestData) =>
                        //        handler.HandleRequest(method, formatter.Map(requestType, requestData))
                        //    )
                        //.Accepts(requestType, "application/json")
                        //.Produces(200, method.ReturnType);// TypeFactory.GetMethodProxyType(method));

                        var config = new RpcEndpointConfiguration(routeEndpoint, requestType, method.ReturnType, contractType, method);
                        serviceCalls.Add(method, config);
                        //serviceCalls.Add(method, (IRpcPayloadClient client, IPayloadFormatter formatter, MethodInfo method, object[] args) =>
                        //    {
                        //        var response = client.Invoke(serviceEndpoint, method, args);
                        //        return formatter.To(method.ReturnType, response);
                        //    }
                        //   );
                    }
                    rpcServices.Add(contractType, serviceCalls);

                    var serviceType = clientServiceType.MakeGenericType(contractType);
                    var implementationType = clientImplementationType.MakeGenericType(contractType);
                    //DotRpc.RpcClient`1[DotRpc.TestCommon.IRpcTestService]
                    //DotRpc.RpcClient`1[[DotRpc.TestCommon.IRpcTestService]
                    services.AddTransient(serviceType, implementationType);
                });

                services.AddSingleton<IRpcConfigurationProvider>(sp => new RpcConfigurationProvider(rpcServices));


                configure?.Invoke(null);
                configure?.Invoke(services);
                return services;
            }
        }

        public class RpcConfigurationProvider : IRpcConfigurationProvider
        {
            public Dictionary<Type, Dictionary<MethodInfo, RpcEndpointConfiguration>> rpcServices;

            public RpcConfigurationProvider(Dictionary<Type, Dictionary<MethodInfo, RpcEndpointConfiguration>> rpcServices)
            {
                this.rpcServices = rpcServices;
            }

            public RpcEndpointConfiguration? GetEndpointConfiguration(Type serviceType, MethodInfo serviceMethod)
            {
                if (rpcServices.ContainsKey(serviceType))
                {
                    var contract = rpcServices[serviceType];
                    if (contract.ContainsKey(serviceMethod))
                    {
                        return contract[serviceMethod];
                    }
                }
                return null;
            }
        }

        public interface IRpcConfigurationProvider
        {
            RpcEndpointConfiguration? GetEndpointConfiguration(Type serviceType, MethodInfo serviceMethod);
        }

        public class RpcEndpointConfiguration
        {
            public RpcEndpointConfiguration(string routeEndpoint, Type requestType, Type returnType, Type contractType, MethodInfo method)
            {
                RouteEndpoint = routeEndpoint;
                RequestType = requestType;
                ReturnType = returnType;
                ContractType = contractType;
                Method = method;
            }

            public string RouteEndpoint { get; }
            public Type RequestType { get; }
            public Type ReturnType { get; }
            public Type ContractType { get; }
            public MethodInfo Method { get; }
        }
    }
    public static class RpcServiceExtensions
    {
        public static IServiceCollection AddDotRpc(
            this IServiceCollection services,
            Action<IServiceCollection>? configure = null)
        {
            configure?.Invoke(services);

            services.AddSingleton<IResponseFormatter, JsonResponseFormatter>();
            services.AddSingleton<IRpcMapper, RpcMapper>();
            services.AddSingleton<IArgumentMapper, ArgumentMapper>();
            services.AddSingleton<IJsonRequestFormatter, JsonRequestFormatter>();
            services.AddSingleton<IMethodInvoker, MethodInvoker>();
            services.AddSingleton<IRpcHandler, RpcHandler>();
            services.AddSingleton<IDataContractRequestFormatter, DataContractRequestFormatter>();
            return services;
        }
        public static WebApplication AddDotRpcFromAssembly(
            this WebApplication app,
            Assembly assembly,
            Action<WebApplication>? configure = null)
        {
            var contracts = assembly.GetTypes().Where(x => IsRpcContact(x)).ToList();
            contracts.ForEach(serviceType =>
            {
                var att = serviceType.GetCustomAttribute<RpcServiceAttribute>();
                var area = $"/{att.Name}/";
                foreach (var method in serviceType.GetMethods())
                {
                    var requestType = TypeFactory.GetMethodProxyType(method);
                    var path = NameService.GetApiPathName(method);
                    var routeEndpoint = $"{area}{path}";
                    app.MapPost(routeEndpoint,

                        //(IRpcHandler handler, RpcPayload request) =>
                        //    handler.HandleRequest(method, request)
                        (
                            IServiceProvider serviceProvider,
                            IRpcHandler handler,

                        IDataContractRequestFormatter formatter,
                        [FromBody] JsonElement requestData) =>
                            {
                                return HandleRequest(serviceProvider, handler, formatter, requestData, serviceType, requestType, method);
                            }
                        )
                    .Accepts(requestType, "application/json")
                    .Produces(200, method.ReturnType);// TypeFactory.GetMethodProxyType(method));
                }
            });
            configure?.Invoke(null);
            return app;
        }

        private static object HandleRequest(
            IServiceProvider serviceProvider,
            IRpcHandler handler,
            IDataContractRequestFormatter formatter,
            JsonElement requestData,
            Type serviceType,
            Type requestType,
            MethodInfo serviceMethod)
        {
  
            var contract = formatter.Map(requestType, requestData);
            //var body = accesor.HttpContext.Request.Body.Length;
            var args = contract.ToArray();
            var result = handler.HandleRequest(serviceType, serviceMethod, args);
            return result;
        }
    }
    public static class Utils
    {


        public static bool IsRpcContact(Type x)
        {
            var att = x.GetCustomAttribute<RpcServiceAttribute>();
            if (att != null)
            {
                return true;
            }
            return false;
        }
    }

}