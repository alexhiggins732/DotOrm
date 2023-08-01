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

namespace DotOrmLibTests
{
    [TestClass]
    public class DotOrmApiTests : IDisposable
    {
        private WebApplication app;

        public DotOrmApiTests()
        {
            var args = new string[] { };
            var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddCodeFirstGrpc(x =>
            {
                //x.Interceptors.Add()
            });
            // builder.Services.AddGrpcService<T>();
            app = builder.Build();

            AddServices(builder.Services);

            Task.Run(() => app.RunAsync());
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
            await runApp();
            using var channel = GrpcChannel.ForAddress("https://localhost:57057/");

            var client = channel.CreateGrpcService<IActionsController>();

            var hc = await client.HealthCheck();
            var l = await client.GetList(0, 10);
            Assert.IsNotNull(l);
            Assert.IsNotNull(l.Items);
            Assert.IsTrue(l.Items.Any());
        }

        private void AddServices(IServiceCollection services)
        {
            var ns = typeof(IActionsController);
            var contracts = GetServiceContractsInNamespace(ns.Assembly, ns.Namespace).ToList();

            foreach (var contract in contracts)
            {
                Type serviceType = buildServiceContoller(contract);

                MapService(serviceType);
            }
        }

        public void MapService(Type serviceControllerType)
        {
            // call mapservice<t> for service controller type.
            var mapServiceMethod = typeof(DotOrmApiTests).GetMethod("MapGrpcService", BindingFlags.Public | BindingFlags.Instance)
                .MakeGenericMethod(serviceControllerType);

            mapServiceMethod.Invoke(this, new object[] { });
        }
        public void MapGrpcService<T>()
            where T : class
        {
            app.MapGrpcService<T>();
        }

        private static Type GetServiceContract(Type entityType)
        {
            var assemblyName = new AssemblyName("DynamicAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
            var contractBuilder = moduleBuilder.DefineType(entityType.Name, TypeAttributes.Public);

            // Define the ServiceContract attribute on the new type
            var serviceContractAttrCtor = typeof(ServiceContractAttribute).GetConstructor(Type.EmptyTypes);
            var serviceContractBuilder = new CustomAttributeBuilder(serviceContractAttrCtor, new object[0]);
            contractBuilder.SetCustomAttribute(serviceContractBuilder);

            // Inherit the IServiceController<T> interface
            var contractBaseType = typeof(IServiceController<>).MakeGenericType(entityType);
            contractBuilder.AddInterfaceImplementation(contractBaseType);

            // Define the methods on the new type (same as in IServiceController<T>)
            foreach (var methodInfo in contractBaseType.GetMethods())
            {
                var methodBuilder = contractBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    methodInfo.ReturnType,
                    methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());

                // Emit IL implementation for the methods (You can leave this empty if you just need the definition)
                //var il = methodBuilder.GetILGenerator();
                //il.ThrowException(typeof(NotImplementedException));
            }

            var serviceContractType = contractBuilder.CreateType();
            return serviceContractType;

        }

        public Type buildServiceContoller(Type serviceContractType)
        {
            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            Type baseInterface = serviceContractType.GetInterfaces().First(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IServiceController<>));
            if (baseInterface is null)
                throw new Exception("Contract does not implement: IServiceController<>");
            // Get the generic type argument used in IServiceController<T>
            Type entityType = baseInterface.GetGenericArguments().First();
            //var baseControllerType=typeof(ControllerBase<>.)
            // Generate a dynamic module in the assembly
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

            // Generate a dynamic type that inherits ControllerBase<T> and implements the service contract interface
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                "DynamicServiceController" + serviceContractType.Name,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
                typeof(ControllerBase<>).MakeGenericType(entityType));




            var contractBuilder = moduleBuilder.DefineType(serviceContractType.Name, TypeAttributes.Public |
                            TypeAttributes.Interface |
                            TypeAttributes.Abstract |
                            TypeAttributes.AutoClass |
                            TypeAttributes.AnsiClass |
                            TypeAttributes.BeforeFieldInit |
                            TypeAttributes.AutoLayout
                            , null);

            // Define the ServiceContract attribute on the new type
            var serviceContractAttrCtor = typeof(ServiceContractAttribute).GetConstructor(Type.EmptyTypes);
            var serviceContractBuilder = new CustomAttributeBuilder(serviceContractAttrCtor, new object[0]);
            contractBuilder.SetCustomAttribute(serviceContractBuilder);

            // Inherit the IServiceController<T> interface
            var contractBaseType = typeof(IServiceController<>).MakeGenericType(entityType);
            contractBuilder.AddInterfaceImplementation(contractBaseType);

            // Define the methods on the new type (same as in IServiceController<T>)
            foreach (var methodInfo in contractBaseType.GetMethods())
            {
                var methodBuilder = contractBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Abstract,
                    methodInfo.ReturnType,
                    methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());

                // Emit IL implementation for the methods (You can leave this empty if you just need the definition)
                //var il = methodBuilder.GetILGenerator();
                //il.ThrowException(typeof(NotImplementedException));
            }

            var serviceContract = contractBuilder.CreateType();
            typeBuilder.AddInterfaceImplementation(serviceContract);
            //typeBuilder.AddInterfaceImplementation(serviceContractType);
            // Create the type
            Type dynamicType = typeBuilder.CreateType();

            return dynamicType;
        }
        public static IEnumerable<Type> GetServiceContractsInNamespace(Assembly asm, string targetNamespace)
        {

            var serviceContractTypes = asm.GetExportedTypes()
                .Where(type =>
                    type.Namespace == targetNamespace &&
                    type.GetCustomAttributes(typeof(ServiceContractAttribute), true).Any());

            return serviceContractTypes;
        }
        public void Dispose()
        {
            app.StopAsync().Wait();
        }
    }
}
