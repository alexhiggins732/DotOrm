using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;

namespace DotRpc
{
    using Castle.DynamicProxy;
    using System.Reflection.Emit;
    using System.Reflection.Metadata;
    using System.Reflection.PortableExecutable;
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    public interface IRpcTypeFactory
    {
        Type GetMethodProxyType(MethodInfo method);
        RpcPayload GetPayload(int id, string name, string value);
    }

    public interface IRpxProxyGenerator
    {
        Type GetProxyType(MethodInfo method);
    }
    public class RpcTypeFactory : IRpcTypeFactory
    {
        private ILogger<RpcTypeFactory> logger;

        public RpcTypeFactory(ILoggerFactory loggerFactory, IRpxProxyGenerator proxyGenerator)
        {
            this.logger = loggerFactory.CreateLogger<RpcTypeFactory>();
            ProxyGenerator = proxyGenerator;
        }

        public IRpxProxyGenerator ProxyGenerator { get; }

        public Type GetMethodProxyType(MethodInfo method)
        {
            var result = ProxyGenerator.GetProxyType(method);
            return result;
        }

        public RpcPayload GetPayload(int id, string name, string value)
        {
            var methodParams = new { id, name, value };
            var json = JsonSerializer.Serialize(methodParams);
            var bytes = Encoding.UTF8.GetBytes(json);
            var result = new RpcPayload { Payload = bytes };
            return result;

        }


    }

    //public static class Compilation
    //{
    //    public static CompilationResult CompileAssemblyFromType(Type type)
    //    {
    //        var compilation = new System.Runtime.Loader.Compilation();
    //        compilation.AddAssemblyFromType(type);
    //        compilation.SetEntryPoint(type.GetMethod("MyMethod"));

    //        return compilation.Emit();
    //    }
    //}

    #region AutoMapper
    //
    public class RpcProxyGenerator : IRpxProxyGenerator
    {
        private static readonly MethodInfo DelegateCombine = typeof(Delegate).GetMethod(nameof(Delegate.Combine), new[] { typeof(Delegate), typeof(Delegate) });
        private static readonly MethodInfo DelegateRemove = typeof(Delegate).GetMethod(nameof(Delegate.Remove));
        private static readonly EventInfo PropertyChanged = typeof(INotifyPropertyChanged).GetEvent(nameof(INotifyPropertyChanged.PropertyChanged));
        private static readonly ConstructorInfo ProxyBaseCtor = typeof(ProxyBase).GetConstructor(Type.EmptyTypes);
        private static readonly ModuleBuilder ProxyModule = CreateProxyModule();
        private readonly LockingConcurrentDictionary<TypeDescription, Type> ProxyTypes;// = new(EmitProxy);
        private readonly LockingConcurrentDictionary<MethodTypeDescription, Type> MethodProxyTypes; //= new(, EmitMethodProxy);
        private readonly LockingConcurrentDictionary<MethodTypeDescription, Type> ClientProxyTypes; //= new(EmitClientProxy);
        private readonly LockingConcurrentDictionary<MethodTypeDescription, Type> ServerProxyTypes;// = new(EmitServerProxy);
        private ILogger<RpcProxyGenerator> logger;

        public RpcProxyGenerator(ILoggerFactory loggerFactory)
        {

            this.logger = loggerFactory.CreateLogger<RpcProxyGenerator>();
            ProxyTypes = new(x => EmitProxy(loggerFactory, x));
            MethodProxyTypes = new(x => EmitMethodProxy(loggerFactory, x));
            ClientProxyTypes = new(x => EmitClientProxy(loggerFactory, x));
            ServerProxyTypes = new(x => EmitServerProxy(loggerFactory, x));
        }
        private static ModuleBuilder CreateProxyModule()
        {
            var assemblyName = typeof(RpcTypeFactory).Assembly.GetName();
            assemblyName.Name = "AutoMapper.Proxies.emit";
            var builder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var result = builder.DefineDynamicModule(assemblyName.Name);
            return result;
        }

        private static Type EmitClientProxy(ILoggerFactory loggerFactory, MethodTypeDescription typeDescription)
        {
            var typeBuilder = GenerateType(typeDescription);
            GenerateConstructor(typeBuilder);
            return typeBuilder.CreateTypeInfo().AsType();

        }
        private static Type EmitServerProxy(ILoggerFactory loggerFactory, MethodTypeDescription typeDescription)
        {
            var typeBuilder = GenerateType(typeDescription);
            GenerateConstructor(typeBuilder);
            return typeBuilder.CreateTypeInfo().AsType();
        }
        private static Type EmitMethodProxy(ILoggerFactory loggerFactory, MethodTypeDescription typeDescription)
        {
            //var methodArgs = typeDescription..GetParameters();
            //foreach (var arg in methodArgs)
            //{
            //    typeDescription.MethodArguments.Add(new(arg));
            //}
            var logger = loggerFactory.CreateLogger(typeof(RpcProxyGenerator));
            logger.LogInformation($"Emitting method proxy for type {typeDescription.GenerateRpcMethodName()}");
            var propLogger = loggerFactory.CreateLogger<PropertyEmitter>();
            var args = typeDescription.MethodArguments;


            var interfaceType = typeDescription.Type;
            var typeBuilder = GenerateType(typeDescription,
                new[] { CreateAttributeBuilder<DataContractAttribute>() },
                implementedInterfaceTypes: new Type[] { typeof(IDataContract) });
            GenerateConstructor(typeBuilder);
            

            var properties = GenerateProperties(logger);
            GenerateConstructorForMethodArgs(typeBuilder, typeDescription, properties);

            //EmitClientProxy( typeDescription);
            // EmitServerProxy( typeDescription);
            return typeBuilder.CreateTypeInfo().AsType();

            CustomAttributeBuilder CreateAttributeBuilder<AttributeType>(object[]? constructorArgs = null, object? properties = null)
            {
                var t = typeof(AttributeType);
                constructorArgs ??= new object[0];
                ConstructorInfo attributeCtor = t.GetConstructor(constructorArgs.Select(x => x.GetType()).ToArray());
                if (properties != null)
                {
                    // Retrieve the properties from the anonymous type
                    var propertyInfos = properties.GetType().GetProperties();

                    // Get the constructor parameters and values
                    var propertyArgs = new List<object>();
                    var atttributeProperties = new List<PropertyInfo>();
                    foreach (var propertyInfo in propertyInfos)
                    {
                        propertyArgs.Add(propertyInfo.GetValue(properties));
                        atttributeProperties.Add(t.GetProperty(propertyInfo.Name));
                    }


                    // Create the custom attribute builder with the constructor parameters
                    var propArgs = propertyArgs.ToArray();
                    return new CustomAttributeBuilder(attributeCtor, constructorArgs, atttributeProperties.ToArray(), propArgs.ToArray());
                }
                else
                {

                    return new CustomAttributeBuilder(attributeCtor, constructorArgs);
                }

            }
            Dictionary<string, PropertyEmitter> GenerateProperties(ILogger propLogger)
            {
                var fieldBuilders = new Dictionary<string, PropertyEmitter>();
                int order = 0;
                foreach (var property in PropertiesToImplement())
                {
                    if (fieldBuilders.TryGetValue(property.Name, out var propertyEmitter))
                    {
                        if (propertyEmitter.PropertyType != property.Type && (property.CanWrite || !property.Type.IsAssignableFrom(propertyEmitter.PropertyType)))
                        {
                            throw new ArgumentException($"The interface has a conflicting property {property.Name}", nameof(interfaceType));
                        }
                    }
                    else
                    {
                        //var parameterProperty = new PropertyDescription(property.Name, property.Type, property.CanWrite);
                        //fieldBuilders.Add(property.Name, new PropertyEmitter(typeBuilder, parameterProperty, null));

                        var parameterProperty = new PropertyDescription(property.Name, property.Type, property.CanWrite);
                        var dataMember = CreateAttributeBuilder<DataMemberAttribute>(properties: new { Order = order });
                        var jsonPropertyOrder = CreateAttributeBuilder<JsonPropertyOrderAttribute>(new object[] { order });
                        var JsonProperty = CreateAttributeBuilder<Newtonsoft.Json.JsonPropertyAttribute>(properties: new { Order = order });
                        order++;
                        var atts = new CustomAttributeBuilder[] { dataMember, jsonPropertyOrder, JsonProperty };
                        fieldBuilders.Add(property.Name, new PropertyEmitter(logger, typeBuilder, parameterProperty, null, atts));

                    }
                }
                return fieldBuilders;
            }
            List<MethodArgumentDescription> PropertiesToImplement()
            {
                var propertiesToImplement = typeDescription.MethodArguments.ToList();
                return propertiesToImplement;
            }

        }

        static void GenerateConstructor(TypeBuilder typeBuilder)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var ctorIl = constructorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, ProxyBaseCtor);
            ctorIl.Emit(OpCodes.Ret);
        }

        static void GenerateConstructorForMethodArgs(TypeBuilder typeBuilder,
            MethodTypeDescription method,
            Dictionary<string, PropertyEmitter> properties)
        {
            var argumentTypes = method.MethodArguments.Select(x => x.Type).ToArray();
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, argumentTypes);
            var ctorIl = constructorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, ProxyBaseCtor);
            var argIndex = 1;
            foreach (var arg in method.MethodArguments)
            {

                var backingField = properties[arg.Name]._fieldBuilder;
                ctorIl.Emit(OpCodes.Ldarg_0);
                ctorIl.Emit(OpCodes.Ldarg_S, argIndex);
                ctorIl.Emit(OpCodes.Stfld, backingField);
            }
            ctorIl.Emit(OpCodes.Ret);
        }


        static TypeBuilder GenerateType(
            MethodTypeDescription typeDescription,
            IEnumerable<CustomAttributeBuilder>? customAttributes = null,
            IEnumerable<Type>? implementedInterfaceTypes = null)
        {
            var interfaceType = typeDescription.Type;

            var propertyNames = string.Join("_", typeDescription.MethodArguments.Select(p => p.Name));
            var typeName = $"Proxy_{interfaceType.Name}_{typeDescription.method.Name}_{typeDescription.GetHashCode()}_{propertyNames}";
            const int MaxTypeNameLength = 1023;
            typeName = typeName.Substring(0, Math.Min(MaxTypeNameLength, typeName.Length));
            typeName = typeDescription.GenerateRpcMethodName();
            Debug.WriteLine(typeName, "Emitting proxy type");
            var builder = ProxyModule.DefineType(typeName,
                TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public, typeof(RpcProxyBase),
                 implementedInterfaceTypes?.ToArray() ?? Type.EmptyTypes);
            if (customAttributes != null)
            {
                foreach (var att in customAttributes)
                {
                    builder.SetCustomAttribute(att);
                }
            }
            return builder;
        }

        private static Type EmitProxy(ILoggerFactory loggerFactory, TypeDescription typeDescription)
        {
            var propLogger = loggerFactory.CreateLogger<PropertyEmitter>();
            var logger = loggerFactory.CreateLogger(typeof(RpcProxyGenerator));
            logger.LogInformation($"Emitting proxy for type {typeDescription.Type.FullName}");
            var interfaceType = typeDescription.Type;
            var typeBuilder = GenerateType();
            GenerateConstructor();
            FieldBuilder propertyChangedField = null;
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(interfaceType))
            {
                GeneratePropertyChanged();
            }
            GenerateFields();
            return typeBuilder.CreateTypeInfo().AsType();
            TypeBuilder GenerateType()
            {
                var propertyNames = string.Join("_", typeDescription.AdditionalProperties.Select(p => p.Name));
                var typeName = $"Proxy_{interfaceType.FullName}_{typeDescription.GetHashCode()}_{propertyNames}";
                const int MaxTypeNameLength = 1023;
                typeName = typeName.Substring(0, Math.Min(MaxTypeNameLength, typeName.Length));
                Debug.WriteLine(typeName, "Emitting proxy type");
                return ProxyModule.DefineType(typeName,
                    TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public, typeof(ProxyBase),
                    interfaceType.IsInterface ? new[] { interfaceType } : Type.EmptyTypes);
            }
            void GeneratePropertyChanged()
            {
                propertyChangedField = typeBuilder.DefineField(PropertyChanged.Name, typeof(PropertyChangedEventHandler), FieldAttributes.Private);
                EventAccessor(PropertyChanged.AddMethod, DelegateCombine);
                EventAccessor(PropertyChanged.RemoveMethod, DelegateRemove);
            }
            void EventAccessor(MethodInfo method, MethodInfo delegateMethod)
            {
                var eventAccessor = typeBuilder.DefineMethod(method.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual, typeof(void),
                    new[] { typeof(PropertyChangedEventHandler) });
                var addIl = eventAccessor.GetILGenerator();
                addIl.Emit(OpCodes.Ldarg_0);
                addIl.Emit(OpCodes.Dup);
                addIl.Emit(OpCodes.Ldfld, propertyChangedField);
                addIl.Emit(OpCodes.Ldarg_1);
                addIl.Emit(OpCodes.Call, delegateMethod);
                addIl.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
                addIl.Emit(OpCodes.Stfld, propertyChangedField);
                addIl.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(eventAccessor, method);
            }
            void GenerateFields()
            {
                var fieldBuilders = new Dictionary<string, PropertyEmitter>();
                foreach (var property in PropertiesToImplement())
                {
                    if (fieldBuilders.TryGetValue(property.Name, out var propertyEmitter))
                    {
                        if (propertyEmitter.PropertyType != property.Type && (property.CanWrite || !property.Type.IsAssignableFrom(propertyEmitter.PropertyType)))
                        {
                            throw new ArgumentException($"The interface has a conflicting property {property.Name}", nameof(interfaceType));
                        }
                    }
                    else
                    {
                        fieldBuilders.Add(property.Name, new PropertyEmitter(propLogger, typeBuilder, property, propertyChangedField));
                    }
                }
            }
            List<PropertyDescription> PropertiesToImplement()
            {
                var propertiesToImplement = new List<PropertyDescription>();
                var allInterfaces = new List<Type>(interfaceType.GetInterfaces()) { interfaceType };
                // first we collect all properties, those with setters before getters in order to enable less specific redundant getters
                foreach (var property in
                    allInterfaces.Where(intf => intf != typeof(INotifyPropertyChanged))
                        .SelectMany(intf => intf.GetProperties())
                        .Select(p => new PropertyDescription(p))
                        .Concat(typeDescription.AdditionalProperties))
                {
                    if (property.CanWrite)
                    {
                        propertiesToImplement.Insert(0, property);
                    }
                    else
                    {
                        propertiesToImplement.Add(property);
                    }
                }
                return propertiesToImplement;
            }
            void GenerateConstructor()
            {
                var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
                var ctorIl = constructorBuilder.GetILGenerator();
                ctorIl.Emit(OpCodes.Ldarg_0);
                ctorIl.Emit(OpCodes.Call, ProxyBaseCtor);
                ctorIl.Emit(OpCodes.Ret);
            }
        }
        public Type GetProxyType(Type interfaceType) => ProxyTypes.GetOrAdd(new(interfaceType, Array.Empty<PropertyDescription>()));
        public Type GetProxyType(MethodInfo methodInfo) => MethodProxyTypes.GetOrAdd(new(methodInfo));
        public Type GetSimilarType(Type sourceType, IEnumerable<PropertyDescription> additionalProperties) =>
            ProxyTypes.GetOrAdd(new(sourceType, additionalProperties.OrderBy(p => p.Name).ToArray()));
        public class PropertyEmitter
        {
            private static readonly MethodInfo ProxyBaseNotifyPropertyChanged = typeof(ProxyBase).GetInstanceMethod("NotifyPropertyChanged");
            public readonly FieldBuilder _fieldBuilder;
            public readonly MethodBuilder _getterBuilder;
            public readonly PropertyBuilder _propertyBuilder;
            public readonly MethodBuilder? _setterBuilder;
            public PropertyEmitter(ILogger logger, TypeBuilder owner, PropertyDescription property, FieldBuilder propertyChangedField,
                IEnumerable<CustomAttributeBuilder>? customAttributes = null)
            {
                logger.LogInformation($"Creating property for {property.Name}");
                var name = property.Name;
                var propertyType = property.Type;
                _fieldBuilder = owner.DefineField($"<{name}>", propertyType, FieldAttributes.Private);
                _propertyBuilder = owner.DefineProperty(name, PropertyAttributes.None, propertyType, null);

                // Apply custom attributes to the property
                if (customAttributes != null)
                {
                    foreach (var attribute in customAttributes)
                    {
                        _propertyBuilder.SetCustomAttribute(attribute);
                    }
                }

                _getterBuilder = owner.DefineMethod($"get_{name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName, propertyType, Type.EmptyTypes);
                ILGenerator getterIl = _getterBuilder.GetILGenerator();
                getterIl.Emit(OpCodes.Ldarg_0);
                getterIl.Emit(OpCodes.Ldfld, _fieldBuilder);
                getterIl.Emit(OpCodes.Ret);
                _propertyBuilder.SetGetMethod(_getterBuilder);
                if (!property.CanWrite)
                {
                    return;
                }
                _setterBuilder = owner.DefineMethod($"set_{name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName, typeof(void), new[] { propertyType });
                ILGenerator setterIl = _setterBuilder.GetILGenerator();
                setterIl.Emit(OpCodes.Ldarg_0);
                setterIl.Emit(OpCodes.Ldarg_1);
                setterIl.Emit(OpCodes.Stfld, _fieldBuilder);
                if (propertyChangedField != null)
                {
                    setterIl.Emit(OpCodes.Ldarg_0);
                    setterIl.Emit(OpCodes.Dup);
                    setterIl.Emit(OpCodes.Ldfld, propertyChangedField);
                    setterIl.Emit(OpCodes.Ldstr, name);
                    setterIl.Emit(OpCodes.Call, ProxyBaseNotifyPropertyChanged);
                }
                setterIl.Emit(OpCodes.Ret);
                _propertyBuilder.SetSetMethod(_setterBuilder);
            }
            public Type PropertyType => _propertyBuilder.PropertyType;
        }
    }
    public abstract class ProxyBase : IDataContract
    {
        public ProxyBase() { }
        protected void NotifyPropertyChanged(PropertyChangedEventHandler handler, string method) =>
            handler?.Invoke(this, new PropertyChangedEventArgs(method));
        public object?[] ToArray()
        {
            var properties = this.GetType().GetProperties();
            return properties.Select(prop => prop.GetValue(this, null)).ToArray();
        }
    }
    public abstract class RpcProxyBase : IDataContract
    {
        public RpcProxyBase() { }
        public object?[] ToArray()
        {
            var properties = this.GetType().GetProperties();
            return properties.Select(prop => prop.GetValue(this, null)).ToArray();
        }
    }
    public readonly record struct TypeDescription(Type Type, PropertyDescription[] AdditionalProperties)
    {
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Type);
            foreach (var property in AdditionalProperties)
            {
                hashCode.Add(property);
            }
            return hashCode.ToHashCode();
        }
        public bool Equals(TypeDescription other) => Type == other.Type && AdditionalProperties.SequenceEqual(other.AdditionalProperties);
    }

    public readonly record struct MethodTypeDescription(MethodInfo method, Type Type, List<MethodArgumentDescription> MethodArguments)
    {

        public MethodTypeDescription(MethodInfo method)
            : this(method, method.ReturnType, method.GetParameters().Select(x => new MethodArgumentDescription(x)).ToList())
        {
        }
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Type);
            foreach (var property in MethodArguments)
            {
                hashCode.Add(property);
            }
            return hashCode.ToHashCode();
        }
        public bool Equals(MethodTypeDescription other) => Type == other.Type && MethodArguments.SequenceEqual(other.MethodArguments);
    }

    [DebuggerDisplay("{Name}-{Type.Name}")]
    public readonly record struct PropertyDescription(string Name, Type Type, bool CanWrite = true)
    {
        public PropertyDescription(PropertyInfo property) : this(property.Name, property.PropertyType, property.CanWrite) { }
    }


    [DebuggerDisplay("{Name}-{Type.Name}")]
    public readonly record struct MethodArgumentDescription(string Name, Type Type, bool CanWrite = true)
    {
        public MethodArgumentDescription(ParameterInfo property) : this(property.Name, property.ParameterType, true) { }
    }

    #endregion
}
