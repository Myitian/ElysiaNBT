using Myitian.Text;
using System.Text;

namespace ElysiaNBT;

public struct BinaryNbtOptions(NbtOptions options)
{
    public static readonly BinaryNbtOptions JavaEdition = new() { IsLittleEndian = false, UseVarInt = false };
    public static readonly BinaryNbtOptions BedrockEdition = new() { IsLittleEndian = true, UseVarInt = false };
    public static readonly BinaryNbtOptions BedrockEditionNetwork = new() { IsLittleEndian = true, UseVarInt = true };

    private Encoding stringEncoding = ModifiedUTF8Encoding.Instance;
    public NbtOptions GenericOptions = options;
    public bool IsLittleEndian { get; set; }
    public bool UseVarInt { get; set; }
    public Encoding StringEncoding
    {
        readonly get => stringEncoding;
        set => stringEncoding = value ?? ModifiedUTF8Encoding.Instance;
    }

    public BinaryNbtOptions() : this(new() { HasRootName = true })
    {
    }
}