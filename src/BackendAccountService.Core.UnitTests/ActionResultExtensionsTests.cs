using BackendAccountService.Core.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BackendAccountService.Core.UnitTests;

[TestClass]
public class ActionResultExtensionsTests
{
    // Tests for: public static ActionResult? ThenValidate(this ActionResult? existingProblem, Func<ActionResult?> nextValidation)

    [TestMethod]
    public void ThenValidate_ExistingProblemIsNotNull_ReturnsExistingProblemAndDoesNotCallNextValidation()
    {
        // Arrange
        var existingProblem = new BadRequestObjectResult("Existing problem");
        var nextValidationMock = new Mock<Func<ActionResult?>>();
        nextValidationMock.Setup(nv => nv()).Returns(new OkResult());

        // Act
        var result = existingProblem.ThenValidate(nextValidationMock.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(existingProblem, result, "Should return the existing problem.");
        nextValidationMock.Verify(nv => nv(), Times.Never, "Next validation should not have been called.");
    }

    [TestMethod]
    public void ThenValidate_ExistingProblemIsNull_CallsNextValidationAndReturnsItsResult()
    {
        // Arrange
        ActionResult? existingProblem = null;
        var expectedValidationResult = new NotFoundObjectResult("Not found from validation");
        var nextValidationMock = new Mock<Func<ActionResult?>>();
        nextValidationMock.Setup(nv => nv()).Returns(expectedValidationResult);

        // Act
        var result = existingProblem.ThenValidate(nextValidationMock.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(expectedValidationResult, result, "Should return the result from nextValidation.");
        nextValidationMock.Verify(nv => nv(), Times.Once, "Next validation should have been called once.");
    }

    [TestMethod]
    public void ThenValidate_ExistingProblemIsNullAndNextValidationReturnsNull_ReturnsNull()
    {
        // Arrange
        ActionResult? existingProblem = null;
        var nextValidationMock = new Mock<Func<ActionResult?>>();
        nextValidationMock.Setup(nv => nv()).Returns(() => null); // Explicitly return null

        // Act
        var result = existingProblem.ThenValidate(nextValidationMock.Object);

        // Assert
        Assert.IsNull(result, "Result should be null when nextValidation returns null.");
        nextValidationMock.Verify(nv => nv(), Times.Once, "Next validation should have been called once.");
    }

    // Tests for: public static async Task<ActionResult?> ThenValidate(this Task<ActionResult?> existingProblemTask, Func<ActionResult?> nextValidation)

    [TestMethod]
    public async Task ThenValidate_TaskWithExistingProblemIsNotNull_ReturnsExistingProblemAndDoesNotCallNextValidation()
    {
        // Arrange
        var existingProblem = new BadRequestObjectResult("Existing problem from task");
        var existingProblemTask = Task.FromResult<ActionResult?>(existingProblem);
        var nextValidationMock = new Mock<Func<ActionResult?>>();
        nextValidationMock.Setup(nv => nv()).Returns(new OkResult());

        // Act
        var result = await existingProblemTask.ThenValidate(nextValidationMock.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(existingProblem, result, "Should return the existing problem from the task.");
        nextValidationMock.Verify(nv => nv(), Times.Never, "Next validation should not have been called.");
    }

    [TestMethod]
    public async Task ThenValidate_TaskWithExistingProblemIsNull_CallsNextValidationAndReturnsItsResult()
    {
        // Arrange
        var existingProblemTask = Task.FromResult<ActionResult?>(null);
        var expectedValidationResult = new NotFoundObjectResult("Not found from validation after task");
        var nextValidationMock = new Mock<Func<ActionResult?>>();
        nextValidationMock.Setup(nv => nv()).Returns(expectedValidationResult);

        // Act
        var result = await existingProblemTask.ThenValidate(nextValidationMock.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(expectedValidationResult, result, "Should return the result from nextValidation.");
        nextValidationMock.Verify(nv => nv(), Times.Once, "Next validation should have been called once.");
    }

    [TestMethod]
    public async Task ThenValidate_TaskWithExistingProblemIsNullAndNextValidationReturnsNull_ReturnsNull()
    {
        // Arrange
        var existingProblemTask = Task.FromResult<ActionResult?>(null);
        var nextValidationMock = new Mock<Func<ActionResult?>>();
        nextValidationMock.Setup(nv => nv()).Returns(() => null);

        // Act
        var result = await existingProblemTask.ThenValidate(nextValidationMock.Object);

        // Assert
        Assert.IsNull(result, "Result should be null when nextValidation returns null.");
        nextValidationMock.Verify(nv => nv(), Times.Once, "Next validation should have been called once.");
    }

    // Tests for: public static async Task<ActionResult?> ThenValidateAsync(this Task<ActionResult?> existingProblemTask, Func<Task<ActionResult?>> nextValidationAsync)

    [TestMethod]
    public async Task ThenValidateAsync_TaskWithExistingProblemIsNotNull_ReturnsExistingProblemAndDoesNotCallNextValidationAsync()
    {
        // Arrange
        var existingProblem = new ConflictObjectResult("Existing conflict from task");
        var existingProblemTask = Task.FromResult<ActionResult?>(existingProblem);
        var nextValidationAsyncMock = new Mock<Func<Task<ActionResult?>>>();
        nextValidationAsyncMock.Setup(nv => nv()).ReturnsAsync(new OkResult());

        // Act
        var result = await existingProblemTask.ThenValidateAsync(nextValidationAsyncMock.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(existingProblem, result, "Should return the existing problem from the task.");
        nextValidationAsyncMock.Verify(nv => nv(), Times.Never, "Next async validation should not have been called.");
    }

    [TestMethod]
    public async Task ThenValidateAsync_TaskWithExistingProblemIsNull_CallsNextValidationAsyncAndReturnsItsResult()
    {
        // Arrange
        var existingProblemTask = Task.FromResult<ActionResult?>(null);
        var expectedValidationResult = new UnprocessableEntityObjectResult("Unprocessable from async validation after task");
        var nextValidationAsyncMock = new Mock<Func<Task<ActionResult?>>>();
        nextValidationAsyncMock.Setup(nv => nv()).ReturnsAsync(expectedValidationResult);

        // Act
        var result = await existingProblemTask.ThenValidateAsync(nextValidationAsyncMock.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(expectedValidationResult, result, "Should return the result from nextValidationAsync.");
        nextValidationAsyncMock.Verify(nv => nv(), Times.Once, "Next async validation should have been called once.");
    }

    [TestMethod]
    public async Task ThenValidateAsync_TaskWithExistingProblemIsNullAndNextValidationAsyncReturnsNull_ReturnsNull()
    {
        // Arrange
        var existingProblemTask = Task.FromResult<ActionResult?>(null);
        var nextValidationAsyncMock = new Mock<Func<Task<ActionResult?>>>();
        nextValidationAsyncMock.Setup(nv => nv()).ReturnsAsync(() => null);

        // Act
        var result = await existingProblemTask.ThenValidateAsync(nextValidationAsyncMock.Object);

        // Assert
        Assert.IsNull(result, "Result should be null when nextValidationAsync returns null.");
        nextValidationAsyncMock.Verify(nv => nv(), Times.Once, "Next async validation should have been called once.");
    }

    // Tests for: public static async Task<ActionResult?> ThenValidateAsync(this ActionResult? existingProblem, Func<Task<ActionResult?>> nextValidationAsync)

    [TestMethod]
    public async Task ThenValidateAsync_ExistingProblemIsNotNull_ReturnsExistingProblemAndDoesNotCallNextValidationAsync()
    {
        // Arrange
        var existingProblem = new StatusCodeResult(StatusCodes.Status403Forbidden);
        var nextValidationAsyncMock = new Mock<Func<Task<ActionResult?>>>();
        nextValidationAsyncMock.Setup(nv => nv()).ReturnsAsync(new OkResult());

        // Act
        var result = await existingProblem.ThenValidateAsync(nextValidationAsyncMock.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(existingProblem, result, "Should return the existing problem.");
        nextValidationAsyncMock.Verify(nv => nv(), Times.Never, "Next async validation should not have been called.");
    }

    [TestMethod]
    public async Task ThenValidateAsync_ExistingProblemIsNull_CallsNextValidationAsyncAndReturnsItsResult()
    {
        // Arrange
        ActionResult? existingProblem = null;
        var expectedValidationResult = new OkObjectResult("Success from async validation");
        var nextValidationAsyncMock = new Mock<Func<Task<ActionResult?>>>();
        nextValidationAsyncMock.Setup(nv => nv()).ReturnsAsync(expectedValidationResult);

        // Act
        var result = await existingProblem.ThenValidateAsync(nextValidationAsyncMock.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(expectedValidationResult, result, "Should return the result from nextValidationAsync.");
        nextValidationAsyncMock.Verify(nv => nv(), Times.Once, "Next async validation should have been called once.");
    }

    [TestMethod]
    public async Task ThenValidateAsync_ExistingProblemIsNullAndNextValidationAsyncReturnsNull_ReturnsNull()
    {
        // Arrange
        ActionResult? existingProblem = null;
        var nextValidationAsyncMock = new Mock<Func<Task<ActionResult?>>>();
        nextValidationAsyncMock.Setup(nv => nv()).ReturnsAsync(() => null);

        // Act
        var result = await existingProblem.ThenValidateAsync(nextValidationAsyncMock.Object);

        // Assert
        Assert.IsNull(result, "Result should be null when nextValidationAsync returns null.");
        nextValidationAsyncMock.Verify(nv => nv(), Times.Once, "Next async validation should have been called once.");
    }
}