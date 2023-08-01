using Dapper;
using DotOrmLib;
using DotOrmLib.Proxy;
using DotOrmLib.Sql;
using System.Reflection.Emit;
using FlowControl = DotOrmLib.Proxy.ILRuntime.FlowControl;
using TestModel = DotOrmLib.Proxy.ILRuntime.TestModel;
namespace DotOrm
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var cn = ConnectionStringProvider.Create();
            Console.WriteLine("Hello, World!");
            ScaffoldDb("Scc1");
            await TestDynamic();
            //
        }

        private static void ScaffoldDb(string dbName)
        {
            var conn = ConnectionStringProvider.Create(dbName).ConnectionString;


            var def = DotOrmLib.Sql.ModelBuilder.GetDbSchema(dbName);


            var modelDirectory = GetModelDirectory();
            var modelClassFilePath = Path.Combine(modelDirectory.FullName, $"{def.ClassName}.cs");

            var code = def.BuildCsharpModels();
            Console.WriteLine(code);

            File.WriteAllText(modelClassFilePath, code);
        }
        static async Task TestDynamic()
        {
            var conn = ConnectionStringProvider.Create("ILRuntime").ConnectionString;
            var model = ModelBuilder.GetTableDefinition<DotOrmLib.Proxy.ILRuntime.TestModel>(conn);
            var whereParam = new { Name = "Branch" };
            var repo = new DotOrmRepo<DotOrmLib.Proxy.ILRuntime.TestModel>(conn);
            var result = await repo.Get(whereParam);

            var byId = await repo.GetById(result.First().Id);
            var updateCount = await repo.Update(new { Description = "Exception throw instruction" }, new { Id = 9 });
            var update = await repo.GetById(9);

            var update2Count = await repo.Update(new { Description = "Exception throw instruction." }, new { Id = 9 });

            var update2 = await repo.GetById(9);
            var get2 = await repo.Where(x => x.Id == 9).Or(x => x.Name == "Branch").ToList();
            var get3 = await repo.Where(x => (x.Id == 9 || x.Name == "Branch") && x.Description != null).ToList();
            var copy = await repo.Add(update2);
            int deleted = await repo.Delete(copy);
        }

        public static void AddDynamicParams(object param)
        {
            var obj = param;
            if (obj != null)
            {

                if (obj is DynamicParameters subDynamic)
                {
                    //{
                    //    if (subDynamic.o != null)
                    //    {
                    //        foreach (var kvp in subDynamic.parameters)
                    //        {
                    //            parameters.Add(kvp.Key, kvp.Value);
                    //        }
                    //    }

                    //    if (subDynamic.templates != null)
                    //    {
                    //        templates ??= new List<object>();
                    //        foreach (var t in subDynamic.templates)
                    //        {
                    //            templates.Add(t);
                    //        }
                    //    }
                    //}
                }
                else
                {
                    if (obj is IEnumerable<KeyValuePair<string, object>> dictionary)
                    {
                        foreach (var kvp in dictionary)
                        {
                            //Add(kvp.Key, kvp.Value, null, null, null);
                        }
                    }
                    else
                    {
                        //templates ??= new List<object>();
                        //templates.Add(obj);
                        var d = new Dictionary<string, object>();
                        var props = obj.GetType().GetProperties().ToDictionary(x => x.Name, x => new { x.PropertyType, Value = x.GetValue(obj) });
                    }
                }
            }
        }

        private static void CreateTableDef(string dbName, string tableName)
        {
            var def = DotOrmLib.Sql.ModelBuilder.GetTableDefinition(tableName);
            var code = def.GenerateCSharpModel();
            Console.WriteLine(code);

            def = DotOrmLib.Sql.ModelBuilder.GetTableDefinition("OpCode");
            code = def.GenerateCSharpModel();
            Console.WriteLine(code);

            var modelDirectory = GetModelDirectory();
            var modelClass = Path.Combine(modelDirectory.FullName, "class1.cs");
            File.WriteAllText(modelClass, code);
        }

        private static DirectoryInfo GetModelDirectory()
        {
            var di = new DirectoryInfo(".");
            var modelDirectoryName = "DotOrmTestModels";
            DirectoryInfo? GetModelDirectory()
            {
                return di.GetDirectories().FirstOrDefault(x => x.Name == modelDirectoryName);
            }
            var modelDir = GetModelDirectory();
            while (modelDir is null)
            {
                di = di.Parent;
                if (di is null)
                    throw new Exception($"Failed to find model directory: {modelDirectoryName}");
                modelDir = GetModelDirectory();
            }

            return modelDir;
        }
    }
}