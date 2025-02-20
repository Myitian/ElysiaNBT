using System.Runtime.InteropServices;

namespace ElysiaNBT;

[StructLayout(LayoutKind.Explicit)]
public struct Union
{
    [FieldOffset(0)] public sbyte I8;
    [FieldOffset(0)] public byte U8;
    [FieldOffset(0)] public short I16;
    [FieldOffset(0)] public ushort U16;
    [FieldOffset(0)] public int I32;
    [FieldOffset(0)] public uint U32;
    [FieldOffset(0)] public long I64;
    [FieldOffset(0)] public ulong U64;
    [FieldOffset(0)] public float FP32;
    [FieldOffset(0)] public double FP64;
}
