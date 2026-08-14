using Denarius.Domain.Exceptions;
using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Tests.ValueObjects;

public class ColorTests
{
    [Theory]
    [InlineData("#FF0000", "#FF0000")]
    [InlineData("ff0000", "#FF0000")]
    [InlineData("#f00", "#FF0000")]
    [InlineData("f00", "#FF0000")]
    [InlineData("  #FF0000  ", "#FF0000")]
    public void Constructor_ValidHexCode_NormalizesToUppercaseSixDigitWithHash(string input, string expected)
    {
        var color = new Color(input);

        Assert.Equal(expected, color.HexCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("#GGGGGG")]
    [InlineData("#FF00")]
    [InlineData("not-a-color")]
    public void Constructor_InvalidHexCode_ThrowsDomainException(string? input)
    {
        Assert.Throws<DomainException>(() => new Color(input!));
    }

    [Fact]
    public void Equals_SameNormalizedHexCode_ReturnsTrue()
    {
        var a = new Color("#ff0000");
        var b = new Color("f00");

        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentHexCode_ReturnsFalse()
    {
        var a = new Color("#FF0000");
        var b = new Color("#00FF00");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_ReturnsHexCode()
    {
        var color = new Color("#FF0000");

        Assert.Equal("#FF0000", color.ToString());
    }
}
