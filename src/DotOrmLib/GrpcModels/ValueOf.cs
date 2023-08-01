using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace DotOrmLib.GrpcModels
{
    using DotMpi;
    using DotOrmLib.GrpcModels.Interfaces;
    using Newtonsoft.Json;
    using Scalars;
    using System.Numerics;
    using YamlDotNet.Core.Tokens;

    namespace Interfaces
    {
        [ServiceContract(Namespace = "http://DotOrmLib.Proxy.Scc1")]
        public interface IGrpcScalarTestService

        {
            ValueTask<Ref<string>> EchoString(Ref<string> request);
            ValueTask<NullableRef<string>> EchoNullableString(NullableRef<string> request);
            ValueTask<Value<int>> EchoInt(Value<int> request);
            ValueTask<NullableValue<int>> EchoNullableInt(NullableValue<int> request);

            ValueTask<Ref<byte[]>> EchoByteArray(Ref<byte[]> request);
            ValueTask<Value<BigInteger>> EchoValueOfBigInteger(Value<BigInteger> request);
            ValueTask<GrpcValue<BigInteger>> EchoGrpcValueOfBigInteger(GrpcValue<BigInteger> request);

            ValueTask<Ref<SerializableValue>> EchoRefOfSerializable(Ref<SerializableValue> request);
            ValueTask<GrpcSerializableValue> EchoGrpcSerializable(GrpcSerializableValue request);
            ValueTask<GrpcSerializableValue<BigInteger>> EchoGrpcSerializableOfBigInteger(GrpcSerializableValue<BigInteger> request);
        }
    }
    namespace Services
    {
        public class GrpcScalarTestService
            : IGrpcScalarTestService
        {
            public ValueTask<NullableRef<string>> EchoNullableString(NullableRef<string> request)
            {
                return request;
            }

            public ValueTask<NullableValue<int>> EchoNullableInt(NullableValue<int> request)
            {
                return request;
            }

            public ValueTask<Ref<string>> EchoString(Ref<string> request)
            {
                return request;
            }

            public ValueTask<Value<int>> EchoInt(Value<int> request)
            {
                return request;
            }

            public ValueTask<Ref<byte[]>> EchoByteArray(Ref<byte[]> request)
            {
                return request;
            }

            public ValueTask<Value<BigInteger>> EchoValueOfBigInteger(Value<BigInteger> request)
            {
                return request;
            }

            public ValueTask<Ref<SerializableValue>> EchoRefOfSerializable(Ref<SerializableValue> request)
            {
                return request;
            }

            public ValueTask<GrpcSerializableValue> EchoGrpcBigInteger(GrpcSerializableValue request)
            {
                return request;
            }

            public ValueTask<GrpcSerializableValue<BigInteger>> EchoGrpcBigInteger(GrpcSerializableValue<BigInteger> request)
            {
                return request;
            }

            public ValueTask<GrpcValue<BigInteger>> EchoGrpcValueOfBigInteger(GrpcValue<BigInteger> request)
            {
                return request;
            }

            public ValueTask<GrpcSerializableValue> EchoGrpcSerializable(GrpcSerializableValue request)
            {
                return request;
            }

            public ValueTask<GrpcSerializableValue<BigInteger>> EchoGrpcSerializableOfBigInteger(GrpcSerializableValue<BigInteger> request)
            {
                return request;
            }
        }
    }
    namespace Scalars
    {

        [DataContract]
        public class Ref<T> 
            where T : class
        {
            [DataMember(Order = 1)]
            public T Item { get; set; }
            public Ref() { Item = default!; }

            public Ref(T value)
            {
                Item = value;
            }

            public static implicit operator Ref<T>(T value)
            {
                return new() { Item = value };
            }

            public static implicit operator T(Ref<T> value)
            {
                return value.Item;
            }

            public static implicit operator ValueTask<Ref<T>>(Ref<T> value)
            {
                return new ValueTask<Ref<T>>(value);
            }
        }



        [DataContract]
        public class NullableRef<T>
        where T : class
        {
            [DataMember(Order = 1)]
            public T? Item { get; set; }
            public NullableRef() { }

            public NullableRef(T? value)
            {
                Item = value;
            }

            public static implicit operator NullableRef<T>(T? value)
            {
                return new() { Item = value };
            }

            public static implicit operator T?(NullableRef<T> value)
            {
                return value.Item;
            }

            public static implicit operator ValueTask<NullableRef<T>>(NullableRef<T> value)
            {
                return new ValueTask<NullableRef<T>>(value);
            }
        }

        [DataContract]
        public class Value<T>
            where T : struct
        {
            [DataMember(Order = 1)]
            public T Item { get; set; }
            public Value() { }

            public Value(T value)
            {
                Item = value;
            }

            public static implicit operator Value<T>(T value)
            {
                return new() { Item = value };
            }

            public static implicit operator T(Value<T> value)
            {
                return value.Item;
            }

            public static implicit operator ValueTask<Value<T>>(Value<T> value)
            {
                return new ValueTask<Value<T>>(value);
            }
        }


        [DataContract]
        public class NullableValue<T>
            where T : struct
        {
            [DataMember(Order = 1)]
            public T? Item { get; set; }
            public NullableValue() { }

            public NullableValue(T? value)
            {
                Item = value;
            }

            public static implicit operator NullableValue<T>(T? value)
            {
                return new() { Item = value };
            }

            public static implicit operator T?(NullableValue<T> value)
            {
                return value.Item;
            }

            public static implicit operator ValueTask<NullableValue<T>>(NullableValue<T> value)
            {
                return new ValueTask<NullableValue<T>>(value);
            }
        }

        [DataContract]
        public class GrpcValue<T>
        {

            [DataMember(Order = 1)]
            public string ObjectData { get; set; }
            private SerializableValue<T>? _item;
            private SerializableValue<T> item
            {
                get
                {
                    if(_item is null)
                    {
                        var result = JsonConvert.DeserializeObject<SerializableValue<T>>(ObjectData);
                        if (result is null)
                            throw new Exception("Failed to deserialize object data");
                        // _item = result;
                        return result;
                    }
                    return _item;
                }
                set
                {
                    ObjectData = JsonConvert.SerializeObject(value);
                    //_item = value;
                }
            }
            //[DataMember(Order = 1)]
            public T? Item { get => item.Result; set => item = new SerializableValue<T>(value); }
            public GrpcValue() { Item = default; }

            public GrpcValue(T value)
            {
                Item = value;
            }


            public static implicit operator GrpcValue<T>(T value)
            {
                return new() { Item = value };
            }

            public static implicit operator T(GrpcValue<T> value)
            {
                return value.Item;
            }

            public static implicit operator ValueTask<GrpcValue<T>>(GrpcValue<T> value)
            {
                return new ValueTask<GrpcValue<T>>(value);
            }
        }
    }
}
