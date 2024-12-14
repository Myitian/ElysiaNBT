using System.Reflection;

namespace ElysiaNBT.Serialization;

public interface IReadOnlyNbtConverter<out T> where T : allows ref struct
{
    bool CanRead { get; }
    IReadOnlySet<NbtTagType> AcceptedTagTypes { get; }
    T ReadNbtBody(INbtReader reader, NbtSerializerContext context);
    T ReadNbt(INbtReader reader, NbtSerializerContext context);
    object? BaseReadNbtBody(INbtReader reader, Type type, NbtSerializerContext context);
    object? BaseReadNbt(INbtReader reader, Type type, NbtSerializerContext context);
}