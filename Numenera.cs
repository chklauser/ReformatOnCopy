using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace ReformatOnCopy;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public static class Numenera
{
    const string H3 = "### $0\n\n";

    public static readonly IImmutableList<HeadingPattern> Headings = ImmutableArray.Create<HeadingPattern>(
        new("Level", H3),
        new("Usable", H3),
        new("Effect", H3),
        new("Wearable", H3),
        new("Form", H3),
        new("Internal", H3),
        new("Depletion", H3),
        new("Form", H3)
    );
}