using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EnrolmentStatus = BackendAccountService.Data.DbConstants.EnrolmentStatus;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class OrganisationServiceTests
{
    private AccountsDbContext _accountContext= null!;
    private OrganisationService _organisationService = null!;

    private const string DuplicateCompaniesHouseNumber = "12345678";
    private const string DeletedCompaniesHouseNumber = "DL123456";
    private const string SingleCompaniesHouseNumber = "87654321";
    private const string NoneExistentCompaniesHouseNumber = "12121212";
    private readonly Guid userId = new Guid("00000000-0000-0000-0000-000000000001");
    private readonly Guid orgExternalId = Guid.NewGuid();
    private readonly Guid orgId = new Guid("00000000-0000-0000-0000-000000000010");
    private readonly Guid requestingPersonId = new Guid("00000000-0000-0000-0000-000000000020");

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("OrganisationServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);

        _accountContext = new AccountsDbContext(contextOptions);
        _organisationService = new OrganisationService(_accountContext);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task WhenQueryForEmpyStringNumber_ThenThrowException()
    {
        //Act
        _ = await _organisationService.GetOrganisationsByCompaniesHouseNumberAsync("");
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
    public async Task WhenQueryForNoneExistingNumber_ThenReturnEmptyList()
    {
        //Act
        var organisationList = await _organisationService.GetOrganisationsByCompaniesHouseNumberAsync(NoneExistentCompaniesHouseNumber);

        //Assert
        organisationList.Should().NotBeNull();
        organisationList.Should().HaveCount(0);
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
        organisationList.Should().HaveCount(2);
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
    private void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);
        
        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();

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
            IsDeleted= true
        };
        setupContext.Organisations.Add(organisation4);
        
        var organisation5 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            CompaniesHouseNumber = SingleCompaniesHouseNumber,
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
            Email = "user2@test.com"
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
        
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
}
