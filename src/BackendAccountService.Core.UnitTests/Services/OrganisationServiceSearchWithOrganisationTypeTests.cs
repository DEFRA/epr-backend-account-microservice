using BackendAccountService.Core.Extensions;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class OrganisationServiceSearchWithOrganisationTypeTests
{
    private AccountsDbContext _accountContext= null!;
    private OrganisationService _organisationService = null!;
    
    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _accountContext = new AccountsDbContext(contextOptions);
        _organisationService = new OrganisationService(_accountContext);
        SetUpDatabase(_accountContext);
    }

    [TestMethod]
    public async Task WhenIsComplianceSchemeTrue_Then_OrganisationSchemeTypeId_Returns_ComplianceScheme()
    {
        var query = "Test Compliance Scheme Org";
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);
        results.Items.Count.Should().Be(1);
        results.Items.Should().Contain(item => item.OrganisationName == "Test Compliance Scheme Org");
        results.Items.Should().Contain(item => item.OrganisationType == OrganisationSchemeType.ComplianceScheme.ToString());
    }
    
    [TestMethod]
    public async Task WhenIsComplianceSchemeFalse_AndOrganisationConnectionReturnNoRows_Then_OrganisationSchemeTypeId_Returns_DirectProducer()
    {
        var query = "Direct Producer Org";
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);
        results.Items.Count.Should().Be(1);
        results.Items.Should().Contain(item => item.OrganisationName == "Direct Producer Org");
        results.Items.Should().Contain(item => item.OrganisationType == OrganisationSchemeType.DirectProducer.ToString());
    }
    
    [TestMethod]
    public async Task WhenIsComplianceSchemeFalse_AndOrganisationConnectionReturnRows_Then_OrganisationSchemeTypeId_Returns_IndirectProducer()
    {
        var query = "Producer Indirect Org";
        var results = await _organisationService.GetOrganisationsBySearchTerm(query, Data.DbConstants.Nation.England, 3, 1);
        results.Items.Count.Should().Be(1);
        results.Items.Should().Contain(item => item.OrganisationName == "Producer Indirect Org");
        results.Items.Should().Contain(item => item.OrganisationType == OrganisationSchemeType.InDirectProducer.ToString());
        
    }

    private static void SetUpDatabase(AccountsDbContext setupContext)
    {
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();
        /* SET UP ORGANISATIONS*/
        var complianceSchemeOrg = new Organisation
        {
            Name = "Test Compliance Scheme Org",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("11111111-0000-0000-0000-000000000001"),
            IsComplianceScheme = true,
            CompaniesHouseNumber = "CS123456",
            ReferenceNumber = "3000000",
            NationId = 1
        };
        
        var directProducerOrgWithNoCompaniesHouseNumber = new Organisation
        {
            Name = "Direct Producer Org",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("11111111-0000-0000-0000-000000000001"),
            IsComplianceScheme = false,
            CompaniesHouseNumber = null,
            ReferenceNumber = "4000000",
            NationId = 1
        };
        
        var directProducerOrgWithChn = new Organisation
        {
            Name = "Test org 3",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("11111111-0000-0000-0000-000000000001"),
            IsComplianceScheme = false,
            CompaniesHouseNumber = "DP123457",
            ReferenceNumber = "5000000",
            NationId = 1
        };
        
        var indirectProducerOrg = new Organisation
        {
            Name = "Producer Indirect Org",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ExternalId = new Guid("11111111-0000-0000-0000-000000000001"),
            IsComplianceScheme = false,
            CompaniesHouseNumber = "IDP1234745",
            ReferenceNumber = "6000000",
            NationId = 1,
            Id = 458569
        };
        
        setupContext.Organisations.Add(complianceSchemeOrg);
        setupContext.Organisations.Add(directProducerOrgWithNoCompaniesHouseNumber);
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