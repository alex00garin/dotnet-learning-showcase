using Xunit;

namespace DotnetLearningShowcase.Tests;

public class BasicTest
{
    [Fact]
    public void BasicTest_ShouldPass()
    {
        // Arrange
        var expected = 2;

        // Act
        var actual = 1 + 1;

        // Assert
        Assert.Equal(expected, actual);
    }
} 