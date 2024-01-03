using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class OrganisationServiceProducerUsersTests
{
    private AccountsDbContext _accountsDbContext= null!;
    private OrganisationService _organisationService = null!;
    
    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("OrganisationServiceProducerUsersTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _accountsDbContext = new AccountsDbContext(contextOptions);
        _accountsDbContext.Database.EnsureDeleted();
        _accountsDbContext.Database.EnsureCreated();
        
        _organisationService = new OrganisationService(_accountsDbContext);
    }
    
    [TestMethod]
    [DataRow(Data.DbConstants.PersonRole.Admin, Data.DbConstants.EnrolmentStatus.Approved)]
    [DataRow(Data.DbConstants.PersonRole.Admin, Data.DbConstants.EnrolmentStatus.Enrolled)]
    [DataRow(Data.DbConstants.PersonRole.Admin, Data.DbConstants.EnrolmentStatus.Pending)]
    [DataRow(Data.DbConstants.PersonRole.Employee, Data.DbConstants.EnrolmentStatus.Approved)]
    [DataRow(Data.DbConstants.PersonRole.Employee, Data.DbConstants.EnrolmentStatus.Enrolled)]
    [DataRow(Data.DbConstants.PersonRole.Employee, Data.DbConstants.EnrolmentStatus.Pending)]
    public async Task WhenGettingProducerOrganisationUsers_ShouldReturnCorrectData(int personRoleId, int enrolmentStatusId)
    {
        var organisation = SetUpOrganisation(_accountsDbContext);
        var  person = SetUpEnrolment(organisation.Id, personRoleId, enrolmentStatusId);
        _accountsDbContext.SaveChanges(Guid.Empty, Guid.Empty);
        
        var results = await _organisationService.GetProducerUsers(organisation.ExternalId);

        results.Count.Should().Be(1);
        var actual = results.First();

        actual.FirstName.Should().Be(person.FirstName);
        actual.LastName.Should().Be(person.LastName);
        actual.Email.Should().Be(person.Email);
        actual.PersonExternalId.ToString().Should().Be(person.ExternalId.ToString());
    }
    
    [TestMethod]
    [DataRow(Data.DbConstants.PersonRole.NotSet, Data.DbConstants.EnrolmentStatus.Approved)]
    [DataRow(Data.DbConstants.PersonRole.NotSet, Data.DbConstants.EnrolmentStatus.Enrolled)]
    [DataRow(Data.DbConstants.PersonRole.NotSet, Data.DbConstants.EnrolmentStatus.Pending)]
    [DataRow(Data.DbConstants.PersonRole.Admin, Data.DbConstants.EnrolmentStatus.NotSet)]
    [DataRow(Data.DbConstants.PersonRole.Admin, Data.DbConstants.EnrolmentStatus.Rejected)]
    [DataRow(Data.DbConstants.PersonRole.Admin, Data.DbConstants.EnrolmentStatus.Invited)]
    [DataRow(Data.DbConstants.PersonRole.Admin, Data.DbConstants.EnrolmentStatus.Nominated)]
    [DataRow(Data.DbConstants.PersonRole.Admin, Data.DbConstants.EnrolmentStatus.OnHold)]
    [DataRow(Data.DbConstants.PersonRole.Employee, Data.DbConstants.EnrolmentStatus.NotSet)]
    [DataRow(Data.DbConstants.PersonRole.Employee, Data.DbConstants.EnrolmentStatus.Rejected)]
    [DataRow(Data.DbConstants.PersonRole.Employee, Data.DbConstants.EnrolmentStatus.Invited)]
    [DataRow(Data.DbConstants.PersonRole.Employee, Data.DbConstants.EnrolmentStatus.Nominated)]
    [DataRow(Data.DbConstants.PersonRole.Employee, Data.DbConstants.EnrolmentStatus.OnHold)]
    public async Task WhenGettingProducerOrganisationWithNoValidUsers_ShouldReturnNoData(int personRoleId, int enrolmentStatusId)
    {
        var organisation = SetUpOrganisation(_accountsDbContext);
        _ = SetUpEnrolment(organisation.Id, personRoleId, enrolmentStatusId);
        _accountsDbContext.SaveChanges(Guid.Empty, Guid.Empty);
        
        var results = await _organisationService.GetProducerUsers(organisation.ExternalId);

        results.Count.Should().Be(0);
    }

    private Organisation SetUpOrganisation(AccountsDbContext setupContext)
    {
        var organisation1 = new Organisation
        {
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = Data.DbConstants.ProducerType.SoleTrader,
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
            NationId = Data.DbConstants.Nation.England,
            ExternalId = Guid.NewGuid(),
            ReferenceNumber = "Ref Number 1",
        };
        setupContext.Add(organisation1);

        return organisation1;
    }

    private Person SetUpEnrolment(int organisationId, int personRoleId, int enrolmentStatusId)
    {
        var user1 = new User()
        {
            UserId = new Guid("00000000-0000-0000-0000-000000000001"),
            Email = "user1@test.com"
        };
        _accountsDbContext.Add(user1);
        
        var person1 = new Person()
        {
            FirstName = $"User {organisationId}",
            LastName = $"Test {organisationId}",
            Email = $"user{organisationId}@test.com",
            Telephone = "0123456789",
            ExternalId = new Guid("00000000-0000-0000-0000-000000000020"),
            UserId = 1
        };
        _accountsDbContext.Add(person1);
        
        var enrolment1 = new Enrolment
        {
            Connection = new PersonOrganisationConnection()
            {
                PersonId = 1,
                OrganisationId = organisationId,
                PersonRoleId = personRoleId, //Data.DbConstants.PersonRole.Admin,
                OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer
            },
            EnrolmentStatusId = enrolmentStatusId, //Data.DbConstants.EnrolmentStatus.Approved,
            ServiceRoleId = Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id
        };
        
        _accountsDbContext.Add(enrolment1);

        return person1;
    }
}