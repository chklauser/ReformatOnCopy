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

/// <summary>
/// Indicates, which regular expressions should be compiled (using <see cref="RegexOptions.Compiled"/>).
/// </summary>
[Flags]
public enum ReformatRegexMode
{
    NoneCompiled = 0,
    /// <summary>
    /// Compile the regular expression that detects whether the current line starts with any of the headings.
    /// </summary>
    AggregateHeadingsCompiled = 2,
    /// <summary>
    /// Compile the individual heading regular expressions that get used to detect _which_ heading the current line starts with.
    /// (After the aggregate expression has already detected _some_ heading)
    /// </summary>
    IndividualHeadingsCompiled = 4,
    AllCompiled = AggregateHeadingsCompiled | IndividualHeadingsCompiled
}

/// <summary>
/// Indicates which transformation passes should be applied to the input.
/// </summary>
[Flags]
public enum ReformatPasses
{
    None = 0,
    /// <summary>
    /// Removes unnecessary line breaks.
    /// </summary>
    UnnecessaryLineBreaks = 1,
    /// <summary>
    /// Detects headings and formats them for Markdown. Uses <see cref="HeadingPattern"/>s.
    /// </summary>
    DetectHeadings = 2,
    All = UnnecessaryLineBreaks | DetectHeadings
}

/// <summary>
/// A heading pattern is matched against the beginning of each line (after removing unnecessary line breaks).
/// </summary>
/// <param name="Pattern">The regular expression that detects the heading. Must not include <c>^</c> or <c>$</c>.</param>
/// <param name="Replacement">The replacement pattern for the heading. Use <c>$0</c> to refer to the matched heading. Use <c>${named}</c> capture groups instead of indexed capture groups beyond <c>$0</c>.</param>
/// <param name="ExpectColon">Whether to expect <c>":\s"</c> after the heading pattern. Defaults to <c>true</c>.</param>
public record HeadingPattern([RegexPattern] string Pattern, string Replacement, bool ExpectColon = true);

public class Reformatter
{
    [RegexPattern]
    private const string SpacePattern = @"[\t\p{Z}-[\p{Zl}]]";

    private const string HeadingGroupName = "_heading";

    private readonly ReformatPasses passes;

    private readonly Regex? aggregateHeadingPattern;
    private readonly ImmutableList<(HeadingPattern Pattern, Regex CachedRegex)> headingPatterns;

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
            aggregateHeadingPattern = null;
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
        // Encode input as UTF-8 into the input buffer 
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
    
    /// <summary>
    /// Rust implementation of the line break  removal. The regular expression used to detect line breaks is not very
    /// fast using the .NET regular expression implementation. The rust regex crate is roughly 1000× faster.
    /// </summary>
    /// <param name="inputBuf">Pointer to a buffer containing the UTF-8 encoded input string of length <paramref name="inputLen"/>. No terminating zero byte required. The allocated buffer may of course be larger than <paramref name="inputLen"/>.</param>
    /// <param name="inputLen">The length of the UTF-8 encoded input string, in bytes.</param>
    /// <param name="outputBuf">Pointer to the buffer that the function will write its output to. Must not alias <paramref name="inputBuf"/>! Must be at least 1.5 as large as <paramref name="inputLen"/>. Contents of the output buffer are never read.</param>
    /// <param name="outputCapacity">The length of the <paramref name="outputBuf"/>.</param>
    /// <returns>If >= 0, the number of UTF-8 encoded bytes written to the <paramref name="outputBuf"/>. If &lt; 0, an error has occurred</returns>
    [DllImport("libreformat", EntryPoint = "reformat", ExactSpelling = true)]
    static extern unsafe nint reformat(byte* inputBuf, int inputLen, byte* outputBuf, int outputCapacity);
    
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