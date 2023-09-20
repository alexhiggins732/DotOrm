using System.Reflection;

namespace DotRpc
{
    [RpcService]
    public interface INameService: ICSharpDeclarationProvider
    {
        string CleanName(string name);
        string CleanName(Type type);
        string GenerateRpcMethodName(MethodTypeDescription method);
        string GenerateSwaggerSchemaId(Type type);
        string GenerateSwaggerTag(Type type);
        string GetApiPathName(MethodInfo method);
        string GetSwaggerSchemaId(Type type);
        string ToPascalCase(string value);
        string ToPropertyName(string name);
        string ToPropertyName(Assembly name);
        string ToPropertyName(Type name);
        string ToPropertyName(PropertyInfo name);
        string ToPropertyName(MethodInfo name);
        string ToPropertyName(MethodTypeDescription name);
        string GetParameterName(ParameterInfo x);
    }

}