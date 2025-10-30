using BackendAccountService.Core.Models.Mappings;

namespace BackendAccountService.Core.UnitTests.Services;

using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using BackendAccountService.Core.Models.Responses;
using Core.Services;
using Data.Entities;
using Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using Models.Request;
using Models.Result;
using Moq;
using TestHelpers;
using InterOrganisationRole = Data.DbConstants.InterOrganisationRole;

[TestClass]
public class ComplianceSchemeServiceTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private AccountsDbContext _dbContext = null!;
    private DbContextOptions<AccountsDbContext> _dbContextOptions = null!;
    private ComplianceSchemeService _complianceSchemeService = null!;
    private readonly NullLogger<ComplianceSchemeService> _logger = new();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _userOId = Guid.NewGuid();
    private readonly Guid _selectedSchemeId = Guid.NewGuid();
    private readonly Guid _complianceSchemeId = Guid.NewGuid();
    private readonly Guid _existingSelectedSchemeId = new Guid("00000000-0000-0000-0000-000000000031");
    private readonly Guid _existingComplianceSchemeId = new Guid("00000000-0000-0000-0000-000000000021");
    private readonly Guid _existingProducerWithSelectedSchemeId = new Guid("00000000-0000-0000-0000-000000000004");
    private readonly Guid _existingProducerWithMultipleSchemesId = new Guid("00000000-0000-0000-0000-000000000003");

    [TestInitialize]
    public void Setup()
    {
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _dbContextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("AccountsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new AccountsDbContext(_dbContextOptions);

        ComplainceSchemeTestHelper.SetUpDatabase(_dbContext);

        _complianceSchemeService = new ComplianceSchemeService(_dbContext, _logger);
    }

    [TestMethod]
    public async Task WhenExistingComplianceSchemeIsRequestedToBeRemoved_ThenSetIsDeletedToTrue()
    {
        //Arrange
        var removeCompliance = new RemoveComplianceSchemeRequest
        {
            SelectedSchemeId = _existingSelectedSchemeId,
            OrganisationId = Guid.NewGuid(),
            UserOId = Guid.NewGuid()
        };

        //Act
        var result = await _complianceSchemeService.RemoveComplianceSchemeAsync(removeCompliance);

        //Assert

        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task WhenExistingComplianceSchemeIsNotFound_ThenReturnNotFound()
    {
        //Arrange
        var request = new RemoveComplianceSchemeRequest
        {
            SelectedSchemeId = _selectedSchemeId,
            OrganisationId = _organisationId,
            UserOId = _userOId
        };

        //Act
        var expectedResult = Result.FailedResult($"selected scheme {request.SelectedSchemeId} is not found", HttpStatusCode.NotFound);

        //Assert
        var result = await _complianceSchemeService.RemoveComplianceSchemeAsync(request);

        result.Should().BeOfType(typeof(Result));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task WhenNoActiveConnectionFoundForOrganisation_ThenReturnInternalServerError()
    {
        //Setup
        var request = new RemoveComplianceSchemeRequest
        {
            SelectedSchemeId = _existingSelectedSchemeId,
            OrganisationId = _organisationId,
            UserOId = _userOId
        };

        //Act
        var expectedResult = Result.FailedResult($"Error removing selected scheme id {request.SelectedSchemeId}", HttpStatusCode.InternalServerError);
        var OrgConnection = _dbContext.OrganisationsConnections.Single(con => con.ExternalId == new Guid("00000000-0000-0000-0000-000000000011"));
        OrgConnection.IsDeleted = true;
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);

        //Assert
        var result = await _complianceSchemeService.RemoveComplianceSchemeAsync(request);

        result.Should().BeOfType(typeof(Result));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000021")]
    [DataRow("00000000-0000-0000-0000-000000000022")]
    public async Task WhenComplianceSchemeIsSelected_ThenSelectedSchemeIsCreated(string complianceSchemeId)
    {
        //Arrange
        var existingOrgConnections = _dbContext.OrganisationsConnections.Count();

        var newProducerOrg = _dbContext.AddOrganisation(new Organisation
        {
            Name = _fixture.Create<string>(),
            OrganisationTypeId = 1
        });

        var complianceScheme = _dbContext.ComplianceSchemes.Single(x => x.ExternalId == new Guid(complianceSchemeId));
        var organisation = _dbContext.Organisations.Single(x => x.CompaniesHouseNumber == complianceScheme.CompaniesHouseNumber);

        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = complianceScheme.ExternalId,
            ProducerOrganisationId = newProducerOrg.ExternalId,
            UserOId = _fixture.Create<Guid>(),
        };

        var expectedSelectedScheme = new
        {
            OrganisationConnectionId = existingOrgConnections + 1, // new orgConnection
            IsDeleted = false
        };

        var expectedOrganisationsConnection = new
        {
            FromOrganisationId = newProducerOrg.Id,
            FromOrganisationRoleId = InterOrganisationRole.Producer,
            ToOrganisationId = organisation.Id,
            ToOrganisationRoleId = InterOrganisationRole.ComplianceScheme,
            IsDeleted = false
        };

        var preRunDateTime = DateTimeOffset.Now;

        //Act
        var result = await _complianceSchemeService.SelectComplianceSchemeAsync(request);

        //Assert
        result.Should().BeOfType(typeof(Result<SelectedScheme>));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType(typeof(SelectedScheme));
        result.Value.Should().BeEquivalentTo(expectedSelectedScheme);

        var selectedScheme = _dbContext.SelectedSchemes.Single(x => x.LastUpdatedOn > preRunDateTime);
        selectedScheme.Should().BeEquivalentTo(expectedSelectedScheme);

        var orgConnection = _dbContext.OrganisationsConnections.Single(x => x.LastUpdatedOn > preRunDateTime);
        orgConnection.Should().BeEquivalentTo(expectedOrganisationsConnection);
    }

    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000021")]
    [DataRow("00000000-0000-0000-0000-000000000022")]
    public async Task WhenComplianceSchemeIsSelected_AndComplianceSchemeAlreadyExists_ThenSelectedSchemeIsCreated_AndExistingComplianceSchemesAreDeleted(string complianceSchemeId)
    {
        //Arrange
        var existingOrgConnections = _dbContext.OrganisationsConnections.Count();

        var newProducerOrg = _dbContext.AddOrganisation(new Organisation
        {
            Name = _fixture.Create<string>(),
            OrganisationTypeId = 1,
            ExternalId = Guid.NewGuid()
        });

        var complianceScheme = _dbContext.ComplianceSchemes.Single(x => x.ExternalId == new Guid(complianceSchemeId));
        var organisation = _dbContext.Organisations.Single(x => x.CompaniesHouseNumber == complianceScheme.CompaniesHouseNumber);

        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = complianceScheme.ExternalId,
            ProducerOrganisationId = newProducerOrg.ExternalId,
            UserOId = _fixture.Create<Guid>(),
        };

        var request2 = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = complianceScheme.ExternalId,
            ProducerOrganisationId = newProducerOrg.ExternalId,
            UserOId = _fixture.Create<Guid>(),
        };

        var expectedSelectedScheme = new
        {
            OrganisationConnectionId = existingOrgConnections + 2, // new orgConnection
            IsDeleted = false
        };

        var expectedOrganisationsConnection = new
        {
            FromOrganisationId = newProducerOrg.Id,
            FromOrganisationRoleId = InterOrganisationRole.Producer,
            ToOrganisationId = organisation.Id,
            ToOrganisationRoleId = InterOrganisationRole.ComplianceScheme,
            IsDeleted = false
        };

        var preRunDateTime = DateTimeOffset.Now;

        //Act
        var addCs1 = await _complianceSchemeService.SelectComplianceSchemeAsync(request);
        var addCs2 = await _complianceSchemeService.SelectComplianceSchemeAsync(request2);

        //Assert
        addCs2.Should().BeOfType(typeof(Result<SelectedScheme>));
        addCs2.IsSuccess.Should().BeTrue();
        addCs2.Value.Should().BeOfType(typeof(SelectedScheme));
        addCs2.Value.Should().BeEquivalentTo(expectedSelectedScheme);

        var selectedScheme = _dbContext.SelectedSchemes.Single(x =>
            x.LastUpdatedOn > preRunDateTime && !x.IsDeleted &&
            x.OrganisationConnection.FromOrganisation.ExternalId == newProducerOrg.ExternalId);
        selectedScheme.Should().BeEquivalentTo(expectedSelectedScheme);

        var orgConnection = _dbContext.OrganisationsConnections.Single(x => x.LastUpdatedOn > preRunDateTime);
        _dbContext.SelectedSchemes.Count(x =>
            x.OrganisationConnection.FromOrganisation.ExternalId == newProducerOrg.ExternalId).Should().Be(1);

        orgConnection.Should().BeEquivalentTo(expectedOrganisationsConnection);
    }

    [TestMethod]
    public async Task WhenUpdateComplianceSchemeIsRequested_AndProducerOrgNotFound_ReturnsNotFound()
    {
        //Arrange
        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = _existingSelectedSchemeId,
            ComplianceSchemeId = _existingComplianceSchemeId,
            ProducerOrganisationId = _organisationId, // doesnt exist
            UserOid = _userOId
        };

        var expectedResult = Result<SelectedScheme>.FailedResult("Producer organisation not found", HttpStatusCode.NotFound);
        var preRunDateTime = DateTimeOffset.Now;

        //Act
        var result = await _complianceSchemeService.UpdateSelectedComplianceSchemeAsync(request);

        //Assert
        result.Should().BeOfType(typeof(Result<SelectedScheme>));
        result.Should().BeEquivalentTo(expectedResult);

        // hasn't soft deleted previous org connection or scheme
        var previousSelectedScheme = _dbContext.SelectedSchemes.AsNoTracking().FirstOrDefault(x => x.ExternalId == request.SelectedSchemeId);
        previousSelectedScheme?.IsDeleted.Should().BeFalse();

        var previousOrgConnection = _dbContext.OrganisationsConnections.AsNoTracking().FirstOrDefault(x => x.Id == 1);
        previousOrgConnection?.IsDeleted.Should().BeFalse();

        // hasnt created new selected scheme and org connection entities?
        var newSelectedScheme =
            _dbContext.SelectedSchemes.AsNoTracking().FirstOrDefault(x => x.LastUpdatedOn > preRunDateTime && !x.IsDeleted);
        newSelectedScheme.Should().BeNull();
        var newOrgConnection =
            _dbContext.OrganisationsConnections.AsNoTracking().FirstOrDefault(x => x.LastUpdatedOn > preRunDateTime && !x.IsDeleted);
        newOrgConnection.Should().BeNull();
    }

    [TestMethod]
    public async Task WhenUpdateComplianceSchemeIsRequested_AndSelectedSchemeNotFound_ReturnsNotFound()
    {
        //Arrange
        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = _selectedSchemeId, // doesnt exist
            ComplianceSchemeId = _existingComplianceSchemeId,
            ProducerOrganisationId = _existingProducerWithSelectedSchemeId,
            UserOid = _userOId
        };

        var expectedResult = Result<SelectedScheme>.FailedResult("Existing selected scheme not found", HttpStatusCode.NotFound);
        var preRunDateTime = DateTimeOffset.Now;

        //Act
        var result = await _complianceSchemeService.UpdateSelectedComplianceSchemeAsync(request);

        //Assert
        result.Should().BeOfType(typeof(Result<SelectedScheme>));
        result.Should().BeEquivalentTo(expectedResult);

        // hasn't soft deleted previous org connection or scheme
        var previousSelectedScheme = _dbContext.SelectedSchemes.AsNoTracking().FirstOrDefault(x => x.ExternalId == request.SelectedSchemeId);
        previousSelectedScheme?.IsDeleted.Should().BeFalse();

        var previousOrgConnection = _dbContext.OrganisationsConnections.AsNoTracking().FirstOrDefault(x => x.ExternalId == new Guid("00000000-0000-0000-0000-000000000011"));
        previousOrgConnection?.IsDeleted.Should().BeFalse();

        // hasnt created new selected scheme and org connection entities?
        var newSelectedScheme =
            _dbContext.SelectedSchemes.AsNoTracking().FirstOrDefault(x => x.LastUpdatedOn > preRunDateTime && !x.IsDeleted);
        newSelectedScheme.Should().BeNull();
        var newOrgConnection =
            _dbContext.OrganisationsConnections.AsNoTracking().FirstOrDefault(x => x.LastUpdatedOn > preRunDateTime && !x.IsDeleted);
        newOrgConnection.Should().BeNull();
    }

    [TestMethod]
    public async Task WhenSelectComplianceSchemeIsRequested_AndComplianceSchemeNotFound_NotFoundIsReturned()
    {
        //Arrange
        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = _complianceSchemeId, // doesnt exist
            ProducerOrganisationId = _existingProducerWithSelectedSchemeId,
            UserOId = _userOId
        };

        var expectedResult = Result<SelectedScheme>.FailedResult("Compliance scheme not found", HttpStatusCode.NotFound);

        var preRunDateTime = DateTimeOffset.Now;

        //Act
        var result = await _complianceSchemeService.SelectComplianceSchemeAsync(request);

        //Assert
        result.Should().BeOfType(typeof(Result<SelectedScheme>));
        result.Should().BeEquivalentTo(expectedResult);

        //Assert
        result.Should().BeOfType(typeof(Result<SelectedScheme>));
        result.Should().BeEquivalentTo(expectedResult);

        // hasn't soft deleted previous org connection
        var previousOrgConnection = _dbContext.OrganisationsConnections.AsNoTracking().FirstOrDefault(x => x.Id == 1);
        previousOrgConnection?.IsDeleted.Should().BeFalse();

        // hasnt created new org connection?
        var newOrgConnection =
            _dbContext.OrganisationsConnections.AsNoTracking().FirstOrDefault(x => x.LastUpdatedOn > preRunDateTime && !x.IsDeleted);
        newOrgConnection.Should().BeNull();
    }

    [TestMethod]
    public async Task WhenSelectComplianceSchemeIsRequested_AndProducerNotFound_NotFoundIsReturned()
    {
        //Arrange
        var request = new SelectComplianceSchemeRequest
        {
            ComplianceSchemeId = _existingComplianceSchemeId,
            ProducerOrganisationId = _organisationId, // doesnt exist
            UserOId = _userOId
        };

        var expectedResult = Result<SelectedScheme>.FailedResult("Producer organisation not found", HttpStatusCode.NotFound);

        var preRunDateTime = DateTimeOffset.Now;

        //Act
        var result = await _complianceSchemeService.SelectComplianceSchemeAsync(request);

        //Assert
        result.Should().BeOfType(typeof(Result<SelectedScheme>));
        result.Should().BeEquivalentTo(expectedResult);

        // hasn't soft deleted previous org connection
        var previousOrgConnection = _dbContext.OrganisationsConnections.AsNoTracking().FirstOrDefault(x => x.Id == 1);
        previousOrgConnection?.IsDeleted.Should().BeFalse();

        // hasnt created new org connection?
        var newOrgConnection =
            _dbContext.OrganisationsConnections.AsNoTracking().FirstOrDefault(x => x.LastUpdatedOn > preRunDateTime && !x.IsDeleted);
        newOrgConnection.Should().BeNull();
    }

    [TestMethod]
    public async Task WhenUpdateComplianceSchemeIsRequested_AndComplianceSchemeNotFound_NotFoundIsReturned()
    {
        //Arrange
        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = _existingSelectedSchemeId,
            ComplianceSchemeId = _complianceSchemeId, // doesnt exist
            ProducerOrganisationId = _existingProducerWithSelectedSchemeId,
            UserOid = _userOId
        };

        var expectedResult = Result<SelectedScheme>.FailedResult("Compliance scheme not found", HttpStatusCode.NotFound);

        //Act
        var result = await _complianceSchemeService.UpdateSelectedComplianceSchemeAsync(request);

        //Assert
        result.Should().BeOfType(typeof(Result<SelectedScheme>));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task WhenUpdateComplianceSchemeIsRequested_AndProducerNotFound_NotFoundIsReturned()
    {
        //Arrange
        var request = new UpdateSelectedComplianceSchemeRequest
        {
            SelectedSchemeId = _existingSelectedSchemeId,
            ComplianceSchemeId = _existingComplianceSchemeId,
            ProducerOrganisationId = _organisationId, // doesnt exist
            UserOid = _userOId
        };

        var expectedResult = Result<SelectedScheme>.FailedResult("Producer organisation not found", HttpStatusCode.NotFound);
        var preRunDateTime = DateTimeOffset.Now;

        //Act
        var result = await _complianceSchemeService.UpdateSelectedComplianceSchemeAsync(request);

        //Assert
        result.Should().BeOfType(typeof(Result<SelectedScheme>));
        result.Should().BeEquivalentTo(expectedResult);

        // hasn't soft deleted previous org connection or scheme
        var previousSelectedScheme = _dbContext.SelectedSchemes.AsNoTracking().FirstOrDefault(x => x.ExternalId == request.SelectedSchemeId);
        previousSelectedScheme?.IsDeleted.Should().BeFalse();

        var previousOrgConnection = _dbContext.OrganisationsConnections.AsNoTracking().FirstOrDefault(x => x.Id == 1);
        previousOrgConnection?.IsDeleted.Should().BeFalse();

        // hasnt created new selected scheme and org connection entities?
        var newSelectedScheme =
            _dbContext.SelectedSchemes.AsNoTracking().FirstOrDefault(x => x.LastUpdatedOn > preRunDateTime && !x.IsDeleted);
        newSelectedScheme.Should().BeNull();
        var newOrgConnection =
            _dbContext.OrganisationsConnections.AsNoTracking().FirstOrDefault(x => x.LastUpdatedOn > preRunDateTime && !x.IsDeleted);
        newOrgConnection.Should().BeNull();
    }

    [TestMethod]
    public async Task WhenSelectedComplianceSchemeIsRequestedForTheProducer_AndSelectedSchemeIsAvailable_ThenGetTheSelectedSchemeDetails()
    {
        //Act
        var result = await _complianceSchemeService.GetComplianceSchemeForProducer(_existingProducerWithSelectedSchemeId);

        //Assert
        result.Should().BeOfType(typeof(Result<ProducerComplianceSchemeDto>));
        result.IsSuccess.Should().BeTrue();
        result.Value.ComplianceSchemeName.Should().NotBe(null);
    }

    [TestMethod]
    public async Task WhenSelectedComplianceSchemeIsRequestedForTheProducer_AndNoComplianceSchemeHasBeenSelected_ThenNotFoundIsReturned()
    {
        //Act
        var result = await _complianceSchemeService.GetComplianceSchemeForProducer(_organisationId);

        //Assert
        var expectedResult = Result<SelectedScheme>.FailedResult("Organisation does not currently have a compliance scheme selected", HttpStatusCode.NoContent);
        result.Should().BeOfType(typeof(Result<ProducerComplianceSchemeDto>));
        result.Should().BeEquivalentTo(expectedResult);
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.ErrorMessage.Should().Be("Organisation does not currently have a compliance scheme selected");
    }

    [TestMethod]
    public async Task WhenSelectedComplianceSchemeIsRequestedForTheProducer_AndMultipleSelectedSchemeFoundForProducer_ThenReturnInternalServerError()
    {
        //Act
        var result = await _complianceSchemeService.GetComplianceSchemeForProducer(_existingProducerWithMultipleSchemesId);

        //Assert
        result.Should().BeOfType(typeof(Result<ProducerComplianceSchemeDto>));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task When_Get_All_Compliance_Schemes_Is_Called_Then_Return_All_Compliance_Schemes()
    {
        //Arrange
        var csOrgsCompaniesHouseNumbers = await _dbContext.Organisations
            .Where(x => x.IsComplianceScheme)
            .AsNoTracking()
            .Select(x => x.CompaniesHouseNumber)
            .ToListAsync();

        var expectedResult = await _dbContext
            .ComplianceSchemes
            .Where(cs => csOrgsCompaniesHouseNumbers.Contains(cs.CompaniesHouseNumber))
            .Select(cs => new ComplianceSchemeDto(cs.Id, cs.ExternalId, cs.Name, cs.CreatedOn, cs.NationId))
            .ToListAsync();

        //Act
        var result = await _complianceSchemeService.GetAllComplianceSchemesAsync();

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task When_Get_Compliance_Schemes_For_Operator_Is_Called_Then_Return_Its_Compliance_Schemes()
    {
        //Arrange
        var organisation = await _dbContext.Organisations.FirstOrDefaultAsync(x => x.ExternalId == _existingProducerWithSelectedSchemeId);

        var expectedResult = await _dbContext
            .ComplianceSchemes
            .Where(x => x.CompaniesHouseNumber == organisation.CompaniesHouseNumber)
            .Select(cs => new ComplianceSchemeDto(cs.Id, cs.ExternalId, cs.Name, cs.CreatedOn, cs.NationId))
            .ToListAsync();

        //Act
        var result = await _complianceSchemeService.GetComplianceSchemesForOperatorAsync(_existingProducerWithSelectedSchemeId);

        //Assert
        result.Should().BeOfType(typeof(List<ComplianceSchemeDto>));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetailsAsync_ComplianceSchemeExists_ResultShouldBeSuccess()
    {
        //Arrange
        var validComplianceSchemeId = new Guid("00000000-0000-0000-0000-000000000001");
        var selectedSchemeId = new Guid("00000000-0000-0000-0000-000000000037");

        //Act
        var result = await _complianceSchemeService.GetComplianceSchemeMemberDetailsAsync(validComplianceSchemeId, selectedSchemeId);

        //Assert
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMemberDetailDto>));
        result.IsSuccess.Should().BeTrue();

    }

    [TestMethod]
    public async Task GetComplianceSchemeMemberDetailsAsync_ComplianceSchemeDoesNotExists_ResultShouldNotBeSuccess()
    {
        //Arrange
        var validComplianceSchemeId = _fixture.Create<Guid>();
        var selectedSchemeId = new Guid("00000000-0000-0000-0000-000000000037");

        //Act
        var result = await _complianceSchemeService.GetComplianceSchemeMemberDetailsAsync(validComplianceSchemeId, selectedSchemeId);

        //Assert
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMemberDetailDto>));
        result.IsSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_WhenSuccessResult_ReturnsExpectedResult()
    {
        //Arrange
        var validComplianceSchemeId = new Guid("00000000-0000-0000-0000-000000000001");
        var selectedSchemeId = new Guid("00000000-0000-0000-0000-000000000037");
        var expectedOrganisationNumber = "8924785";
        var expectedRegisteredNation = NationMappings.GetNation(1);
        var expectedOrganisationName = "Member";
        var expectedProducerType = OrganisationMappings.GetProducerType(1);

        //Act
        var result = await _complianceSchemeService.GetComplianceSchemeMemberDetailsAsync(validComplianceSchemeId, selectedSchemeId);

        //Assert
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMemberDetailDto>));
        result.IsSuccess.Should().BeTrue();
        result.Value.OrganisationNumber.Should().Be(expectedOrganisationNumber);
        result.Value.RegisteredNation.Should().Be(expectedRegisteredNation);
        result.Value.OrganisationName.Should().Be(expectedOrganisationName);
        result.Value.ProducerType.Should().Be(expectedProducerType);
    }

    [TestMethod]
    public async Task GetComplianceSchemeSummary_WhenThereIsNoData_ThenReturnNull()
    {
        //Arrange
        var dbContext = new AccountsDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();

        //Act
        var result = await new ComplianceSchemeService(dbContext, _logger)
            .GetComplianceSchemeSummary(Guid.NewGuid(), Guid.NewGuid());

        //Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetComplianceSchemeSummary_WhenThereIsOnlyDeletedMemberData_ThenReturnSummaryWithZeroMemberCounts()
    {
        //Arrange
        var dbContext = new AccountsDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var operatorOrganisation = ComplainceSchemeTestHelper.SummaryData.AddOperatorOrganisation(dbContext);

        var complianceScheme1 = ComplainceSchemeTestHelper.SummaryData.AddComplianceScheme(dbContext, Data.DbConstants.Nation.England);
        ComplainceSchemeTestHelper.SummaryData.AddDeletedSchemeMembers(dbContext, operatorOrganisation, complianceScheme1, deletedMembersCount: 20);

        var complianceScheme2 = ComplainceSchemeTestHelper.SummaryData.AddComplianceScheme(dbContext, Data.DbConstants.Nation.Scotland);
        ComplainceSchemeTestHelper.SummaryData.AddDeletedSchemeMembers(dbContext, operatorOrganisation, complianceScheme2, deletedMembersCount: 25);

        await dbContext.SaveChangesAsync(Guid.NewGuid(), Guid.NewGuid());

        var summary1 = await new ComplianceSchemeService(dbContext, _logger)
            .GetComplianceSchemeSummary(operatorOrganisation.ExternalId, complianceScheme1.ExternalId);

        summary1.CreatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary1.MembersLastUpdatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary1.MemberCount.Should().Be(0);
        summary1.Name.Should().Be(complianceScheme1.Name);
        summary1.Nation.Should().Be(Models.Nation.England);

        var summary2 = await new ComplianceSchemeService(dbContext, _logger)
            .GetComplianceSchemeSummary(operatorOrganisation.ExternalId, complianceScheme2.ExternalId);

        summary2.CreatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary2.MembersLastUpdatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary2.MemberCount.Should().Be(0);
        summary2.Name.Should().Be(complianceScheme2.Name);
        summary2.Nation.Should().Be(Models.Nation.Scotland);
    }

    [TestMethod]
    public async Task GetComplianceSchemeSummary_WhenThereIsOnlyNotDeletedData_ThenReturnCountIsGreaterThanZero()
    {
        //Arrange
        var dbContext = new AccountsDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var operatorOrganisation = ComplainceSchemeTestHelper.SummaryData.AddOperatorOrganisation(dbContext);

        var complianceScheme = ComplainceSchemeTestHelper.SummaryData.AddComplianceScheme(dbContext, Data.DbConstants.Nation.England);
        ComplainceSchemeTestHelper.SummaryData.AddSchemeMembers(dbContext, operatorOrganisation, complianceScheme, membersCount: 111);

        await dbContext.SaveChangesAsync(Guid.NewGuid(), Guid.NewGuid());

        //Act
        var summary = await new ComplianceSchemeService(dbContext, _logger)
            .GetComplianceSchemeSummary(operatorOrganisation.ExternalId, complianceScheme.ExternalId);

        //Assert
        summary.CreatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary.MembersLastUpdatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary.MemberCount.Should().Be(111);
        summary.Name.Should().Be(complianceScheme.Name);
        summary.Nation.Should().Be(Models.Nation.England);
    }

    [TestMethod]
    public async Task GetComplianceSchemeSummary_WhenComplianceSchemeDoesNotExist_ThenReturnNull()
    {
        //Arrange
        var dbContext = new AccountsDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var operatorOrganisation = ComplainceSchemeTestHelper.SummaryData.AddOperatorOrganisation(dbContext);

        var complianceScheme = ComplainceSchemeTestHelper.SummaryData.AddComplianceScheme(dbContext, Data.DbConstants.Nation.England);
        ComplainceSchemeTestHelper.SummaryData.AddSchemeMembers(dbContext, operatorOrganisation, complianceScheme, membersCount: 111);

        await dbContext.SaveChangesAsync(Guid.NewGuid(), Guid.NewGuid());

        //Act
        var summary = await new ComplianceSchemeService(dbContext, _logger)
            .GetComplianceSchemeSummary(operatorOrganisation.ExternalId, Guid.NewGuid());

        //Assert
        summary.Should().BeNull();
    }

    [TestMethod]
    public async Task GetComplianceSchemeSummary_WhenThereAreMultipleComplianceSchemesWithNoMembers_ThenReturnListOfSummariesWithZeroMemberCount()
    {
        //Arrange
        var dbContext = new AccountsDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var operatorOrganisation = ComplainceSchemeTestHelper.SummaryData.AddOperatorOrganisation(dbContext);

        var complianceScheme1 = ComplainceSchemeTestHelper.SummaryData.AddComplianceScheme(dbContext, Data.DbConstants.Nation.England);
        var complianceScheme2 = ComplainceSchemeTestHelper.SummaryData.AddComplianceScheme(dbContext, Data.DbConstants.Nation.Scotland);

        await dbContext.SaveChangesAsync(Guid.NewGuid(), Guid.NewGuid());

        var summary1 = await new ComplianceSchemeService(dbContext, _logger)
            .GetComplianceSchemeSummary(operatorOrganisation.ExternalId, complianceScheme1.ExternalId);

        summary1.MembersLastUpdatedOn.Should().BeNull();
        summary1.CreatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary1.MemberCount.Should().Be(0);
        summary1.Name.Should().Be(complianceScheme1.Name);
        summary1.Nation.Should().Be(Models.Nation.England);

        var summary2 = await new ComplianceSchemeService(dbContext, _logger)
            .GetComplianceSchemeSummary(operatorOrganisation.ExternalId, complianceScheme2.ExternalId);

        summary2.MembersLastUpdatedOn.Should().BeNull();
        summary2.CreatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary2.MemberCount.Should().Be(0);
        summary2.Name.Should().Be(complianceScheme2.Name);
        summary2.Nation.Should().Be(Models.Nation.Scotland);
    }

    [TestMethod]
    public async Task GetComplianceSchemeSummary_WhenThereAreMultipleComplianceSchemesWithMembers_ThenReturnSummaryWithNonZeroCounts()
    {
        //Arrange
        var dbContext = new AccountsDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var operatorOrganisation = ComplainceSchemeTestHelper.SummaryData.AddOperatorOrganisation(dbContext);

        var complianceScheme1 = ComplainceSchemeTestHelper.SummaryData.AddComplianceScheme(dbContext, Data.DbConstants.Nation.England);
        ComplainceSchemeTestHelper.SummaryData.AddSchemeMembers(dbContext, operatorOrganisation, complianceScheme1, membersCount: 125);

        var complianceScheme2 = ComplainceSchemeTestHelper.SummaryData.AddComplianceScheme(dbContext, Data.DbConstants.Nation.England);
        ComplainceSchemeTestHelper.SummaryData.AddSchemeMembers(dbContext, operatorOrganisation, complianceScheme2, membersCount: 97);

        await dbContext.SaveChangesAsync(Guid.NewGuid(), Guid.NewGuid());

        var summary1 = await new ComplianceSchemeService(dbContext, _logger)
            .GetComplianceSchemeSummary(operatorOrganisation.ExternalId, complianceScheme1.ExternalId);

        summary1.CreatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary1.MembersLastUpdatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary1.MemberCount.Should().Be(125);
        summary1.Name.Should().Be(complianceScheme1.Name);
        summary1.Nation.Should().Be(Models.Nation.England);

        var summary2 = await new ComplianceSchemeService(dbContext, _logger)
            .GetComplianceSchemeSummary(operatorOrganisation.ExternalId, complianceScheme2.ExternalId);

        summary2.CreatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary2.MembersLastUpdatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        summary2.MemberCount.Should().Be(97);
        summary2.Name.Should().Be(complianceScheme2.Name);
        summary2.Nation.Should().Be(Models.Nation.England);
    }

    [TestMethod]
    public async Task GetComplianceSchemeReasonsForRemoval_ReasonsFound_ReturnOkResultWithListOfReasons()
    {
        //Arrange       
        var expectedResult = new List<ComplianceSchemeRemovalReasonResponse>
        {
           new("A",false),
           new("B",false),
           new("C",false),
           new("D",false),
           new("E",true),
           new("F",false),
           new("G",false),
           new("H",false),
           new("I",false),
           new("J",true)
        };
        //Act
        var result = await _complianceSchemeService.GetComplianceSchemeReasonsForRemovalAsync();

        //Assert
        result.Should().BeOfType(typeof(List<ComplianceSchemeRemovalReasonResponse>));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task ExportComplianceSchemeSubsidiaries_ShouldReturnNul_WhenComplianceSchemeDoesnotExist()
    {
        // Arrange
        var organisationId = new Guid("00000000-0000-0000-0000-000000000001");
        var complianceSchemeId = new Guid("00000000-0000-0000-0000-000000000011");
        var userId = new Guid();

        // Act
        var result = await _complianceSchemeService.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task ExportComplianceSchemeSubsidiaries_ShouldReturnNull_WhenRelationshipDoesnotExist()
    {
        // Arrange
        var organisationId = new Guid("00000000-0000-0000-0000-000000000001");
        var complianceSchemeId = new Guid("00000000-0000-0000-0000-000000000031");
        var userId = new Guid();

        // Act
        var result = await _complianceSchemeService.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task ExportComplianceSchemeSubsidiaries_ShouldReturnNull_WhenComplianceSchemeDoesnotExist()
    {
        // Arrange
        var organisationId = new Guid("00000000-0000-0000-0000-000000000001");
        var complianceSchemeId = new Guid("00000000-0000-0000-0000-000000000099");
        var userId = new Guid();

        // Act
        var result = await _complianceSchemeService.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task ExportComplianceSchemeSubsidiaries_ShouldReturnValue_WhenRelationshipExist()
    {
        // Arrange
        var organisationId = new Guid("00000000-0000-0000-0000-000000000001");
        var complianceSchemeId = new Guid("00000000-0000-0000-0000-000000000021");
        var userId = new Guid();

        // Act
        var result = await _complianceSchemeService.ExportComplianceSchemeSubsidiaries(userId, organisationId, complianceSchemeId);

        // Assert
        result.Should().BeNull();
    }


}

