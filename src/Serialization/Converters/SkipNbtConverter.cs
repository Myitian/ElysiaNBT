﻿using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class SkipNbtConverter : NbtConverter<object?>
{
    public static readonly SkipNbtConverter Instance = new();

    public override bool CanRead => true;
    public override bool CanWrite => false;
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.AllNormalTypes;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.AllNormalTypes;
    public override object? ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        switch (reader.TokenType)
        {
            case TokenType.StartCompound:
                while (reader.Read() is not TokenType.EndCompound)
                {
                    if (reader.TokenType is not TokenType.Name)
                        throw new Exception();
                    reader.Skip();
                    ReadNbt(reader, context);
                }
                break;
            case TokenType.StartList:
            case TokenType.StartByteArray:
            case TokenType.StartIntArray:
            case TokenType.StartLongArray:
                while (reader.Read() is not TokenType.EndArray)
                {
                    if (reader.TokenType is TokenType.None)
                        throw new Exception();
                    ReadNbtBody(reader, context);
                }
                break;
            default:
                reader.Skip();
                break;
        }
        return null;
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        throw new NotSupportedException();
    }
    public override void WriteNbt(INbtWriter writer, object? value, NbtSerializerContext context)
    {
        throw new NotSupportedException();
    }
}