namespace Benchmark.Models;

public class BigTest
{
    public byte byteTest { get; set; }
    public short shortTest { get; set; }
    public int intTest { get; set; }
    public long longTest { get; set; }
    public float floatTest { get; set; }
    public double doubleTest { get; set; }
    public string? stringTest { get; set; }

    [ElysiaNBT.Serialization.NbtEntryName("nested compound test")]
    [NbtLib.NbtProperty(PropertyName = "nested compound test")]
    public Dictionary<string, NestedObject>? nested_compound_test { get; set; }

    [ElysiaNBT.Serialization.NbtEntryName("listTest (long)")]
    [NbtLib.NbtProperty(PropertyName = "listTest (long)")]
    public List<long>? listTest_long { get; set; }

    [ElysiaNBT.Serialization.NbtEntryName("listTest (compound)")]
    [NbtLib.NbtProperty(PropertyName = "listTest (compound)")]
    public List<ListObject>? listTest_compound { get; set; }

    [ElysiaNBT.Serialization.NbtEntryName("byteArrayTest (the first 1000 values of (n*n*255+n*7)%100, starting with n=0 (0, 62, 34, 16, 8, ...))")]
    [NbtLib.NbtProperty(PropertyName = "byteArrayTest (the first 1000 values of (n*n*255+n*7)%100, starting with n=0 (0, 62, 34, 16, 8, ...))")]
    public byte[]? byteArrayTest { get; set; }

    public class NestedObject
    {
        public string? name { get; set; }
        public float value { get; set; }
    }
    public class ListObject
    {
        public string? name { get; set; }

        [NbtLib.NbtProperty(PropertyName = "created-on")]
        public long created_on { get; set; }
    }
}