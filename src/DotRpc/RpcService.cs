
using Castle.DynamicProxy;
using DotRpc.RpcClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;


namespace DotRpc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public class RpcServiceAttribute : Attribute
    {
        public RpcServiceAttribute()
        {
            Name = nameof(IRpcHandler).ToPropertyName();
            Namespace = (Assembly.GetEntryAssembly() ?? typeof(RpcServiceAttribute).Assembly).ToPropertyName();

        }

        public RpcServiceAttribute(string name, string @namespace, Type callBackContract)
        {
            Name = name.ToPropertyName();
            Namespace = @namespace.ToPropertyName();
            CallbackContract = callBackContract;
        }
        public Type? CallbackContract { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
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
            try
            {
                if (args is null || args.Length == 0)
                {
                    var res = Activator.CreateInstance(requestType);
                    return (IDataContract)res;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Type {requestType} does not have a default constructo");
            }
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

        public JsonRequestFormatter(IArgumentMapper argumentMapper, IRpcTypeFactory typeFactory)
        {
            TypeFactory = typeFactory;
            this.argumentMapper = argumentMapper;

        }

        public IRpcTypeFactory TypeFactory { get; }

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
    public interface IRpcEndpointProvider { string Endpoint { get; set; } }
    public class RpcEndpointProvider : IRpcEndpointProvider
    {
        public string Endpoint { get; set; }
        public RpcEndpointProvider() { Endpoint = "https://localhost:5001"; }
        public RpcEndpointProvider(string endpoint) { Endpoint = endpoint; }
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
            object result = null!;
            if (method.IsGenericMethod)
            {
                List<Type> argumentTypes = new();
                var methodParameters = method.GetParameters();
                for (var i = 0; i < methodParameters.Length; i++)
                {
                    if (methodParameters[i].ParameterType.IsGenericParameter)
                    {
                        argumentTypes.Add(args[i].GetType());
                    }
                }

                var argTypes = argumentTypes.ToArray();
                var genericMethod = method.MakeGenericMethod(argTypes);
                result = genericMethod.Invoke(service, args);

            }
            else
            {
                result = method.Invoke(service, args);
            }
            return result;
        }
    }

    public interface IRpcPayloadClient
    {
        public RpcPayload Invoke(string endpoint, MethodInfo method, object[] args);
    }

    public interface IRpcService { }
    public interface IRpcClient<TService> :IRpcService
    {
        TService Service { get; }
    }


    public class RpcClient<TService> : IRpcClient<TService>
        where TService : class
    {
        private readonly IRpcConfigurationProvider rpcConfiguration;

        public TService Service { get; set; }


        public RpcClient(ILoggerFactory loggerFactory, IRpcConfigurationProvider rpcConfiguration, IRpcTypeFactory typeFactory, IRpcEndpointProvider rpcEndpointProvider)
        {
            var gen = new Castle.DynamicProxy.ProxyGenerator();

            IInterceptor interceptor = new RpcClientServiceInterceptor(loggerFactory, rpcConfiguration, typeFactory, rpcEndpointProvider);
            this.Service = gen.CreateInterfaceProxyWithoutTarget<TService>(interceptor);
            this.rpcConfiguration = rpcConfiguration;
        }
        //public RpcClient(IServiceProvider sp)
        //    : this   (
        //          sp.GetRequiredService<ILoggerFactory>(),
        //          sp.GetRequiredService<IRpcConfigurationProvider>(),
        //          sp.GetRequiredService<IRpcTypeFactory>(),
        //          sp.GetRequiredService<IRpcEndpointProvider>()
        //          )
        //{

        //}


        public class RpcClientServiceInterceptor : IInterceptor
        {
            private ILogger<RpcClient<TService>.RpcClientServiceInterceptor> logger;

            public IRpcTypeFactory TypeFactory { get; }
            public IRpcEndpointProvider RpcEndpointProvider { get; }

            private readonly IRpcConfigurationProvider rpcConfiguration;

            public RpcClientServiceInterceptor(
                ILoggerFactory loggerFactory,
                IRpcConfigurationProvider rpcConfiguration,
                IRpcTypeFactory typeFactory,
                IRpcEndpointProvider rpcEndpointProvider)
            {
                logger = loggerFactory.CreateLogger<RpcClientServiceInterceptor>();
                TypeFactory = typeFactory;
                RpcEndpointProvider = rpcEndpointProvider;
                this.rpcConfiguration = rpcConfiguration;
            }
            public void Intercept(IInvocation invocation)
            {
                var value = TypeFactory.GetMethodProxyType(invocation.Method);

                bool isGenericCall = invocation.Method.IsGenericMethod || invocation.Method.DeclaringType.IsGenericType;
                List<Type> genericArgumentTypes = new();

                //Todo implement generic service call.
                if (invocation.Method.IsGenericMethod)
                {
                    var genericArgs = invocation.Method.GetGenericArguments();
                    if (genericArgs != null)
                    {
                        genericArgumentTypes.AddRange(genericArgs);
                    }
                }

                var serviceType = invocation.Method.DeclaringType;
                var formatter = new DataContractRequestFormatter();
                IDataContract contract = null!;

                string[] typeArgumentTypes = null!;
                if (value.IsGenericType)
                {
                    var declaring = invocation.Method.DeclaringType;
                    var typeGenericAgs = declaring.GetGenericArguments();


                    //var genericParameters = declaring.GetGenericParameterConstraints();
                    var allGenericArgs = genericArgumentTypes.Concat(typeGenericAgs).ToArray();
                    var genericContractType = value.MakeGenericType(allGenericArgs.ToArray());
                    typeArgumentTypes = typeGenericAgs.Select(x => x.FullName).ToArray();
                    var genericContractArgs = genericContractType.GetGenericArguments();
                    contract = formatter.Map(genericContractType, invocation.Arguments);
                }
                else
                {
                    contract = formatter.Map(value, invocation.Arguments);
                }

                string?[]? genericParameterTypes = genericArgumentTypes.Select(x => x.FullName).ToArray();




                var config = rpcConfiguration.GetEndpointConfiguration(invocation.Method.DeclaringType, invocation.Method);
                var path = config.RouteEndpoint;
                using var client = new HttpClient();
                //var endpoint = $"https://localhost:63601{path}";
                var endpoint = $"{RpcEndpointProvider.Endpoint}{path}";
                HttpResponseMessage response = null!;
                bool useNewtonSoft = true;
                var request = !isGenericCall ? contract
                    : !value.IsGenericType ? (object)new DotRpcRequest(contract, genericParameterTypes)
                    : (object)new DotRpcGenericServiceRequest(contract, genericParameterTypes, typeArgumentTypes);
                string requestJson = null!;
                if (useNewtonSoft)
                {

                    requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);
                    var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                    response = client.PostAsync(endpoint, content).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                else
                {
                    //      var json = JsonSerializer.Serialize(contract);
                    requestJson = JsonSerializer.Serialize(contract);
                    var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                    response = client.PostAsync(endpoint, content).ConfigureAwait(false).GetAwaiter().GetResult();

                }


                //response.EnsureSuccessStatusCode();

                if (useNewtonSoft)
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();

                        var jsonResult = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        try
                        {
                            var result = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResult, invocation.Method.ReturnType);
                            invocation.ReturnValue = result;
                        }
                        catch (Exception ex)
                        {

                            var error = Newtonsoft.Json.JsonConvert.DeserializeObject<DotRpcErrorPayload>(jsonResult);
                            if (error is not null)
                            {
                                var dotRpcError = new DotRpcError(error);
                                logger.LogError(ex, $"Error converting jsonResult to return type '{config.ReturnType}' - json {jsonResult} - {dotRpcError}");
                                throw dotRpcError;
                            }
                            else
                                throw;

                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Error executing RPC call '{endpoint}' with request json {requestJson} - {ex}");
                        throw;
                    }

                }
                else
                {
                    var result = response.Content.ReadFromJsonAsync(config.ReturnType).ConfigureAwait(false).GetAwaiter().GetResult();
                    invocation.ReturnValue = result;
                }


                //invocation.Proceed();



            }
        }
    }

    public class DotRpcErrorPayload
    {
        public DotRpcErrorPayload() { }

        public string Message { get; set; }
        public string TypeName { get; set; }
        public string StackTrace { get; set; }
        public string Details { get; set; }
        public string Source { get; set; }
        public DotRpcErrorPayload InnerException { get; set; }
    }
    public class DotRpcError : Exception
    {
        public DotRpcError() { }
        public DotRpcError(Exception ex)
        {
            this.Message = ex.Message;
            this.TypeName = ex.GetType().FullName;
            this.StackTrace = ex.StackTrace.ToString();
            this.Details = ex.ToString();
            base.Source = ex.Source;
            if (ex.InnerException != null)
            {
                var inner = new DotRpcError(ex.InnerException);
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(inner);
                this.InnerException = Newtonsoft.Json.JsonConvert.DeserializeObject<DotRpcErrorPayload>(json);
            };
        }

        public DotRpcError(DotRpcErrorPayload error)
            : base(error.Message, error.InnerException is null ? null : new DotRpcError(error.InnerException))
        {
            this.Message = error.Message;
            this.TypeName = error.TypeName;
            this.StackTrace = error.StackTrace;
            this.Details = error.Details;
            InnerException = error.InnerException;
        }

        public string Message { get; set; }
        public string TypeName { get; set; }
        public new string StackTrace { get; set; }
        public string Details { get; set; }
        public new DotRpcErrorPayload? InnerException { get; set; }
    }
    public class DotRpcRequest
    {
        public DotRpcRequest() { }
        public DotRpcRequest(IDataContract contract,
            string[] methodArgumentTypes)
        {
            Contract = contract;
            MethodArgumentTypes = methodArgumentTypes; ;
        }


        public virtual object Contract { get; set; }

        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[] MethodArgumentTypes { get; set; }

        public object?[] ToArray()
        {
            return MethodArgumentTypes.Select(x => (object)x).ToArray();
        }
    }

    public class DotRpcGenericServiceRequest : DotRpcRequest
    {
        public DotRpcGenericServiceRequest() { }
        public DotRpcGenericServiceRequest(IDataContract contract,
            string[] methodArgumentTypes,
            string[] typeArgumentsTypes)
            : base(contract, methodArgumentTypes)
        {
            this.TypeArgumentsTypes = typeArgumentsTypes;
            //Contract = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(contract));// Newtonsoft.Json.JsonConvert.SerializeObject(contract);

        }


        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[] TypeArgumentsTypes { get; set; }
    }

    public class DotRpcRequest<T> : DotRpcRequest
    {

        public T Contract { get; set; }
        public DotRpcRequest()
        {

        }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? MethodArgumentTypes { get; set; }


    }

    public class DotRpcGenericRequest<T> : DotRpcRequest
    {

        public DotRpcGenericRequest()
        { }


        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? TypeArgumentsTypes { get; set; }
    }


    public class GenericArgumentTypes : IDataContract
    {
        public GenericArgumentTypes() { }
        public GenericArgumentTypes(object[]? argumentTypes)
            : this(argumentTypes?.Select(x => x?.ToString()).ToArray())
        {

        }
        public string[] ArgumentTypes { get; set; }

        public GenericArgumentTypes(string[] argumentTypes)
        {
            ArgumentTypes = argumentTypes;
        }

        public object?[] ToArray()
        {
            return ArgumentTypes.Select(x => (object)x).ToArray();
        }
    }
    public class GenericArgumentTypes1 : IDataContract//, IEnumerable<string>
    {
        public GenericArgumentTypes1() { }
        public GenericArgumentTypes1(object[]? argumentTypes)
            : this(argumentTypes?.Select(x => x?.ToString()).ToArray())
        {

        }
        public GenericArgumentTypes1(string?[]? argumentTypes)
        {
            if (argumentTypes is null || argumentTypes.Any(x => string.IsNullOrEmpty(x)))
            {
                throw new ArgumentNullException(nameof(argumentTypes), $"Argument Types cannot be null and must contain nonempty string values");
            }
            if (argumentTypes != null && argumentTypes.Length > 0)
            {
                Arg0 = argumentTypes.Length > 0 ? argumentTypes[0] : null;
                Arg1 = argumentTypes.Length > 1 ? argumentTypes[1] : null;
                Arg2 = argumentTypes.Length > 2 ? argumentTypes[2] : null;
                Arg3 = argumentTypes.Length > 3 ? argumentTypes[3] : null;
                Arg4 = argumentTypes.Length > 4 ? argumentTypes[4] : null;
                Arg5 = argumentTypes.Length > 5 ? argumentTypes[5] : null;
                Arg6 = argumentTypes.Length > 6 ? argumentTypes[6] : null;
                Arg7 = argumentTypes.Length > 7 ? argumentTypes[7] : null;
                Arg8 = argumentTypes.Length > 8 ? argumentTypes[8] : null;
                Arg9 = argumentTypes.Length > 9 ? argumentTypes[9] : null;
                Arg10 = argumentTypes.Length > 10 ? argumentTypes[10] : null;
                Arg11 = argumentTypes.Length > 11 ? argumentTypes[11] : null;
                Arg12 = argumentTypes.Length > 12 ? argumentTypes[12] : null;
                Arg13 = argumentTypes.Length > 13 ? argumentTypes[13] : null;
                Arg14 = argumentTypes.Length > 14 ? argumentTypes[14] : null;
                Arg15 = argumentTypes.Length > 15 ? argumentTypes[15] : null;
            }
        }

        public object[] ToArray()
        {
            return ArgumentsArray().Select(x => (object)x).ToArray();
        }

        public List<string> ArgumentsArray()
        {
            return (new[] { Arg0, Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8, Arg9, Arg10, Arg11, Arg12, Arg13, Arg14, Arg15 })
                .TakeWhile(x => !string.IsNullOrEmpty(x)).Select(x => x ?? "").ToList();
        }


        public string? Arg0 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg1 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg2 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg3 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg4 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg5 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg6 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg7 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg8 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg9 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg10 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg11 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg12 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg13 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg14 { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Arg15 { get; set; }

    }
}