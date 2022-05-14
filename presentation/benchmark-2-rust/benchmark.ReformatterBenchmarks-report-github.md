``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=6.0.202
  [Host]     : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT
  DefaultJob : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT


```
| Method |                 Text |           Mean |        Error |       StdDev |            P95 | Ratio |  Gen 0 | Allocated |
|------- |--------------------- |---------------:|-------------:|-------------:|---------------:|------:|-------:|----------:|
| **Dotnet** |            **ProsePage** | **3,860,886.3 μs** | **61,330.57 μs** | **57,368.65 μs** | **3,964,122.8 μs** | **1.000** |      **-** |    **177 KB** |
|   Rust |            ProsePage |       883.6 μs |     12.06 μs |     11.28 μs |       899.5 μs | 0.000 | 0.9766 |     13 KB |
|        |                      |                |              |              |                |       |        |           |
| **Dotnet** |    **HeadingsHeavyPage** | **2,182,444.1 μs** | **27,576.73 μs** | **24,446.05 μs** | **2,230,336.6 μs** | **1.000** |      **-** |    **172 KB** |
|   Rust |    HeadingsHeavyPage |       645.3 μs |      2.31 μs |      1.93 μs |       648.9 μs | 0.000 | 3.9063 |     34 KB |
|        |                      |                |              |              |                |       |        |           |
| **Dotnet** | **HeadingsHeavySnippet** |   **171,958.6 μs** |    **872.86 μs** |    **681.47 μs** |   **172,782.8 μs** | **1.000** |      **-** |     **55 KB** |
|   Rust | HeadingsHeavySnippet |       130.5 μs |      1.71 μs |      1.60 μs |       133.3 μs | 0.001 | 1.7090 |     16 KB |
