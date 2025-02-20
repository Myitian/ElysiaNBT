using Benchmark.Models;
using BenchmarkDotNet.Attributes;

namespace Benchmark;

[ShortRunJob]
[MeanColumn]
[MemoryDiagnoser]
[MarkdownExporter]
public class Level_1_NBT
{
    static byte[] Data;
    [GlobalSetup]
    public void Setup()
    {
        using Stream s = Program.Entries.First(e => e.Name == "level.1.nbt").Open();
        using MemoryStream ms = new();
        s.CopyTo(ms);
        Data = ms.ToArray();
    }

    [Benchmark(Baseline = true)]
    public void Benchmark_ElysiaNBT()
    {
        using MemoryStream stream = new(Data);
        ElysiaNBT.BinaryNbtOptions options = ElysiaNBT.BinaryNbtOptions.JavaEdition;
        options.GenericOptions.HasRootName = true;
        object result = ElysiaNBT.Serialization.NbtSerializer.DeserializeBinary<object>(stream, options);
    }

    [Benchmark]
    public void Benchmark_ElysiaNBT_ToObject()
    {
        using MemoryStream stream = new(Data);
        ElysiaNBT.BinaryNbtOptions options = ElysiaNBT.BinaryNbtOptions.JavaEdition;
        options.GenericOptions.HasRootName = true;
        Level result = ElysiaNBT.Serialization.NbtSerializer.DeserializeBinary<Level>(stream, options);
    }

    [Benchmark]
    public void Benchmark_AadevNBT_NoStream()
    {
        Aadev.NBT.NTag result = Aadev.NBT.NReader.FromByteArray(Data);
    }

    [Benchmark]
    public void Benchmark_CyotekDataNbt()
    {
        using MemoryStream stream = new(Data);
        Cyotek.Data.Nbt.Serialization.BinaryTagReader reader = new(stream);
        Cyotek.Data.Nbt.TagCompound result = reader.ReadDocument();
    }

    [Benchmark]
    public void Benchmark_DaanV2NBT()
    {
        using MemoryStream stream = new(Data);
        DaanV2.NBT.ITag? result = DaanV2.NBT.Serialization.NBTReader.Read(stream, DaanV2.NBT.Endian.Big);
    }

    [Benchmark]
    public void Benchmark_fNBT()
    {
        using MemoryStream stream = new(Data);
        fNbt.NbtReader reader = new(stream);
        fNbt.NbtTag result = reader.ReadAsTag();
    }

    [Benchmark]
    public void Benchmark_InkNbt_NoStream()
    {
        using MemoryStream stream = new(Data);
        Ink.Nbt.Tags.NbtTag result = Ink.Nbt.Serialization.NbtSerializer.Deserialize<Ink.Nbt.JavaNbtDatatypeReader, Ink.Nbt.Tags.NbtTag>(Data, Ink.Nbt.Tags.NbtTag.TypeInfo);
    }

    [Benchmark]
    public void Benchmark_KonvesNbt()
    {
        using MemoryStream stream = new(Data);
        Konves.Nbt.NbtReader reader = new(stream);
        Konves.Nbt.NbtTagInfo info = reader.ReadTagInfo();
        Konves.Nbt.NbtTag result = reader.ReadTag(info);
    }

    [Benchmark]
    public void Benchmark_KonvesNbt_ToObject()
    {
        using MemoryStream stream = new(Data);
        Konves.Nbt.Serialization.NbtSerializer serializer = new(typeof(Level));
        object result = serializer.Deserialize(stream);
    }

    [Benchmark]
    public void Benchmark_McProtoNetNBT()
    {
        using MemoryStream stream = new(Data);
        McProtoNet.NBT.NbtReader reader = new(stream);
        McProtoNet.NBT.NbtTag result = reader.ReadAsTag();
    }

    [Benchmark]
    public void Benchmark_MinesharpNbt()
    {
        using MemoryStream stream = new(Data);
        Minesharp.Nbt.Reader.TagReader reader = new(stream);
        Minesharp.Nbt.CompoundTag result = reader.ReadTag();
    }

    [Benchmark]
    public void Benchmark_MoCNBT()
    {
        using MemoryStream stream = new(Data);
        MinecraftNBTLibrary.NBTNode result = MinecraftNBTLibrary.NBT.ParseFromStream(stream);
    }

    [Benchmark]
    public void Benchmark_NBTStandard()
    {
        using MemoryStream stream = new(Data);
        NBT.Serialization.BinaryTagReader reader = new(stream);
        NBT.TagCompound result = reader.ReadDocument();
    }

    [Benchmark]
    public void Benchmark_NbtLib_Debug()
    {
        using MemoryStream stream = new(Data);
        NbtLib.NbtParser nbtParser = new();
        NbtLib.NbtCompoundTag result = nbtParser.ParseNbtStream(stream);
    }

    [Benchmark]
    public void Benchmark_NbtLib_Debug_ToObject()
    {
        using MemoryStream stream = new(Data);
        NbtLib.NbtDeserializer deserializer = new();
        Level result = deserializer.DeserializeObject<Level>(stream);
    }

    [Benchmark]
    public void Benchmark_NormalNBT()
    {
        using MemoryStream stream = new(Data);
        NormalNBT.NBT result = NormalNBT.NBT.Read(stream);
    }

    [Benchmark]
    public void Benchmark_RaspiteSerializer_NoStream()
    {
        Raspite.Serializer.Tags.TagBase result = Raspite.Serializer.BinaryTagSerializer.Deserialize(Data);
    }

    [Benchmark]
    public void Benchmark_SharpNBT()
    {
        using MemoryStream stream = new(Data);
        SharpNBT.TagReader reader = new(stream, SharpNBT.FormatOptions.Java);
        SharpNBT.Tag result = reader.ReadTag();
    }

    [Benchmark]
    public void Benchmark_SmartNbt_Debug()
    {
        using MemoryStream stream = new(Data);
        SmartNbt.NbtReader reader = new(stream);
        SmartNbt.Tags.NbtTag result = reader.ReadAsTag();
    }

    [Benchmark]
    public void Benchmark_WaxNBT_Debug()
    {
        using MemoryStream stream = new(Data);
        WaxNBT.NbtFile result = WaxNBT.NbtFile.Parse(stream);
    }
}
