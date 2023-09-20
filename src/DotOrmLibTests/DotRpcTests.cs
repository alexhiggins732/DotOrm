using Castle.DynamicProxy;
using DotOrmLib;
using DotRpc;
using DotRpc.RpcClient;
using DotRpc.TestCommon;
using Microsoft.AspNetCore.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using DotRpc.TestServer;
using DotRpc.TestCommon.IamCrudService;
using ProtoBuf.Grpc.Client;
using System.Reflection;
using System.Linq.Expressions;

namespace DotRpc.DotOrmLibTests
{

    [TestClass]
    public class ProxyCacheTests
    {
        public class TestNonGeneric
        {
            public int Get() { return 1; }
            public int Get(int a) { return a; }
            public int Get(string value) { return int.Parse(value); }


        }
        public class TestGenericMethodClass
        {
            public int Get() { return 1; }
            public T Get<T>() { return default; }
            public T Get<T>(T a) { return a; }
            public T Get<T>(string value) { return (T)(object)int.Parse(value); }
        }

        public class TestGenericClass<T>
        {
            public T Get() { return default; }
            public T Get(T a) { return a; }
            public T Get(string value) { return (T)(object)int.Parse(value); }
        }

        public class TestGenericClass<T1, T2>
        {
            public T1 Get() { return default; }
            public T2 Get<M1>() { return default; }
            public T1 Get<M1>(T1 value) { return default; }
            public T1 Get<M1>(M1 m1Value, T1 t1Value) { return default; }


            public T1 Get<M1, M2>() { return default; }
            public T1 Get<M1, M2>(T1 t1Value, T2 t2value, M2 m2Value) { return default; }
            public T1 Get<M1, M2>(T1 t1Value, T2 t2value) { return default; }
        }

        [TestMethod]
        public void TestNonGenericCache()
        {
            var d = new Dictionary<MethodInfo, int>();
            var t = typeof(TestNonGeneric);

            var m = t.GetMethod(nameof(TestNonGeneric.Get), Type.EmptyTypes);
            Assert.IsNotNull(m);
            var expected = m.MetadataToken;
            d.Add(m, expected);
            var actual = d[m];
            Assert.AreEqual(expected, actual);


            var a = new TestNonGeneric();
            MethodInfo ma = GetMethod(() => a.Get());
            Assert.IsNotNull(ma);
            Assert.AreEqual(expected, ma.MetadataToken);
            Assert.IsTrue(d.ContainsKey(ma));


            Func<int> get = () => a.Get();
            var method = get.Method;
            Assert.AreNotEqual(m.MetadataToken, method.MetadataToken);
            Assert.IsFalse(d.ContainsKey(method));



            var m2 = t.GetMethod(nameof(TestNonGeneric.Get), GetTypes(1));
            Assert.IsNotNull(m2);
            Assert.AreNotEqual(m.MetadataToken, m2.MetadataToken);
            Assert.IsFalse(d.ContainsKey(m2));
            d.Add(m2, m2.MetadataToken);

            MethodInfo ma2 = GetMethod(() => a.Get(1));
            Assert.IsNotNull(ma2);
            Assert.AreEqual(m2.MetadataToken, ma2.MetadataToken);
            Assert.IsTrue(d.ContainsKey(ma2));


            var m3 = t.GetMethod(nameof(TestNonGeneric.Get), GetTypes(""));
            Assert.IsNotNull(m);
            Assert.AreNotEqual(m.MetadataToken, m3.MetadataToken);
            Assert.IsFalse(d.ContainsKey(m3));
            d.Add(m3, m2.MetadataToken);

            MethodInfo ma3 = GetMethod(() => a.Get(""));
            Assert.IsNotNull(ma3);
            Assert.AreEqual(m3.MetadataToken, ma3.MetadataToken);
            Assert.IsTrue(d.ContainsKey(ma3));

        }

        void AssertAdd(Dictionary<MethodInfo, int> cache, MethodInfo method)
        {
            Assert.IsNotNull(method);
            Assert.IsFalse(cache.ContainsKey(method));
            cache.Add(method, method.MetadataToken);
            Assert.AreEqual(method.MetadataToken, cache[method]);
        }

        [TestMethod]
        public void TestGenericMethod()
        {
            var d = new Dictionary<MethodInfo, int>();
            var t = typeof(TestNonGeneric);

            var a = new TestGenericMethodClass();
            var m = GetMethod(() => a.Get());
            AssertAdd(d, m);


            var m2 = GetMethod(() => a.Get<int>());
            AssertAdd(d, m2);

            var m3 = GetMethod(() => a.Get<int>(1));
            AssertAdd(d, m3);

            var m4 = GetMethod(() => a.Get<int>(""));
            AssertAdd(d, m4);
        }

        [TestMethod]
        public void TestGenericClassMethods()
        {
            var d = new Dictionary<MethodInfo, int>();
            var t = typeof(TestNonGeneric);


            {
                var a = new TestGenericClass<int>();
                var m = GetMethod(() => a.Get());
                AssertAdd(d, m);


                var m2 = GetMethod(() => a.Get());
                AssertAdd(d, m2);

                var m3 = GetMethod(() => a.Get(1));
                AssertAdd(d, m3);

                var m4 = GetMethod(() => a.Get(""));
                AssertAdd(d, m4);
            }

            {
                var a = new TestGenericClass<string>();
                var m = GetMethod(() => a.Get());
                AssertAdd(d, m);


                var m2 = GetMethod(() => a.Get());
                AssertAdd(d, m2);

                var m3 = GetMethod(() => a.Get(""));
                AssertAdd(d, m3);

                var m4 = GetMethod(() => a.Get(""));
                AssertAdd(d, m4);
            }




        }


        [TestMethod]
        public void TestGenericClassWithGenericMethods()
        {
            var d = new Dictionary<MethodInfo, int>();
            var t = typeof(TestNonGeneric);

            /*
           
            public T1 Get() { return default; }
            public T2 Get<M1>() { return default; }
            public T1 Get<M1>(T1 value) { return default; }
            public T1 Get<M1>(M1 m1value, T1 t1Value) { return default; }
            public T1 Get<M1>( T1 t1Value, M1 m1Value) { return default; }

            public T1 Get<M1, M2>() { return default; }
            public T1 Get<M1, M2>(T1 t1Value, T2 t2value, M2 m2Value) { return default; }
            public T1 Get<M1, M2>(T1 t1Value, T2 t2value) { return default; }
             * */
            {
                var a = new TestGenericClass<int, int>();

                {
                    var m = GetMethod(() => a.Get());
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int>()); ;
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<string>());
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<string>(1));
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int>(1, 1));
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<string>("", 1));
                    AssertAdd(d, m);
                }

                {
                    var m = GetMethod(() => a.Get<string, int>());
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int, int>());
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int, string>());
                    AssertAdd(d, m);
                }

                {
                    var m = GetMethod(() => a.Get<string, int>(1, 1, 1));
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int, int>(1, 1, 1));
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int, string>(1, 1, ""));
                    AssertAdd(d, m);
                }
            

                {
                    var m = GetMethod(() => a.Get<int, int>(1, 1));
                    AssertAdd(d, m);
                }

                {
                    var m = GetMethod(() => a.Get<int, string>(1, 1));
                    AssertAdd(d, m);
                }

           

                {
                    var m = GetMethod(() => a.Get<string, string>(1, 1));
                    AssertAdd(d, m);
                }
            }

            {
                var a = new TestGenericClass<int, string>();

                {
                    var m = GetMethod(() => a.Get());
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int>()); ;
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<string>());
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<string>(1));
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int>(1, 1));
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<string>("", 1));
                    AssertAdd(d, m);
                }

                {
                    var m = GetMethod(() => a.Get<string, int>());
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int, int>());
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int, string>());
                    AssertAdd(d, m);
                }

                {
                    var m = GetMethod(() => a.Get<string, int>(1, "1", 1));
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int, int>(1, "1", 1));
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<int, string>(1, "1", ""));
                    AssertAdd(d, m);
                }


                {
                    var m = GetMethod(() => a.Get<int, int>(1, "1"));
                    AssertAdd(d, m);
                }

                {
                    var m = GetMethod(() => a.Get<int, string>(1, "1"));
                    AssertAdd(d, m);
                }
                {
                    var m = GetMethod(() => a.Get<string, string>(1, "1"));
                    AssertAdd(d, m);
                }


            }



        }



        public static MethodInfo GetMethod(Expression<Action> expression)
        {
            if (expression.Body is MethodCallExpression methodCallExpression)
            {
                return methodCallExpression.Method;
            }

            throw new ArgumentException("Invalid expression. Must be a method call expression.", nameof(expression));
        }

        Type[] GetTypes(params object[] values)
        {

            return values.Select(x => x.GetType()).ToArray();
        }
    }

    [TestClass]
    public class DotRpcTests : IDisposable
    {


        ServiceProvider serviceProvider;
        private bool serverStopped = false;

        private static readonly Mutex TestMutex = new Mutex();

        public DotRpcTests()
        {
            TestMutex.WaitOne(); // Acquire the mutex
            var args = new string[] { };
            Server.RunServer(args, true);
            var endpoint = Server.WebApplication.Urls.First(x => x.StartsWith("https", StringComparison.OrdinalIgnoreCase));
            var services = new ServiceCollection();
            services.AddDotRpc();
            services.AddDotRpcClientsFromAssembly(typeof(IRpcTestService));
            services.AddSingleton<IRpcEndpointProvider>(new RpcEndpointProvider(endpoint));
            serviceProvider = services.BuildServiceProvider();
        }

        public void Dispose()
        {
            Server.WebApplication.StopAsync().Wait();
            TestMutex.ReleaseMutex(); // Release the mutex
        }

        [TestMethod]
        public void TestServiceResolution()
        {

            IRpcService client = serviceProvider.GetRequiredService<IRpcClient<IRpcTestService>>();
            Assert.IsNotNull(client);

            client = serviceProvider.GetRequiredService<IRpcClient<IRpcGenericTestService>>();
            Assert.IsNotNull(client);

            client = serviceProvider.GetRequiredService<IRpcClient<IAppCrudService>>();
            Assert.IsNotNull(client);

            client = serviceProvider.GetRequiredService<IRpcClient<IIntEntityCrudService<App>>>();
            Assert.IsNotNull(client);

            client = serviceProvider.GetRequiredService<IRpcClient<IIntEntityCrudService<Role>>>();
            Assert.IsNotNull(client);
        }

        [TestMethod]
        public void TestDotRpcTestService()
        {
            var client = serviceProvider.GetRequiredService<IRpcClient<IRpcTestService>>();
            string testKey = nameof(testKey);
            string testValue = nameof(testValue);

            //add a non-existent key;
            var result = client.Service.Add(testKey, testValue);
            Assert.IsTrue(result);

            //add a key that already exists
            result = client.Service.Add(testKey, testValue);
            Assert.IsFalse(result);

            //get a key that already exists
            var actualValue = client.Service.Get(testKey);
            Assert.AreEqual(testValue, actualValue);
            //get a key that already exists, ignoring case
            actualValue = client.Service.Get(testKey.ToLower());
            Assert.AreEqual(testValue, actualValue);
            //get a key that does not exists
            actualValue = client.Service.Get(testValue);
            Assert.IsNull(actualValue);


            //add or update a key that already exists
            string testUpdatedValue = nameof(testUpdatedValue);
            result = client.Service.AddOrUpdate(testKey, testUpdatedValue);
            Assert.IsTrue(result);
            //get the updated a key, ignore case
            actualValue = client.Service.Get(testKey.ToLower());
            Assert.AreEqual(testUpdatedValue, actualValue);

            //add or update a key that does not exists
            result = client.Service.AddOrUpdate(testUpdatedValue, testUpdatedValue);
            Assert.IsTrue(result);
            actualValue = client.Service.Get(testUpdatedValue.ToLower());
            Assert.AreEqual(testUpdatedValue, actualValue);

            //remove a key that does not exist
            result = client.Service.Remove(testValue);
            Assert.IsFalse(result);
            //remove a key that does exists, ignoring case
            result = client.Service.Remove(testUpdatedValue.ToLower());
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void TestDotRpcGenericTestService()
        {
            var client = serviceProvider.GetRequiredService<IRpcClient<IRpcGenericTestService>>();
            string testKey = nameof(testKey);
            string testValue = nameof(testValue);

            //add a non-existent key;
            var result = client.Service.AddGeneric(testKey, testValue);
            Assert.IsTrue(result);

            //add a key that already exists
            result = client.Service.AddGeneric(testKey, testValue);
            Assert.IsFalse(result);

            //get a key that already exists
            var actualValue = client.Service.GetGeneric<string>(testKey);
            Assert.AreEqual(testValue, actualValue);
            //get a key that already exists, ignoring case
            actualValue = client.Service.GetGeneric<string>(testKey.ToLower());
            Assert.AreEqual(testValue, actualValue);
            //get a key that does not exists
            actualValue = client.Service.GetGeneric<string>(testValue);
            Assert.IsNull(actualValue);


            Assert.ThrowsException<DotRpcError>(() => client.Service.GetGeneric<DateTime?>(testKey));

            string testDateKey = nameof(testDateKey);
            var testDateValue = DateTime.Now;
            result = client.Service.AddGeneric(testDateKey, testDateValue);
            Assert.IsTrue(result);

            result = client.Service.AddGeneric(testDateKey, testDateValue);
            Assert.IsFalse(result);


            //get a key that already exists
            var actualDateValue = client.Service.GetGeneric<DateTime?>(testDateKey);
            Assert.AreEqual(testDateValue, actualDateValue);
            //get a key that already exists, ignoring case
            actualDateValue = client.Service.GetGeneric<DateTime?>(testDateKey.ToLower());
            Assert.AreEqual(testDateValue, actualDateValue);


            //returns null when key doesn't exist
            actualDateValue = client.Service.GetGeneric<DateTime?>(testValue);
            Assert.IsNull(actualDateValue);

            //throw exception when key exists for an invalid datatype
            Assert.ThrowsException<DotRpcError>(() => actualDateValue = client.Service.GetGeneric<DateTime?>(testKey));



            string testUpdatedValue = nameof(testUpdatedValue);
            result = client.Service.AddOrUpdateGeneric(testKey, testUpdatedValue);
            Assert.IsTrue(result);
            //get the updated a key, ignore case
            actualValue = client.Service.GetGeneric<string>(testKey.ToLower());
            Assert.AreEqual(testUpdatedValue, actualValue);

            //add or update a key that does not exists
            result = client.Service.AddOrUpdateGeneric(testUpdatedValue, testUpdatedValue);
            Assert.IsTrue(result);
            actualValue = client.Service.GetGeneric<string>(testUpdatedValue.ToLower());
            Assert.AreEqual(testUpdatedValue, actualValue);

            //remove a key that does not exist
            result = client.Service.RemoveGeneric(testValue);
            Assert.IsFalse(result);
            //remove a key that does exists, ignoring case
            result = client.Service.RemoveGeneric(testUpdatedValue.ToLower());
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void TestDotRpcTestAppCrudService()
        {
            var client = serviceProvider.GetRequiredService<IRpcClient<IAppCrudService>>();
            string testKey = nameof(testKey);
            string testValue = nameof(testValue);

            //add a non-existent key;
            var app = new App { Name = "Test App Name" };
            var result = client.Service.Add(app);
            Assert.IsTrue(result > 0);

            //add a key that already exists
            app.Id = result;
            result = client.Service.Add(app);
            Assert.AreEqual(0, result);

            //get a key that already exists
            var actualValue = client.Service.GetFirst(app.Id);
            Assert.IsNotNull(actualValue);
            Assert.AreEqual(app.Id, actualValue.Id);
            Assert.AreEqual(app.ClientSecret, actualValue.ClientSecret);
            Assert.AreEqual(app.ClientId, actualValue.ClientId);
            Assert.AreEqual(app.Name, actualValue.Name);

            //get a key that does not exists
            actualValue = client.Service.GetFirst(app.Id + 1);
            Assert.IsNull(actualValue);


            //add or update a key that already exists
            app.ClientSecret = nameof(app.ClientSecret);
            app.ClientId = nameof(app.ClientId);
            var boolResult = client.Service.Update(app);
            Assert.IsTrue(boolResult);
            //get the updated a key, ignore case
            actualValue = client.Service.GetFirst(app.Id);
            Assert.IsNotNull(actualValue);
            Assert.AreEqual(app.Id, actualValue.Id);
            Assert.AreEqual(app.ClientSecret, actualValue.ClientSecret);
            Assert.AreEqual(app.ClientId, actualValue.ClientId);
            Assert.AreEqual(app.Name, actualValue.Name);
            //add or update a key that does not exists
            var listItems = client.Service.GetAll();
            Assert.IsTrue(listItems.Any());
            Assert.IsTrue(listItems.Any(x => x.Id == app.Id));

            //remove a key that does not exist
            var items = client.Service.GetByPage();
            Assert.IsTrue(items.Any());
            Assert.IsTrue(items.Any(x => x.Id == app.Id));

            //items = client.Service.GetByIds(new[] { app.Id });
            //Assert.IsTrue(items.Any());
            //Assert.IsTrue(items.Any(x => x.Id == app.Id));

            //remove a key that does exists, ignoring case
            boolResult = client.Service.Delete(app.Id);
            Assert.IsTrue(boolResult);

            actualValue = client.Service.GetFirst(app.Id);
            Assert.IsNull(actualValue);

        }

        [TestMethod]
        public void TestDotRpcTestGenericAppCrudService()
        {
            var client = serviceProvider.GetRequiredService<IRpcClient<IIntEntityCrudService<App>>>();
            string testKey = nameof(testKey);
            string testValue = nameof(testValue);

            //add a non-existent key;
            var app = new App { Name = "Test App Name" };
            var result = client.Service.Add(app);
            Assert.IsTrue(result > 0);

            //add a key that already exists
            app.Id = result;
            result = client.Service.Add(app);
            Assert.AreEqual(0, result);

            //get a key that already exists
            var actualValue = client.Service.GetFirst(app.Id);
            Assert.IsNotNull(actualValue);
            Assert.AreEqual(app.Id, actualValue.Id);
            Assert.AreEqual(app.ClientSecret, actualValue.ClientSecret);
            Assert.AreEqual(app.ClientId, actualValue.ClientId);
            Assert.AreEqual(app.Name, actualValue.Name);

            //get a key that does not exists
            actualValue = client.Service.GetFirst(app.Id + 1);
            Assert.IsNull(actualValue);


            //add or update a key that already exists
            app.ClientSecret = nameof(app.ClientSecret);
            app.ClientId = nameof(app.ClientId);
            var boolResult = client.Service.Update(app);
            Assert.IsTrue(boolResult);
            //get the updated a key, ignore case
            actualValue = client.Service.GetFirst(app.Id);
            Assert.IsNotNull(actualValue);
            Assert.AreEqual(app.Id, actualValue.Id);
            Assert.AreEqual(app.ClientSecret, actualValue.ClientSecret);
            Assert.AreEqual(app.ClientId, actualValue.ClientId);
            Assert.AreEqual(app.Name, actualValue.Name);
            //add or update a key that does not exists
            var listItems = client.Service.GetAll();
            Assert.IsTrue(listItems.Any());
            Assert.IsTrue(listItems.Any(x => x.Id == app.Id));

            //remove a key that does not exist
            var items = client.Service.GetByPage();
            Assert.IsTrue(items.Any());
            Assert.IsTrue(items.Any(x => x.Id == app.Id));

            //items = client.Service.GetByIds(new[] { app.Id });
            //Assert.IsTrue(items.Any());
            //Assert.IsTrue(items.Any(x => x.Id == app.Id));

            //remove a key that does exists, ignoring case
            boolResult = client.Service.Delete(app.Id);
            Assert.IsTrue(boolResult);

            actualValue = client.Service.GetFirst(app.Id);
            Assert.IsNull(actualValue);

        }

    }
}
