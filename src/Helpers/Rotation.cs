using AsitLib.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AsitLib
{
    public class AsitRotationF : IEquatable<AsitRotationF>, ICloneable, IConvertible
    {
        private float rotation;
        public float Degrees
        {
            get
            {
                rotation = MathFI.Normalize(rotation);
                return rotation;
            }
            set => rotation = MathFI.Normalize(value);
        }
        public float Radiants
        {
            get
            {
                rotation = MathFI.Normalize(rotation);
                return MathFI.ToRadiants(rotation);
            }
            set => rotation = MathFI.Normalize(MathFI.ToDegrees(value));
        }
        public AsitRotationF(float rotation, bool isRadiants)
        {
            rotation = MathFI.Normalize(rotation);
            if (isRadiants) rotation = MathFI.ToDegrees(rotation);
            this.rotation = rotation;
        }
        public static AsitRotationF Zero => new AsitRotationF(0, false);
        public object Clone() => new AsitRotationF(rotation, false);
        public bool Equals([AllowNull] AsitRotationF other)
            => other != null && rotation == other.rotation;
        double GetDoubleValue() => rotation;
        byte IConvertible.ToByte(IFormatProvider? provider)
            => Convert.ToByte(GetDoubleValue());
        char IConvertible.ToChar(IFormatProvider? provider)
            => Convert.ToChar(GetDoubleValue());
        DateTime IConvertible.ToDateTime(IFormatProvider? provider)
            => Convert.ToDateTime(GetDoubleValue());
        decimal IConvertible.ToDecimal(IFormatProvider? provider)
           => Convert.ToDecimal(GetDoubleValue());
        double IConvertible.ToDouble(IFormatProvider?    provider)
            => GetDoubleValue();
        short IConvertible.ToInt16(IFormatProvider? provider)
            => Convert.ToInt16(GetDoubleValue());
        int IConvertible.ToInt32(IFormatProvider? provider)
            => Convert.ToInt32(GetDoubleValue());
        long IConvertible.ToInt64(IFormatProvider? provider)
            => Convert.ToInt64(GetDoubleValue());
        sbyte IConvertible.ToSByte(IFormatProvider? provider)
            => Convert.ToSByte(GetDoubleValue());
        float IConvertible.ToSingle(IFormatProvider? provider)
            => Convert.ToSingle(GetDoubleValue());
        string IConvertible.ToString(IFormatProvider? provider) => "deg:{" + Degrees + "} rad:{" + Radiants + "}";
        object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
            => Convert.ChangeType(GetDoubleValue(), conversionType);
        ushort IConvertible.ToUInt16(IFormatProvider? provider)
            => Convert.ToUInt16(GetDoubleValue());
        uint IConvertible.ToUInt32(IFormatProvider? provider)
            => Convert.ToUInt32(GetDoubleValue());
        ulong IConvertible.ToUInt64(IFormatProvider? provider)
            => Convert.ToUInt64(GetDoubleValue());
        public TypeCode GetTypeCode()
            => TypeCode.Object;
        public bool ToBoolean(IFormatProvider? provider)
            => rotation <= 0;
    }
}
