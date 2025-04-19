using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace ReformatOnCopy;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public static class CityOfMist
{
    public static readonly IImmutableList<HeadingPattern> Headings = ImmutableArray.Create<HeadingPattern>(
        new("KONZEPT", "Konzept\n"),
        new("CONCEPT", "Concept\n"),
        new("FRAGEN ZU DEN STÄRKEN", "Fragen zu den Stärken\n"),
        new("POWER TAG QUESTIONS", "Power tag questions\n"),
        new("FRAGEN ZU DEN SCHWÄCHEN", "Fragen zu den Schwächen\n"),
        new("WEAKNESS TAG QUESTIONS", "weakness tag questions\n"),
        new("ZUSÄTZLICHE STÄRKEN UND SCHWÄCHEN", "Zusätzliche Stärken und Schwächen\n"),
        new("EXTRA TAGS", "Extra tags\n"),
        new("MYSTERIUM", "Mysterium\n"),
        new("MYSTERY", "Mystery\n"),
        new("NAME", "Name\n"),
        new("TITLE", "Title\n"),
        new("BEZIEHUNGEN INNERHALB DES TEAMS", "Beziehungen innerhalb der Gruppe\n"),
        new("CREW RELATIONSHIPS", "Crew relationships\n"),
        new(@"VERBESSERUNGEN DES (\w+)-THEMAS","Verbesserung des $1-Themas\n"),
        new(@"(\w+) THEME IMPROVEMENTS","$1 theme improvements\n")
    );
}