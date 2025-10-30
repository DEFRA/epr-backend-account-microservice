using BackendAccountService.ValidationData.Api.Models;
using BackendAccountService.ValidationData.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text;
using System.Text.Json;

namespace BackendAccountService.ValidationData.Api.UnitTests;

[TestClass]
public class SubsidiaryFunctionsTests
{
    private readonly Mock<ISubsidiaryDataService> _subsidiaryDataServiceMock = new();
    private readonly Mock<ILogger<SubsidiaryFunctions>> _loggerMock = new();

    [TestMethod]
    public async Task GetSubsidiaryDetailsAsync_WhenValidRequest_SuccessfullyReturnsResponse()
    {
        // Arrange
        var subRequest = GetSubsidiaryDetailsRequest();
        var expectedResponse = new SubsidiaryDetailsResponse { SubsidiaryOrganisationDetails = subRequest.SubsidiaryOrganisationDetails };
        var requestMock = GetMockHttpRequest(subRequest);

        _subsidiaryDataServiceMock
            .Setup(service => service.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(expectedResponse);

        var sut = new SubsidiaryFunctions(_subsidiaryDataServiceMock.Object, _loggerMock.Object);

        // Act
        var actionResult = await sut.GetSubsidiaryDetailsAsync(requestMock.Object);

        // Assert
        Assert.IsNotNull(actionResult);
        var okObjectResult = actionResult as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okObjectResult.StatusCode);
        var subsidiaryDetailResponse = okObjectResult.Value as SubsidiaryDetailsResponse;
        subsidiaryDetailResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public async Task GetSubsidiaryDetailsAsync_WhenExceptionThrown_ReturnsProblemResponse()
    {
        // Arrange
        var subRequest = GetSubsidiaryDetailsRequest();
        var requestMock = GetMockHttpRequest(subRequest);

        _subsidiaryDataServiceMock
            .Setup(service => service.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .Throws<Exception>();

        var sut = new SubsidiaryFunctions(_subsidiaryDataServiceMock.Object, _loggerMock.Object);

        // Act
        var actionResult = await sut.GetSubsidiaryDetailsAsync(requestMock.Object);

        // Assert
        Assert.IsNotNull(actionResult);
        var objectResult = actionResult as ObjectResult;
        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }

    [TestMethod]
    public async Task GetSubsidiaryDetailsAsync_WhenInvalidRequest_ReturnsProblemResponse()
    {
        // Arrange
        var subRequest = GetSubsidiaryDetailsRequest();
        var requestMock = GetMockHttpRequest(subRequest);
        _subsidiaryDataServiceMock
            .Setup(service => service.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync((SubsidiaryDetailsResponse)null);

        var sut = new SubsidiaryFunctions(_subsidiaryDataServiceMock.Object, _loggerMock.Object);

        // Act
        var actionResult = await sut.GetSubsidiaryDetailsAsync(requestMock.Object);

        // Assert
        Assert.IsNotNull(actionResult);
        var objectResult = actionResult as ObjectResult;
        Assert.AreEqual(StatusCodes.Status404NotFound, objectResult.StatusCode);
    }

    private static SubsidiaryDetailsRequest GetSubsidiaryDetailsRequest()
    {
        return new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new()
            {
                new()
                {
                    OrganisationReference = "OrgRef",
                    SubsidiaryDetails = new()
                    {
                        new() { ReferenceNumber = "Sub1Ref" }
                    }
                }
            }
        };
    }

    private static Mock<HttpRequest> GetMockHttpRequest(SubsidiaryDetailsRequest subRequest)
    {
        var requestMock = new Mock<HttpRequest>();
        var jsonPayload = JsonSerializer.Serialize(subRequest);
        var requestBodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var requestBodyStream = new MemoryStream(requestBodyBytes);
        requestMock.Setup(req => req.Body).Returns(requestBodyStream);

        return requestMock;
    }
}