using BackendAccountService.Core.Models;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class RegulatorServiceWithOrganisationTypeTests
{
    private AccountsDbContext _dbContext;
    private RegulatorService _regulatorService;
    private OrganisationService _organisationService;
    private Mock<ILogger<RegulatorService>> _logger = null;
    private Mock<ITokenService> _tokenService;
    private static readonly Guid ComplianceSchemeOrg = new Guid("00000000-0000-0000-0000-000000000010");
    private static readonly Guid DirectProducerOrg = new Guid("11111111-0000-0000-0000-000000000001");
    private static readonly Guid IndirectProducerOrg = new Guid("22222222-0000-0000-0000-000000000001");
    
    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("AccountsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _tokenService = new Mock<ITokenService>();
        _logger = new Mock<ILogger<RegulatorService>>();
        
        _dbContext = new AccountsDbContext(contextOptions);
        _organisationService = new OrganisationService(_dbContext);
        _regulatorService = new RegulatorService(_dbContext, _organisationService, _tokenService.Object, _logger.Object);
    }
    
    [TestMethod]
    public async Task When_Organisation_No_match_Then_Return_Object_With_OrganisationType_As_Direct_producer()
    {
        // act 
        var organisationData = await _regulatorService.GetCompanyDetailsById(ComplianceSchemeOrg);
        
        // Assert
        organisationData.Should().NotBeNull();
        organisationData.Company.Should().NotBeNull();
        organisationData.Company.OrganisationType.Should().Be(OrganisationSchemeType.ComplianceScheme.ToString());
    }
    
    [TestMethod]
    public async Task When_IsCompliance_Is_True_For_Organisation_Then_Return_Object_With_OrganisationType_As_Compliance_scheme()
    {
        // act 
        var organisationData = await _regulatorService.GetCompanyDetailsById(DirectProducerOrg);
        
        // Assert
        organisationData.Should().NotBeNull();
        organisationData.Company.Should().NotBeNull();
        organisationData.Company.OrganisationType.Should().Be(OrganisationSchemeType.DirectProducer.ToString());
        
    }
    
    [TestMethod]
    public async Task When_Organisation_Has_match_Via_OrganisationConnectionTable_Then_Return_Object_With_OrganisationType_As_Indirect_producer()
    {
        // act 
        var organisationData = await _regulatorService.GetCompanyDetailsById(IndirectProducerOrg);
        
        // Assert
        organisationData.Should().NotBeNull();
        organisationData.Company.Should().NotBeNull();
        organisationData.Company.OrganisationType.Should().Be(OrganisationSchemeType.InDirectProducer.ToString());
    }

    private static void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);
        
        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();
        
          /* SET UP ORGANISATIONS*/
        var complianceSchemeOrg = new Organisation
        {
            Name = "Test Compliance Scheme Org",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = ComplianceSchemeOrg,
            IsComplianceScheme = true,
            CompaniesHouseNumber = "CS123456",
            ReferenceNumber = "3000000",
            NationId = 1
        };
        
        var directProducerOrgWithChn = new Organisation
        {
            Name = "Test org 3",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = DirectProducerOrg,
            IsComplianceScheme = false,
            CompaniesHouseNumber = "DP123457",
            ReferenceNumber = "5000000",
            NationId = 1
        };
        
        var indirectProducerOrg = new Organisation
        {
            Name = "Producer Indirect Org",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = IndirectProducerOrg,
            IsComplianceScheme = false,
            CompaniesHouseNumber = "IDP1234745",
            ReferenceNumber = "6000000",
            NationId = 1,
            Id = 458569
        };
        
        setupContext.Organisations.Add(complianceSchemeOrg);
        setupContext.Organisations.Add(directProducerOrgWithChn);
        setupContext.Organisations.Add(indirectProducerOrg);
        
        /* SET UP COMPLIANCE SCHEME */
        var complianceScheme1 = new ComplianceScheme
        {
            Name = $"Compliance scheme {Guid.NewGuid()}",
            ExternalId = new Guid("22222222-0000-0000-0000-000000000000"),
            CompaniesHouseNumber = "CS123456",
            Id = 5236
        };
        setupContext.ComplianceSchemes.Add(complianceScheme1);
        
        /* SET UP ORGANISATION CONNECTION */
        var organisationConnection = new OrganisationsConnection
        {
            FromOrganisationId = 458569,
            ToOrganisation = indirectProducerOrg,
            ToOrganisationId = 5236,
            ExternalId = new Guid("33333333-0000-0000-0000-000000000001"),
            Id = 84752562
        };
        
        setupContext.OrganisationsConnections.Add(organisationConnection);
        
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
}