using DotOrmLib.GrpcServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using ProtoBuf.Grpc.Server;

namespace DotOrmLib
{

    public static class ServiceBuilderExtensions
    {
        public static void AddDynamicGrpcServiceFromTypeAssembly(this IEndpointRouteBuilder app, Type contractType)
        {
            var builder = new ServiceBuilder(app);
            builder.AddServicesFromTypeAssembly(contractType);
        }
    }
    public class ServiceBuilder
    {
        IEndpointRouteBuilder grpcApp;
        public ServiceBuilder(IEndpointRouteBuilder grpcApp)
        {
            this.grpcApp = grpcApp;
        }
        public void AddServicesFromTypeAssembly(Type contractType)
        {

            var dynamicAssemblyName = $"Dynamic{contractType.Assembly.ManifestModule.Name}";
            AssemblyName assemblyName = new AssemblyName(dynamicAssemblyName);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
            var contracts = GetServiceContractsInNamespace(contractType.Assembly, contractType.Namespace).ToList();

            foreach (var contract in contracts)
            {
                Type serviceType = buildServiceContoller(moduleBuilder, contract);
                MapService(serviceType);
            }
        }

        public void MapService(Type serviceControllerType)
        {
            // call mapservice<t> for service controller type.
            var mapServiceMethod = typeof(ServiceBuilder).GetMethod("MapGrpcService", BindingFlags.Public | BindingFlags.Instance)
                .MakeGenericMethod(serviceControllerType);

            mapServiceMethod.Invoke(this, new object[] { });
        }
        public void MapGrpcService<T>()
            where T : class
        {
            grpcApp.MapGrpcService<T>();
        }


        static Type buildServiceContoller(ModuleBuilder moduleBuilder, Type serviceContractType)
        {


            Type baseInterface = serviceContractType.GetInterfaces().First(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IServiceController<>));
            if (baseInterface is null)
                throw new Exception("Contract does not implement: IServiceController<>");
            // Get the generic type argument used in IServiceController<T>
            Type entityType = baseInterface.GetGenericArguments().First();
            //var baseControllerType=typeof(ControllerBase<>.)
            // Generate a dynamic module in the assembly


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


    }
}
