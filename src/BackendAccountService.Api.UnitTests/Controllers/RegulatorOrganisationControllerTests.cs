using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers
{
    [TestClass]
    public class RegulatorOrganisationControllerTests
    {
        private RegulatorOrganisationController _regulatorOrganisationController = null!;
        private Mock<IRegulatorOrganisationService> _regulatorOrganisationServiceMock = null!;
        private Mock<IRegulatorService> _regulatorServiceMock = null!;
        private Mock<ILogger<RegulatorOrganisationController>> _logger = null;
        private Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = null!;

        [TestInitialize]
        public void Setup()
        {
            _regulatorOrganisationServiceMock = new Mock<IRegulatorOrganisationService>();
            _regulatorServiceMock = new Mock<IRegulatorService>();

            _logger = new Mock<ILogger<RegulatorOrganisationController>>();

            _apiConfigOptionsMock = new Mock<IOptions<ApiConfig>>();

            _apiConfigOptionsMock
            .Setup(x => x.Value)
            .Returns(new ApiConfig
            {
                BaseProblemTypePath = "https://dummytest/"
            });

            _regulatorOrganisationController = new RegulatorOrganisationController(_regulatorOrganisationServiceMock.Object, _regulatorServiceMock.Object, _apiConfigOptionsMock.Object, _logger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [TestMethod]
        public async Task GetRegulatorOrganisationByNation_WhenRecordNotExist_ReturnOkWithResultValueToBeNull()
        {
            // Arrange
            _regulatorOrganisationServiceMock
                .Setup(service => service.GetRegulatorOrganisationByNationAsync(It.IsAny<string>()))
                .ReturnsAsync((CheckOrganisationExistResponseModel?)null);

            // Act
            var result = await _regulatorOrganisationController
                .GetOrganisationIdFromNation(It.IsAny<string>()) as OkObjectResult;

            // Assert
            result.Value.Should().BeNull();
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task GetRegulatorOrganisationByNation_WhenValidNation_ReturnOk()
        {
            // Arrange
            _regulatorOrganisationServiceMock
                .Setup(service => service.GetRegulatorOrganisationByNationAsync(It.IsAny<string>()))
                .ReturnsAsync(new CheckOrganisationExistResponseModel
                {
                    CreatedOn = DateTime.Now,
                    ExternalId = Guid.NewGuid(),
                });

            // Act
            var result = await _regulatorOrganisationController
                .GetOrganisationIdFromNation(It.IsAny<string>()) as OkObjectResult;

            // Assert
            result.Value.Should().NotBeNull();
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task CreateNewRegulatorOrganisationAsync_WhenOrganisationAlreadyExist_ReturnBadRequest()
        {
            // Arrange
            var request = new CreateRegulatorOrganisationRequest
            {
                Name = "Test",
                NationId = 1,
                ServiceId = "Regulating"
            };

            _regulatorOrganisationServiceMock
                .Setup(service => service
                    .CreateNewRegulatorOrganisationAsync(It.IsAny<CreateRegulatorOrganisationRequest>()))
                .ReturnsAsync(Result<CreateRegulatorOrganisationResponse>.FailedResult(
                    $"Error: regulator account with name {request.Name} already exists",
                                HttpStatusCode.BadRequest));

            // Act
            var result = await _regulatorOrganisationController
                .CreateRegulatorOrganisation(request) as BadRequestObjectResult;

            // Assert            
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task CreateNewRegulatorOrganisationAsync_WhenOrganisationIsValid_ReturnCreated()
        {
            // Arrange
            var request = new CreateRegulatorOrganisationRequest
            {
                Name = "Test",
                NationId = 1,
                ServiceId = "Regulating"
            };

            _regulatorOrganisationServiceMock
                .Setup(service => service.
                    CreateNewRegulatorOrganisationAsync(It.IsAny<CreateRegulatorOrganisationRequest>()))
                .ReturnsAsync(Result<CreateRegulatorOrganisationResponse>
                    .SuccessResult(new CreateRegulatorOrganisationResponse
                    {
                        ExternalId = Guid.NewGuid(),
                        Nation = "England",
                    }));

            // Act
            var result = await _regulatorOrganisationController
                .CreateRegulatorOrganisation(request) as CreatedAtRouteResult;

            // Assert            
            result.StatusCode.Should().Be((int)HttpStatusCode.Created);
        }
        
        [TestMethod]
        public async Task GetNationIdFromOrganisationId_WhenOrganisationNotExist_ReturnOkWithResultValueToBeZero()
        {
            // Arrange
            _regulatorServiceMock
                .Setup(service => service.GetOrganisationNationIds(It.IsAny<Guid>()))
                .ReturnsAsync(new List<int>{0});

            // Act
            var result = await _regulatorOrganisationController
                .GetNationIdsFromOrganisationId(It.IsAny<Guid>()) as OkObjectResult;

            // Assert
            (result.Value as List<int>).Should().BeEquivalentTo(new List<int>{0});
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
        
        [TestMethod]
        public async Task GetRegulatorOrganisationByNation_WhenOrganisationExists_ReturnOk()
        {
            // Arrange
            _regulatorServiceMock.Setup(service => service.GetOrganisationNationIds(It.IsAny<Guid>()))
                .ReturnsAsync(new List<int>());
            
            // Act
            var result = await _regulatorOrganisationController
                .GetNationIdsFromOrganisationId(It.IsAny<Guid>()) as OkObjectResult;

            // Assert
            result.Value.Should().NotBeNull();
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}
