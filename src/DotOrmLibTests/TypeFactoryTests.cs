using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler;

namespace DotRpc.Tests
{
    [TestClass()]
    public class TypeFactoryTests
    {
        [TestMethod()]
        public void GetMethodProxyTypeTest()
        {
            Func<int, string, Dictionary<string, object>, MyModel>
                factory = (Id, Name, Parameters) => GetMyModel(Id, Name, Parameters);

            var type = TypeFactory.GetMethodProxyType(factory.Method);
            Assert.IsNotNull(type);

            var actualId = 1;
            string actualName = nameof(actualName);
            var actualParams = new Dictionary<string, object> { { nameof(actualId), actualId },{ nameof(actualName), actualName } };
            var actual = GetMyModel(actualId, actualName, actualParams);
            var srcJson = JsonConvert.SerializeObject(actual, Formatting.Indented);

            var expectedFromJson = JsonConvert.DeserializeObject<MyModel>(srcJson);
            var dynamicFromJson = JsonConvert.DeserializeObject(srcJson, type);

            Assert.IsNotNull(expectedFromJson);
            Assert.IsNotNull(dynamicFromJson);

            var expectedToJson = JsonConvert.SerializeObject(expectedFromJson, Formatting.Indented);
            var actualFromJson = JsonConvert.SerializeObject(dynamicFromJson, Formatting.Indented);
  
            Assert.AreEqual(expectedToJson, actualFromJson);
        }

        string GetSourceCode(Type type)
        {
            DecompilerSettings settings = new DecompilerSettings();

            // Create CSharpDecompiler instance
           // CSharpDecompiler decompiler = new CSharpDecompiler(type.Assembly.ManifestModule, settings);

            // Decompile the assembly
            StringWriter output = new StringWriter();
            //decompiler.DecompileType(new();

            // Print the decompiled C# code
            var result = output.ToString();
            Console.WriteLine(result);
            return result;
        }

        public MyModel GetMyModel(int id, string name, Dictionary<string, object> parameters)
        {
            return new MyModel(id, name, parameters);
        }

        [TestMethod()]
        public void GetPayloadTest()
        {
            Assert.Fail();
        }
    }

    public class MyModel
    {
        public MyModel(int id, string name, Dictionary<string, dynamic> parameters)
        {
            Id = id;
            Name = name;
            Parameters = parameters;
        }
        [JsonPropertyOrder(1)]
        [JsonProperty(Order = 1)]
        public int Id { get; }
        public string Name { get; }
        public Dictionary<string, dynamic> Parameters { get; }
    }
}