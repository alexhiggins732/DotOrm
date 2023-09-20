using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using static DotRpc.Tests.NameServiceGenericTests;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Reflection.Emit;
using DotRpc.TestCommon.IamCrudService;

namespace DotRpc.Tests
{
    [TestClass]
    public class NameServiceGenericTests
    {

        IServiceProvider serviceProvider;
        INameService nameService;
        public NameServiceGenericTests()
        {
            nameService = NameServiceExtensions.Instance;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<INameService, NameService>();
            serviceCollection.AddSingleton<ICSharpDeclarationProvider, CSharpDeclarationProvider>();
            var provider = serviceCollection.BuildServiceProvider();
            this.serviceProvider = provider;
        }


        [TestMethod]
        public void TestGenericServiceName()
        {
            var actual = typeof(ICrudService<>).GetSwaggerSchemaId();
            var expected = "ICrudServiceT";
            Assert.AreEqual(expected, actual);

            actual = typeof(ICrudService<App>).GetSwaggerSchemaId();
            expected = "ICrudServiceApp";
            Assert.AreEqual(expected, actual);

            actual = typeof(ICrudService<Role>).GetSwaggerSchemaId();
            expected = "ICrudServiceRole";
            Assert.AreEqual(expected, actual);

            actual = typeof(EntityCrudService<,>).GetSwaggerSchemaId();
            expected = "EntityCrudServiceTEntityTKey";
            Assert.AreEqual(expected, actual);

            actual = typeof(EntityCrudService<App, int>).GetSwaggerSchemaId();
            expected = "EntityCrudServiceAppInt32";
            Assert.AreEqual(expected, actual);

            actual = typeof(EntityCrudService<Role, int>).GetSwaggerSchemaId();
            expected = "EntityCrudServiceRoleInt32";
            Assert.AreEqual(expected, actual);



            actual = typeof(IEntityCrudService<,>).GetSwaggerSchemaId();
            expected = "IEntityCrudServiceTEntityTKey";
            Assert.AreEqual(expected, actual);

            actual = typeof(IEntityCrudService<App, int>).GetSwaggerSchemaId();
            expected = "IEntityCrudServiceAppInt32";
            Assert.AreEqual(expected, actual);

            actual = typeof(IIntEntityCrudService<>).GetSwaggerSchemaId();
            expected = "IIntEntityCrudServiceTEntityOfInt";
            Assert.AreEqual(expected, actual);

            actual = typeof(IIntEntityCrudService<App>).GetSwaggerSchemaId();
            expected = "IIntEntityCrudServiceApp";
            Assert.AreEqual(expected, actual);
        }



    }


}