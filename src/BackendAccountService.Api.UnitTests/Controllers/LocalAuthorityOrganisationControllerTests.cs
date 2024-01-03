using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.UnitTests.Controllers
{
    [TestClass]
    public class LocalAuthorityOrganisationControllerTests
    {
        private LocalAuthorityOrganisationController _laOrganisationsController = null!;
        private readonly Mock<ILocalAuthorityService> _localAuthorityServiceMock = new();
        private readonly Mock<IOptions<ApiConfig>> _apiConfigOptionsMock = new();
        private readonly NullLogger<LocalAuthorityOrganisationController> _nullLogger = new();

        [TestInitialize]
        public void Setup()
        {
            _apiConfigOptionsMock
                .Setup(x => x.Value)
                .Returns(new ApiConfig
                {
                    BaseProblemTypePath = "https://dummytest/"
                });

            _laOrganisationsController = new LocalAuthorityOrganisationController(
                _localAuthorityServiceMock.Object,
                _apiConfigOptionsMock.Object,
                _nullLogger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [TestMethod]
        public async Task
            CreateNewLocalAuthorityOrganisation_WhenInvalidRequest_ReturnsException()
        {
            // Arrange
            var newRecord = new CreateLocalAuthorityRequest
            {
                UserId = new Guid("4a100496-02b0-4ab5-9c31-17b9ab562e6d"),
                WasteAuthorityType = "Limited Company",
                Name = "Testing LA Organisation 001",
                TradingName = "Testing TN LA Organisation 001",
                ReferenceNumber = "001",
                BuildingName = "The Towers",
                BuildingNumber = "152",
                Street = "The Street",
                Locality = "Nowhere",
                DependentLocality = "Dreams",
                Town = "No Town",
                County = "No County",
                Country = "No Country",
                Postcode = "AS34RG",
                ValidatedWithCompaniesHouse = true,
                IsComplianceScheme = true,
                Nation = "Not Set"
            };

            var resultRecord = new LocalAuthorityResponseModel { ModifiedOn = DateTimeOffset.Now };

            var message = "Local Authority or Organisation invalid request";
            _localAuthorityServiceMock
                .Setup(service =>
                    service.CreateNewLocalAuthorityOrganisationAsync(It.IsAny<CreateLocalAuthorityRequest>()))
                .ReturnsAsync(
                    new Result<LocalAuthorityResponseModel>(false, resultRecord, message,
                        HttpStatusCode.InternalServerError));

            // Act
            var result =
                await _laOrganisationsController.CreateNewLocalAuthorityOrganisation(newRecord) as
                    StatusCodeResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod]
        public async Task CreateNewLocalAuthorityOrganisation_WhenLocalAuthorityNotPresent_ReturnsOk()
        {
            // Arrange
            var newRecord = new CreateLocalAuthorityRequest
            {
                UserId = new Guid("4a100496-02b0-4ab5-9c31-17b9ab562e6d"),
                DistrictCode = "DDD333444",
                WasteAuthorityType = "Limited Company",
                Name = "Testing LA Organisation 001",
                TradingName = "Testing TN LA Organisation 001",
                ReferenceNumber = "001",
                BuildingName = "The Towers",
                BuildingNumber = "152",
                Street = "The Street",
                Locality = "Nowhere",
                DependentLocality = "Dreams",
                Town = "No Town",
                County = "No County",
                Country = "No Country",
                Postcode = "AS34RG",
                ValidatedWithCompaniesHouse = true,
                IsComplianceScheme = true,
                Nation = "Not Set"
            };

            var resultRecord = new LocalAuthorityResponseModel
            {
                ModifiedOn = DateTimeOffset.Now,
                ExternalId = new Guid("12345678-1234-1234-1234-123456789012"),
                DistrictCode = "DDD333444",
                CompaniesHouseNumber = "",
                Name = "Testing LA Organisation 001",
                TradingName = "Testing TN LA Organisation 001",
                ReferenceNumber = "001",
                Address = new AddressModel
                {
                    BuildingName = "The Towers",
                    BuildingNumber = "152",
                    Street = "The Street",
                    Locality = "Nowhere",
                    DependentLocality = "Dreams",
                    Town = "No Town",
                    County = "No Country",
                    Country = "No County",
                    Postcode = "AS34RG"
                },
                ValidatedWithCompaniesHouse = true,
                IsComplianceScheme = true,
            };

            _localAuthorityServiceMock
                .Setup(service =>
                    service.CreateNewLocalAuthorityOrganisationAsync(It.IsAny<CreateLocalAuthorityRequest>()))
                .ReturnsAsync(
                    new Result<LocalAuthorityResponseModel>(true, resultRecord, "", HttpStatusCode.OK));

            // Act
            var result =
                await _laOrganisationsController.CreateNewLocalAuthorityOrganisation(newRecord) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status200OK);
            (result?.Value as LocalAuthorityResponseModel)?.Name.Should().Be(resultRecord.Name);
            (result?.Value as LocalAuthorityResponseModel)?.ExternalId.Should().NotBeEmpty();
            Guid.TryParse(((Result<LocalAuthorityResponseModel>)result?.Value).Value.ExternalId.ToString(), out _)
                .Should().BeTrue();
        }

        [TestMethod]
        public async Task GetAllLocalAuthorityOrganisation_WhenOrganisationExists_ReturnsOk()
        {
            // Arrange
            var laResponse = new List<LocalAuthorityResponseModel> { new LocalAuthorityResponseModel() };

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityOrganisationAsync())
                .ReturnsAsync(laResponse);

            // Act
            var resultList =
                await _laOrganisationsController.GetLocalAuthorityOrganisation() as
                    OkObjectResult;

            // Assert
            resultList.Should().NotBeNull();
            resultList?.StatusCode.Should().Be(StatusCodes.Status200OK);
            (resultList?.Value as List<LocalAuthorityResponseModel>)?.Count.Should().BeGreaterThan(0);
            (resultList?.Value as List<LocalAuthorityResponseModel>)[0].ExternalId.ToString().Should().NotBeEmpty();
            Guid.TryParse((resultList.Value as List<LocalAuthorityResponseModel>)[0].ExternalId.ToString(), out _)
                .Should().BeTrue();
        }

        [TestMethod]
        public async Task GetLocalAuthorityOrganisationByOrganisationTypeId_WhenOrganisationExists_ReturnsOk()
        {
            // Arrange
            var organisationType = 1;
            var laResponse = new List<LocalAuthorityResponseModel>();
            var resultRecord = new LocalAuthorityResponseModel
            {
                DistrictCode = "E01000101",
                Name = "Producer 1",
                ReferenceNumber = "100312",
                Address = new AddressModel
                {
                    Town = "London",
                    Postcode = "LN1 1LN"
                },
                ValidatedWithCompaniesHouse = false,
                IsComplianceScheme = false
            };
            laResponse.Add(resultRecord);

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityOrganisationByOrganisationTypeIdAsync(organisationType))
                .ReturnsAsync(laResponse);

            // Act
            var resultList =
                await _laOrganisationsController.GetLocalAuthorityOrganisationByOrganisationTypeId(organisationType) as
                    OkObjectResult;

            // Assert
            resultList.Should().NotBeNull();
            resultList?.StatusCode.Should().Be(StatusCodes.Status200OK);
            (resultList?.Value as List<LocalAuthorityResponseModel>)?.Count.Should().BeGreaterThan(0);
            (resultList?.Value as List<LocalAuthorityResponseModel>)?[0].DistrictCode.Should()
                .Be(resultRecord.DistrictCode);
            (resultList?.Value as List<LocalAuthorityResponseModel>)?[0].ReferenceNumber.Should()
                .Be(resultRecord.ReferenceNumber);
            (resultList?.Value as List<LocalAuthorityResponseModel>)[0].ExternalId.ToString().Should().NotBeEmpty();
            Guid.TryParse((resultList?.Value as List<LocalAuthorityResponseModel>)[0].ExternalId.ToString(), out _)
                .Should().BeTrue();
        }

        [TestMethod]
        public async Task GetLocalAuthorityOrganisationByOrganisationTypeId_WhenLocalAuthorityNotPresent_ReturnsNotFound()
        {
            // Arrange
            var organisationType = 9999;
            var laResponse = new List<LocalAuthorityResponseModel>();

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityOrganisationByOrganisationTypeIdAsync(organisationType))
                .ReturnsAsync(laResponse);

            // Act
            var resultList =
                await _laOrganisationsController.GetLocalAuthorityOrganisationByOrganisationTypeId(organisationType) as
                    NotFoundObjectResult;

            // Assert
            resultList.Should().NotBeNull();
            resultList?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            (resultList?.Value as List<LocalAuthorityResponseModel>)?.Count.Should().Be(0);
        }

        [TestMethod]
        public async Task GetLocalAuthorityOrganisationByOrganisationName_WhenOrganisationExists_ReturnsOk()
        {
            // Arrange
            var organisationName = " BBPA Environmental Ltd";
            var resultRecord = new LocalAuthorityResponseModel
            {

                ModifiedOn = DateTimeOffset.Now,
                DistrictCode = "S10000106",
                CompaniesHouseNumber = "9100134",
                Name = "BBPA Environmental Ltd",
                ReferenceNumber = "100317",
                Address = new AddressModel
                {
                    Town = "London",
                    Postcode = "LN1 1LN"
                },
                ValidatedWithCompaniesHouse = false,
                IsComplianceScheme = true
            };

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityByOrganisationNameAsync(organisationName))
                .ReturnsAsync(new Result<LocalAuthorityResponseModel>(true, resultRecord, "", HttpStatusCode.OK));

            // Act
            var result =
                await _laOrganisationsController.GetLocalAuthorityOrganisationByOrganisationName(organisationName) as
                    OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status200OK);
            (result?.Value as LocalAuthorityResponseModel)?.DistrictCode.Should().Be(resultRecord.DistrictCode);
            (result?.Value as LocalAuthorityResponseModel)?.CompaniesHouseNumber.Should()
                .Be(resultRecord.CompaniesHouseNumber);
            (result?.Value as LocalAuthorityResponseModel)?.Name.Should().Be(resultRecord.Name);
            (result?.Value as LocalAuthorityResponseModel)?.ExternalId.Should().NotBeEmpty();
            Guid.TryParse(((Result<LocalAuthorityResponseModel>)result?.Value).Value.ExternalId.ToString(), out _)
                .Should().BeTrue();
        }

        [TestMethod]
        public async Task GetLocalAuthorityOrganisationByOrganisationName_WhenLocalAuthorityNotPresent_ReturnsNotFound()
        {
            // Arrange
            var organisationName = "No Name";
            var resultRecord = new LocalAuthorityResponseModel { ModifiedOn = DateTimeOffset.Now };
            var message = $"No Local Authority / Organisation found with the organisation id: {organisationName}";

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityByOrganisationNameAsync(organisationName))
                .ReturnsAsync(
                    new Result<LocalAuthorityResponseModel>(false, resultRecord, message, HttpStatusCode.NotFound));

            // Act
            var result =
                await _laOrganisationsController.GetLocalAuthorityOrganisationByOrganisationName(organisationName) as
                    NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task GetLocalAuthorityOrganisationByDistrictCode_WhenOrganisationExists_ReturnsOk()
        {
            // Arrange
            var districtCode = "W01000104";
            var resultRecord = new LocalAuthorityResponseModel
            {
                ModifiedOn = DateTimeOffset.Now,
                DistrictCode = "W01000104",
                Name = "Producer 4",
                ReferenceNumber = "100315",
                Address = new AddressModel
                {
                    Town = "London",
                    Postcode = "LN1 1LN"
                },
                ValidatedWithCompaniesHouse = false,
                IsComplianceScheme = false
            };

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityByDistrictCodeAsync(districtCode))
                .ReturnsAsync(new Result<LocalAuthorityResponseModel>(true, resultRecord, "", HttpStatusCode.OK));

            // Act
            var result =
                await _laOrganisationsController.GetLocalAuthorityOrganisationByDistrictCode(districtCode) as
                    OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status200OK);
            (result?.Value as LocalAuthorityResponseModel)?.DistrictCode.Should().Be(resultRecord.DistrictCode);
            (result?.Value as LocalAuthorityResponseModel)?.ReferenceNumber.Should().Be(resultRecord.ReferenceNumber);
            (result?.Value as LocalAuthorityResponseModel)?.ExternalId.Should().NotBeEmpty();
            Guid.TryParse(((Result<LocalAuthorityResponseModel>)result?.Value).Value.ExternalId.ToString(), out _)
                .Should().BeTrue();
        }

        [TestMethod]
        public async Task GetLocalAuthorityOrganisationByDistrictCode_WhenLocalAuthorityNotPresent_ReturnsNotFound()
        {
            // Arrange
            var districtCode = "W01999104";
            var resultRecord = new LocalAuthorityResponseModel { ModifiedOn = DateTimeOffset.Now };
            var message = $"No Local Authority / Organisation found with the district code: {districtCode}";

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityByDistrictCodeAsync(districtCode))
                .ReturnsAsync(
                    new Result<LocalAuthorityResponseModel>(false, resultRecord, message, HttpStatusCode.NotFound));

            // Act
            var result =
                await _laOrganisationsController.GetLocalAuthorityOrganisationByDistrictCode(districtCode) as
                    NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task GetLocalAuthorityOrganisationByExternalId_WhenLocalAuthorityExists_ReturnsOk()
        {
            // Arrange
            var externalId = "CD7CFB62-1DBF-4111-9E3C-6DC176A640F0";
            var resultRecord = new LocalAuthorityResponseModel
            {
                ModifiedOn = DateTimeOffset.Now,
                DistrictCode = "S10000110",
                CompaniesHouseNumber = "5695937",
                Name = "Comply Direct Ltd",
                ReferenceNumber = "100321",
                Address = new AddressModel
                {
                    Town = "London",
                    Postcode = "LN1 1LN"
                },
                ValidatedWithCompaniesHouse = false,
                IsComplianceScheme = true
            };

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityOrganisationByExternalIdAsync(externalId))
                .ReturnsAsync(new Result<LocalAuthorityResponseModel>(true, resultRecord, "", HttpStatusCode.OK));

            // Act
            var result =
                await _laOrganisationsController
                    .GetLocalAuthorityOrganisationByExternalId(externalId) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status200OK);
            (result?.Value as LocalAuthorityResponseModel)?.DistrictCode.Should().Be(resultRecord.DistrictCode);
            (result?.Value as LocalAuthorityResponseModel)?.CompaniesHouseNumber.Should()
                .Be(resultRecord.CompaniesHouseNumber);
            (result?.Value as LocalAuthorityResponseModel)?.ExternalId.Should().NotBeEmpty();
            Guid.TryParse(((Result<LocalAuthorityResponseModel>)result?.Value).Value.ExternalId.ToString(), out _)
                .Should().BeTrue();
        }

        [TestMethod]
        public async Task GetLocalAuthorityOrganisationByExternalId_WhenLocalAuthorityNotPresent_ReturnsNotFound()
        {
            // Arrange
            var externalId = "11704899-C75E-4891-A3A1-28DD67C73902";
            var resultRecord = new LocalAuthorityResponseModel { ModifiedOn = DateTimeOffset.Now };
            var message = $"No Local Authority / Organisation found with the external id: {externalId}";

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityOrganisationByExternalIdAsync(externalId))
                .ReturnsAsync(
                    new Result<LocalAuthorityResponseModel>(false, resultRecord, message, HttpStatusCode.NotFound));

            // Act
            var result =
                await _laOrganisationsController.GetLocalAuthorityOrganisationByExternalId(externalId) as
                    NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task UpdateLocalAuthorityOrganisationByExternalId_WhenLocalAuthorityPresent_ReturnsNoContent()
        {
            // Arrange
            var externalId = "CD7CFB62-1DBF-4111-9E3C-6DC176A640F0";
            UpdateLocalAuthorityRequest request = new UpdateLocalAuthorityRequest
            {
                ExternalId = externalId,
                UserId = new Guid("4a100496-02b0-4ab5-9c31-17b9ab562e6d"),
                Name = "Testing Direct Ltd",
                Nation = "Not Set",
                WasteAuthorityType = "2",
                DistrictCode = "EEE666888",
            };

            var laResponse = Result<LocalAuthorityResponseModel>.SuccessResult(new LocalAuthorityResponseModel());

            _localAuthorityServiceMock
                .Setup(service => service.UpdateLocalAuthorityByExternalIdAsync(request))
                .ReturnsAsync(laResponse);

            // Act
            var result =
                await _laOrganisationsController.UpdateLocalAuthorityOrganisationByExternalId(request) as
                    NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [TestMethod]
        public async Task UpdateLocalAuthorityOrganisationByExternalId_WhenLocalAuthorityNotPresent_ReturnsException()
        {
            // Arrange
            var externalId = "7B071638-759F-4931-8B70-444444444444";
            UpdateLocalAuthorityRequest request = new UpdateLocalAuthorityRequest
            {
                ExternalId = externalId,
                UserId = new Guid("4a100496-02b0-4ab5-9c31-17b9ab562e6d"),
                Name = "Testing Direct Ltd",
                Nation = "Not Set",
                WasteAuthorityType = "2",
                DistrictCode = "EEE666888",
            };
            var resultRecord = new LocalAuthorityResponseModel { ModifiedOn = DateTimeOffset.Now };
            var message = $" No Local Authority / Organisation found with the external id {request.ExternalId}";

            _localAuthorityServiceMock
                .Setup(service => service.UpdateLocalAuthorityByExternalIdAsync(request))
                .ReturnsAsync(
                    new Result<LocalAuthorityResponseModel>(false, resultRecord, message, HttpStatusCode.NotFound));

            // Act
            var result =
                await _laOrganisationsController
                    .UpdateLocalAuthorityOrganisationByExternalId(request) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task UpdateLocalAuthorityOrganisationByDistrictCode_WhenLocalAuthorityPresent_ReturnsNoContent()
        {
            // Arrange
            var districtCode = "E01000101";
            UpdateLocalAuthorityRequest request = new UpdateLocalAuthorityRequest
            {
                ExternalId = "CD7CFB62-1DBF-4111-9E3C-6DC176A640F0",
                UserId = new Guid("4a100496-02b0-4ab5-9c31-17b9ab562e6d"),
                Name = "Testing Direct Ltd",
                Nation = "Not Set",
                WasteAuthorityType = "2",
                DistrictCode = districtCode
            };

            var laResponse = Result<LocalAuthorityResponseModel>.SuccessResult(new LocalAuthorityResponseModel());

            _localAuthorityServiceMock
                .Setup(service => service.UpdateLocalAuthorityByDistrictCodeAsync(request))
                .ReturnsAsync(laResponse);

            // Act
            var result =
                await _laOrganisationsController.UpdateLocalAuthorityOrganisationByDistrictCode(request) as
                    NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [TestMethod]
        public async Task UpdateLocalAuthorityOrganisationByDistrictCode_WhenLocalAuthorityNotPresent_ReturnsException()
        {
            // Arrange
            var districtCode = "AAA111222";
            UpdateLocalAuthorityRequest request = new UpdateLocalAuthorityRequest
            {
                ExternalId = "CD7CFB62-1DBF-4111-9E3C-6DC176A640F0",
                UserId = new Guid("4a100496-02b0-4ab5-9c31-17b9ab562e6d"),
                Name = "Testing Direct Ltd",
                Nation = "Not Set",
                WasteAuthorityType = "2",
                DistrictCode = districtCode
            };
            var resultRecord = new LocalAuthorityResponseModel { ModifiedOn = DateTimeOffset.Now };
            var message = $" No Local Authority / Organisation found with the district code {request.DistrictCode}";

            _localAuthorityServiceMock
                .Setup(service => service.UpdateLocalAuthorityByDistrictCodeAsync(request))
                .ReturnsAsync(
                    new Result<LocalAuthorityResponseModel>(false, resultRecord, message, HttpStatusCode.NotFound));

            // Act
            var result =
                await _laOrganisationsController.UpdateLocalAuthorityOrganisationByDistrictCode(request) as
                    NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task RemoveAuthorityOrganisationByExternalId_WhenLocalAuthorityPresent_ReturnsNoContent()
        {
            // Arrange
            var externalId = "E63F3DEF-C022-4AE4-9578-178CFD999258";
            RemoveLocalAuthorityRequest request = new RemoveLocalAuthorityRequest { ExternalId = externalId };

            _localAuthorityServiceMock
                .Setup(service => service.RemoveLocalAuthorityByExternalIdAsync(request))
                .ReturnsAsync(new Result(true, "", HttpStatusCode.NoContent));

            // Act
            var result =
                await _laOrganisationsController.RemoveAuthorityOrganisationByExternalId(request) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [TestMethod]
        public async Task RemoveAuthorityOrganisationByExternalId_WhenInvalidId_ReturnsNoContent()
        {
            // Arrange
            var externalId = "ffffff-0000-0000-0000-erererer";
            RemoveLocalAuthorityRequest request = new RemoveLocalAuthorityRequest { ExternalId = externalId };

            _localAuthorityServiceMock
                .Setup(service => service.RemoveLocalAuthorityByExternalIdAsync(request))
                .ReturnsAsync(new Result(true, "", HttpStatusCode.BadRequest));

            // Act
            var result =
                await _laOrganisationsController.RemoveAuthorityOrganisationByExternalId(request) as StatusCodeResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [TestMethod]
        public async Task RemoveAuthorityOrganisationByExternalId_WhenLocalAuthorityNotPresent_ReturnsException()
        {
            // Arrange
            var externalId = "7B071638-759F-4931-8B70-444444444444";
            RemoveLocalAuthorityRequest request = new RemoveLocalAuthorityRequest { ExternalId = externalId };
            var message = $" No Local Authority / Organisation found with the external id {request.ExternalId}";

            _localAuthorityServiceMock
                .Setup(service => service.RemoveLocalAuthorityByExternalIdAsync(request))
                .ReturnsAsync(new Result(false, message, HttpStatusCode.NotFound));

            // Act
            var result =
                await _laOrganisationsController.RemoveAuthorityOrganisationByExternalId(request) as
                    NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task RemoveAuthorityOrganisationByDistrictCode_WhenLocalAuthorityPresent_ReturnsNoContent()
        {
            // Arrange
            var districtCode = "E01000101";
            RemoveLocalAuthorityRequest request = new RemoveLocalAuthorityRequest { DistrictCode = districtCode };

            _localAuthorityServiceMock
                .Setup(service => service.RemoveLocalAuthorityByDistrictCodeAsync(request))
                .ReturnsAsync(new Result(true, "", HttpStatusCode.NoContent));

            // Act
            var result =
                await _laOrganisationsController.RemoveAuthorityOrganisationByDistrictCode(request) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [TestMethod]
        public async Task RemoveAuthorityOrganisationByDistrictCode_WhenLocalAuthorityNotPresent_ReturnsException()
        {
            // Arrange
            var districtCode = "AAA111222";
            RemoveLocalAuthorityRequest request = new RemoveLocalAuthorityRequest { DistrictCode = districtCode };
            var message = $" No Local Authority / Organisation found with the district code {request.DistrictCode}";

            _localAuthorityServiceMock
                .Setup(service => service.RemoveLocalAuthorityByDistrictCodeAsync(request))
                .ReturnsAsync(new Result(false, message, HttpStatusCode.NotFound));

            // Act
            var result =
                await _laOrganisationsController.RemoveAuthorityOrganisationByDistrictCode(request) as
                    NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task RemoveAuthorityOrganisationByDistrictCode_WhenInvalidCode_ReturnsException()
        {
            // Arrange
            var districtCode = "";
            RemoveLocalAuthorityRequest request = new RemoveLocalAuthorityRequest { DistrictCode = districtCode };
            var message = $" No Local Authority / Organisation found with the district code {request.DistrictCode}";

            _localAuthorityServiceMock
                .Setup(service => service.RemoveLocalAuthorityByDistrictCodeAsync(request))
                .ReturnsAsync(new Result(false, message, HttpStatusCode.NotFound));

            // Act
            var result =
                await _laOrganisationsController.RemoveAuthorityOrganisationByDistrictCode(request) as StatusCodeResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [TestMethod]
        public async Task GetNationByName_WhenNationExists_ReturnsOk()
        {
            // Arrange
            var nationName = "Northern Ireland";
           
            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityOrganisationNationAsync(nationName))
                .ReturnsAsync(new Result(true, "", HttpStatusCode.OK));

            // Act
            var result =
                await _laOrganisationsController.GetNation(nationName) as
                    OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task GetNationByName_WhenNationNotPresent_ReturnsNotFound()
        {
            // Arrange
            var nationName = "Malta";
            var message = $"No nation with name: {nationName} found";

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityOrganisationNationAsync(nationName))
                .ReturnsAsync(
                    new Result(false, message, HttpStatusCode.NotFound));

            // Act
            var result =
                await _laOrganisationsController.GetNation(nationName) as
                    NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
        
        [TestMethod]
        public async Task GetNationByName_WhenOrganisationTypeExists_ReturnsOk()
        {
            // Arrange
            var organisationTypeName = "Waste Collection Authority & Waste Disposal Authority";
           
            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityOrganisationByOrganisationTypeNameAsync(organisationTypeName))
                .ReturnsAsync(new Result(true, "", HttpStatusCode.OK));

            // Act
            var result =
                await _laOrganisationsController.OrganisationType(organisationTypeName) as
                    OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task GetNationByName_WhenOrganisationTypeNotPresent_ReturnsNotFound()
        {
            // Arrange
            var organisationTypeName = "Testing Type";
            var message = $"No organisation type with name: {organisationTypeName} found";

            _localAuthorityServiceMock
                .Setup(service => service.GetLocalAuthorityOrganisationByOrganisationTypeNameAsync(organisationTypeName))
                .ReturnsAsync(
                    new Result(false, message, HttpStatusCode.NotFound));

            // Act
            var result =
                await _laOrganisationsController.OrganisationType(organisationTypeName) as
                    NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}