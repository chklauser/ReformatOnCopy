using System.Text.RegularExpressions;

namespace DefaultNamespace;

public class Reformatter
{
    private static readonly Regex _pattern = new(
        @"(?<![.])-?\s*(\r?\n|\r)", 
        RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled
    );
    
    public string Reformat(string input)
    {
        return _pattern.Replace(input, "");
    }
}