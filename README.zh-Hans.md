# ElysiaNBT
[![NuGet version (ElysiaNBT)](https://img.shields.io/nuget/v/ElysiaNBT?color=6cf&style=for-the-badge)](https://www.nuget.org/packages/ElysiaNBT)

[English](./README.md) | **简体中文**

又一个NBT序列化/反序列化器，让序列化/反序列化NBT就像JSON一样简单。

## 依赖
- .NET 9.0
- [Modified-UTF8-Encoding](https://github.com/Myitian/Modified-UTF8-Encoding) v1.2.2

## 特性
- 使用原生类型，不使用类似NbtCompound这样的专用类型；
- 自动查找最匹配的类型转换器；
- 自动缓存反射结果和转换器；
- 支持Java版NBT、基岩版NBT以及SNBT；
- 支持反序列化到object类型（默认为Dictionary&lt;string, object>）
- 支持反序列化到dynamic类型（默认为ExpandoObject）
- 可用`NbtEntryNameAttribute`指定项名；
- 可用`NbtEntryOrderAttribute`指定顺序；
- 可用`NbtIgnoreAttribute`指定忽略规则；
- 可用`NbtConverterAttribute`指定转换器；

## TODO
- 源生成器（用于支持AOT生成）

## 参考资料
- [minecraft.wiki](https://minecraft.wiki/w/NBT_format)
- [wiki.vg（minecraft.wiki上的归档）](https://minecraft.wiki/w/Minecraft_Wiki:Projects/wiki.vg_merge/NBT)

<img src="./icon.png" width="256" alt="爱门">