using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReformatOnCopy;

[Flags]
public enum ReformatRegexMode
{
    NoneCompiled = 0,
    BogusCompiled = 1,
    HeadingsCompiled = 2,
    BothCompiled = BogusCompiled | HeadingsCompiled
}

[Flags]
public enum ReformatPasses
{
    None = 0,
    BogusLineBreaks = 1,
    DetectHeadings = 2,
    All = BogusLineBreaks | DetectHeadings
}

public class Reformatter
{
    private readonly ReformatPasses passes;
    // Zs = [\t\p{Z}-[\p{Zl}]];
    // Line break according to Unicode standard = [\r\n\f\u0085\u2028\u2029]

    private readonly Regex bogusLineBreakDetector;

    private readonly Regex headings;

    public Reformatter(ReformatPasses passes = ReformatPasses.All, ReformatRegexMode mode = ReformatRegexMode.BogusCompiled)
    {
        this.passes = passes;
        bogusLineBreakDetector = new(
            @"(?<before>[.!?:])?(?<dash>-)?(?<space>[\t\p{Z}-[\p{Zl}]]+)?(?<break>(\r*?[\r\n\f\u0085\u2028\u2029][\t\p{Z}-[\p{Zl}]]*)+)(?<after>\p{Lu}\w+([\t\p{Z}-[\p{Zl}]]*\w+){0,9}:)?",
            compiledIf(ReformatRegexMode.BogusCompiled, mode, RegexOptions.Multiline | RegexOptions.CultureInvariant)
        );
        headings = new(
            @"^(?<title1>Beispiele?|Probe|(Auswirkungen[\t\p{Z}-[\p{Zl}]]*)?Verheerend|Misslungen|Knapp misslungen|Gelungen|Herausragend|Landschaft|Klima|Flora und Fauna|Handel und Verkehr|Bevölkerung|Städte und Dörfer|Herrschaft|Wappen|Provinzen|Religion|Wer ist wer \w+([\t\p{Z}-[\p{Zl}]]+\w+){1,5}|Bemerkenswertes|Allgemeine Stimmung|Einwohner|Besonderheiten):[\t\p{Z}-[\p{Zl}]]*|(?<title2>Ausrüstung und Umstände)[\t\p{Z}-[\p{Zl}]]*",
            compiledIf(ReformatRegexMode.HeadingsCompiled, mode, RegexOptions.Multiline | RegexOptions.CultureInvariant)
        );
    }

    public string Reformat(string input)
    {
        var breaksRemoved = enabled(ReformatPasses.BogusLineBreaks) ? bogusLineBreakDetector
            .Replace(input, m =>
                    m.Groups["dash"].Length > 0
                        ? m.Result("${before}${after}") // remove break, dash and space
                        : m.Groups["before"].Length > 0 || m.Groups["after"].Length > 0 ||
                        hasMoreThanOneLineBreak(m.Groups["break"].ValueSpan)
                            ? m.Result("${before}\n\n${after}") // normalize the break, drop the space
                            : m.Result("${before} ${after}") // drop the break, normalize the space (no dash)
            ) : input;
        //Console.WriteLine($"{breaksRemoved.Count(c => c == '\n')}]{breaksRemoved}");
        return
            enabled(ReformatPasses.DetectHeadings) ?
            headings.Replace(
                breaksRemoved,
                m =>
                {
                    var t = oneOf(m.Groups["title1"], m.Groups["title2"]);
                    return t switch
                    {
                        "Auswirkungen Verheerend" => "## Auswirkungen\n\n### Verheerend\n\n",
                        "Probe" or "Auswirkungen" or "Ausrüstung und Umstände" => $"## {t}\n\n",
                        "Verheerend" or "Misslungen" or "Knapp misslungen" or "Gelungen" or "Herausragend" =>
                            $"### {t}\n\n",
                        "Beispiel" or "Beispiele" => $"**{t}**\n", // no paragraph break
                        // Region
                        "Landschaft" or "Klima" or "Flora und Fauna" or "Handel und Verkehr" or "Bevölkerung"
                            or "Städte und Dörfer" or "Herrschaft" or "Wappen" or "Provinzen" or "Religion"
                            or "Bemerkenswertes" or "Allgemeine Stimmung" or "Einwohner" or "Besonderheiten" =>
                            $"## {t}\n\n",
                        var x when x.StartsWith("Wer ist wer") => $"## {t}\n\n",
                        // Fallback
                        _ => m.Result("**$1**: ")
                    };
                }) : breaksRemoved;
    }
    
    public string ReformatEx(string input)
    {
        var breaksRemoved = enabled(ReformatPasses.BogusLineBreaks) ? bogusLineBreakDetector
            .Replace(input, m => m.Groups["dash"].Length > 0
                    ? $"{m.Groups["before"].ValueSpan}{m.Groups["after"].ValueSpan}"
                    : m.Groups["before"].Length > 0 || m.Groups["after"].Length > 0 ||
                    hasMoreThanOneLineBreak(m.Groups["break"].ValueSpan)
                        ? $"{m.Groups["before"].ValueSpan}\n\n${m.Groups["after"].ValueSpan}" // normalize the break, drop the space
                        : $"{m.Groups["before"].ValueSpan} ${m.Groups["after"].ValueSpan}" // drop the break, normalize the space (no dash)
            ) : input;
        //Console.WriteLine($"{breaksRemoved.Count(c => c == '\n')}]{breaksRemoved}");
        return
            enabled(ReformatPasses.DetectHeadings) ?
            headings.Replace(
                breaksRemoved,
                m =>
                {
                    var t = oneOf(m.Groups["title1"], m.Groups["title2"]);
                    return t switch
                    {
                        "Auswirkungen Verheerend" => "## Auswirkungen\n\n### Verheerend\n\n",
                        "Probe" or "Auswirkungen" or "Ausrüstung und Umstände" => $"## {t}\n\n",
                        "Verheerend" or "Misslungen" or "Knapp misslungen" or "Gelungen" or "Herausragend" =>
                            $"### {t}\n\n",
                        "Beispiel" or "Beispiele" => $"**{t}**\n", // no paragraph break
                        // Region
                        "Landschaft" or "Klima" or "Flora und Fauna" or "Handel und Verkehr" or "Bevölkerung"
                            or "Städte und Dörfer" or "Herrschaft" or "Wappen" or "Provinzen" or "Religion"
                            or "Bemerkenswertes" or "Allgemeine Stimmung" or "Einwohner" or "Besonderheiten" =>
                            $"## {t}\n\n",
                        var x when x.StartsWith("Wer ist wer") => $"## {t}\n\n",
                        // Fallback
                        _ => m.Result("**$1**: ")
                    };
                }) : breaksRemoved;
    }


    private bool enabled(ReformatPasses pass)
    {
        return (passes & pass) == pass;
    }

    private static bool hasMoreThanOneLineBreak(ReadOnlySpan<char> group)
    {
        var seenBreak = false;
        foreach (var c in group)
        {
            if (c == '\n')
            {
                if (seenBreak)
                {
                    return true;
                }

                seenBreak = true;
            }
        }

        return false;
    }

    private static string oneOf(params Capture[] captures)
    {
        return captures.First(c => c.Length > 0).Value;
    }
    

    private static RegexOptions compiledIf(
        ReformatRegexMode condition,
        ReformatRegexMode mode,
        RegexOptions baseOptions
    )
    {
        if ((mode & condition) == condition)
        {
            return baseOptions | RegexOptions.Compiled;
        }
        else
        {
            return baseOptions;
        }
    }
}