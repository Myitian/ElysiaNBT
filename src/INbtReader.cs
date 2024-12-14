namespace ElysiaNBT;

public interface INbtReader : IDisposable
{
    NbtOptions Options { get; }
    TokenType TokenType { get; }
    State CurrentState { get; }
    int CurrentDepth { get; }
    int CurrentContentLength { get; }
    int Position { get; }
    ref Union StoredPayloadValue { get; }

    TokenType Read();

    public bool GetBool(bool strict = false)
        => BaseGetBool(strict);
    public byte GetByte(bool strict = false)
        => BaseGetByte(strict);
    public sbyte GetSByte(bool strict = false)
        => BaseGetSByte(strict);
    public short GetShort(bool strict = false)
        => BaseGetShort(strict);
    public ushort GetUShort(bool strict = false)
        => BaseGetUShort(strict);
    public int GetInt(bool strict = false)
        => BaseGetInt(strict);
    public uint GetUInt(bool strict = false)
        => BaseGetUInt(strict);
    public long GetLong(bool strict = false)
        => BaseGetLong(strict);
    public ulong GetULong(bool strict = false)
        => BaseGetULong(strict);
    public float GetFloat(bool strict = false)
        => BaseGetFloat(strict);
    public double GetDouble(bool strict = false)
        => BaseGetDouble(strict);

    bool BaseGetBool(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Byte or TokenType.True or TokenType.False ?
                StoredPayloadValue.U8 != 0 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.U8 != 0,
            TokenType.Short
                => (byte)StoredPayloadValue.U16 != 0,
            TokenType.Int
                => (byte)StoredPayloadValue.U32 != 0,
            TokenType.Long
                => (byte)StoredPayloadValue.U64 != 0,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    byte BaseGetByte(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Byte or TokenType.True or TokenType.False ?
                StoredPayloadValue.U8 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.U8,
            TokenType.Short
                => (byte)StoredPayloadValue.U16,
            TokenType.Int
                => (byte)StoredPayloadValue.U32,
            TokenType.Long
                => (byte)StoredPayloadValue.U64,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    sbyte BaseGetSByte(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Byte or TokenType.True or TokenType.False ?
                StoredPayloadValue.I8 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.I8,
            TokenType.Short
                => (sbyte)StoredPayloadValue.I16,
            TokenType.Int
                => (sbyte)StoredPayloadValue.I32,
            TokenType.Long
                => (sbyte)StoredPayloadValue.I64,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    short BaseGetShort(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Short ?
                StoredPayloadValue.I16 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.I8,
            TokenType.Short
                => StoredPayloadValue.I16,
            TokenType.Int
                => (short)StoredPayloadValue.I32,
            TokenType.Long
                => (short)StoredPayloadValue.I64,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    ushort BaseGetUShort(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Short ?
                StoredPayloadValue.U16 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.U8,
            TokenType.Short
                => StoredPayloadValue.U16,
            TokenType.Int
                => (ushort)StoredPayloadValue.U32,
            TokenType.Long
                => (ushort)StoredPayloadValue.U64,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    int BaseGetInt(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Int ?
                StoredPayloadValue.I32 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.I8,
            TokenType.Short
                => StoredPayloadValue.I16,
            TokenType.Int
                => StoredPayloadValue.I32,
            TokenType.Long
                => (int)StoredPayloadValue.I64,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    uint BaseGetUInt(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Int ?
                StoredPayloadValue.U32 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.U8,
            TokenType.Short
                => StoredPayloadValue.U16,
            TokenType.Int
                => StoredPayloadValue.U32,
            TokenType.Long
                => (uint)StoredPayloadValue.U64,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    long BaseGetLong(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Long ?
                StoredPayloadValue.I64 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.I8,
            TokenType.Short
                => StoredPayloadValue.I16,
            TokenType.Int
                => StoredPayloadValue.I32,
            TokenType.Long
                => StoredPayloadValue.I64,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    ulong BaseGetULong(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Long ?
                StoredPayloadValue.U64 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.U8,
            TokenType.Short
                => StoredPayloadValue.U16,
            TokenType.Int
                => StoredPayloadValue.U32,
            TokenType.Long
                => StoredPayloadValue.U64,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    float BaseGetFloat(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Float ?
                StoredPayloadValue.FP32 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.U8,
            TokenType.Short
                => StoredPayloadValue.U16,
            TokenType.Int
                => StoredPayloadValue.U32,
            TokenType.Long
                => StoredPayloadValue.U64,
            TokenType.Float
                => StoredPayloadValue.FP32,
            TokenType.Double
                => (float)StoredPayloadValue.FP64,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    double BaseGetDouble(bool strict = false)
    {
        if (strict)
        {
            return TokenType is TokenType.Double ?
                StoredPayloadValue.FP64 :
                throw new NbtException("Invalid type provided!");
        }
        return TokenType switch
        {
            TokenType.Byte or TokenType.True or TokenType.False
                => StoredPayloadValue.U8,
            TokenType.Short
                => StoredPayloadValue.U16,
            TokenType.Int
                => StoredPayloadValue.U32,
            TokenType.Long
                => StoredPayloadValue.U64,
            TokenType.Float
                => StoredPayloadValue.FP32,
            TokenType.Double
                => StoredPayloadValue.FP64,
            _ => throw new NbtException("Invalid type provided!")
        };
    }
    string GetString();
    char[] GetCharArray();
    ReadOnlySpan<char> GetCharSpan();
    Span<char> GetSafeCharSpan();

    void Skip();

    public enum State
    {
        ReadingName,
        ReadingPayload,
        ReadingBinaryRoot,
        Stopped
    }
}
