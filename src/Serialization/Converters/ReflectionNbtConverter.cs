using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ElysiaNBT.Serialization.Converters;

[RequiresUnreferencedCode(".ctor")]
public class ReflectionNbtConverter<
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.NonPublicProperties |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.NonPublicFields |
        DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : NbtConverter<T>
{
    protected static readonly Lock _variantCacheLock = new();
    protected static readonly Dictionary<ReflectionNbtConverterParameters, ReflectionNbtConverter<T>> _variantCache = [];
    protected readonly FrozenDictionary<string, ReadNbtMemberInfo> _membersReadNbt;
    protected readonly ReadOnlyCollection<WriteNbtMemberInfo> _membersWriteNbt;

    public DefaultIncluded DefaultIncludeProperties { get; }
    public DefaultIncluded DefaultIncludeFields { get; }
    public NbtIgnoreCondition DefaultIgnore { get; }

    protected ReflectionNbtConverter(ReflectionNbtConverterParameters parameters)
        : this(parameters.DefaultIncludeProperties, parameters.DefaultIncludeFields, parameters.DefaultIgnore)
    { }

    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    protected ReflectionNbtConverter(
        DefaultIncluded defaultIncludeProperties = DefaultIncluded.Public,
        DefaultIncluded defaultIncludeFields = DefaultIncluded.None,
        NbtIgnoreCondition defaultIgnore = NbtIgnoreCondition.WhenWritingNull)
    {
        DefaultIncludeProperties = defaultIncludeProperties;
        DefaultIncludeFields = defaultIncludeFields;
        DefaultIgnore = defaultIgnore;
        Type type = typeof(T);
        List<NbtMemberInfo> membersReadNbt = [];
        List<NbtMemberInfo> membersWriteNbt = [];

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            if (property.PropertyType.IsByRef || property.PropertyType.IsByRefLike)
                continue;
            NbtIgnoreAttribute? ignoreAttr = property.GetCustomAttribute<NbtIgnoreAttribute>();
            NbtEntryNameAttribute? nameAttr = property.GetCustomAttribute<NbtEntryNameAttribute>();
            NbtEntryOrderAttribute? orderAttr = property.GetCustomAttribute<NbtEntryOrderAttribute>();
            NbtConverterAttribute? converterAttr = property.GetCustomAttribute<NbtConverterAttribute>();
            if (ignoreAttr?.Condition is NbtIgnoreCondition.Always)
                continue;
            NbtIgnoreCondition? typeCond = ReflectionNbtSerializerContext.GetDefaultIgnoreStatus(property.PropertyType);
            NbtIgnoreCondition cond = ignoreAttr?.Condition ?? typeCond ?? DefaultIgnore;
            if (cond is NbtIgnoreCondition.Always)
                continue;
            if (DefaultIncludeProperties is DefaultIncluded.None
                && ignoreAttr is null
                && nameAttr is null
                && orderAttr is null
                && converterAttr is null)
                continue;
            NbtMemberInfo info = new(nameAttr?.Name ?? property.Name)
            {
                Property = property,
                IgnoreCondition = cond,
                Converter = converterAttr?.Converter,
                IsDynamic = property.GetCustomAttribute<DynamicAttribute>() is not null
            };
            if (orderAttr is not null)
                info.Order = orderAttr.Order;
            foreach (MethodInfo accessor in property.GetAccessors(true))
            {
                DefaultIncluded target = accessor.IsPublic ? DefaultIncluded.Public : DefaultIncluded.NonPublic;
                if ((target & DefaultIncludeProperties) is DefaultIncluded.None)
                    continue;
                if (accessor.ReturnType == SharedObjects.TypeVoid)
                    membersReadNbt.Add(info);
                else
                    membersWriteNbt.Add(info);
            }
        }
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            if (field.FieldType.IsByRef || field.FieldType.IsByRefLike)
                continue;
            NbtIgnoreAttribute? ignoreAttr = field.GetCustomAttribute<NbtIgnoreAttribute>();
            NbtEntryNameAttribute? nameAttr = field.GetCustomAttribute<NbtEntryNameAttribute>();
            NbtEntryOrderAttribute? orderAttr = field.GetCustomAttribute<NbtEntryOrderAttribute>();
            NbtConverterAttribute? converterAttr = field.GetCustomAttribute<NbtConverterAttribute>();
            if (ignoreAttr?.Condition is NbtIgnoreCondition.Always)
                continue;
            NbtIgnoreCondition? typeCond = ReflectionNbtSerializerContext.GetDefaultIgnoreStatus(field.FieldType);
            NbtIgnoreCondition cond = ignoreAttr?.Condition ?? typeCond ?? DefaultIgnore;
            if (cond is NbtIgnoreCondition.Always)
                continue;
            if (ignoreAttr is null && nameAttr is null && orderAttr is null && converterAttr is null)
            {
                DefaultIncluded target = field.IsPublic ? DefaultIncluded.Public : DefaultIncluded.NonPublic;
                if ((target & DefaultIncludeFields) is DefaultIncluded.None)
                    continue;
            }
            NbtMemberInfo info = new(nameAttr?.Name ?? field.Name)
            {
                Field = field,
                IgnoreCondition = cond,
                Converter = converterAttr?.Converter,
                IsDynamic = field.GetCustomAttribute<DynamicAttribute>() is not null
            };
            if (orderAttr is not null)
                info.Order = orderAttr.Order;
            if (!field.IsInitOnly)
                membersReadNbt.Add(info);
            membersWriteNbt.Add(info);
        }
        membersReadNbt.Sort();
        Dictionary<string, ReadNbtMemberInfo> rDict = [];
        foreach (NbtMemberInfo member in membersReadNbt)
            rDict.TryAdd(member.Name, new(member));
        _membersReadNbt = rDict.ToFrozenDictionary();
        membersWriteNbt.Sort();
        HashSet<string> wNames = membersWriteNbt.Select(static m => m.Name).ToHashSet();
        WriteNbtMemberInfo[] wArray = new WriteNbtMemberInfo[wNames.Count];
        int i = 0;
        foreach (NbtMemberInfo member in membersWriteNbt)
        {
            if (!wNames.Remove(member.Name))
                continue;
            wArray[i++] = new(member);
        }
        _membersWriteNbt = wArray.AsReadOnly();
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Compound;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Compound;
    }

    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public override T ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        object result = Activator.CreateInstance<T>()!;
        while (reader.Read() is not TokenType.EndCompound)
        {
            if (reader.TokenType is not TokenType.Name)
                throw new Exception();
            string key = reader.GetString();
            if (!_membersReadNbt.TryGetValue(key, out ReadNbtMemberInfo info))
            {
                context.SkipNbtConverterInstance.ReadNbt(reader, context);
                continue;
            }
            INbtConverter converter = ReflectionNbtSerializerContext.GetConverterByType(info.Converter)
                ?? (info.IsDynamic && context.DynamicNbtConverterInstance?.CanRead is true ? context.DynamicNbtConverterInstance : null)
                ?? context.GetDefaultReadConverter(info.Type);
            info.Setter(result, converter.BaseReadNbt(reader, info.Type, context));
        }
        return (T)result;
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Compound;
    }

    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public override void WriteNbt(INbtWriter writer, T value, NbtSerializerContext context)
    {
        writer.WriteStartCompound();
        if (value is not null)
        {
            foreach (WriteNbtMemberInfo info in _membersWriteNbt)
            {
                if (info.IgnoreCondition is NbtIgnoreCondition.Always)
                    continue;
                INbtConverter converter = ReflectionNbtSerializerContext.GetConverterByType(info.Converter)
                    ?? (info.IsDynamic && context.DynamicNbtConverterInstance?.CanWrite is true ? context.DynamicNbtConverterInstance : null)
                    ?? context.GetDefaultWriteConverter(info.Type);
                object? memberValue = info.Getter(value);
                NbtTagType tagType = converter.BaseGetTargetTagType(memberValue, context);
                writer.WriteName(info.Name, tagType);
                if (info.IgnoreCondition is NbtIgnoreCondition.WhenWritingNull && memberValue is null)
                    continue;
                if (info.IgnoreCondition is NbtIgnoreCondition.WhenWritingDefault
                    && info.Type.IsValueType ? Equals(memberValue, Activator.CreateInstance(info.Type)) : memberValue is null)
                    continue;
                converter.BaseWriteNbt(writer, memberValue!, context);
            }
        }
        writer.WriteEndCompound();
    }
    public override void BaseWriteNbt(INbtWriter writer, object? value, NbtSerializerContext context)
    {
        if (value is null)
            context.WriteNull(writer);
        else if (value is T t)
            WriteNbt(writer, t, context);
        else
            throw new Exception();
    }

    public static ReflectionNbtConverter<T> GetInstance(ReflectionNbtConverterParameters parameters)
    {
        lock (_variantCacheLock)
        {
            if (_variantCache.TryGetValue(parameters, out ReflectionNbtConverter<T>? converter))
                return converter;
            return _variantCache[parameters] = new(parameters);
        }
    }
    public static ReflectionNbtConverter<T> GetInstance(
        DefaultIncluded defaultIncludeProperties = DefaultIncluded.Public,
        DefaultIncluded defaultIncludeFields = DefaultIncluded.None,
        NbtIgnoreCondition defaultIgnore = NbtIgnoreCondition.WhenWritingNull)
    {
        return GetInstance(new(defaultIncludeProperties, defaultIncludeFields, defaultIgnore));
    }

    protected struct NbtMemberInfo(string name) : IComparable<NbtMemberInfo>
    {
        public string Name = name;
        public PropertyInfo? Property;
        public FieldInfo? Field;
        public int Order = int.MaxValue;
        public NbtIgnoreCondition IgnoreCondition = NbtIgnoreCondition.WhenWritingNull;
        public Type? Converter = null;
        public bool IsDynamic = false;

        public readonly int CompareTo(NbtMemberInfo other)
            => Order.CompareTo(other.Order);
    }
    protected readonly struct ReadNbtMemberInfo
    {
        public readonly Type Type;
        public readonly Action<object?, object?> Setter;
        public readonly Type? Converter;
        public readonly bool IsDynamic;

        public ReadNbtMemberInfo(NbtMemberInfo info)
        {
            if (info.Property is not null)
            {
                Type = info.Property.PropertyType;
                Setter = info.Property.SetValue;
            }
            else if (info.Field is not null)
            {
                Type = info.Field.FieldType;
                Setter = info.Field.SetValue;
            }
            else
                throw new Exception();
            Converter = info.Converter;
            IsDynamic = info.IsDynamic;
        }
    }
    protected readonly struct WriteNbtMemberInfo
    {
        public readonly string Name;
        public readonly NbtIgnoreCondition IgnoreCondition;
        public readonly Type Type;
        public readonly Func<object?, object?> Getter;
        public readonly Type? Converter;
        public readonly bool IsDynamic;


        [RequiresUnreferencedCode(".ctor")]
        public WriteNbtMemberInfo(NbtMemberInfo info)
        {
            Name = info.Name;
            IgnoreCondition = info.IgnoreCondition;
            if (info.Property is not null)
            {
                Type = info.Property.PropertyType;
                Getter = info.Property.GetValue;
            }
            else if (info.Field is not null)
            {
                Type = info.Field.FieldType;
                Getter = info.Field.GetValue;
            }
            else
                throw new Exception();
            Converter = info.Converter;
            IsDynamic = info.IsDynamic;
        }
    }
}
public struct ReflectionNbtConverterParameters(
    DefaultIncluded defaultIncludeProperties = DefaultIncluded.Public,
    DefaultIncluded defaultIncludeFields = DefaultIncluded.None,
    NbtIgnoreCondition defaultIgnore = NbtIgnoreCondition.WhenWritingNull) : IEquatable<ReflectionNbtConverterParameters>
{
    public DefaultIncluded DefaultIncludeProperties = defaultIncludeProperties;
    public DefaultIncluded DefaultIncludeFields = defaultIncludeFields;
    public NbtIgnoreCondition DefaultIgnore = defaultIgnore;

    public ReflectionNbtConverterParameters()
        : this(DefaultIncluded.Public) { }

    public readonly bool Equals(ReflectionNbtConverterParameters other)
        => DefaultIncludeProperties == other.DefaultIncludeProperties
        && DefaultIncludeFields == other.DefaultIncludeFields
        && DefaultIgnore == other.DefaultIgnore;
    public override readonly bool Equals(object? obj)
        => obj is ReflectionNbtConverterParameters
        && Equals(obj);
    public override readonly int GetHashCode()
        => (int)DefaultIncludeProperties << 24
        | (int)DefaultIncludeFields << 16
        | (int)DefaultIgnore << 8;

    public static bool operator ==(ReflectionNbtConverterParameters left, ReflectionNbtConverterParameters right)
        => left.Equals(right);
    public static bool operator !=(ReflectionNbtConverterParameters left, ReflectionNbtConverterParameters right)
        => !left.Equals(right);
}