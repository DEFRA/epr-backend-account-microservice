namespace BackendAccountService.Api.UnitTests.Helpers;

using System.Net;
using Api.Helpers;
using AutoFixture;
using AutoFixture.AutoMoq;
using Core.Models.Result;
using Microsoft.AspNetCore.Mvc;

[TestClass]
public class ResultHelpersTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    
    [TestMethod]
    public void Build_error_response_should_return_bad_request_result_and_error_when_bad_request_failure()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var result = Result<string>.FailedResult(errorMessage, HttpStatusCode.BadRequest);
        
        // Act
        var response = result.BuildErrorResponse();
        
        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        response.As<BadRequestObjectResult>().Value.Should().Be(errorMessage);
    }
    
    [TestMethod]
    public void Build_error_response_should_return_not_found_result_and_error_when_not_found_failure()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var result = Result<string>.FailedResult(errorMessage, HttpStatusCode.NotFound);
        
        // Act
        var response = result.BuildErrorResponse();
        
        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        response.As<NotFoundObjectResult>().Value.Should().Be(errorMessage);
    }
    
    [TestMethod]
    public void Build_response_should_return_internal_server_error_when_internal_server_error_failure()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var result = Result<string>.FailedResult(errorMessage, HttpStatusCode.InternalServerError);
        
        // Act
        var response = result.BuildErrorResponse();
        
        // Assert
        response.Should().BeOfType<StatusCodeResult>();
        response.As<StatusCodeResult>().StatusCode.Should().Be((int) HttpStatusCode.InternalServerError);
    }
    
    [TestMethod]
    [DataRow(HttpStatusCode.Ambiguous)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [DataRow(HttpStatusCode.AlreadyReported)]
    [DataRow(HttpStatusCode.BadGateway)]
    public void Build_response_should_return_internal_server_error_when_unmapped_code_failure(HttpStatusCode statusCode)
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var result = Result<string>.FailedResult(errorMessage, statusCode);
        
        // Act
        var response = result.BuildErrorResponse();
        
        // Assert
        response.Should().BeOfType<StatusCodeResult>();
        response.As<StatusCodeResult>().StatusCode.Should().Be((int) HttpStatusCode.InternalServerError);
    }
}