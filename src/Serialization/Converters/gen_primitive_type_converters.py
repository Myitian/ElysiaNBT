def gen_primitive_type_converters():
    template = """using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class {0}NbtConverter : NbtConverter<{1}>, IInstance<{0}NbtConverter>
{{
    public static {0}NbtConverter Instance {{ get; }} = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.{2};
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return {3};
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override {1} ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {{
        return reader.Get{0}();
    }}
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {{
        return NbtTagType.{2};
    }}
    public override void WriteNbt(INbtWriter writer, {1} value, NbtSerializerContext context)
    {{
        writer.WritePayload(value);
    }}
}}"""
    for t in {
        "Byte",
        "SByte",
        "Short",
        "UShort",
        "Int",
        "UInt",
        "Long",
        "ULong",
        "Float",
        "Double",
    }:
        if t[0] == "U" or t == "SByte":
            tn = t[1:]
        else:
            tn = t
        if t[0] in "FD":
            at = "SharedObjects.FloatAcceptedTypes"
        else:
            at = "SharedObjects.IntAcceptedTypes"

        with open(f"{t}NbtConverter.cs", "wt") as f:
            f.write(template.format(t, t.lower(), tn, at))


def gen_array_type_converters():
    template = """using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace ElysiaNBT.Serialization.Converters;

public sealed class {0}ArrayNbtConverter :
    NbtConverter<ICollection<{1}>>,
    IReadOnlyNbtConverter<{1}[]>,
    IInstance<{0}ArrayNbtConverter>
{{
    public static {0}ArrayNbtConverter Instance {{ get; }} = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.{2}Array;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.List{2}Array;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override {1}[] ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {{
        int estimatedLength = reader.CurrentContentLength;
        if (estimatedLength >= 0)
        {{
            {1}[] result = GC.AllocateUninitializedArray<{1}>(estimatedLength);
            int i = 0;
            while (reader.Read() is not TokenType.EndArray)
            {{
                if (reader.TokenType is TokenType.None || i >= result.Length)
                    throw new Exception();
                result[i++] = reader.Get{0}();
            }}
            return result;
        }}
        else
        {{
            List<{1}> result = [];
            while (reader.Read() is not TokenType.EndArray)
            {{
                if (reader.TokenType is TokenType.None)
                    throw new Exception();
                result.Add(reader.Get{0}());
            }}
            return [.. result];
        }}
    }}
    public override {1}[] ReadNbt(INbtReader reader, NbtSerializerContext context)
    {{
        return ({1}[])base.ReadNbt(reader, context);
    }}
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {{
        return NbtTagType.{2}Array;
    }}
    public override void WriteNbt(INbtWriter writer, ICollection<{1}> value, NbtSerializerContext context)
    {{
        switch (value)
        {{
            case {1}[] array:
                writer.WritePayload(array);
                break;
            case ArraySegment<{1}> arraySeg:
                writer.WritePayload(arraySeg);
                break;
            case List<{1}> list:
                writer.WritePayload(CollectionsMarshal.AsSpan(list));
                break;
            default:
                writer.WriteStart{2}Array(value.Count);
                foreach ({1} b in value)
                    writer.WritePayload(b);
                writer.WriteEndArray();
                break;
        }}
    }}
}}"""
    for t in {
        ("Byte", "Byte"),
        ("SByte", "Byte"),
        ("Int", "Int"),
        ("UInt", "Int"),
        ("Long", "Long"),
        ("ULong", "Long"),
    }:
        with open(f"{t[0]}ArrayNbtConverter.cs", "wt") as f:
            f.write(template.format(t[0], t[0].lower(), t[1]))


gen_primitive_type_converters()
gen_array_type_converters()