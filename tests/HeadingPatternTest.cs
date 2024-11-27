using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ReformatOnCopy;

namespace ReformatOnCopyTests;

public class HeadingPatternTest
{
    

    [Test]
    public void Reformat_HeadingIsReplaced()
    {
        // Arrange
        var text = @"xxxxxxxx xx Xxxxx xxx xxx Xxx xxx xxxxxxxxxxxx Xxxxx xx Xxxx
xxxxxxxx xxx.
Heading
Xxxx xxxx xxx xxxxxx xxx Xxxxxxx xxxx xxx Xxxxxxxxxxxx
xxxxxx, xxxx";
        var reformatter = new Reformatter(new List<HeadingPattern>
        {
            new("Heading", "# $0\n\n", false),
            new("NotMatched", "Z $0", false)
        });

        // Act
        var result = reformatter.Reformat(text);

        // Assert
        result.Should().Contain("# Heading\n\n").And.NotContain("Z");
    }

    [Test]
    public void Reformat_NotAtTheBeginningOfALine_NotReplaced()
    {
        // Arrange
        var text = @"xxx xxxxx Xxxxxx xxxx xxxx – xxx Xxxxxx xxxxxxxxx – xxxxx
xxxxxx Xxxxxxxxx xxx xxxxx Xxxxxxxx. Xxx Xxxxxx xxxxxxxxx
xxxx xxxxxxxxx xxx Xxxxxx xxx xxxxxxxxxxxx Xxxxxxxx xxxxxx. Xxxx
xxx Xxxxxxx xxx xxx Xxxxxxxxxxxxxxxx xxxxx Xxxxxxxxxx xxxx
xxxxxx Heading xxxxxxxxx xxxx (x.X. xxx Xxxxxxxxxxxx xxxxs Xxxxxx
xxxx xxx Xxxxxxxxxx xxxxx xxxxxxxxxxxxx Xxxxxxxxxx xxxxx xx";
        var reformatter = new Reformatter(new List<HeadingPattern>
        {
            new("Heading", "# $0\n\n", false),
            new("Ignored", "` $0\n\n"),
            new("NotMatched", "% $0", false)
        });
        
        // Act
        var result = reformatter.Reformat(text);

        // Assert
        result.Should().NotContain("#").And.NotContain("`").And.NotContain("%");
    }
}