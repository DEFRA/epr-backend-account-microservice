using BackendAccountService.ValidationData.Api.Models;
using BackendAccountService.ValidationData.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Text;
using System.Text.Json;

namespace BackendAccountService.ValidationData.Api.UnitTests;

[TestClass]
public class CompanyDetailsFunctionsTests
{
    private readonly Mock<ICompanyDetailsDataService> _companyDetailsServiceMock = new();
    private readonly Mock<ILogger<CompanyDetailsFunctions>> _loggerMock = new();

    private CompanyDetailsFunctions _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _systemUnderTest = new CompanyDetailsFunctions(
            _companyDetailsServiceMock.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetCompanyDetailsAsync_NoExceptionThrown_WhenValidPayload()
    {
        // Arrange
        var expectedOrganisation = "123456";
        _companyDetailsServiceMock.Setup(service => service
                .GetCompanyDetailsByOrganisationReferenceNumber(expectedOrganisation));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x
                .GetCompanyDetailsAsync(It.IsAny<HttpRequest>(), expectedOrganisation))
            .Should()
            .NotThrowAsync();
    }

    [TestMethod]
    public async Task GetCompanyDetailsAsync_IsNotNull_ReturnsOk()
    {
        // Arrange
        var expectedOrganisation = "123456";
        var requestMock = new Mock<HttpRequest>();
        var companyDetails = new CompanyDetailResponse { ReferenceNumber = "123456", CompaniesHouseNumber = "12345678" };

        var organisations = new List<CompanyDetailResponse>
        {
            companyDetails
        };

        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = organisations
        };

        requestMock.Setup(req => req.Method).Returns("GET");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _companyDetailsServiceMock.Setup(service => service
                .GetCompanyDetailsByOrganisationReferenceNumber(expectedOrganisation))
            .ReturnsAsync(organisationsResult);

        // Act
        var result = await _systemUnderTest
            .GetCompanyDetailsAsync(It.IsAny<HttpRequest>(), expectedOrganisation);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GetCompanyDetailsAsync_CatchesAndLogsException_WhenThrown()
    {
        // Arrange
        var expectedOrganisation = "123456";
        const string exceptionErrorMessage = "Error attempting to fetch organisation";

        _companyDetailsServiceMock.Setup(service => service
                .GetCompanyDetailsByOrganisationReferenceNumber(expectedOrganisation))
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
            .GetCompanyDetailsAsync(It.IsAny<HttpRequest>(), expectedOrganisation);

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
    public async Task GetCompanyDetailsByComplianceSchemeIdAsync_NoExceptionThrown_WhenValidPayload()
    {
        // Arrange
        var expectedOrganisation = "123456";
        _companyDetailsServiceMock.Setup(service => service
            .GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId(
                expectedOrganisation,
                It.IsAny<Guid>()));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x
                .GetCompanyDetailsByComplianceSchemeIdAsync(It.IsAny<HttpRequest>(), expectedOrganisation, It.IsAny<Guid>()))
            .Should()
            .NotThrowAsync();
    }

    [TestMethod]
    public async Task GetCompanyDetailsByComplianceSchemeId_IsNotNull_ReturnsOk()
    {
        // Arrange
        var expectedOrganisation = "123456";
        var companyDetails = new CompanyDetailResponse { ReferenceNumber = "123456", CompaniesHouseNumber = "12345678" };

        var organisations = new List<CompanyDetailResponse>
        {
            companyDetails
        };

        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = organisations
        };

        _companyDetailsServiceMock.Setup(service => service
                .GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId(
                    expectedOrganisation,
                    It.IsAny<Guid>()))
            .ReturnsAsync(organisationsResult);

        // Act
        var result = await _systemUnderTest
            .GetCompanyDetailsByComplianceSchemeIdAsync(
                It.IsAny<HttpRequest>(),
                expectedOrganisation,
                It.IsAny<Guid>());

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GetCompanyDetailsByComplianceSchemeIdAsync_CatchesAndLogsException_WhenThrown()
    {
        // Arrange
        var expectedOrganisation = "123456";
        const string exceptionErrorMessage = "Error attempting to fetch organisation";

        _companyDetailsServiceMock
            .Setup(service => service
                .GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId(
                    expectedOrganisation,
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
            .GetCompanyDetailsByComplianceSchemeIdAsync(
                It.IsAny<HttpRequest>(),
                expectedOrganisation,
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
    public async Task GetAllProducersCompanyDetailsAsync_CatchesAndLogsException_WhenThrown()
    {
        // Arrange
        const string exceptionErrorMessage = "Error attempting to fetch organisation";
        var requestMock = new Mock<HttpRequest>();

        var referenceNumbers = new OrganisationsResponse()
        {
            ReferenceNumbers = new List<string> { "123456", "789123" }
        };

        requestMock.Setup(req => req.Method).Returns("POST");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _companyDetailsServiceMock
           .Setup(service => service
               .GetAllProducersCompanyDetailsAsProducer(
                   It.IsAny<OrganisationReferencesRequest>()
                   )).ThrowsAsync(new Exception(exceptionErrorMessage));

        var problem = new ProblemDetails
        {
            Detail = exceptionErrorMessage,
            Status = 500,
            Title = "Unhandled exception",
            Type = "Exception"
        };

        var organisationReferencesRequest = new OrganisationReferencesRequest
        {
            ReferenceNumbers = referenceNumbers.ReferenceNumbers,
            OrganisationExternalId = string.Empty
        };
        var jsonPayload = JsonSerializer.Serialize(organisationReferencesRequest);
        var requestBodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var requestBodyStream = new MemoryStream(requestBodyBytes);
        requestMock.Setup(req => req.Body).Returns(requestBodyStream);

        // Act
        var result = await _systemUnderTest.GetAllProducersCompanyDetailsAsync(requestMock.Object);

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
    public async Task GetAllProducersCompanyDetailsAsync_RequestBodyAndReferenceNumbersAreNotNull_ReturnsOk()
    {
        // Arrange
        var requestMock = new Mock<HttpRequest>();
        var companyDetails = new CompanyDetailResponse { ReferenceNumber = "123456", CompaniesHouseNumber = "12345678" };

        var organisations = new List<CompanyDetailResponse>
        {
            companyDetails
        };

        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = organisations
        };

        var referenceNumbers = new OrganisationsResponse()
        {
            ReferenceNumbers = new List<string> { "123456", "789123" }
        };

        requestMock.Setup(req => req.Method).Returns("POST");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _companyDetailsServiceMock
           .Setup(service => service
               .GetAllProducersCompanyDetailsAsProducer(
                   It.IsAny<OrganisationReferencesRequest>()
                   )).ReturnsAsync(organisationsResult);

        var organisationReferencesRequest = new OrganisationReferencesRequest
        {
            ReferenceNumbers = referenceNumbers.ReferenceNumbers,
            OrganisationExternalId = string.Empty
        };
        var jsonPayload = JsonSerializer.Serialize(organisationReferencesRequest);
        var requestBodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var requestBodyStream = new MemoryStream(requestBodyBytes);
        requestMock.Setup(req => req.Body).Returns(requestBodyStream);

        // Act
        var result = await _systemUnderTest.GetAllProducersCompanyDetailsAsync(requestMock.Object);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        result.Should().BeEquivalentTo(new OkObjectResult(organisationsResult));
    }

    [TestMethod]
    public async Task GetAllProducersCompanyDetailsAsync_RequestBodyAndReferenceNumbersAreNotNull_CompanyDetailsResultIsNull_ReturnsProblem()
    {
        // Arrange
        var requestMock = new Mock<HttpRequest>();

        var problem = new ProblemDetails
        {
            Status = 404,
            Title = "Organisations not found"
        };

        var referenceNumbers = new OrganisationsResponse()
        {
            ReferenceNumbers = new List<string> { "123456", "789123" }
        };

        requestMock.Setup(req => req.Method).Returns("POST");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _companyDetailsServiceMock
           .Setup(service => service
               .GetAllProducersCompanyDetailsAsProducer(
                   It.IsAny<OrganisationReferencesRequest>()
                   )).ReturnsAsync(default(CompanyDetailsResponse));

        var organisationReferencesRequest = new OrganisationReferencesRequest
        {
            ReferenceNumbers = referenceNumbers.ReferenceNumbers,
            OrganisationExternalId = string.Empty
        };
        var jsonPayload = JsonSerializer.Serialize(organisationReferencesRequest);
        var requestBodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var requestBodyStream = new MemoryStream(requestBodyBytes);
        requestMock.Setup(req => req.Body).Returns(requestBodyStream);

        // Act
        var result = await _systemUnderTest.GetAllProducersCompanyDetailsAsync(requestMock.Object);

        // Assert
        result.Should().BeOfType<ObjectResult>();

        result.Should().BeEquivalentTo(new ObjectResult(problem)
        {
            StatusCode = 404
        });
    }

    [TestMethod]
    public async Task GetAllProducersCompanyDetailsAsync_NoExceptionThrown_WhenValidPayload()
    {
        // Arrange
        var requestMock = new Mock<HttpRequest>();
        var companyDetails = new CompanyDetailResponse { ReferenceNumber = "123456", CompaniesHouseNumber = "12345678" };

        var organisations = new List<CompanyDetailResponse>
        {
            companyDetails
        };

        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = organisations
        };

        var referenceNumbers = new OrganisationsResponse()
        {
            ReferenceNumbers = new List<string> { "123456", "789123" }
        };

        requestMock.Setup(req => req.Method).Returns("POST");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _companyDetailsServiceMock
           .Setup(service => service
               .GetAllProducersCompanyDetails(
                   It.IsAny<IEnumerable<string>>()
                   )).ReturnsAsync(organisationsResult);

        var jsonPayload = JsonSerializer.Serialize<IEnumerable<string>>(referenceNumbers.ReferenceNumbers);
        var requestBodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var requestBodyStream = new MemoryStream(requestBodyBytes);
        requestMock.Setup(req => req.Body).Returns(requestBodyStream);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x
             .GetAllProducersCompanyDetailsAsync(requestMock.Object))
            .Should()
            .NotThrowAsync();
    }

    [TestMethod]
    public async Task GetAllProducersCompanyDetailsAsync_ReferenceNumbersIsNull_ReturnsBadRequest()
    {
        // Arrange
        const string exceptionErrorMessage = "Invalid ReferenceNumbers property in request body";
        var requestMock = new Mock<HttpRequest>();
        var companyDetails = new CompanyDetailResponse { ReferenceNumber = "123456", CompaniesHouseNumber = "12345678" };

        var organisations = new List<CompanyDetailResponse>
        {
            companyDetails
        };

        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = organisations
        };

        var referenceNumbers = new OrganisationsResponse()
        {
            ReferenceNumbers = null
        };

        requestMock.Setup(req => req.Method).Returns("POST");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _companyDetailsServiceMock
           .Setup(service => service
               .GetAllProducersCompanyDetails(
                   It.IsAny<IEnumerable<string>>()
                   )).ReturnsAsync(organisationsResult);

        var problem = new ProblemDetails
        {
            Status = 400,
            Title = exceptionErrorMessage
        };

        var jsonPayload = JsonSerializer.Serialize<IEnumerable<string>>(referenceNumbers.ReferenceNumbers);
        var requestBodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var requestBodyStream = new MemoryStream(requestBodyBytes);
        requestMock.Setup(req => req.Body).Returns(requestBodyStream);

        // Act
        var result = await _systemUnderTest
            .GetAllProducersCompanyDetailsAsync(
                requestMock.Object);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        result.Should().BeEquivalentTo(new ObjectResult(problem)
        {
            StatusCode = 400,
            Value = problem
        });
    }

    [TestMethod]
    public async Task GetCompanyDetailsByProducerAsync_NoExceptionThrown_WhenValidPayload()
    {
        // Arrange
        var expectedProducerId = "33D44FDA-5C21-4021-A06C-202AA9B5E946";
        _companyDetailsServiceMock.Setup(service => service
                .GetCompanyDetailsByOrganisationReferenceNumber(expectedProducerId));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x
                .GetCompanyDetailsByProducerAsync(It.IsAny<HttpRequest>(), expectedProducerId))
            .Should()
            .NotThrowAsync();
    }

    [TestMethod]
    public async Task GetCompanyDetailsByProducerAsync_IsNotNull_ReturnsOk()
    {
        // Arrange
        var expectedProducerId = "33D44FDA-5C21-4021-A06C-202AA9B5E946";
        var producerIdAsGuid = Guid.Parse(expectedProducerId);
        var requestMock = new Mock<HttpRequest>();
        var companyDetails = new CompanyDetailResponse { ReferenceNumber = "123456", CompaniesHouseNumber = "12345678" };

        var organisations = new List<CompanyDetailResponse>
        {
            companyDetails
        };

        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = organisations
        };

        requestMock.Setup(req => req.Method).Returns("GET");
        requestMock.Setup(req => req.ContentType).Returns("application/json");

        _companyDetailsServiceMock.Setup(service => service
                .GetCompanyDetailsByOrganisationExternalId(producerIdAsGuid))
            .ReturnsAsync(organisationsResult);

        // Act
        var result = await _systemUnderTest
            .GetCompanyDetailsByProducerAsync(It.IsAny<HttpRequest>(), expectedProducerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task GetCompanyDetailsByProducerAsync_CatchesAndLogsException_WhenThrown()
    {
        // Arrange
        var expectedProducerId = "33D44FDA-5C21-4021-A06C-202AA9B5E946";
        var producerIdAsGuid = Guid.Parse(expectedProducerId);
        const string exceptionErrorMessage = "Error attempting to fetch organisation";

        _companyDetailsServiceMock.Setup(service => service
                .GetCompanyDetailsByOrganisationExternalId(producerIdAsGuid))
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
            .GetCompanyDetailsByProducerAsync(It.IsAny<HttpRequest>(), expectedProducerId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        result.Should().BeEquivalentTo(new ObjectResult(problem)
        {
            StatusCode = 500,
            Value = problem
        });
        _loggerMock.VerifyLog(logger => logger.LogError(exceptionErrorMessage));
    }
}