using System;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Filters;
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

    public enum Texts
    {
        MertaliaProsePage = 0,
        FertigkeitHeilen = 1,
        MertaliaShortListing = 2,
    }
        
    [ParamsAllValues]
    public Texts Text { get; set; }

    private Reformatter reformatterHeadingInterpreted = null!;
    private Reformatter reformatterHeadingCompiled = null!;

    [GlobalSetup]
    public void Setup()
    {
        reformatterHeadingInterpreted = new(ReformatPasses.All, ReformatRegexMode.BogusCompiled);
        reformatterHeadingCompiled = new(ReformatPasses.All,ReformatRegexMode.BogusCompiled | ReformatRegexMode.HeadingsCompiled);
    }

    [Benchmark(Baseline = true)]
    public string ReformatH0()
    {
        return reformatterHeadingInterpreted.Reformat(_texts[(int)Text]);
    }

    [Benchmark]
    public string ReformatHc()
    {
        return reformatterHeadingCompiled.Reformat(_texts[(int)Text]);
    }

    [Benchmark]
    public string ReformatH0Ex()
    {
        return reformatterHeadingInterpreted.ReformatEx(_texts[(int)Text]);
    }

    [Benchmark]
    public string ReformatHcEx()
    {
        return reformatterHeadingCompiled.ReformatEx(_texts[(int)Text]);
    }

    #region Texts

    private static readonly string[] _texts =
    {
        @"
Klima
Das angenehme Seeklima sorgt nicht nur für regelmäßige Ern-
ten, sondern auch dafür, dass man von nördlich der Vogelber-
ge neidisch auf die „elysischen“ Verhältnisse blickt: Die Winter
sind mild und regenreich, bisweilen aber stürmisch, die Sommer
warm, aber nur im Hochsommer in der Aylischen Ebene drü-
ckend, und ein regelmäßiger Südwestwind sorgt selbst dann für
Abkühlung.
Schnee gibt es im Winter nur in den Gebirgen – am Südhang der
Vogelberge und am Westhang der Ventellen dafür aber reichlich.
Im Frühjahr verwandeln sich dann viele Bäche in reißende Ge-
wässer, sodass einige Gebirgsorte für Wochen von der Außenwelt
abgeschnitten sind.
Flora und Fauna
Typische Bäume Mertalias sind Zedern, Zypressen, Eichen und
Olivenbäume, aber viele ursprüngliche Bäume sind dem Holzhun-
ger der Flotten zum Opfer gefallen. Echte Urwälder gibt es fast nur
noch in schwer zugänglichen Gebirgstälern, dann meist Jagdreser-
vate der städtischen Magnaten. Wo die alten Wälder gefallen sind,
haben sie Heiden, Hochmooren und strauchreichen Karstflächen
mit gelegentlichen Birken- und Kieferwäldern Platz gemacht.
In der Umgebung der Metropolen ist das Land kultiviert, mit
Hecken oder Zypressen gesäumte Felder und Weiden bedecken
das Land. Weiter in den Bergen finden sich Äcker nur in direk-
ter Nähe der Bergdörfer, aber die ein oder andere Lichtung hat
Hochalmen Platz gemacht, auf denen Ziegen und Gebirgsrinder
grasen. Wo noch alte Eichenwälder stehen, werden die Schweine
des Dorfs geweidet. Wichtigste Getreide Mertalias sind Weizen
und Reis, daneben werden Hülsenfrüchte und Kürbisse, Obst
und Wein angebaut.
Neben den genannten Nutztieren werden um Aylantha herum
Schafe, Milchvieh und edle Reitpferde gezüchtet (andernorts
sind eher Esel und Maultiere im Gebrauch), Eisenbrann ist
bekannt für seine Kampf- und Jagdhunde, Talaberis für seine
„Gnomenrinder“ – eine große, schmackhafte Hauskaninchenart.
Die Gewässer um Mertalia sind fischreich, Sardinen, Thunfisch
und Meeresfrüchte haben einen großen Anteil an der Nahrungs-
versorgung.
Größere wilde Tiere sind selten, bisweilen kann man in den alten
Wäldern Wildschweinen oder Bären begegnen, aber schon die
meisten Hirsche und Rehe sind bereits ausgewilderte Exemplare.
Greifvögel und Kleinräuber können sich jedoch stellenweise zu
einer echten Plage entwickeln.
Außer einigen Schlangenarten (wie der kleinen Gürtelnat-
ter), der gefürchteten Roten Hornisse und der Hundsspinne
gibt es keine giftigen Tiere. Mythische und magische Bestien
sind ebenso selten, was damit zusammenhängen mag, dass
die wenigen Übergänge in die Feenwelten meist am oder im
Wasser liegen.
Magnaten und Bettler – die mertalische Gesellschaft
Die meisten Bewohner leben in den Metropolen und den kleine-
ren, tributpflichtigen Städten. Auch wenn natürlich viele Bauern
in den Dörfern für Fleisch und Korn sorgen, ist die urbane Le-
bensweise bestimmend für die mertalische Gesellschaft.
An ihrer Spitze stehen die Magnaten, wohlhabende Handelsherren,
mächtige Großgrundbesitzer oder Gildenvorsteher. Es gibt keine
formellen Bedingungen, um zu dieser Klasse zu gehören, vielmehr
muss man von den anderen Magnaten als ihresgleichen anerkannt
werden – was zugegebenermaßen nur selten geschieht.
An zweiter Stelle steht die breite Schicht der Bürger: Wer die
jährliche Summe zum Erwerb eines Bürgerbriefes aufbringen
kann, erhält damit Rechte, die ihn vor der völligen Willkür der
Machthaber schützen sollen. In der Praxis sind die meisten Bür-
ger einem Magnaten als Klienten verpflichtet und von seinem
Wohlwollen abhängig.
Ein Großteil der Einwohner Mertalias kann sich den Bürger-
brief jedoch nicht leisten und gilt damit als Nichtbürger: Beinahe
rechtlos schuften sie als Arbeiter in den Betrieben der Magnaten,
erledigen niedere Dienste für die Bürgerschaft oder sind auf das
Betteln angewiesen.
Doch es geht noch schlimmer: Zwar gibt es im Städtebund keine
Leibeigenschaft oder Sklaverei, doch das Los verurteilter Verbre-
cher und säumiger Schuldner unterscheidet sich nicht sehr da-
von. Als Schuldknechte werden sie in ganz Mertalia in Steinbrü-
chen und Werften, als Wald- und Feldarbeiter eingesetzt.
Die Gesellschaftsstruktur der Metropolen und Städte spiegelt
sich auf dem Land und in den Dörfern in kleinerem Maßstab
wieder: So mancher Großbauer spielt sich als Magnat auf, Mägde
und Knechte werden wie Nichtbürger behandelt, der Rest ordnet
sich irgendwo dazwischen ein.
Mentalität
Bereits zu Zeiten des Königreiches zählten die Taten jedes
Einzelnen mehr als seine Herkunft und sein Geblüt. Der feste
Glaube, hier durch eigene Leistung reich werden zu können,
zog in den folgenden Jahrhunderten viele Einwanderer und
Flüchtlinge an, sodass die Einwohnerschaft der Halbinsel heute
ein regelrechtes Völkergemisch bildet. Dementsprechend zeich-
nen sich die Mertalier durch Weltoffenheit und oftmals eine
gute Menschenkenntnis aus. Darüber hinaus gelten sie als le-
bensfroh, genussfreudig und neigen zu einer gewissen Schwatz-
haftigkeit.
Entgegen verbreiteter Vorurteile ist nicht jeder ein unehrenhafter
Gauner und Halsabschneider, doch sind die meisten Einwohner
des Städtebunds geschäftstüchtig und auf ihren gesunden Vorteil
bedacht.
Glaube und Magie
Es gibt keine Staatsreligion oder auch nur ein festes Pantheon:
Es wird der Gott angebetet, der in der jeweiligen Situation am
besten hilft. So stehen in den mertalischen Metropolen Tempel
so ziemlich jedes Gottes der Binnenmeerregion. Hinzu kom-
men lokale Gottheiten, die als Schutzherr der jeweiligen Stadt
gelten.
Einzige Konstante des hiesigen Glaubens ist Ulmon der Bun-
desherr, Schutzgott des Landes und personifizierte Einheit des
mertalischen Bundes. Manche sehen in ihm den vergöttlichten
Reichsgründer Darion Garameos, andere halten Ulmon für ein
abstraktes Prinzip oder einen mächtigen Herrschaftsgott.
",@"
Heilung
Im Laufe der Abenteuer, die ein Splitterträger in Lorakis erlebt,
muss er sich vielen Gefahren stellen und wird sicherlich nicht
ewig ohne Verletzungen davonkommen. Um weiter ausziehen zu
können, benötigt er daher die Hilfe kompetenter Heiler.
Möglichkeiten
Die verbreitetste Möglichkeit, Wunden, Vergiftungen oder Krank-
heiten zu versorgen, ist die Heilkunde (S. 119). Auch die Magie-
schule Heilungsmagie (S. 207) ist weit verbreitet und hoch angese-
hen. Zahlreiche Alchemisten (S. 103) kennen viele Mittel, gerade
zur Bekämpfung von Giften, und auch mit Naturkunde gesam-
melte Heilkräuter (S. 125) können in vielen Fällen weiterhelfen.
Tiere und andere Wesen behandeln
Die in diesem Kapitel genannten Schwierigkeiten
gelten für die Behandlung von Wesen aus den fünf
Spielerrassen. Bei anderen Lebewesen (Tieren, an-
deren Rassen, Monstren) ist die Schwierigkeit um 5
(weit verbreitete Kreaturen) oder um 10 (seltene Kre-
aturen, Feenwesen) erhöht.
Heilung von Schaden
Das grundlegende Anwendungsgebiet der Heilkunde ist die Hei-
lung von erlittenem Schaden, ob dieser nun durch Verletzungen,
Vergiftungen, Zauberei oder andere Ursachen entstanden ist.
Heilkunde ist keine Zauberei: Sie bringt einen Patienten nicht
auf wundersame Weise wieder auf die Beine, hilft ihm aber, sich
schneller von Wunden zu erholen.
Probe: Einfache Heilkunde-Probe, die Schwierigkeit beträgt 20
zuzüglich des negativen Modifikators der Gesundheitsstufe, auf
der sich der Abenteurer befindet (also 20 bei Unverletzt, 21 bei
Angeschlagen, 28 bei Todgeweiht etc.).
Pro Tag und Patient darf nur eine einzige Probe abgelegt werden.
Die Probe nimmt 5 Minuten in Anspruch.
Auswirkungen
Verheerend: Dem Heiler unterläuft ein grobes Missgeschick und
die Heilung misslingt. Zusätzlich erhält der Patient 5 Schadens-
punkte +1 pro negativem Erfolgsgrad.
Misslungen: Die Heilung misslingt und der Patient erhält 1
Schadenspunkt pro negativem Erfolgsgrad.
Knapp misslungen: Der Abenteurer begeht einen Fehler und
die Heilung misslingt. Die Probe zählt aber nicht für die Anzahl
der erlaubten Proben pro Tag.
Gelungen: Die Wunde des Patienten wird kompetent versorgt.
Die Heilung während der nächsten Ruhephase erhöht sich um 1
Punkt +1 pro Erfolgsgrad.
Herausragend: Zusätzlich zum Effekt einer gelungenen Heilung
erhält der Patient auf der Stelle 1 Lebenspunkt zurück.
Ausrüstung und Umstände
Besonders saubere Umgebung (leicht positiv), verschmutzte Um-
gebung (leicht bis stark negativ), keine Ausrüstung vorhanden
(leicht bis stark negativ)
Heilung der Zustände
Blutend, Sterbend und Verwundet
Bestimmte schwere Verletzungen oder andere Ursachen bewir-
ken körperliche Zustände (S. 168) bei einem Abenteurer, die
möglichst rasch behandelt werden sollten. Diese Zustände sind
Blutend (stetiger Verlust von Lebenspunkten), Verwundet (eine
schmerzhafte Verletzung, etwa ein gebrochenes Bein) und der
Zustand Sterbend.
Probe: Einfache Heilkunde-Probe, die Schwierigkeit beträgt 20
zuzüglich der höchsten Stufe eines der drei Zustände Blutend,
Sterbend oder Verwundet, an denen der Abenteurer leidet. Es
spielt keine Rolle, welcher Zustand behoben werden soll, die
Schwierigkeit orientiert sich immer an der höchsten Stufe eines
der drei Zustände.
Pro Zustand des Patienten darf nur eine einzige Probe pro Tag
abgelegt werden, die auch nur für diesen Zustand gilt. Die Pro-
be nimmt 5 Minuten in Anspruch. Sobald ein Heilungsversuch
gestartet wurde, wird der Zustand für die Zeit des Heilungsver-
suchs ausgesetzt.
Hat der Zustand eine bestimmte Ursache (siehe S. 168), muss
zunächst die Ursache beseitigt werden, bevor der Zustand geheilt
werden kann.
Auswirkungen
Verheerend: Dem Heiler unterläuft ein grobes Missgeschick und
die Heilung misslingt. Zusätzlich erhält der Patient 5 Schadens-
punkte und 1 weiteren pro negativem Erfolgsgrad.
Misslungen: Die Heilung misslingt und der Patient erhält 1
Schadenspunkt pro negativem Erfolgsgrad.
Knapp misslungen: Der Abenteurer begeht einen Fehler und
die Heilung misslingt. Die Probe zählt aber nicht für die Anzahl
der erlaubten Proben pro Tag.
Gelungen: Der Zustand sinkt um 1 Stufe (+1 pro 2 Erfolgsgra-
de). Sollte der Patient beim Zustand Sterbend keine Lebenspunk-
te mehr gehabt haben, erhält er 1 Lebenspunkt in der untersten
Gesundheitsstufe.
Herausragend: Zusätzlich zum Effekt einer gelungenen Heilung
erhält der Patient auf der Stelle 1W6 Lebenspunkte zurück.
Ausrüstung und Umstände
Besonders saubere Umgebung (leicht positiv), verschmutzte Um-
gebung (leicht bis stark negativ), keine Ausrüstung vorhanden
(leicht bis stark negativ)",
        @"
Landschaft: Halbinsel entlang des Gebirgsrückens der Ven-
tellen, Aylische Ebene, verkarstete Hügelländer
Klima: mediterran; warme Sommer und stürmische, milde
Winter
Flora und Fauna: nur wenige Wildtiere und -pflanzen in
unangetasteten Wäldern
Handel und Verkehr: eine der wichtigsten Handelsmächte
der Drei Meere mit großen Handels- und Kriegsflotten;
Metallwaren aus Eisenbrann, gnomische Mechaniken aus
Talaberis, Weizen aus der Aylanthischen Ebene
Bevölkerung: 2,1 Millionen; je nach Stadt stark wechselnde
Anteile der verschiedenen Rassen, jedoch überall Men-
schen als stärkste Gruppe
Städte und Dörfer: Talaberis (57.000
Einwohner), Aylantha (82.000),
Gondalis (52.000), Nuum (49.000),
Drevilna (125.000), Fulnia (44.000),
Aurigion (31.000), Taupio (38.000),
Eisenbrann (62.000), dazu viele klei-
ne Dörfer im Umland der Metropolen.
Herrschaft: autonome Stadtstaaten, die
sich im Rat der Stadtherren absprechen
Wappen: neunzackiger goldener Stern auf Blau
Religion: bunt gemischt; allgemein anerkannt ist Ulmon
der Bundesherr als Schutzgott des Städtebunds
Allgemeine Stimmung: lebenslustig und geschäftig, aber
starkes soziales Gefälle
"
    };

    #endregion
}