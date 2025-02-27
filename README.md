# ElysiaNBT
[![NuGet version (ElysiaNBT)](https://img.shields.io/nuget/v/ElysiaNBT?color=FF9AFC&style=for-the-badge)](https://www.nuget.org/packages/ElysiaNBT)

**English** | [简体中文](./README.zh-Hans.md)

Yet another NBT serializer/deserializer, making serializing/deserializing NBT as easy as JSON.

## Dependencies
- .NET 9.0
- [Modified-UTF8-Encoding](https://github.com/Myitian/Modified-UTF8-Encoding) v1.2.2

## Features
- Use native types instead of specialized types like NbtCompound;
- Automatically find the best matching type converter;
- Automatically cache reflection results and converters;
- Supports Java Edition NBT, Bedrock Edition NBT and SNBT;
- Support deserialization to object type (default is Dictionary&lt;string, object>);
- Support deserialization to dynamic type (default is ExpandoObject);
- You can use `NbtEntryNameAttribute` to specify the entry name;
- You can use `NbtEntryNameAttribute` to specify the entry order;
- You can use `NbtIgnoreAttribute` to specify ignore rules;
- You can use `NbtConverterAttribute` to specify converters;

## TODO
- Source generator (to support AOT generation)

## References
- [minecraft.wiki](https://minecraft.wiki/w/NBT_format)
- [wiki.vg (archive on minecraft.wiki)](https://minecraft.wiki/w/Minecraft_Wiki:Projects/wiki.vg_merge/NBT)

<img src="./icon.png" width="256" alt="爱门">