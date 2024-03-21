using BackendAccountService.ValidationData.Api.Models;
using BackendAccountService.ValidationData.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
    public async Task GetOrganisationAsync_NoExceptionThrown_WhenValidPayload()
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
    public async Task GetCompanyDetailsByOrganisationReferenceNumber_IsNotNull_ReturnsOk()
    {
        // Arrange
        var expectedOrganisation = "123456";
        var requestMock = new Mock<HttpRequest>();
        var CompanyDetails = new CompanyDetailResponse { ReferenceNumber = "123456", CompaniesHouseNumber = "12345678" };

        var organisations = new List<CompanyDetailResponse>();
        organisations.Add(CompanyDetails);

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
        var requestMock = new Mock<HttpRequest>();
        var CompanyDetails = new CompanyDetailResponse { ReferenceNumber = "123456", CompaniesHouseNumber = "12345678" };

        var organisations = new List<CompanyDetailResponse>();
        organisations.Add(CompanyDetails);

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

}