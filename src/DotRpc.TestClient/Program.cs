using DotRpc.RpcClient;
using DotRpc.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Runtime.InteropServices;

namespace DotRpc.TestClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var test = new DotRpcClientTest();
            test.TestRpcCall();
        }

        public class DotRpcClientTest
        {
            private ServiceProvider serviceProvider;

            public DotRpcClientTest()
            {
                var services = new ServiceCollection();
                services.AddDotRpcClientsFromAssembly(typeof(IRpcTestService));
                serviceProvider = services.BuildServiceProvider();
            }

            public void TestRpcCall()
            {
                var client = serviceProvider.GetRequiredService<IRpcClient<IRpcTestService>>();
                client.Service.Add("a", "b");
            }
        }
    }
}