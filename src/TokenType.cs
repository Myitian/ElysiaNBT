namespace ElysiaNBT;

public enum TokenType
{
    None,
    /// <summary><c>{</c></summary>
    StartCompound,
    /// <summary><c>}</c></summary>
    EndCompound,
    /// <summary><c>[</c></summary>
    StartList,
    /// <summary><c>[B;</c></summary>
    StartByteArray,
    /// <summary><c>[I;</c></summary>
    StartIntArray,
    /// <summary><c>[L;</c></summary>
    StartLongArray,
    /// <summary><c>]</c></summary>
    EndArray,
    Name,
    String,
    Byte,
    Short,
    Int,
    Long,
    Float,
    Double,
    True,
    False,
    End,
    UnknownPayload
}
