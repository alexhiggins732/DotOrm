
using Castle.DynamicProxy;
using DotRpc.RpcClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

using System.ComponentModel.Design.Serialization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;


namespace DotRpc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public class RpcServiceAttribute : Attribute
    {
        public RpcServiceAttribute() { }
        public Type CallbackContract { get; set; } = null!;
        public string Name { get; set; } = NameService.ToProperyName(nameof(IRpcHandler));
        public string Namespace { get; set; } = NameService.ToProperyName(Path.GetFileNameWithoutExtension((Assembly.GetEntryAssembly() ?? typeof(RpcServiceAttribute).Assembly).ManifestModule.Name));
    }

    public class RpcPayload
    {

        public byte[]? Payload { get; set; }

    }
    public interface IPayloadFormatter
    {
        T? To<T>(RpcPayload payload);
        object? To(Type type, RpcPayload payload);
    }
    public class JsonPayloadFormatter : IPayloadFormatter
    {
        public T? To<T>(RpcPayload payload)
            => (T?)To(typeof(T), payload);

        public object? To(Type type, RpcPayload payload)
        {
            if (payload.Payload == null) return null;
            var json = Encoding.UTF8.GetString(payload.Payload);
            return JsonSerializer.Deserialize(json, type);
        }
    }
    public static class RpcServiceProvider
    {
        public static IServiceCollection MapRpcService(
            this IServiceCollection services,
            Action<IServiceCollection>? preConfigure = null,
            Action<IServiceCollection>? postConfigure = null
            )
        {
            preConfigure?.Invoke(services);



            services.AddSingleton<IMethodInvoker, MethodInvoker>();
            services.AddSingleton<IRpcMapper, RpcMapper>();
            services.AddSingleton<IRpcHandler, RpcHandler>();

            services.AddSingleton<IJsonRequestFormatter, JsonRequestFormatter>();
            services.AddSingleton<IResponseFormatter, JsonResponseFormatter>();
            services.AddSingleton<IArgumentMapper, ArgumentMapper>();
            postConfigure?.Invoke(services);
            return services;
        }

    }
    public interface IArgumentMapper
    {
        public Dictionary<string, object?> Map(object? value, MethodInfo methodInfo);
    }
    public class ArgumentMapper : IArgumentMapper
    {
        public Dictionary<string, object?> Map(object? value, MethodInfo methodInfo)
        {
            var methodParams = methodInfo.GetParameters().Where(x => !string.IsNullOrEmpty(x.Name))
                .ToDictionary(x => GetParameterName(x), x => x);

            var result = new Dictionary<string, object?>();
            if (value != null)
            {
                var props = value.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
                foreach (var kvp in methodParams)
                {
                    object? argValue = null;
                    if (props.ContainsKey(kvp.Key))
                    {
                        argValue = props[kvp.Key].GetValue(value);
                    }
                    result.Add(kvp.Key, argValue);
                }
            }
            return result;
        }

        private string GetParameterName(ParameterInfo x)
        {
            if (x.Name is null)
                throw new ArgumentNullException("Parameter name can not be null");
            return x.Name;
        }
    }

    public interface IDataContractRequestFormatter
    {
        IDataContract Map(Type requestType, object[] args);
        IDataContract Map(Type requestType, JsonElement requestElement);
    }
    public class DataContractRequestFormatter : IDataContractRequestFormatter
    {
        public DataContractRequestFormatter()
        {

        }
        public IDataContract Map(Type requestType, object[] args)
        {
            var result = Activator.CreateInstance(requestType, args);
            if (result is null)
                throw new Exception($"Failed to create instance of {requestType.Name} from args: {string.Join(", ", args)} ");

            return (IDataContract)result;
        }
        public IDataContract Map(Type requestType, JsonElement requestElement)
        {
            var result = requestElement.Deserialize(requestType);// JsonSerializer.Deserialize(requestElement, requestType);
            if (result is null)
                throw new Exception($"Failed to create instance of {requestType.Name} from args: {requestElement.GetRawText()} ");

            return (IDataContract)result;
        }
    }

    public class JsonRequestFormatter : IJsonRequestFormatter
    {
        private readonly IArgumentMapper argumentMapper;

        public JsonRequestFormatter(IArgumentMapper argumentMapper)
        {
            this.argumentMapper = argumentMapper;
        }
        public MethodArgs Map(RpcPayload request, MethodInfo methodInfo)
        {
            var json = request.Payload is null ? string.Empty : Encoding.UTF8.GetString(request.Payload);
            var proxyType = TypeFactory.GetMethodProxyType(methodInfo);
            object? argumentProxy = JsonSerializer.Deserialize(json, proxyType);
            var args = argumentMapper.Map(argumentProxy, methodInfo);
            var result = new MethodArgs
            {
                Method = methodInfo,
                MethodArguments = args
            };
            return result;

        }
    }

    public class JsonResponseFormatter : IResponseFormatter
    {
        public RpcPayload Map(MethodResult result)
        {
            var json = JsonSerializer.Serialize(result.Result);
            var bytes = Encoding.UTF8.GetBytes(json);
            return new RpcPayload { Payload = bytes };
        }


    }

    public interface IMethodInvoker
    {
        RpcPayload GetResponse(MethodArgs args);
    }
    public class MethodInvoker : IMethodInvoker
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IRpcMapper mapper;
        public MethodInvoker(IServiceProvider serviceProvider, IRpcMapper mapper)
        {
            this.serviceProvider = serviceProvider;
            this.mapper = mapper;
        }

        public RpcPayload GetResponse(MethodArgs args)
        {
            var service = serviceProvider.GetRequiredService(args.Method.DeclaringType);
            var methodArgs = args.MethodArguments.Values.ToArray();
            var result = args.Method.Invoke(service, methodArgs);
            var methodResult = new MethodResult(args, result);
            var response = mapper.MapResponse(methodResult);
            return response;

        }
    }

    public class MethodArgs
    {
        public MethodInfo Method;
        public Dictionary<string, object?> MethodArguments;

    }
    public class MethodResult
    {
        public object Result;
        private MethodArgs args;

        public MethodResult(MethodArgs args, object? result)
        {
            this.args = args;
            Result = result;
        }
    }

    public interface IJsonRequestFormatter
    {
        public MethodArgs Map(RpcPayload request, MethodInfo methodInfo);
    }

    public interface IDataContract
    {
        object?[] ToArray();
    }
    public interface IResponseFormatter
    {
        public RpcPayload Map(MethodResult result);
    }

    public interface IRpcMapper
    {
        RpcPayload MapResponse(MethodResult methodResult);
    }

    public class RpcMapper : IRpcMapper
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IJsonRequestFormatter requestFormatter;
        private readonly IResponseFormatter responseFormatter;
        public RpcMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.requestFormatter = serviceProvider.GetRequiredService<IJsonRequestFormatter>();
            this.responseFormatter = serviceProvider.GetRequiredService<IResponseFormatter>();
        }

        public RpcPayload MapResponse(MethodResult methodResult)
        {
            var payload = responseFormatter.Map(methodResult);
            return payload;
        }

        //internal static MethodInvoker MapRequestPayload<T1, T2>(IServiceProvider sp, RpcPayload request, MethodInfo method)
        //{
        //    var mapper = sp.GetRequiredService<RpcMapper>();
        //    return mapper.MapInvoker(sp, request, method);
        //    //return new MethodInvoker(sp, request, method);
        //}



        //public T2 MapResponse<T1, T2>(T1 result)
        //{
        //    var formatted = responseFormatter.Map(result);
        //    throw new NotImplementedException();
        //}
    }


    public interface IRpcHandler
    {
        RpcPayload HandleRequest(MethodInfo method, RpcPayload request);
        object HandleRequest(Type serviceType, MethodInfo method, IDataContract request);
        object HandleRequest(Type serviceType, MethodInfo method, object?[] args);
    }
    public class RpcHandler : IRpcHandler
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IJsonRequestFormatter requestFormatter;
        private readonly IMethodInvoker methodInvoker;

        public RpcHandler(IServiceProvider serviceProvider, IJsonRequestFormatter requestFormatter, IMethodInvoker methodInvoker)
        {
            this.serviceProvider = serviceProvider;
            this.requestFormatter = requestFormatter;
            this.methodInvoker = methodInvoker;
        }
        public RpcPayload? HandleRequest(MethodInfo handlerMethod, RpcPayload request)
        {
            var args = requestFormatter.Map(request, handlerMethod);
            var response = methodInvoker.GetResponse(args);
            return response;

        }

        public object HandleRequest(Type ServiceType, MethodInfo method, IDataContract request)
        {
            var service = serviceProvider.GetService(ServiceType);
            var args = request.GetType().GetProperties().Select(x => x.GetValue(x)).ToArray();
            var result = method.Invoke(service, args);
            return result;
        }

        public object HandleRequest(Type ServiceType, MethodInfo method, object?[]? args)
        {
            var service = serviceProvider.GetService(ServiceType);
            var result = method.Invoke(service, args);
            return result;
        }
    }

    public interface IRpcPayloadClient
    {
        public RpcPayload Invoke(string endpoint, MethodInfo method, object[] args);
    }

    public interface IRpcClient<TService>
    {
        TService Service { get; }
    }

    public class RpcClient<TService> : IRpcClient<TService>
        where TService : class
    {
        private readonly IRpcConfigurationProvider rpcConfiguration;

        public TService Service { get; set; }


        public RpcClient(IRpcConfigurationProvider rpcConfiguration)
        {
            var gen = new Castle.DynamicProxy.ProxyGenerator();

            IInterceptor interceptor = new RpcClientServiceInterceptor(rpcConfiguration);
            this.Service = gen.CreateInterfaceProxyWithoutTarget<TService>(interceptor);
            this.rpcConfiguration = rpcConfiguration;
        }

        public class RpcClientServiceInterceptor : IInterceptor
        {
            private readonly IRpcConfigurationProvider rpcConfiguration;

            public RpcClientServiceInterceptor(IRpcConfigurationProvider rpcConfiguration)
            {
                this.rpcConfiguration = rpcConfiguration;
            }
            public void Intercept(IInvocation invocation)
            {
                var value = DotRpc.TypeFactory.GetMethodProxyType(invocation.Method);

                var formatter = new DataContractRequestFormatter();
                var contract = formatter.Map(value, invocation.Arguments);
          

                var config = rpcConfiguration.GetEndpointConfiguration(invocation.Method.DeclaringType, invocation.Method);
                var path = config.RouteEndpoint;
                using var client = new HttpClient();
                var endpoint = $"https://localhost:63601{path}";

                HttpResponseMessage response = null!;
                bool useNewtonSoft = true;
                if (useNewtonSoft)
                {
                    var nsJson = Newtonsoft.Json.JsonConvert.SerializeObject(contract);
                    var content = new StringContent(nsJson, Encoding.UTF8, "application/json");
                    response = client.PostAsync(endpoint, content).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                else
                {
                    //      var json = JsonSerializer.Serialize(contract);
                    response = client.PostAsJsonAsync(endpoint, contract).ConfigureAwait(false).GetAwaiter().GetResult();

                }


                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadFromJsonAsync(config.ReturnType).ConfigureAwait(false).GetAwaiter().GetResult();
                invocation.ReturnValue = result;
                //invocation.Proceed();



            }
        }
    }
}