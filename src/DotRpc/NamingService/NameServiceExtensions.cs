using System.Reflection;

namespace DotRpc
{
    public static class NameServiceExtensions
    {
        public static INameService Instance = new NameService();

        public static string CleanName(this string name)
        {
            return Instance.CleanName(name);
        }

        public static string CleanName(this Type type)
        {
            return Instance.CleanName(type);
        }

        public static string GenerateRpcMethodName(this MethodTypeDescription method)
        {
            return Instance.GenerateRpcMethodName(method);
        }

        public static string GenerateSwaggerSchemaId(this Type type)
        {
            return Instance.GenerateSwaggerSchemaId(type);
        }

        public static string GenerateSwaggerTag(this Type type)
        {
            return Instance.GenerateSwaggerTag(type);
        }

        public static string GetApiPathName(this MethodInfo method)
        {
            return Instance.GetApiPathName(method);
        }

        public static string GetSwaggerSchemaId(this Type type)
        {
            return Instance.GetSwaggerSchemaId(type);
        }

        public static string ToPascalCase(this string value)
        {
            return Instance.ToPascalCase(value);
        }

        public static string ToPropertyName(this string name)
        {
            return Instance.ToPropertyName(name);
        }
        public static string ToPropertyName(this Assembly name)
        {
            return Path.GetFileNameWithoutExtension(name.ManifestModule.Name).ToPropertyName();
        }
        public static string ToPropertyName(Type type)
        {
            return Instance.ToPropertyName(type);
        }

        public static string ToPropertyName(MethodInfo method)
        {
            return Instance.ToPropertyName(method);
        }
        public static string ToPropertyName(PropertyInfo property)
        {
            return Instance.ToPropertyName(property);
        }
        public static string ToPropertyName(MethodTypeDescription methodDescription)
        {
            return Instance.ToPropertyName(methodDescription);
        }
    }

}