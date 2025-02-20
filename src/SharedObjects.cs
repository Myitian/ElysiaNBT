using ElysiaNBT.Serialization;
using System.Collections.Frozen;

namespace ElysiaNBT;

internal class SharedObjects
{
    internal static readonly FrozenSet<NbtTagType> AllNormalTypes = FrozenSet.ToFrozenSet([
        NbtTagType.Byte,
        NbtTagType.Short,
        NbtTagType.Int,
        NbtTagType.Long,
        NbtTagType.Float,
        NbtTagType.Double,
        NbtTagType.ByteArray,
        NbtTagType.String,
        NbtTagType.List,
        NbtTagType.Compound,
        NbtTagType.IntArray,
        NbtTagType.LongArray
    ]);
    internal static readonly FrozenSet<NbtTagType> IntAcceptedTypes = FrozenSet.ToFrozenSet([
        NbtTagType.Byte,
        NbtTagType.Short,
        NbtTagType.Int,
        NbtTagType.Long
    ]);
    internal static readonly FrozenSet<NbtTagType> FloatAcceptedTypes = FrozenSet.ToFrozenSet([
        NbtTagType.Byte,
        NbtTagType.Short,
        NbtTagType.Int,
        NbtTagType.Long,
        NbtTagType.Float,
        NbtTagType.Double
    ]);
    internal static readonly FrozenSet<NbtTagType> Byte = FrozenSet.ToFrozenSet([NbtTagType.Byte]);
    internal static readonly FrozenSet<NbtTagType> Short = FrozenSet.ToFrozenSet([NbtTagType.Short]);
    internal static readonly FrozenSet<NbtTagType> Int = FrozenSet.ToFrozenSet([NbtTagType.Int]);
    internal static readonly FrozenSet<NbtTagType> Long = FrozenSet.ToFrozenSet([NbtTagType.Long]);
    internal static readonly FrozenSet<NbtTagType> Float = FrozenSet.ToFrozenSet([NbtTagType.Float]);
    internal static readonly FrozenSet<NbtTagType> Double = FrozenSet.ToFrozenSet([NbtTagType.Double]);
    internal static readonly FrozenSet<NbtTagType> ByteArray = FrozenSet.ToFrozenSet([NbtTagType.ByteArray]);
    internal static readonly FrozenSet<NbtTagType> IntArray = FrozenSet.ToFrozenSet([NbtTagType.IntArray]);
    internal static readonly FrozenSet<NbtTagType> LongArray = FrozenSet.ToFrozenSet([NbtTagType.LongArray]);
    internal static readonly FrozenSet<NbtTagType> String = FrozenSet.ToFrozenSet([NbtTagType.String]);
    internal static readonly FrozenSet<NbtTagType> Compound = FrozenSet.ToFrozenSet([NbtTagType.Compound]);
    internal static readonly FrozenSet<NbtTagType> List = FrozenSet.ToFrozenSet([NbtTagType.List]);
    internal static readonly FrozenSet<NbtTagType> ListLike = FrozenSet.ToFrozenSet([NbtTagType.List, NbtTagType.ByteArray, NbtTagType.IntArray, NbtTagType.LongArray]);
    internal static readonly FrozenSet<NbtTagType> ListByteArray = FrozenSet.ToFrozenSet([NbtTagType.List, NbtTagType.ByteArray]);
    internal static readonly FrozenSet<NbtTagType> ListIntArray = FrozenSet.ToFrozenSet([NbtTagType.List, NbtTagType.IntArray]);
    internal static readonly FrozenSet<NbtTagType> ListLongArray = FrozenSet.ToFrozenSet([NbtTagType.List, NbtTagType.LongArray]);

    internal static readonly Type TypeBool = typeof(bool);
    internal static readonly Type TypeSByte = typeof(sbyte);
    internal static readonly Type TypeByte = typeof(byte);
    internal static readonly Type TypeShort = typeof(short);
    internal static readonly Type TypeUShort = typeof(ushort);
    internal static readonly Type TypeInt = typeof(int);
    internal static readonly Type TypeUInt = typeof(uint);
    internal static readonly Type TypeLong = typeof(long);
    internal static readonly Type TypeULong = typeof(ulong);
    internal static readonly Type TypeChar = typeof(char);
    internal static readonly Type TypeFloat = typeof(float);
    internal static readonly Type TypeDouble = typeof(double);
    internal static readonly Type TypeString = typeof(string);
    internal static readonly Type TypeObject = typeof(object);
    internal static readonly Type TypeVoid = typeof(void);
    internal static readonly Type TypeNbtConverter = typeof(NbtConverter<>);
    internal static readonly Type TypeINbtConverter = typeof(INbtConverter);
    internal static readonly Type TypeIReadOnlyNbtConverter = typeof(IReadOnlyNbtConverter<>);
    internal static readonly Type TypeIWriteOnlyNbtConverter = typeof(IWriteOnlyNbtConverter<>);
    internal static readonly Type[] NbtConverterMethodReadNbtParams = [typeof(INbtReader), typeof(NbtSerializerContext)];

    internal static readonly FrozenDictionary<Type, NbtTagType> BasicTagTypeMappings = new Dictionary<Type, NbtTagType>()
    {
        { TypeBool, NbtTagType.Byte },
        { TypeSByte, NbtTagType.Byte },
        { TypeByte, NbtTagType.Byte },
        { TypeShort, NbtTagType.Short },
        { TypeUShort, NbtTagType.Short },
        { TypeInt, NbtTagType.Int },
        { TypeUInt, NbtTagType.Int },
        { TypeLong, NbtTagType.Long },
        { TypeULong, NbtTagType.Long },
        { TypeFloat, NbtTagType.Float },
        { TypeDouble, NbtTagType.Double },
        { TypeString, NbtTagType.String }
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, FrozenSet<NbtTagType>> BasicTypeMappings = new Dictionary<Type, FrozenSet<NbtTagType>>()
    {
        { TypeBool, Byte },
        { TypeSByte, Byte },
        { TypeByte, Byte },
        { TypeShort, Short },
        { TypeUShort, Short },
        { TypeInt, Int },
        { TypeUInt, Int },
        { TypeLong, Long },
        { TypeULong, Long },
        { TypeFloat, Float },
        { TypeDouble, Double },
        { TypeString, String }
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, FrozenSet<NbtTagType>> AcceeptedTypeMappings = new Dictionary<Type, FrozenSet<NbtTagType>>()
    {
        { TypeBool, IntAcceptedTypes },
        { TypeSByte, IntAcceptedTypes },
        { TypeByte, IntAcceptedTypes },
        { TypeShort, IntAcceptedTypes },
        { TypeUShort, IntAcceptedTypes },
        { TypeInt, IntAcceptedTypes },
        { TypeUInt, IntAcceptedTypes },
        { TypeLong, IntAcceptedTypes },
        { TypeULong, IntAcceptedTypes },
        { TypeFloat, FloatAcceptedTypes },
        { TypeDouble, FloatAcceptedTypes },
        { TypeString, String }
    }.ToFrozenDictionary();
    internal static class ListType<T>
    {
        private static readonly Type _type = typeof(T);
        internal static readonly FrozenSet<NbtTagType> Accepted =
            _type == TypeSByte || _type == TypeByte ? ListByteArray :
            _type == TypeInt || _type == TypeUInt ? ListIntArray :
            _type == TypeLong || _type == TypeULong ? ListLongArray :
            List;
    }
}
