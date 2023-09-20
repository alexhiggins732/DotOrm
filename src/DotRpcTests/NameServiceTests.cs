using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DotRpc.Tests
{
    [TestClass()]
    public class NameServiceTests
    {
        IServiceProvider serviceProvider;
        INameService nameService;
        public NameServiceTests()
        {
            nameService = NameServiceExtensions.Instance;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<INameService, NameService>();
            serviceCollection.AddSingleton<ICSharpDeclarationProvider, CSharpDeclarationProvider>();
            var provider = serviceCollection.BuildServiceProvider();
            this.serviceProvider = provider;
        }


        //CleanName(string input)
        [TestMethod()]
        [DataRow("  Test  ", "Test")]  // Inline data with expected result
        [DataRow("  Another Name  ", "AnotherName")]
        [DataRow("John Smith", "JohnSmith")]
        public void CleanName(string input, string expected)
        {
            var actual = nameService.CleanName(input);
            Assert.AreEqual(expected, actual);
        }


        // string CleanName(Type type);
        [TestMethod()]
        [DataRow(typeof(NameServiceTests), "Test")]  // Inline data with expected result
        [DataRow(typeof(IRpcClient<>), "Another Name")]
        [DataRow(typeof(IRpcClient<IRpcHandler>), "John Smith")]
        public void CleanName(Type input, string expected)
        {
            var actual = nameService.CleanName(input);
            Assert.AreEqual(expected, actual);
        }


        //string GenerateMethodName(MethodTypeDescription method);
        [TestMethod()]
        public void GenerateMethodName()
        {
            var methodTests = new Dictionary<MethodInfo, string>
            {
                { typeof(INameService).GetMethod(nameof(INameService.GenerateRpcMethodName)), "Testhost.NameService.GenerateRpcMethodNameWithMethodContract" },
            };

            methodTests.ToList().ForEach(x => GenerateRpcMethodName(new MethodTypeDescription(x.Key), x.Value));

        }
        public void GenerateRpcMethodName(MethodTypeDescription method, string expected)
        {
            var actual = nameService.GenerateRpcMethodName(method);
            Assert.AreEqual(expected, actual);
        }

        //string GenerateSwaggerSchemaId(Type type);
        [TestMethod()]
        [DataRow(typeof(NameServiceTests), "NameServiceTests")]  // Inline data with expected result
        [DataRow(typeof(IRpcClient<>), "IRpcClientTService")]
        [DataRow(typeof(IRpcClient<IRpcHandler>), "IRpcClientIRpcHandler")]
        public void GenerateSwaggerSchemaId(Type type, string expected)
        {
            var actual = nameService.GenerateSwaggerSchemaId(type);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DataRow(typeof(NameServiceTests), "NameServiceTests")]  // Inline data with expected result
        [DataRow(typeof(IRpcClient<>), "IRpcClientTService")]
        [DataRow(typeof(IRpcClient<IRpcHandler>), "IRpcClientIRpcHandler")]
        public void GetSwaggerSchemaId(Type type, string expected)
        {
            var actual = nameService.GetSwaggerSchemaId(type);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        [DataRow(typeof(NameServiceTests), "Test")]  // Inline data with expected result
        [DataRow(typeof(IRpcClient<>), "Another Name")]
        [DataRow(typeof(IRpcClient<IRpcHandler>), "John Smith")]
        void GenerateSwaggerTag(Type type, string expected)
        {
            var actual = nameService.GenerateSwaggerTag(type);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetApiPathName()
        {
            var methodTests = new Dictionary<MethodInfo, string>
            {
                { typeof(NameService).GetMethod(nameof(NameService.GenerateRpcMethodName)), "GenerateRpcMethodNameWithMethod" },
                { typeof(NameService).GetMethod(nameof(NameService.GenerateSwaggerSchemaId)), "GenerateSwaggerSchemaIdWithType" },
            };

            methodTests.ToList().ForEach(x => GetApiPathName(x.Key, x.Value));
        }

        public void GetApiPathName(MethodInfo method, string expected)
        {
            var actual = nameService.GetApiPathName(method);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        [DataRow("Inline data with expected result", "Inline Data With Expected Result")]  // Inline data with expected result
        [DataRow("typeof(iRpc_client<>)", "Typeof(IRpc_Client<>)")]
        [DataRow("typeof(iRpc_client<iGRPCHandler>)", "Typeof(IRpc_Client<IGrpcHandler>)")]
        public void ToPascalCase(string value, string expected)
        {
            var actual = nameService.ToPascalCase(value);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DataRow("Inline data with expected result", "InlineDataWithExpectedResult")]  // Inline data with expected result
        [DataRow("typeof(iRpc_client<>)", "TypeofIRpcClient")]
        [DataRow("typeof(iRpc_client<iGRPCHandler>)", "TypeofIRpcClientIGrpcHandler")]
        public void ToPropertyName(string name, string expected)
        {
            var actual = nameService.ToPropertyName(name);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void ToPropertyNameFromAssembly()
        {
            var methodTests = new Dictionary<Assembly, string>
            {
                { typeof(NameServiceTests).Assembly, "DotRpcTests" },
                { typeof(IRpcMapper).Assembly, "DotRpc" },
            };

            methodTests.ToList().ForEach(x => ToPropertyName(x.Key, x.Value));
        }

        [DataRow(typeof(IRpcClient<IRpcHandler>), "John Smith")]
        public void ToPropertyName(Assembly assembly, string expected)
        {
            var actual = nameService.ToPropertyName(assembly);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DataRow(typeof(NameServiceTests), "Test")]  // Inline data with expected result
        [DataRow(typeof(IRpcClient<>), "Another Name")]
        public void ToPropertyName(Type type, string expected)
        {
            var actual = nameService.ToPropertyName(type);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ToPropertyNameFromProperty()
        {
            var methodTests = new Dictionary<PropertyInfo, string>
            {
                { typeof(RpcPayload).GetProperty(nameof(RpcPayload.Payload)), "Payload" },
            };

            methodTests.ToList().ForEach(x => ToPropertyName(x.Key, x.Value));
        }

        public void ToPropertyName(PropertyInfo property, string expected)
        {
            var actual = nameService.ToPropertyName(property);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void ToPropertyNameFromMethodInfo()
        {
            var methodTests = new Dictionary<MethodInfo, string>
            {
                { typeof(NameService).GetMethod(nameof(NameService.GenerateRpcMethodName)), "GenerateRpcMethodName" },
                { typeof(NameService).GetMethod(nameof(NameService.GenerateSwaggerSchemaId)), "GenerateSwaggerSchemaId" },
            };

            methodTests.ToList().ForEach(x => ToPropertyName(x.Key, x.Value));
        }


        [TestMethod()]
        public void ToPropertyName(MethodInfo method, string expected)
        {
            var actual = nameService.ToPropertyName(method);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ToPropertyNameFromMethodDescription()
        {
            var methodTests = new Dictionary<MethodInfo, string>
            {
                { typeof(IRpcPayloadClient).GetMethod(nameof(IRpcPayloadClient.Invoke)), "Invoke" },
                { typeof(IRpcMapper).GetMethod(nameof(IRpcMapper.MapResponse)), "MapResponse" },
            };

            methodTests.ToList().ForEach(x => ToPropertyName(new MethodTypeDescription(x.Key), x.Value));
        }

        public void ToPropertyName(MethodTypeDescription description, string expected)
        {
            var actual = nameService.ToPropertyName(description);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ToPropertyNameFromParameterInfo()
        {
            var methodTests = new Dictionary<MethodInfo, string>
            {
                { typeof(IRpcMapper).GetMethod(nameof(IRpcMapper.MapResponse)), "MethodResult" },
            };

            methodTests.ToList().ForEach(x => GetParameterName(x.Key.GetParameters().First(), x.Value));
        }


        public void GetParameterName(ParameterInfo x, string expected)
        {
            var actual = nameService.GetParameterName(x);
            Assert.AreEqual(expected, actual);
        }




        [TestMethod()]
        [DataRow(typeof(IRpcHandler), false, false, "IRpcHandler")]
        [DataRow(typeof(IRpcClient<>), false, false, "IRpcClient<TService>")]
        [DataRow(typeof(IRpcClient<IRpcMapper>), false, false, "IRpcClient<IRpcMapper>")]
        [DataRow(typeof(IRpcClient<IObservable<int?>>), false, false, "IRpcClient<IObservable<Nullable<Int32>>>")]

        [DataRow(typeof(IRpcHandler), true, false, "DotRpc.IRpcHandler")]
        [DataRow(typeof(IRpcClient<>), true, false, "DotRpc.IRpcClient<>")]
        [DataRow(typeof(IRpcClient<IRpcMapper>), true, false, "DotRpc.IRpcClient<DotRpc.IRpcMapper>")]
        [DataRow(typeof(IRpcClient<IObservable<int?>>), true, false, "DotRpc.IRpcClient<System.IObservable<System.Nullable<System.Int32>>>")]

        [DataRow(typeof(IRpcHandler), true, true, "DotRpc.DotRpc.IRpcHandler")]
        [DataRow(typeof(IRpcClient<>), true, true, "DotRpc.DotRpc.IRpcClient<>")]
        [DataRow(typeof(IRpcClient<IRpcMapper>), true, true, "DotRpc.DotRpc.IRpcClient<DotRpc.IRpcMapper>")]
        [DataRow(typeof(IRpcClient<IObservable<int?>>), true, true, "DotRpc.DotRpc.IRpcClient<System.IObservable<System.Nullable<System.Int32>>>")]
        public void CSharpDeclarationTests(Type type, bool includeNamespace, bool includeAssemblyName, string expected)
        {
            var decService = serviceProvider.GetRequiredService<ICSharpDeclarationProvider>();
            var actual = decService.ToCSharpDeclaration(type, includeNamespace, includeAssemblyName);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        [DataRow(typeof(IRpcHandler), false, false, "IRpcHandler")]
        [DataRow(typeof(IRpcClient<>), false, false, "IRpcClientTService")]
        [DataRow(typeof(IRpcClient<IRpcMapper>), false, false, "IRpcClientIRpcMapper")]
        [DataRow(typeof(IRpcClient<IObservable<int?>>), false, false, "IRpcClientIObservableNullableInt32")]

        [DataRow(typeof(IRpcHandler), true, false, "DotRpcIRpcHandler")]
        [DataRow(typeof(IRpcClient<>), true, false, "DotRpcIRpcClient")]
        [DataRow(typeof(IRpcClient<IRpcMapper>), true, false, "DotRpcIRpcClientDotRpcIRpcMapper")]
        [DataRow(typeof(IRpcClient<IObservable<int?>>), true, false, "DotRpcIRpcClientSystemIObservableSystemNullableSystemInt32")]

        [DataRow(typeof(IRpcHandler), true, true, "DotRpcDotRpcIRpcHandler")]
        [DataRow(typeof(IRpcClient<>), true, true, "DotRpcDotRpcIRpcClient")]
        [DataRow(typeof(IRpcClient<IRpcMapper>), true, true, "DotRpcDotRpcIRpcClientDotRpcIRpcMapper")]
        [DataRow(typeof(IRpcClient<IObservable<int?>>), true, true, "DotRpcDotRpcIRpcClientSystemIObservableSystemNullableSystemInt32")]
        public void CSharpTypeNameTests(Type type, bool includeNamespace, bool includeAssemblyName, string expected)
        {
            var decService = serviceProvider.GetRequiredService<ICSharpDeclarationProvider>();
            var decl = decService.ToCSharpDeclaration(type, includeNamespace, includeAssemblyName);
            var actual = nameService.ToPropertyName(decl);
            Assert.AreEqual(expected, actual);
        }


    }
}