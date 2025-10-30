using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.DbConstants;

namespace BackendAccountService.Core.UnitTests.TestHelpers;

public static class EnrolmentsTestHelper
{
    public static void SetUpDatabase(AccountsDbContext setupContext)
    {
        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();

        var organisation1 = setupContext.AddOrganisation("organisation1");
        var organisation2 = setupContext.AddOrganisation("organisation2");
        var organisation3 = setupContext.AddRegulatorOrganisation("organisation3");
        
        // setup basic users
        setupContext.AddUserToOrganisation(organisation1, "basicuser1@test.com", "Basic", "User1", PersonRole.Employee, Data.DbConstants.ServiceRole.Packaging.BasicUser.Id);
        setupContext.AddUserToOrganisation(organisation1, "basicuser2@test.com", "Basic", "User2", PersonRole.Employee, Data.DbConstants.ServiceRole.Packaging.BasicUser.Id);
        
        // setup basic admins
        setupContext.AddUserToOrganisation(organisation1, "basicadmin1@test.com", "Basic", "Admin1", PersonRole.Admin, Data.DbConstants.ServiceRole.Packaging.BasicUser.Id);
        setupContext.AddUserToOrganisation(organisation1, "basicadmin2@test.com", "Basic", "Admin2", PersonRole.Admin, Data.DbConstants.ServiceRole.Packaging.BasicUser.Id);
        
        // setup delegated persons
        setupContext.AddUserToOrganisation(organisation1, "delegatedperson1@test.com", "Delegated", "Person1", PersonRole.Admin, Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Id);
        setupContext.AddUserToOrganisation(organisation1, "delegatedperson2@test.com", "Delegated", "Person2", PersonRole.Admin, Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Id);

        // setup authorized persons
        setupContext.AddUserToOrganisation(organisation1, "approvedperson1@test.com", "Approved", "Person1", PersonRole.Admin, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id);
        setupContext.AddUserToOrganisation(organisation1, "approvedperson2@test.com", "Approved", "Person2", PersonRole.Admin, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id);
        
        // setup basic user from another org
        setupContext.AddUserToOrganisation(organisation2, "basicuseranotherorg@test.com", "Basic", "UserOtherOrg", PersonRole.Employee, Data.DbConstants.ServiceRole.Packaging.BasicUser.Id);
        
        setupContext.AddUserToOrganisation(organisation3, "regulatoruser@test.com", "Basic", "UserOtherOrg", PersonRole.Employee, Data.DbConstants.ServiceRole.Regulator.Basic.Id, Data.DbConstants.EnrolmentStatus.Invited);
        
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    public static void AddDelegatedPersonEnrolment(this AccountsDbContext setupContext, Enrolment enrolment, Enrolment nominatorEnrolment)
    {
        setupContext.DelegatedPersonEnrolments.Add(new DelegatedPersonEnrolment()
        {
            Enrolment = enrolment,
            NominatorEnrolment = nominatorEnrolment
        });
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
    
    public static void SetUpRegulatorDatabase(AccountsDbContext setupContext)
    {
        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();

        var organisation1 = setupContext.AddOrganisation("regulatorOrg");
        var organisation2 = setupContext.AddOrganisation("packingOrg");
        
        // setup regulator users
        setupContext.AddUserToOrganisation(organisation1, "regulatorBasic@test.com", "reg", "User1", PersonRole.Employee, Data.DbConstants.ServiceRole.Regulator.Basic.Id);
        setupContext.AddUserToOrganisation(organisation1, "regulatorBasic2@test.com", "reg", "User2", PersonRole.Employee, Data.DbConstants.ServiceRole.Regulator.Basic.Id);
        setupContext.AddUserToOrganisation(organisation1, "regulatorAdmin@test.com", "reg", "Admin", PersonRole.Admin, Data.DbConstants.ServiceRole.Regulator.Admin.Id);
        
        // setup packaging org users
        setupContext.AddUserToOrganisation(organisation2, "packagingUser@test.com", "pack", "User", PersonRole.Employee, Data.DbConstants.ServiceRole.Packaging.BasicUser.Id);
        setupContext.AddUserToOrganisation(organisation2, "packagingAdmin@test.com", "pack", "Admin", PersonRole.Admin, Data.DbConstants.ServiceRole.Packaging.BasicUser.Id);
        setupContext.AddUserToOrganisation(organisation2, "packagingDelegated@test.com", "pack", "Delegated", PersonRole.Admin, Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Id);
        setupContext.AddUserToOrganisation(organisation2, "packagingApproved@test.com", "pack", "Approved", PersonRole.Admin, Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    private static void AddUserToOrganisation(this AccountsDbContext setupContext, Organisation organisation, string email, 
        string firstName, string lastName, int personRoleId, int serviceRoleId, int enrolmentStatus = Data.DbConstants.EnrolmentStatus.Pending)
    {
        var user = setupContext.AddUser(email);
        var person = setupContext.AddPerson(user,firstName, lastName);
        var connection = setupContext.AddPersonOrganisationConnection(person, organisation, personRoleId);
        var enrolment = setupContext.AddEnrolment(connection, serviceRoleId, enrolmentStatus);
    }
    
    private static Enrolment AddEnrolment(this AccountsDbContext setupContext, PersonOrganisationConnection personOrganisationConnection, int serviceRoleId, int enrolmentStatus)
    {
        var enrolment = new Enrolment
        {
            Connection = personOrganisationConnection,
            ServiceRoleId = serviceRoleId,
            EnrolmentStatusId = enrolmentStatus,
            IsDeleted = false,
        };
        setupContext.Enrolments.Add(enrolment);

        return enrolment;
    }
    
   

    private static PersonOrganisationConnection AddPersonOrganisationConnection(
        this AccountsDbContext setupContext, 
        Person person,
        Organisation organisation,
        int personRoleId)
    {
        var personOrganisationConnection = new PersonOrganisationConnection
        {
            Person = person,
            Organisation = organisation,
            IsDeleted = false,
            PersonRoleId = personRoleId
        };
        
        setupContext.PersonOrganisationConnections.Add(personOrganisationConnection);

        return personOrganisationConnection;
    }

    private static User AddUser(this AccountsDbContext setupContext, string email)
    {
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = email,
            IsDeleted = false,
            InviteToken = $"{email}InviteToken"
        };
        setupContext.Users.Add(user);

        return user;
    }

    private static Person AddPerson(this AccountsDbContext setupContext, User user, string firstName, string lastName)
    {
        var person = new Person
        {
            User = user,
            ExternalId = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = user.Email,
            Telephone = "02890123456"
        };
        setupContext.Persons.Add(person);
        
        return person;
    }

    private static Organisation AddOrganisation(this AccountsDbContext setupContext, string name)
    {
        var organisation = new Organisation
        {
            Name = name,
            OrganisationTypeId = 1,
            ExternalId =  Guid.NewGuid()
        };
        setupContext.Organisations.Add(organisation);

        return organisation;
    }
    
    private static Organisation AddRegulatorOrganisation(this AccountsDbContext setupContext, string name)
    {
        var organisation = new Organisation
        {
            Name = name,
            OrganisationTypeId = Data.DbConstants.OrganisationType.Regulators,
            ExternalId =  Guid.NewGuid()
        };
        setupContext.Organisations.Add(organisation);

        return organisation;
    }
}