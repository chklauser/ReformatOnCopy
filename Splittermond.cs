using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace ReformatOnCopy;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public static class Splittermond
{
    private const string H1 = "# $0\n";
    private const string H2 = "## $0\n\n";
    private const string H3 = "### $0\n\n";
    private const string PrefixColon = "**$0**: ";

    public static readonly IImmutableList<HeadingPattern> Headings = ImmutableList.Create<HeadingPattern>(
        new("Beispiele?", "**$0**\n"), // no paragraph break
        new("Probe", H2),
        new(@"Auswirkungen\s+Verheerend", "## Auswirkungen\n\n### Verheerend\n\n"),
        new(@"Verheerend", H3),
        new(@"Misslungen", H3),
        new(@"Knapp misslungen", H3),
        new(@"Gelungen", H3),
        new(@"Herausragend", H3),
        new(@"Landschaft", H2),
        new(@"Klima", H2),
        new(@"Flora und Fauna", H2),
        new(@"Handel und Verkehr", H2),
        new(@"Bevölkerung", H2),
        new(@"Städte und Dörfer", H2),
        new(@"Herrschaft", H2),
        new(@"Wappen", H2),
        new(@"Provinzen", H2),
        new(@"Religion", H2),
        new(@"Bemerkenswertes", H2),
        new(@"Besondere Orte", H2),
        new(@"Bedeutende Städte", H2),
        new(@"Nachbarn", H2),
        new(@"Allgemeine Stimmung", H2),
        new(@"Einwohner", H2),
        new(@"Besonderheiten", H2),
        new(@"Wer ist wer \w+([\t\p{Z}-[\p{Zl}]]+\w+){1,5}", H2),
        new(@"Ausrüstung und Umstände", H2, ExpectColon: false),
        // Orden und Banden
        new(@"Hintergrund", H1, false),
        new(@"Motivation", H1, false),
        new(@"Hierarchie und Struktur", H1, false),
        new(@"Ressourcen und Mittel", H1, false),
        new(@"Vorgehen", H1, false),
        new(@"Verbündete und Feinde", H1, false),
        new(@"Mitglieder", H2, false),
        new(@"Ziele", H2, false)
    );
}