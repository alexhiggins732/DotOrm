using DotMpi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace DotOrmLib.GrpcModels
{


    [DataContract]
    public class GrpcSerializableTypeInfo
    {
        [DataMember(Order = 1)]
        public virtual string AssemblyName { get; set; } = null!;

        [DataMember(Order = 2)]
        public virtual string TypeName { get; set; } = null!;
        public GrpcSerializableTypeInfo()
        {

        }
        public GrpcSerializableTypeInfo(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            //Type wil have assembly and fullname
#pragma warning disable CS8601 // Possible null reference assignment.
            AssemblyName = type.Assembly.FullName;
            TypeName = type.FullName;
#pragma warning restore CS8601 // Possible null reference assignment.

        }
    }

    [DataContract]
    [JsonConverter(typeof(GrpcSerializableValueConverter))]
    public class GrpcSerializableValue : GrpcSerializableTypeInfo
    {
        [DataMember(Order = 1)]
        public override string AssemblyName { get => base.AssemblyName; set => base.AssemblyName=value; } 

        [DataMember(Order = 2)]
        public override string TypeName { get => base.TypeName; set => base.TypeName = value; } 

        [DataMember(Order = 3)]
        public virtual string? JsonValue { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Order = 4)]
        public virtual GrpcSerializableErrorData? ErrorData { get; set; }

        private object? objectValue;

        [JsonIgnore]
        public object? ObjectValue
        {
            get
            {
                if (objectValue is null && JsonValue is not null)
                {
                    var fromJson = FromJson(ToJson());
                    objectValue = fromJson?.ObjectValue;

                }
                return objectValue;
            }
            set
            {
                JsonValue = JsonConvert.SerializeObject(value);
                objectValue = value;
            }
        }

        public static GrpcSerializableValue? FromJson(string json)
            => JsonConvert.DeserializeObject<GrpcSerializableValue>(json);

        public bool HasError => ErrorData is not null;

        public GrpcSerializableValue()
        {

        }
        public GrpcSerializableValue(object? value)
            : base()
        {
            var type = value?.GetType() ?? typeof(object);

            //type will always have an assembly with a name
#pragma warning disable CS8601 // Possible null reference assignment.
            AssemblyName = type.Assembly.FullName;
            TypeName = type.FullName;
#pragma warning restore CS8601 // Possible null reference assignment.
            ObjectValue = value;
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
        public byte[] ToByteArray() => Encoding.UTF8.GetBytes(ToJson());

        public static implicit operator ValueTask<GrpcSerializableValue>(GrpcSerializableValue value)
            => new ValueTask<GrpcSerializableValue>(value);


    }

    [DataContract]
    public class GrpcSerializableValue<TResult> : GrpcSerializableValue
    {


        [DataMember(Order = 1)]
        public override string AssemblyName { get => base.AssemblyName; set => base.AssemblyName = value; }

        [DataMember(Order = 2)]
        public override string TypeName { get => base.TypeName; set => base.TypeName = value; }

        [DataMember(Order = 3)]
        public override string? JsonValue { get => base.JsonValue; set => base.JsonValue = value; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Order = 4)]
        public override GrpcSerializableErrorData? ErrorData { get => base.ErrorData; set => base.ErrorData = value; }

        [DataMember(Order = 5)]
        public string? InitDate { get; set; }

        [JsonIgnore]
        public TResult? Result => ObjectValue is null ? default : (TResult?)ObjectValue;
        //public GrpcSerializableValue() { InitDate = DateTime.Now.ToString(); }
        public GrpcSerializableValue(TResult? value)
            : base(value)
        {
            ObjectValue = value;
            InitDate = DateTime.Now.ToString();
        }
        public GrpcSerializableValue()
        {
            var t = typeof(TResult);
            this.TypeName = t.FullName;
            this.AssemblyName = t.Assembly.FullName;
        }
        public static implicit operator ValueTask<GrpcSerializableValue<TResult>>(GrpcSerializableValue<TResult> value)
          => new ValueTask<GrpcSerializableValue<TResult>>(value);

        public static implicit operator TResult?(GrpcSerializableValue<TResult> value)
            => (value is not null) ? value.Result : throw new ArgumentNullException(nameof(value));

        public static implicit operator GrpcSerializableValue<TResult>(TResult value)
            => new GrpcSerializableValue<TResult>(value);

        public static implicit operator GrpcSerializableValue<TResult>(ValueTask<TResult> value)
            => new GrpcSerializableValue<TResult>(value.Result);
    }

    public class GrpcSerializableValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(GrpcSerializableErrorData).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            var assemblyToken = jsonObject[nameof(GrpcSerializableErrorData.AssemblyName)];
            if (assemblyToken is null)
            {
                throw new ArgumentException($"{nameof(GrpcSerializableErrorData.AssemblyName)} not set");
            }
            var typeToken = jsonObject[nameof(GrpcSerializableErrorData.TypeName)];
            if (typeToken is null)
            {
                throw new ArgumentException($"{nameof(GrpcSerializableErrorData.TypeName)} not set");
            }

            //what? nameof(SerializableValue.Value)
            var valueToken = jsonObject[nameof(GrpcSerializableErrorData.JsonValue)];
            if (valueToken is null)
            {
                throw new ArgumentException($"{nameof(GrpcSerializableErrorData.TypeName)} not set");
            }
            string assemblyName = assemblyToken.ToString();
            string typeName = typeToken.ToString();
            var asm = Assembly.Load(assemblyName);
            var type = asm.GetType(typeName);
            if (type is null)
            {
                throw new TypeLoadException($"Failed to resolve type '{typeName}, {assemblyName}'");
            }



            JToken value = valueToken;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Converting null literal or possible null value to non-nullable type.
            object valueObject = null!;
            if (value != null)
                valueObject = value.ToObject(type);

            GrpcSerializableValue result = null!;
            if (objectType.IsGenericType)
            {
                var t = typeof(GrpcSerializableValue<>).MakeGenericType(type);
                ConstructorInfo constructor = t.GetConstructor(new Type[] { type });
#pragma warning disable CS8601 // Possible null reference assignment.
                object serializableValue = constructor.Invoke(new object[] { valueObject }); // creates a SerializableValue<int> with value 
#pragma warning restore CS8601 // Possible null reference assignment.
                result = (GrpcSerializableValue)serializableValue;
            }
            else
            {
                result = new GrpcSerializableValue(valueObject);
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Converting null literal or possible null value to non-nullable type.


            var errorToken = jsonObject[nameof(GrpcSerializableValue.ErrorData)];
            if (errorToken != null && errorToken is JObject errorObject)
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                result.ErrorData = (GrpcSerializableErrorData)errorToken.ToObject(typeof(ErrorData));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not null)
            {
                var argInfo = (GrpcSerializableValue)value;
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(GrpcSerializableValue.AssemblyName));
                writer.WriteValue(argInfo.AssemblyName);
                writer.WritePropertyName(nameof(GrpcSerializableValue.TypeName));
                writer.WriteValue(argInfo.TypeName);
                writer.WritePropertyName(nameof(GrpcSerializableValue.JsonValue));
                serializer.Serialize(writer, argInfo.JsonValue);
                if (argInfo.ErrorData != null)
                {
                    writer.WritePropertyName(nameof(GrpcSerializableValue.ErrorData));
                    serializer.Serialize(writer, argInfo.ErrorData);
                }
                writer.WriteEndObject();
            }
        }
    }

    [DataContract()]
    /// <summary>
    /// Serializable Value wrapper for Exceptions
    /// </summary>
    public class GrpcSerializableErrorData : GrpcSerializableValue
    {
        [DataMember(Order = 1)]
        public override string AssemblyName { get => base.AssemblyName; set => base.AssemblyName = value; }

        [DataMember(Order = 2)]
        public override string TypeName { get => base.TypeName; set => base.TypeName = value; }

        [DataMember(Order = 3)]
        public override string? JsonValue { get => base.JsonValue; set => base.JsonValue = value; }

        public GrpcSerializableErrorData() { }
        public GrpcSerializableErrorData(Exception value)
            : base(value)
        {
        }

        /// <summary>
        /// Creates an exception wrapper of <see cref="GrpcSerializableErrorData{TException}"/> type with the specified <paramref name="caught"/> exception for use with IPC communication
        /// </summary>
        /// <typeparam name="TException">The type of the exception to wrap.</typeparam>
        /// <param name="caught">The exception to wrap.</param>
        /// <returns>An <see cref="GrpcSerializableErrorData"/> object wrapping the specified exception.</returns>
        public static GrpcSerializableErrorData? Create<TException>(TException caught)
            where TException : Exception
        {
            var data = new GrpcSerializableErrorData<TException>(caught);
            return data;
        }

        /// <summary>
        /// Converts the <see cref="ErrorData"/> instance to an <see cref="Exception"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="GrpcSerializableErrorData"/> instance to convert.</param>
        /// <returns>The <see cref="Exception"/> instance represented by the <see cref="GrpcSerializableErrorData"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="value"/> parameter is not an <see cref="GrpcSerializableErrorData"/> instance.</exception>
        public static Exception ToException(GrpcSerializableErrorData value)
        {
            if (value is GrpcSerializableValue exceptionData
                && value.ObjectValue is not null
                && value.ObjectValue is Exception exception)
            {
                return exception;
            }
            else
            {
                throw new ArgumentException("Value must be an Error Data", nameof(value));
            }
        }

        /// <summary>
        /// Converts the current <see cref="GrpcSerializableErrorData"/> instance to an <see cref="Exception"/>.
        /// </summary>
        /// <returns>The <see cref="Exception"/> instance represented by the current <see cref="GrpcSerializableErrorData"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the <see cref="GrpcSerializableValue.ObjectValue"/> property of the current <see cref="GrpcSerializableErrorData"/> instance is not an <see cref="Exception"/>.</exception>

        public Exception ToException()
        {
            if (this.ObjectValue is not null && this.ObjectValue is Exception exception)
                return exception;
            throw new ArgumentException("ObjectValue must be an exception", nameof(ObjectValue));
        }
    }

    [DataContract]
    /// <summary>
    /// Serializable value wrapper for exceptions of type <typeparamref name="TException"/>.
    /// </summary>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    public class GrpcSerializableErrorData<TException> : GrpcSerializableErrorData
        where TException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcSerializableErrorData{TException}"/> class.
        /// </summary>
        public GrpcSerializableErrorData() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcSerializableErrorData{TException}"/> class with the specified <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The exception to wrap.</param>
        public GrpcSerializableErrorData(TException exception)
            : base(exception)
        {

        }
    }
}
