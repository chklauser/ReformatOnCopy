``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=6.0.202
  [Host]     : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT
  DefaultJob : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT


```
|              Method |                 Text |        Mean |     Error |   StdDev |         P95 | Ratio | Allocated |
|-------------------- |--------------------- |------------:|----------:|---------:|------------:|------:|----------:|
| **ReformatInterpreted** |            **ProsePage** | **12,933.5 ms** |  **67.23 ms** | **52.49 ms** | **13,003.1 ms** |  **1.00** |    **180 KB** |
|    ReformatCompiled |            ProsePage |  3,750.4 ms |  17.16 ms | 14.33 ms |  3,773.3 ms |  0.29 |    177 KB |
|                     |                      |             |           |          |             |       |           |
| **ReformatInterpreted** |    **HeadingsHeavyPage** |  **7,479.9 ms** | **101.08 ms** | **94.55 ms** |  **7,613.0 ms** |  **1.00** |    **170 KB** |
|    ReformatCompiled |    HeadingsHeavyPage |  2,162.8 ms |  36.24 ms | 33.90 ms |  2,211.4 ms |  0.29 |    170 KB |
|                     |                      |             |           |          |             |       |           |
| **ReformatInterpreted** | **HeadingsHeavySnippet** |    **604.4 ms** |   **3.70 ms** |  **3.28 ms** |    **609.2 ms** |  **1.00** |     **59 KB** |
|    ReformatCompiled | HeadingsHeavySnippet |    170.3 ms |   1.79 ms |  1.49 ms |    173.2 ms |  0.28 |     55 KB |
