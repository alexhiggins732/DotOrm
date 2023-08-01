


using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Numerics;


namespace DotOrmLib.GrpcModels
{
    public partial class GrpcScalars
    {
        
        [DataContract]
        public class ByteValue
        {
            
            [DataMember(Order = 1)]
            public byte Value { get; set; }
            public ByteValue() {}

            public ByteValue(byte value)
            {
                Value = value;
            }

            public static implicit operator ByteValue(byte value)
            {
                return new() {Value = value};
            }

            public static implicit operator byte(ByteValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class SByteValue
        {
            
            [DataMember(Order = 1)]
            public sbyte Value { get; set; }
            public SByteValue() {}

            public SByteValue(sbyte value)
            {
                Value = value;
            }

            public static implicit operator SByteValue(sbyte value)
            {
                return new() {Value = value};
            }

            public static implicit operator sbyte(SByteValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class ShortValue
        {
            
            [DataMember(Order = 1)]
            public short Value { get; set; }
            public ShortValue() {}

            public ShortValue(short value)
            {
                Value = value;
            }

            public static implicit operator ShortValue(short value)
            {
                return new() {Value = value};
            }

            public static implicit operator short(ShortValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class UShortValue
        {
            
            [DataMember(Order = 1)]
            public ushort Value { get; set; }
            public UShortValue() {}

            public UShortValue(ushort value)
            {
                Value = value;
            }

            public static implicit operator UShortValue(ushort value)
            {
                return new() {Value = value};
            }

            public static implicit operator ushort(UShortValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class IntValue
        {
            
            [DataMember(Order = 1)]
            public int Value { get; set; }
            public IntValue() {}

            public IntValue(int value)
            {
                Value = value;
            }

            public static implicit operator IntValue(int value)
            {
                return new() {Value = value};
            }

            public static implicit operator int(IntValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class UIntValue
        {
            
            [DataMember(Order = 1)]
            public uint Value { get; set; }
            public UIntValue() {}

            public UIntValue(uint value)
            {
                Value = value;
            }

            public static implicit operator UIntValue(uint value)
            {
                return new() {Value = value};
            }

            public static implicit operator uint(UIntValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class LongValue
        {
            
            [DataMember(Order = 1)]
            public long Value { get; set; }
            public LongValue() {}

            public LongValue(long value)
            {
                Value = value;
            }

            public static implicit operator LongValue(long value)
            {
                return new() {Value = value};
            }

            public static implicit operator long(LongValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class ULongValue
        {
            
            [DataMember(Order = 1)]
            public ulong Value { get; set; }
            public ULongValue() {}

            public ULongValue(ulong value)
            {
                Value = value;
            }

            public static implicit operator ULongValue(ulong value)
            {
                return new() {Value = value};
            }

            public static implicit operator ulong(ULongValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class FloatValue
        {
            
            [DataMember(Order = 1)]
            public float Value { get; set; }
            public FloatValue() {}

            public FloatValue(float value)
            {
                Value = value;
            }

            public static implicit operator FloatValue(float value)
            {
                return new() {Value = value};
            }

            public static implicit operator float(FloatValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class DoubleValue
        {
            
            [DataMember(Order = 1)]
            public double Value { get; set; }
            public DoubleValue() {}

            public DoubleValue(double value)
            {
                Value = value;
            }

            public static implicit operator DoubleValue(double value)
            {
                return new() {Value = value};
            }

            public static implicit operator double(DoubleValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class DecimalValue
        {
            
            [DataMember(Order = 1)]
            public decimal Value { get; set; }
            public DecimalValue() {}

            public DecimalValue(decimal value)
            {
                Value = value;
            }

            public static implicit operator DecimalValue(decimal value)
            {
                return new() {Value = value};
            }

            public static implicit operator decimal(DecimalValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class BigIntegerValue
        {
            
            [DataMember(Order = 1)]
            public BigInteger Value { get; set; }
            public BigIntegerValue() {}

            public BigIntegerValue(BigInteger value)
            {
                Value = value;
            }

            public static implicit operator BigIntegerValue(BigInteger value)
            {
                return new() {Value = value};
            }

            public static implicit operator BigInteger(BigIntegerValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class DateTimeValue
        {
            
            [DataMember(Order = 1)]
            public DateTime Value { get; set; }
            public DateTimeValue() {}

            public DateTimeValue(DateTime value)
            {
                Value = value;
            }

            public static implicit operator DateTimeValue(DateTime value)
            {
                return new() {Value = value};
            }

            public static implicit operator DateTime(DateTimeValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class DateTimeOffsetValue
        {
            
            [DataMember(Order = 1)]
            public DateTimeOffset Value { get; set; }
            public DateTimeOffsetValue() {}

            public DateTimeOffsetValue(DateTimeOffset value)
            {
                Value = value;
            }

            public static implicit operator DateTimeOffsetValue(DateTimeOffset value)
            {
                return new() {Value = value};
            }

            public static implicit operator DateTimeOffset(DateTimeOffsetValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class StringValue
        {
            
            [DataMember(Order = 1)]
            public string Value { get; set; } = null!; 
            public StringValue() {}

            public StringValue(string value)
            {
                Value = value;
            }

            public static implicit operator StringValue(string value)
            {
                return new() {Value = value};
            }

            public static implicit operator string(StringValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class BooleanValue
        {
            
            [DataMember(Order = 1)]
            public bool Value { get; set; }
            public BooleanValue() {}

            public BooleanValue(bool value)
            {
                Value = value;
            }

            public static implicit operator BooleanValue(bool value)
            {
                return new() {Value = value};
            }

            public static implicit operator bool(BooleanValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class CharValue
        {
            
            [DataMember(Order = 1)]
            public char Value { get; set; }
            public CharValue() {}

            public CharValue(char value)
            {
                Value = value;
            }

            public static implicit operator CharValue(char value)
            {
                return new() {Value = value};
            }

            public static implicit operator char(CharValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class GuidValue
        {
            
            [DataMember(Order = 1)]
            public Guid Value { get; set; }
            public GuidValue() {}

            public GuidValue(Guid value)
            {
                Value = value;
            }

            public static implicit operator GuidValue(Guid value)
            {
                return new() {Value = value};
            }

            public static implicit operator Guid(GuidValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableByteValue
        {
            
            [DataMember(Order = 1)]
            public byte? Value { get; set; }
            public NullableByteValue() {}

            public NullableByteValue(byte? value)
            {
                Value = value;
            }

            public static implicit operator NullableByteValue(byte? value)
            {
                return new() {Value = value};
            }

            public static implicit operator byte?(NullableByteValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableSByteValue
        {
            
            [DataMember(Order = 1)]
            public sbyte? Value { get; set; }
            public NullableSByteValue() {}

            public NullableSByteValue(sbyte? value)
            {
                Value = value;
            }

            public static implicit operator NullableSByteValue(sbyte? value)
            {
                return new() {Value = value};
            }

            public static implicit operator sbyte?(NullableSByteValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableShortValue
        {
            
            [DataMember(Order = 1)]
            public short? Value { get; set; }
            public NullableShortValue() {}

            public NullableShortValue(short? value)
            {
                Value = value;
            }

            public static implicit operator NullableShortValue(short? value)
            {
                return new() {Value = value};
            }

            public static implicit operator short?(NullableShortValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableUShortValue
        {
            
            [DataMember(Order = 1)]
            public ushort? Value { get; set; }
            public NullableUShortValue() {}

            public NullableUShortValue(ushort? value)
            {
                Value = value;
            }

            public static implicit operator NullableUShortValue(ushort? value)
            {
                return new() {Value = value};
            }

            public static implicit operator ushort?(NullableUShortValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableIntValue
        {
            
            [DataMember(Order = 1)]
            public int? Value { get; set; }
            public NullableIntValue() {}

            public NullableIntValue(int? value)
            {
                Value = value;
            }

            public static implicit operator NullableIntValue(int? value)
            {
                return new() {Value = value};
            }

            public static implicit operator int?(NullableIntValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableUIntValue
        {
            
            [DataMember(Order = 1)]
            public uint? Value { get; set; }
            public NullableUIntValue() {}

            public NullableUIntValue(uint? value)
            {
                Value = value;
            }

            public static implicit operator NullableUIntValue(uint? value)
            {
                return new() {Value = value};
            }

            public static implicit operator uint?(NullableUIntValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableLongValue
        {
            
            [DataMember(Order = 1)]
            public long? Value { get; set; }
            public NullableLongValue() {}

            public NullableLongValue(long? value)
            {
                Value = value;
            }

            public static implicit operator NullableLongValue(long? value)
            {
                return new() {Value = value};
            }

            public static implicit operator long?(NullableLongValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableULongValue
        {
            
            [DataMember(Order = 1)]
            public ulong? Value { get; set; }
            public NullableULongValue() {}

            public NullableULongValue(ulong? value)
            {
                Value = value;
            }

            public static implicit operator NullableULongValue(ulong? value)
            {
                return new() {Value = value};
            }

            public static implicit operator ulong?(NullableULongValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableFloatValue
        {
            
            [DataMember(Order = 1)]
            public float? Value { get; set; }
            public NullableFloatValue() {}

            public NullableFloatValue(float? value)
            {
                Value = value;
            }

            public static implicit operator NullableFloatValue(float? value)
            {
                return new() {Value = value};
            }

            public static implicit operator float?(NullableFloatValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableDoubleValue
        {
            
            [DataMember(Order = 1)]
            public double? Value { get; set; }
            public NullableDoubleValue() {}

            public NullableDoubleValue(double? value)
            {
                Value = value;
            }

            public static implicit operator NullableDoubleValue(double? value)
            {
                return new() {Value = value};
            }

            public static implicit operator double?(NullableDoubleValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableDecimalValue
        {
            
            [DataMember(Order = 1)]
            public decimal? Value { get; set; }
            public NullableDecimalValue() {}

            public NullableDecimalValue(decimal? value)
            {
                Value = value;
            }

            public static implicit operator NullableDecimalValue(decimal? value)
            {
                return new() {Value = value};
            }

            public static implicit operator decimal?(NullableDecimalValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableBigIntegerValue
        {
            
            [DataMember(Order = 1)]
            public BigInteger? Value { get; set; }
            public NullableBigIntegerValue() {}

            public NullableBigIntegerValue(BigInteger? value)
            {
                Value = value;
            }

            public static implicit operator NullableBigIntegerValue(BigInteger? value)
            {
                return new() {Value = value};
            }

            public static implicit operator BigInteger?(NullableBigIntegerValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableDateTimeValue
        {
            
            [DataMember(Order = 1)]
            public DateTime? Value { get; set; }
            public NullableDateTimeValue() {}

            public NullableDateTimeValue(DateTime? value)
            {
                Value = value;
            }

            public static implicit operator NullableDateTimeValue(DateTime? value)
            {
                return new() {Value = value};
            }

            public static implicit operator DateTime?(NullableDateTimeValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableDateTimeOffsetValue
        {
            
            [DataMember(Order = 1)]
            public DateTimeOffset? Value { get; set; }
            public NullableDateTimeOffsetValue() {}

            public NullableDateTimeOffsetValue(DateTimeOffset? value)
            {
                Value = value;
            }

            public static implicit operator NullableDateTimeOffsetValue(DateTimeOffset? value)
            {
                return new() {Value = value};
            }

            public static implicit operator DateTimeOffset?(NullableDateTimeOffsetValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableStringValue
        {
            
            [DataMember(Order = 1)]
            public string? Value { get; set; }
            public NullableStringValue() {}

            public NullableStringValue(string? value)
            {
                Value = value;
            }

            public static implicit operator NullableStringValue(string? value)
            {
                return new() {Value = value};
            }

            public static implicit operator string?(NullableStringValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableBooleanValue
        {
            
            [DataMember(Order = 1)]
            public bool? Value { get; set; }
            public NullableBooleanValue() {}

            public NullableBooleanValue(bool? value)
            {
                Value = value;
            }

            public static implicit operator NullableBooleanValue(bool? value)
            {
                return new() {Value = value};
            }

            public static implicit operator bool?(NullableBooleanValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableCharValue
        {
            
            [DataMember(Order = 1)]
            public char? Value { get; set; }
            public NullableCharValue() {}

            public NullableCharValue(char? value)
            {
                Value = value;
            }

            public static implicit operator NullableCharValue(char? value)
            {
                return new() {Value = value};
            }

            public static implicit operator char?(NullableCharValue value)
            {
                return value.Value;
            }
        }
   
        [DataContract]
        public class NullableGuidValue
        {
            
            [DataMember(Order = 1)]
            public Guid? Value { get; set; }
            public NullableGuidValue() {}

            public NullableGuidValue(Guid? value)
            {
                Value = value;
            }

            public static implicit operator NullableGuidValue(Guid? value)
            {
                return new() {Value = value};
            }

            public static implicit operator Guid?(NullableGuidValue value)
            {
                return value.Value;
            }
        }
   
    }
}