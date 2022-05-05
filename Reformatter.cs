using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ReformatOnCopy;

[Flags]
public enum ReformatRegexMode
{
    NoneCompiled = 0,
    AggregateHeadingsCompiled = 2,
    IndividualHeadingsCompiled = 4,
    AllCompiled = AggregateHeadingsCompiled | IndividualHeadingsCompiled
}

[Flags]
public enum ReformatPasses
{
    None = 0,
    UnnecessaryLineBreaks = 1,
    DetectHeadings = 2,
    All = UnnecessaryLineBreaks | DetectHeadings
}

public record HeadingPattern([RegexPattern] string Pattern, string Replacement, bool ExpectColon = true);

public class Reformatter
{
    [RegexPattern]
    private const string SpacePattern = @"[\t\p{Z}-[\p{Zl}]]";

    private const string HeadingGroupName = "_heading";

    private readonly ReformatPasses passes;

    private readonly Regex? aggregateHeadingPattern;
    private readonly ImmutableList<(HeadingPattern Pattern, Regex CachedRegex)> headingPatterns;

    [DllImport("libreformat", EntryPoint = "reformat", ExactSpelling = true)]
    static extern unsafe nint reformat(byte* inputBuf, int inputLen, byte* outputBuf, int outputCapacity);

    public Reformatter(
        IEnumerable<HeadingPattern> headingPatterns,
        ReformatPasses passes = ReformatPasses.All,
        ReformatRegexMode mode = ReformatRegexMode.AllCompiled
    )
    {
        this.passes = passes;

        this.headingPatterns = headingPatterns
            .Select(h => (
                h with {Replacement = Regex.Replace(h.Replacement,@"(?!<\$)\$0", "${" + HeadingGroupName + "}")}, 
                new Regex($@"^(?<{HeadingGroupName}>{h.Pattern})$", RegexOptions.CultureInvariant)))
            .ToImmutableList();
        if (AssembleHeadingPattern(this.headingPatterns.Select(x => x.Pattern)) is { } pattern)
        {
            aggregateHeadingPattern = new(pattern,
                compiledIf(ReformatRegexMode.AggregateHeadingsCompiled, mode,
                    RegexOptions.Multiline | RegexOptions.CultureInvariant)
            );
        }
        else
        {
            this.aggregateHeadingPattern = null;
        }
    }

    internal static string? AssembleHeadingPattern(IEnumerable<HeadingPattern> headingPatterns)
    {
        var result = "^" + string.Join('|', headingPatterns.GroupBy(p => p.ExpectColon)
            .OrderByDescending(g => g.Key) // for predictability (colon first)
            .Select(g =>
                Regex.Replace(
                    $@"(?<title{(g.Key ? "1" : "2")}>{string.Join('|', g.Select(h => h.Pattern))}){(g.Key ? ":" : "")}\s*",
                    @"(?!<\\)\\s",
                    SpacePattern
                )
            ));
        return result == "^" ? null : result;
    }

    public string Reformat(string input)
    {
        var breaksRemoved = enabled(ReformatPasses.UnnecessaryLineBreaks) ? removeLineBreaksViaRust(input) : input;
        //Console.WriteLine($"{breaksRemoved.Count(c => c == '\n')}]{breaksRemoved}");
        return
            aggregateHeadingPattern != null && enabled(ReformatPasses.DetectHeadings) 
                ? reformatHeadings(breaksRemoved, aggregateHeadingPattern)
                : breaksRemoved;
    }

    private string reformatHeadings(string input, Regex aggregatePattern)
    {
        return aggregatePattern.Replace(
            input,
            m =>
            {
                var content = oneOf(m.Groups["title1"], m.Groups["title2"]);
                var (heading, match) = headingPatterns
                    .Select(p => (p.Pattern, Match: p.CachedRegex.Match(content)))
                    .FirstOrDefault(p => p.Match.Success);
                if (heading == null)
                {
                    Console.WriteLine($"WARNING: no heading found for {content}");
                    return m.Value;
                }
                else
                {
                    return match.Result(heading.Replacement);
                }
            });
    }

    private readonly ArrayPool<byte> pool = ArrayPool<byte>.Create(32 * 1024 * 1024, 50);
    private readonly Encoder encoder = Encoding.UTF8.GetEncoder();

    private string removeLineBreaksViaRust(string input)
    {
        var inputBuf = pool.Rent(Encoding.UTF8.GetMaxByteCount(input.Length));
        var effectiveUtf8ByteCount = encoder.GetBytes(input.AsSpan(), inputBuf.AsSpan(), true);
        // Output can be at most 1.5 times the input size (and only in terms of ASCII characters)
        var outputBuf = pool.Rent(effectiveUtf8ByteCount + effectiveUtf8ByteCount / 2 +
            (effectiveUtf8ByteCount % 2 == 0 ? 0 : 1));

        string output;
        unsafe
        {
            fixed (byte* inputPtr = inputBuf)
            fixed (byte* outputPtr = outputBuf)
            {
                var result = reformat(inputPtr, effectiveUtf8ByteCount, outputPtr, outputBuf.Length);
                if (result < 0)
                {
                    throw new("Error during line break detection. Error code: " + result);
                }

                output = new((sbyte*)outputPtr, 0, (int)result, Encoding.UTF8);
            }
        }

        pool.Return(inputBuf);
        pool.Return(outputBuf);
        return output;
    }

    private bool enabled(ReformatPasses pass)
    {
        return (passes & pass) == pass;
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