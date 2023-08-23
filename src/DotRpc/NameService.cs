using Humanizer;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace DotRpc
{

    public static class NameService
    {
        public static string ToProperyName(this string name)
        {
            if (name.Length > 1 && name[0] == 'I' && Char.IsUpper(name[1]))
                name = name.Substring(1);
            var humanized = name.Humanize();
            var result = humanized.Dehumanize().ToPascalCaseWithAcronyms();
            return result;
        }

        public static string ToPascalCaseWithAcronyms(this string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2)
                return value;

            var b = new System.Text.StringBuilder();
            var last = value[0];
            b.Append(last);
            for (var i = 1; i < value.Length; i++)
            {

                var c = value[i];
                if (char.IsLower(c))
                    b.Append(c);
                else // (char.IsUpper(c))
                {
                    if (!char.IsUpper(last))
                        b.Append(c);
                    else //(char.IsUpper(last))
                    {
                        if (i + 1 >= value.Length || char.IsUpper(value[i + 1]))
                            b.Append(char.ToLower(c));
                        else
                            b.Append(c);
                    }
                }
                last = c;
            }



            return b.ToString();
        }

        public static string GetApiPathName(MethodInfo method)
        {
            var methodName = CleanName(method.Name);
            var argNames = method.GetParameters().Select(x => GetParameterName(x)).ToArray();
            var args = string.Join("", argNames);
            return $"{methodName}With{args}";
        }

        private static object GetParameterName(ParameterInfo x)
        {
            var result = x.Name;
            if (string.IsNullOrEmpty(result))
                throw new ArgumentException("Parameter missing name");
            return result.ToProperyName();
        }

        public static string CleanName(string name)
            => NameService.ToProperyName(name);

        public static string CleanName(Type type)
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
            return NameService.ToProperyName(name);
        }

        internal static string GenerateTypeName(MethodTypeDescription method)
        {
            var contractTypeName = GetApiPathName(method.method) + "Contract";
            var type = method.method.DeclaringType;
            var att = type.GetCustomAttribute<RpcServiceAttribute>();
            var result = $"{att?.Namespace}.{att?.Name}.{contractTypeName}";
            if (att == null)
            {
                var name = NameService.ToProperyName(type.Name);
                var ns  =  NameService.ToProperyName(Path.GetFileNameWithoutExtension((Assembly.GetEntryAssembly() ?? typeof(RpcServiceAttribute).Assembly).ManifestModule.Name));
                result = $"{ns}.{name}.{contractTypeName}";
            }
            return result;
         
            
        }
    }
}