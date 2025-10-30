using AutoMapper;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Profiles;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Repositories;
using Moq;
using System;
using System.Threading.Tasks;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class ReprocessorExporterServiceTests
{
    private Mock<IReprocessorExporterRepository> _reprocessorExporterRepositoryMock;
    private IMapper _mapper;
    private ReprocessorExporterService _service;

    [TestInitialize]
    public void SetUp()
    {
        _reprocessorExporterRepositoryMock = new Mock<IReprocessorExporterRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ReprocessorExporterProfile>();
        });

        _mapper = config.CreateMapper();

        _service = new ReprocessorExporterService(_reprocessorExporterRepositoryMock.Object, _mapper);
    }

    [TestMethod]
    public async Task GetNationDetailsByNationId_ReturnsMappedNationDetails_WhenDataExists()
    {
        // Arrange
        var nationId = 1;  
        var mockEntity = new Nation  
        {
            Id = nationId,
            Name = "England",
            NationCode = "GB-ENG"
        };

        _reprocessorExporterRepositoryMock
            .Setup(repo => repo.GetNationDetailsByNationId(nationId))
            .ReturnsAsync(mockEntity);

        // Act
        var result = await _service.GetNationDetailsByNationId(nationId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("England");
        result.NationCode.Should().Be("GB-ENG");
    }

    [TestMethod]
    public async Task GetNationDetailsByNationId_ReturnsNull_WhenNoDataFound()
    {
        // Arrange
        var nationId = 999;  

        _reprocessorExporterRepositoryMock
            .Setup(repo => repo.GetNationDetailsByNationId(nationId))
            .ReturnsAsync((Nation)null);

        // Act
        var result = await _service.GetNationDetailsByNationId(nationId);

        // Assert
        result.Should().BeNull();  
    }

    [TestMethod]
    public async Task GetOrganisationDetailsByOrgId_ReturnsMappedOrganisationDetails_WhenDataExists()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var mockEntity = new Organisation
        {
            ExternalId = organisationId,
            Name = "Test Organisation",
            TradingName = "Test Trading Name",
            CompaniesHouseNumber = "Test Company House Number",
            BuildingNumber = "1",
            BuildingName = "Test Building Name",
            SubBuildingName = "Test SubBuilding Name",
            Street = "Test Street",
            Locality = "Test Locality",
            Town = "Test Town",
            Country = "Test Country",
            Postcode = "Test Postcode",
            OrganisationType = new OrganisationType
            {
                Name = "Test Organisation Type"
            },
            PersonOrganisationConnections = new[]
            {
                new PersonOrganisationConnection{
                    JobTitle = "Test Job Title",
                    Person = new()
                    {
                        FirstName = "Test First Name",
                        LastName = "Test Last Name",
                        Email = "Test Email",
                        Telephone = "Test Telephone",
                        ExternalId = Guid.NewGuid()
                    }
               }
            }
        };

        _reprocessorExporterRepositoryMock
            .Setup(repo => repo.GetOrganisationDetailsByOrgId(organisationId))
            .ReturnsAsync(mockEntity);

        // Act
        var result = await _service.GetOrganisationDetailsByOrgId(organisationId);

        // Assert
        result.Should().NotBeNull();
        result.OrganisationName.Should().Be("Test Organisation");
        result.TradingName.Should().Be("Test Trading Name");
        result.OrganisationType.Should().Be("Test Organisation Type");
    }

    [TestMethod]
    public async Task OrganisationDetailsResponseDto_ReturnsNull_WhenNoDataFound()
    {
        // Arrange
        var organisationId = Guid.Empty;

        _reprocessorExporterRepositoryMock
            .Setup(repo => repo.GetOrganisationDetailsByOrgId(organisationId))
            .ReturnsAsync((Organisation)null);

        // Act
        var result = await _service.GetOrganisationDetailsByOrgId(organisationId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetPersonDetailsByIds_ReturnsMappedPersonDetails_WhenDataExists()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var requestDto = new Models.PersonsDetailsRequestDto
        {
            OrgId = organisationId,
            UserIds = new List<Guid>() { Guid.Parse("5a562763-7a07-4290-bad9-a42ad1e6cc18"), Guid.Parse("0293d53c-2c6c-4ea0-bd64-0f4b386e03f5") }
        };
        var mockEntity = new List<PersonOrganisationConnection>()
            {
                new PersonOrganisationConnection{
                    Id = 10,
                    JobTitle = "Test Job Title",
                    Person = new()
                    {
                        Id = 8,
                        FirstName = "Test First Name",
                        LastName = "Test Last Name",
                        Email = "Test Email",
                        Telephone = "Test Telephone",
                        User = new User
                        {
                           UserId = Guid.Parse("5a562763-7a07-4290-bad9-a42ad1e6cc18")
                        }
                    }
               },
               new PersonOrganisationConnection{
                    Id = 20,
                    JobTitle = "Test Job Title 2",
                    Person = new()
                    {
                        Id = 12,
                        FirstName = "Test First Name 2",
                        LastName = "Test Last Name 2",
                        Email = "Test Email 2",
                        Telephone = "Test Telephone 2",
                        User = new User
                        {
                           UserId = Guid.Parse("0293d53c-2c6c-4ea0-bd64-0f4b386e03f5")
                        }
                    }
               }
            };

        _reprocessorExporterRepositoryMock
            .Setup(repo => repo.GetPersonDetailsByIds(requestDto.OrgId, requestDto.UserIds))
            .ReturnsAsync(mockEntity);

        // Act
        var result = await _service.GetPersonDetailsByIds(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetPersonDetailsByIds_ReturnsNull_WhenNoDataFound()
    {
        // Arrange
        var requestDto = new Models.PersonsDetailsRequestDto
        {
            OrgId = Guid.Empty,
            UserIds = new List<Guid>()
        };

        _reprocessorExporterRepositoryMock
            .Setup(repo => repo.GetPersonDetailsByIds(requestDto.OrgId, requestDto.UserIds))
            .ReturnsAsync((List<PersonOrganisationConnection>)null);

        // Act
        var result = await _service.GetPersonDetailsByIds(requestDto);

        // Assert
        result.Should().HaveCount(0);
    }
}