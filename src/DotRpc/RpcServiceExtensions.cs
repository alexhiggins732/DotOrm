using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.ServiceModel;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.ComponentModel.Design;
using DotRpc.RpcClient;
using System.Reflection.Metadata;

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
                var contracts = assembly.GetTypes().Where(x => IsRpcContract(x)).ToList();
                Dictionary<Type, Dictionary<MethodInfo, RpcEndpointConfiguration>> rpcServices = new();
                var clientServiceType = typeof(IRpcClient<>);
                var clientImplementationType = typeof(RpcClient<>);

                services.AddSingleton(clientServiceType, clientImplementationType);
                var sp = services.BuildServiceProvider();
                var TypeFactory = sp.GetService<IRpcTypeFactory>();
                contracts.ForEach(contractType =>
                {

                    var att = contractType.GetRpcContract();// contractType.GetCustomAttribute<RpcServiceAttribute>();
                    var area = $"/{att.Name}/";
                    Dictionary<MethodInfo, RpcEndpointConfiguration> serviceCalls = new();
                    var contractMethods = GetAllMethodsFromInterface(contractType);
                    foreach (var method in contractMethods)
                    {

                        var requestType = TypeFactory.GetMethodProxyType(method);
                        var path = method.GetApiPathName();
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
                    //services.AddSingleton(serviceType, implementationType);
                    // services.AddScoped(serviceType, sp=> GetImplemenation(sp, contractType) );
                });

                services.AddSingleton<IRpcConfigurationProvider>(sp => new RpcConfigurationProvider(rpcServices));



                configure?.Invoke(services);
                return services;
            }

            private static object GetImplemenation(IServiceProvider sp, Type implementationType)
            {
                if (!implementationType.ContainsGenericParameters)
                {
                    var t = typeof(RpcClient<>).MakeGenericType(implementationType);
                    var instance = Activator.CreateInstance(t, new object[] { sp });
                    if (instance == null)
                        throw new Exception("Error creating service with default servicep provider constructor");
                    return instance;
                }
                else
                {
                    return null;
                }
            }

            public static MethodInfo[] GetAllMethodsFromInterface(this Type interfaceType)
            {
                var methods = interfaceType.GetMethods();

                var implementedInterfaces = interfaceType.GetInterfaces();

                foreach (var implementedInterface in implementedInterfaces)
                {
                    var interfaceMethods = GetAllMethodsFromInterface(implementedInterface);
                    methods = methods.Concat(interfaceMethods).ToArray();
                }

                return methods;
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
                    else// if (serviceMethod.IsGenericMethod)
                    {
                        var methodArgs = serviceMethod.GetParameters();
                        foreach (var kvp in contract)
                        {
                            if (kvp.Key.Name == serviceMethod.Name)
                            {
                                if (kvp.Key == serviceMethod)
                                {
                                    return kvp.Value;
                                }
                                var valueArgs = kvp.Key.GetParameters();
                                //TODO: match generic parameter types to method.
                                if (valueArgs.Length == methodArgs.Length)
                                {
                                    return kvp.Value;
                                }
                                //if (kvp.Key..)
                            }
                           
                        }
                    }
                }
                else
                {
                    foreach (var service in rpcServices)
                    {
                        if (serviceType.IsAssignableFrom(service.Key))
                        {
                            //todo cache:
                            rpcServices.Add(serviceType, service.Value);
                            var contract = service.Value;
                            if (contract.ContainsKey(serviceMethod))
                            {
                                return contract[serviceMethod];
                            }
                            else //if (serviceMethod.IsGenericMethod)
                            {
                                foreach (var kvp in contract)
                                {
                                    if (kvp.Key.Name == serviceMethod.Name)
                                        return kvp.Value;
                                }
                            }
                        }
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
            services.AddLogging();
            services.AddSingleton<IRpcTypeFactory, RpcTypeFactory>();
            services.AddSingleton<IRpcProxyGenerator, RpcProxyGenerator>();
            services.AddSingleton<IResponseFormatter, JsonResponseFormatter>();
            services.AddSingleton<IRpcMapper, RpcMapper>();
            services.AddSingleton<IArgumentMapper, ArgumentMapper>();
            services.AddSingleton<IJsonRequestFormatter, JsonRequestFormatter>();
            services.AddSingleton<IMethodInvoker, MethodInvoker>();
            services.AddSingleton<IRpcHandler, RpcHandler>();
            services.AddSingleton<IDataContractRequestFormatter, DataContractRequestFormatter>();

            //services.AddApiVersioning(options =>
            //{
            //    options.DefaultApiVersion = new ApiVersion(1, 0); // Set your default API version
            //    options.ReportApiVersions = true;
            //    options.AssumeDefaultVersionWhenUnspecified = true;
            //});

            //services.AddVersionedApiExplorer(options =>
            //{
            //    //options.GroupNameFormat = "'v'VVV"; // Use a version prefix for group names
            //});

            return services;
        }

        public static IServiceCollection AddDotRpcSwagger(
           this IServiceCollection services,
           Action<IServiceCollection>? configure = null)
        {
            services.AddHttpContextAccessor();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SchemaGeneratorOptions.SchemaIdSelector = x => x.DotRpcSwaggerSchemaIdGenerator();

            });
            configure?.Invoke(services);
            return services;
        }
        public static WebApplication UseDotRpcSwagger(
           this WebApplication app,
           Action<WebApplication>? configure = null)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            configure?.Invoke(app);
            return app;
        }

        public static WebApplication UseDotRpcFromAssembly(
        this WebApplication app,
        Assembly assembly,
        Action<WebApplication>? configure = null)
        {
            var contracts = assembly.GetTypes().Where(x => IsRpcContract(x)).ToList();
            var sp = app.Services;
            var TypeFactory = sp.GetService<IRpcTypeFactory>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            contracts.ForEach(serviceType =>
            {
                var att = serviceType.GetRpcContract();


                var area = $"/{att.Name}/";

                foreach (var method in serviceType.GetAllMethodsFromInterface())
                {
                    var requestType = TypeFactory.GetMethodProxyType(method);
                    var path = method.GetApiPathName();
                    var routeEndpoint = $"{area}{path}";
                    var implementedRequest = requestType;
                    if (requestType.IsGenericType)
                    {
                        var args = requestType.GetGenericArguments();
                        if (!serviceType.IsGenericType && !method.IsGenericMethod)
                            implementedRequest = typeof(DotRpcRequest<>).MakeGenericType(new Type[] { requestType });
                        else
                        {
                            var typeArgs = new List<Type>();
                            if (method.IsGenericMethod)
                                typeArgs.AddRange(method.GetGenericArguments().Select(x => typeof(string)).ToArray());
                            if (serviceType.IsGenericType)
                                typeArgs.AddRange(serviceType.GetGenericArguments().Select(x => typeof(string)).ToArray());
                            var instance = requestType.MakeGenericType(typeArgs.ToArray());
                            var implementementedRequest = RpcProxyGenerator.EmitExampleProxy(loggerFactory, requestType);
                            if (!serviceType.IsGenericType)
                                implementedRequest = typeof(DotRpcRequest<>).MakeGenericType(new Type[] { implementementedRequest });
                            else
                                implementedRequest = typeof(DotRpcGenericRequest<>).MakeGenericType(new Type[] { implementementedRequest });
                        }

                    }
                    var builder = app.MapPost(routeEndpoint,

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

                        //.ExcludeFromDescription()
                        .WithTags(att.Name)
                        .Accepts(implementedRequest, "application/json")
                        .Produces(200, GetReturnType(method.ReturnType));
                    if (method.IsGenericMethod)
                    {
                        //todo: provide custom example
                    }

                }



            });
            configure?.Invoke(app);
            return app;
        }

        private static Type? GetReturnType(Type returnType)
        {
            if (!returnType.IsGenericType)
            {
                return returnType;
            }

            // Handle Nullable<T>
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return GetReturnType(returnType.GetGenericArguments()[0]);
            }

            // Handle Task<T>
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return GetReturnType(returnType.GetGenericArguments()[0]);
            }

            // Handle ValueTask<T>
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                return GetReturnType(returnType.GetGenericArguments()[0]);
            }

            // Handle ActionResult<T>
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
            {
                return GetReturnType(returnType.GetGenericArguments()[0]);
            }


            // Handle other cases
            var genericArguments = returnType.GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                genericArguments[i] = GetReturnType(genericArguments[i]);
            }

            return returnType.GetGenericTypeDefinition().MakeGenericType(genericArguments);

            return returnType;
        }

        public static RpcServiceAttribute GetRpcContract(this Type serviceType, bool optional = false)
        {
            var result = serviceType.GetCustomAttribute<RpcServiceAttribute>();
            if (result is null)
            {

                var serviceContract = serviceType.GetCustomAttribute<ServiceContractAttribute>();
                if (serviceContract != null)
                {
                    result = new RpcServiceAttribute(
                        serviceContract.Name ?? serviceType.Name,
                        serviceType.Assembly.ToPropertyName(),
                        serviceType);
                }

            }
            if (result is not null)
            {
                result.CallbackContract = serviceType;
                if (result.Name == nameof(IRpcHandler).ToPropertyName())
                {
                    result.Name = (serviceType.Name).ToPropertyName();
                }
                if (result.Name.Length > 1 && result.Name.StartsWith("I") && char.IsUpper(result.Name[1]))
                {
                    result.Name = result.Name.Substring(1);
                }
            }
            else if (!optional)
            {
                throw new Exception($"{serviceType.Name} is not an RpcService");
            }


            return result;
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
            if (serviceMethod.IsGenericMethod || requestType.IsGenericType || serviceType.IsGenericType)
                return HandleGenericRequest(serviceProvider, handler, formatter, requestData, serviceType, requestType, serviceMethod);
            try
            {
                //var contractElement= requestData.EnumerateObject().Last().Value; 
                var contract = JsonConvert.DeserializeObject(requestData.GetRawText(), requestType);
                //var body = accesor.HttpContext.Request.Body.Length;
                var args = ((IDataContract)contract).ToArray();
                var result = handler.HandleRequest(serviceType, serviceMethod, args);
                var resultJson = JsonConvert.SerializeObject(result);
                return resultJson;
            }
            catch (Exception ex)
            {
                return new DotRpcError(ex);
            }
        }
        private static object HandleGenericRequest(
            IServiceProvider serviceProvider,
            IRpcHandler handler,
            IDataContractRequestFormatter formatter,
            JsonElement requestData,
            Type serviceType,
            Type requestType,
            MethodInfo serviceMethod)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<DotRpcGenericServiceRequest>(requestData.GetRawText());
                var request1 = requestData.Deserialize<DotRpcGenericServiceRequest>();
                if (request is null)
                {
                    throw new Exception($"Failed to deserialize DotRpcRequest: {requestData.GetRawText()}");
                }


                //todo: correctly handle generic/non-generic method and generic/non-generic type.
                var genericArguments = new List<Type>();
                if (request.MethodArgumentTypes != null)
                {
                    foreach (var genericParameter in request.MethodArgumentTypes)
                    {
                        var t = Type.GetType(TypeNameService.GetFullTypeName(genericParameter));
                        if (t is null)
                        {
                            throw new Exception($"Failed to load method argument type: {genericParameter}");
                        }
                        genericArguments.Add(t);
                    }
                }


                var genericTypeArguments = new List<Type>();
                if (request is DotRpcGenericServiceRequest genericTypeRequest)
                {
                    if (genericTypeRequest.TypeArgumentsTypes != null)
                    {
                        foreach (var genericParameter in genericTypeRequest.TypeArgumentsTypes)
                        {
                            var t = Type.GetType(genericParameter)
                                ?? serviceType.Assembly.GetType(genericParameter)
                                ?? serviceMethod.DeclaringType.Assembly.GetType(genericParameter)
                                ?? requestType.Assembly.GetType(genericParameter);
                            if (t is null)
                            {
                                throw new Exception($"Failed to load method argument type: {genericParameter}");
                            }
                            genericTypeArguments.Add(t);
                        }
                    }
                }

                object? service = null!;

                if (serviceType.IsGenericType)
                    serviceType = serviceType.MakeGenericType(genericTypeArguments.ToArray());

                service = serviceProvider.GetService(serviceType);
                if (service is null)
                    throw new Exception($"Failed to resolve service for service type: {serviceType.ToCSharpDeclaration()}");

                var genericMethod = serviceMethod;
                if (serviceMethod.IsGenericMethod)
                    genericMethod = serviceMethod.MakeGenericMethod(genericArguments.ToArray());

                var TypeFactory = serviceProvider.GetService<IRpcTypeFactory>();

                var proxyType = TypeFactory.GetMethodProxyType(genericMethod);
                var genericProxyType = proxyType;
                if (proxyType.IsGenericType)
                    genericProxyType = proxyType.MakeGenericType(genericArguments.Concat(genericTypeArguments).ToArray());
                var proxyInstance = Activator.CreateInstance(genericProxyType);
                var contract = JsonConvert.DeserializeObject(request.Contract.ToString(), genericProxyType);
                //var contract = request.Contract.Deserialize(genericProxyType);
                var dataContract = (IDataContract)contract;
                var genericArgs = dataContract.ToArray();

                var genericResult = genericMethod.Invoke(service, genericArgs);
                var genericResultJson = JsonConvert.SerializeObject(genericResult);
                return genericResultJson;
            }
            catch (Exception ex)
            {
                return new DotRpcError(ex);
            }





        }
    }
    public static class Utils
    {


        public static bool IsRpcContract(Type x)
        {
            var att = x.GetCustomAttribute<RpcServiceAttribute>();
            if (att != null)
            {
                return true;
            }
            var serviceContract = x.GetCustomAttribute<ServiceContractAttribute>();
            if (serviceContract != null)
            {
                return true;
            }
            return false;
        }
    }

}