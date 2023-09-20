
using DotRpc.Tests.ProxyGeneratorTestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace DotRpc.Tests
{
    [TestClass]
    public class ProxyGeneratorTests
    {

    }

    [TestClass]
    public class OpenRpcDefinitionTests
    {
        string root = "";

        public ServiceProvider ServiceProvider { get; }

        public OpenRpcDefinitionTests()
        {
            root = GetTestModelRoot();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IRpcProxyGenerator, RpcProxyGenerator>();
            services.AddSingleton<IRpcTypeFactory, RpcTypeFactory>();

            this.ServiceProvider = services.BuildServiceProvider();

        }

        private string GetTestModelRoot()
        {
            var path = Path.GetFullPath(".");
            var di = new DirectoryInfo(path);
            while (di != null && di.Parent != null && di.Name != "DotRpcTests")
            {
                di = di.Parent;
            }
            if (di == null)
                throw new Exception("Failed to find project root");
            path = Path.Combine(di.FullName, "ProxyGeneratorTestModels");
            return path;
        }

        private string GetModelCode(string fileName)
        {
            var path = Path.Combine(root, fileName);
            return File.ReadAllText(path);
        }

        [TestMethod]
        public void PlainPocoTest()
        {
            var srcType = typeof(PlainPoco);
            var t = OpenRpcSchema.Create(srcType);
            var actual = OpenRpcSchema.ToCSharp(t);
            var expected = GetModelCode("PlainPoco.cs");
            Assert.AreEqual(expected, actual);

            var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
            var example = RpcProxyGenerator.EmitExampleProxy(logger, srcType);
            var actualInstance = Activator.CreateInstance(example);
            Assert.IsNotNull(actualInstance);
            var actualJson = System.Text.Json.JsonSerializer.Serialize(actualInstance);
            var expectedInstance = Activator.CreateInstance(srcType);
            var expectedJson = System.Text.Json.JsonSerializer.Serialize(expectedInstance);
            Assert.IsNotNull(expectedInstance);
            Assert.AreEqual(expectedJson, actualJson);

        }

        [TestMethod]
        public void GenericPocoTest()
        {
            var srcType = typeof(GenericPoco<,>);
            var t = OpenRpcSchema.Create(srcType);
            var actual = OpenRpcSchema.ToCSharp(t);
            var expected = GetModelCode("GenericPoco.cs");
            Assert.AreEqual(expected, actual);


            var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
            var example = RpcProxyGenerator.EmitExampleProxy(logger, srcType);
            var actualInstance = Activator.CreateInstance(example);
            Assert.IsNotNull(actualInstance);
            var actualJson = System.Text.Json.JsonSerializer.Serialize(actualInstance);
            var expectedType = srcType.MakeGenericType(typeof(string), typeof(string));
            var expectedInstance = Activator.CreateInstance(expectedType);
            var expectedJson = System.Text.Json.JsonSerializer.Serialize(expectedInstance);
            Assert.IsNotNull(expectedInstance);
            Assert.AreEqual(expectedJson, actualJson);
        }

        [TestMethod]
        public void NestedGenericPocoTest()
        {
            var srcType = typeof(GenericPoco<,>);
            var t = OpenRpcSchema.Create(srcType);
            var actual = OpenRpcSchema.ToCSharp(t);
            var expected = GetModelCode("GenericPoco.cs");
            Assert.AreEqual(expected, actual);

            var apiRequestType = typeof(ApiRequest<>);
            var genericSrcType = srcType.MakeGenericType(apiRequestType, apiRequestType);
            var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
            var actualGeneric = RpcProxyGenerator.EmitExampleProxy(logger, genericSrcType);
            var actualInstance = Activator.CreateInstance(actualGeneric);
            Assert.IsNotNull(actualInstance);
            var actualJson = System.Text.Json.JsonSerializer.Serialize(actualInstance);
            var expectedGenericType = apiRequestType.MakeGenericType(typeof(string));
            var expectedType = srcType.MakeGenericType(expectedGenericType, expectedGenericType);
            var expectedInstance = Activator.CreateInstance(expectedType);
            var expectedJson = System.Text.Json.JsonSerializer.Serialize(expectedInstance);
            Assert.IsNotNull(expectedInstance);
            Assert.AreEqual(expectedJson, actualJson);
        }




        [TestMethod]
        public void ApiRequestTest()
        {

            var t = OpenRpcSchema.Create(typeof(ApiRequest<>));
            var actual = OpenRpcSchema.ToCSharp(t);
            var expected = GetModelCode("ApiRequest.cs");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ApiResponseTest()
        {

            var t = OpenRpcSchema.Create(typeof(ApiResponse<>));
            var actual = OpenRpcSchema.ToCSharp(t);
            var expected = GetModelCode("ApiResponse.cs");
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void IPlainPocoService()
        {

            var t = OpenRpcSchema.Create(typeof(IPlainPocoService));
            var actual = OpenRpcSchema.ToCSharp(t);
            var expected = GetModelCode("IPlainPocoService.cs");
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void IGenericPocoService()
        {

            var t = OpenRpcSchema.Create(typeof(IGenericPocoService<,>));
            var actual = OpenRpcSchema.ToCSharp(t);
            var expected = GetModelCode("IGenericPocoService.cs");
            Assert.AreEqual(expected, actual);
        }



        [TestMethod]
        public void IGenericPocoServiceWithNestedGenerics()
        {

            var t = OpenRpcSchema.Create(typeof(IGenericPocoServiceWithNestedGenerics<,>));
            var actual = OpenRpcSchema.ToCSharp(t);
            var expected = GetModelCode("IGenericPocoServiceWithNestedGenerics.cs");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IGenericPocoServiceWithMixedAndNestedGenerics()
        {

            var t = OpenRpcSchema.Create(typeof(IGenericPocoServiceWithMixedAndNestedGenerics<>));
            var actual = OpenRpcSchema.ToCSharp(t);
            var expected = GetModelCode("IGenericPocoServiceWithMixedAndNestedGenerics.cs");
            Assert.AreEqual(expected, actual);
        }




    }

    public class IndentedTextWriter : TextWriter
    {
        public StringBuilder StringBuilder { get; }

        private TextWriter writer;
        private StringBuilder indentBuilder;
        private string currentIndent;

        public int Indent { get; set; }


        public override Encoding Encoding => writer.Encoding;

        public IndentedTextWriter(out StringBuilder sb, int indent = 4, string newLine = "\r\n")
        {
            this.StringBuilder = sb = new StringBuilder();
            this.writer = new StringWriter(sb);
            this.Indent = indent;
            this.NewLine = newLine;
            this.indentBuilder = new StringBuilder();
            this.currentIndent = string.Empty;
        }


        public override void Write(string? value)
        {
            string[] lines = value.Split(new[] { NewLine }, StringSplitOptions.None);

            for (var i = 0; i < lines.Length; i++)
            {
                if (!wroteIndent)
                {
                    WriteIndent();
                    wroteIndent = true;
                }
                writer.Write(value);
                if (i < lines.Length - 1)
                {
                    writer.Write(NewLine);
                    wroteIndent = false;
                }
            }

        }

        public void WriteIndent()
        {
            currentIndent = indentBuilder.ToString();
            writer.Write(currentIndent);
        }

        public void IncreaseIndent()
        {
            indentBuilder.Append(' ', Indent);
            currentIndent = indentBuilder.ToString();
        }

        public void DecreaseIndent()
        {
            if (indentBuilder.Length >= Indent)
            {
                indentBuilder.Remove(indentBuilder.Length - Indent, Indent);
                currentIndent = indentBuilder.ToString();
            }
        }

        bool wroteIndent = false;
        public override void WriteLine()
        {
            if (!wroteIndent)
            {
                WriteIndent();
                wroteIndent = true;
            }
            writer.Write(NewLine);
            wroteIndent = false;
        }

        public override void WriteLine(string? value)
        {
            Write(value);
            WriteLine();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                writer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    public class OpenRpcSchema
    {
        public static CSharpDeclarationProvider DeclarationProvider = new();
        //public static LockingConcurrentDictionary<Type, OpenRpcTypeDefinition> cache = new(CreateDefinition);
        public static ConcurrentDictionary<Type, OpenRpcTypeDefinition> cache = new ConcurrentDictionary<Type, OpenRpcTypeDefinition>();
        internal static OpenRpcTypeDefinition Create(Type type)
        {
            if (!cache.TryGetValue(type, out var result))
            {
                result = cache.GetOrAdd(type, CreateDefinition);
            }
            return result;
        }
        internal static OpenRpcTypeDefinition CreateDefinition(Type type)
        {


            var result = new OpenRpcTypeDefinition();
            result.ClrType = type;
            result.Name = TypeNameService.GetTypeAlias(type.Name);
            result.Namespace = type.Namespace;
            result.Assembly = Path.GetFileNameWithoutExtension(type.Assembly.ManifestModule.Name);
            result.Kind = type.IsValueType ? TypeKind.Struct : type.IsEnum ? TypeKind.Enum : type.IsInterface ? TypeKind.Interface : TypeKind.Class;
            if (type.IsGenericType)
            {
                result.IsGeneric = true;
                result.GenericArguments = type.GetGenericArguments()
                    .Select(Create) // Recursively create definitions for generic arguments
                    .ToList();
            }

            if (type.IsEnum)
            {
                //result.EnumValues = type.GetEnumNames()
                //    .Select(name => new OpenRpcTypeDefinition { Name = name })
                //    .ToList();

                result.EnumValues = GetEnumValues(type);


            }

            if (!IsPrimitiveTypeOrStringSerializable(type))
            {
                var typeProps = type.GetProperties().ToList();
                result.Properties = typeProps
                    .Select(prop => new OpenRpcTypeProperty
                    {
                        Name = prop.Name,
                        Type = Create(prop.PropertyType),
                        IsReadOnly = !prop.CanWrite,
                        IsRequired = prop.CustomAttributes.Any(attr => attr.AttributeType == typeof(RequiredAttribute))
                    })
                    .ToList();

                var interfaces = type.GetInterfaces().ToList();
                result.Interfaces = interfaces
                    .Select(Create) // Recursively create definitions for implemented interfaces
                    .ToList();

                //var attributes = type.GetCustomAttributesData().ToList()
                //    .Where(x => x.AttributeType.Name.IndexOf("Nullable") == -1).ToList();
                //result.Attributes = attributes
                //    .Select(attrData => Create(attrData.AttributeType))
                //    .ToList();

                var methods = type.GetMethods().ToList();
                if (type.IsInterface && methods.Count > 0)
                {
                    result.Methods = new();
                    foreach (var method in methods)
                    {
                        //Console.WriteLine($"Creating method for {method.ReturnType} {method.Name}");
                        var schemaParameters = new List<OpenRpcTypeProperty>();
                        var methodParameters = method.GetParameters().ToList();
                        foreach (var methodParameter in methodParameters)
                        {
                            //Console.WriteLine($"Creating property for {methodParameter.ParameterType} {methodParameter.Name}");
                            var prop = new OpenRpcTypeProperty
                            {
                                Name = methodParameter.Name,
                                Type = Create(methodParameter.ParameterType),
                                IsRequired = !methodParameter.IsOptional
                            };
                            schemaParameters.Add(prop);
                        }

                        var def = new OpenRpcMethodDefinition
                        {
                            Name = method.Name,
                            ReturnType = Create(method.ReturnType),
                            Parameters = schemaParameters
                        };
                        def.GenericArguments = method.GetGenericArguments()
                            .Select(Create) // Recursively create definitions for generic arguments
                            .ToList();
                        def.ClrMethod = method;
                        result.Methods.Add(def);
                    }
                }

                // If the type has a base type, create a definition for it
                if (type.BaseType != null && type.BaseType != typeof(object))
                {
                    result.BaseType = Create(type.BaseType);
                }
            }
            return result;
        }

        private static bool IsPrimitiveTypeOrStringSerializable(Type type)
        {
            if (type.IsPrimitive || type == typeof(string))
            {
                return true; // It's a primitive type or string
            }

            // Check if the type has a TypeConverter that supports string serialization/deserialization
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            return converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string));
        }

        private static List<OpenRpcEnumValue> GetEnumValues(Type enumType)
        {
            var enumValues = Enum.GetValues(enumType);
            var enumValueList = new List<OpenRpcEnumValue>();

            foreach (var enumValue in enumValues)
            {
                var valueName = enumValue.ToString();
                var enumValueDefinition = new OpenRpcEnumValue
                {
                    Name = valueName,
                    Value = (int)enumValue // You can cast this to nullable if needed
                };

                enumValueList.Add(enumValueDefinition);
            }

            return enumValueList;
        }

        internal static string ToCSharp(OpenRpcTypeDefinition type, bool generateDefaultCtor = false)
        {

            using var writer = new IndentedTextWriter(out var sb);

            GenerateTypeDeclaration(writer, type);

            writer.IncreaseIndent();
            if (type.IsEnum)
            {
                GenerateEnumMembers(writer, type.EnumValues);
            }
            else
            {
                if (type.Kind == TypeKind.Interface)
                    GenerateInterfaceMembers(writer, type);
                else
                {
                    GenerateProperties(writer, type.Properties);
                    if (generateDefaultCtor)
                        GenerateConstructors(writer, type);
                }

            }
            writer.DecreaseIndent();
            writer.WriteLine("}");
            writer.DecreaseIndent();
            writer.Write("}");

            var result = sb.ToString();
            return result;
        }


        private static void GenerateTypeDeclaration(IndentedTextWriter writer, OpenRpcTypeDefinition type)
        {
            writer.WriteLine($"namespace {type.Namespace}");
            writer.WriteLine("{");
            writer.IncreaseIndent();
            var typeName = type.IsGeneric ? type.Name.Split('`')[0] : type.Name;
            writer.Write($"public {(type.Kind.ToString().ToLower())} {typeName}");

            if (type.IsGeneric)
            {
                writer.Write("<");
                writer.Write(string.Join(", ", type.GenericArguments.Select(arg => DeclarationProvider.ToCSharpDeclaration(arg.ClrType, useAlias: true))));
                writer.Write(">");

            }
            if (type.BaseType != null)
            {
                writer.WriteLine();
                writer.Write($" : {type.BaseType.Name}");
            }

            if (type.Interfaces.Any())
            {
                writer.WriteLine();
                writer.Write(" : ");
                writer.Write(string.Join(", ", type.Interfaces.Select(iface => iface.Name)));
            }
            writer.WriteLine();
            writer.WriteLine("{");

        }

        private static void GenerateEnumMembers(IndentedTextWriter writer, List<OpenRpcEnumValue> enumValues)
        {
            for (int i = 0; i < enumValues.Count; i++)
            {
                writer.WriteLine($"{enumValues[i].Name} = {enumValues[i].Value},");
            }
        }

        private static void GenerateProperties(IndentedTextWriter writer, List<OpenRpcTypeProperty> properties)
        {
            foreach (var property in properties)
            {
                writer.WriteLine($"public {property.Type.Name} {property.Name} {{ get; set; }}");
            }
        }


        private static void GenerateInterfaceMembers(IndentedTextWriter writer, OpenRpcTypeDefinition type)
        {
            if (type == null || type.Properties == null)
            {
                return;
            }

            foreach (var method in type.Methods)
            {

                var decl = DeclarationProvider.ToCSharpDeclaration(method.ReturnType.ClrType, useAlias: true);
                writer.Write($"{decl} {method.Name}");
                if (method.ClrMethod.IsGenericMethod)
                {
                    writer.Write("<");
                    writer.Write(string.Join(", ", method.ClrMethod.GetGenericArguments().Select(arg => DeclarationProvider.ToCSharpDeclaration(arg, useAlias: true))));
                    writer.Write(">");
                }
                writer.Write("(");
                // Generate method parameters
                if (method.Parameters != null)
                {
                    for (int i = 0; i < method.Parameters.Count; i++)
                    {
                        var parameter = method.Parameters[i];
                        var parameterDeclaration = DeclarationProvider.ToCSharpDeclaration(parameter.Type.ClrType, useAlias: true);
                        writer.Write($"{parameterDeclaration} {parameter.Name}");

                        if (i < method.Parameters.Count - 1)
                        {
                            writer.Write(", ");
                        }
                    }
                }

                writer.WriteLine(");");

            }
        }




        private static void GenerateConstructors(IndentedTextWriter writer, OpenRpcTypeDefinition type)
        {
            writer.WriteLine($"public {type.Name}() {{ }}");
        }


    }

    public enum TypeKind
    {
        Class,
        Struct,
        Enum,
        Interface
    }
    public class OpenRpcTypeDefinition
    {
        //public static OpenRpcTypeDefinition Object = new OpenRpcTypeDefinition() { Assembly = "System", Namespace = "System", Name = "Object" };
        public OpenRpcTypeDefinition()
        {
            Assembly = Name = Namespace = string.Empty;
            GenericArguments = new();
            Properties = new();
            //BaseType = Object;
            Interfaces = new();
            Attributes = new();
            NestedTypes = new();
            EnumValues = new();
            Methods = new();
        }
        public string Assembly { get; set; }
        public TypeKind Kind { get; set; } // Kind of type (class, struct, enum)
        public string Name { get; set; } // Name of the type
        public string Namespace { get; set; } // Namespace or module of the type (optional)
        public bool IsGeneric { get; set; } // Indicates if the type is generic
        public List<OpenRpcTypeDefinition> GenericArguments { get; set; } // Generic type arguments (if generic)
        public List<OpenRpcTypeProperty> Properties { get; set; } // Properties of the type
        public List<OpenRpcMethodDefinition> Methods { get; set; } // Properties of the type
        public OpenRpcTypeDefinition BaseType { get; set; } // Base type (for inheritance)
        public List<OpenRpcTypeDefinition> Interfaces { get; set; } // Implemented interfaces
        public List<OpenRpcTypeDefinition> Attributes { get; set; } // Implemented attributes
        public List<OpenRpcTypeDefinition> NestedTypes { get; set; } // Nested types (if any)
        //public List<OpenRpcTypeDefinition> EnumValues { get; set; } // Enum values (if an enum)
        public List<OpenRpcEnumValue> EnumValues { get; set; }
        public bool IsEnum => EnumValues != null && EnumValues.Any();

        public Type ClrType { get; internal set; }
    }

    public class OpenRpcMethodDefinition
    {
        public string Name { get; set; } // Method name
        public OpenRpcTypeDefinition ReturnType { get; set; } // Return type
        public List<OpenRpcTypeProperty> Parameters { get; set; } // Method parameters
        public List<OpenRpcTypeDefinition> GenericArguments { get; set; } // Generic type arguments (if generic)
        public MethodInfo ClrMethod { get; internal set; }
    }

    public class OpenRpcTypeProperty
    {
        public string Name { get; set; } // Property name
        public OpenRpcTypeDefinition Type { get; set; } // Property type
        public bool IsReadOnly { get; set; } // Indicates if the property is read-only
        public bool IsRequired { get; set; } // Indicates if the property is required
    }

    public class OpenRpcEnumValue : OpenRpcTypeDefinition
    {
        public int? Value { get; set; } // Enum value numeric value (optional)
    }

    public class OpenRpcConstantValue
    {
        public string Value { get; set; }
    }
}
