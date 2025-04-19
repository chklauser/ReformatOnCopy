using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using ReformatOnCopy;
// ReSharper disable StringLiteralTypo

namespace benchmark;

[Config(typeof(Config))]
public class ReformatterBenchmarks
{
        
    private class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.Default);
            AddDiagnoser(MemoryDiagnoser.Default);
            AddColumn(StatisticColumn.P95);
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum Texts
    {
        ProsePage = 0,
        HeadingsHeavyPage = 1,
        HeadingsHeavySnippet = 2,
    }
        
    [ParamsAllValues]
    public Texts Text { get; set; }

    private Reformatter reformatterHeadingBaseline = null!;
    private Reformatter reformatterHeadingAlternative = null!;
    private Reformatter reformatterHeadingAlternative2 = null!;

    [GlobalSetup]
    public void Setup()
    {
        reformatterHeadingBaseline = new(Splittermond.Headings, ReformatPasses.All, ReformatRegexMode.AllCompiled);
        reformatterHeadingAlternative = new(Splittermond.Headings, ReformatPasses.All, ReformatRegexMode.AggregateHeadingsCompiled);
        reformatterHeadingAlternative2 = new(Splittermond.Headings, ReformatPasses.All, ReformatRegexMode.NoneCompiled);
    }

    [Benchmark(Baseline = true)]
    public string Reformat()
    {
        return reformatterHeadingBaseline.Reformat(_texts[(int)Text]);
    }
    [Benchmark]
    public string ReformatAlt()
    {
        return reformatterHeadingAlternative.Reformat(_texts[(int)Text]);
    }
    [Benchmark]
    public string ReformatAlt2()
    {
        return reformatterHeadingAlternative2.Reformat(_texts[(int)Text]);
    }

    #region Texts

    private static readonly string[] _texts = [
        @"
Klima
Zzzz Zzzzzzzzzz Zzzzzzzzz Zzzzzz Zzzzzz Zzzz Zzzz Zzzzzzzzzzzz Zzzz-
Zzzz, Zzzzzzzz Zzzzz Zzzzzz, Zzzzz Zzzz Zzzz Zzzzzzzzz Zzzz Zzzzzzzzz-
Zzz Zzzzzzzzz Zzzz Zzzz „Zzzzzzzzzzz“ Zzzzzzzzzzzzz Zzzzzzz: Zzzz Zzzzzzz
Zzzzz Zzzzz und Zzzzzzzzzzz, Zzzzzzzzzz Zzzzz Zzzzzzzzzz, Zzzz Zzzzzzz
Zzzzz, Zzzzz Zzzz Zzz Zzzzzzzzzzz Zzz Zzzz Zzzzzzzzzz Zzzzzz Zzzz-
Zzzzzz, und Zzzz Zzzzzzzzzzzzz Zzzzzzzzzzzz Zzzzzz Zzzzzzz Zzzzz Zzzz
Zzzzzzzzzz.
Zzzzzzz Zzzzz Zzz Zzz Zzzzzzz Zzzz Zzz Zzzz Zzzzzzzzz – Zzz Zzzzzzzz Zzzz
Zzzzzzzzzzz und Zzz Zzzzzzzzz Zzzz Zzzzzzzzzz Zzzzzz Zzzzz Zzzzzzzzzz.
Zzz Zzzzzzzzz Zzzzzzzzzzz Zzzzz Zzzzz Zzzzzz Zzzzzz Zzz Zzzzzzzzz Zzz-
Zzzzzzz, Zzzzzzz Zzzzzzz Zzzzzzzzzzzz Zzzz Zzzzzzz Zzzz Zzzz Zzzzzzzzzz
Zzzzzzzzzzzzzz Zzzzz.
Flora und Fauna
Zzzzzzzzz Zzzzzz Zzzzzzzzzz Zzzzz Zzzzzzz, Zzzzzzzzzz, Zzzzzzz und
Zzzzzzzzzzzz, Zzzzz Zzzzzz Zzzzzzzzzzzzzz Zzzzzz Zzzzz Zzzz Zzzzzzzz-
Zzzz Zzzz Zzzzzzzz Zzzz Zzzzzz Zzzzzzzzz. Zzzzzz Zzzzzzzzz Zzzzz Zzz Zzzzz Zzzz
Zzzzz Zzz Zzzzzzz Zzzzzzzzzzzzz Zzzzzzzzzzzzzz, Zzzzz Zzzzzz Zzzzzzzzzz-
Zzzzz Zzzz Zzzzzzzzzzzz Zzzzzzzzz. Zzz Zzzz Zzzzzz Zzzzzzz Zzzzzzzzz Zzzzz,
Zzzzzz Zzzz Zzzzzzz, Zzzzzzzzzzz und Zzzzzzzzzzzzzzz Zzzzzzzzzzzzz
Zzzz Zzzzzzzzzzzzzzz Zzzzzzz- und Zzzzzzzzzzzzzz Zzzzzz Zzzzzzzz.
Zzz Zzzz Zzzzzzzzz Zzzz Zzzzzzzzzzz Zzzz Zzzz Land Zzzzzzzzzzz, Zzzz
Zzzzzzz Zzzzz Zzzzzzzzzz Zzzzzzzzz Zzzzzzz und Zzzzzzz Zzzzzzzzz
Zzzz Land. Zzzzzzz Zzz Zzzz Zzzzzzz Zzzzzzz Zzzzz Zzzzzz Zzzz Zzz Zzzzzz-
Zzzz Zzzzz Zzzz Zzzzzzzzzzz, Zzzzz Zzzz Zzzz Zzzzz Zzzzzzz Zzzzzzzzz Zzzz
Zzzzzzzzzz Zzzzzz Zzzzzzzz, Zzzz Zzzzzz Zzzzzzz und Zzzzzzzzzzzzzz
Zzzzzzz. Zzz Zzzzz Zzzzz Zzzzzzzzzzzzz Zzzzzzz, Zzzzzzz Zzzz Zzzzzzzzz
Zzzz Zzzzzz Zzzzzzzzz. Zzzzzzzzzzz Zzzzzzzzz Zzzzzzzzzz Zzzzz Zzzzzzz
und Zzzzz, Zzzzzzzz Zzzzzzz Zzzzzzzzzzzzzz und Zzzzzzzzz, Zzzzz
und Zzzzz Zzzzzzzzz.
Zzzzzz Zzzz Zzzzzzzzzz Zzzzzzzzzzz Zzzzzzz Zzz Zzzzzzzzz Zzzzzz
Zzzzzzz, Zzzzzzzzzz und Zzzzz Zzzzzzzzzzz Zzzzzzzzzz (Zzzzzzzzzzz
Zzzzz Zzzzz Zzzzz und Zzzzzzzzzz Zzz Zzzzzzzzz), Zzzzzzzzzzz Zzzz
Zzzzzzzz Zzzz Zzzzzz Zzzzzz- und Zzzzzzzzzz, Zzzzzzzzzz Zzzz Zzzzzz
„Zzzzzzzzzzzzz“ – Zzzzz Zzzzzz, Zzzzzzzzzzzzz Zzzzzzzzzzzzzzzzz.
Zzzz Zzzzzzzzz Zzz Zzzzzzzzz Zzzzz Zzzzzzzzzzz, Zzzzzzzzz, Zzzzzzzzzz
und Zzzzzzzzzzzzzz Zzzzzz Zzzzzz Zzzzzzz Zzzzzzz Zzz Zzzz Zzzzzzzzz-
Zzzzzzzzzzz.
Zzzzzzzz Zzzzzz Zzzzzz Zzzzz Zzzzzzz, Zzzzzzzzzz Zzzzz Zzzz Zzz Zzzz Zzzzzz
Zzzzzzzz Zzzzzzzzzzzzzz Zzzzz Zzzzzz Zzzzzzzzz, Zzzzz Zzzzzz Zzzz
Zzzzzzzz Zzzzzzzz und Zzzzz Zzzzz Zzzzzzzz Zzzzzzzzzzzzzz Zzzzzzzzzz.
Zzzzzzzzzzz und Zzzzzzzzzzzz Zzzzzzz Zzzzz Zzzzzzz Zzzzzzzzzzzzz Zzz
Zzzzzz Zzzzzzz Zzzzzz Zzzzzzzzzzz.
Zzzzzz Zzzzzzzz Zzzzzzzzzzzzzzz (Zzzz Zzzz Zzzzzzzz Zzzzzzzzzz-
Zzzz), Zzzz Zzzzzzzzzzzzz Zzzzzz Zzzzzzzzz und Zzzz Zzzzzzzzzzzz
Zzzzz Zzz Zzzzzz Zzzzzzzzz Zzzzzz. Zzzzzzzzzz und Zzzzzzzzz Zzzzzzzz
Zzzzz Zzzzzzz Zzzzzzz, Zzzz Zzzzzz Zzzzzzzzzzzzzzz Zzzz, Zzzzz
Zzzz Zzzzzzzz Zzzzzzzzzz Zzz Zzzz Zzzzzzzzzzz Zzzzzz Zzz Zzzzz Zzz
Zzzzzzz Zzzzzzz.
Zzzzzzzzz und Zzzzzzzz – Zzzz Zzzzzzzzzzzz Zzzzzzzzzzzzz
Zzzz Zzzzzzzz Zzzzzzzzz Zzzzzz Zzz Zzzz Zzzzzzzzzzz und Zzzz Zzzzzzz-
Zzzz, Zzzzzzzzzzzzzzzzzz Zzzzzzzz. Zzzzz Zzzzz Zzzzzzzzzz Zzzzzz Zzzzzzz
Zzz Zzzz Dörfern Zzzz Zzzzzzzz und Zzzzz Zzzzzzz, Zzzz Zzzz Zzzzzzz Zzz-
Zzzzzzzzzz Zzzzzzzzzzz Zzzz Zzzz Zzzzzzzzzzzz Zzzzzzzzzzzzz.
Zzz Zzzzzz Zzzzzzz Zzzzzzz Zzzz Zzzzzzzzz, Zzzzzzzzzzzz Handelsherren,
Zzzzzzzzz Zzzzzzzzzzzzzzzzzz Zzzzz Zzzzzzzzzzzzzzzz. Zzz Zzzzz Zzzzzz
Zzzzzzzzzz Zzzzzzzzzzzz, Zzz Zzz Zzzzzzz Zzzzzzz Zzz Zzzzzzzz, Zzzzzzzzz
Zzzzz Zzzz Zzzz Zzzz Zzzzzzzz Zzzzzzzzz Zzzz Zzzzzzzzzzzzzz Zzzzzzzzzz
Zzzzzzz – Zzzz Zzzzzzzzzzzzzzzzz Zzzz Zzzzzzz Zzzzzzzzzz.
Zzz Zzzzzzzz Zzzzzzz Zzzzzz Zzzz Zzzzzzz Zzzzzzzz Zzzz Zzzzzzz: Zzzz Zzzz
Zzzzzzzzzz Zzzzzz Zzzz Zzzzzzz Zzzzzz Zzzzzzzzzzzzzz Zzzzzzzzzzz
Zzzzz, Zzzzzzz Zzzzzz Zzzzzzz, Zzzz Zzzz Zzzz Zzzz Zzzzzzzzz Zzzzzzzz Zzzz
Zzzzzzzzzzz Zzzzzzzzz Zzzzzzz. Zzz Zzzz Zzzzzzz Zzzzz Zzzz Zzzzzzzz Zzzz-
Zzzz Zzzzzz Zzzzzzzzz Zzzz Zzzzzzzzz Zzzzzzzzzzzzz und Zzzz Zzzzzzz
Zzzzzzzzzzz Zzzzzzzzz.
Zzzz Zzzzzzzzz Zzzz Einwohner Zzzzzzzzzz Zzzzz Zzzzz Zzzz Zzzzzzz-
Zzzzzz Zzzzzzz Zzzzzz Zzzzzzzz und Zzzzz Zzzzzz Zzzz Zzzzzzzzzzzz: Zzzzzzzz
Zzzzzzzzz Zzzzzzzzz Zzzz Zzzz Zzzzzzzzz Zzz Zzzz Zzzzzzzzzz Zzzz Zzzzzzzzz,
Zzzzzzzzzz Zzzzzzzz Zzzzzzzz Zzzz Zzzz Zzzzzzzzzzzzz Zzzzz Zzzzz Zzzz Zzzz
Zzzzzzzz Zzzzzzzzzzz.
Zzzzz Zzz Zzzzz Zzzzz Zzzzzzzzzz: Zzzzz Zzzzz Zzz Zzz Zzzzzzzzzzz Zzzzzz
Zzzzzzzzzzzzzzzz Zzzzz Zzzzzzzzzz, Zzzzz Zzzz Zzzz Zzzzzzzzzzzzz Zzzzzzz-
Zzzzz und Zzzzzzzzz Zzzzzzzzzz Zzzzzzzzzzzzzz Zzzzz Zzzzzz Zzzzz Zzz-
Zzzz. Zzzz Zzzzzzzzzzzzzz Zzzzzzz Zzzz Zzz Zzzzz Zzzzzzzzz Zzz Zzzzzzzzz-
Zzzzz und Zzzzzzzz, Zzzz Zzzzz- und Zzzzzzzzzzzzz Zzzzzzzzzzz.
Zzzz Zzzzzzzzzzzzzzzzzzzzzz Zzzz Zzzzzzzzzzz und Zzzzzzz Zzzzzzzzz
Zzzzz Zzzz Zzzz Land und Zzz Zzzz Dörfern Zzz Zzzzzzzzzz Zzzzzzzz
Zzzzzzz: Zzz Zzzzzzzz Zzzzzzzzzz Zzzzzzz Zzzzz Zzzz Zzzzzzz Zzzz, Zzzzzz
und Zzzzzzzz Zzzzzzz Zzzz Zzzzzzzzzzzz Zzzzzzzzzz, Zzzz Zzzzz Zzzzzzz
Zzzzz Zzzzzzzzz Zzzzzzzzzzz Zzzz.
Zzzzzzzzzzz
Zzzzzzzz Zzz Zzzzzzz Zzzz Zzzzzzzzzzzzz Zzzzzzzz Zzzz Zzzzzz Zzzzzz
Zzzzzzzzzz Zzzzz Zzzz Zzzzzz Zzzzzzzzz und Zzzzz Zzzzzzz. Zzzz Zzzzzz
Zzzzzzz, Zzzzz Zzzzzz Zzzzzzz Zzzzzzzzz Zzzzzz Zzzzzzz Zzz Zzzzzzz,
Zzzz Zzz Zzzz Zzzzzzzzzz Zzzzzzzzzzzzzz Zzzzzz Zzzzzzzzzzzz und
Zzzzzzzzzzzz Zzz, Zzzzzzz Zzzz Einwohnerschaft Zzzz Zzzzzzzzzz Zzzzzz
Zzzz Zzzzzzzzzzzzz Zzzzzzzzzzzzzz Zzzzzzz. Zzzzzzzzzzzzzzzz Zzzzzz-
Zzzz Zzzzz Zzzz Zzzzzzzzzz Zzzzzz Zzzzzzzzzzzzzz und Zzzzzzzz Zzzzz
Zzzzz Zzzzzzzzzzzzzzzzz Zzzz. Zzzzzzzz Zzzzzzz Zzzzzzz Zzzz Zzzz Zzz-
Zzzzzzzzz, Zzzzzzzzzzzzzz und Zzzzzzz Zzz Zzzzzz Zzzzzzzzz Zzzzzzzz-
Zzzzzzzzzzz.
Zzzzzzzzz Zzzzzzzzzzzzz Zzzzzzzzzzz Zzzz Zzzzzz Zzzzzz Zzzz Zzzzzzzzzzzzzz
Zzzzzzz und Zzzzzzzzzzzzzzzz, Zzzzz Zzzzz Zzzz Zzzzzzzz Einwohner
Zzzz Zzzzzzzzzzzz Zzzzzzzzzzzzzzzzz und Zzzz Zzzzzz Zzzzzzzzz Zzzzzzzz
Zzzzzzzz.
Zzzzzzz und Zzzzzz
Zzz Zzzzz Zzzzzz Zzzzzzzzzzzzzzz Zzzzz Zzzzz Zzzz Zzzz Zzzzzzz Zzzzzzzzz:
Zzz Zzzzz Zzzz Zzzzz Zzzzzzzzzz, Zzzz Zzz Zzzz Zzzzzzzzzzz Zzzzzzzzzz Zzz
Zzzzzzz Zzzzzz. Zzz Zzzzzzz Zzz Zzzz Zzzzzzzzzzzzz Zzzzzzzzzzz Zzzzzzz
Zzz Zzzzzzzzz Zzzzzz Zzzzzzz Zzzz Zzzzzzzzzzzzzzzzz. Zzzzzz Zzzz-
Zzzz Zzzzzzz Zzzzzzzzzzz, Zzzz Zzzz Zzzzzzzzzzz Zzzz Zzzzzzzzzzz Zzzzzz
Zzzzzzz.
Zzzzzzzz Zzzzzzzzzz Zzzz Zzzzzzzzz Zzzzzzzzz Zzzz Zzzzzz Zzzz Zzzz-
Zzzzzzzz, Zzzzzzzzzzz Zzzz Landes und Zzzzzzzzzzzzzzzz Zzzzzzzz Zzzz
Zzzzzzzzzzzzz Zzzzzzz. Zzzzzzz Zzzzzz Zzz Zzzz Zzzz Zzzzzzzzzzzzzzz
Zzzzzzzzzzzzzz Zzzzzzz Zzzzzzzzz, Zzzzzzz Zzzzzzz Zzzzzz Zzzz Zzzz
Zzzzzzzzzzz Zzzzzzzz Zzzzz Zzzzzz Zzzzzzzzzz Herrschaftsgott.
",@"
Zzzzzzzz
Zzz Zzzzzz Zzzz Zzzzzzzzzz, Zzzz Zzzz Zzzzzzzzzzzzzzz Zzz Zzzzzzzz Zzzzzzz,
Zzzzz Zzz Zzzzz Zzzzzzz Zzzzzzzzz Zzzzzzzz und Zzzzz Zzzzzzzzzzz Zzzzzz
Zzzzz Zzzzz Zzzzzzzzzzzzz Zzzzzzzzzzzz. Zzz Zzzzzzz Zzzzzzzzzz Zzz
Zzzzzzz, Zzzzzzzzz Zzz Zzzzzz Zzzz Zzzzzz Zzzzzzzzzzzz Zzzzzzz.
Zzzzzzzzzzzzzz
Zzzz Zzzzzzzzzzzzzz Zzzzzzzzzzzz, Zzzzzzz, Zzzzzzzzzzzzz Zzzzz Zzzzzz-
Zzzzzzz Zzz Zzzzzzzzzz, Zzzz Zzzz Zzzzzzzzzz (Zz. 000). Zzzzz Zzzz Zzzzzz-
Zzzzzzz Zzzzzzzzzzzzzz (Zz. 000) Zzzz Zzzzz Zzzzzzzzzzz und Zzzzz Zzzzzzz-
Zzzz. Zzzzzzzzzzz Zzzzzzzzzzzz (Zz. 000) Zzzzzzz Zzzzzz Zzzzzzz, Zzzzzzz
Zzzz Zzzzzzzzzzz Zzzz Zzzzzzz, und Zzzzz Zzzz Zzzzzzzzzzz Zzzzzz-
Zzzzzz Zzzzzzzzzzzz (Zz. 000) Zzzzzzz Zzz Zzzzzzz Zzzzzzz Zzzzzzzzzzzzz.
Zzzzzz und Zzzzzzz Zzzzzz Zzzzzzzzzz
Zzzz Zzz Zzzzzzz Zzzzzzzz Zzzzzzzzzz Zzzzzzzzzzzzzzzz
Zzzzzzz Zzzz Zzzz Zzzzzzzzzzz Zzzz Zzzzzz Zzzz Zzzz Zzzzz
Zzzzzzzzzzzzzz. Zzzz Zzzzzzzz Zzzzzzzzzz (Zzzzzzz, Zzz-
Zzzzzz Zzzzzzz, Zzzzzzzzz) Zzzz Zzzz Zzzzzzzzzzzzzz Zzz 0
(Zzzzz Zzzzzzzzzzzz Zzzzzzzzzz) Zzzzz Zzz 00 (Zzzzzzzz Zzzz-
Zzzzzzz, Zzzzzzzzzz) Zzzzzzz.
Zzzzzzzz Zzzz Zzzzzzzz
Zzzz Zzzzzzzzzzzzz Zzzzzzzzzzzzzzzzz Zzzz Zzzzzzzzzz Zzzz Zzzz Zzzz-
Zzzzz Zzzz Zzzzzzzzzzz Zzzzzzzz, Zzz Zzzzzzz Zzzz Zzzzzz Zzzzzzzzzzzzz,
Zzzzzzzzzzzzz, Zzzzzzzzz Zzzzz Zzzzzzz Zzzzzzzzz Zzzzzzzzzzz Zzzz.
Zzzzzzzzzz Zzzz Zzzzzz Zzzzzzzzz: Zzzz Zzzzzzz Zzzzzz Zzzzzzzzzz Zzzzzz
Zzzz Zzzzzzzzzzz Zzzzzz Zzzzzzz Zzzz Zzzz Zzzzzz, Zzzzzz Zzzz Zzzzz, Zzzzz
Zzzzzzzzzz Zzzz Zzzzzzz Zzz Zzzzzzzz.
Probe: Zzzzzzzzz Zzzzzzzzzz-Probe, Zzzz Zzzzzzzzzzzzzz Zzzzzzzz 00
Zzzzzzzzzz Zzzz Zzzzzzzzzz Zzzzzzzzzzzzz Zzzz Zzzzzzzzzzzzzzzzz, Zzzz
Zzzz Zzzzz Zzzz Zzzzzzzzzzz Zzzzzzzzz (Zzzzz 00 Zzzz Zzzzzzzzzzz, 00 Zzzz
Zzzzzzzzzzzzz, 00 Zzzz Zzzzzzzzzzz Zzzz.).
Zzzz Zzzz und Zzzzzzzz Zzzzz Zzzz Zzzzz Zzzzzzzz Probe Zzzzzzzzz Zzzzzzz.
Zzzz Probe Zzzzzz 0 Zzzzzzzz Zzz Zzzzzzzzz.
Auswirkungen
Verheerend: Zzzz Zzzzzzz Zzzzzzzzzzz Zzzz Zzzzzzz Zzzzzzzzzzzzz und
Zzzz Zzzzzzzz Zzzzzzzzzz. Zzzzzzzzzzz Zzzzzzz Zzzz Zzzzzzzz 0 Zzzzzzzzz-
Zzzzzzz +0 Zzzz Zzzzzzzzzz Zzzzzzzzzzzz.
Misslungen: Zzzz Zzzzzzzz Zzzzzzzzzz und Zzzz Zzzzzzzz Zzzzzzz 0
Zzzzzzzzzzzzzz Zzzz Zzzzzzzzzz Zzzzzzzzzzzz.
Zzzzzz misslungen: Zzzz Zzzzzzzzzzz Zzzzzzz Zzzzzz Zzzzzzz und
Zzzz Zzzzzzzz Zzzzzzzzzz. Zzzz Probe Zzzzzz Zzzzz Zzzzzz Zzzz Zzzz Zzzzzzz
Zzzz Zzzzzzzzzz Proben Zzzz Zzzz.
Gelungen: Zzzz Zzzzzz Zzzz Zzzzzzzzzz Zzzzz Zzzzzzzzzz Zzzzzzzzz.
Zzzz Zzzzzzzz Zzzzzzzz Zzzz Zzzzzzzzz Zzzzzzzzzz Zzzzzzz Zzzzz Zzz 0
Zzzzzz +0 Zzzz Zzzzzzzzzzzz.
Herausragend: Zzzzzzzzzzz Zzzz Zzzzzzz Zzzzzz gelungenen Zzzzzzzz
Zzzzzzz Zzzz Zzzzzzzz Zzzz Zzzz Zzzzzzz 0 Zzzzzzzzzzzz Zzzzzzz.
Ausrüstung und Umstände
Zzzzzzzzzz Zzzzzzzz Zzzzzzzzz (Zzzzzzz Zzzzzzzz), Zzzzzzzzzzzzz Zzz-
Zzzzzzz (Zzzzzzz Zzzz Zzzzzz Zzzzzzzz), Zzzzzz Ausrüstung Zzzzzzzzzz
(Zzzzzzz Zzzz Zzzzzz Zzzzzzzz)
Zzzzzzzz Zzzz Zzzzzzzzz
Zzzzzzzz, Zzzzzzzzz und Zzzzzzzzzz
Zzzzzzzzzz Zzzzzzzz Zzzzzzzzzzzzz Zzzzz Zzzzzzz Zzzzzzzzz Zzzzzz-
Zzzz Zzzzzzzzzzzz Zzzzzzzzz (Zz. 000) Zzzz Zzzzzz Zzzzzzzzzzz, Zzzz
Zzzzzzzzzz Zzzzzz Zzzzzzzzzz Zzzzzzz Zzzzzzzz. Zzzzzz Zzzzzzzzz Zzzzz
Zzzzzzzz (Zzzzzzzzz Zzzzzzzz Zzzz Zzzzzzzzzzzzzz), Zzzzzzzzzz (Zzzzz
Zzzzzzzzzzzzz Zzzzzzzzzzz, Zzzzz Zzzz Zzzzzzzzzzzz Zzzzz) und Zzzz
Zzzzzzzz Zzzzzzzzz.
Probe: Zzzzzzzzz Zzzzzzzzzz-Probe, Zzzz Zzzzzzzzzzzzzz Zzzzzzzz 00
Zzzzzzzzzz Zzzz Zzzzzzzzz Zzzzzz Zzzzzz Zzzz Zzzzz Zzzzzzzzz Zzzzzzzz,
Zzzzzzzzz Zzzzz Zzzzzzzzzz, Zzz Zzzzzz Zzzz Zzzzzzzzzzz Zzzzzzz. Zzz
Zzzzzzz Zzzzzz Zzzzzz, Zzzzzzzz Zzzzzzzz Zzzzzzzz Zzzzzzz Zzzzz, Zzzz
Zzzzzzzzzzzzzz Zzzzzzzzzzz Zzzzz Zzzzzz Zzz Zzzz Zzzzzzzzz Zzzzzz Zzzzzz
Zzzz Zzzzz Zzzzzzzzz.
Zzzz Zzzzzzzz Zzzz Zzzzzzzzzz Zzzzz Zzzz Zzzzz Zzzzzzzz Probe Zzzz Zzzz
Zzzzzzzzz Zzzzzzz, Zzzz Zzzzz Zzzz Zzzz Zzzzzzz Zzzzzzzz Zzzzz. Zzzz Zzzz-
Zzz Zzzzzz 0 Zzzzzzzz Zzz Zzzzzzzzz. Zzzzzzz Zzzz Zzzzzzzzzzzzzzzz
Zzzzzzzzzz Zzzzzz, Zzzzz Zzzz Zzzzzzzz Zzzz Zzzz Zzzzz Zzzz Zzzzzzzzzzzz-
Zzzzzz Zzzzzzzzzzz.
Zzzz Zzzz Zzzzzzzz Zzzzz Zzzzzzzzzz Zzzzzzzz (Zzzzzz Zz. 000), Zzzzz
Zzzzzzzzz Zzzz Zzzzzzzz Zzzzzzzzzz Zzzzzzz, Zzzzzz Zzzz Zzzzzzzz Zzzzzzzz
Zzzzzzz Zzzzz.
Auswirkungen
Verheerend: Zzzz Zzzzzzz Zzzzzzzzzzz Zzzz Zzzzzzz Zzzzzzzzzzzzz und
Zzzz Zzzzzzzz Zzzzzzzzzz. Zzzzzzzzzzz Zzzzzzz Zzzz Zzzzzzzz 0 Zzzzzzzzz-
Zzzzzzz und 0 Zzzzzzzzz Zzzz Zzzzzzzzzz Zzzzzzzzzzzz.
Misslungen: Zzzz Zzzzzzzz Zzzzzzzzzz und Zzzz Zzzzzzzz Zzzzzzz 0
Zzzzzzzzzzzzzz Zzzz Zzzzzzzzzz Zzzzzzzzzzzz.
Zzzzzz misslungen: Zzzz Zzzzzzzzzzz Zzzzzzz Zzzzzz Zzzzzzz und
Zzzz Zzzzzzzz Zzzzzzzzzz. Zzzz Probe Zzzzzz Zzzzz Zzzzzz Zzzz Zzzz Zzzzzzz
Zzzz Zzzzzzzzzz Proben Zzzz Zzzz.
Gelungen: Zzzz Zzzzzzzz Zzzzzz Zzz 0 Zzzzzz (+0 Zzzz 0 Zzzzzzzzzzz-
Zzz). Zzzzzzz Zzzz Zzzzzzzz Zzzzz Zzzzzzzz Zzzzzzzzz Zzzzzz Zzzzzzzzzzz-
Zzz Zzzzz Zzzzzzz Zzzzzz, Zzzzzzz Zzz 0 Zzzzzzzzzzzz Zzz Zzzz Zzzzzzzzzz
Zzzzzzzzzzzzzzzzz.
Herausragend: Zzzzzzzzzzz Zzzz Zzzzzzz Zzzzzz gelungenen Zzzzzzzz
Zzzzzzz Zzzz Zzzzzzzz Zzzz Zzzz Zzzzzzz 0W0 Zzzzzzzzzzzzz Zzzzzzz.
Ausrüstung und Umstände
Zzzzzzzzzz Zzzzzzzz Zzzzzzzzz (Zzzzzzz Zzzzzzzz), Zzzzzzzzzzzzz Zzz-
Zzzzzzz (Zzzzzzz Zzzz Zzzzzz Zzzzzzzz), Zzzzzz Ausrüstung Zzzzzzzzzz
(Zzzzzzz Zzzz Zzzzzz Zzzzzzzz)",
        @"
Landschaft: Zzzzzzzzzz Zzzzzzzz Zzzz Zzzzzzzzzzzzzzz Zzzz Zzzz-
Zzzzzzz, Zzzzzzzzz Zzzzzz, Zzzzzzzzzzzz Zzzzzzzzzzzz
Klima: Zzzzzzzzzzz; Zzzzzz Zzzzzzz und Zzzzzzzzzzz, Zzzzzz
Zzzzzzz
Flora und Fauna: Zzzz Zzzzzzz Zzzzzzzzzz und -Zzzzzzzzz Zzz
Zzzzzzzzzzzzzzz Zzzzzzzz
Handel und Verkehr: Zzzzz Zzzz Zzzzzzzzzzzz Handelsmächte
Zzzz Zzzzz Zzzzzz Zzzz Zzzzzzz Handels- und Zzzzzzzzzzzzzz;
Zzzzzzzzzzzz Zzzz Zzzzzzzzzzz, Zzzzzzzzzz Zzzzzzzzzzz Zzzz
Zzzzzzzzzz, Zzzzzzz Zzzz Zzzz Zzzzzzzzzzzzzz Zzzzzz
Bevölkerung: 0,0 Zzzzzzzzzz; Zzz Zzzzz Zzzzzz Zzzzzz Zzzzzzzzzzz
Zzzzzzzz Zzzz Zzzzzzzzzzzzzz Zzzzzzz, Zzzzzzz Zzzzzzzz Zzzz-
Zzzzzz Zzzz Zzzzzzzzz Zzzzzzz
Städte und Dörfer: Zzzzzzzzzz (00.000
Einwohner), Zzzzzzzzz (00.000),
Zzzzzzzzz (00.000), Zzzzz (00.000),
Zzzzzzzzz (000.000), Zzzzzzz (00.000),
Zzzzzzzzz (00.000), Zzzzzzz (00.000),
Zzzzzzzzzzz (00.000), Zzzzz Zzzzzz Zzzzz-
Zzz Dörfer Zzz Zzzzzzz Zzzz Zzzzzzzzzzz.
Herrschaft: Zzzzzzzzz Zzzzzzzzzzzzz, Zzzz
Zzzzz Zzz Zzzz Zzzz Zzzzzzzzzzzz Zzzzzzzzzzz
Wappen: Zzzzzzzzzzzzz Zzzzzzzzz Zzzzzz Zzzz Zzzzz
Religion: Zzzzz Zzzzzzzzz; Zzzzzzzzzz Zzzzzzzzzz Zzzz Zzzzzz
Zzzz Zzzzzzzzzzz Zzzz Zzzzzzzzzzz Zzzz Zzzzzzzzzzzz
Allgemeine Zzzzzzzzz: Zzzzzzzzzzzzz und Zzzzzzzzzzz, Zzzzz
Zzzzzzzz Zzzzzzzzz Zzzzzzzz
",
    ];

    #endregion
}