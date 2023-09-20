using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace DotRpc.TestServer
{
    public partial class Server
    {
        public class CustomType : Type, IEquatable<Type?>, IEquatable<CustomType?>
        {
            public CustomType(Type srcType)
            {
                SrcType = srcType;
                metadataToken = srcType.MetadataToken;
            }

            private int metadataToken;
            public override int MetadataToken => metadataToken;
            public Type SrcType { get; }

            public override Assembly Assembly => SrcType.Assembly;

            public override string? AssemblyQualifiedName => SrcType.AssemblyQualifiedName;

            public override Type? BaseType => SrcType.BaseType;

            public override string? FullName => SrcType.FullName;

            public override Guid GUID => SrcType.GUID;

            public override Module Module => SrcType.Module;

            public override string? Namespace => SrcType.Namespace;

            public override Type UnderlyingSystemType => SrcType.UnderlyingSystemType;

            public override string Name => SrcType.Name;


            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            {
                return SrcType.GetConstructors(bindingAttr);
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return SrcType.GetCustomAttributes(inherit);
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return SrcType.GetCustomAttributes(attributeType, inherit);
            }

            public override Type? GetElementType()
            {
                return SrcType.GetElementType();
            }

            public override EventInfo? GetEvent(string name, BindingFlags bindingAttr)
            {
                return SrcType.GetEvent(name, bindingAttr);
            }

            public override EventInfo[] GetEvents(BindingFlags bindingAttr)
            {
                return SrcType.GetEvents(bindingAttr);
            }

            public override FieldInfo? GetField(string name, BindingFlags bindingAttr)
            {
                return SrcType.GetField(name, bindingAttr);
            }

            public override FieldInfo[] GetFields(BindingFlags bindingAttr)
            {
                return SrcType.GetFields(bindingAttr);
            }

            [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
            public override Type? GetInterface(string name, bool ignoreCase)
            {
                return SrcType.GetInterface(name, ignoreCase);
            }

            public override Type[] GetInterfaces()
            {
                return SrcType.GetInterfaces();
            }

            public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            {
                return SrcType.GetMembers(bindingAttr);
            }

            public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
            {
                return SrcType.GetMethods(bindingAttr);
            }

            public override Type? GetNestedType(string name, BindingFlags bindingAttr)
            {
                return SrcType.GetNestedType(name, bindingAttr);
            }

            public override Type[] GetNestedTypes(BindingFlags bindingAttr)
            {
                return SrcType.GetNestedTypes(bindingAttr);
            }

            public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            {
                return SrcType.GetProperties(bindingAttr);
            }

            public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder? binder, object? target, object?[]? args, ParameterModifier[]? modifiers, CultureInfo? culture, string[]? namedParameters)
            {
                return SrcType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return SrcType.IsDefined(attributeType, inherit);
            }

            protected override TypeAttributes GetAttributeFlagsImpl()
            {
                throw new NotImplementedException();
            }

            protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers)
            {
                throw new NotImplementedException();
            }

            protected override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers)
            {
                throw new NotImplementedException();
            }

            protected override PropertyInfo? GetPropertyImpl(string name, BindingFlags bindingAttr, Binder? binder, Type? returnType, Type[]? types, ParameterModifier[]? modifiers)
            {
                throw new NotImplementedException();
            }

            protected override bool HasElementTypeImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsArrayImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsByRefImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsCOMObjectImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsPointerImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsPrimitiveImpl()
            {
                throw new NotImplementedException();
            }

            public static bool operator ==(CustomType? left, CustomType? right)
            {
                return EqualityComparer<CustomType>.Default.Equals(left, right);
            }

            public static bool operator !=(CustomType? left, CustomType? right)
            {
                return !(left == right);
            }

            public static bool operator ==(Type? left, CustomType? right)
            {
                return EqualityComparer<Type>.Default.Equals(left, right);
            }

            public static bool operator !=(Type? left, CustomType? right)
            {
                return !(left == right);
            }

            public static bool operator ==(CustomType? left, Type? right)
            {
                return EqualityComparer<Type>.Default.Equals(left, right);
            }

            public static bool operator !=(CustomType? left, Type? right)
            {
                return !(left == right);
            }

            public override bool Equals(object? obj)
            {

                var otherType = obj as Type;
                return this.MetadataToken == otherType?.MetadataToken;
            }

            public bool Equals(CustomType? other)
            {
                return MetadataToken == other?.MetadataToken;
            }
            public override bool Equals(Type? other)
            {
                return MetadataToken == other?.MetadataToken;
            }

            public override int GetHashCode()
            {
                int srcToken = 0;
                try
                {
                    srcToken = SrcType.MetadataToken;
                }
                catch (Exception ex)
                {

                }
                return srcToken;
            }



        }
    }
}