using Castle.DynamicProxy;
using DotOrmLib.GrpcModels.Scalars;
using DotOrmLib.Proxy.Scc1.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
using YamlDotNet.Core.Tokens;


namespace DotOrmLibTests
{
    namespace MappedDynamicProxies
    {
        public interface IApiContract
        {
            object?[]? ToArray();
            IApiContract FromArray(object[]? array);

        }
        [DataContract]
        public class ApiResponse<T> : IApiContract
        {
            [DataMember(Order = 0)]
            public bool Success { get; set; }
            [DataMember(Order = 1)]
            public List<string>? Errors { get; set; }
            [DataMember(Order = 2)]
            public T? Result { get; set; }

            public IApiContract FromArray(object[]? array)
            {
                return new ApiResponse<T>() { Success = (bool)array[0], Errors = (List<string>?)array[1], Result = (T?)array[2] };
            }
            public object[]? ToArray()
            {
                return new object[] { Success, Errors, Result };
            }
        }


        public class ApiRequest
        {
            public static Type GetApiRequestType(Type[] argTypes)
            {
                //TODO: Cache the types for efficiency
                Type genericType = null!;
                if (argTypes.Length == 1)
                    genericType = typeof(ApiRequest<>).MakeGenericType(argTypes);
                else if (argTypes.Length == 2)
                    genericType = typeof(ApiRequest<,>).MakeGenericType(argTypes);
                else if (argTypes.Length == 3)
                    genericType = typeof(ApiRequest<,,>).MakeGenericType(argTypes);
                else if (argTypes.Length == 4)
                    genericType = typeof(ApiRequest<,,,>).MakeGenericType(argTypes);
                else if (argTypes.Length == 5)
                    genericType = typeof(ApiRequest<,,,,>).MakeGenericType(argTypes);
                else
                    throw new NotImplementedException($"ApiRequest is not implement for the {argTypes.Length} generic arguments");
                return genericType;
            }
            public static IApiContract Create(Type[] argTypes, object[] arguments)
            {
                var type = GetApiRequestType(argTypes);
                var instance = Activator.CreateInstance(type);
                var contract = (IApiContract)instance;
                var result = contract.FromArray(arguments);
                return result;
            }
        }

        [DataContract]
        public class ApiRequest<T0> : IApiContract
        {
            [DataMember(Order = 0)] public T0? Arg0 { get; set; }

            public IApiContract FromArray(object[]? array)
            {
                return new ApiRequest<T0>() { Arg0 = (T0?)array[0] };
            }
            public object[]? ToArray()
            {
                return new object[] { Arg0, };
            }
        }

        [DataContract]
        public class ApiRequest<T0, T1> : IApiContract
        {
            [DataMember(Order = 0)] public T0? Arg0 { get; set; }
            [DataMember(Order = 1)] public T1? Arg1 { get; set; }

            public IApiContract FromArray(object[]? array)
            {
                return new ApiRequest<T0, T1>() { Arg0 = (T0?)array[0], Arg1 = (T1?)array[1] };
            }
            public object[]? ToArray()
            {
                return new object[] { Arg0, Arg1 };
            }
        }
        [DataContract]
        public class ApiRequest<T0, T1, T2> : IApiContract
        {
            [DataMember(Order = 0)] public T0? Arg0 { get; set; }
            [DataMember(Order = 1)] public T1? Arg1 { get; set; }
            [DataMember(Order = 2)] public T2? Arg2 { get; set; }

            public IApiContract FromArray(object[]? array)
            {
                return new ApiRequest<T0, T1, T2>() { Arg0 = (T0?)array[0], Arg1 = (T1?)array[1], Arg2 = (T2?)array[2] };
            }
            public object[]? ToArray()
            {
                return new object[] { Arg0, Arg1, Arg2 };
            }
        }

        [DataContract]
        public class ApiRequest<T0, T1, T2, T3> : IApiContract
        {
            [DataMember(Order = 0)] public T0? Arg0 { get; set; }
            [DataMember(Order = 1)] public T1? Arg1 { get; set; }
            [DataMember(Order = 2)] public T2? Arg2 { get; set; }
            [DataMember(Order = 3)] public T3? Arg3 { get; set; }

            public IApiContract FromArray(object[]? array)
            {
                return new ApiRequest<T0, T1, T2, T3>() { Arg0 = (T0?)array[0], Arg1 = (T1?)array[1], Arg2 = (T2?)array[2], Arg3 = (T3?)array[3] };
            }
            public object[]? ToArray()
            {
                return new object[] { Arg0, Arg1, Arg2, Arg3 };
            }
        }

        [DataContract]
        public class ApiRequest<T0, T1, T2, T3, T4> : IApiContract
        {
            [DataMember(Order = 0)] public T0? Arg0 { get; set; }
            [DataMember(Order = 1)] public T1? Arg1 { get; set; }
            [DataMember(Order = 2)] public T2? Arg2 { get; set; }
            [DataMember(Order = 3)] public T3? Arg3 { get; set; }
            [DataMember(Order = 4)] public T4? Arg4 { get; set; }
            public IApiContract FromArray(object[]? array)
            {
                return new ApiRequest<T0, T1, T2, T3, T4>()
                {
                    Arg0 = (T0?)array[0],
                    Arg1 = (T1?)array[1],
                    Arg2 = (T2?)array[2],
                    Arg3 = (T3?)array[3],
                    Arg4 = (T4?)array[4]
                };
            }
            public object[]? ToArray()
            {
                return new object[] { Arg0, Arg1, Arg2, Arg3, Arg4 };
            }
        }


        public interface IMyService { int Add(int a, int b); }
        public interface IMyServiceProxy { ApiResponse<int> Add(ApiRequest<int, int> request); }
        public interface IMyServiceProxyRequestMapper { ApiRequest<int, int> Add(int a, int b); }
        public interface IMyServiceProxyResponseMapper { int Add(ApiResponse<int> response); }
        public interface IMyServiceProxyServiceResponseMapper { ApiResponse<int> Add(int reponse); }

        namespace Proxy
        {
            public class MyServiceProxyRequestMapper
            {
                IMyServiceProxyRequestMapper requestMapperProxy;
                public MyServiceProxyRequestMapper()
                {
                    var gen = new Castle.DynamicProxy.ProxyGenerator();

                    IInterceptor interceptor = new MyServiceProxyRequestMapperInterceptor();
                    this.requestMapperProxy = gen.CreateInterfaceProxyWithoutTarget<IMyServiceProxyRequestMapper>(interceptor);

                }

            }
            class MyServiceProxyRequestMapperInterceptor : IInterceptor
            {
                public void Intercept(IInvocation invocation)
                {
                    var args = invocation.Method.GetParameters();
                    var argTypes = args.Select(x => x.ParameterType).ToArray();
                    invocation.ReturnValue = ApiRequest.Create(argTypes, invocation.Arguments);
                }
            }

        }
        namespace Compiled
        {
            public class MyServiceClient : IMyService
            {
                IMyServiceProxy serviceProxy;
                IMyServiceProxyRequestMapper requestMapper;
                IMyServiceProxyResponseMapper responseMapper;

                public MyServiceClient(IMyServiceProxy serviceProxy, IMyServiceProxyRequestMapper requestMapper, IMyServiceProxyResponseMapper responseMapper)
                {
                    this.serviceProxy = serviceProxy;
                    this.requestMapper = requestMapper;
                    this.responseMapper = responseMapper;
                }

                public int Add(int a, int b)
                {
                    var request = requestMapper.Add(a, b);
                    var response = serviceProxy.Add(request);
                    var result = responseMapper.Add(response);
                    return result;
                }
            }


            public class MyService : IMyService
            {
                public MyService() { }

                public int Add(int a, int b)
                {
                    return a + b;
                }
            }
            public class MyServiceHandler : IMyServiceProxy
            {
                public IMyService myService;
                public IMyServiceProxyResponseMapper myResponseMapper;
                IMyServiceProxyServiceResponseMapper myServiceResponseMapper;
                public ApiResponse<int> Add(ApiRequest<int, int> request)
                {
                    var result = myService.Add(request.Arg0, request.Arg1);
                    var response = myServiceResponseMapper.Add(result);
                    return response;
                }
            }

        }
    }


    [TestClass]
    public class DotRpcMappedDynamicProxyTests
    {

    }
}
