using Castle.DynamicProxy;
using DotOrmLib;
using DotRpc;
using DotRpc.RpcClient;
using DotRpc.TestCommon;
using Microsoft.AspNetCore.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace DotOrmLibTests
{
  


    [TestClass]
    public class DotRpcTests
    {
        private ServiceProvider serviceProvider;

        public DotRpcTests()
        {
            var services= new ServiceCollection();
            services.AddDotRpcClientsFromAssembly(typeof(IRpcTestService));
            serviceProvider= services.BuildServiceProvider();
        }
        [TestMethod]
        public void TestRpcMock()
        {
            var client = serviceProvider.GetRequiredService<IRpcClient<IRpcTestService>>();
            client.Service.Add("a", "b");
        }
    }
}
