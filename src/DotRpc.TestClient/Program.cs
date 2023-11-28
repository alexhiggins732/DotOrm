using DotRpc.RpcClient;
using DotRpc.TestCommon;
using Grpc.Net.Client;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProtoBuf.Grpc.Client;
using System.Net;
using System.Runtime.InteropServices;

namespace DotRpc.TestClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var endpoint = "https://localhost:63601/";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var endpointwindows = "https://dotrpctestserver.azurewebsites.net/";
            var endpointLinux = "https://dotrpctestserverlinux.azurewebsites.net";
            var test = new DotRpcClientTest(endpointLinux);

            //test.TestRpcCall();
            var grpcTest = new DotGrpcClientTest(endpointLinux);


            GetOrAddValueRequest getOrAddValueRequest = new GetOrAddValueRequest { Key = "key", Value = "value" };
            var getOrAddResult = grpcTest.GetOrAdd(getOrAddValueRequest);
            Console.WriteLine($"DotGRpc.IRpcTestServiceWithRequests.GetOrAdd( {{Key = \"key\",Value = \"value\"}}).Result = {getOrAddResult}");
            AddValueRequest addRequest = new AddValueRequest { Key = "test", Value = "test" };
            var result = grpcTest.Add(addRequest);
            Console.WriteLine($"DotGRpc.IRpcTestServiceWithRequests.Add( {{Key = \"test\",Value = \"test\"}}).Result = {result}");
        }

        public class DotGrpcClientTest : IRpcTestServiceWithRequests, IDisposable
        {
            private IRpcTestServiceWithRequests client;
            private GrpcChannel channel;

            public DotGrpcClientTest(string apiEndPoint = "https://localhost:57057/")
            {
                // GrpcChannelOptions options = new GrpcChannelOptions();

                this.channel = GrpcChannel.ForAddress(apiEndPoint);

                var client = channel.CreateGrpcService<IRpcTestServiceWithRequests>();
                this.client = client;

            }

            public ApiBoolResponse Add(AddValueRequest request)
            {
                return client.Add(request);
            }

            public void Dispose()
            {
                channel.Dispose();
            }

            public ApiStringResponse Get(GetValueRequest request)
            {
                return client.Get(request);
            }

            public ApiStringResponse GetOrAdd(GetOrAddValueRequest request)
            {
                return client.GetOrAdd(request);
            }

            public ApiBoolResponse Remove(RemoveValueRequest request)
            {
                return client.Remove(request);
            }

            public ApiBoolResponse Set(SetValueRequest request)
            {
                return client.Set(request);
            }
        }

        public class DotRpcClientTest
        {
            private ServiceProvider serviceProvider;

            public DotRpcClientTest(string apiEndPoint)
            {
                var services = new ServiceCollection();
                services.AddDotRpc();
                services.AddDotRpcClientsFromAssembly(typeof(IRpcTestService));


                services.AddSingleton<IRpcEndpointProvider>(new RpcEndpointProvider(apiEndPoint));

                //services.AddDotRpcClientsFromAssembly(typeof(IRpcTestService));
                serviceProvider = services.BuildServiceProvider();
            }

            public void TestRpcCall()
            {
                var client = serviceProvider.GetRequiredService<IRpcClient<IRpcTestService>>();
                var result = client.Service.Add("a", "b");
                Console.WriteLine($"DotRpc.IRpcTestService.Add(a,b).Result = {result}");
            }
        }
    }
}