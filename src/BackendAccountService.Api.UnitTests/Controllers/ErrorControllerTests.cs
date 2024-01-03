using BackendAccountService.Api.Configuration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class ErrorControllerTests
{
    private Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
    
    [TestInitialize]
    public void Setup()
    {
        _apiConfigOptionsMock = new Mock<IOptions<ApiConfig>>();

        _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });
    }
    
    [TestMethod]
    public void ErrorsController_WhenErrorWithoutExceptionHandlerFeatureIsTriggered_ThenReturnProblem()
    {
        var controller = new ErrorsController(_apiConfigOptionsMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        
        var problem = controller.HandleError() as ObjectResult;
        
        problem.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        
        var problemDetails = problem.Value as ProblemDetails;

        problemDetails.Title.Should().Be("Unhandled exception");
        problemDetails.Type.Should().Be("https://dummytest/internalservererror");
        problemDetails.Status.Should().Be(500);
    }

    [TestMethod]
    public void ErrorsController_WhenErrorWithExceptionHandlerFeatureIsTriggered_ThenReturnProblem()
    {
        var controller = new ErrorsController(_apiConfigOptionsMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        
        var exceptionHandlerFeature = new ExceptionHandlerFeature()
        {
            Error = new Exception("Some Error")
        };
        
        controller.HttpContext.Features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);

        var problem = controller.HandleError() as ObjectResult;
        
        problem.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        
        var problemDetails = problem.Value as ProblemDetails;

        problemDetails.Detail.Should().Be("Some Error");
        problemDetails.Type.Should().Be("https://dummytest/exception");
        problemDetails.Status.Should().Be(500);
    }
    
}