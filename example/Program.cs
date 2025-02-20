using ElysiaNBT;
using ElysiaNBT.Serialization;
using System.Numerics;

namespace Example;

internal class Program
{
    static void Main(string[] args)
    {
        List<Ex> e0 = [new(), new() { Val = AA.B }];
        Ex2 e1 = new();
        string s0 = NbtSerializer.SerializeString(e0, StringNbtOptions.Minimal);
        Console.WriteLine(s0);
        List<Ex> ee = NbtSerializer.DeserializeString(s0, typeof(List<Ex>)) as List<Ex>;
        string s1 = NbtSerializer.SerializeString(e1, StringNbtOptions.Minimal);
        Console.WriteLine(s1);
    }
}

class Ex
{
    [NbtIgnore(Condition = NbtIgnoreCondition.Never)]
    public AA Val { get; set; }
    [NbtEntryName("$ %")]
    private Vector2 VV = new Vector2(114.514f, 1919.810f);
}
public partial class Ex2
{
    public AA Val { get; set; }
    [NbtEntryName("$ %")]
    private Vector2 VV = new Vector2(114.514f, 1919.810f);
}
[NbtIgnore]
public enum AA
{
    A,
    B,
    C
}
