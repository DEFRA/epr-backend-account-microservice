using System;
using System.Net;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using EnrolmentStatus = BackendAccountService.Data.Entities.EnrolmentStatus;
using Service = BackendAccountService.Data.Entities.Service;
using ServiceRole = BackendAccountService.Data.Entities.ServiceRole;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class OrganisationServiceTests
{
    private AccountsDbContext _accountContext = null!;
    private OrganisationService _organisationService = null!;

    private const string DuplicateCompaniesHouseNumber = "12345678";
    private const string DeletedCompaniesHouseNumber = "DL123456";
    private const string SingleCompaniesHouseNumber = "87654321";
    private const string NonExistentCompaniesHouseNumber = "12121212";
    private const string NonExistentCompaniesName = "test12345678";
    private const string NonExistentCompaniesReferenceNumber = "test1345678";
    private readonly Guid userId = new Guid("00000000-0000-0000-0000-000000000001");
    private readonly Guid orgExternalId = Guid.NewGuid();
    private readonly Guid orgId = new Guid("00000000-0000-0000-0000-000000000010");
    private readonly Guid orgRelationshipExternalId = new Guid("00000000-0000-0000-0000-000000000007");
    private readonly Guid orgNoRelationshipExternalId = new Guid("00000000-0000-0000-0000-000000000009");
    private readonly Guid requestingPersonId = new Guid("00000000-0000-0000-0000-000000000020");
    private readonly string inviteToken1 = "Some invite  token";
    private readonly string org5Name = "Test org 3";
    private readonly Guid complianceSchemeExternalId = Guid.NewGuid();

    private readonly Guid orgWithNullFieldsExternalId = new Guid("00000000-0000-0000-0000-000000000016");
    private const string orgWithNullFieldsName = "Org With Null Fields";
    private const string orgWithNullFieldsCompaniesHouseNumber = "Rel10016";

    private const int parentOrgId = 10000;
    private const int orgWithNullFieldsId = 10026;

    private readonly Guid producerOrg1ExternalId = Guid.NewGuid();
    private readonly Guid producerOrg2ExternalId = Guid.NewGuid();
    private readonly Guid producerOrg3ExternalId = Guid.NewGuid();
    private readonly Guid producerOrg4ExternalId = Guid.NewGuid();
    private readonly Guid producerOrg5ExternalId = Guid.NewGuid();
    private readonly Guid producerOrg1Sub1ExternalId = Guid.NewGuid();
    private readonly Guid producerOrg1Sub2ExternalId = Guid.NewGuid();
    private readonly Guid producerOrg3Sub1ExternalId = Guid.NewGuid();
    private readonly Guid producerOrg3Sub2ExternalId = Guid.NewGuid();
    private readonly Guid complianceSchemeOrg1ExternalId = Guid.NewGuid();
    private readonly Guid complianceSchemeOrg2ExternalId = Guid.NewGuid();
    private readonly Guid complianceScheme1ExternalId = Guid.NewGuid();
    private readonly Guid complianceScheme2ExternalId = Guid.NewGuid();
    private readonly Guid complianceScheme3ExternalId = Guid.NewGuid();

    private readonly Guid user101Guid = Guid.NewGuid();
    private readonly Guid user102Guid = Guid.NewGuid();
    private readonly Guid user103Guid = Guid.Empty;
    private readonly Guid user104Guid = Guid.NewGuid();
    private readonly Guid user105Guid = Guid.NewGuid();
    private readonly Guid organisation101ExternalId = Guid.NewGuid();
    private readonly Guid organisation102ExternalId = Guid.NewGuid();
    private readonly int adminServiceRoleId = 101;
    private readonly int approvedPersonServiceRoleId = 102;
    private readonly int basicUserServiceRoleId = 103;
    private readonly int otherUserServiceRoleId = 99;
    private readonly int reExServiceId = 101;
    private readonly int otherServiceId = 102;

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("OrganisationServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _accountContext = new AccountsDbContext(contextOptions);
        _organisationService = new OrganisationService(
            _accountContext);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task WhenQueryForEmpyStringNumber_ThenThrowException()
    {
        //Act
        _ = await _organisationService.GetOrganisationsByCompaniesHouseNumberAsync("");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task WhenQueryForEmpyStringName_ThenThrowException()
    {
        //Act
        _ = await _organisationService.GetOrganisationsByCompaniesHouseNameAsync("");
    }

    [TestMethod]
    public async Task WhenQueryForExistingNumber_ThenReturnList()
    {
        //Act
        var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNumberAsync(DuplicateCompaniesHouseNumber);

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Should().HaveCount(2);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task GetByCompaniesHouseNumberAsync_WhenQueryForEmpyStringName_ThenThrowException()
    {
        //Act
        _ = await _organisationService.GetByCompaniesHouseNumberAsync("");
    }


    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task GetByCompaniesHouseNumberAsync_WhenQueryForNullString_ThenThrowException()
    {
        //Act
        _ = await _organisationService.GetByCompaniesHouseNumberAsync(null);
    }


    [TestMethod]
    public async Task WhenQueryForNoneExistingNumber_ThenReturnEmptyList()
    {
        //Act
        var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNumberAsync(NonExistentCompaniesHouseNumber);

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task WhenQueryForNoneExistingNumber_ThenReturnEmptyObject()
    {
        //Act
        var organisation = await _organisationService.GetByCompaniesHouseNumberAsync(NonExistentCompaniesHouseNumber);

        //Assert
        organisation.Should().BeNull();
    }


    [TestMethod]
    public async Task WhenQueryForNoneExistingNumber_ThenReturnParentOrgObject()
    {
        //Act
        var organisation = await _organisationService.GetByCompaniesHouseNumberAsync("Rel10000");

        //Assert
        organisation.Should().NotBeNull();
        organisation.ParentCompanyName.Should().BeSameAs("Org Relationship Parent Organisation");

    }

    [TestMethod]
    public async Task WhenQueryForNoneExistingNumber_ThenReturnChildOrgObject()
    {
        //Act
        var organisation = await _organisationService.GetByCompaniesHouseNumberAsync("Rel10012");

        //Assert
        organisation.Should().NotBeNull();
        organisation.Name.Should().BeSameAs("Org Second Test Relationship Organisation");

    }


    [TestMethod]
    public async Task WhenQueryExistingCompanyHouseNumber_ThenReturnList()
    {
        //Act
        var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNumberAsync("Rel10016");

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Should().HaveCount(2);
        organisationList.ToList()[0].OrganisationRelationship.Should().NotBeNull();
    }

    [TestMethod]
    public async Task WhenQueryForNoneExistingCompanyName_ThenReturnEmptyList()
    {
        //Act
        var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNameAsync(NonExistentCompaniesName);

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task WhenQueryForNoneExistingCompanyReferenceNumber_ThenReturnEmptyList()
    {
        //Act
        var organisationList = await _organisationService.GetOrganisationByReferenceNumber(NonExistentCompaniesReferenceNumber);

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task WhenQueryForExistingCompanyReferenceNumber_ThenReturnList()
    {
        //Act
        var organisationList = await _organisationService.GetOrganisationByReferenceNumber("1000015");

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Should().HaveCount(1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task WhenQueryForNullCompanyName_ThenReturnEmptyList()
    {
        //Act
        var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNameAsync(null);

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Should().BeEmpty();
    }

    [TestMethod]
    public async Task WhenQueryForDeletedNumber_ThenReturnEmptyList()
    {
        //Act
        var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNumberAsync(DeletedCompaniesHouseNumber);

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task WhenQueryForUsersListInOrganisation_ThenReturnList()
    {
        //Act
        var organisationList = await _organisationService.GetUserListForOrganisation(userId, orgId, 3);

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task When_Query_For_Users_List_In_Organisation_Then_List_Should_Not_Contain_Requested_User()
    {
        //Act
        var organisationList = await _organisationService.GetUserListForOrganisation(userId, orgId, 3);

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Any(x => x.PersonId == requestingPersonId).Should().BeFalse();
    }

    [TestMethod]
    public async Task When_Query_For_Users_List_In_Organisation_Then_List_Should_Not_Contain_Enrolement_Rejected_User()
    {
        //Act
        var organisationList = await _organisationService.GetUserListForOrganisation(userId, orgId, 3);

        //Assert
        organisationList.Should().NotBeNull();

        organisationList.ToList().Any(x => x.PersonId != null && x.Enrolments.Any(s => s.EnrolmentStatusId == Data.DbConstants.EnrolmentStatus.Rejected)).Should().BeFalse();
    }

    [TestMethod]
    public async Task When_Query_For_Users_List_In_Organisation_Then_List_Should_Not_Contain_Enrolement_NotSet_User()
    {
        //Act
        var organisationList = await _organisationService.GetUserListForOrganisation(userId, orgId, 3);

        //Assert
        organisationList.Should().NotBeNull();

        organisationList.ToList().Any(x => x.PersonId != null && x.Enrolments.Any(s => s.EnrolmentStatusId == Data.DbConstants.EnrolmentStatus.NotSet)).Should().BeFalse();
    }

    [TestMethod]
    public async Task WhenQueryForOrganisationByExternalId_ThenReturnList()
    {
        //Act
        var organisation = await _organisationService.GetOrganisationByExternalId(orgExternalId);

        //Assert
        organisation.Should().NotBeNull();
        organisation.Name.Equals("RemovedUser Company").Should().BeTrue();
        organisation.OrganisationNumber.Equals("1000520").Should().BeTrue();

    }

    [TestMethod]
    public async Task WhenQueryForOrganisationResponseByExternalId_ThenReturnList()
    {
        //Act
        var organisation = await _organisationService.GetOrganisationResponseByExternalId(orgExternalId);

        //Assert
        organisation.Should().NotBeNull();
        organisation.Name.Equals("RemovedUser Company").Should().BeTrue();
        organisation.ReferenceNumber.Equals("1000520").Should().BeTrue();

    }

    [TestMethod]
    public async Task WhenAddOrganisationAndOrganisationRelationshipsAsync_ThenReturnCorrectValues()
    {
        var org = new Models.OrganisationModel
        {
            OrganisationType = Models.OrganisationType.NotSet,
            ProducerType = Models.ProducerType.NotSet,
            CompaniesHouseNumber = "123456",
            Name = "Test company",
            Address = new Models.AddressModel
            {
                SubBuildingName = "Test SubBuildingName",
                BuildingName = "Test BuildingName",
                BuildingNumber = "123",
                Street = "Collage Ln",
                Locality = "Herts",
                DependentLocality = "Herts",
                Town = "StAlbs",
                County = "Herts",
                Country = "England",
                Postcode = "AL10 9AB"
            },
            ValidatedWithCompaniesHouse = true,
            IsComplianceScheme = true,
            Nation = Models.Nation.England,
        };

        var orgRelationships = new Models.OrganisationRelationshipModel
        {
            FirstOrganisationId = 789,
            SecondOrganisationId = 901,
            OrganisationRegistrationTypeId = 1,
            RelationFromDate = DateTime.Now,
            LastUpdatedById = 6,
            LastUpdatedByOrganisationId = 789,
            JoinerDate = DateTime.Now
        };

        var results = await _organisationService.AddOrganisationAndOrganisationRelationshipsAsync(org, orgRelationships, It.IsAny<Guid>());
        results.Should().NotBeNull();
        results.CompaniesHouseNumber.Should().Be("123456");
    }

    [TestMethod]
    public async Task AddOrganisationAndOrganisationRelationshipsAsync_ShouldAddFranchiseeOrganisation_WhenSubsidiaryOrganisationIdProvided()
    {
        var org = new OrganisationModel
        {
            OrganisationType = Models.OrganisationType.NotSet,
            ProducerType = Models.ProducerType.NotSet,
            CompaniesHouseNumber = "123456",
            Name = "Test company",
            Address = new Models.AddressModel
            {
                SubBuildingName = "Test SubBuildingName",
                BuildingName = "Test BuildingName",
                BuildingNumber = "123",
                Street = "Collage Ln",
                Locality = "Herts",
                DependentLocality = "Herts",
                Town = "StAlbs",
                County = "Herts",
                Country = "England",
                Postcode = "AL10 9AB"
            },
            ValidatedWithCompaniesHouse = true,
            IsComplianceScheme = true,
            Nation = Models.Nation.England,
            SubsidiaryOrganisationId = "97654",
            Franchisee_Licensee_Tenant = "Y"
        };

        var orgRelationships = new Models.OrganisationRelationshipModel
        {
            FirstOrganisationId = 789,
            SecondOrganisationId = 901,
            OrganisationRegistrationTypeId = 1,
            RelationFromDate = DateTime.Now,
            LastUpdatedById = 6,
            LastUpdatedByOrganisationId = 789,
            JoinerDate = DateTime.Now
        };

        var results = await _organisationService.AddOrganisationAndOrganisationRelationshipsAsync(org, orgRelationships, It.IsAny<Guid>());
        results.Should().NotBeNull();
        results.SubsidiaryOrganisations.Should().NotBeNull();
        Assert.IsNotNull(results.SubsidiaryOrganisations.FirstOrDefault().Id);
    }

    [TestMethod]
    public async Task AddOrganisationAndOrganisationRelationshipsAsync_ShouldNotFranchiseeOrganisation_WhenSubsidiaryOrganisationIdProvided()
    {
        var org = new OrganisationModel
        {
            OrganisationType = Models.OrganisationType.NonCompaniesHouseCompany,
            ProducerType = Models.ProducerType.NotSet,
            CompaniesHouseNumber = "87654211",
            Name = "organisation61",
            Address = new Models.AddressModel
            {
                SubBuildingName = "Test SubBuildingName",
                BuildingName = "Test BuildingName",
                BuildingNumber = "123",
                Street = "Collage Ln",
                Locality = "Herts",
                DependentLocality = "Herts",
                Town = "StAlbs",
                County = "Herts",
                Country = "England",
                Postcode = "AL10 9AB"
            },
            ValidatedWithCompaniesHouse = true,
            IsComplianceScheme = true,
            Nation = Models.Nation.England,
            SubsidiaryOrganisationId = "97654",
            Franchisee_Licensee_Tenant = "Y"
        };

        var orgRelationships = new Models.OrganisationRelationshipModel
        {
            FirstOrganisationId = 7189,
            SecondOrganisationId = 9011,
            OrganisationRegistrationTypeId = 1,
            RelationFromDate = DateTime.Now,
            LastUpdatedById = 6,
            LastUpdatedByOrganisationId = 7189
        };

        var results = await _organisationService.AddOrganisationAndOrganisationRelationshipsAsync(org, orgRelationships, It.IsAny<Guid>());
        results.Should().NotBeNull();
    }

    [TestMethod]
    public async Task AddOrganisationAndOrganisationRelationshipsAsync_ShouldAddSubsidiaryOrganisation_WhenSubsidiaryOrganisationIdProvided()
    {
        var org = new Models.OrganisationModel
        {
            OrganisationType = Models.OrganisationType.NotSet,
            ProducerType = Models.ProducerType.NotSet,
            CompaniesHouseNumber = "123456",
            Name = "Test company",
            Address = new Models.AddressModel
            {
                SubBuildingName = "Test SubBuildingName",
                BuildingName = "Test BuildingName",
                BuildingNumber = "123",
                Street = "Collage Ln",
                Locality = "Herts",
                DependentLocality = "Herts",
                Town = "StAlbs",
                County = "Herts",
                Country = "England",
                Postcode = "AL10 9AB"
            },
            ValidatedWithCompaniesHouse = true,
            IsComplianceScheme = true,
            Nation = Models.Nation.England,
            SubsidiaryOrganisationId = "97654"
        };

        var orgRelationships = new Models.OrganisationRelationshipModel
        {
            FirstOrganisationId = 789,
            SecondOrganisationId = 901,
            OrganisationRegistrationTypeId = 1,
            RelationFromDate = DateTime.Now,
            LastUpdatedById = 6,
            LastUpdatedByOrganisationId = 789
        };

        var results = await _organisationService.AddOrganisationAndOrganisationRelationshipsAsync(org, orgRelationships, It.IsAny<Guid>());
        results.Should().NotBeNull();
        results.SubsidiaryOrganisations.Should().NotBeNull();
        Assert.IsNotNull(results.SubsidiaryOrganisations.FirstOrDefault().Id);
    }

    [TestMethod]
    public async Task AddOrganisationRelationships_For_ExistingOrganisation_WhenSubsidiaryOrganisationIdProvided()
    {
        var org = new Models.OrganisationModel
        {
            OrganisationType = Models.OrganisationType.CompaniesHouseCompany,
            ProducerType = Models.ProducerType.NotSet,
            CompaniesHouseNumber = "87654211",
            Name = "organisation61",
            Address = new Models.AddressModel
            {
                SubBuildingName = "Test SubBuildingName",
                BuildingName = "Test BuildingName",
                BuildingNumber = "123",
                Street = "Collage Ln",
                Locality = "Herts",
                DependentLocality = "Herts",
                Town = "StAlbs",
                County = "Herts",
                Country = "England",
                Postcode = "AL10 9AB"
            },
            ValidatedWithCompaniesHouse = true,
            IsComplianceScheme = true,
            Nation = Models.Nation.England,
            SubsidiaryOrganisationId = "8765421197654"
        };

        var orgRelationships = new Models.OrganisationRelationshipModel
        {
            FirstOrganisationId = 789,
            SecondOrganisationId = 901,
            OrganisationRegistrationTypeId = 1,
            RelationFromDate = DateTime.Now,
            LastUpdatedById = 6,
            LastUpdatedByOrganisationId = 789
        };

        var results = await _organisationService.AddOrganisationAndOrganisationRelationshipsAsync(org, orgRelationships, It.IsAny<Guid>());
        results.Should().NotBeNull();
        results.SubsidiaryOrganisations.Should().BeNull();
    }

    [TestMethod]
    public async Task GetOrganisationNameByInviteToken_works()
    {
        // Arrange

        //Act
        var organisation = await _organisationService.GetOrganisationNameByInviteTokenAsync(inviteToken1);

        //Assert
        organisation.Should().NotBeNull();
        organisation.OrganisationName.Should().Be(org5Name);

    }

    [TestMethod]
    public async Task GetOrganisationName_by_InvalidInviteToken_Returns_Null()
    {
        // Arrange

        //Act

        //Assert
        Assert.ThrowsException<AggregateException>(() =>
            _organisationService.GetOrganisationNameByInviteTokenAsync("not" + inviteToken1).Result);
    }

    [TestMethod]
    public async Task GetPagedOrganisationRelationships_WithoutSearchParameter_ReturnsPagedAllRows()
    {
        // Arrange
        var page = 1;
        var showPerPage = 2;

        // Act
        var result = await _organisationService.GetPagedOrganisationRelationships(page, showPerPage);

        // Assert
        Assert.AreEqual(page, result.CurrentPage);
        Assert.AreEqual(showPerPage, result.PageSize);
        Assert.AreEqual(10, result.TotalItems);
        Assert.AreEqual(showPerPage, result.Items.Count);
        Assert.AreEqual("Child Subsidiary Organisation 1 For Producer Organisation 1", result.Items[0].OrganisationName);
        Assert.AreEqual("Child Subsidiary Organisation 1 For Producer Organisation 3", result.Items[1].OrganisationName);
        Assert.AreEqual(21, result.SearchTerms.Count);
    }

    [TestMethod]
    public async Task GetPagedOrganisationRelationships_WithSearchParameter_ReturnsPagedMatchingRows()
    {
        // Arrange
        var page = 1;
        var showPerPage = 2;
        var search = "Organisation 1";

        // Act
        var result = await _organisationService.GetPagedOrganisationRelationships(page, showPerPage, search);

        // Assert
        Assert.AreEqual(page, result.CurrentPage);
        Assert.AreEqual(showPerPage, result.PageSize);
        Assert.AreEqual(3, result.TotalItems);
        Assert.AreEqual(showPerPage, result.Items.Count);
        Assert.AreEqual("Child Subsidiary Organisation 1 For Producer Organisation 1", result.Items[0].OrganisationName);
        Assert.AreEqual("Child Subsidiary Organisation 1 For Producer Organisation 3", result.Items[1].OrganisationName);
        Assert.AreEqual(21, result.SearchTerms.Count);
    }

    [TestMethod]
    public async Task GetUnpagedOrganisationRelationships_NoRelationships_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _organisationService.GetUnpagedOrganisationRelationships();

        // Assert
        Assert.AreEqual(10, result.Count);
        Assert.AreEqual("Child Subsidiary Organisation 1 For Producer Organisation 1", result[0].OrganisationName);
        Assert.AreEqual("Child Subsidiary Organisation 1 For Producer Organisation 3", result[1].OrganisationName);
        Assert.AreEqual("Child Subsidiary Organisation 2 For Producer Organisation 1", result[2].OrganisationName);
        Assert.AreEqual("Child Subsidiary Organisation 2 For Producer Organisation 3", result[3].OrganisationName);
        Assert.AreEqual("Org 4th Test Relationship Organisation", result[4].OrganisationName);
        Assert.AreEqual("Org Relationship Subsidiary Organisation", result[5].OrganisationName);
        Assert.AreEqual("Org Second Test Relationship Organisation", result[6].OrganisationName);
        Assert.AreEqual("Org Sixth Test Relationship Organisation", result[7].OrganisationName);
        Assert.AreEqual("Org Third Test Relationship Organisation", result[8].OrganisationName);
        Assert.AreEqual("Org With Null Fields", result[9].OrganisationName);
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationId_NoRelationships_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _organisationService.GetOrganisationRelationshipsByOrganisationId(orgNoRelationshipExternalId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationId_WithRelationships_ReturnsDetailModel()
    {
        // Arrange

        // Act
        var result = await _organisationService.GetOrganisationRelationshipsByOrganisationId(orgRelationshipExternalId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Org Relationship Parent Organisation", result.Organisation.Name);
        Assert.AreEqual("Org Relationship Subsidiary Organisation", result.Relationships[0].OrganisationName);
        Assert.AreEqual(6, result.Relationships.Count);
    }

    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationId_WithRelationships_ReturnsDetailModel_With_ReportingTypeAndJoinerDate()
    {
        // Arrange

        // Act
        var result = await _organisationService.GetOrganisationRelationshipsByOrganisationId(orgRelationshipExternalId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Org Relationship Parent Organisation", result.Organisation.Name);
        Assert.AreEqual("Org Relationship Subsidiary Organisation", result.Relationships[0].OrganisationName);
        Assert.AreEqual(6, result.Relationships.Count);
        Assert.IsNotNull(result.Relationships[0].JoinerDate, "Joiner Date is null");
    }


    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationId_WhenJoinerDateAndReportingTypeAreNull_Returns_NullValues()
    {
        // Act
        var result = await _organisationService.GetOrganisationRelationshipsByOrganisationId(orgRelationshipExternalId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Org Relationship Parent Organisation", result.Organisation.Name);

        var relationshipWithNulls = result.Relationships.FirstOrDefault(r => r.OrganisationName == orgWithNullFieldsName);

        Assert.IsNotNull(relationshipWithNulls, "Expected relationship not found.");
        Assert.IsNull(relationshipWithNulls.JoinerDate, "JoinerDate should be null.");
    }


    [TestMethod]
    public async Task GetOrganisationRelationshipsByOrganisationId_WithTerminatedRelationship_ReturnsSingleDetailModel()
    {
        // Arrange
        var firstRelationship = await _accountContext.OrganisationRelationships
            .SingleAsync(rel => rel.SecondOrganisationId == 10001);

        firstRelationship.RelationToDate = DateTime.UtcNow;
        firstRelationship.RelationExpiryReason = "Test";
        await _accountContext.SaveChangesAsync(Guid.Empty, Guid.Empty);

        // Act
        var result = await _organisationService.GetOrganisationRelationshipsByOrganisationId(orgRelationshipExternalId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Org Relationship Parent Organisation", result.Organisation.Name);
        Assert.AreEqual("Org Second Test Relationship Organisation", result.Relationships[0].OrganisationName);
        Assert.AreEqual(5, result.Relationships.Count);
    }

    [TestMethod]
    public async Task GetOrganisationSubsidiaries_ReturnsOk_WhenDataExists()
    {
        //Arrange

        // Act
        var result = await _organisationService.ExportOrganisationSubsidiaries(orgRelationshipExternalId);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThanOrEqualTo(2);
        result[0].CompaniesHouseNumber.Should().Be("Rel10000");
        result[1].CompaniesHouseNumber.Should().Be("Rel10001");
        result[1].SubsidiaryId.Should().Be("1000008");
        result[1].JoinerDate.Should().NotBeNull();
    }

    [TestMethod]
    public async Task GetOrganisationSubsidiaries_NoRelationships_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _organisationService.ExportOrganisationSubsidiaries(orgNoRelationshipExternalId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task AddOrganisationRelationshipsAsync_WhenNoExistingRelationships_CreatesAndReturnsNewRelationship()
    {
        // Arrange
        const int parentOrganisation7Id = 10000;
        const int childOrganisation9Id = 10011;
        var request = new OrganisationRelationshipModel
        {
            FirstOrganisationId = parentOrganisation7Id,
            SecondOrganisationId = childOrganisation9Id,
            OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
            OrganisationRegistrationTypeId = null,
            CreatedOn = DateTime.UtcNow,
            RelationFromDate = DateTime.UtcNow,
            LastUpdatedById = 123,
            LastUpdatedOn = DateTime.UtcNow,
        };

        // Act
        var result = await _organisationService.AddOrganisationRelationshipsAsync(request, orgRelationshipExternalId, userId);

        // Assert
        Assert.IsNotNull(result);
        var newRelationship = await _accountContext.OrganisationRelationships.SingleOrDefaultAsync(relationship =>
            relationship.FirstOrganisationId == parentOrganisation7Id &&
            relationship.SecondOrganisationId == childOrganisation9Id);

        Assert.IsNotNull(newRelationship);
    }

    [TestMethod]
    public async Task AddOrganisationRelationshipsAsync_WhenNoExistingRelationships_With_ReportingType_SE_CreatesNewRelationship()
    {
        // Arrange
        const int parentOrganisation7Id = 10000;
        const int childOrganisation9Id = 10011;
        var request = new OrganisationRelationshipModel
        {
            FirstOrganisationId = parentOrganisation7Id,
            SecondOrganisationId = childOrganisation9Id,
            OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
            OrganisationRegistrationTypeId = null,
            CreatedOn = DateTime.UtcNow,
            RelationFromDate = DateTime.UtcNow,
            LastUpdatedById = 123,
            LastUpdatedOn = DateTime.UtcNow,
            JoinerDate = DateTime.UtcNow
        };

        // Act
        var result = await _organisationService.AddOrganisationRelationshipsAsync(request, orgRelationshipExternalId, userId);

        // Assert
        Assert.IsNotNull(result);
        var newRelationship = await _accountContext.OrganisationRelationships.SingleOrDefaultAsync(relationship =>
            relationship.FirstOrganisationId == parentOrganisation7Id &&
            relationship.SecondOrganisationId == childOrganisation9Id);

        Assert.IsNotNull(newRelationship);
    }

    [TestMethod]
    public async Task AddOrganisationRelationshipsAsync_WhenExistingRelationship_OldRelationshipIsTerminated()
    {
        // Arrange
        const int oldParentOrganisation7Id = 10000;
        const int newParentOrganisation8Id = 10001;
        const int childOrganisation12Id = 10014;
        var request = new OrganisationRelationshipModel
        {
            FirstOrganisationId = newParentOrganisation8Id,
            SecondOrganisationId = childOrganisation12Id,
            OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
            OrganisationRegistrationTypeId = null,
            CreatedOn = DateTime.UtcNow,
            RelationFromDate = DateTime.UtcNow,
            LastUpdatedById = 123,
            LastUpdatedOn = DateTime.UtcNow,
        };

        // Act
        var result = await _organisationService.AddOrganisationRelationshipsAsync(request, orgRelationshipExternalId, userId);

        // Assert
        Assert.IsNotNull(result);
        var oldRelationship = await _accountContext.OrganisationRelationships.SingleOrDefaultAsync(relationship =>
            relationship.FirstOrganisationId == oldParentOrganisation7Id &&
            relationship.SecondOrganisationId == childOrganisation12Id);

        Assert.IsNotNull(oldRelationship);
        Assert.IsNotNull(oldRelationship.RelationToDate);

        var newRelationship = await _accountContext.OrganisationRelationships.SingleOrDefaultAsync(relationship =>
            relationship.FirstOrganisationId == newParentOrganisation8Id &&
            relationship.SecondOrganisationId == childOrganisation12Id);

        Assert.IsNotNull(newRelationship);
        Assert.IsNull(newRelationship.RelationToDate);
    }

    [TestMethod]
    public async Task NotAddOrganisationRelationshipsAsync_WhenDuplicateRelaltionshipAttempted()
    {
        // Arrange
        const int parentOrganisation7Id = 10000;
        const int childOrganisation12Id = 10015;
        var request = new OrganisationRelationshipModel
        {
            FirstOrganisationId = parentOrganisation7Id,
            SecondOrganisationId = childOrganisation12Id,
            OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
            OrganisationRegistrationTypeId = null,
            CreatedOn = DateTime.UtcNow,
            RelationFromDate = DateTime.UtcNow,
            LastUpdatedById = 123,
            LastUpdatedOn = DateTime.UtcNow,
        };

        // Act
        var result = await _organisationService.AddOrganisationRelationshipsAsync(request, orgRelationshipExternalId, userId);

        // Assert
        Assert.IsNotNull(result);
        var checkRelationship = await _accountContext.OrganisationRelationships.SingleOrDefaultAsync(relationship =>
            relationship.FirstOrganisationId == parentOrganisation7Id &&
            relationship.SecondOrganisationId == childOrganisation12Id);

        Assert.IsNotNull(checkRelationship);
        Assert.IsNull(checkRelationship.RelationToDate);

        // Act
        var resultDuplicate = await _organisationService.AddOrganisationRelationshipsAsync(request, orgRelationshipExternalId, userId);

        // Assert
        Assert.IsNull(resultDuplicate);

        var checkRelationshipAgain = await _accountContext.OrganisationRelationships.SingleOrDefaultAsync(relationship =>
            relationship.FirstOrganisationId == parentOrganisation7Id &&
            relationship.SecondOrganisationId == childOrganisation12Id);

        Assert.IsNotNull(checkRelationshipAgain);
        Assert.IsNull(checkRelationshipAgain.RelationToDate);
    }

    [TestMethod]
    public async Task NotAddOrganisationRelationshipsAsync_WhenSameSubId_Attempted()
    {
        // Arrange
        const int parentOrganisation7Id = 10000;
        const int childOrganisation12Id = 10000;
        var request = new OrganisationRelationshipModel
        {
            FirstOrganisationId = parentOrganisation7Id,
            SecondOrganisationId = childOrganisation12Id,
            OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
            OrganisationRegistrationTypeId = null,
            CreatedOn = DateTime.UtcNow,
            RelationFromDate = DateTime.UtcNow,
            LastUpdatedById = 123,
            LastUpdatedOn = DateTime.UtcNow,
        };

        // Act
        var result = await _organisationService.AddOrganisationRelationshipsAsync(request, orgRelationshipExternalId, userId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task TerminateOrganisationRelationshipsAsync_WhenChildRelationshipExists_RelationshipTerminated()
    {
        // Arrange
        var request = new TerminateSubsidiaryModel
        {
            ParentOrganisationId = 10000,
            ParentExternalId = Guid.Parse("00000000-0000-0000-0000-000000000007"),
            ChildOrganisationId = 10013,
            UserExternalId = userId,
            UserId = 1234
        };

        // Act
        var result = await _organisationService.TerminateOrganisationRelationshipsAsync(request);

        // Assert
        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeTrue();

        var updatedRelationship = await _accountContext.OrganisationRelationships.SingleAsync(relationship =>
            relationship.FirstOrganisationId == 10000 && relationship.SecondOrganisationId == 10013);

        updatedRelationship.Should().NotBeNull();
        updatedRelationship.RelationToDate.Should().NotBeNull();
        updatedRelationship.LastUpdatedById.Should().Be(request.UserId);
    }

    [TestMethod]
    public async Task TerminateOrganisationRelationshipsAsync_WhenChildRelationshipDoesNotExist_ReturnsFailedResult()
    {
        // Arrange
        var request = new TerminateSubsidiaryModel
        {
            ParentOrganisationId = 99999,
            ChildOrganisationId = 88888
        };

        // Act
        var result = await _organisationService.TerminateOrganisationRelationshipsAsync(request);

        // Assert
        result.Should().BeOfType(typeof(Result));
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);

        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();
        var organisation61 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            CompaniesHouseNumber = "87654211",
            ReferenceNumber = "R87654211",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "organisation61",
            SubBuildingName = "Sub building 7",
            BuildingName = "Building 7",
            BuildingNumber = "4",
            Street = "Street 7",
            Locality = "Locality 7",
            DependentLocality = "Dependent Locality 7",
            Town = "Town 7",
            County = "County 7",
            Postcode = "BT44 7QW",
            Country = "Country 7",
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000010"),
        };
        setupContext.Organisations.Add(organisation61);

        var organisation51 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "87654221",
            ReferenceNumber = "R87654221",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "organisation51",
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
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.Organisations.Add(organisation51);

        var organisation1 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = DuplicateCompaniesHouseNumber,
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
            NationId = Data.DbConstants.Nation.England
        };
        setupContext.Organisations.Add(organisation1);

        var organisation2 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = DuplicateCompaniesHouseNumber,
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 2",
            SubBuildingName = "Sub building 2",
            BuildingName = "Building 2",
            BuildingNumber = "2",
            Street = "Street 2",
            Locality = "Locality 2",
            DependentLocality = "Dependent Locality 2",
            Town = "Town 2",
            County = "County 2",
            Postcode = "BT44 5QW",
            Country = "Country 2",
            NationId = Data.DbConstants.Nation.England
        };
        setupContext.Organisations.Add(organisation2);

        var organisation3 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = SingleCompaniesHouseNumber,
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 3",
            SubBuildingName = "Sub building 3",
            BuildingName = "Building 3",
            BuildingNumber = "3",
            Street = "Street 3",
            Locality = "Locality 3",
            DependentLocality = "Dependent Locality 3",
            Town = "Town 3",
            County = "County 3",
            Postcode = "BT44 5QW",
            Country = "Country 3",
            NationId = Data.DbConstants.Nation.England
        };
        setupContext.Organisations.Add(organisation3);

        var organisation4 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = DeletedCompaniesHouseNumber,
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Test org 4",
            SubBuildingName = "Sub building 4",
            BuildingName = "Building 4",
            BuildingNumber = "4",
            Street = "Street 4",
            Locality = "Locality 4",
            DependentLocality = "Dependent Locality 4",
            Town = "Town 4",
            County = "County 4",
            Postcode = "BT44 5QW",
            Country = "Country 4",
            NationId = Data.DbConstants.Nation.England,
            IsDeleted = true
        };
        setupContext.Organisations.Add(organisation4);

        var organisation5 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            CompaniesHouseNumber = SingleCompaniesHouseNumber,
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = org5Name,
            SubBuildingName = "Sub building 4",
            BuildingName = "Building 4",
            BuildingNumber = "4",
            Street = "Street 4",
            Locality = "Locality 4",
            DependentLocality = "Dependent Locality 4",
            Town = "Town 4",
            County = "County 4",
            Postcode = "BT44 5QW",
            Country = "Country 4",
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000010"),
        };
        setupContext.Organisations.Add(organisation5);

        var organisation6 = new Organisation
        {
            Name = "RemovedUser Company",
            ReferenceNumber = "1000520",
            ExternalId = orgExternalId
        };
        setupContext.Organisations.Add(organisation6);

        var user = new User()
        {
            UserId = new Guid("00000000-0000-0000-0000-000000000001"),
            Email = "user1@test.com"
        };
        setupContext.Users.Add(user);

        var user2 = new User()
        {
            UserId = new Guid("00000000-0000-0000-0000-000000000002"),
            Email = "user2@test.com",
            InviteToken = inviteToken1
        };
        setupContext.Users.Add(user2);

        var user3 = new User()
        {
            UserId = new Guid("00000000-0000-0000-0000-000000000003"),
            Email = "user3@test.com"
        };
        setupContext.Users.Add(user3);

        var person1 = new Person()
        {
            FirstName = "User 1",
            LastName = "Test",
            Email = "user1@test.com",
            Telephone = "0123456789",
            ExternalId = new Guid("00000000-0000-0000-0000-000000000020"),
            UserId = 1
        };
        setupContext.Persons.Add(person1);

        var person2 = new Person()
        {
            FirstName = "User 2",
            LastName = "Test",
            Email = "user2@test.com",
            Telephone = "0123456789",
            ExternalId = new Guid("00000000-0000-0000-0000-000000000021"),
            UserId = 2
        };
        setupContext.Persons.Add(person2);

        var person3 = new Person()
        {
            FirstName = "User 3",
            LastName = "Test",
            Email = "user3@test.com",
            Telephone = "0123456789",
            ExternalId = new Guid("00000000-0000-0000-0000-000000000022"),
            UserId = 3
        };
        setupContext.Persons.Add(person3);

        var enrolment1 = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = 1,
                OrganisationId = 5,
                PersonRoleId = Data.DbConstants.PersonRole.Admin,
                OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer
            },
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Approved,
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id
        };
        setupContext.Enrolments.Add(enrolment1);

        var enrolment2 = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = 2,
                OrganisationId = 5,
                PersonRoleId = Data.DbConstants.PersonRole.Admin,
                OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer
            },
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Approved,
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id
        };
        setupContext.Enrolments.Add(enrolment2);

        var enrolment3 = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = 2,
                OrganisationId = 5,
                PersonRoleId = Data.DbConstants.PersonRole.Admin,
                OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer
            },
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Enrolled,
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id
        };
        setupContext.Enrolments.Add(enrolment3);

        var enrolment4 = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = 2,
                OrganisationId = 5,
                PersonRoleId = Data.DbConstants.PersonRole.Admin,
                OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer
            },
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Invited,
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id
        };
        setupContext.Enrolments.Add(enrolment4);

        var enrolment5 = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = 2,
                OrganisationId = 5,
                PersonRoleId = Data.DbConstants.PersonRole.Admin,
                OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer
            },
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Pending,
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id
        };
        setupContext.Enrolments.Add(enrolment5);

        var enrolment6 = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = 2,
                OrganisationId = 5,
                PersonRoleId = Data.DbConstants.PersonRole.Admin,
                OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer
            },
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.NotSet,
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id
        };
        setupContext.Enrolments.Add(enrolment6);

        var enrolment7 = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = 2,
                OrganisationId = 5,
                PersonRoleId = Data.DbConstants.PersonRole.Admin,
                OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer
            },
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Rejected,
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id
        };
        setupContext.Enrolments.Add(enrolment7);

        var enrolment8 = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = 3,
                OrganisationId = 5,
                PersonRoleId = Data.DbConstants.PersonRole.Admin,
                OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer
            },
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Approved,
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Id
        };
        setupContext.Enrolments.Add(enrolment8);

        var organisation7 = new Organisation
        {
            Id = 10000,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "Rel10000",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Org Relationship Parent Organisation",
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
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000007"),
            ReferenceNumber = "1000007",
        };
        setupContext.Organisations.Add(organisation7);

        var organisation8 = new Organisation
        {
            Id = 10001,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "Rel10001",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Org Relationship Subsidiary Organisation",
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
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000008"),
            ReferenceNumber = "1000008",
        };
        setupContext.Organisations.Add(organisation8);

        var organisation11 = new Organisation
        {
            Id = 10011,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "Rel10011",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Org No Relationship Organisation",
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
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000009"),
            ReferenceNumber = "1000009",
        };
        setupContext.Organisations.Add(organisation11);

        var organisation12 = new Organisation
        {
            Id = 10012,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "Rel10012",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Org Second Test Relationship Organisation",
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
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000010"),
            ReferenceNumber = "1000010",
        };
        setupContext.Organisations.Add(organisation12);

        var organisation13 = new Organisation
        {
            Id = 10013,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "Rel10013",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Org Third Test Relationship Organisation",
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
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000011"),
            ReferenceNumber = "1000011",
        };
        setupContext.Organisations.Add(organisation13);

        var organisation14 = new Organisation
        {
            Id = 10014,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "Rel10014",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Org 4th Test Relationship Organisation",
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
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000012"),
            ReferenceNumber = "1000012",
        };
        setupContext.Organisations.Add(organisation14);

        var organisation15 = new Organisation
        {
            Id = 10015,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "Rel10015",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Org 5th Test Relationship Organisation",
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
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000015"),
            ReferenceNumber = "1000015",
        };
        setupContext.Organisations.Add(organisation15);

        var organisation16 = new Organisation
        {
            Id = 10016,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = "Rel10016",
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = "Org Sixth Test Relationship Organisation",
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
            NationId = Data.DbConstants.Nation.England,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000015"),
            ReferenceNumber = "1000016",
        };
        setupContext.Organisations.Add(organisation16);

        var relationship1 = new OrganisationRelationship
        {
            FirstOrganisationId = 10000,
            SecondOrganisationId = 10001,
            OrganisationRelationshipTypeId = 10007,
            JoinerDate = DateTime.Now
        };
        setupContext.OrganisationRelationships.Add(relationship1);

        var relationship2 = new OrganisationRelationship
        {
            FirstOrganisationId = 10000,
            SecondOrganisationId = 10012,
            OrganisationRelationshipTypeId = 10007
        };
        setupContext.OrganisationRelationships.Add(relationship2);

        var relationship3 = new OrganisationRelationship
        {
            FirstOrganisationId = 10000,
            SecondOrganisationId = 10013,
            OrganisationRelationshipTypeId = 10007
        };
        setupContext.OrganisationRelationships.Add(relationship3);

        var relationship4 = new OrganisationRelationship
        {
            FirstOrganisationId = 10000,
            SecondOrganisationId = 10014,
            OrganisationRelationshipTypeId = 10007
        };
        setupContext.OrganisationRelationships.Add(relationship4);

        var relationship16 = new OrganisationRelationship
        {
            FirstOrganisationId = 10000,
            SecondOrganisationId = 10016,
            OrganisationRelationshipTypeId = 10007,
            JoinerDate = DateTime.Now
        };
        setupContext.OrganisationRelationships.Add(relationship16);

        var relationshipType1 = new Data.Entities.OrganisationRelationshipType
        {
            Id = 10007,
            Name = "Parent"
        };
        setupContext.OrganisationRelationshipTypes.Add(relationshipType1);

        var subsidiaryOrganisation1 = new SubsidiaryOrganisation
        {
            Id = 100,
            OrganisationId = organisation11.Id,
            SubsidiaryId = "Sub-Test-3",
            CreatedOn = DateTime.UtcNow,
            LastUpdatedOn = DateTime.UtcNow
        };
        setupContext.SubsidiaryOrganisations.Add(subsidiaryOrganisation1);

        var complianceSchemeOrg = new ComplianceScheme
        {
            ExternalId = complianceSchemeExternalId,
            Id = 684531,
            NationId = 2,
            CompaniesHouseNumber = "CompScheme1CHNumber",
            Name = "CompScheme1Name"
        };
        setupContext.ComplianceSchemes.Add(complianceSchemeOrg);


        var organisationWithNullFields = new Organisation
        {
            Id = orgWithNullFieldsId,
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.NotSet,
            CompaniesHouseNumber = orgWithNullFieldsCompaniesHouseNumber,
            IsComplianceScheme = false,
            ValidatedWithCompaniesHouse = true,
            Name = orgWithNullFieldsName,
            SubBuildingName = "Sub building Null",
            BuildingName = "Building Null",
            BuildingNumber = "99",
            Street = "Street Null",
            Locality = "Locality Null",
            DependentLocality = "Dependent Locality Null",
            Town = "Town Null",
            County = "County Null",
            Postcode = "BT44 5QW",
            Country = "Country Null",
            NationId = Data.DbConstants.Nation.England,
            ExternalId = orgWithNullFieldsExternalId
        };

        setupContext.Organisations.Add(organisationWithNullFields);

        var relationshipWithNulls = new OrganisationRelationship
        {
            FirstOrganisationId = parentOrgId,
            SecondOrganisationId = orgWithNullFieldsId,
            OrganisationRelationshipTypeId = 10007,
            JoinerDate = null
        };

        setupContext.OrganisationRelationships.Add(relationshipWithNulls);

        var producerOrganisation1 = new Organisation
        {
            Id = 50000,
            ExternalId = producerOrg1ExternalId,
            Name = "Parent Producer Organisation 1",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.Organisations.Add(producerOrganisation1);

        var producerOrganisation1Sub1 = new Organisation
        {
            Id = 50002,
            ExternalId = producerOrg1Sub1ExternalId,
            Name = "Child Subsidiary Organisation 1 For Producer Organisation 1",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.Organisations.Add(producerOrganisation1Sub1);

        var producerOrganisation1Sub2 = new Organisation
        {
            Id = 50003,
            ExternalId = producerOrg1Sub2ExternalId,
            Name = "Child Subsidiary Organisation 2 For Producer Organisation 1",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.Organisations.Add(producerOrganisation1Sub2);

        var producerOrganisation1Relationship1 = new OrganisationRelationship
        {
            FirstOrganisationId = producerOrganisation1.Id,
            SecondOrganisationId = producerOrganisation1Sub1.Id,
            OrganisationRelationshipTypeId = 10007,
            RelationToDate = null // Active relationship
        };

        setupContext.OrganisationRelationships.Add(producerOrganisation1Relationship1);

        var producerOrganisation1Relationship2 = new OrganisationRelationship
        {
            FirstOrganisationId = producerOrganisation1.Id,
            SecondOrganisationId = producerOrganisation1Sub2.Id,
            OrganisationRelationshipTypeId = 10007,
            RelationToDate = null // Active relationship
        };

        setupContext.OrganisationRelationships.Add(producerOrganisation1Relationship2);

        var producerOrganisation2 = new Organisation
        {
            Id = 50004,
            ExternalId = producerOrg2ExternalId,
            Name = "Producer Organisation 2 with No Relationship",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.Organisations.Add(producerOrganisation2);

        var producerOrganisation3 = new Organisation
        {
            Id = 50005,
            ExternalId = producerOrg3ExternalId,
            Name = "Parent Producer Organisation 3",
            CompaniesHouseNumber = "87654321",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.Organisations.Add(producerOrganisation3);

        var producerOrganisation3Sub1 = new Organisation
        {
            Id = 50006,
            ExternalId = producerOrg3Sub1ExternalId,
            Name = "Child Subsidiary Organisation 1 For Producer Organisation 3",
            CompaniesHouseNumber = "87654321",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.Organisations.Add(producerOrganisation3Sub1);

        var producerOrganisation3Sub2 = new Organisation
        {
            Id = 50007,
            ExternalId = producerOrg3Sub2ExternalId,
            Name = "Child Subsidiary Organisation 2 For Producer Organisation 3",
            CompaniesHouseNumber = "87654321",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.Organisations.Add(producerOrganisation3Sub2);

        var producerOrganisation3Relationship1 = new OrganisationRelationship
        {
            FirstOrganisationId = producerOrganisation3.Id,
            SecondOrganisationId = producerOrganisation3Sub1.Id,
            OrganisationRelationshipTypeId = 10007,
            RelationToDate = null // Active relationship
        };

        setupContext.OrganisationRelationships.Add(producerOrganisation3Relationship1);

        var producerOrganisation3Relationship2 = new OrganisationRelationship
        {
            FirstOrganisationId = producerOrganisation3.Id,
            SecondOrganisationId = producerOrganisation3Sub2.Id,
            OrganisationRelationshipTypeId = 10007,
            RelationToDate = null // Active relationship
        };

        setupContext.OrganisationRelationships.Add(producerOrganisation3Relationship2);

        var producerOrganisation4 = new Organisation
        {
            Id = 50008,
            ExternalId = producerOrg4ExternalId,
            Name = "Producer Organisation 4 with No Relationship",
            CompaniesHouseNumber = "87654321",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.Organisations.Add(producerOrganisation4);

        var complianceSchemeOrg1 = new Organisation
        {
            Id = 50009,
            ExternalId = complianceSchemeOrg1ExternalId,
            Name = "Compliance Scheme Operator Organisation 1",
            CompaniesHouseNumber = "87654321",
            NationId = Data.DbConstants.Nation.England,
            IsComplianceScheme = true
        };

        setupContext.Organisations.Add(complianceSchemeOrg1);

        var complianceScheme1 = new ComplianceScheme
        {
            Id = 50100,
            ExternalId = complianceScheme1ExternalId,
            Name = "Compliance Scheme 1",
            CompaniesHouseNumber = "87654321",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.ComplianceSchemes.Add(complianceScheme1);

        var organisationsConnection1 = new OrganisationsConnection
        {
            Id = 50200,
            FromOrganisation = producerOrganisation3,
            ToOrganisation = complianceSchemeOrg1,
            FromOrganisationRoleId = Data.DbConstants.InterOrganisationRole.Producer,
            ToOrganisationRoleId = Data.DbConstants.InterOrganisationRole.ComplianceScheme
        };

        setupContext.OrganisationsConnections.Add(organisationsConnection1);

        var organisationsConnection2 = new OrganisationsConnection
        {
            Id = 50201,
            FromOrganisation = producerOrganisation4,
            ToOrganisation = complianceSchemeOrg1,
            FromOrganisationRoleId = Data.DbConstants.InterOrganisationRole.Producer,
            ToOrganisationRoleId = Data.DbConstants.InterOrganisationRole.ComplianceScheme
        };

        setupContext.OrganisationsConnections.Add(organisationsConnection2);

        var selectedScheme1 = new SelectedScheme
        {
            Id = 50300,
            ComplianceSchemeId = complianceScheme1.Id,
            OrganisationConnection = organisationsConnection1
        };

        setupContext.SelectedSchemes.Add(selectedScheme1);

        var selectedScheme2 = new SelectedScheme
        {
            Id = 50301,
            ComplianceSchemeId = complianceScheme1.Id,
            OrganisationConnection = organisationsConnection2
        };

        setupContext.SelectedSchemes.Add(selectedScheme2);

        var producerOrganisation5 = new Organisation
        {
            Id = 50010,
            ExternalId = producerOrg5ExternalId,
            Name = "Producer Organisation 5",
            CompaniesHouseNumber = "87654322",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.Organisations.Add(producerOrganisation5);

        var complianceScheme2 = new ComplianceScheme
        {
            Id = 50101,
            ExternalId = complianceScheme2ExternalId,
            Name = "Compliance Scheme 2",
            CompaniesHouseNumber = "87654322",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.ComplianceSchemes.Add(complianceScheme2);

        var organisationsConnection3 = new OrganisationsConnection
        {
            Id = 50202,
            FromOrganisation = producerOrganisation5,
            ToOrganisation = complianceSchemeOrg1,
            FromOrganisationRoleId = Data.DbConstants.InterOrganisationRole.Producer,
            ToOrganisationRoleId = Data.DbConstants.InterOrganisationRole.ComplianceScheme
        };

        setupContext.OrganisationsConnections.Add(organisationsConnection3);

        var selectedScheme3 = new SelectedScheme
        {
            Id = 50303,
            ComplianceSchemeId = complianceScheme2.Id,
            OrganisationConnection = organisationsConnection3
        };

        setupContext.SelectedSchemes.Add(selectedScheme3);


        var complianceSchemeOrg2 = new Organisation
        {
            Id = 50011,
            ExternalId = complianceSchemeOrg2ExternalId,
            Name = "Compliance Scheme Operator Organisation 2",
            CompaniesHouseNumber = "87654323",
            NationId = Data.DbConstants.Nation.England,
            IsComplianceScheme = true
        };

        setupContext.Organisations.Add(complianceSchemeOrg2);

        var complianceScheme3 = new ComplianceScheme
        {
            Id = 50102,
            ExternalId = complianceScheme3ExternalId,
            Name = "Compliance Scheme 3",
            CompaniesHouseNumber = "87654323",
            NationId = Data.DbConstants.Nation.England
        };

        setupContext.ComplianceSchemes.Add(complianceScheme3);

        //Team Members entity setup
        var services = new List<Service>
        {
            new()
            {
                Id = reExServiceId,
                Key = "ReprocessorExporter",
                Name = "Reprocessor Exporter"
            },
            new()
            {
                Id = otherServiceId,
                Key = "Other",
                Name = "Other"
            }
        };
        setupContext.Services.AddRange(services);

        var reExAdminUserServiceRole = new ServiceRole
        {
            Id = adminServiceRoleId,
            Key = "Re-Ex.AdminUser",
            Name = "Admin user",
            ServiceId = reExServiceId
        };
        var reExApprovedPersonServiceRole = new ServiceRole
        {
            Id = approvedPersonServiceRoleId,
            Key = "Re-Ex.ApprovedPerson",
            Name = "Approved Person",
            ServiceId = reExServiceId
        };
        var reExBasicUserServiceRole = new ServiceRole
        {
            Id = basicUserServiceRoleId,
            Key = "Re-Ex.BasicUser",
            Name = "Basic User",
            ServiceId = reExServiceId
        };
        var otherUserServiceRole = new ServiceRole
        {
            Id = otherUserServiceRoleId,
            Key = "Other",
            Name = "Other",
            ServiceId = otherServiceId
        };
        setupContext.ServiceRoles.Add(reExAdminUserServiceRole);
        setupContext.ServiceRoles.Add(reExApprovedPersonServiceRole);
        setupContext.ServiceRoles.Add(reExBasicUserServiceRole);
        setupContext.ServiceRoles.Add(otherUserServiceRole);

        var person101 = new Person
        {
            FirstName = "John101",
            LastName = "Doe101",
            Email = "john101.doe101@example.com",
            Telephone = "0123456789",
            ExternalId = Guid.NewGuid(),
            User = new User
            {
                UserId = user101Guid
            }
        };
        var person102 = new Person
        {
            FirstName = "John102",
            LastName = "Doe102",
            Email = "john102.doe102@example.com",
            Telephone = "0123456789",
            ExternalId = Guid.NewGuid(),
            User = new User
            {
                UserId = user102Guid
            }
        };
        var person103 = new Person
        {
            FirstName = string.Empty,
            LastName = string.Empty,
            Email = "john103.doe103@example.com",
            Telephone = string.Empty,
            ExternalId = Guid.NewGuid(),
            User = new User
            {
                UserId = user103Guid
            }
        };
        var person104 = new Person
        {
            FirstName = "John4",
            LastName = "Doe4",
            Email = "john104.doe104@example.com",
            Telephone = "0123456789",
            ExternalId = Guid.NewGuid(),
            User = new User
            {
                UserId = user104Guid
            }
        };
        var person105 = new Person
        {
            FirstName = "John105",
            LastName = "Doe105",
            Email = "john105.doe104@example.com",
            Telephone = "0123456789",
            ExternalId = Guid.NewGuid(),
            User = new User
            {
                UserId = user105Guid
            }
        };
        setupContext.Persons.Add(person101);
        setupContext.Persons.Add(person102);
        setupContext.Persons.Add(person103);
        setupContext.Persons.Add(person104);
        setupContext.Persons.Add(person105);

        var organisation101 = new Organisation
        {
            Name = "Name101",
            ExternalId = organisation101ExternalId
        };
        var organisation102 = new Organisation
        {
            Name = "Name102",
            ExternalId = organisation102ExternalId
        };
        setupContext.Organisations.Add(organisation101);
        setupContext.Organisations.Add(organisation102);

        var connection101 = new PersonOrganisationConnection
        {
            ExternalId = Guid.NewGuid(),
            Organisation = organisation101,
            Person = person101
        };
        var connection102 = new PersonOrganisationConnection
        {
            ExternalId = Guid.NewGuid(),
            Organisation = organisation101,
            Person = person102
        };
        var connection103 = new PersonOrganisationConnection
        {
            ExternalId = Guid.NewGuid(),
            Organisation = organisation101,
            Person = person103
        };
        var connection104 = new PersonOrganisationConnection
        {
            ExternalId = Guid.NewGuid(),
            Organisation = organisation102,
            Person = person104
        };
        var connection105 = new PersonOrganisationConnection
        {
            ExternalId = Guid.NewGuid(),
            Organisation = organisation102,
            Person = person105
        };
        setupContext.PersonOrganisationConnections.Add(connection101);
        setupContext.PersonOrganisationConnections.Add(connection102);
        setupContext.PersonOrganisationConnections.Add(connection103);
        setupContext.PersonOrganisationConnections.Add(connection104);
        setupContext.PersonOrganisationConnections.Add(connection105);

        var enrolments = new List<Enrolment>
        {
            new()
            {
                ServiceRole = reExAdminUserServiceRole,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Enrolled,
                Connection = connection101,
                IsDeleted = false,
            },
            new()
            {
                ServiceRole = reExAdminUserServiceRole,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Enrolled,
                Connection = connection102,
                IsDeleted = false,
            },
            new()
            {
                ServiceRole = reExApprovedPersonServiceRole,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Enrolled,
                Connection = connection102,
                IsDeleted = false
            },
            new()
            {
                ServiceRole = reExBasicUserServiceRole,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Invited,
                Connection = connection103,
                IsDeleted = false
            },
            new()
            {
                ServiceRole = reExBasicUserServiceRole,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Rejected,
                Connection = connection103,
                IsDeleted = true
            },
            new()
            {
                ServiceRole = reExAdminUserServiceRole,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Enrolled,
                Connection = connection104,
                IsDeleted = false
            },
            new()
            {
                ServiceRole = reExBasicUserServiceRole,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Enrolled,
                Connection = connection105,
                IsDeleted = false
            }
        };
        setupContext.Enrolments.AddRange(enrolments);

        var invite101 = new PersonOrganisationConnectionInvite
        {
            IsDeleted = false,
            IsUsed = false,
            Organisation = organisation101,
            Person = person103,
            InvitedByUserId = person101.User.Id
        };
        var invite102 = new PersonOrganisationConnectionInvite
        {
            IsDeleted = true,
            IsUsed = true,
            Organisation = organisation101,
            Person = person102,
            InvitedByUserId = person101.User.Id
        };
        setupContext.PersonOrganisationConnectionInvites.Add(invite101);
        setupContext.PersonOrganisationConnectionInvites.Add(invite102);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    [TestMethod]
    public async Task UpdateByOrganisationNationId_WithValidParameters_Succeeds()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var organisation = new OrganisationUpdateModel
        {
            Name = "Name",
            SubBuildingName = "SubBuildingName",
            BuildingName = "BuildingName",
            BuildingNumber = "BuildingNumber",
            Street = "Street",
            Postcode = "Postcode",
            Locality = "Locality",
            DependentLocality = "DependentLocality",
            Town = "Town",
            County = "County",
            Country = "Country",
            NationId = 3
        };

        // need to add relevant organisation into test db
        await _accountContext.Organisations.AddAsync(
            new Organisation
            {
                Name = string.Empty,
                ExternalId = organisationId,
            });
        await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationId);

        // Act
        var result = await _organisationService.UpdateOrganisationDetails(
            userId,
            organisationId,
            organisation);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        var org = await _accountContext.Organisations.SingleOrDefaultAsync(o => o.ExternalId == organisationId);
        Assert.IsNotNull(org);
        org.Name.Should().Be("Name");
        org.SubBuildingName.Should().Be("SubBuildingName");
        org.BuildingName.Should().Be("BuildingName");
        org.BuildingNumber.Should().Be("BuildingNumber");
        org.Street.Should().Be("Street");
        org.Postcode.Should().Be("Postcode");
        org.Locality.Should().Be("Locality");
        org.DependentLocality.Should().Be("DependentLocality");
        org.Town.Should().Be("Town");
        org.County.Should().Be("County");
        org.Country.Should().Be("Country");
        org.NationId.Should().Be(3);
    }

    [TestMethod]
    public async Task UpdateByOrganisationNationId_WithInvalidParameters_ReturnsFailure()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var organisation = new OrganisationUpdateModel
        {
            Name = "Name",
            SubBuildingName = "SubBuildingName",
            BuildingName = "BuildingName",
            BuildingNumber = "BuildingNumber",
            Street = "Street",
            Locality = "Locality",
            DependentLocality = "DependentLocality",
            Town = "Town",
            County = "County",
            Country = "Country",
            NationId = 3
        };

        // need to add relevant organisation into test db
        await _accountContext.Organisations.AddAsync(
            new Organisation
            {
                Name = string.Empty,
                ExternalId = Guid.NewGuid(),
            });
        await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationId);

        // Act
        var result = await _organisationService.UpdateOrganisationDetails(
            userId,
            organisationId,
            organisation);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        var org = await _accountContext.Organisations.SingleOrDefaultAsync(o => o.ExternalId == organisationId);
        Assert.IsNull(org);
    }

    [TestMethod]
    public async Task UpdateByOrganisation_WithInvalidParameters_For_NonCompanieshouse_ReturnsFailure()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var organisation = new OrganisationUpdateModel
        {
            Name = "Name",
            SubBuildingName = "SubBuildingName",
            BuildingName = "BuildingName",
            BuildingNumber = "BuildingNumber",
            Street = "Street",
            Locality = "Locality",
            DependentLocality = "DependentLocality",
            Town = "",
            County = "County",
            Country = "Country",
            NationId = 3
        };

        // need to add relevant organisation into test db
        await _accountContext.Organisations.AddAsync(
            new Organisation
            {
                Name = string.Empty,
                ExternalId = organisationId,
                OrganisationTypeId = 2
            });
        await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationId);

        // Act
        var result = await _organisationService.UpdateOrganisationDetails(
            userId,
            organisationId,
            organisation);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task UpdateByOrganisation_WithInvalidNation_For_NonCompanieshouse_ReturnsFailure()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var organisation = new OrganisationUpdateModel
        {
            Name = "Name",
            SubBuildingName = "SubBuildingName",
            BuildingName = "BuildingName",
            BuildingNumber = "BuildingNumber",
            Street = "Street",
            Locality = "Locality",
            DependentLocality = "DependentLocality",
            Town = "Town",
            County = "County",
            Country = "Country",
            NationId = 0
        };

        // need to add relevant organisation into test db
        await _accountContext.Organisations.AddAsync(
            new Organisation
            {
                Name = string.Empty,
                ExternalId = organisationId,
                OrganisationTypeId = 2
            });
        await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationId);

        // Act
        var result = await _organisationService.UpdateOrganisationDetails(
            userId,
            organisationId,
            organisation);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task IsOrganisationValidAsync_Returns_ExpectedResult(bool expectedResult)
    {
        var organisationId = Guid.NewGuid();

        //Arrange
        if (expectedResult)
        {
            // need to add relevant organisation into test db
            await _accountContext.Organisations.AddAsync(
                new Organisation
                {
                    Name = string.Empty,
                    ExternalId = organisationId,
                    OrganisationTypeId = 2
                });
            await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationId);
        }
        //Act
        var isValidOrganisation = await _organisationService.IsOrganisationValidAsync(organisationId);

        //Assert
        isValidOrganisation.Should().Be(expectedResult);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task IsCSOrganisationValidAsync_Returns_ExpectedResult(bool expectedResult)
    {
        // Arrange
        var organisationId = expectedResult ? complianceSchemeExternalId : Guid.NewGuid();

        // Act
        var isValidOrganisation = await _organisationService.IsCSOrganisationValidAsync(organisationId);

        // Assert
        isValidOrganisation.Should().Be(expectedResult);
    }

    [TestMethod]
    public async Task GetUpdatedProducers_ReturnsOrganisationsWithinDateRange()
    {
        // Arrange
        var request = new UpdatedProducersRequest
        {
            From = DateTime.UtcNow.AddDays(-5),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _organisationService.GetUpdatedProducers(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(29); // Adjust count based on known test data setup
    }

    [TestMethod]
    public async Task GetUpdatedProducers_ReturnsEmptyList_WhenNoOrganisationsWithinDateRange()
    {
        // Arrange
        var request = new UpdatedProducersRequest
        {
            From = DateTime.UtcNow.AddDays(-100),
            To = DateTime.UtcNow.AddDays(-50)
        };

        // Act
        var result = await _organisationService.GetUpdatedProducers(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty(); // No organisations should match this date range
    }

    [TestMethod]
    public async Task GetPersonEmails_WithExistingOrganisationId_ReturnsListOfEmails()
    {
        // Arrange
        var organisationExternalId = Guid.NewGuid();
        var organisationId = 11;
        var person1Id = 22;
        var person2Id = 33;
        var orgConnection1Id = 500;
        var orgConnection2Id = 501;

        // Set up the organization
        await _accountContext.Organisations.AddAsync(new Organisation
        {
            Id = organisationId,
            ExternalId = organisationExternalId,
            Name = "Test Organisation"
        });

        // Set up persons
        await _accountContext.Persons.AddRangeAsync(
            new Person { Id = person1Id, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Telephone = "0123456789", ExternalId = organisationExternalId },
            new Person { Id = person2Id, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Telephone = "0123456789", ExternalId = organisationExternalId }
        );

        // Set up connections between persons and organization
        await _accountContext.PersonOrganisationConnections.AddRangeAsync(
            new PersonOrganisationConnection { Id = orgConnection1Id, PersonId = person1Id, OrganisationId = organisationId },
            new PersonOrganisationConnection { Id = orgConnection2Id, PersonId = person2Id, OrganisationId = organisationId }
        );

        // Set up enrolments
        await _accountContext.Enrolments.AddRangeAsync
        (
            new Enrolment { ConnectionId = orgConnection1Id },
            new Enrolment { ConnectionId = orgConnection2Id }
        );

        await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationExternalId);

        // Act
        var result = await _organisationService.GetPersonEmails(organisationExternalId, "DR");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.FirstName == "John" && p.LastName == "Doe" && p.Email == "john.doe@example.com");
        result.Should().Contain(p => p.FirstName == "Jane" && p.LastName == "Smith" && p.Email == "jane.smith@example.com");
    }

    [TestMethod]
    public async Task GetPersonEmails_WithExistingOrganisationId_ReturnsListOfEmails_ExcludingUnenrolledPeople()
    {
        // Arrange
        var organisationExternalId = Guid.NewGuid();
        var organisationId = 11;
        var person1Id = 22;
        var person2Id = 33;
        var orgConnection1Id = 500;
        var orgConnection2Id = 501;

        // Set up the organization
        await _accountContext.Organisations.AddAsync(new Organisation
        {
            Id = organisationId,
            ExternalId = organisationExternalId,
            Name = "Test Organisation"
        });

        // Set up persons
        await _accountContext.Persons.AddRangeAsync(
            new Person { Id = person1Id, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Telephone = "0123456789", ExternalId = organisationExternalId },
            new Person { Id = person2Id, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Telephone = "0123456789", ExternalId = organisationExternalId }
        );

        // Set up connections between persons and organization
        await _accountContext.PersonOrganisationConnections.AddRangeAsync(
            new PersonOrganisationConnection { Id = orgConnection1Id, PersonId = person1Id, OrganisationId = organisationId },
            new PersonOrganisationConnection { Id = orgConnection2Id, PersonId = person2Id, OrganisationId = organisationId }
        );

        // Set up enrolments
        await _accountContext.Enrolments.AddRangeAsync
        (
            new Enrolment { ConnectionId = orgConnection1Id }
        );

        await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationExternalId);

        // Act
        var result = await _organisationService.GetPersonEmails(organisationExternalId, "DR");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.FirstName == "John" && p.LastName == "Doe" && p.Email == "john.doe@example.com");
    }


    [TestMethod]
    public async Task GetPersonEmails_WithNonExistentOrganisationId_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentOrganisationId = Guid.NewGuid();

        // Act
        var result = await _organisationService.GetPersonEmails(nonExistentOrganisationId, "CS");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetPersonEmails_ForComplianceScheme_WithUnEnrolledUsers_ReturnsCorrectUserList()
    {
        // Arrange
        var organisationExternalId = Guid.NewGuid();
        var organisationId = 11;
        var person1Id = 22;
        var person2Id = 33;
        var companiesHouseNumber = "012345678";

        // Set up the organization
        await _accountContext.Organisations.AddAsync(new Organisation
        {
            Id = organisationId,
            ExternalId = organisationExternalId,
            Name = "Test Organisation",
            CompaniesHouseNumber = companiesHouseNumber
        });

        // Set up persons
        await _accountContext.Persons.AddRangeAsync(
            new Person { Id = person1Id, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Telephone = "0123456789", ExternalId = organisationExternalId },
            new Person { Id = person2Id, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Telephone = "0123456789", ExternalId = organisationExternalId }
        );

        // Set up connections between persons and organization
        await _accountContext.PersonOrganisationConnections.AddRangeAsync(
            new PersonOrganisationConnection { PersonId = person1Id, OrganisationId = organisationId },
            new PersonOrganisationConnection { PersonId = person2Id, OrganisationId = organisationId }
        );

        await _accountContext.ComplianceSchemes.AddAsync(
            new ComplianceScheme { Name = "beyond.there", CompaniesHouseNumber = companiesHouseNumber, ExternalId = organisationExternalId }
        );

        await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationExternalId);

        await _accountContext.Enrolments.AddRangeAsync
        (
            new Enrolment
            {
                ConnectionId = (await _accountContext.PersonOrganisationConnections.FirstOrDefaultAsync(a => a.PersonId == person1Id)).Id,
                ServiceRoleId = 1,
                EnrolmentStatus = new() { Id = 10, Name = "Approved" }
            }
        );

        await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationExternalId);

        // Act
        var result = await _organisationService.GetPersonEmails(organisationExternalId, "CS");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.FirstName == "John" && p.LastName == "Doe" && p.Email == "john.doe@example.com");
    }

    [TestMethod]
    public async Task GetPersonEmails_WithOrganisationHavingNoPersons_ReturnsEmptyList()
    {
        // Arrange
        var organisationId = Guid.NewGuid();

        // need to add relevant organisation into test db
        await _accountContext.Organisations.AddAsync(
            new Organisation
            {
                Name = "Empty Organisation",
                ExternalId = organisationId,
            });
        await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationId);


        // Act
        var result = await _organisationService.GetPersonEmails(organisationId, "DR");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetPersonEmails_WithOrganisationHavingNoEntityTypeCode_ReturnsEmptyList()
    {
        // Arrange
        var organisationId = Guid.NewGuid();

        // need to add relevant organisation into test db
        await _accountContext.Organisations.AddAsync(
            new Organisation
            {
                Name = "Empty Organisation",
                ExternalId = organisationId,
            });
        await _accountContext.SaveChangesAsync(Guid.NewGuid(), organisationId);


        // Act
        var result = await _organisationService.GetPersonEmails(organisationId, "");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetTeamMemberListForOrganisation_ShouldReturnMappedTeamMembers()
    {
        // Arrange        

        // Act
        var result = await _organisationService.GetTeamMemberListForOrganisation(user101Guid, organisation101ExternalId, adminServiceRoleId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(tm => tm.Enrolments.Any(e => e.AddedBy == "John101 Doe101"));
    }

    [TestMethod]
    public async Task GetTeamMemberListForOrganisation_ShouldReturnEmpty_WhenNoEnrolments()
    {
        // Arrange

        // Act
        var result = await _organisationService.GetTeamMemberListForOrganisation(user104Guid, Guid.NewGuid(), basicUserServiceRoleId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetTeamMemberListForOrganisation_ShouldReturnEnrolmentsWithoutInvite()
    {
        // Arrange

        // Act
        var result = await _organisationService.GetTeamMemberListForOrganisation(user104Guid, organisation102ExternalId, adminServiceRoleId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(tm => tm.FirstName == "John105" && tm.LastName == "Doe105");
        result.Should().Contain(tm => tm.Enrolments.Count() == 1);
        result.Should().Contain(tm => tm.Enrolments.Any(e => e.AddedBy == string.Empty)); // Invite not found
    }

    [TestMethod]
    public async Task UpdateOrganisationRelationshipsAsync_WhenNotExists()
    {
        // Arrange
        var model = new OrganisationRelationshipModel
        {
            FirstOrganisationId = 12345,
            SecondOrganisationId = 67890
        };

        var parentExternalID = Guid.NewGuid();
        var userExternalID = Guid.NewGuid();

        // Act
        var result = await _organisationService.UpdateOrganisationRelationshipsAsync(model, parentExternalID, userExternalID);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task UpdateOrganisationRelationshipsAsync_WhenRelationshipDoesNotExist_ReturnsNull_AndDoesNotModifyDb()
    {
        // Arrange
        // IDs that are guaranteed not to exist in the seeded data
        var model = new OrganisationRelationshipModel
        {
            FirstOrganisationId = 12345,
            SecondOrganisationId = 67890
        };

        var parentExternalID = Guid.NewGuid();
        var userExternalID = Guid.NewGuid();

        // Snapshot counts before
        var beforeCount = await _accountContext.OrganisationRelationships.CountAsync();

        // Act
        var result = await _organisationService.UpdateOrganisationRelationshipsAsync(model, parentExternalID, userExternalID);

        // Assert
        result.Should().BeNull(); // triggers the null branch (Commit + return)
        var afterCount = await _accountContext.OrganisationRelationships.CountAsync();
        afterCount.Should().Be(beforeCount); // no rows added/updated

        // also ensure nothing is being tracked as modified
        _accountContext.ChangeTracker.Entries()
            .All(e => e.State == EntityState.Unchanged)
            .Should().BeTrue();
    }

    [TestMethod]
    public async Task UpdateOrganisationRelationshipsAsync_WhenRelationshipExists_UpdatesAndReturnsEntity()
    {
        // Arrange
        // Use one of the seeded active relationships to ensure existingRelationshipCheck != null
        // From SetUpDatabase: (10000 -> 10013) exists and RelationToDate == null
        var model = new OrganisationRelationshipModel
        {
            FirstOrganisationId = 10000,
            SecondOrganisationId = 10013
        };

        var parentExternalID = Guid.NewGuid();
        var userExternalID = Guid.NewGuid();

        // Act
        var result = await _organisationService.UpdateOrganisationRelationshipsAsync(model, parentExternalID, userExternalID);

        // Assert
        result.Should().NotBeNull();
        result.FirstOrganisationId.Should().Be(10000);
        result.SecondOrganisationId.Should().Be(10013);

        // Ensure SaveChangesAsync path was taken by verifying entity still present and active
        var reloaded = await _accountContext.OrganisationRelationships
            .FirstOrDefaultAsync(r => r.FirstOrganisationId == 10000 &&
                                      r.SecondOrganisationId == 10013 &&
                                      r.RelationToDate == null);

        reloaded.Should().NotBeNull();
    }

}