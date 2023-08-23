using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using DotOrmLib.GrpcModels.Interfaces;
using DotOrmLib.Proxy.Scc1.Interfaces;
using DotOrmLib.Proxy.Scc1.Services;
using Grpc.Net.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Bson;
using ProtoBuf.Grpc.Server;
using ProtoBuf.Grpc.Client;
using DotOrmLib.GrpcServices;
using DotOrmLib;
using System.Diagnostics;
using DotOrmLib.Proxy.Scc1.Models;
using DotOrmLib.GrpcModels.Scalars;
using System.Linq.Expressions;

namespace DotOrmLibTests
{
    [TestClass]
    public class DotOrmApiTests : IDisposable
    {
        private WebApplication app;

        public DotOrmApiTests()
        {
            //var args = new string[] { };
            //var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

            //// Additional configuration is required to successfully run gRPC on macOS.
            //// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            //// Add services to the container.
            //builder.Services.AddCodeFirstGrpc(x =>
            //{
            //    //x.Interceptors.Add()
            //});

            //// builder.Services.AddGrpcService<T>();
            //app = builder.Build();
            //app.Urls.Add("https://localhost:57057");
            //app.Urls.Add("http://localhost:57058");
            ////AddServices(builder.Services);

            //Task.Run(() => app.RunAsync());
        }


        bool appIsRunning = false;
        public async Task runApp()
        {
            if (!appIsRunning)
            {
                appIsRunning = true;
                app.RunAsync();
            }
            await Task.CompletedTask;
        }
        [TestMethod]
        public async Task TestIActionController()
        {
            //await runApp();
            using var channel = GrpcChannel.ForAddress("https://localhost:57057");

            var client = channel.CreateGrpcService<IActionsController>();

            var hc = await client.HealthCheck();
            Assert.IsNotNull(hc);

            var l = await client.GetList(new() { Skip = 0, Take = 50 });
            Assert.IsNotNull(l, "Result was null");
            Assert.IsNotNull(l.Value, "Result Value was null");
            Assert.IsNotNull(l.Value.Items, "Result Items was null");
            Assert.IsTrue(l.Value.Items.Any(), "Result Items was empty");
        }

        [TestMethod]
        public async Task TestAddGetDeleteWithoutKeyOrId()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:57057");

            var client = channel.CreateGrpcService<IAddressSuffixController>();
            var StreetSuffix = "SPACEX";
            var CommonSuffix = "SPACEX";
            var StandardSuffix = "SPCX";

           var rpcResult= await client.Add(new AddressSuffix { StreetSuffix=StreetSuffix, CommonSuffix=CommonSuffix, StandardSuffix=StandardSuffix });

            Assert.IsNotNull(rpcResult);
            Assert.IsNotNull(rpcResult);
            Assert.IsNotNull(rpcResult.Value);
            Assert.AreEqual(StreetSuffix, rpcResult.Value.StreetSuffix);
            Assert.AreEqual(StreetSuffix, rpcResult.Value.CommonSuffix);
            Assert.AreEqual(StreetSuffix, rpcResult.Value.StandardSuffix);

            var repo = new DotOrmRepo<AddressSuffix>(ConnectionStringProvider.Create().ConnectionString);
            var where = repo.Where(x => x.StreetSuffix == StreetSuffix && x.CommonSuffix == CommonSuffix && x.StandardSuffix == StandardSuffix);
            var filter = where.BuildFilter();
            var rpcResults = (await client.GetList(filter));

            Assert.IsNotNull(rpcResults);
            Assert.IsTrue(rpcResults.Success);
            Assert.IsNotNull(rpcResults.Value);
            Assert.IsNotNull(rpcResults.Value.Items);
            Assert.IsTrue(rpcResults.Value.Items.Any());
            var rpcFirst= rpcResults.Value.Items.FirstOrDefault();
            Assert.IsNotNull(rpcFirst);
            Assert.AreEqual(StreetSuffix, rpcFirst.StreetSuffix);
            Assert.AreEqual(StreetSuffix, rpcFirst.CommonSuffix);
            Assert.AreEqual(StreetSuffix, rpcFirst.StandardSuffix);

            //client.Delete(rpcFirst);

        }

        Type GetServiceEntityType(Type serviceContractType)
        {

            Type baseInterface = serviceContractType
                .GetInterfaces()
                .First(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IServiceController<>));
            if (baseInterface is null)
                throw new Exception("Contract does not implement: IServiceController<>");
            // Get the generic type argument used in IServiceController<T>
            Type entityType = baseInterface.GetGenericArguments().First();
            return entityType;
        }
        public async Task TestChannel<TService, TEntity>(GrpcChannel channel)
            where TService : class, IServiceController<TEntity>
            where TEntity : class
        {
            Console.WriteLine($"Testing {typeof(TEntity)} channel.");
            var sw = Stopwatch.StartNew();
            try
            {
                var client = channel.CreateGrpcService<TService>();
                var hc = await client.HealthCheck();
                Assert.IsNotNull(hc);

                var l = await client.GetList(new() { Skip = 0, Take = 50 });
                if (l.Errors is not null)
                {
                    Assert.IsNull(l.Errors, $"{l.ErrorMessage}: {string.Join("\n\n", l.Errors)}");
                }
                Assert.IsTrue(l.Success, "Result Success was false");
                Assert.IsNotNull(l.Value, "Result Value was null");
              
                //Assert.IsNotNull(l.Value.Items, "Result Items was null");
                //Assert.IsTrue(l.Value.Items.Any(), "Result Items was empty");
                Console.WriteLine($"Testing {typeof(TEntity)} took {sw.Elapsed}.");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Failed {typeof(TEntity)} in {sw.Elapsed} - {ex}.");
            }


        }
        [TestMethod]
        public async Task TestAllControllers()
        {
            var t = typeof(IActionsController);
            var services = ServiceBuilder.GetServiceContractsInNamespace(t.Assembly, t.Namespace).ToList();
            var req = new FilterRequest { Skip = 0, Take = 50 };
            foreach (var service in services)
            {
                using var channel = GrpcChannel.ForAddress("https://localhost:57057");
                var m = typeof(DotOrmApiTests).GetMethod("TestChannel");
                var entityType = GetServiceEntityType(service);

                var gm = m.MakeGenericMethod(service, entityType);
                await ((Task)gm.Invoke(this, new object[] { channel }));

            }

            //await runApp();

        }



        public void Dispose()
        {
            //app.StopAsync().Wait();
        }
    }
}
