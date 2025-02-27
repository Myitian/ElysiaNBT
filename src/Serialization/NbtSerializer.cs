using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;

namespace ElysiaNBT.Serialization;

public static class NbtSerializer
{
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? Deserialize(INbtReader reader, Type type)
    {
        return Deserialize(reader, type, ReflectionNbtSerializerContext.Default);
    }
    public static object? Deserialize(INbtReader reader, Type type, NbtSerializerContext context)
    {
        INbtConverter converter = context.GetDefaultReadConverter(type);
        if (reader.Options.HasRootName)
        {
            reader.Read();
            reader.Skip();
        }
        return converter.BaseReadNbt(reader, type, context);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T Deserialize<T>(INbtReader reader)
    {
        return Deserialize<T>(reader, ReflectionNbtSerializerContext.Default);
    }
    public static T Deserialize<T>(INbtReader reader, NbtSerializerContext context)
    {
        IReadOnlyNbtConverter<T> converter = context.GetDefaultReadConverter<T>();
        if (reader.Options.HasRootName)
        {
            reader.Read();
            reader.Skip();
        }
        return converter.ReadNbt(reader, context);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeBinary(byte[] bytes, Type type, BinaryNbtOptions? options = null)
    {
        return DeserializeBinary(bytes, type, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeBinary(byte[] bytes, Type type, NbtOptions? options)
    {
        return DeserializeBinary(bytes, type, ReflectionNbtSerializerContext.Default, options);
    }
    public static object? DeserializeBinary(byte[] bytes, Type type, NbtSerializerContext context, BinaryNbtOptions? options = null)
    {
        using BinaryNbtReader bnw = new(bytes, options ?? BinaryNbtOptions.JavaEdition);
        return Deserialize(bnw, type, context);
    }
    public static object? DeserializeBinary(byte[] bytes, Type type, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeBinary(bytes, type, context, options is null ? null : new BinaryNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeBinary(Stream stream, Type type, BinaryNbtOptions? options = null)
    {
        return DeserializeBinary(stream, type, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeBinary(Stream stream, Type type, NbtOptions? options)
    {
        return DeserializeBinary(stream, type, ReflectionNbtSerializerContext.Default, options);
    }
    public static object? DeserializeBinary(Stream stream, Type type, NbtSerializerContext context, BinaryNbtOptions? options = null)
    {
        using BinaryNbtReader bnw = new(stream, options ?? BinaryNbtOptions.JavaEdition);
        return Deserialize(bnw, type, context);
    }
    public static object? DeserializeBinary(Stream stream, Type type, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeBinary(stream, type, context, options is null ? null : new BinaryNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeBinary<T>(byte[] bytes, BinaryNbtOptions? options = null)
    {
        return DeserializeBinary<T>(bytes, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeBinary<T>(byte[] bytes, NbtOptions? options)
    {
        return DeserializeBinary<T>(bytes, ReflectionNbtSerializerContext.Default, options);
    }
    public static T DeserializeBinary<T>(byte[] bytes, NbtSerializerContext context, BinaryNbtOptions? options = null)
    {
        if (bytes.Length > 2)
            switch (bytes[0])
            {
                case 0x78:
                    using (ZLibStream zs = new(new MemoryStream(bytes), CompressionMode.Decompress, false))
                        return DeserializeRawBinary<T>(zs, context, options);
                case 0x1F when bytes[1] == 0x8B:
                    using (GZipStream gs = new(new MemoryStream(bytes), CompressionMode.Decompress, false))
                        return DeserializeRawBinary<T>(gs, context, options);
            }
        return DeserializeBinary<T>(bytes, context, options);
    }
    public static T DeserializeBinary<T>(byte[] bytes, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeBinary<T>(bytes, context, options is null ? null : new BinaryNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeBinary<T>(Stream stream, BinaryNbtOptions? options = null, bool leaveOpen = false)
    {
        return DeserializeBinary<T>(stream, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeBinary<T>(Stream stream, NbtOptions? options, bool leaveOpen = false)
    {
        return DeserializeBinary<T>(stream, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    public static T DeserializeBinary<T>(Stream stream, NbtSerializerContext context, BinaryNbtOptions? options = null, bool leaveOpen = false)
    {
        using Preview2Stream p2s = new(stream, leaveOpen);
        switch (p2s.Header0)
        {
            case 0x78:
                using (ZLibStream zs = new(p2s, CompressionMode.Decompress, false))
                    return DeserializeRawBinary<T>(zs, context, options);
            case 0x1F when p2s.Header1 == 0x8B:
                using (GZipStream gs = new(p2s, CompressionMode.Decompress, false))
                    return DeserializeRawBinary<T>(gs, context, options);
        }
        return DeserializeRawBinary<T>(p2s, context, options);
    }
    public static T DeserializeBinary<T>(Stream stream, NbtSerializerContext context, NbtOptions? options, bool leaveOpen = false)
    {
        return DeserializeBinary<T>(stream, context, options is null ? null : new BinaryNbtOptions(options.Value), leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeRawBinary(byte[] bytes, Type type, BinaryNbtOptions? options = null)
    {
        return DeserializeRawBinary(bytes, type, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeRawBinary(byte[] bytes, Type type, NbtOptions? options)
    {
        return DeserializeRawBinary(bytes, type, ReflectionNbtSerializerContext.Default, options);
    }
    public static object? DeserializeRawBinary(byte[] bytes, Type type, NbtSerializerContext context, BinaryNbtOptions? options = null)
    {
        using BinaryNbtReader bnw = new(bytes, options ?? BinaryNbtOptions.JavaEdition);
        return Deserialize(bnw, type, context);
    }
    public static object? DeserializeRawBinary(byte[] bytes, Type type, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeRawBinary(bytes, type, context, options is null ? null : new BinaryNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeRawBinary(Stream stream, Type type, BinaryNbtOptions? options = null, bool leaveOpen = false)
    {
        return DeserializeRawBinary(stream, type, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeRawBinary(Stream stream, Type type, NbtOptions? options, bool leaveOpen = false)
    {
        return DeserializeRawBinary(stream, type, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    public static object? DeserializeRawBinary(Stream stream, Type type, NbtSerializerContext context, BinaryNbtOptions? options = null, bool leaveOpen = false)
    {
        using BinaryNbtReader bnw = new(stream, options ?? BinaryNbtOptions.JavaEdition, leaveOpen);
        return Deserialize(bnw, type, context);
    }
    public static object? DeserializeRawBinary(Stream stream, Type type, NbtSerializerContext context, NbtOptions? options, bool leaveOpen = false)
    {
        return DeserializeRawBinary(stream, type, context, options is null ? null : new BinaryNbtOptions(options.Value), leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeRawBinary<T>(byte[] bytes, BinaryNbtOptions? options = null)
    {
        return DeserializeRawBinary<T>(bytes, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeRawBinary<T>(byte[] bytes, NbtOptions? options)
    {
        return DeserializeRawBinary<T>(bytes, ReflectionNbtSerializerContext.Default, options);
    }
    public static T DeserializeRawBinary<T>(byte[] bytes, NbtSerializerContext context, BinaryNbtOptions? options = null)
    {
        using BinaryNbtReader bnw = new(bytes, options ?? BinaryNbtOptions.JavaEdition);
        return Deserialize<T>(bnw, context);
    }
    public static T DeserializeRawBinary<T>(byte[] bytes, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeRawBinary<T>(bytes, context, options is null ? null : new BinaryNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeRawBinary<T>(Stream stream, BinaryNbtOptions? options = null)
    {
        return DeserializeRawBinary<T>(stream, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeRawBinary<T>(Stream stream, NbtOptions? options)
    {
        return DeserializeRawBinary<T>(stream, ReflectionNbtSerializerContext.Default, options);
    }
    public static T DeserializeRawBinary<T>(Stream stream, NbtSerializerContext context, BinaryNbtOptions? options = null)
    {
        using BinaryNbtReader bnw = new(stream, options ?? BinaryNbtOptions.JavaEdition);
        return Deserialize<T>(bnw, context);
    }
    public static T DeserializeRawBinary<T>(Stream stream, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeRawBinary<T>(stream, context, options is null ? null : new BinaryNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeString(Stream stream, Type type, StringNbtOptions? options = null)
    {
        return DeserializeString(stream, type, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeString(Stream stream, Type type, NbtOptions? options)
    {
        return DeserializeString(stream, type, ReflectionNbtSerializerContext.Default, options);
    }
    public static object? DeserializeString(Stream stream, Type type, NbtSerializerContext context, StringNbtOptions? options = null)
    {
        using StringNbtReader snw = new(stream, options ?? StringNbtOptions.Default);
        return Deserialize(snw, type, context);
    }
    public static object? DeserializeString(Stream stream, Type type, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeString(stream, type, context, options is null ? null : new StringNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeString(string str, Type type, StringNbtOptions? options = null)
    {
        return DeserializeString(str, type, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeString(string str, Type type, NbtOptions? options)
    {
        return DeserializeString(str, type, ReflectionNbtSerializerContext.Default, options);
    }
    public static object? DeserializeString(string str, Type type, NbtSerializerContext context, StringNbtOptions? options = null)
    {
        using StringNbtReader snw = new(str, options ?? StringNbtOptions.Default);
        return Deserialize(snw, type, context);
    }
    public static object? DeserializeString(string str, Type type, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeString(str, type, context, options is null ? null : new StringNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeString(TextReader reader, Type type, StringNbtOptions? options = null)
    {
        return DeserializeString(reader, type, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static object? DeserializeString(TextReader reader, Type type, NbtOptions? options)
    {
        return DeserializeString(reader, type, ReflectionNbtSerializerContext.Default, options);
    }
    public static object? DeserializeString(TextReader reader, Type type, NbtSerializerContext context, StringNbtOptions? options = null)
    {
        using StringNbtReader snw = new(reader, options ?? StringNbtOptions.Default);
        return Deserialize(snw, type, context);
    }
    public static object? DeserializeString(TextReader reader, Type type, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeString(reader, type, context, options is null ? null : new StringNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeString<T>(Stream stream, StringNbtOptions? options = null)
    {
        return DeserializeString<T>(stream, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeString<T>(Stream stream, NbtOptions? options)
    {
        return DeserializeString<T>(stream, ReflectionNbtSerializerContext.Default, options);
    }
    public static T DeserializeString<T>(Stream stream, NbtSerializerContext context, StringNbtOptions? options = null)
    {
        using StringNbtReader snw = new(stream, options ?? StringNbtOptions.Default);
        return Deserialize<T>(snw, context);
    }
    public static T DeserializeString<T>(Stream stream, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeString<T>(stream, context, options is null ? null : new StringNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeString<T>(string str, StringNbtOptions? options = null)
    {
        return DeserializeString<T>(str, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeString<T>(string str, NbtOptions? options)
    {
        return DeserializeString<T>(str, ReflectionNbtSerializerContext.Default, options);
    }
    public static T DeserializeString<T>(string str, NbtSerializerContext context, StringNbtOptions? options = null)
    {
        using StringNbtReader snw = new(str, options ?? StringNbtOptions.Default);
        return Deserialize<T>(snw, context);
    }
    public static T DeserializeString<T>(string str, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeString<T>(str, context, options is null ? null : new StringNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeString<T>(TextReader reader, StringNbtOptions? options = null)
    {
        return DeserializeString<T>(reader, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static T DeserializeString<T>(TextReader reader, NbtOptions? options)
    {
        return DeserializeString<T>(reader, ReflectionNbtSerializerContext.Default, options);
    }
    public static T DeserializeString<T>(TextReader reader, NbtSerializerContext context, StringNbtOptions? options = null)
    {
        using StringNbtReader snw = new(reader, options ?? StringNbtOptions.Default);
        return Deserialize<T>(snw, context);
    }
    public static T DeserializeString<T>(TextReader reader, NbtSerializerContext context, NbtOptions? options)
    {
        return DeserializeString<T>(reader, context, options is null ? null : new StringNbtOptions(options.Value));
    }

    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void Serialize(INbtWriter writer, object? value, Type type)
    {
        Serialize(writer, value, type, ReflectionNbtSerializerContext.Default);
    }
    public static void Serialize(INbtWriter writer, object? value, Type type, NbtSerializerContext context)
    {
        INbtConverter converter = context.GetDefaultWriteConverter(type);
        if (writer.Options.HasRootName)
            writer.WriteName("");
        converter.BaseWriteNbt(writer, value, context);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void Serialize<T>(INbtWriter writer, T value)
    {
        Serialize(writer, value, ReflectionNbtSerializerContext.Default);
    }
    public static void Serialize<T>(INbtWriter writer, T value, NbtSerializerContext context)
    {
        IWriteOnlyNbtConverter<T> converter = context.GetDefaultWriteConverter<T>();
        if (writer.Options.HasRootName)
            writer.WriteName("");
        converter.WriteNbt(writer, value, context);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static byte[] SerializeBinary(object value, Type type, BinaryNbtOptions? options = null)
    {
        return SerializeBinary(value, type, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static byte[] SerializeBinary(object value, Type type, NbtOptions? options)
    {
        return SerializeBinary(value, type, ReflectionNbtSerializerContext.Default, options);
    }
    public static byte[] SerializeBinary(object value, Type type, NbtSerializerContext context, BinaryNbtOptions? options = null)
    {
        using MemoryStream ms = new();
        using BinaryNbtWriter bnw = new(ms, options ?? BinaryNbtOptions.JavaEdition);
        Serialize(bnw, value, type, context);
        return ms.ToArray();
    }
    public static byte[] SerializeBinary(object value, Type type, NbtSerializerContext context, NbtOptions? options)
    {
        return SerializeBinary(value, type, context, options is null ? null : new BinaryNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeBinary(object value, Type type, Stream stream, BinaryNbtOptions? options = null, bool leaveOpen = false)
    {
        SerializeBinary(value, type, stream, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeBinary(object value, Type type, Stream stream, NbtOptions? options, bool leaveOpen = false)
    {
        SerializeBinary(value, type, stream, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    public static void SerializeBinary(object value, Type type, Stream stream, NbtSerializerContext context, BinaryNbtOptions? options = null, bool leaveOpen = false)
    {
        using BinaryNbtWriter bnw = new(stream, options ?? BinaryNbtOptions.JavaEdition, leaveOpen);
        Serialize(bnw, value, type, context);
    }
    public static void SerializeBinary(object value, Type type, Stream stream, NbtSerializerContext context, NbtOptions? options, bool leaveOpen = false)
    {
        SerializeBinary(value, type, stream, context, options is null ? null : new BinaryNbtOptions(options.Value), leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static byte[] SerializeBinary<T>(T value, BinaryNbtOptions? options = null)
    {
        return SerializeBinary(value, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static byte[] SerializeBinary<T>(T value, NbtOptions? options)
    {
        return SerializeBinary(value, ReflectionNbtSerializerContext.Default, options);
    }
    public static byte[] SerializeBinary<T>(T value, NbtSerializerContext context, BinaryNbtOptions? options = null)
    {
        using MemoryStream ms = new();
        using BinaryNbtWriter bnw = new(ms, options ?? BinaryNbtOptions.JavaEdition);
        Serialize(bnw, value, context);
        return ms.ToArray();
    }
    public static byte[] SerializeBinary<T>(T value, NbtSerializerContext context, NbtOptions? options)
    {
        return SerializeBinary(value, context, options is null ? null : new BinaryNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeBinary<T>(T value, Stream stream, BinaryNbtOptions? options = null, bool leaveOpen = false)
    {
        SerializeBinary(value, stream, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeBinary<T>(T value, Stream stream, NbtOptions? options, bool leaveOpen = false)
    {
        SerializeBinary(value, stream, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    public static void SerializeBinary<T>(T value, Stream stream, NbtSerializerContext context, BinaryNbtOptions? options = null, bool leaveOpen = false)
    {
        using BinaryNbtWriter bnw = new(stream, options ?? BinaryNbtOptions.JavaEdition, leaveOpen);
        Serialize(bnw, value, context);
    }
    public static void SerializeBinary<T>(T value, Stream stream, NbtSerializerContext context, NbtOptions? options, bool leaveOpen = false)
    {
        SerializeBinary(value, stream, context, options is null ? null : new BinaryNbtOptions(options.Value), leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static string SerializeString(object value, Type type, StringNbtOptions? options = null)
    {
        return SerializeString(value, type, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static string SerializeString(object value, Type type, NbtOptions? options)
    {
        return SerializeString(value, type, ReflectionNbtSerializerContext.Default, options);
    }
    public static string SerializeString(object value, Type type, NbtSerializerContext context, StringNbtOptions? options = null)
    {
        using StringWriter sw = new();
        using StringNbtWriter snw = new(sw, options ?? StringNbtOptions.Minimal);
        Serialize(snw, value, type, context);
        return sw.ToString();
    }
    public static string SerializeString(object value, Type type, NbtSerializerContext context, NbtOptions? options)
    {
        return SerializeString(value, type, context, options is null ? null : new StringNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeString(object value, Type type, StringBuilder builder, StringNbtOptions? options = null)
    {
        SerializeString(value, type, builder, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeString(object value, Type type, StringBuilder builder, NbtOptions? options)
    {
        SerializeString(value, type, builder, ReflectionNbtSerializerContext.Default, options);
    }
    public static void SerializeString(object value, Type type, StringBuilder builder, NbtSerializerContext context, StringNbtOptions? options = null)
    {
        using StringNbtWriter snw = new(builder, options ?? StringNbtOptions.Minimal);
        Serialize(snw, value, type, context);
    }
    public static void SerializeString(object value, Type type, StringBuilder builder, NbtSerializerContext context, NbtOptions? options)
    {
        SerializeString(value, type, builder, context, options is null ? null : new StringNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeString(object value, Type type, TextWriter writer, StringNbtOptions? options = null, bool leaveOpen = false)
    {
        SerializeString(value, type, writer, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeString(object value, Type type, TextWriter writer, NbtOptions? options, bool leaveOpen = false)
    {
        SerializeString(value, type, writer, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    public static void SerializeString(object value, Type type, TextWriter writer, NbtSerializerContext context, StringNbtOptions? options = null, bool leaveOpen = false)
    {
        using StringNbtWriter snw = new(writer, options ?? StringNbtOptions.Minimal, leaveOpen);
        Serialize(snw, value, type, context);
    }
    public static void SerializeString(object value, Type type, TextWriter writer, NbtSerializerContext context, NbtOptions? options, bool leaveOpen = false)
    {
        SerializeString(value, type, writer, context, options is null ? null : new StringNbtOptions(options.Value), leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static string SerializeString<T>(T value, StringNbtOptions? options = null)
    {
        return SerializeString(value, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static string SerializeString<T>(T value, NbtOptions? options)
    {
        return SerializeString(value, ReflectionNbtSerializerContext.Default, options);
    }
    public static string SerializeString<T>(T value, NbtSerializerContext context, StringNbtOptions? options = null)
    {
        using StringWriter sw = new();
        using StringNbtWriter snw = new(sw, options ?? StringNbtOptions.Minimal);
        Serialize(snw, value, context);
        return sw.ToString();
    }
    public static string SerializeString<T>(T value, NbtSerializerContext context, NbtOptions? options)
    {
        return SerializeString(value, context, options is null ? null : new StringNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeString<T>(T value, StringBuilder builder, StringNbtOptions? options = null)
    {
        SerializeString(value, builder, ReflectionNbtSerializerContext.Default, options);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeString<T>(T value, StringBuilder builder, NbtOptions? options)
    {
        SerializeString(value, builder, ReflectionNbtSerializerContext.Default, options);
    }
    public static void SerializeString<T>(T value, StringBuilder builder, NbtSerializerContext context, StringNbtOptions? options = null)
    {
        using StringNbtWriter snw = new(builder, options ?? StringNbtOptions.Minimal);
        Serialize(snw, value, context);
    }
    public static void SerializeString<T>(T value, StringBuilder builder, NbtSerializerContext context, NbtOptions? options)
    {
        SerializeString(value, builder, context, options is null ? null : new StringNbtOptions(options.Value));
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeString<T>(T value, TextWriter writer, StringNbtOptions? options = null, bool leaveOpen = false)
    {
        SerializeString(value, writer, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    [RequiresDynamicCode("Type.MakeGenericType()")]
    [RequiresUnreferencedCode("Type.GetInterfaces()")]
    public static void SerializeString<T>(T value, TextWriter writer, NbtOptions? options, bool leaveOpen = false)
    {
        SerializeString(value, writer, ReflectionNbtSerializerContext.Default, options, leaveOpen);
    }
    public static void SerializeString<T>(T value, TextWriter writer, NbtSerializerContext context, StringNbtOptions? options = null, bool leaveOpen = false)
    {
        using StringNbtWriter snw = new(writer, options ?? StringNbtOptions.Minimal, leaveOpen);
        Serialize(snw, value, context);
    }
    public static void SerializeString<T>(T value, TextWriter writer, NbtSerializerContext context, NbtOptions? options, bool leaveOpen = false)
    {
        SerializeString(value, writer, context, options is null ? null : new StringNbtOptions(options.Value), leaveOpen);
    }

    public sealed class Preview2Stream : Stream
    {
        private byte _state = 0;
        private readonly bool _leaveOpen = false;
        public int Header0 { get; }
        public int Header1 { get; }
        public Stream BaseStream { get; }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => BaseStream.Length;
        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        public Preview2Stream(Stream baseStream, bool leaveOpen = false)
        {
            BaseStream = baseStream;
            _leaveOpen = leaveOpen;
            Header0 = BaseStream.ReadByte();
            Header1 = BaseStream.ReadByte();
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            return _state switch
            {
                0 or 1 => Read(buffer.AsSpan(offset, count)),
                _ => BaseStream.Read(buffer, offset, count),
            };
        }
        public override int Read(Span<byte> buffer)
        {
            switch (_state)
            {
                case 0:
                    if (Header0 < 0 || buffer.Length == 0)
                        return 0;
                    buffer[0] = (byte)Header0;
                    if (Header1 < 0 || buffer.Length == 1)
                    {
                        _state = 1;
                        return 1;
                    }
                    buffer[1] = (byte)Header1;
                    _state = 2;
                    return 2;
                case 1:
                    if (Header1 < 0 || buffer.Length == 0)
                        return 0;
                    buffer[0] = (byte)Header1;
                    _state = 2;
                    return 1;
                default:
                    return BaseStream.Read(buffer);
            }
        }
        public override int ReadByte()
        {
            switch (_state)
            {
                case 0:
                    _state = 1;
                    return Header0;
                case 1:
                    _state = 2;
                    return Header1;
                default:
                    return BaseStream.ReadByte();
            }
        }
        public override void Flush()
        {
            throw new NotSupportedException();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
        public override void Close()
        {
            if (!_leaveOpen)
                BaseStream.Close();
        }
    }
}