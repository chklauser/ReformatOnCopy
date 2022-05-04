using System;
using System.Buffers;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ReformatOnCopy;

[Flags]
public enum ReformatRegexMode
{
    NoneCompiled = 0,
    HeadingsCompiled = 2,
    AllCompiled = HeadingsCompiled
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

    private readonly Regex headings;

    [DllImport("libreformat", EntryPoint = "reformat", ExactSpelling = true)]
    static extern unsafe nint reformat(byte* inputBuf, int inputLen, byte* outputBuf, int outputCapacity);
    
    public Reformatter(ReformatPasses passes = ReformatPasses.All, ReformatRegexMode mode = ReformatRegexMode.HeadingsCompiled)
    {
        this.passes = passes;
        headings = new(
            @"^(?<title1>Beispiele?|Probe|(Auswirkungen[\t\p{Z}-[\p{Zl}]]*)?Verheerend|Misslungen|Knapp misslungen|Gelungen|Herausragend|Landschaft|Klima|Flora und Fauna|Handel und Verkehr|Bevölkerung|Städte und Dörfer|Herrschaft|Wappen|Provinzen|Religion|Wer ist wer \w+([\t\p{Z}-[\p{Zl}]]+\w+){1,5}|Bemerkenswertes|Allgemeine Stimmung|Einwohner|Besonderheiten):[\t\p{Z}-[\p{Zl}]]*|(?<title2>Ausrüstung und Umstände)[\t\p{Z}-[\p{Zl}]]*",
            compiledIf(ReformatRegexMode.HeadingsCompiled, mode, RegexOptions.Multiline | RegexOptions.CultureInvariant)
        );
    }

    public string Reformat(string input)
    {
        var breaksRemoved = enabled(ReformatPasses.BogusLineBreaks) ? removeLineBreaksViaRust(input) : input;
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

    private readonly ArrayPool<byte> pool = ArrayPool<byte>.Create(32*1024*1024, 50);
    private readonly Encoder encoder = Encoding.UTF8.GetEncoder();

    private string removeLineBreaksViaRust(string input)
    {
        var inputBuf = pool.Rent(Encoding.UTF8.GetMaxByteCount(input.Length));
        var effectiveUtf8ByteCount = encoder.GetBytes(input.AsSpan(), inputBuf.AsSpan(), true);
        // Output can be at most 1.5 times the input size (and only in terms of ASCII characters)
        var outputBuf = pool.Rent(effectiveUtf8ByteCount + effectiveUtf8ByteCount/2 + (effectiveUtf8ByteCount%2 == 0 ? 0 : 1));

        string output;
        unsafe
        {
            fixed (byte* inputPtr = inputBuf)
            fixed (byte* outputPtr = outputBuf)
            {
                var result = reformat(inputPtr, effectiveUtf8ByteCount, outputPtr, outputBuf.Length);
                if(result < 0)
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