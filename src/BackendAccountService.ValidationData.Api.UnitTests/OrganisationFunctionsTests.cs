using System.Text;
using BackendAccountService.ValidationData.Api.Models;
using BackendAccountService.ValidationData.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text.Json;

namespace BackendAccountService.ValidationData.Api.UnitTests;

[TestClass]
public class OrganisationFunctionsTests
{

    private readonly Mock<IOrganisationDataService> _organisationServiceMock = new();
    private readonly Mock<ILogger<OrganisationFunctions>> _loggerMock = new();

    private OrganisationFunctions _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _systemUnderTest = new OrganisationFunctions(
            _organisationServiceMock.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetOrganisationAsync_NoExceptionThrown_WhenValidPayload()
    {
        // Arrange
        _organisationServiceMock.Setup(service => service
                .GetOrganisationByExternalId(It.IsAny<Guid>()));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x
                .GetOrganisationAsync(It.IsAny<HttpRequest>(), It.IsAny<Guid>()))
            .Should()
            .NotThrowAsync();
    }

    [TestMethod]
    public async Task GetOrganisationByExternalId_IsNotNull_ReturnsOk()
    {
        // Arrange
        var organisationMock = new OrganisationResponse()
        {
            ReferenceNumber = "000000",
            IsComplianceScheme = false
        };
        _organisationServiceMock.Setup(service => service
                .GetOrganisationByExternalId(It.IsAny<Guid>()))
            .ReturnsAsync(organisationMock);

        // Act
        var result = await _systemUnderTest
            .GetOrganisationAsync(It.IsAny<HttpRequest>(), It.IsAny<Guid>());

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GetOrganisationAsync_CatchesAndLogsException_WhenThrown()
    {
        // Arrange
        const string exceptionErrorMessage = "Error attempting to fetch organisation";

        _organisationServiceMock
            .Setup(service => service
                .GetOrganisationByExternalId(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception(exceptionErrorMessage));

        var problem = new ProblemDetails
        {
            Detail = exceptionErrorMessage,
            Status = 500,
            Title = "Unhandled exception",
            Type = "Exception"
        };

        // Act
        var result = await _systemUnderTest
            .GetOrganisationAsync(It.IsAny<HttpRequest>(), It.IsAny<Guid>());

        // Assert
        result.Should().BeOfType<ObjectResult>();
        result.Should().BeEquivalentTo(new ObjectResult(problem)
        {
            StatusCode = 500,
            Value = problem
        });
        _loggerMock.VerifyLog(logger => logger.LogError(exceptionErrorMessage));
    }

    [TestMethod]
    public async Task GetOrganisationMembersAsync_NoExceptionThrown_WhenValidPayload()
    {
        // Arrange
        _organisationServiceMock.Setup(service => service
            .GetOrganisationMembersByComplianceSchemeId(
                It.IsAny<Guid>(),
                It.IsAny<Guid>()));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x
                .GetOrganisationMembersAsync(It.IsAny<HttpRequest>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Should()
            .NotThrowAsync();
    }

    [TestMethod]
    public async Task GetOrganisationMembersByComplianceId_IsNotNull_ReturnsOk()
    {
        // Arrange
        var organisationMock = new OrganisationMembersResponse()
        {
            MemberOrganisations = new [] { "000000" }
        };
        _organisationServiceMock.Setup(service => service
                .GetOrganisationMembersByComplianceSchemeId(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>()))
            .ReturnsAsync(organisationMock);

        // Act
        var result = await _systemUnderTest
            .GetOrganisationMembersAsync(
                It.IsAny<HttpRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>());

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GetOrganisationMembersAsync_CatchesAndLogsException_WhenThrown()
    {
        // Arrange
        const string exceptionErrorMessage = "Error attempting to fetch organisation";

        _organisationServiceMock
            .Setup(service => service
                .GetOrganisationMembersByComplianceSchemeId(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>()))
            .ThrowsAsync(new Exception(exceptionErrorMessage));

        var problem = new ProblemDetails
        {
            Detail = exceptionErrorMessage,
            Status = 500,
            Title = "Unhandled exception",
            Type = "Exception"
        };

        // Act
        var result = await _systemUnderTest
            .GetOrganisationMembersAsync(
                It.IsAny<HttpRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>());

        // Assert
        result.Should().BeOfType<ObjectResult>();
        result.Should().BeEquivalentTo(new ObjectResult(problem)
        {
            StatusCode = 500,
            Value = problem
        });

        _loggerMock.VerifyLog(logger => logger.LogError(exceptionErrorMessage));
    }

    [TestMethod]
    public async Task GetExistingOrganisationsAsync_CatchesAndLogsException_WhenThrown()
    {
        // Arrange
        const string exceptionErrorMessage = "Error attempting to fetch organisation";
        var requestMock = new Mock<HttpRequest>();

        requestMock.Setup(req => req.Method).Returns("POST");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _organisationServiceMock
            .Setup(service => service
                .GetExistingOrganisationsByReferenceNumber(
                    It.IsAny<List<string>>()))
            .ThrowsAsync(new Exception(exceptionErrorMessage));

        var problem = new ProblemDetails
        {
            Detail = exceptionErrorMessage,
            Status = 500,
            Title = "Unhandled exception",
            Type = "Exception"
        };

        var requestModel = new OrganisationsRequest
        {
            ReferenceNumbers = new List<string>{ "123456", "789123" }
        };


        var jsonPayload = JsonSerializer.Serialize<OrganisationsRequest>(requestModel);
        var requestBodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var requestBodyStream = new MemoryStream(requestBodyBytes);
        requestMock.Setup(req => req.Body).Returns(requestBodyStream);

        // Act
        var result = await _systemUnderTest
            .GetExistingOrganisationsAsync(
                requestMock.Object);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        result.Should().BeEquivalentTo(new ObjectResult(problem)
        {
            StatusCode = 500,
            Value = problem
        });
    }

    [TestMethod]
    public async Task GetExistingOrganisations_RequestBodyAndReferenceNumbersAreNotNull_ReturnsOk()
    {
        // Arrange
        var requestMock = new Mock<HttpRequest>();
        var organisations = new OrganisationsResponse()
        {
            ReferenceNumbers = new List<string> { "123456", "789123" }
        };

        requestMock.Setup(req => req.Method).Returns("POST");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _organisationServiceMock
            .Setup(service => service
                .GetExistingOrganisationsByReferenceNumber(
                    It.IsAny<List<string>>()))
            .ReturnsAsync(organisations);

        var requestModel = new OrganisationsRequest
        {
            ReferenceNumbers = organisations.ReferenceNumbers
        };


        var jsonPayload = JsonSerializer.Serialize<OrganisationsRequest>(requestModel);
        var requestBodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var requestBodyStream = new MemoryStream(requestBodyBytes);
        requestMock.Setup(req => req.Body).Returns(requestBodyStream);

        // Act
        var result = await _systemUnderTest.GetExistingOrganisationsAsync(requestMock.Object);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        result.Should().BeEquivalentTo(new OkObjectResult(organisations));
    }

    [TestMethod]
    public async Task GetExistingOrganisationsAsync_NoExceptionThrown_WhenValidPayload()
    {
        // Arrange
        var requestMock = new Mock<HttpRequest>();
        var organisations = new OrganisationsResponse()
        {
            ReferenceNumbers = new List<string> { "123456", "789123" }
        };

        requestMock.Setup(req => req.Method).Returns("POST");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _organisationServiceMock
            .Setup(service => service
                .GetExistingOrganisationsByReferenceNumber(
                    It.IsAny<List<string>>()))
            .ReturnsAsync(organisations);

        var requestModel = new OrganisationsRequest
        {
            ReferenceNumbers = organisations.ReferenceNumbers
        };


        var jsonPayload = JsonSerializer.Serialize<OrganisationsRequest>(requestModel);
        var requestBodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var requestBodyStream = new MemoryStream(requestBodyBytes);
        requestMock.Setup(req => req.Body).Returns(requestBodyStream);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x
                .GetExistingOrganisationsAsync(requestMock.Object))
            .Should()
            .NotThrowAsync();
    }

    [TestMethod]
    public async Task GetExistingOrganisationsAsync_ReferenceNumbersIsNull_ReturnsBadRequest()
    {
        // Arrange
        const string exceptionErrorMessage = "Invalid ReferenceNumbers property in request body";
        var organisations = new OrganisationsResponse()
        {
            ReferenceNumbers = new List<string> { "123456", "789123" }
        };
        var requestMock = new Mock<HttpRequest>();

        requestMock.Setup(req => req.Method).Returns("POST");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _organisationServiceMock
            .Setup(service => service
                .GetExistingOrganisationsByReferenceNumber(
                    It.IsAny<List<string>>()))
            .ReturnsAsync(organisations);

        var problem = new ProblemDetails
        {
            Status = 400,
            Title = exceptionErrorMessage
        };

        var requestModel = new OrganisationsRequest
        {
            ReferenceNumbers = null
        };

        var jsonPayload = JsonSerializer.Serialize<OrganisationsRequest>(requestModel);
        var requestBodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var requestBodyStream = new MemoryStream(requestBodyBytes);
        requestMock.Setup(req => req.Body).Returns(requestBodyStream);


        // Act
        var result = await _systemUnderTest
            .GetExistingOrganisationsAsync(
                requestMock.Object);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        result.Should().BeEquivalentTo(new ObjectResult(problem)
        {
            StatusCode = 400,
            Value = problem
        });
    }
}