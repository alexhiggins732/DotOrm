using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;

namespace DotRpc
{
    using Castle.DynamicProxy;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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

    public interface IRpcProxyGenerator
    {
        Type GetProxyType(MethodInfo method);
    }
    public class RpcTypeFactory : IRpcTypeFactory
    {
        private ILogger<RpcTypeFactory> logger;

        public RpcTypeFactory(ILoggerFactory loggerFactory, IRpcProxyGenerator proxyGenerator)
        {
            this.logger = loggerFactory.CreateLogger<RpcTypeFactory>();
            ProxyGenerator = proxyGenerator;
        }

        public IRpcProxyGenerator ProxyGenerator { get; }

        //guard against Dependency Injection creating new instances of singletons.
        static ConcurrentDictionary<MethodTypeDescription, Type> methodProxyTypes = new();
        static ConcurrentDictionary<TypeDescription, Type> exampleProxyTypes = new();
        public Type GetMethodProxyType(MethodInfo method)
        {
            try
            {
                var description = new MethodTypeDescription(method);

                if (methodProxyTypes.TryGetValue(description, out var proxyType))
                {
                    return proxyType;
                }
                var exists = NameServiceExtensions.RpcMethodNameCache.TryGetValue(description, out var rpcMethodName);
                if (exists)
                {
                    logger.LogWarning($"Warning: Generating existing proxy type for method {rpcMethodName}");
                }
                var result = methodProxyTypes.GetOrAdd(description, x => ProxyGenerator.GetProxyType(method));
                return result;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error generating proxy for method: {method.Name} - {e}");
                return null;
            }

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



    #region AutoMapper
    //
    public class RpcProxyGenerator : IRpcProxyGenerator
    {
        private static readonly MethodInfo DelegateCombine = typeof(Delegate).GetMethod(nameof(Delegate.Combine), new[] { typeof(Delegate), typeof(Delegate) });
        private static readonly MethodInfo DelegateRemove = typeof(Delegate).GetMethod(nameof(Delegate.Remove));
        private static readonly EventInfo PropertyChanged = typeof(INotifyPropertyChanged).GetEvent(nameof(INotifyPropertyChanged.PropertyChanged));
        private static readonly ConstructorInfo ProxyBaseCtor = typeof(ProxyBase).GetConstructor(Type.EmptyTypes);
        private static readonly ModuleBuilder ProxyModule = CreateProxyModule();
        public readonly LockingConcurrentDictionary<TypeDescription, Type> ProxyTypes;// = new(EmitProxy);
        public readonly LockingConcurrentDictionary<TypeDescription, Type> ExampleProxyTypes;// = new(EmitProxy);
        public readonly LockingConcurrentDictionary<MethodTypeDescription, Type> MethodProxyTypes; //= new(, EmitMethodProxy);
        public readonly LockingConcurrentDictionary<MethodTypeDescription, Type> ClientProxyTypes; //= new(EmitClientProxy);
        public readonly LockingConcurrentDictionary<MethodTypeDescription, Type> ServerProxyTypes;// = new(EmitServerProxy);
        private ILogger<RpcProxyGenerator> logger;
        public static LoggerFactory LoggerFactory;
        public RpcProxyGenerator(ILoggerFactory loggerFactory)
        {
            LoggerFactory = LoggerFactory;
            this.logger = loggerFactory.CreateLogger<RpcProxyGenerator>();
            ProxyTypes = new(x => EmitProxy(loggerFactory, x));
            ExampleProxyTypes = new(x => EmitExampleProxy(loggerFactory, x));
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
            var result = GenerateType(typeDescription);
            var typeBuilder = result.Builder;
            GenerateConstructor(typeBuilder);
            return typeBuilder.CreateTypeInfo().AsType();

        }
        private static Type EmitServerProxy(ILoggerFactory loggerFactory, MethodTypeDescription typeDescription)
        {
            var result = GenerateType(typeDescription);
            var typeBuilder = result.Builder;
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

            var typeName = typeDescription.GenerateRpcMethodName();
            if (methodProxies.TryGetValue(typeName, out var methodProxy))
            {
                return methodProxy;
            }


            var interfaceType = typeDescription.Type;
            var result = GenerateType(typeDescription,
                new[] { CreateAttributeBuilder<DataContractAttribute>() },
                implementedInterfaceTypes: new Type[] { typeof(IDataContract) });
            var typeBuilder = result.Builder;
            GenerateConstructor(typeBuilder);


            var properties = GenerateProperties(logger, ref result.GenericParameters);
            if (properties.Count == 0)
            {
                string bp = "";
            }
            //else
            GenerateConstructorForMethodArgs(typeBuilder, typeDescription, properties);

            //EmitClientProxy( typeDescription);
            // EmitServerProxy( typeDescription);


            var proxyType = typeBuilder.CreateTypeInfo().AsType();
            methodProxies.TryAdd(typeBuilder.FullName, proxyType);
            return proxyType;

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
            Dictionary<string, PropertyEmitter> GenerateProperties(ILogger propLogger,
                ref Dictionary<string, GenericTypeParameterBuilder> genericParameters)
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
                        var propEmitter = new PropertyEmitter(logger, typeBuilder, parameterProperty, null, ref genericParameters, atts);
                        fieldBuilders.Add(property.Name, propEmitter);

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
            //if there are no method argments, create a constructor that takes an empty object array
            //  to comply with the IDataContract interface.
            if (argumentTypes.Length == 0) { argumentTypes = new Type[] { typeof(object[]) }; }
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, argumentTypes);
            var ctorIl = constructorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, ProxyBaseCtor);
            var argIndex = 0;
            foreach (var arg in method.MethodArguments)
            {

                var backingField = properties[arg.Name]._fieldBuilder;
                ctorIl.Emit(OpCodes.Ldarg_0);
                ctorIl.Emit(OpCodes.Ldarg_S, ++argIndex);
                ctorIl.Emit(OpCodes.Stfld, backingField);
            }
            ctorIl.Emit(OpCodes.Ret);
        }

        class GenerateTypeResult
        {
            public TypeBuilder Builder;
            public Dictionary<string, GenericTypeParameterBuilder> GenericParameters;
        }

        static ConcurrentDictionary<string, Type> methodProxies = new();
        static GenerateTypeResult GenerateType(
            MethodTypeDescription typeDescription,
            IEnumerable<CustomAttributeBuilder>? customAttributes = null,
            IEnumerable<Type>? implementedInterfaceTypes = null)
        {
            string typeName = typeDescription.GenerateRpcMethodName();

            Debug.WriteLine(typeName, $"Emitting proxy type: {typeName}");
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
            var result = new GenerateTypeResult { Builder = builder, GenericParameters = new() };
            var declaringType = typeDescription.method.DeclaringType;
            if (declaringType is null)
            {
                throw new ArgumentNullException(nameof(declaringType));
            }
            if (declaringType.IsGenericType)
            {
                var genericArguments = declaringType.GetGenericArguments();
                var genericArgumentNames = genericArguments.Select(x => x.Name).ToArray();
                var genericParameterBuilders = builder.DefineGenericParameters(genericArgumentNames);
                result.GenericParameters = new();
                genericParameterBuilders.ToList().ForEach(x => result.GenericParameters.Add(x.Name, x));
            }

            if (typeDescription.method.IsGenericMethod)
            {
                var genericArguments = typeDescription.method.GetGenericArguments();
                var genericArgumentNames = genericArguments.Select(x => x.Name).ToArray();
                var genericParameterBuilders = builder.DefineGenericParameters(genericArgumentNames);
                result.GenericParameters ??= new();


                genericParameterBuilders.ToList().ForEach(x => result.GenericParameters.Add(x.Name, x));
            }

            return result;
        }

        static GenerateTypeResult GenerateType(
            TypeDescription typeDescription,
            IEnumerable<CustomAttributeBuilder>? customAttributes = null,
            IEnumerable<Type>? implementedInterfaceTypes = null)
        {
            string typeName = typeDescription.GenerateRpcTypeName();

            Debug.WriteLine(typeName, $"Emitting proxy type: {typeName}");
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
            var result = new GenerateTypeResult { Builder = builder, GenericParameters = new() };
            var declaringType = typeDescription.Type;
            if (declaringType is null)
            {
                throw new ArgumentNullException(nameof(declaringType));
            }
            if (declaringType.IsGenericType)
            {
                var genericArguments = declaringType.GetGenericArguments().Distinct().ToList();
                //var names= genericArguments.Select(x=> NameServiceExtensions.Instance.GenerateSwaggerSchemaId(x));
                var genericArgumentNames = genericArguments.Select(x => x.Name).ToArray();
                var genericParameterBuilders = builder.DefineGenericParameters(genericArgumentNames);
                result.GenericParameters = new();
                genericParameterBuilders.ToList().ForEach(x => result.GenericParameters.Add(x.Name, x));
            }

            return result;
        }


        static ConcurrentDictionary<TypeDescription, Type> exampleProxies = new();

        public static Type EmitExampleProxy(ILoggerFactory loggerFactory, Type type)
        {
            var typeProps = type.GetProperties();
            var props = typeProps.Select(x =>
            {
                if (x.PropertyType.IsGenericType || x.PropertyType.IsGenericParameter)
                    //return new PropertyDescription(x.Name, typeof(string), true);
                    return GetExamplePropertyDescription(loggerFactory, x, type);
                else
                    return new PropertyDescription(x);
            }).ToArray();
            var description = new TypeDescription(type, props.ToArray());
            return exampleProxies.GetOrAdd(description, type => EmitExampleProxy(loggerFactory, description));
        }

        static Type GetPrimitiveExampleType(Type type)
        {
            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments();
                var exampleTypes = args.Select(x => GetPrimitiveExampleType(x)).ToArray();
                var genericExampleType = type.MakeGenericType(exampleTypes);
                return genericExampleType;
            }
            var primitiveExampleTypes = PrimitiveTypes()
                .FirstOrDefault(x => x.IsAssignableFrom(type) || type.IsAssignableFrom(x));
            if (primitiveExampleTypes is not null)
            {
                return primitiveExampleTypes;
            }
            else
            {
                return typeof(string);
            }
        }
        static PropertyDescription GetExamplePropertyDescription(ILoggerFactory loggerFactory, PropertyInfo property, Type type)
        {
            if (!property.PropertyType.IsGenericParameter && !property.PropertyType.IsGenericType)
            {
                return new PropertyDescription(property.Name, property.PropertyType, true);
            }
            //    throw new NotFiniteNumberException();
            var propType = property.PropertyType;
            if (propType.IsGenericType)
            {
                var exampleType= GetPrimitiveExampleType(propType);
                return new PropertyDescription(property.Name, exampleType, true);
            }
            var constraints = property.PropertyType.GetGenericParameterConstraints();
            if (constraints.Length == 0)
                return new PropertyDescription(property.Name, typeof(string), true);

            var primitiveExampleTypes = PrimitiveTypes()
                .FirstOrDefault(x => x.IsAssignableFrom(property.PropertyType) || property.PropertyType.IsAssignableFrom(x));
            if (primitiveExampleTypes is not null)
            {
                return new PropertyDescription(property.Name, primitiveExampleTypes, true);
            }
            else
            {

                //TODO: find a class that meets the constraints of typeof(t)
                return new PropertyDescription(property.Name, typeof(string), true);

            }
        }

        static Type[]? _primitiveTypes = null;
        static Type[] PrimitiveTypes()
        {
            if (_primitiveTypes is null)
            {
                var baseTypes = primitiveBaseTypes;
                var nullableTypes = baseTypes.Where(x => x.IsValueType).Select(x => GetNullableType(x));
                var baseAndNullable = baseTypes.Concat(nullableTypes).ToArray();
                var arrayTypes = baseAndNullable.Select(x => GetArrayType(x));
                var enumerableTypes = baseAndNullable.Select(x => GetEnumerableType(x));
                _primitiveTypes = baseAndNullable.Concat(arrayTypes).Concat(enumerableTypes).ToArray();
            }
            return _primitiveTypes;
        }

        static Type GetNullableType(Type x) => typeof(Nullable<>).MakeGenericType(x);
        static Type GetArrayType(Type x) => x.MakeArrayType();
        static Type GetEnumerableType(Type x) => typeof(IEnumerable<>).MakeGenericType(x);
        static Type[] primitiveBaseTypes = new Type[]
        {
            typeof(string),
            typeof(int),
            typeof(Guid),
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(short),
            typeof(ushort),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),

        };

        public static Type EmitExampleProxy(ILoggerFactory loggerFactory, TypeDescription typeDescription)
        {
            var propLogger = loggerFactory.CreateLogger<PropertyEmitter>();
            var logger = loggerFactory.CreateLogger(typeof(RpcProxyGenerator));
            logger.LogInformation($"Emitting proxy for type {typeDescription.Type.FullName}");
            string typeName = typeDescription.Type.DotRpcSwaggerSchemaIdGenerator() + "Contract";
            Debug.WriteLine(typeName, $"Emitting proxy type: {typeName}");
            var interfaceType = typeDescription.Type;
            var result = GenerateType(typeDescription);


            //var typeBuilder = ProxyModule.DefineType(typeName,
            //    TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public, typeof(RpcProxyBase),
            //      Type.EmptyTypes);
            GenerateConstructor(result.Builder);


            foreach (var prop in typeDescription.AdditionalProperties)
            {
                var propEmitter = new PropertyEmitter(logger, result.Builder, prop, null, ref result.GenericParameters);
            }

            //EmitClientProxy( typeDescription);
            // EmitServerProxy( typeDescription);
            var exampleType = result.Builder.CreateTypeInfo().AsType();
            if (exampleType.IsGenericType)
            {
                var genericArgs = exampleType.GetGenericArguments();
                var exampleTypes = genericArgs.Select(x => GetPrimitiveExampleType(x)).ToArray();
                var genericExampleType = exampleType.MakeGenericType(exampleTypes);
                exampleType= genericExampleType;
            }
            return exampleType;

        }
        public static Type EmitProxy(ILoggerFactory loggerFactory, TypeDescription typeDescription)
        {
            var propLogger = loggerFactory.CreateLogger<PropertyEmitter>();
            var logger = loggerFactory.CreateLogger(typeof(RpcProxyGenerator));
            logger.LogInformation($"Emitting proxy for type {typeDescription.Type.FullName}");
            var interfaceType = typeDescription.Type;
            //var typeBuilder = GenerateType();
            var result = GenerateType(typeDescription);
            var typeBuilder = result.Builder;
            GenerateConstructor();
            FieldBuilder propertyChangedField = null;
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(interfaceType))
            {
                GeneratePropertyChanged();
            }
            GenerateFields();
            return result.Builder.CreateTypeInfo().AsType();
            //TypeBuilder GenerateType()
            //{
            //    var propertyNames = string.Join("_", typeDescription.AdditionalProperties.Select(p => p.Name));
            //    var typeName = $"Proxy_{interfaceType.FullName}_{typeDescription.GetHashCode()}_{propertyNames}";
            //    const int MaxTypeNameLength = 1023;
            //    typeName = typeName.Substring(0, Math.Min(MaxTypeNameLength, typeName.Length));
            //    Debug.WriteLine(typeName, "Emitting proxy type");
            //    return ProxyModule.DefineType(typeName,
            //        TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public, typeof(ProxyBase),
            //        interfaceType.IsInterface ? new[] { interfaceType } : Type.EmptyTypes);
            //}
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
                        fieldBuilders.Add(property.Name, new PropertyEmitter(propLogger, typeBuilder, property, propertyChangedField, ref result.GenericParameters));
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

    }
    public class PropertyEmitter
    {
        private static readonly MethodInfo ProxyBaseNotifyPropertyChanged = typeof(ProxyBase).GetInstanceMethod("NotifyPropertyChanged");
        public readonly FieldBuilder _fieldBuilder;
        public readonly MethodBuilder _getterBuilder;
        public readonly PropertyBuilder _propertyBuilder;
        public readonly MethodBuilder? _setterBuilder;
        public PropertyEmitter(ILogger logger, TypeBuilder owner, PropertyDescription property, FieldBuilder propertyChangedField,
            ref Dictionary<string, GenericTypeParameterBuilder> genericParameters,
            IEnumerable<CustomAttributeBuilder>? customAttributes = null)
        {
            logger.LogInformation($"Creating property for {property.Name}");
            var name = property.Name;
            var propertyType = property.Type;
            if (propertyType.IsGenericParameter || propertyType.IsGenericType)
            {
                if (genericParameters is not null && genericParameters.ContainsKey(propertyType.Name))
                {
                    propertyType = genericParameters[propertyType.Name];
                }
                else
                {
                    genericParameters ??= new();
                    var genericArguments = propertyType.GetGenericArguments();
                    var genericArgumentNames = genericArguments.Select(x => x.Name).ToArray();
                    var genericParameterBuilders = owner.DefineGenericParameters(genericArgumentNames);
                    foreach (var b in genericParameterBuilders)
                    {
                        genericParameters.Add(b.Name, b);
                    }
                    //genericParameterBuilders.ToList().ForEach(x => genericParameters.Add(x.Name, x));

                    logger.LogWarning($"Invalid generic parameter defined: {propertyType.Name}");
                }

            }
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
            foreach (var genericArgument in method.GetGenericArguments())
            {
                hashCode.Add(genericArgument.FullName);
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
