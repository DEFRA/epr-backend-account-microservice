using BackendAccountService.Core.Extensions;

namespace BackendAccountService.Core.UnitTests.Extensions;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    [DataRow("123456", "123456")]
    [DataRow("123 456", "123456")]
    [DataRow(" 1 2 3 4 5 6 ", "123456")]
    [DataRow("1234567", null)]
    [DataRow("12345", null)]
    [DataRow("12345A", null)]
    public void GetReferenceNumber_ReturnsExpectedResult(string testValue, string? expectedResult)
    {
        testValue.TryExtractReferenceNumberFromQuery().Should().Be(expectedResult);
    }

}