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
    private static readonly Dictionary<Type, ReadOnlyCollection<ReadOnlyCollection<Type>>> _inheritanceDepthInfoCache = [];
    private static readonly Dictionary<Type, INbtConverter> _converterCache = [];
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
            RegisteredReadConverters = [.. RegisteredReadConverters, .. read],
            RegisteredWriteConverters = [.. RegisteredWriteConverters, .. write],
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
        [MaybeNullWhen(false)] out Type target,
        [MaybeNullWhen(false)] out Type declaredConverterType,
        [MaybeNullWhen(false)] out Dictionary<Type, Type> mapping,
        bool isReadNbt = true)
    {
        Type? baseConverter = converter;
        while (baseConverter is not null)
        {
            if (baseConverter.IsGenericType)
            {
                Type gDef = baseConverter.GetGenericTypeDefinition();
                if (gDef == SharedObjects.TypeNbtConverter)
                    goto NEXT;
            }
            baseConverter = baseConverter.BaseType;
        }
        goto FAILED;
    NEXT:
        mapping = [];
        declaredConverterType = baseConverter.GenericTypeArguments[0];
        if (isReadNbt)
        {
            MethodInfo? method = converter.GetMethod(nameof(NbtConverter<object>.ReadNbt), SharedObjects.NbtConverterMethodReadNbtParams);
            if (method is null)
                goto FAILED;
            target = method.ReturnType;
            ReadOnlyCollection<ReadOnlyCollection<Type>> inheritInfo = GetInheritanceDepthInfo(target);
            for (int i = 0; i < inheritInfo.Count; i++)
            {
                foreach (Type t in inheritInfo[i])
                {
                    mapping.Clear();
                    if (Match(t, type, mapping))
                    {
                        depth = i;
                        return true;
                    }
                }
            }
        }
        else
        {
            target = declaredConverterType;
            ReadOnlyCollection<ReadOnlyCollection<Type>> inheritInfo = GetInheritanceDepthInfo(type);
            for (int i = 0; i < inheritInfo.Count; i++)
            {
                foreach (Type t in inheritInfo[i])
                {
                    mapping.Clear();
                    if (Match(target, t, mapping))
                    {
                        depth = i;
                        return true;
                    }
                }
            }
        }
    FAILED:
        depth = -1;
        target = null;
        declaredConverterType = null;
        mapping = null;
        return false;
    }
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static ReadOnlyCollection<ReadOnlyCollection<Type>> GetInheritanceDepthInfo(Type type)
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
        return result.AsReadOnly();

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
    public static bool Match(Type a, Type b, Dictionary<Type, Type> mappings)
    {
        if (a.IsGenericParameter)
            return mappings.TryAdd(a, b) || mappings[a] == b;
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
            return atA is not null && atB is not null && Match(atA, atB, mappings);
        }
        Type gA = a.GetGenericTypeDefinition(), gB = b.GetGenericTypeDefinition();
        if (gA != gB)
            return false;
        ReadOnlySpan<Type> gpA = a.GetGenericArguments(), gpB = b.GetGenericArguments();
        if (gpA.Length != gpB.Length)
            return false;
        for (int i = 0; i < gpA.Length; i++)
        {
            if (!Match(gpA[i], gpB[i], mappings))
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
        foreach (INbtConverter converter in _converterCache.Values)
            (converter as IDisposable)?.Dispose();
        _converterCache.Clear();
        _inheritanceDepthInfoCache.Clear();
    }

    public static INbtConverter? GetConverterByType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type? type)
    {
        if (type is null)
            return null;
        if (_converterCache.TryGetValue(type, out INbtConverter? converterC))
            return converterC;
        object? instance = Activator.CreateInstance(type);
        if (instance is INbtConverter converter)
            return _converterCache[type] = converter;
        (instance as IDisposable)?.Dispose();
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
        if(src == SharedObjects.TypeObject)
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
        Type? converterAcceptedType = null;
        Type? converterRealType = null;
        foreach (Type converterType in _builtinConverters.Concat(RegisteredReadConverters))
        {
            try
            {
                if (!TryMapTo(type, converterType, out int depth, out Type? target, out Type? dConverterType, out Dictionary<Type, Type>? map, true))
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
                converterAcceptedType = target;
                converterRealType = dConverterType;
                bestFitConverter = converter;
                if (converterRealType == type)
                    break;
            }
            catch
            {
            }
        }
        if (bestFitConverter is not null)
        {
            return _readConverterCache[type] = bestFitConverter;
        }
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
        Type? converterAcceptedType = null;
        Type? converterRealType = null;
        foreach (Type converterType in _builtinConverters.Concat(RegisteredWriteConverters))
        {
            try
            {
                if (!TryMapTo(type, converterType, out int depth, out Type? target, out Type? dConverterType, out Dictionary<Type, Type>? map, false))
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
                converterAcceptedType = target;
                converterRealType = dConverterType;
                bestFitConverter = converter;
                if (converterRealType == type)
                    break;
            }
            catch
            {
            }
        }
        if (bestFitConverter is not null && converterAcceptedType is not null)
        {
            _writeConverterCache[type] = bestFitConverter;
            return bestFitConverter;
        }
        Type reflectionConverter = typeof(ReflectionNbtConverter<>).MakeGenericType(type);
        MethodInfo? getInstance = reflectionConverter.GetMethod(nameof(ReflectionNbtConverter<object>.GetInstance), [typeof(ReflectionNbtConverterParameters)]);
        bestFitConverter = getInstance?.Invoke(null, [new ReflectionNbtConverterParameters()]) as INbtConverter;
        if (bestFitConverter is null)
            throw new Exception();
        _reflectionConverterCache[type] = bestFitConverter;
        return bestFitConverter;
    }

    public override IReadOnlyNbtConverter<T> MakeReadWrapper<T>(INbtConverter converter)
    {
        Type? baseConverter = converter.GetType();
        while (baseConverter is not null)
        {
            if (baseConverter.IsGenericType &&
                baseConverter.GetGenericTypeDefinition() == SharedObjects.TypeNbtConverter)
                goto NEXT;
            baseConverter = baseConverter.BaseType;
        }
        goto FAILED;
    NEXT:
        Type wrapperType = SharedObjects.TypeNbtConverterReadWrapper.MakeGenericType([typeof(T), baseConverter.GenericTypeArguments[0]]);
        IReadOnlyNbtConverter<T>? wrapper = GetWrapperByType(wrapperType, converter) as IReadOnlyNbtConverter<T>;
        if (wrapper is not null)
            return wrapper;
        FAILED:
        throw new Exception();
    }
}
