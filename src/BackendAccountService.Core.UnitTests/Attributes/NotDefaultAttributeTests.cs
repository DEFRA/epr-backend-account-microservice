using BackendAccountService.Core.Models.Request.Attributes;

namespace BackendAccountService.Core.UnitTests.Attributes;

[TestClass]
public class NotDefaultAttributeTests
{
    private readonly NotDefaultAttribute _sut = new();

    [TestMethod]
    public void IsValid_WhenValueIsNull_ReturnsTrue()
    {
        // Act
        var result = _sut.IsValid(null);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsValid_WhenValueIsReferenceType_ReturnsTrue()
    {
        // Act
        var result = _sut.IsValid("123");

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(0, false)]
    [DataRow(123, true)]
    public void IsValid_WhenValueIsValueType_ReturnsCorrectResponse(int value, bool expectedResult)
    {
        // Act
        var result = _sut.IsValid(value);

        // Assert
        result.Should().Be(expectedResult);
    }
}