namespace BackendAccountService.Core.UnitTests.Services;

using AutoFixture;
using AutoFixture.AutoMoq;
using Core.Services;
using Data.Entities;
using Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Models.Request;
using Models.Responses;
using Models.Result;
using TestHelpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

[TestClass]
public class ComplianceSchemeServiceMemberTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private AccountsDbContext _dbContext = null!;
    private ComplianceSchemeService _complianceSchemeService = null!;
    private readonly NullLogger<ComplianceSchemeService> _logger = new();

    private readonly Guid _validComplianceSchemeId = new Guid("22222222-0000-0000-0000-000000000000");
    private readonly Guid _invalidComplianceSchemeId = new Guid("33333333-0000-0000-0000-000000000000");
    
    private readonly Guid _validOrganisationId = new Guid("11111111-0000-0000-0000-000000000001");
    private readonly Guid _invalidOrganisationId = new Guid("11111111-0000-0000-0000-000000000002");
    
    private readonly Guid _validSelectedSchemeId = new Guid("44444444-0000-0000-0000-000000000001");
    private readonly Guid _invalidSelectedSchemeId = new Guid("44444444-0000-0000-0000-000000000002");
    
    private readonly Guid _validUserId = new Guid("55555555-0000-0000-0000-000000000001");
    
    


    [TestInitialize]
    public void Setup()
    {
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("ComplianceSchemeServiceMemberTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new AccountsDbContext(contextOptions);

        ComplainceSchemeMemberTestHelper.SetUpDatabase(_dbContext);

        _complianceSchemeService = new ComplianceSchemeService(_dbContext, _logger);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_ComplianceSchemeDoesNotExists_ResultShouldNotBeSuccess()
    {
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_validOrganisationId, _invalidComplianceSchemeId, "", 10, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_QueryIsTooLong_ResultShouldNotBeSuccess()
    {
        var tooLongQuery = new string('X', 161);

        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_validOrganisationId, _validComplianceSchemeId, tooLongQuery, 10, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        result.ErrorMessage.Should().Be($"Length {tooLongQuery.Length} of parameter 'query' exceeds max length 160");
        result.IsSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_PageSizeExceedsMax_PageSizeSetToMax()
    {
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_validOrganisationId, _validComplianceSchemeId, "", 1000, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.Value.PagedResult.PageSize.Should().Be(100);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_GetAllRecords_ResultShouldContain200()
    {
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_validOrganisationId, _validComplianceSchemeId, "", 10, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeTrue();
        result.Value.PagedResult.TotalItems.Should().Be(201);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_ReferenceNumberExists_ResultShouldContainOneRecord()
    {
        var expectedOrganisationNumber = "199999";
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_validOrganisationId, _validComplianceSchemeId, expectedOrganisationNumber, 10, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeTrue();
        result.Value.PagedResult.TotalItems.Should().Be(1);
        result.Value.PagedResult.Items[0].OrganisationNumber.Should().Be(expectedOrganisationNumber);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_WhenJoinerDateAndReportingTypeAreNull_ShouldReturnNullValues()
    {
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(
            _validOrganisationId, _validComplianceSchemeId, "", 10, 1, false);

        var member = result.Value.PagedResult.Items.FirstOrDefault(m => m.OrganisationNumber == "199999");
        member.Should().NotBeNull();

        var relationshipWithNulls = member.Relationships.FirstOrDefault(r => r.OrganisationName == "Org With Null Fields");
        relationshipWithNulls.Should().NotBeNull();
        relationshipWithNulls.JoinerDate.Should().BeNull();
    }


    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_WhenJoinerDateAndReportingTypeExist_ShouldReturnCorrectValues()
    {
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(
            _validOrganisationId, _validComplianceSchemeId, "", 10, 1, false);

        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeTrue();
        result.Value.PagedResult.TotalItems.Should().BeGreaterThan(0);

        var member = result.Value.PagedResult.Items.FirstOrDefault(x=>x.OrganisationNumber == "199999");
        member.Should().NotBeNull();

        var relationship = member.Relationships.FirstOrDefault();
        relationship.Should().NotBeNull();
        relationship.JoinerDate.Should().NotBeNull();
    }


    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_ReferenceNumberExistsButHideNoSubsidiaries_ResultShouldContainNoRecords()
    {
        var expectedOrganisationNumber = "199998";
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_validOrganisationId, _validComplianceSchemeId, expectedOrganisationNumber, 10, 1, true);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeTrue();
        result.Value.PagedResult.TotalItems.Should().Be(0);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_ReferenceNumberDoesNotExist_ResultShouldContainNoRecords()
    {
        var expectedOrganisationNumber = "999111";
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_validOrganisationId, _validComplianceSchemeId, expectedOrganisationNumber, 10, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeTrue();
        result.Value.PagedResult.TotalItems.Should().Be(0);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_MemberNameExists_ResultShouldContainOneRecord()
    {
        var expectedMemberName = "Member 100";
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_validOrganisationId, _validComplianceSchemeId, expectedMemberName, 10, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeTrue();
        result.Value.PagedResult.TotalItems.Should().Be(1);
        result.Value.PagedResult.Items[0].OrganisationName.Should().Be(expectedMemberName);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_MemberNameDoesNotExist_ResultShouldContainOneRecord()
    {
        var expectedMemberName = "Member non existent";
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_validOrganisationId, _validComplianceSchemeId, expectedMemberName, 10, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeTrue();
        result.Value.PagedResult.TotalItems.Should().Be(0);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_MemberNameExistsMoreThanOnce_ResultShouldContainOneRecord()
    {
        var expectedMemberName = "Member 10";
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_validOrganisationId, _validComplianceSchemeId, expectedMemberName, 10, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeTrue();
        result.Value.PagedResult.TotalItems.Should().Be(11);
    }

    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_WhenOperatorNotExistForTheComplianceScheme_ResultShouldContainNoRecord()
    {
        var expectedMemberName = "Member 10";
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(_invalidOrganisationId, _validComplianceSchemeId, expectedMemberName, 10, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeFalse();
    }
    
    [TestMethod]
    public async Task GetComplianceSchemeMembersAsync_MemberNameExistsButNotInOrganisation_ResultShouldContainNoRecord()
    {
        var organisationWithNoMembers = new Guid("11111111-0000-0000-0000-000000000003");
        var complianceSchemeId = new Guid("22222222-0000-0000-0000-000000000001");
        var result = await _complianceSchemeService.GetComplianceSchemeMembersAsync(organisationWithNoMembers, complianceSchemeId, "", 10, 1, false);
        result.Should().BeOfType(typeof(Result<ComplianceSchemeMembershipResponse>));
        result.IsSuccess.Should().BeTrue();
    }

    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_WhenRequestCorrect_ResultShouldBeSuccess()
    {
        var request = new RemoveComplianceSchemeMemberRequest
        {
            Code = "A",
            TellUsMore = "Test"
        };
        var result = await _complianceSchemeService.RemoveComplianceSchemeMember(_validOrganisationId, _validSelectedSchemeId, _validUserId, request);
        result.Should().BeOfType(typeof(Result<RemoveComplianceSchemeMemberResponse>));
        result.IsSuccess.Should().BeTrue();
        
        var selectedSchemeToRemove = _dbContext.SelectedSchemes
            .Include(selectedScheme => selectedScheme.OrganisationConnection)
            .Where(selectedScheme =>
                selectedScheme.ExternalId == _validSelectedSchemeId);

        selectedSchemeToRemove.Should().BeNullOrEmpty();

    }
    
    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_RemoveReasonDoesNotExists_ResultShouldNotBeSuccess()
    {
        var request = new RemoveComplianceSchemeMemberRequest
        {
            Code = "1",
            TellUsMore = "Test"
        };
        var result = await _complianceSchemeService.RemoveComplianceSchemeMember(_validOrganisationId, _validSelectedSchemeId, _validUserId, request);
        result.Should().BeOfType(typeof(Result<RemoveComplianceSchemeMemberResponse>));
        result.IsSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_WhenOrganisationIdIsInvalid_ResultShouldNotBeSuccess()
    {
        var request = new RemoveComplianceSchemeMemberRequest
        {
            Code = "A",
            TellUsMore = "Test"
        };
        var result = await _complianceSchemeService.RemoveComplianceSchemeMember(_invalidOrganisationId, _validSelectedSchemeId, _validUserId, request);
        result.Should().BeOfType(typeof(Result<RemoveComplianceSchemeMemberResponse>));
        result.IsSuccess.Should().BeFalse();
    }
    
    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_WhenSelectedSchemeIdIsInvalid_ResultShouldNotBeSuccess()
    {
        var request = new RemoveComplianceSchemeMemberRequest
        {
            Code = "A",
            TellUsMore = "Test"
        };
        var result = await _complianceSchemeService.RemoveComplianceSchemeMember(_validOrganisationId, _invalidSelectedSchemeId, _validUserId, request);
        result.Should().BeOfType(typeof(Result<RemoveComplianceSchemeMemberResponse>));
        result.IsSuccess.Should().BeFalse();
    }
    
    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_WhenRequestCorrect_ShouldCreateLog()
    {
        var request = new RemoveComplianceSchemeMemberRequest
        {
            Code = "A",
            TellUsMore = "Test"
        };
        
        await _complianceSchemeService.RemoveComplianceSchemeMember(_validOrganisationId, _validSelectedSchemeId, _validUserId, request);

        var expectedLog = new ComplianceSchemeMemberRemovalAuditLogsReason
        {
            AuditLog = new ComplianceSchemeMemberRemovalAuditLog
            {
                SchemeOrganisationId = 1,
                MemberOrganisationId = 204,
                ComplianceSchemeId = 69,
                RemovedBy = _validUserId,
                ReasonDescription = "Test"
            },
            Reason = new ComplianceSchemeMemberRemovalReason
            {
                Code = "A"
            }
        };

        var audit = _dbContext.ComplianceSchemeMemberRemovalAuditLogsReasons
            .FirstOrDefault();

        audit.AuditLog.SchemeOrganisationId.Should().Be(expectedLog.AuditLog.SchemeOrganisationId);
        audit.AuditLog.MemberOrganisationId.Should().Be(expectedLog.AuditLog.MemberOrganisationId);
        audit.AuditLog.ComplianceSchemeId.Should().Be(expectedLog.AuditLog.ComplianceSchemeId);
        audit.AuditLog.RemovedBy.Should().Be(expectedLog.AuditLog.RemovedBy);
        audit.AuditLog.ReasonDescription.Should().Be(expectedLog.AuditLog.ReasonDescription);
        audit.Reason.Code.Should().Be(expectedLog.Reason.Code);
    }
    
    [TestMethod]
    public async Task RemoveComplianceSchemeMemberAsync_WhenRequestCorrect_ResultShouldReturnCorrectData()
    {
        var expectedResult = new RemoveComplianceSchemeMemberResponse
        {
            OrganisationName = "Organisation Name"
        };
        var request = new RemoveComplianceSchemeMemberRequest
        {
            Code = "A",
            TellUsMore = "Test"
        };
        var result = await _complianceSchemeService.RemoveComplianceSchemeMember(_validOrganisationId, _validSelectedSchemeId, _validUserId, request);
        result.Value.Should().BeEquivalentTo(expectedResult);
    }
}
        