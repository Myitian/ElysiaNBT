using ElysiaNBT.Serialization.Converters;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ElysiaNBT.Serialization;

[RequiresDynamicCode("Type.MakeGenericType()")]
[RequiresUnreferencedCode("Type.GetInterfaces()")]
public class ReflectionNbtSerializerContext : NbtSerializerContext
{
    protected static readonly Lock _inheritanceDepthInfoCacheLock = new();
    protected static readonly Dictionary<Type, ReadOnlyCollection<ReadOnlyCollection<Type>>> _inheritanceDepthInfoCache = [];
    protected static readonly Lock _converterCacheLock = new();
    protected static readonly Dictionary<Type, INbtConverter> _converterCache = [];
    protected static readonly Lock _defaultIgnoreConditionLock = new();
    protected static readonly Dictionary<Type, NbtIgnoreCondition?> _defaultIgnoreCondition = [];
    protected readonly static FrozenSet<Type> _builtinConverters = new Type[]
    {
        typeof(BoolNbtConverter),
        typeof(SByteNbtConverter),
        typeof(ByteNbtConverter),
        typeof(ShortNbtConverter),
        typeof(UShortNbtConverter),
        typeof(IntNbtConverter),
        typeof(UIntNbtConverter),
        typeof(LongNbtConverter),
        typeof(ULongNbtConverter),
        typeof(FloatNbtConverter),
        typeof(DoubleNbtConverter),
        typeof(StringNbtConverter),
        typeof(CharArrayNbtConverter),
        typeof(SByteArrayNbtConverter),
        typeof(ByteArrayNbtConverter),
        typeof(IntArrayNbtConverter),
        typeof(UIntArrayNbtConverter),
        typeof(LongArrayNbtConverter),
        typeof(ULongArrayNbtConverter),
        typeof(ExpandoObjectNbtConverter),
        typeof(Vector2NbtConverter),
        typeof(Vector3NbtConverter),
        typeof(Vector4NbtConverter),
        typeof(QuaternionNbtConverter),
        typeof(BigIntegerNbtConverter),
        typeof(DecimalNbtConverter),
        typeof(ArrayListNbtConverter),
        typeof(IEnumerableNbtConverter),
        typeof(DictionaryLikeNbtConverter),
        typeof(ArrayNbtConverter<>),
        typeof(ListNbtConverter<>),
        typeof(IEnumerableNbtConverter<>),
        typeof(DictionaryLikeNbtConverter<>),
        typeof(EnumNbtConverter<>),
        typeof(NullableNbtConverter<>),
    }.ToFrozenSet();
    protected readonly static FrozenDictionary<Type, INbtConverter> _basicTypeConverters = new Dictionary<Type, INbtConverter>()
    {
        { SharedObjects.TypeBool, BoolNbtConverter.Instance },
        { SharedObjects.TypeByte, ByteNbtConverter.Instance },
        { SharedObjects.TypeSByte, SByteNbtConverter.Instance },
        { SharedObjects.TypeShort, ShortNbtConverter.Instance },
        { SharedObjects.TypeUShort, UShortNbtConverter.Instance },
        { SharedObjects.TypeInt, IntNbtConverter.Instance },
        { SharedObjects.TypeUInt, UIntNbtConverter.Instance },
        { SharedObjects.TypeLong, LongNbtConverter.Instance },
        { SharedObjects.TypeULong, ULongNbtConverter.Instance },
        { SharedObjects.TypeFloat, FloatNbtConverter.Instance },
        { SharedObjects.TypeDouble, DoubleNbtConverter.Instance },
        { SharedObjects.TypeString, StringNbtConverter.Instance },
        { typeof(char[]), CharArrayNbtConverter.Instance },
        { typeof(byte[]), ByteArrayNbtConverter.Instance },
        { typeof(sbyte[]), SByteArrayNbtConverter.Instance },
        { typeof(int[]), IntArrayNbtConverter.Instance },
        { typeof(uint[]), UIntArrayNbtConverter.Instance },
        { typeof(long[]), LongArrayNbtConverter.Instance },
        { typeof(ulong[]), ULongArrayNbtConverter.Instance },
    }.ToFrozenDictionary();

    public static readonly ReflectionNbtSerializerContext Default = new();

    protected readonly Dictionary<Type, INbtConverter> _readConverterCache = [];
    protected readonly Dictionary<Type, INbtConverter> _writeConverterCache = [];
    protected readonly Dictionary<Type, INbtConverter> _reflectionConverterCache = [];
    private readonly Dictionary<(Type, INbtConverter), object> _wrapperCache = [];

    public ReflectionNbtConverterParameters ReflectionNbtConverterParameters { get; init; } = new();
    public FrozenSet<Type> RegisteredReadConverters { get; init; } = [];
    public FrozenSet<Type> RegisteredWriteConverters { get; init; } = [];
    public FrozenDictionary<Type, INbtConverter> ReadConverterOverrides { get; init; } = FrozenDictionary<Type, INbtConverter>.Empty;
    public FrozenDictionary<Type, INbtConverter> WriteConverterOverrides { get; init; } = FrozenDictionary<Type, INbtConverter>.Empty;
    public override NbtConverter<dynamic>? DynamicNbtConverterInstance { get; init; } = DynamicNbtConverter.Instance;
    public override NbtConverter<List<dynamic>>? ListDynamicNbtConverterInstance { get; init; } = ListDynamicNbtConverter.Instance;

    public ReflectionNbtSerializerContext WithConverterTypes(IEnumerable<Type>? read = null, IEnumerable<Type>? write = null)
    {
        return new()
        {
            RegisteredReadConverters = read is null ? RegisteredReadConverters : [.. RegisteredReadConverters, .. read],
            RegisteredWriteConverters = write is null ? RegisteredWriteConverters : [.. RegisteredWriteConverters, .. write]
        };
    }
    public virtual ReflectionNbtSerializerContext WithConverterOverrides(
        IEnumerable<KeyValuePair<Type, INbtConverter>>? read = null,
        IEnumerable<KeyValuePair<Type, INbtConverter>>? write = null)
    {
        return new()
        {
            ReadConverterOverrides = read is null ?
                ReadConverterOverrides :
                ReadConverterOverrides.Concat(read).ToFrozenDictionary(),
            WriteConverterOverrides = write is null ?
                WriteConverterOverrides :
                WriteConverterOverrides.Concat(write).ToFrozenDictionary(),
        };
    }
    public static NbtIgnoreCondition? GetDefaultIgnoreStatus(Type type)
    {
        lock (_defaultIgnoreCondition)
        {
            if (!_defaultIgnoreCondition.TryGetValue(type, out NbtIgnoreCondition? cond))
                cond = _defaultIgnoreCondition[type] = type.GetCustomAttribute<NbtIgnoreAttribute>()?.Condition;
            return cond;
        }
    }

    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static bool IsSuitable(
        Type type,
        Type target,
        out int depth,
        bool isReadNbt = true)
    {
        if (isReadNbt)
        {
            ReadOnlyCollection<ReadOnlyCollection<Type>> inheritInfo = GetInheritanceDepthInfo(target);
            for (int i = 0; i < inheritInfo.Count; i++)
            {
                foreach (Type t in inheritInfo[i])
                {
                    if (t == type)
                    {
                        depth = i;
                        return true;
                    }
                }
            }
        }
        else
        {
            ReadOnlyCollection<ReadOnlyCollection<Type>> inheritInfo = GetInheritanceDepthInfo(type);
            for (int i = 0; i < inheritInfo.Count; i++)
            {
                foreach (Type t in inheritInfo[i])
                {
                    if (target == t)
                    {
                        depth = i;
                        return true;
                    }
                }
            }
        }
        depth = -1;
        return false;
    }
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static bool TryMapTo(
        Type type,
        Type converter,
        out int depth,
        [MaybeNullWhen(false)] out Type declaredConverterType,
        [MaybeNullWhen(false)] out Dictionary<Type, Type> mapping,
        bool isReadNbt = true)
    {
        ReadOnlyCollection<ReadOnlyCollection<Type>> converterInheritInfo = GetInheritanceDepthInfo(converter);
        Type targetInterfaceType = isReadNbt ? SharedObjects.TypeIReadOnlyNbtConverter : SharedObjects.TypeIWriteOnlyNbtConverter;
        Type? targetInterface = null;
        bool hasINbtConverter = false;
        foreach (ReadOnlyCollection<Type> types in converterInheritInfo)
        {
            foreach (Type t in types)
            {
                if (t.IsGenericType && targetInterface is null && t.GetGenericTypeDefinition() == targetInterfaceType)
                    targetInterface = t;
                else if (!hasINbtConverter && t == SharedObjects.TypeINbtConverter)
                    hasINbtConverter = true;
            }
        }
        if (!hasINbtConverter || targetInterface is null)
            goto FAILED;
        mapping = [];
        declaredConverterType = targetInterface.GenericTypeArguments[0];
        if (isReadNbt)
        {
            ReadOnlyCollection<ReadOnlyCollection<Type>> inheritInfo = GetInheritanceDepthInfo(declaredConverterType);
            for (int i = 0; i < inheritInfo.Count; i++)
            {
                foreach (Type t in inheritInfo[i])
                {
                    mapping.Clear();
                    if (MapParameters(t, type, mapping))
                    {
                        depth = i;
                        return true;
                    }
                }
            }
        }
        else
        {
            ReadOnlyCollection<ReadOnlyCollection<Type>> inheritInfo = GetInheritanceDepthInfo(type);
            for (int i = 0; i < inheritInfo.Count; i++)
            {
                foreach (Type t in inheritInfo[i])
                {
                    mapping.Clear();
                    if (MapParameters(declaredConverterType, t, mapping))
                    {
                        depth = i;
                        return true;
                    }
                }
            }
        }
    FAILED:
        depth = -1;
        declaredConverterType = null;
        mapping = null;
        return false;
    }
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static ReadOnlyCollection<ReadOnlyCollection<Type>> GetInheritanceDepthInfo(Type type)
    {
        lock (_inheritanceDepthInfoCacheLock)
        {
            if (_inheritanceDepthInfoCache.TryGetValue(type, out ReadOnlyCollection<ReadOnlyCollection<Type>>? r))
                return r;
            List<HashSet<Type>> tmp = [[type]];
            Dictionary<Type, int> depth = new() { { type, 0 } };
            for (int i = 0; ;)
            {
                HashSet<Type> previousLayer = tmp[i++];
                HashSet<Type> currentLayer = [];
                foreach (Type t in previousLayer)
                {
                    Type? tBase = t.BaseType;
                    if (tBase is not null && tBase != SharedObjects.TypeObject)
                    {
                        if (depth.TryGetValue(tBase, out int td))
                            tmp[td].Remove(tBase);
                        depth[tBase] = i;
                        currentLayer.Add(tBase);
                    }
                    Type[] interfaces = t.GetInterfaces();
                    foreach (Type tInterface in interfaces)
                    {
                        if (depth.TryGetValue(tInterface, out int td) && td != i)
                            tmp[td].Remove(tInterface);
                        depth[tInterface] = i;
                        currentLayer.Add(tInterface);
                    }
                }
                if (currentLayer.Count > 0)
                    tmp.Add(currentLayer);
                else
                    break;
            }
            ReadOnlyCollection<Type>[] result = new ReadOnlyCollection<Type>[tmp.Count];
            for (int i = 0; i < result.Length; i++)
            {
                Type[] types = [.. tmp[i]];
                Array.Sort(types, Comapre);
                result[i] = types.AsReadOnly();
            }
            return _inheritanceDepthInfoCache[type] = result.AsReadOnly();
        }

        static int Comapre(Type type1, Type type2)
        {
            bool i1 = type1.IsInterface;
            bool i2 = type2.IsInterface;
            if (i1 != i2)
                return i2 ? 1 : -1;
            Type[] ga1 = type1.GetGenericArguments();
            Type[] ga2 = type2.GetGenericArguments();
            return ga2.Length.CompareTo(ga1.Length);
        }
    }
    public static bool MapParameters(Type a, Type b, Dictionary<Type, Type> mappings)
    {
        if (a.IsGenericParameter)
        {
            if (b.IsByRef)
                return false;
            if (mappings.TryGetValue(a, out Type? b0))
                return b0 == b;
            GenericParameterAttributes attr = a.GenericParameterAttributes;
            if ((attr & GenericParameterAttributes.NotNullableValueTypeConstraint) is not GenericParameterAttributes.None
                && !b.IsValueType)
                return false;
            if ((attr & GenericParameterAttributes.ReferenceTypeConstraint) is not GenericParameterAttributes.None
                && b.IsValueType)
                return false;
            if ((attr & GenericParameterAttributes.AllowByRefLike) is GenericParameterAttributes.None
                && b.IsByRefLike)
                return false;
            foreach (Type constraint in a.GetGenericParameterConstraints())
            {
                if (!constraint.IsInterface && !constraint.IsGenericType && !b.IsAssignableTo(constraint))
                    return false;
            }
            mappings[a] = b;
            return true;
        }
        bool gtA = a.IsGenericType, gtB = b.IsGenericType;
        if (gtA != gtB)
            return false;
        if (!gtA)
        {
            bool aA = a.IsArray, aB = b.IsArray;
            if (!aA)
                return a == b;
            if (aA != aB || a.GetArrayRank() != b.GetArrayRank())
                return false;
            Type? atA = a.GetElementType(), atB = b.GetElementType();
            return atA is not null && atB is not null && MapParameters(atA, atB, mappings);
        }
        Type gA = a.GetGenericTypeDefinition(), gB = b.GetGenericTypeDefinition();
        if (gA != gB)
            return false;
        ReadOnlySpan<Type> gpA = a.GetGenericArguments(), gpB = b.GetGenericArguments();
        if (gpA.Length != gpB.Length)
            return false;
        for (int i = 0; i < gpA.Length; i++)
        {
            if (!MapParameters(gpA[i], gpB[i], mappings))
                return false;
        }
        return true;
    }

    public void ClearCache()
    {
        foreach (INbtConverter converter in _readConverterCache.Values)
            (converter as IDisposable)?.Dispose();
        foreach (INbtConverter converter in _writeConverterCache.Values)
            (converter as IDisposable)?.Dispose();
        foreach (INbtConverter converter in _reflectionConverterCache.Values)
            (converter as IDisposable)?.Dispose();
        foreach (((Type _, INbtConverter baseConverter), object wrapper) in _wrapperCache)
        {
            (wrapper as IDisposable)?.Dispose();
            (wrapper as IDisposable)?.Dispose();
        }
        _readConverterCache.Clear();
        _writeConverterCache.Clear();
        _reflectionConverterCache.Clear();
        _wrapperCache.Clear();
    }
    public static void ClearStaticCache()
    {
        lock (_converterCacheLock)
        {
            foreach (INbtConverter converter in _converterCache.Values)
                (converter as IDisposable)?.Dispose();
            _converterCache.Clear();
        }
        _inheritanceDepthInfoCache.Clear();
    }

    public static INbtConverter? GetConverterByType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type? type)
    {
        if (type is null)
            return null;
        lock (_converterCacheLock)
        {
            if (_converterCache.TryGetValue(type, out INbtConverter? converterC))
                return converterC;
            object? instance = Activator.CreateInstance(type);
            if (instance is INbtConverter converter)
                return _converterCache[type] = converter;
            (instance as IDisposable)?.Dispose();
        }
        throw new Exception();
    }
    public object GetWrapperByType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type,
        INbtConverter baseConverter)
    {
        (Type, INbtConverter) key = (type, baseConverter);
        if (_wrapperCache.TryGetValue(key, out object? wrapper))
            return wrapper;
        object? instance = Activator.CreateInstance(type, [baseConverter]);
        if (instance is not null)
            return _wrapperCache[key] = instance;
        throw new Exception();
    }

    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public INbtConverter? ResolveTypeConverter(Type src, bool isRead = true)
    {
        if (src == SharedObjects.TypeObject)
            return ObjectNbtConverterInstance;
        FrozenDictionary<Type, INbtConverter>? dictOverrides;
        Dictionary<Type, INbtConverter> dictCache;
        if (isRead)
        {
            dictOverrides = ReadConverterOverrides;
            dictCache = _readConverterCache;
        }
        else
        {
            dictOverrides = WriteConverterOverrides;
            dictCache = _writeConverterCache;
        }
        if (dictCache.TryGetValue(src, out INbtConverter? bestFitConverter))
            return bestFitConverter;
        if (_reflectionConverterCache.TryGetValue(src, out bestFitConverter))
            return bestFitConverter;
        if (dictOverrides?.TryGetValue(src, out bestFitConverter) is true)
            return bestFitConverter;
        if (_basicTypeConverters.TryGetValue(src, out bestFitConverter))
            return bestFitConverter;
        int minDepth = int.MaxValue;
        ReadOnlyCollection<ReadOnlyCollection<Type>> inheritInfo = GetInheritanceDepthInfo(src);
        for (int i = 0; i < inheritInfo.Count; i++)
        {
            foreach (Type t in inheritInfo[i])
            {
                NbtConverterAttribute? converterAttr = t.GetCustomAttribute<NbtConverterAttribute>();
                if (converterAttr is not null)
                {
                    minDepth = i;
                    bestFitConverter = GetConverterByType(converterAttr.Converter);
                    break;
                }
            }
        }
        if (dictOverrides is not null)
        {
            foreach ((Type t, INbtConverter c) in dictOverrides)
            {
                if (!IsSuitable(src, t, out int depth, isRead))
                    continue;
                if (depth >= minDepth)
                    continue;
                minDepth = depth;
                bestFitConverter = c;
            }
        }

        foreach ((Type t, INbtConverter c) in dictCache)
        {
            if (!IsSuitable(src, t, out int depth, isRead))
                continue;
            if (depth >= minDepth)
                continue;
            minDepth = depth;
            bestFitConverter = c;
        }
        if (bestFitConverter is not null)
            dictCache[src] = bestFitConverter;
        return bestFitConverter;
    }

    public override INbtConverter GetDefaultReadConverter(Type type)
    {
        INbtConverter? bestFitConverter = ResolveTypeConverter(type, true);
        if (bestFitConverter is not null)
            return bestFitConverter;
        bestFitConverter = null;
        int minDepth = int.MaxValue;
        int minDepthGenericArgCount = 0;
        foreach (Type converterType in _builtinConverters.Concat(RegisteredReadConverters))
        {
            try
            {
                if (!TryMapTo(type, converterType, out int depth, out Type? dConverterType, out Dictionary<Type, Type>? map, true))
                    continue;
                if (depth > minDepth)
                    continue;
                int gLen = dConverterType.GetGenericArguments().Length;
                if (depth == minDepth)
                {
                    if (gLen <= minDepthGenericArgCount)
                        continue;
                }
                Type xconverter = converterType;
                Type[] genericParams = converterType.GetGenericArguments();
                if (genericParams.Length > 0)
                {
                    for (int i = 0; i < genericParams.Length; i++)
                    {
                        if (map.TryGetValue(genericParams[i], out Type? t))
                            genericParams[i] = t;
                    }
                    xconverter = converterType.MakeGenericType(genericParams);
                }
                INbtConverter? converter = GetConverterByType(xconverter);
                if (converter is null || !converter.CanRead)
                {
                    (converter as IDisposable)?.Dispose();
                    continue;
                }
                minDepth = depth;
                minDepthGenericArgCount = gLen;
                (bestFitConverter as IDisposable)?.Dispose();
                bestFitConverter = converter;
                if (dConverterType == type)
                    break;
            }
            catch
            {
            }
        }
        if (bestFitConverter is not null)
            return _readConverterCache[type] = bestFitConverter;
        Type reflectionConverter = typeof(ReflectionNbtConverter<>).MakeGenericType(type);
        MethodInfo? getInstance = reflectionConverter.GetMethod(nameof(ReflectionNbtConverter<object>.GetInstance), [typeof(ReflectionNbtConverterParameters)]);
        bestFitConverter = getInstance?.Invoke(null, [ReflectionNbtConverterParameters]) as INbtConverter;
        if (bestFitConverter is null)
            throw new Exception();
        _reflectionConverterCache[type] = bestFitConverter;
        return bestFitConverter;
    }

    public override INbtConverter GetDefaultWriteConverter(Type type)
    {
        INbtConverter? bestFitConverter = ResolveTypeConverter(type, false);
        if (bestFitConverter is not null)
            return bestFitConverter;
        bestFitConverter = null;
        int minDepth = int.MaxValue;
        foreach (Type converterType in _builtinConverters.Concat(RegisteredWriteConverters))
        {
            try
            {
                if (!TryMapTo(type, converterType, out int depth, out Type? dConverterType, out Dictionary<Type, Type>? map, false))
                    continue;
                if (depth >= minDepth)
                    continue;
                Type xconverter = converterType;
                if (converterType.IsGenericType)
                {
                    Type[] genericParams = converterType.GetGenericArguments();
                    for (int i = 0; i < genericParams.Length; i++)
                    {
                        if (map.TryGetValue(genericParams[i], out Type? t))
                            genericParams[i] = t;
                    }
                    xconverter = converterType.MakeGenericType(genericParams);
                }
                INbtConverter? converter = GetConverterByType(xconverter);
                if (converter is null || !converter.CanRead)
                {
                    (converter as IDisposable)?.Dispose();
                    continue;
                }
                minDepth = depth;
                (bestFitConverter as IDisposable)?.Dispose();
                bestFitConverter = converter;
                if (dConverterType == type)
                    break;
            }
            catch
            {
            }
        }
        if (bestFitConverter is not null)
            return _writeConverterCache[type] = bestFitConverter;
        Type reflectionConverter = typeof(ReflectionNbtConverter<>).MakeGenericType(type);
        MethodInfo? getInstance = reflectionConverter.GetMethod(nameof(ReflectionNbtConverter<object>.GetInstance), [typeof(ReflectionNbtConverterParameters)]);
        bestFitConverter = getInstance?.Invoke(null, [new ReflectionNbtConverterParameters()]) as INbtConverter;
        if (bestFitConverter is null)
            throw new Exception();
        _reflectionConverterCache[type] = bestFitConverter;
        return bestFitConverter;
    }
}
