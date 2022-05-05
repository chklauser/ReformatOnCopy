using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ReformatOnCopy;

namespace ReformatOnCopyTests;

public class ReformatterTests
{
    private Reformatter reformatter = null!;
    
    [SetUp]
    public void Setup()
    {
        reformatter = new(Enumerable.Empty<HeadingPattern>());
    }

    [Test]
    public void SingleLine_NothingShouldChange()
    {
        // Act
        var result = reformatter.Reformat(@"Nothing should change in a single line.");
        
        // Assert
        result.Should().Be(@"Nothing should change in a single line.");
    }

    [Test]
    public void LineBreakInTheMiddleOfASentence_ShouldBeRemoved()
    {
        // Act
        var result = reformatter.Reformat("Line break in\nthe middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in the middle of a sentence.");
    }

    [Test]
    public void Hyphenation_ShouldBeRemoved()
    {
        // Act
        var result = reformatter.Reformat("Dashes that are in- \n volved in splitting.");
        
        // Assert
        result.Should().Be("Dashes that are involved in splitting.");
    }

    [Test]
    public void ExoticLineBreakInTheMiddleOfASentence_ShouldBeRemoved()
    {
        // Act
        var result = reformatter.Reformat("Line break in\r\nthe middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in the middle of a sentence.");
    }

    [Test]
    public void DoubleLineBreakInTheMiddleOfASentence_ShouldBeRetained()
    {
        // Act
        var result = reformatter.Reformat("Line break in\n\nthe middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in\n\nthe middle of a sentence.");
    }

    [Test]
    public void BreakAfterFullStop_ShouldBeRetained()
    {
        // Act
        var result = reformatter.Reformat("End. \n And beginning.");
        
        // Assert
        result.Should().Be("End.\n\nAnd beginning.");
    }

    [Test]
    public void BreakAfterColon_ShouldBeRetained()
    {
        // Act
        var result = reformatter.Reformat("End: \n And beginning.");
        
        // Assert
        result.Should().Be("End:\n\nAnd beginning.");
    }

    [Test]
    public void BreakAfterExclamation_ShouldBeRetained()
    {
        // Act
        var result = reformatter.Reformat("End! \n And beginning.");
        
        // Assert
        result.Should().Be("End!\n\nAnd beginning.");
    }

    [Test]
    public void BreakAfterQuestion_ShouldBeRetained()
    {
        // Act
        var result = reformatter.Reformat("End? \n And beginning.");
        
        // Assert
        result.Should().Be("End?\n\nAnd beginning.");
    }

    [Test]
    public void BreakBeforeUppercaseThenColon_ShouldBeRetained()
    {
        // Act
        var result = reformatter.Reformat("1577 AZ \n Weight: 1.5 Kg.");
        
        // Assert
        result.Should().Be("1577 AZ\n\nWeight: 1.5 Kg.");
    }

    [Test]
    public void BreakBeforeLowercaseThenColon_ShouldBeRemoved()
    {
        // Act
        var result = reformatter.Reformat("only one thing \n to do: hold the door.");
        
        // Assert
        result.Should().Be("only one thing to do: hold the door.");
    }

    [Test]
    public void TripleLineBreakInTheMiddleOfASentence_ShouldBeNormalized()
    {
        // Act
        var result = reformatter.Reformat("Line break in\n\n\nthe middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in\n\nthe middle of a sentence.");
    }

    [Test]
    public void DoubleExoticLineBreakInTheMiddleOfASentence_ShouldBeNormalized()
    {
        // Act
        var result = reformatter.Reformat("Line break in\r\n\r\nthe middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in\n\nthe middle of a sentence.");
    }

    [Test]
    public void DoubleLineBreakWithSpacesInTheMiddleOfASentence_ShouldBeNormalized()
    {
        // Act
        var result = reformatter.Reformat("Line break in   \n \t\n  the middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in\n\nthe middle of a sentence.");
    }

    [Test]
    public void AssembleHeadings_Zero()
    {
        // Act
        var result = Reformatter.AssembleHeadingPattern(Enumerable.Empty<HeadingPattern>());
        
        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void AssembleHeadings_JustWithColon()
    {
        // Arrange
        var headings = new HeadingPattern[]
        {
            new("h1", "# $0\n\n"),
            new("h2", "## $0\n\n"),
        };
        
        // Act
        var result = Reformatter.AssembleHeadingPattern(headings);
        
        // Assert
        result.Should().Be(@"^(?<title1>h1|h2):[\t\p{Z}-[\p{Zl}]]*");
    }

    [Test]
    public void AssembleHeadings_JustWithoutColon()
    {
        // Arrange
        var headings = new HeadingPattern[]
        {
            new("h1", "# $0\n\n", ExpectColon: false),
            new("h2", "## $0\n\n", ExpectColon: false),
        };
        
        // Act
        var result = Reformatter.AssembleHeadingPattern(headings);
        
        // Assert
        result.Should().Be(@"^(?<title2>h1|h2)[\t\p{Z}-[\p{Zl}]]*");
    }

    [Test]
    public void AssembleHeadings_Both()
    {
        // Arrange
        var headings = new HeadingPattern[]
        {
            new("h1", "# $0\n\n", ExpectColon: false),
            new("h2", "## $0\n\n", ExpectColon: false),
            new("h3", "### $0\n\n"),
            new("h4", "#### $0\n\n"),
        };
        
        // Act
        var result = Reformatter.AssembleHeadingPattern(headings);
        
        // Assert
        result.Should().Be(@"^(?<title1>h3|h4):[\t\p{Z}-[\p{Zl}]]*|(?<title2>h1|h2)[\t\p{Z}-[\p{Zl}]]*");
    }

    [Test]
    public void AssembleHeadings_ReplaceSpacePattern()
    {
        // Arrange
        var headings = new HeadingPattern[]
        {
            new(@"h1\s+h2", "# $0\n\n", ExpectColon: true),
            new(@"h3\s+h4", "## $0\n\n", ExpectColon: false)
        };
        
        // Act
        var result = Reformatter.AssembleHeadingPattern(headings);
        
        // Assert
        result.Should().Be(@"^(?<title1>h1[\t\p{Z}-[\p{Zl}]]+h2):[\t\p{Z}-[\p{Zl}]]*|(?<title2>h3[\t\p{Z}-[\p{Zl}]]+h4)[\t\p{Z}-[\p{Zl}]]*");
    }
}