using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace BackendAccountService.Api.UnitTests.Controllers;

[TestClass]
public class ReprocessorExporterControllerTests
{
    private Mock<IOptions<ApiConfig>> _baseApiConfigOptionsMock = new Mock<IOptions<ApiConfig>>();
    private Mock<IReprocessorExporterService> _reprocessorExporterServiceMock;
    private ReprocessorExporterController _controller;
    private Mock<IValidator<PersonsDetailsRequestDto>> _validatorMock;

    [TestInitialize]
    public void SetUp()
    {
        _reprocessorExporterServiceMock = new Mock<IReprocessorExporterService>();
        _validatorMock = new Mock<IValidator<PersonsDetailsRequestDto>>();

        _baseApiConfigOptionsMock.Setup(x => x.Value).Returns(new ApiConfig());

        _controller = new ReprocessorExporterController(_baseApiConfigOptionsMock.Object, _reprocessorExporterServiceMock.Object, _validatorMock.Object);
    }

    [TestMethod]
    public async Task GetNationDetailsByNationId_ReturnsOk_WithExpectedResult()
    {
        // Arrange
        var nationId = 1; 
        var expectedNationDetails = new NationDetailsResponseDto(); 

        _reprocessorExporterServiceMock
            .Setup(s => s.GetNationDetailsByNationId(nationId))
            .ReturnsAsync(expectedNationDetails);

        // Act
        var result = await _controller.GetNationDetailsByNationId(nationId);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expectedNationDetails);
    }


    [TestMethod]
    public async Task GetOrganisationDetailsByOrgId_ReturnsOk_WithExpectedResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var expectedOrganisationDetails = new OrganisationDetailsResponseDto();

        _reprocessorExporterServiceMock
            .Setup(s => s.GetOrganisationDetailsByOrgId(organisationId))
            .ReturnsAsync(expectedOrganisationDetails);

        // Act
        var result = await _controller.GetOrganisationDetailsByOrgId(organisationId);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expectedOrganisationDetails);
    }

    [TestMethod]
    public async Task GetPersonDetailsByIds_ReturnsOk_WithExpectedResult()
    {
        // Arrange
        var requestDto = new PersonsDetailsRequestDto
        {
            OrgId = Guid.NewGuid(),
            UserIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() }
        };
        var expectedPersonDetails = new List<OrganisationPersonDto>();

        _validatorMock
            .Setup(v => v.ValidateAsync(requestDto, default))
            .ReturnsAsync(new ValidationResult());

        _reprocessorExporterServiceMock
            .Setup(s => s.GetPersonDetailsByIds(requestDto))
            .ReturnsAsync(expectedPersonDetails);

        // Act
        var result = await _controller.GetPersonDetailsByIds(requestDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expectedPersonDetails);
    }

    [TestMethod]
    public async Task GetPersonDetailsByIds_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var validator = new InlineValidator<PersonsDetailsRequestDto>();
        validator.RuleFor(x => x.OrgId).Must(_ => false).WithMessage("Validation failed");

        var requestDto = new PersonsDetailsRequestDto
        {
            OrgId = Guid.NewGuid(),
            UserIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() }
        };

        var controller = new ReprocessorExporterController(
            _baseApiConfigOptionsMock.Object, 
            _reprocessorExporterServiceMock.Object,
            validator
        );

        // Act & Assert
        await FluentActions.Invoking(() =>
            controller.GetPersonDetailsByIds(requestDto)
        ).Should().ThrowAsync<ValidationException>();
    }
}