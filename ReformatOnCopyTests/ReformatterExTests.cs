using FluentAssertions;
using NUnit.Framework;
using ReformatOnCopy;

namespace ReformatOnCopyTests;

public class ReformatterExTests
{
    private Reformatter reformatter = null!;
    
    [SetUp]
    public void Setup()
    {
        reformatter = new();
    }

    [Test]
    public void SingleLine_NothingShouldChange()
    {
        // Act
        var result = reformatter.ReformatEx(@"Nothing should change in a single line.");
        
        // Assert
        result.Should().Be(@"Nothing should change in a single line.");
    }

    [Test]
    public void LineBreakInTheMiddleOfASentence_ShouldBeRemoved()
    {
        // Act
        var result = reformatter.ReformatEx("Line break in\nthe middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in the middle of a sentence.");
    }

    [Test]
    public void Hyphenation_ShouldBeRemoved()
    {
        // Act
        var result = reformatter.ReformatEx("Dashes that are in- \n volved in splitting.");
        
        // Assert
        result.Should().Be("Dashes that are involved in splitting.");
    }

    [Test]
    public void ExoticLineBreakInTheMiddleOfASentence_ShouldBeRemoved()
    {
        // Act
        var result = reformatter.ReformatEx("Line break in\r\nthe middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in the middle of a sentence.");
    }

    [Test]
    public void DoubleLineBreakInTheMiddleOfASentence_ShouldBeRetained()
    {
        // Act
        var result = reformatter.ReformatEx("Line break in\n\nthe middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in\n\nthe middle of a sentence.");
    }

    [Test]
    public void BreakAfterFullStop_ShouldBeRetained()
    {
        // Act
        var result = reformatter.ReformatEx("End. \n And beginning.");
        
        // Assert
        result.Should().Be("End.\n\nAnd beginning.");
    }

    [Test]
    public void BreakAfterColon_ShouldBeRetained()
    {
        // Act
        var result = reformatter.ReformatEx("End: \n And beginning.");
        
        // Assert
        result.Should().Be("End:\n\nAnd beginning.");
    }

    [Test]
    public void BreakAfterExclamation_ShouldBeRetained()
    {
        // Act
        var result = reformatter.ReformatEx("End! \n And beginning.");
        
        // Assert
        result.Should().Be("End!\n\nAnd beginning.");
    }

    [Test]
    public void BreakAfterQuestion_ShouldBeRetained()
    {
        // Act
        var result = reformatter.ReformatEx("End? \n And beginning.");
        
        // Assert
        result.Should().Be("End?\n\nAnd beginning.");
    }

    [Test]
    public void BreakBeforeUppercaseThenColon_ShouldBeRetained()
    {
        // Act
        var result = reformatter.ReformatEx("1577 AZ \n Weight: 1.5 Kg.");
        
        // Assert
        result.Should().Be("1577 AZ\n\nWeight: 1.5 Kg.");
    }

    [Test]
    public void BreakBeforeLowercaseThenColon_ShouldBeRemoved()
    {
        // Act
        var result = reformatter.ReformatEx("only one thing \n to do: hold the door.");
        
        // Assert
        result.Should().Be("only one thing to do: hold the door.");
    }

    [Test]
    public void TripleLineBreakInTheMiddleOfASentence_ShouldBeNormalized()
    {
        // Act
        var result = reformatter.ReformatEx("Line break in\n\n\nthe middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in\n\nthe middle of a sentence.");
    }

    [Test]
    public void DoubleExoticLineBreakInTheMiddleOfASentence_ShouldBeNormalized()
    {
        // Act
        var result = reformatter.ReformatEx("Line break in\r\n\r\nthe middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in\n\nthe middle of a sentence.");
    }

    [Test]
    public void DoubleLineBreakWithSpacesInTheMiddleOfASentence_ShouldBeNormalized()
    {
        // Act
        var result = reformatter.ReformatEx("Line break in   \n \t\n  the middle of a sentence.");
        
        // Assert
        result.Should().Be("Line break in\n\nthe middle of a sentence.");
    }
}