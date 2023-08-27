using Humanizer;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Text;

namespace DotRpc
{

    public class NameService : INameService, ICSharpDeclarationProvider
    {
        public static ConcurrentDictionary<Type, string> SwaggerSchemaIds = new();
        public static ConcurrentDictionary<string, Type> SwaggerSchemaNames = new();

        public ICSharpDeclarationProvider CSharpDeclarationProvider { get; set; }

        public NameService()
        {
            this.CSharpDeclarationProvider = new CSharpDeclarationProvider();

        }
        public string ToPropertyName(string name)
        {
            var pascalTokens = GetPascalTokens(name);
            bool useTokens = bool.Parse(bool.TrueString);
            if (useTokens)
            {
                var result = string.Join("", pascalTokens.Where(x => x.Length != 1 || char.IsLetterOrDigit(x[0])));
                return result;
            }
            else
            {

                if (name.Length > 1 && name[0] == 'I' && Char.IsUpper(name[1]))
                    name = name.Substring(1);
                var humanized = name.Humanize();
                var result = humanized.Dehumanize().ToPascalCase();
                return result;
            }

        }

        public string GetSwaggerSchemaId(Type type)
        {
            return SwaggerSchemaIds.GetOrAdd(type, x => GenerateSwaggerSchemaId(type));
        }

        public string GenerateSwaggerTag(Type type)
        {
            return type.GetRpcContract().Name;
        }

        public string GenerateSwaggerSchemaId(Type type)
        {
            if (SwaggerSchemaIds.TryGetValue(type, out string? typeName))
                return typeName;



            var dec = ToCSharpDeclaration(type);
            typeName = ToPropertyName(dec);
            if (SwaggerSchemaNames.TryAdd(typeName, type)) return typeName;
            else if (SwaggerSchemaNames[typeName] == type) return typeName;



            dec = ToCSharpDeclaration(type, true);
            typeName = ToPropertyName(dec);
            if (SwaggerSchemaNames.TryAdd(typeName, type)) return typeName;
            else if (SwaggerSchemaNames[typeName] == type) return typeName;


            dec = ToCSharpDeclaration(type, true, true);
            typeName = ToPropertyName(dec);
            if (SwaggerSchemaNames.TryAdd(typeName, type)) return typeName;
            else if (SwaggerSchemaNames[typeName] == type) return typeName;


            var att = type.GetRpcContract(true);//.GetCustomAttribute<RpcServiceAttribute>();
            if (att is not null && !string.IsNullOrEmpty(att.Name) && !string.IsNullOrEmpty(att.Namespace))
            {
                typeName = $"{att?.Namespace}.{att?.Name}.{typeName}";
                dec = ToCSharpDeclaration(type, true, true);
                if (SwaggerSchemaNames.TryAdd(typeName, type)) return typeName;
                else if (SwaggerSchemaNames[typeName] == type) return typeName;
            }
 
            int count = 1;
            while (true)
            {
                var uniqueTypeName = $"{typeName}{count}";
                if (SwaggerSchemaNames.TryAdd(uniqueTypeName, type)) return uniqueTypeName;
                else if (SwaggerSchemaNames[uniqueTypeName] == type) return uniqueTypeName;
                count++;
            }

        }

        public string GenerateRpcMethodName(MethodTypeDescription method)
        {
            var contractTypeName = GetApiPathName(method.method) + "Contract";
            var type = method.method.DeclaringType;
            var att = type.GetRpcContract(true);//.GetCustomAttribute<RpcServiceAttribute>();
            var result = $"{att?.Namespace}.{att?.Name}.{contractTypeName}";
            if (att == null)
            {
                var name = ToPropertyName(type.Name);
                var ns = ToPropertyName(Path.GetFileNameWithoutExtension((Assembly.GetEntryAssembly() ?? typeof(RpcServiceAttribute).Assembly).ManifestModule.Name));
                result = $"{ns}.{name}.{contractTypeName}";
            }
            return result;
        }


        List<string> Tokenize(string value)
        {
            var l = new List<string>();
            var sb = new StringBuilder();
            char last = char.MinValue;
            char current = char.MinValue;
            for (var i = 0; i < value.Length; i++)
            {

                current = value[i];
                bool isWhiteSpace = char.IsWhiteSpace(current);
                if (!char.IsLetterOrDigit(current))
                {
                    if (sb.Length > 0)
                        l.Add(sb.ToString());
                    sb.Clear();
                    if (!isWhiteSpace)
                        l.Add(current.ToString());

                }
                else
                {
                    sb.Append(current);
                }
                if (!isWhiteSpace)
                    last = current;
            }
            l.Add(sb.ToString());

            return l;
        }

        List<string> GetPascalTokens(string value)
        {
            var l = Tokenize(value);
            var pascalTokens = l.SelectMany(x =>
            {
                if (x.Length == 1)
                    if (!char.IsLetterOrDigit(x[0]))
                        return new[] { x };

                var tokens = x.Humanize(LetterCasing.Title).Split(' ');
                if (tokens.Length > 1)
                {
                    return tokens.Select(x => ToPascalCase(x)).ToArray();
                }
                else
                {
                    var token = tokens[0];

                    token = token.ToLower().Humanize(LetterCasing.Title);
                    if (token.Length > 1 && token[0] == 'I' && char.IsUpper(token[1]))
                    {
                        var result = $"I{token.Substring(1, 1).ToUpper()}";
                        if (token.Length > 2) result = $"{result}{token.Substring(2)}";
                        return new[] { result };
                    }
                    else return new[] { token };
                }

            }).ToList();
            return pascalTokens;
        }

        public string ToPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2)
                return value;

            var originalvalue = value;
            value = value.Trim();
            if (value.IndexOf(' ') > -1)
            {
                var wordTokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var simpleResult = string.Join(' ', wordTokens.Select(x => ToPascalCase(x)));
                return simpleResult;
            }

            var pascalTokens = GetPascalTokens(value);

            if (pascalTokens.Count == 1)
                return pascalTokens.First();

            var nonEmptyTokens = pascalTokens.Where(x => !string.IsNullOrWhiteSpace(x));
            bool useJoin = bool.Parse(bool.TrueString);
            if (useJoin)
            {

            }
            var result = string.Join("", nonEmptyTokens);
            return result;

        }

        public string GetApiPathName(MethodInfo method)
        {
            var methodName = CleanName(method.Name);
            var argNames = method.GetParameters().Select(x => GetParameterName(x)).ToArray();
            var args = string.Join("", argNames);
            return $"{methodName}With{args}";
        }

        public string GetParameterName(ParameterInfo x)
        {
            var result = x.Name;
            if (string.IsNullOrEmpty(result))
                throw new ArgumentException("Parameter missing name");
            return ToPropertyName(result);
        }

        public string CleanName(string name)
            => ToPropertyName(name);

        public string CleanName(Type type)
        {
            var name = CleanName(type.Name);

            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                foreach (var genericArg in genericArgs)
                {
                    name = $"{name}_{CleanName(genericArg)}";

                }
            }

            return ToPropertyName(name);
        }

        public string ToPropertyName(Assembly assembly)
        {
            return ToPropertyName(Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name));
        }

        public string ToPropertyName(Type type)
        {
            return ToPropertyName(type.Name);
        }

        public string ToPropertyName(MethodInfo method)
        {
            return ToPropertyName(method.Name);
        }

        public string ToPropertyName(PropertyInfo property)
        {
            return ToPropertyName(property.Name);
        }

        public string ToPropertyName(MethodTypeDescription methodDescription)
        {
            return ToPropertyName(methodDescription.method.Name);
        }

        public string ToCSharpDeclaration(Type type, bool includeNamespace = false, bool includeAssemblyName = false)
        {
            return CSharpDeclarationProvider.ToCSharpDeclaration(type, includeNamespace, includeAssemblyName);
        }
    }

    public interface ICSharpDeclarationProvider
    {
        string ToCSharpDeclaration(Type type, bool includeNamespace = false, bool includeAssemblyName = false);
    }
    public class CSharpDeclarationProvider : ICSharpDeclarationProvider
    {


        public Lazy<INameService> nameService { get; }

        public string ToCSharpDeclaration(Type type, bool includeNamespace = false, bool includeAssemblyName = false)
        {
            string typeName = GetTypeName(type, includeNamespace);
            if (includeAssemblyName)
                typeName = $"{type.Assembly.ToPropertyName()}.{typeName}";

            return $"{typeName}";
        }

        private string GetTypeName(Type type, bool includeNamespace)
        {
            string typeName = includeNamespace ? type.FullName : type.Name;

            if (type.IsGenericType)
            {
                string genericArguments = string.Join(", ", type.GetGenericArguments().Select(t => GetTypeName(t, includeNamespace)));
                typeName = $"{typeName.Substring(0, typeName.IndexOf('`'))}<{genericArguments}>";
            }

            return typeName;
        }
    }

}