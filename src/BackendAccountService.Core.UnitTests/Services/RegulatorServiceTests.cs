using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Entities.Conversions;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using EnrolmentStatus = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using Nation = BackendAccountService.Data.DbConstants.Nation;
using OrganisationType = BackendAccountService.Data.DbConstants.OrganisationType;
using PersonRole = BackendAccountService.Data.DbConstants.PersonRole;
using ProducerType = BackendAccountService.Data.DbConstants.ProducerType;
using ServiceRole = BackendAccountService.Data.DbConstants.ServiceRole;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class RegulatorServiceTests
{
    private AccountsDbContext _dbContext;
    private RegulatorService _regulatorService;
    private OrganisationService _organisationService;
    private static readonly Guid RegulatorUserId = new Guid("00000000-0000-0000-0000-000000000001");
    private static readonly Guid OrganisationId = new Guid("00000000-0000-0000-0000-000000000010");
    private static readonly Guid TransferredOrganisationId = new Guid("00000000-0000-0000-0000-000000000100");
    private static readonly Guid OrganisationIdSoleTrader = new Guid("80000000-0000-0000-0000-000000000010");
    private static readonly Guid ApprovedEnrolmentId2 = new Guid("00000000-0000-1000-0000-000000000000");
    private static readonly Guid DelegatedPersonId = new Guid("00000000-0000-0000-1000-000000000000");
    private static readonly Guid MultiOrgUserId = new Guid("00000000-1000-0000-0000-000000000000");
    private static readonly Guid ApprovedUserId1 = new Guid("ed3f8357-a6ca-4873-85b6-602377bbdd68");
    private static readonly Guid ApprovedUserId2 = new Guid("7dd8ea6a-f634-4c43-b87c-1a8cf9aaa501");
    private static readonly Guid BasicUserExternalId1 = new Guid("7dd8ea6a-f634-4c43-b87c-1a8cf9aaa502");
    private static readonly Guid BasicUserExternalId2 = new Guid("7dd8ea6a-f634-4c43-b87c-1a8cf9aaa503");
    private static readonly Guid CsOrganisation = Guid.NewGuid();
    private static readonly Guid CsOrganisation2 = Guid.NewGuid();
    private const int PageIndexOne = 1;
    private const int PageIndexTwo = 2;
    private const int PageSizeOne = 1;
    private const int PageIndexTen = 10;
    private const int PageSizeTwo = 2;
    private const int PageSizeTen = 10;
    private const int NationId = 1;
    private const string SearchOrgName = "Spar";
    private const string ApplicationTypeAll = "All";
    private const string ApplicationTypeApproved = "ApprovedPerson";
    private const string ApplicationTypeDelegated = "DelegatedPerson";
    private const string OtherNationOrgName = "Other Nation Organisation";
    private const string DelegatedPersonApprovedOrgName = "Delegated Person Approved Org";
    private const string ApprovedPersonApprovedOrgName = "Approved Person Approved Org";
    private const string BasicUserOrgName = "Basic User Org";
    private const string DeletedEnrolmentOrgName = "Deleted Enrolment Org";
    private const string RejectedComment = "Sorry, your enrolment has been rejected, not enough information provided.";
    private const string TransferComment = "Organisation under incorrect nation";
    private const string ApprovedStatus = "Approved";
    private const string RejectedStatus = "Rejected";
    private const string InvalidEnrolmentMessage = "enrolment not found";
    private const string InvalidEnrolmentStatusMessage = "unsupported enrolment status";
    private const string ApprovedUser2ProducerEmail = "producer.user4@test.com";
    private Mock<ILogger<RegulatorService>> _logger;
    private Mock<ITokenService> _tokenService;
    
    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase(nameof(RegulatorServiceTests))
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new AccountsDbContext(contextOptions);
        
        SetUpDatabase(_dbContext);

        _logger = new Mock<ILogger<RegulatorService>>();
        _tokenService = new Mock<ITokenService>();
        
        _organisationService = new OrganisationService(_dbContext);
        _regulatorService = new RegulatorService(_dbContext, _organisationService, _tokenService.Object, _logger.Object);
        
    }
    
    [TestMethod]
    public async Task When_Regulator_Nation_Is_Requested_And_User_Exists_Then_Return_Nation_Id()
    {
        //Act
       var result = _regulatorService.GetRegulatorNationId(RegulatorUserId);
       
        //Assert
        result.Should().Be(1);
    }

    [TestMethod]
    public async Task When_Regulator_Nation_Is_Requested_And_User_Not_Exists_Then_Return_Nation_Id()
    {
        //Act
        var result = _regulatorService.GetRegulatorNationId(Guid.NewGuid());
       
        //Assert
        result.Should().Be(0);
    }
    
    [TestMethod]
    public async Task When_Pending_Applications_Requested_Without_Search_Without_Filter_For_Page_One_And_Page_Size_10_Then_Return_All_Pending_Enrolments_Of_The_User_Nation()
    {
        //Act
        PaginatedResponse<OrganisationEnrolments> result = await _regulatorService.GetPendingApplicationsAsync(NationId, PageIndexOne, PageSizeTen,
            null, ApplicationTypeAll);
       
        //Assert
        result.Should().BeOfType(typeof(PaginatedResponse<OrganisationEnrolments>));
        result.Items.Any(o => o.OrganisationName.Equals(OtherNationOrgName)).Should().BeFalse();
        result.Items.Any(o => o.OrganisationName.Equals(DelegatedPersonApprovedOrgName)).Should().BeFalse();
        result.Items.Any(o => o.OrganisationName.Equals(DeletedEnrolmentOrgName)).Should().BeFalse();
        result.Items.Any(o => o.OrganisationName.Equals(ApprovedPersonApprovedOrgName)).Should().BeFalse();
        result.Items.Any(o => o.OrganisationName.Equals(BasicUserOrgName)).Should().BeFalse();
        result.Items.Count.Should().Be(4);
        result.TotalItems.Should().Be(4);
    }

    [TestMethod]
    public async Task When_Pending_Applications_Requested_Only_Orgs_In_NationId_Or_CS_Orgs_With_CS_In_NationId_Returned()
    {
        //Act
        PaginatedResponse<OrganisationEnrolments> result = await _regulatorService.GetPendingApplicationsAsync(NationId, PageIndexOne, PageSizeTen,
            null, ApplicationTypeAll);
       
        //Assert
        var organisationIdsReturned = result.Items.Select(o => o.OrganisationId).ToList();
        
        var producersNationIds = _dbContext.Organisations
            .Where(x => !x.IsComplianceScheme && organisationIdsReturned.Contains(x.ExternalId)).Select(x => x.NationId);
        producersNationIds.Should().AllBeEquivalentTo(NationId, "Because all nationIds should match requested value");
        
        var companiesHouseNumbers = _dbContext.Organisations.Where(x => x.IsComplianceScheme && organisationIdsReturned.Contains(x.ExternalId)).Select(x => x.CompaniesHouseNumber);
        var complianceSchemes  = 
            _dbContext.ComplianceSchemes
                .Where(x => companiesHouseNumbers.Contains(x.CompaniesHouseNumber)).ToList();
            
        var companiesHouseNumbersAndNationIds = complianceSchemes
                .GroupBy(cs => cs.CompaniesHouseNumber)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(cs => cs.NationId).ToList()
                );
        companiesHouseNumbersAndNationIds.Values.Should().OnlyContain(x => x.Contains(NationId), "Because all returned orgs (companiesHouseNumber) should have a compliance scheme in the requested nation");
    }

    [TestMethod]
    public async Task When_Pending_Applications_Requested_Without_Search_Without_Filter_For_Page_Ten_And_Page_Size_10_Then_Return_Empty_List()
    {
        //Act
        PaginatedResponse<OrganisationEnrolments> result = await _regulatorService.GetPendingApplicationsAsync(NationId, PageIndexTen, PageSizeTen,
            null, ApplicationTypeAll);
       
        //Assert
        result.Should().BeOfType(typeof(PaginatedResponse<OrganisationEnrolments>));
        result.Items.Count.Should().Be(0);
        result.TotalItems.Should().Be(4);
        result.CurrentPage.Should().Be(10);
    }
    
    [TestMethod]
    public async Task When_Pending_Applications_Requested_Without_Search_And_Without_Filter_For_Page_Two_And_Page_Size_2_Then_Return_Pending_Enrolments_Of_The_User_Nation()
    {
        //Act
        PaginatedResponse<OrganisationEnrolments> result = await _regulatorService.GetPendingApplicationsAsync(NationId, PageIndexTwo, PageSizeTwo,
            null, ApplicationTypeAll);
       
        //Assert
        result.Should().BeOfType(typeof(PaginatedResponse<OrganisationEnrolments>));
        result.Items.Count.Should().Be(2);
        result.TotalItems.Should().Be(4);
    }
    
    [TestMethod]
    public async Task When_Pending_Applications_Requested_Without_Search_And_Filter_For_Pending_Approved_Person_For_Page_Two_And_Page_Size_2_Then_Return_Pending_Enrolments_Of_The_User_Nation()
    {
        //Act
        PaginatedResponse<OrganisationEnrolments> result = await _regulatorService.GetPendingApplicationsAsync(NationId, PageIndexOne, PageSizeTwo,
            null, ApplicationTypeApproved);
       
        //Assert
        result.Should().BeOfType(typeof(PaginatedResponse<OrganisationEnrolments>));
        result.Items.Count.Should().Be(2);
        result.TotalItems.Should().Be(3);
    }
    
    [TestMethod]
    public async Task When_Pending_Applications_Requested_Without_Search_And_Filter_For_Pending_Delegated_Person_For_Page_Two_And_Page_Size_2_Then_Return_Pending_Enrolments_Of_The_User_Nation()
    {
        //Act
        PaginatedResponse<OrganisationEnrolments> result = await _regulatorService.GetPendingApplicationsAsync(NationId, PageIndexOne, PageSizeOne,
            null, ApplicationTypeDelegated);
       
        //Assert
        result.Should().BeOfType(typeof(PaginatedResponse<OrganisationEnrolments>));
        result.Items.Count.Should().Be(1);
        result.TotalItems.Should().Be(2);
    }
    
    [TestMethod]
    public async Task When_Pending_Applications_Requested_With_Search_For_Asda_And_Without_Filter_For_Page_One_And_Page_Size_2_Then_Return_Pending_Enrolments_Of_The_User_Nation()
    {
        //Act
        PaginatedResponse<OrganisationEnrolments> result = await _regulatorService.GetPendingApplicationsAsync(NationId, PageIndexOne, PageSizeTwo,
            SearchOrgName, ApplicationTypeAll);
       
        //Assert
        result.Should().BeOfType(typeof(PaginatedResponse<OrganisationEnrolments>));
        result.Items.Count.Should().Be(1);
        result.TotalItems.Should().Be(1);
    }

    [TestMethod]
    public async Task When_Pending_Applications_Requested_With_Search_For_Partial_Name_And_Without_Filter_For_Page_One_And_Page_Size_2_Then_Return_Pending_Enrolments_With_Matching_Organisation_Name()
    {
        //Act
        PaginatedResponse<OrganisationEnrolments> result = await _regulatorService.GetPendingApplicationsAsync(
            NationId, PageIndexOne, PageSizeTwo,
            "s", ApplicationTypeAll);

        //Assert
        result.Should().BeOfType(typeof(PaginatedResponse<OrganisationEnrolments>));
        result.Items.Count.Should().Be(2);
        result.TotalItems.Should().Be(3);
    }

    [TestMethod]
    public async Task When_UnKnown_Organisation_Enrolment_Requested_Then_Return_Empty_Application_Enrolment()
    {
        //Act
        var result = await _regulatorService.GetOrganisationEnrolmentDetails(Guid.NewGuid());

        //Assert
        result.Should().BeOfType<ApplicationEnrolmentDetails>();
        result.OrganisationId.Should().Be(Guid.Parse("00000000-0000-0000-0000-000000000000"));
    }

    [TestMethod]
    public async Task When_Organisations_Enrolments_Requested_Then_Return_Pending_Enrolments_For_Organisation()
    {
        //Act
        var result = await _regulatorService.GetOrganisationEnrolmentDetails(OrganisationId);

        //Assert
        result.Should().BeOfType(typeof(ApplicationEnrolmentDetails));
        result.Users.Any(x => x.Enrolments.ServiceRole == ServiceRole.Packaging.BasicUser.Key).Should().BeFalse();
        result.Users.Any(x => x.Enrolments.ServiceRole == ServiceRole.Packaging.ApprovedPerson.Key).Should().BeTrue();
        result.Users.Any(x => x.Enrolments.ServiceRole == ServiceRole.Packaging.DelegatedPerson.Key).Should().BeTrue();
    }
    
    [TestMethod]
    public async Task When_Organisations_Enrolments_Requested_Then_Return_Transfer_Details()
    {
        //Act
        var result = await _regulatorService.GetOrganisationEnrolmentDetails(TransferredOrganisationId);
        var expectedComment = _dbContext.Enrolments
            .Include(e => e.RegulatorComments)
            .Single(e => e.Connection.Organisation.ExternalId == TransferredOrganisationId)
            .RegulatorComments.First();

        //Assert
        result.Should().BeOfType(typeof(ApplicationEnrolmentDetails));
        result.TransferDetails.OldNationId.Should().Be(Nation.Scotland);
        result.TransferDetails.TransferredDate.Should().Be(expectedComment.CreatedOn);
    }
    
    [TestMethod]
    public async Task When_Organisations_Enrolments_Requested_Then_Return_Pending_Enrolments_For_Organisation_Type_Companies_House()
    {
        //Act
        var result = await _regulatorService.GetOrganisationEnrolmentDetails(OrganisationId);
        
        //Assert
        Assert.AreEqual("Companies House Company", result.OrganisationType);
    }
    
    [TestMethod]
    public async Task When_Organisations_Enrolments_Requested_Then_Return_Pending_Enrolments_For_Producer_Type_Sole_Trader()
    {
        //Act
        var result = await _regulatorService.GetOrganisationEnrolmentDetails(OrganisationIdSoleTrader);
        
        //Assert
        Assert.AreEqual("Sole trader", result.OrganisationType);
    }

    [TestMethod]
    public async Task When_Authorising_Regulator_And_Organisation_Name_Is_Same_As_Regulator_Then_Return_True()
    {
        //Act
        var result = _regulatorService.DoesRegulatorNationMatchOrganisationNation(RegulatorUserId, OrganisationId);

        //Assert
        result.Should().BeTrue();
    }
    
    [TestMethod]
    public async Task When_Authorising_Regulator_And_Compliance_Scheme_Organisaiton_Are_Same_As_Regulator_Nation_Then_Return_True()
    {
        //Act
        var result = _regulatorService.DoesRegulatorNationMatchOrganisationNation(RegulatorUserId, CsOrganisation);

        //Assert
        result.Should().BeTrue();
    }
    
    [TestMethod]
    public async Task When_Authorising_Regulator_And_Organisation_Name_Is_Not_Same_As_Regulator_Then_Return_False()
    {
        //Act
        var result = _regulatorService.DoesRegulatorNationMatchOrganisationNation(RegulatorUserId, CsOrganisation2);

        //Assert
        result.Should().BeFalse();
    }
    
    [TestMethod]
    public async Task When_Regulator_Nation_Is_Requested_Then_Return_Nation_Id()
    {
        //Act
        var result = _regulatorService.GetRegulatorNationId(RegulatorUserId);

        //Assert
        result.Should().NotBe(0);
    }
    
    [TestMethod]
    public async Task When_Regulator_Nation_Is_Requested_And_User_Does_Not_Exist_Then_Return_Nation_Id_As_Zero()
    {
        //Act
        var result = _regulatorService.GetRegulatorNationId(Guid.NewGuid());

        //Assert
        result.Should().Be(0);
    }
    
    [TestMethod]
    public async Task When_Update_Enrolment_Is_Requested_And_Enrolment_IS_Not_Found_Then_Return_Failure()
    {
        //Act
        var result = await
            _regulatorService.UpdateEnrolmentStatusForUserAsync(RegulatorUserId, OrganisationId, Guid.NewGuid(), ApprovedStatus,
                null);

        //Assert
        
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be(InvalidEnrolmentMessage);
    }
    
    [TestMethod]
    public async Task When_Update_Enrolment_Is_Requested_And_Enrolment_Status_Is_Not_Accepted_Or_Rejected_Then_Return_Failure()
    {
        //Act
        var result = await
            _regulatorService.UpdateEnrolmentStatusForUserAsync(RegulatorUserId, OrganisationId, DelegatedPersonId, "Enrolled",
                null);

        //Assert
        
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be(InvalidEnrolmentStatusMessage);
    }
    
    [TestMethod]
    public async Task When_Update_Enrolment_Is_Requested_And_Approved_Person_Is_Accepted_Then_Return_Success()
    {
        //Act
        var result = await
            _regulatorService.UpdateEnrolmentStatusForUserAsync(RegulatorUserId, OrganisationId, ApprovedEnrolmentId2, ApprovedStatus,
                null);

        //Assert
        
        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        _dbContext.Enrolments.SingleOrDefault(e =>
                e.ExternalId == ApprovedEnrolmentId2 && e.ServiceRoleId == ServiceRole.Packaging.ApprovedPerson.Id
                                                 && e.EnrolmentStatusId == EnrolmentStatus.Approved)
            .Should().NotBeNull();
    }
    
    [TestMethod]
    public async Task When_Update_Enrolment_Is_Requested_And_Delegated_Person_Is_Accepted_Then_Return_Success()
    {
        //Act
        var result = await
            _regulatorService.UpdateEnrolmentStatusForUserAsync(RegulatorUserId, OrganisationId, DelegatedPersonId, ApprovedStatus,
                null);

        //Assert
        
        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        _dbContext.Enrolments.SingleOrDefault(e =>
                e.ExternalId == DelegatedPersonId && e.ServiceRoleId == ServiceRole.Packaging.DelegatedPerson.Id
                                                  && e.EnrolmentStatusId == EnrolmentStatus.Approved)
            .Should().NotBeNull();
    }
    
    [TestMethod]
    public async Task When_Update_Enrolment_Is_Requested_And_Delegated_Person_Is_Rejected_Then_Save_Rejected_Comments_And_Create_Basic_Enrolment_And_Return_Success()
    {
        //Act
        var result = await
            _regulatorService.UpdateEnrolmentStatusForUserAsync(RegulatorUserId, OrganisationId, DelegatedPersonId, RejectedStatus,
                RejectedComment);

        //Assert
        
        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        var rejectedEnrolment = _dbContext.Enrolments.IgnoreQueryFilters().SingleOrDefault(e =>
            e.ExternalId == DelegatedPersonId && e.ServiceRoleId == ServiceRole.Packaging.DelegatedPerson.Id
                                              && e.EnrolmentStatusId == EnrolmentStatus.Rejected
                                              && e.IsDeleted);
        rejectedEnrolment.Should().NotBeNull();

        _dbContext.RegulatorComments
            .SingleOrDefault(e => e.EnrolmentId == rejectedEnrolment.Id && e.RejectedComments.Equals(RejectedComment))
            .Should().NotBeNull();
        
        _dbContext.Enrolments
            .SingleOrDefault(e => e.ConnectionId == rejectedEnrolment.ConnectionId && e.EnrolmentStatusId == EnrolmentStatus.Enrolled && e.ServiceRoleId == ServiceRole.Packaging.BasicUser.Id)
            .Should().NotBeNull();
    }

    [TestMethod]
    public async Task
        When_Update_Enrolment_Is_Requested_And_Approved_Person_Is_Rejected_Then_Save_Rejected_Comments_And_Soft_Delete_Organisation_And_Related_Records_And_Return_Success()
    {
        //Act
        var result = await
            _regulatorService.UpdateEnrolmentStatusForUserAsync(RegulatorUserId, OrganisationId, ApprovedEnrolmentId2,
                RejectedStatus,
                RejectedComment);

        //Assert

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        var rejectedEnrolment = _dbContext.Enrolments.IgnoreQueryFilters().SingleOrDefault(e =>
            e.ExternalId == ApprovedEnrolmentId2 && e.ServiceRoleId == ServiceRole.Packaging.ApprovedPerson.Id
                                             && e.EnrolmentStatusId == EnrolmentStatus.Rejected
                                             && e.IsDeleted);
        rejectedEnrolment.Should().NotBeNull();

        _dbContext.RegulatorComments
            .SingleOrDefault(e => e.EnrolmentId == rejectedEnrolment.Id && e.RejectedComments.Equals(RejectedComment))
            .Should().NotBeNull();
        _dbContext.Organisations.IgnoreQueryFilters()
            .SingleOrDefault(org => org.ExternalId == OrganisationId && org.IsDeleted).Should().NotBeNull();
        _dbContext.PersonOrganisationConnections.IgnoreQueryFilters()
            .Any(con => con.Organisation.ExternalId == OrganisationId && !con.IsDeleted).Should().BeFalse();

        _dbContext.Users.IgnoreQueryFilters().Any(user =>
                user.Person.OrganisationConnections.Where(org =>
                        org.Organisation.ExternalId == OrganisationId)
                    .Select(org => org.Person.UserId).Contains(user.Id) && !user.IsDeleted &&
                user.UserId != MultiOrgUserId)
            .Should().BeFalse();

        _dbContext.Users.Any(user => user.UserId == MultiOrgUserId && !user.IsDeleted).Should().BeTrue();

        _dbContext.Persons.IgnoreQueryFilters().Any(person =>
            person.OrganisationConnections.Where(org => org.Organisation.ExternalId == OrganisationId)
                .Select(org => org.Person.UserId).Contains(person.Id) && !person.IsDeleted &&
            person.User.UserId != MultiOrgUserId).Should().BeFalse();

        _dbContext.Persons.Any(person => person.User.UserId == MultiOrgUserId && !person.IsDeleted).Should().BeTrue();

        _dbContext.Enrolments.IgnoreQueryFilters()
            .Any(enrolments => enrolments.Connection.Organisation.ExternalId == OrganisationId && !enrolments.IsDeleted)
            .Should().BeFalse();
    }
    
    [TestMethod]
    public async Task
        When_Update_Enrolment_Is_Requested_And_Delegated_Person_Is_Rejected_Then_Do_Not_Set_user_id_to_default_Guid()
    {
        //Act
        var result = await
            _regulatorService.UpdateEnrolmentStatusForUserAsync(RegulatorUserId, OrganisationId, DelegatedPersonId,
                RejectedStatus,
                RejectedComment);

        //Assert

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        var rejectedEnrolment = _dbContext.Enrolments.IgnoreQueryFilters().SingleOrDefault(e =>
            e.ExternalId == DelegatedPersonId && e.ServiceRoleId == ServiceRole.Packaging.DelegatedPerson.Id
                                              && e.EnrolmentStatusId == EnrolmentStatus.Rejected
                                              && e.IsDeleted);
        rejectedEnrolment.Should().NotBeNull();
        
        var user = _dbContext.Users.IgnoreQueryFilters().SingleOrDefault(e => e.UserId == Guid.Empty);

        Assert.IsNull(user);
    }
    
    [TestMethod]
    public async Task
        When_Update_Enrolment_Is_Requested_And_Approved_Person_Is_Rejected_Then_Set_user_id_to_default_Guid()
    {
        //Act
        var result = await
            _regulatorService.UpdateEnrolmentStatusForUserAsync(RegulatorUserId, OrganisationId, ApprovedEnrolmentId2,
                RejectedStatus,
                RejectedComment);

        //Assert

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        var rejectedEnrolment = _dbContext.Enrolments.IgnoreQueryFilters().SingleOrDefault(e =>
            e.ExternalId == ApprovedEnrolmentId2 && e.ServiceRoleId == ServiceRole.Packaging.ApprovedPerson.Id
                                             && e.EnrolmentStatusId == EnrolmentStatus.Rejected
                                             && e.IsDeleted);
        rejectedEnrolment.Should().NotBeNull();

        var user = _dbContext.Users.IgnoreQueryFilters().SingleOrDefault(e => e.UserId == Guid.Empty);

        Assert.IsNotNull(user);
        Assert.AreSame(user.Email, ApprovedUser2ProducerEmail);
        _dbContext.Users.Any(user1 => user1.UserId == ApprovedUserId2).Should().BeFalse();
        _dbContext.Users.Any(user1 => user1.UserId == RegulatorUserId).Should().BeTrue();
    }

    [TestMethod]
    public async Task
        When_Transfer_Enrolment_Is_Requested_Then_Save_Transfer_Comments_And_Change_Nation_Of_Organisation_And_Return_Success()
    {
        //Arrange
        var request = new OrganisationTransferNationRequest
        {
            OrganisationId = OrganisationId,
            TransferComments = TransferComment,
            TransferNationId = 3,
            UserId = RegulatorUserId
        };
        //Act
        var result = await
            _regulatorService.TransferOrganisationNation(request);

        //Assert
        
        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        _dbContext.Organisations.Any(org=>org.ExternalId == OrganisationId && org.Nation.Name == "Scotland").Should().BeTrue();
        _dbContext.Organisations.Any(org=>org.ExternalId == OrganisationId && org.TransferNation.Name == "England").Should().BeTrue();

        _dbContext.RegulatorComments
            .Include(rc => rc.Enrolment)
            .ThenInclude(e => e.Connection)
            .ThenInclude(c => c.Organisation)
        .Count(x => x.Enrolment.Connection.Organisation.ExternalId==OrganisationId).Should().Be(1);
    }
    
    [TestMethod]
    public async Task
        When_Transfer_Enrolment_Is_Requested_And_Nation_Is_Invalid_Then_Return_Error()
    {
        //Arrange
        var request = new OrganisationTransferNationRequest
        {
            OrganisationId = OrganisationId,
            TransferComments = TransferComment,
            TransferNationId = 6,
            UserId = RegulatorUserId
        };
        //Act
        var result = await
            _regulatorService.TransferOrganisationNation(request);
        
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid Nation");
    }
    
    [TestMethod]
    public async Task When_Transfer_Enrolment_Is_Requested_For_Compliance_Scheme_Then_Return_Error()
    {
        //Arrange
        var request = new OrganisationTransferNationRequest
        {
            OrganisationId = CsOrganisation,
            TransferComments = TransferComment,
            TransferNationId = 6,
            UserId = RegulatorUserId
        };
        //Act
        var result = await
            _regulatorService.TransferOrganisationNation(request);
        
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cannot transfer compliance scheme");
    }
    
    [TestMethod]
    public async Task When_User_List_For_Regulator_Requested_Then_Return_List_With_Approved_Users_Only()
    {
        // Arrange
        var enrolment = _dbContext.Enrolments.SingleOrDefault(x => x.ExternalId == DelegatedPersonId);
        enrolment.EnrolmentStatusId = EnrolmentStatus.Approved;
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        
        // Act
        var userList = await _regulatorService.GetUserListForRegulator(OrganisationId, true);

        //Assert
        userList.Should().NotBeNull();
        userList.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task When_Organisation_Data_Requested_Then_Return_Object_With_Organisation_Details()
    {
        // act 
        var organisationData = await _regulatorService.GetCompanyDetailsById(OrganisationId);
        
        // Assert
        organisationData.Should().NotBeNull();
        organisationData.Company.Should().NotBeNull();
        organisationData.CompanyUserInformation.Count().Should().BeGreaterThan(1);
    }
    
    [TestMethod]
    public async Task When_Organisation_Data_Requested_Then_Do_Not_Include_Users_With_Wrong_Status()
    {
        // act 
        var organisationData = await _regulatorService.GetCompanyDetailsById(OrganisationId);
        
        // Assert
        organisationData.CompanyUserInformation.Should().Contain(u =>
            u.ExternalId.Equals(BasicUserExternalId1));
        organisationData.CompanyUserInformation.Should().NotContain(u =>
            u.ExternalId.Equals(BasicUserExternalId2));
    }

 /* Remove only */
    [TestMethod]
    public async Task RemoveApprovedPerson_OnlyRemoveNoPromote_When_Different_Org_ShouldNot_Delete_ApprovedUsers_And_Return_Fail()
    {
        // Arrange
        var organisation = SetUpOrganisation(_dbContext);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        
        var request = new ApprovedUserRequest
        {
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.NewGuid(),
            OrganisationId = organisation.ExternalId,
            PromotedPersonExternalId = Guid.Empty
        };
        
       // Act
        var result = await _regulatorService.RemoveApprovedPerson(request);
        
        // Assert
        result.Should().BeOfType<List<AssociatedPersonResponseModel>>();
    }
    
   [TestMethod]
    public async Task RemoveApprovedPerson_OnlyRemoveNoPromote_When_Valid_Data_Passed_ApprovedUser_Should_Be_Deleted_And_DelegatedPerson_Should_Be_Demoted()
    {
        // Arrange
        var organisation = SetUpOrganisation(_dbContext);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        var connExternalId = organisation.PersonOrganisationConnections.FirstOrDefault().ExternalId;
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
 
        var request = new ApprovedUserRequest
        {
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId =connExternalId,
            OrganisationId = organisation.ExternalId
        };
        
        // Act
        var result = await _regulatorService.RemoveApprovedPerson(request);
        
        // Assert
        result.Should().BeOfType<List<AssociatedPersonResponseModel>>();
        result.FirstOrDefault().EmailNotificationType.Should().Be(EmailNotificationType.RemovedApprovedUser);
        result.LastOrDefault().EmailNotificationType.Should().Be(EmailNotificationType.DemotedDelegatedUsed);
       
        organisation.PersonOrganisationConnections.LastOrDefault().Enrolments.SingleOrDefault().ServiceRoleId.Should()
            .Be(ServiceRole.Packaging.BasicUser.Id);
        organisation.PersonOrganisationConnections.LastOrDefault().PersonRoleId.Should().Be(2);
        
        var approvedUser = organisation.PersonOrganisationConnections.FirstOrDefault();
        approvedUser.IsDeleted.Should().BeTrue();
        approvedUser.Enrolments.FirstOrDefault().IsDeleted.Should().BeTrue();
       
    }
    
    [TestMethod]
    public async Task RemoveApprovedPerson_OnlyRemoveNoPromote_When_Valid_Data_Passed_Data_Is_Passed_For_Email_Notification_For_Enrolled_Users()
    {
        // Arrange
        var organisation = SetUpOrganisation(_dbContext);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        _ = SetUpDelegatedPersonWithoutFirstNameEnrolment(organisation.Id);
        var connExternalId = organisation.PersonOrganisationConnections.FirstOrDefault().ExternalId;
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);

        var request = new ApprovedUserRequest
        {
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = connExternalId,
            OrganisationId = organisation.ExternalId,
            PromotedPersonExternalId = Guid.Empty
        };
        // Act
        var result = await _regulatorService.RemoveApprovedPerson(request);
        
        // Assert
        result.Should().BeOfType<List<AssociatedPersonResponseModel>>();
        result.Count.Should().Be(2);
        result.FirstOrDefault().EmailNotificationType.Should().Be(EmailNotificationType.RemovedApprovedUser);
        result.LastOrDefault().EmailNotificationType.Should().Be(EmailNotificationType.DemotedDelegatedUsed);
    }
    
    /* Promote only */
    [TestMethod]
    public async Task RemoveApprovedPerson_OnlyPromoteNoRemove_When_Different_Org_ShouldNot_Delete_ApprovedUsers_And_Return_Fail()
    {
        // Arrange
        var organisation = SetUpOrganisation(_dbContext);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        
        var request = new ApprovedUserRequest
        {
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.Empty,
            OrganisationId = organisation.ExternalId,
            PromotedPersonExternalId = Guid.NewGuid()
        };
        
        // Act
        var result = await _regulatorService.RemoveApprovedPerson(request);
        
        // Assert
        result.Should().BeOfType<List<AssociatedPersonResponseModel>>();
    }

    [TestMethod]
    public async Task RemoveApprovedPerson_OnlyPromoteNoRemove_When_Valid_Data_Passed_PromotedUser_Should_Be_Added_As_ApprovedUser()
    {
        // Arrange
        var organisation = SetUpOrganisation(_dbContext);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.BasicUser.Id);
        var personExternalId = organisation.PersonOrganisationConnections.LastOrDefault().Person.ExternalId;
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
 
        var request = new ApprovedUserRequest
        {
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.Empty,
            OrganisationId = organisation.ExternalId,
            PromotedPersonExternalId = personExternalId
        };
        
        // Act
        var result = await _regulatorService.RemoveApprovedPerson(request);
        
        // Assert
        result.Should().BeOfType<List<AssociatedPersonResponseModel>>();

        result.Count.Should().Be(1);
        result.FirstOrDefault().EmailNotificationType.Should().Be(EmailNotificationType.PromotedApprovedUser);
        result.FirstOrDefault().ServiceRoleId.Should().Be(ServiceRole.Packaging.ApprovedPerson.Id);
        organisation.PersonOrganisationConnections.FirstOrDefault().Enrolments.Count.Should().Be(2);
        organisation.PersonOrganisationConnections.FirstOrDefault().Enrolments.LastOrDefault().EnrolmentStatusId.Should()
            .Be(EnrolmentStatus.Nominated);
    }
    
    [TestMethod]
    public async Task RemoveApprovedPerson_OnlyPromoteNoRemove_When_Valid_Data_Passed_PromotedUser_Should_Be_Added_As_ApprovedUser_And_Delegated_Users_Demoted()
    {
        // Arrange
        var organisation = SetUpOrganisation(_dbContext);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.BasicUser.Id);
        var personExternalId = organisation.PersonOrganisationConnections.LastOrDefault().Person.ExternalId;
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
 
        var request = new ApprovedUserRequest
        {
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = Guid.Empty,
            OrganisationId = organisation.ExternalId,
            PromotedPersonExternalId = personExternalId
        };
        
        // Act
        var result = await _regulatorService.RemoveApprovedPerson(request);
        
        // Assert
        result.Should().BeOfType<List<AssociatedPersonResponseModel>>();

        result.Count.Should().Be(2);
        result.FirstOrDefault().EmailNotificationType.Should().Be(EmailNotificationType.PromotedApprovedUser);
        result.LastOrDefault().EmailNotificationType.Should().Be(EmailNotificationType.DemotedDelegatedUsed);
        result.FirstOrDefault().ServiceRoleId.Should().Be(ServiceRole.Packaging.ApprovedPerson.Id);
        result.LastOrDefault().ServiceRoleId.Should().Be(ServiceRole.Packaging.BasicUser.Id);
        organisation.PersonOrganisationConnections.LastOrDefault().Enrolments.Count.Should().Be(2);
        organisation.PersonOrganisationConnections.LastOrDefault().Enrolments.LastOrDefault().EnrolmentStatusId.Should()
            .Be(EnrolmentStatus.Nominated);
    }
    
    /* Remove and Promote */
    [TestMethod]
    public async Task RemoveApprovedPerson_RemoveApAndPromoteExistingUser_When_Valid_Data_Passed_PromotedUser_Should_Be_Added_As_ApprovedUser()
    {
        var organisation = SetUpOrganisation(_dbContext);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.BasicUser.Id);
        
        var connExternalId = organisation.PersonOrganisationConnections.FirstOrDefault().ExternalId;
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        var personExternalId = organisation.PersonOrganisationConnections.LastOrDefault().Person.ExternalId;
        
        var request = new ApprovedUserRequest
        {
            UserId = Guid.NewGuid(),
            RemovedConnectionExternalId = connExternalId,
            OrganisationId = organisation.ExternalId,
            PromotedPersonExternalId = personExternalId
        };
        
        // Act
        var result = await _regulatorService.RemoveApprovedPerson(request);
        
        // Assert
        result.Should().BeOfType<List<AssociatedPersonResponseModel>>();

        result.Count.Should().Be(3);
        result[0].EmailNotificationType.Should().Be(EmailNotificationType.RemovedApprovedUser);
        result[0].ServiceRoleId.Should().Be(ServiceRole.Packaging.ApprovedPerson.Id);
        result[1].EmailNotificationType.Should().Be(EmailNotificationType.PromotedApprovedUser);
        result[1].ServiceRoleId.Should().Be(ServiceRole.Packaging.ApprovedPerson.Id);
        result[2].EmailNotificationType.Should().Be(EmailNotificationType.DemotedDelegatedUsed);
        result[2].ServiceRoleId.Should().Be(ServiceRole.Packaging.BasicUser.Id);
       
        organisation.PersonOrganisationConnections.LastOrDefault().Enrolments.Count.Should().Be(2);
        organisation.PersonOrganisationConnections.LastOrDefault().Enrolments.LastOrDefault().EnrolmentStatusId.Should()
            .Be(EnrolmentStatus.Nominated);
    }
    
    [TestMethod]
    public async Task RemovedConnectionExternalIdDoesNotBelongToOrganisation_AddRemoveApprovedPerson_ReturnNoTokenAndDemotedUsers()
    {
        // Arrange
        var organisation = SetUpOrganisation(_dbContext);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        
        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = organisation.ExternalId,
            InvitedPersonEmail = "test1@test.com",
            RemovedConnectionExternalId = Guid.NewGuid()
        };
        
        // Act
        var result = await _regulatorService.AddRemoveApprovedPerson(request);
        
        // Assert
        result.InviteToken.Should().BeNullOrWhiteSpace();
        result.AssociatedPersonList.Count.Should().Be(0);
        result.OrganisationReferenceNumber.Should().Be(organisation.ReferenceNumber);
        result.OrganisationName.Should().Be(organisation.Name);
    }
    
    [TestMethod]
    public async Task RemovedConnectionExternalIdDoesNotBelongToOrganisation_AddRemoveApprovedPerson_ShouldNotChangeOrgDelegatedAndApprovedUsers()
    {
        // Arrange
        var organisation = SetUpOrganisation(_dbContext);
        var approvedPersonId = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id).Id;
        var delegatedPersonId = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id).Id;
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        
        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = organisation.ExternalId,
            InvitedPersonEmail = "test1@test.com",
            RemovedConnectionExternalId = Guid.NewGuid()
        };
        
        // Act
        _ = await _regulatorService.AddRemoveApprovedPerson(request);
        
        // Assert
        var approvedPerson = _dbContext.Persons.Single(x => x.Id == approvedPersonId);
        approvedPerson.IsDeleted.Should().BeFalse();
        
        var delegatedPersonEnrolment = _dbContext.Enrolments
            .Include(x => x.Connection)
            .Include(x => x.Connection.Person)
            .Single(x => x.Connection.Person.Id == delegatedPersonId);

        delegatedPersonEnrolment.ServiceRoleId.Should().Be(ServiceRole.Packaging.DelegatedPerson.Id);
        delegatedPersonEnrolment.IsDeleted.Should().BeFalse();
    }
    
    [TestMethod]
    public async Task RemovedConnectionExternalIdIsNotProvided_AddRemoveApprovedPerson_ShouldNotRemoveApprovedUser()
    {
        var token = "SomeToken";
        // Arrange
        _tokenService
            .Setup(x => x.GenerateInviteToken())
            .Returns(token);
        
        var organisation = SetUpOrganisation(_dbContext);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        
        var regulatorPerson = SetUpPersonEnrolment(organisation.Id, ServiceRole.Regulator.Admin.Id);
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        
        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = organisation.ExternalId,
            InvitedPersonEmail = $"test+{Guid.NewGuid()}@test.com",
            AddingOrRemovingUserEmail = regulatorPerson.Email,
            AddingOrRemovingUserId = regulatorPerson.ExternalId
        };
        
        // Act
        var result = await _regulatorService.AddRemoveApprovedPerson(request);
        
        // Assert
        result.InviteToken.Should().Be(token);
        result.AssociatedPersonList.Count.Should().Be(0);
        result.OrganisationReferenceNumber.Should().Be(organisation.ReferenceNumber);
        result.OrganisationName.Should().Be(organisation.Name);
    }
    
    [TestMethod]
    public async Task RemovedConnectionExternalIdIsProvided_AddRemoveApprovedPerson_ShouldReturnTokenAndDemotedBasicUsers()
    {
        var token = "SomeToken";
        // Arrange
        _tokenService
            .Setup(x => x.GenerateInviteToken())
            .Returns(token);
        
        var organisation = SetUpOrganisation(_dbContext);
        var approvedPersonEnrolment = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        
        var regulatorPerson = SetUpPersonEnrolment(organisation.Id, ServiceRole.Regulator.Admin.Id);
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        
        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = organisation.ExternalId,
            InvitedPersonEmail = $"test+{Guid.NewGuid()}@test.com",
            AddingOrRemovingUserEmail = regulatorPerson.Email,
            AddingOrRemovingUserId = regulatorPerson.ExternalId,
            RemovedConnectionExternalId = approvedPersonEnrolment.OrganisationConnections.First().ExternalId
        };
        
        // Act
        var result = await _regulatorService.AddRemoveApprovedPerson(request);
        
        // Assert
        result.InviteToken.Should().Be(token);
        result.AssociatedPersonList.Count.Should().Be(3);
        result.OrganisationReferenceNumber.Should().Be(organisation.ReferenceNumber);
        result.OrganisationName.Should().Be(organisation.Name);
    }
    
    [TestMethod]
    public async Task RemovedConnectionExternalIdIsProvided_AddRemoveApprovedPerson_ShouldCreateNewlyInviteeAccount()
    {
        var token = "SomeToken";
        // Arrange
        _tokenService
            .Setup(x => x.GenerateInviteToken())
            .Returns(token);
        
        var organisation = SetUpOrganisation(_dbContext);
        var approvedPersonEnrolment = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        
        var regulatorPerson = SetUpPersonEnrolment(organisation.Id, ServiceRole.Regulator.Admin.Id);
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        
        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = organisation.ExternalId,
            InvitedPersonEmail = $"test+{Guid.NewGuid()}@test.com",
            AddingOrRemovingUserEmail = regulatorPerson.Email,
            AddingOrRemovingUserId = regulatorPerson.ExternalId,
            RemovedConnectionExternalId = approvedPersonEnrolment.OrganisationConnections.First().ExternalId
        };
        
        // Act
        await _regulatorService.AddRemoveApprovedPerson(request);
        
        // Assert
        var newlyInvitedUser = GetEnrolmentQuery()
            .Single(x => x.Connection.Person.Email == request.InvitedPersonEmail);

        newlyInvitedUser.Connection.PersonRoleId.Should().Be(PersonRole.Employee);
        newlyInvitedUser.ServiceRoleId.Should().Be(ServiceRole.Packaging.ApprovedPerson.Id);
    }
    
    [TestMethod]
    public async Task RemovedConnectionExternalIdIsProvided_AddRemoveApprovedPerson_ShouldDemoteDelegatedUsersToBasic()
    {
        var token = "SomeToken";
        // Arrange
        _tokenService
            .Setup(x => x.GenerateInviteToken())
            .Returns(token);
        
        var organisation = SetUpOrganisation(_dbContext);
        var approvedPersonEnrolment = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        var existingDelegatedPerson1 = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        var existingDelegatedPerson2 = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        
        var regulatorPerson = SetUpPersonEnrolment(organisation.Id, ServiceRole.Regulator.Admin.Id);
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        
        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = organisation.ExternalId,
            InvitedPersonEmail = $"test+{Guid.NewGuid()}@test.com",
            AddingOrRemovingUserEmail = regulatorPerson.Email,
            AddingOrRemovingUserId = regulatorPerson.ExternalId,
            RemovedConnectionExternalId = approvedPersonEnrolment.OrganisationConnections.First().ExternalId
        };
        
        // Act
        var result = await _regulatorService.AddRemoveApprovedPerson(request);
        
        // Assert
        var approvedUser = GetEnrolmentQuery()
            .Single(x => x.Id == approvedPersonEnrolment.Id);
        
        approvedUser.Connection.PersonRoleId.Should().Be(PersonRole.Admin);
        approvedUser.ServiceRoleId.Should().Be(ServiceRole.Packaging.ApprovedPerson.Id);
       
        var delegatedPersonEnrolment1 = GetEnrolmentQuery()
            .Single(x => x.Connection.Person.Id == existingDelegatedPerson1.Id);
        
        delegatedPersonEnrolment1.Connection.PersonRoleId.Should().Be(PersonRole.Employee);
        delegatedPersonEnrolment1.ServiceRoleId.Should().Be(ServiceRole.Packaging.BasicUser.Id);
        
        var delegatedPersonEnrolment2 = GetEnrolmentQuery()
            .Single(x => x.Connection.Person.Id == existingDelegatedPerson2.Id);
        
        delegatedPersonEnrolment2.Connection.PersonRoleId.Should().Be(PersonRole.Employee);
        delegatedPersonEnrolment2.ServiceRoleId.Should().Be(ServiceRole.Packaging.BasicUser.Id);
        
        result.AssociatedPersonList.Count.Should().Be(3);
        result.AssociatedPersonList.Should().Contain(x => x.Email == delegatedPersonEnrolment1.Connection.Person.Email);
        result.AssociatedPersonList.Should().Contain(x => x.Email == delegatedPersonEnrolment2.Connection.Person.Email);
        result.AssociatedPersonList.Should().Contain(x => x.Email == approvedPersonEnrolment.Email);
        
        result.AssociatedPersonList[0].EmailNotificationType.Should().Be(EmailNotificationType.RemovedApprovedUser);
        result.AssociatedPersonList[1].EmailNotificationType.Should().Be(EmailNotificationType.DemotedDelegatedUsed);
        result.AssociatedPersonList[2].EmailNotificationType.Should().Be(EmailNotificationType.DemotedDelegatedUsed);
    }
    
    [TestMethod]
    public async Task RemovedConnectionExternalIdIsProvided_AddRemoveApprovedPerson_ShouldDeleteGivenApprovedUser()
    {
        var token = "SomeToken";
        // Arrange
        _tokenService
            .Setup(x => x.GenerateInviteToken())
            .Returns(token);
        
        var organisation = SetUpOrganisation(_dbContext);
        var approvedPersonEnrolment = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.ApprovedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        _ = SetUpPersonEnrolment(organisation.Id, ServiceRole.Packaging.DelegatedPerson.Id);
        
        var regulatorPerson = SetUpPersonEnrolment(organisation.Id, ServiceRole.Regulator.Admin.Id);
        await _dbContext.SaveChangesAsync(Guid.Empty, Guid.Empty);
        
        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = organisation.ExternalId,
            InvitedPersonEmail = $"test+{Guid.NewGuid()}@test.com",
            AddingOrRemovingUserEmail = regulatorPerson.Email,
            AddingOrRemovingUserId = regulatorPerson.ExternalId,
            RemovedConnectionExternalId = approvedPersonEnrolment.OrganisationConnections.First().ExternalId
        };
        
        // Act
        _ = await _regulatorService.AddRemoveApprovedPerson(request);
        
        // Assert
        var deletedApprovedUserEnrolment = GetEnrolmentQuery()
            .Single(x => x.Id == approvedPersonEnrolment.Id);

        deletedApprovedUserEnrolment.IsDeleted.Should().BeFalse();
        deletedApprovedUserEnrolment.Connection.Person.IsDeleted.Should().BeFalse();
        deletedApprovedUserEnrolment.Connection.IsDeleted.Should().BeFalse();
    }

    private IIncludableQueryable<Enrolment, Person> GetEnrolmentQuery()
    {
        return _dbContext
            .Enrolments
            .Include(x => x.Connection)
            .Include(x => x.Connection.Person);
    }
    
    private static void SetUpDatabase(AccountsDbContext setupContext)
    {
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();
        
        var multiOrgPerson = new Person
        {
            FirstName = "Multi Org",
            LastName = "User1",
            Email = "multiorguser@abc.com",
            Telephone = "0123456789",
            User = new User
            {
                UserId = MultiOrgUserId,
                Email = "multiorguser@test.com"
            }
        };
        setupContext.Persons.Add(multiOrgPerson);
        var regulatorEnrolment = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Approved,
            ServiceRoleId = ServiceRole.Regulator.Basic.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = "Regulator",
                    OrganisationTypeId = OrganisationType.Regulators,
                    NationId = Nation.England
                },
                Person = new()
                {
                    FirstName = "Regulator",
                    LastName = "User",
                    Email = "regulator@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = RegulatorUserId,
                        Email = "regulator@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            }
        };
        setupContext.Enrolments.Add(regulatorEnrolment);
        var approvedEnrolment1 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Pending,
            ServiceRoleId = ServiceRole.Packaging.ApprovedPerson.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = "Producer Org 1 - ASDA",
                    OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
                    CompaniesHouseNumber = "11111111",
                    NationId = Nation.England,
                    TransferNationId = Nation.Scotland,
                    ExternalId = TransferredOrganisationId
                },
                Person = new()
                {
                    FirstName = "Producer",
                    LastName = "User1",
                    Email = "testuser1@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = ApprovedUserId1,
                        Email = "producer.user1@test.com"
                    },
                    ExternalId = Guid.NewGuid()
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            }
        };
        setupContext.Enrolments.Add(approvedEnrolment1);

        var regulatorComment = new RegulatorComment
        {
            TransferComments = "Some reason to transfer",
            Enrolment = approvedEnrolment1
        };
        setupContext.RegulatorComments.Add(regulatorComment);
        
        var delegatedEnrolment1 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Pending,
            ServiceRoleId = ServiceRole.Packaging.DelegatedPerson.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = "Producer Org 2 - Lidl",
                    OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
                    CompaniesHouseNumber = "22222222",
                    NationId = Nation.England,
                    ExternalId = Guid.NewGuid()
                },
                Person = new()
                {
                    FirstName = "Producer",
                    LastName = "User2",
                    Email = "testuser2@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = Guid.NewGuid(),
                        Email = "producer.user2@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            }
        };
        setupContext.Enrolments.Add(delegatedEnrolment1);
        
        var delegatedOtherNationEnrolment1 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Pending,
            ServiceRoleId = ServiceRole.Packaging.DelegatedPerson.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = OtherNationOrgName,
                    OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
                    CompaniesHouseNumber = "33333333",
                    NationId = Nation.Scotland
                },
                Person = new()
                {
                    FirstName = "Producer",
                    LastName = "User3",
                    Email = "testuser3@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = Guid.NewGuid(),
                        Email = "producer.user3@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = null
        };
        setupContext.Enrolments.Add(delegatedOtherNationEnrolment1);
        
        var producerOrg4 = new Organisation
        {
            Name = "Producer Org 4 - Spar",
            OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
            CompaniesHouseNumber = "44444444",
            NationId = Nation.England,
            ExternalId = OrganisationId
        };
        setupContext.Organisations.Add(producerOrg4);
        
        var approvedEnrolment2 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Pending,
            ServiceRoleId = ServiceRole.Packaging.ApprovedPerson.Id,
            ExternalId = ApprovedEnrolmentId2,
            Connection = new PersonOrganisationConnection
            {
                OrganisationId = 5,
                Person = new()
                {
                    FirstName = "Org5",
                    LastName = "ApprovedPending1",
                    Email = "testuser4@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = ApprovedUserId2,
                        Email = ApprovedUser2ProducerEmail
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = new ()
            {
                RelationshipType = RelationshipType.Employment
            }
        };
        setupContext.Enrolments.Add(approvedEnrolment2);
        var delegatedEnrolment2 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Pending,
            ServiceRoleId = ServiceRole.Packaging.DelegatedPerson.Id,
            ExternalId = DelegatedPersonId,
            Connection = new PersonOrganisationConnection
            {
                OrganisationId = 5,
                Person = new()
                {
                    FirstName = "Org5",
                    LastName = "DelegatedPending1",
                    Email = "testuser5@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = Guid.NewGuid(),
                        Email = "producer.user5@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = new ()
            {
                RelationshipType = RelationshipType.Employment
            }
        };
        setupContext.Enrolments.Add(delegatedEnrolment2);
        
        var basicEnrolment = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Enrolled,
            ServiceRoleId = ServiceRole.Packaging.BasicUser.Id,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = BasicUserExternalId1,
                OrganisationId = 5,
                Person = new()
                {
                    FirstName = "Org5 Basic",
                    LastName = "User1",
                    Email = "testuser1@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = Guid.NewGuid(),
                        Email = "producer.user5@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = new ()
            {
                RelationshipType = RelationshipType.Employment
            }
        };
        setupContext.Enrolments.Add(basicEnrolment);
        
        var basicEnrolment2 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.NotSet,
            ServiceRoleId = ServiceRole.Packaging.BasicUser.Id,
            Connection = new PersonOrganisationConnection
            {
                ExternalId = BasicUserExternalId2,
                OrganisationId = 5,
                Person = new()
                {
                    FirstName = "Org5 Basic",
                    LastName = "User1",
                    Email = "testuser2@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = Guid.NewGuid(),
                        Email = "producer.user5@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = new ()
            {
                RelationshipType = RelationshipType.Employment
            }
        };
        setupContext.Enrolments.Add(basicEnrolment2);
        
        var multiOrgUserBasicEnrolment1 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Enrolled,
            ServiceRoleId = ServiceRole.Packaging.BasicUser.Id,
            Connection = new PersonOrganisationConnection
            {
                OrganisationId = 5,
                PersonId = 1,
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = new ()
            {
                RelationshipType = RelationshipType.Employment
            }
        };
        setupContext.Enrolments.Add(multiOrgUserBasicEnrolment1);
        
        var deletedEnrolment1 = new Enrolment
        {
            IsDeleted = true,
            EnrolmentStatusId = EnrolmentStatus.Pending,
            ServiceRoleId = ServiceRole.Packaging.DelegatedPerson.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = DeletedEnrolmentOrgName,
                    OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
                    CompaniesHouseNumber = "55555555",
                    NationId = Nation.England
                },
                Person = new()
                {
                    FirstName = "Producer",
                    LastName = "User6",
                    Email = "testuser6@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = Guid.NewGuid(),
                        Email = "producer.user6@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = null
        };
        setupContext.Enrolments.Add(deletedEnrolment1);
        
        var basicUserEnrolment1 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Enrolled,
            ServiceRoleId = ServiceRole.Packaging.BasicUser.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = BasicUserOrgName,
                    OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
                    CompaniesHouseNumber = "66666666",
                    NationId = Nation.England
                },
                Person = new()
                {
                    FirstName = "Producer",
                    LastName = "User7",
                    Email = "testuser7@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = Guid.NewGuid(),
                        Email = "producer.user7@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = null
        };
        setupContext.Enrolments.Add(basicUserEnrolment1);
        
        var approvedSoleTraderUserEnrolment1 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Approved,
            ServiceRoleId = ServiceRole.Packaging.ApprovedPerson.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = "Producer Org 99",
                    BuildingName = "Skyscraper",
                    OrganisationTypeId = OrganisationType.NotSet,
                    NationId = Nation.England,
                    ExternalId = OrganisationIdSoleTrader,
                    ProducerTypeId = ProducerType.SoleTrader
                },
                Person = new()
                {
                    FirstName = "Producer (Sole Trader)",
                    LastName = "SoleTraderLastName",
                    Email = "SoleTraderLastName",
                    Telephone = "0123456788",
                    User = new()
                    {
                        UserId = Guid.NewGuid(),
                        Email = "producer.user8@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = null
        };
        
        setupContext.Enrolments.Add(approvedSoleTraderUserEnrolment1);

        var approvedPersonApprovedEnrolment1 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Approved,
            ServiceRoleId = ServiceRole.Packaging.ApprovedPerson.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = ApprovedPersonApprovedOrgName,
                    OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
                    CompaniesHouseNumber = "77777777",
                    NationId = Nation.Scotland
                },
                Person = new()
                {
                    FirstName = "Producer",
                    LastName = "User8",
                    Email = "testuser8@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = Guid.NewGuid(),
                        Email = "producer.user8@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = null
        };
        setupContext.Enrolments.Add(approvedPersonApprovedEnrolment1);
        
        var delegatedPersonApprovedEnrolment1 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Approved,
            ServiceRoleId = ServiceRole.Packaging.DelegatedPerson.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = DelegatedPersonApprovedOrgName,
                    OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
                    CompaniesHouseNumber = "77777777",
                    NationId = Nation.Scotland
                },
                Person = new()
                {
                    FirstName = "Producer",
                    LastName = "User8",
                    Email = "testuser8@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = Guid.NewGuid(),
                        Email = "producer.user8@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = null
        };
        setupContext.Enrolments.Add(delegatedPersonApprovedEnrolment1);
        
        var multiOrgUserEnrolment2 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Approved,
            ServiceRoleId = ServiceRole.Packaging.DelegatedPerson.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = DelegatedPersonApprovedOrgName,
                    OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
                    CompaniesHouseNumber = "88888888",
                    NationId = Nation.Wales
                },
                PersonId = 1,
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = null
        };
        setupContext.Enrolments.Add(multiOrgUserEnrolment2);
        
        var csEnrolment1 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Pending,
            ServiceRoleId = ServiceRole.Packaging.ApprovedPerson.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = "Compliance scheme 1",
                    OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
                    CompaniesHouseNumber = "11111111",
                    NationId = Nation.NotSet,
                    IsComplianceScheme = true,
                    ExternalId = CsOrganisation
                },
                Person = new()
                {
                    FirstName = "CS",
                    LastName = "User1",
                    Email = "testcsuser1@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = ApprovedUserId1,
                        Email = "producer.user1@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = null
        };
        setupContext.Enrolments.Add(csEnrolment1);

        var complianceScheme1 = new ComplianceScheme
        {
            CompaniesHouseNumber = "11111111",
            Name = "Compliance scheme - england",
            NationId = Nation.England
        };
        setupContext.ComplianceSchemes.Add(complianceScheme1);
        
        var csEnrolment2 = new Enrolment
        {
            EnrolmentStatusId = EnrolmentStatus.Pending,
            ServiceRoleId = ServiceRole.Packaging.ApprovedPerson.Id,
            Connection = new PersonOrganisationConnection
            {
                Organisation = new()
                {
                    Name = "Compliance scheme 1",
                    OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
                    CompaniesHouseNumber = "22222222",
                    NationId = Nation.NotSet,
                    IsComplianceScheme = true,
                    ExternalId = CsOrganisation2
                },
                Person = new()
                {
                    FirstName = "CS",
                    LastName = "User1",
                    Email = "testcsuser1@abc.com",
                    Telephone = "0123456789",
                    User = new()
                    {
                        UserId = ApprovedUserId1,
                        Email = "producer.user1@test.com"
                    }
                },
                OrganisationRoleId = OrganisationRole.Employer,
                PersonRoleId = PersonRole.Admin
            },
            DelegatedPersonEnrolment = null
        };
        setupContext.Enrolments.Add(csEnrolment2);

        var complianceScheme3 = new ComplianceScheme
        {
            CompaniesHouseNumber = "22222222",
            Name = "Compliance scheme - Scotland",
            NationId = Nation.Scotland
        };
        setupContext.ComplianceSchemes.Add(complianceScheme3);
        
        var complianceScheme2 = new ComplianceScheme
        {
            CompaniesHouseNumber = "11111111",
            Name = "Compliance scheme - NI",
            NationId = Nation.NorthernIreland
        };
        setupContext.ComplianceSchemes.Add(complianceScheme2);
    setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
    private static Organisation SetUpOrganisation(AccountsDbContext setupContext)
    {
        var organisation1 = new Organisation
        {
            OrganisationTypeId = OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = ProducerType.SoleTrader,
            CompaniesHouseNumber = "Test org 1 Company house number",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 1",
            SubBuildingName = "Sub building 1",
            BuildingName = "Building 1",
            BuildingNumber = "1",
            Street = "Street 1",
            Locality = "Locality 1",
            DependentLocality = "Dependent Locality 1",
            Town = "Town 1",
            County = "County 1",
            Postcode = "BT44 5QW",
            Country = "Country 1",
            NationId = Nation.England,
            ExternalId = Guid.NewGuid(),
            ReferenceNumber = $"RefNumber{Guid.NewGuid()}"
        };
        setupContext.Add(organisation1);

        return organisation1;
    }
    private Person SetUpPersonEnrolment(int organisationId, int serviceRoleId)
    {
        var user1 = new User()
        {
            UserId = Guid.NewGuid(),
            Email = $"user1+{Guid.NewGuid()}@test.com"
        };
        _dbContext.Add(user1);
        
        var person1 = new Person()
        {
            FirstName = $"User {organisationId}",
            LastName = $"Test {organisationId}",
            Email = $"user{organisationId}@test.com",
            Telephone = "0123456789",
            ExternalId = Guid.NewGuid(),
            UserId = user1.Id
        };
        _dbContext.Add(person1);
        
        var enrolment1 = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = person1.Id,
                OrganisationId = organisationId,
                OrganisationRoleId = OrganisationRole.Employer,
                ExternalId = Guid.NewGuid()
            },
            EnrolmentStatusId = EnrolmentStatus.Approved,
            ServiceRoleId = serviceRoleId
        };
        _dbContext.Add(enrolment1);
        return person1;
    }
    private Person SetUpDelegatedPersonWithoutFirstNameEnrolment(int organisationId)
    {
        var userAnother = new User()
        {
            UserId = new Guid("00000000-0000-4000-0000-000000000010"),
            Email = "userAnother@test.com",
            Id= 20
        };
        _dbContext.Add(userAnother);
        
        var personAnother = new Person()
        {
            FirstName = "",
            LastName = $"TestAnother {organisationId}",
            Email = $"userAnother{organisationId}@test.com",
            Telephone = "0123456789",
            ExternalId = new Guid("00000000-0050-0000-0000-000000000120"),
            UserId = userAnother.Id,
            Id= 50
        };
        _dbContext.Add(personAnother);
        
        var enrolmentAnother = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = personAnother.Id,
                OrganisationId = organisationId,
                OrganisationRoleId = OrganisationRole.Employer,
                ExternalId = Guid.NewGuid()
            },
            EnrolmentStatusId = EnrolmentStatus.Enrolled,
            ServiceRoleId = ServiceRole.Packaging.DelegatedPerson.Id
        };
        
        _dbContext.Add(enrolmentAnother);

        return personAnother;
    }
}